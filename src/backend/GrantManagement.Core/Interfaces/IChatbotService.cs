using GrantManagement.Core.DTOs;

namespace GrantManagement.Core.Interfaces;

public interface IChatbotService
{
    Task<ChatResponseDto> AskAsync(ChatRequestDto request);
    IAsyncEnumerable<string> AskStreamAsync(ChatRequestDto request);

    /// <summary>Answers a question by retrieving context from multiple grants and synthesizing a comparison.</summary>
    Task<CompareGrantsResponseDto> CompareGrantsAsync(CompareGrantsRequestDto request);
}
