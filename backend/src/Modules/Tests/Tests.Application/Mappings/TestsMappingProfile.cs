using System.Text.Json;
using AutoMapper;
using Tests.Application.DTOs;
using Tests.Domain.Entities;

namespace Tests.Application.Mappings;

public class TestsMappingProfile : Profile
{
    public TestsMappingProfile()
    {
        CreateMap<Test, TestDto>()
            .ForMember(d => d.QuestionsCount, opt => opt.MapFrom(s => s.Questions.Count));

        CreateMap<Test, TestDetailDto>()
            .ForMember(d => d.QuestionsCount, opt => opt.MapFrom(s => s.Questions.Count))
            .ForMember(d => d.Questions, opt => opt.MapFrom(s => s.Questions.OrderBy(q => q.OrderIndex)));

        CreateMap<Question, QuestionDto>()
            .ForMember(d => d.AnswerOptions, opt => opt.MapFrom(s => s.AnswerOptions.OrderBy(a => a.OrderIndex)));

        CreateMap<Question, StudentQuestionDto>()
            .ForMember(d => d.AnswerOptions, opt => opt.MapFrom(s => s.AnswerOptions.OrderBy(a => a.OrderIndex)));

        CreateMap<AnswerOption, AnswerOptionDto>();

        CreateMap<AnswerOption, StudentAnswerOptionDto>();

        CreateMap<TestAttempt, TestAttemptDto>()
            .ForMember(d => d.MaxScore, opt => opt.MapFrom(s => s.Test != null ? s.Test.MaxScore : 0));

        CreateMap<TestAttempt, TestAttemptDetailDto>()
            .ForMember(d => d.MaxScore, opt => opt.MapFrom(s => s.Test != null ? s.Test.MaxScore : 0))
            .ForMember(d => d.Questions, opt => opt.Ignore());

        CreateMap<TestResponse, TestResponseDto>()
            .ForMember(d => d.SelectedOptionIds, opt => opt.MapFrom(s => DeserializeOptionIds(s.SelectedOptionIds)));
    }

    private static List<string>? DeserializeOptionIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json);
        }
        catch
        {
            return null;
        }
    }
}
