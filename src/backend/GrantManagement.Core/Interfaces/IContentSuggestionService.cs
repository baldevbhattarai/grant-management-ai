using GrantManagement.Core.DTOs;

namespace GrantManagement.Core.Interfaces;

public interface IContentSuggestionService
{
    Task<SuggestionResponseDto> GenerateSuggestionAsync(SuggestionRequestDto request);
    Task RecordFeedbackAsync(FeedbackRequestDto feedback);
    IAsyncEnumerable<string> StreamSuggestionAsync(SuggestionRequestDto request);

    /// <summary>Generates AI draft text for every missing text section in the report (up to 4 concurrent).</summary>
    Task<List<SectionDraftDto>> DraftReportAsync(Guid reportId, Guid userId);
}
