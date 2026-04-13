using GrantManagement.Core.DTOs;

namespace GrantManagement.Core.Interfaces;

public interface IContentSuggestionService
{
    Task<SuggestionResponseDto> GenerateSuggestionAsync(SuggestionRequestDto request);
    Task RecordFeedbackAsync(FeedbackRequestDto feedback);
}
