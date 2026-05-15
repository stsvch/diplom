// UpdateQuestionCommandHandler.cs

using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;

namespace Tests.Application.Tests.Commands.UpdateQuestion;

/// <summary>
/// Обработчик CQRS-команды UpdateQuestionCommand: выполняет сценарий изменения состояния и сохраняет результат.
/// </summary>
public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, Result<QuestionDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public UpdateQuestionCommandHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<Result<QuestionDto>> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Include(q => q.Test)
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionDto>("Вопрос не найден.");

        if (question.Test.CreatedById != request.CreatedById)
            return Result.Failure<QuestionDto>("Вы не являетесь автором этого теста.");

        question.Type = request.Type;
        question.Text = request.Text;
        question.Points = request.Points;
        if (request.GradeType.HasValue)
            question.GradeType = request.GradeType.Value;
        question.Explanation = request.Explanation;
        question.ExpectedAnswer = request.ExpectedAnswer;

        // Синхронизируем варианты ответа:
        //  - Id есть и совпадает → обновляем поля
        //  - Id null или не найден → добавляем новый
        //  - в БД есть, но в request нет → удаляем
        // Так избегаем конфликта delete-and-recreate с EF tracker, из-за которого
        // ловили DbUpdateConcurrencyException при автосохранении.
        var existingById = question.AnswerOptions.ToDictionary(o => o.Id);
        var keepIds = new HashSet<Guid>();

        for (int i = 0; i < request.AnswerOptions.Count; i++)
        {
            var input = request.AnswerOptions[i];
            if (input.Id.HasValue && existingById.TryGetValue(input.Id.Value, out var existing))
            {
                existing.Text = input.Text;
                existing.IsCorrect = input.IsCorrect;
                existing.OrderIndex = i;
                existing.MatchingPairValue = input.MatchingPairValue;
                keepIds.Add(existing.Id);
            }
            else
            {
                // Важно: НЕ через question.AnswerOptions.Add(...). BaseEntity.Id генерится в
                // конструкторе как Guid.NewGuid(), и EF detect-changes на tracked-родителе
                // принимает new entity с не-default Id за Modified → UPDATE WHERE Id=...,
                // Ноль измененных строк приводит к DbUpdateConcurrencyException.
                // _context.AnswerOptions.Add форсит state=Added.
                var newOpt = new AnswerOption
                {
                    QuestionId = question.Id,
                    Text = input.Text,
                    IsCorrect = input.IsCorrect,
                    OrderIndex = i,
                    MatchingPairValue = input.MatchingPairValue
                };
                _context.AnswerOptions.Add(newOpt);
            }
        }

        // Удаляем существующие, которых нет в новом списке (по Id).
        var toRemove = existingById.Values
            .Where(o => !keepIds.Contains(o.Id))
            .ToList();
        if (toRemove.Count > 0)
            _context.AnswerOptions.RemoveRange(toRemove);

        // Пересчитываем максимальный балл теста после изменения вопросов.
        var test = question.Test;
        var allQuestions = await _context.Questions
            .Where(q => q.TestId == test.Id)
            .ToListAsync(cancellationToken);
        test.MaxScore = allQuestions.Where(q => q.Id != question.Id).Sum(q => q.Points) + request.Points;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<QuestionDto>(question);
        return Result.Success(dto);
    }
}
