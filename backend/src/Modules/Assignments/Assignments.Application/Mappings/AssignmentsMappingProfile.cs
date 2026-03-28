using Assignments.Application.DTOs;
using Assignments.Domain.Entities;
using AutoMapper;

namespace Assignments.Application.Mappings;

public class AssignmentsMappingProfile : Profile
{
    public AssignmentsMappingProfile()
    {
        CreateMap<Assignment, AssignmentDto>()
            .ForMember(d => d.SubmissionsCount, opt => opt.MapFrom(s => s.Submissions.Count));

        CreateMap<Assignment, AssignmentDetailDto>()
            .ForMember(d => d.SubmissionsCount, opt => opt.MapFrom(s => s.Submissions.Count))
            .ForMember(d => d.Submissions, opt => opt.MapFrom(s => s.Submissions.OrderByDescending(sub => sub.SubmittedAt)));

        CreateMap<AssignmentSubmission, SubmissionDto>()
            .ForMember(d => d.MaxScore, opt => opt.MapFrom(s => s.Assignment != null ? s.Assignment.MaxScore : 0));

        CreateMap<AssignmentSubmission, SubmissionDetailDto>()
            .ForMember(d => d.MaxScore, opt => opt.MapFrom(s => s.Assignment != null ? s.Assignment.MaxScore : 0))
            .ForMember(d => d.AttachmentFileNames, opt => opt.Ignore());
    }
}
