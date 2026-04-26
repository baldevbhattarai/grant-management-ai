using GrantManagement.Core.DTOs;

namespace GrantManagement.Core.Interfaces;

public interface IChatbotService
{
    Task<ChatResponseDto> AskAsync(ChatRequestDto request);
    IAsyncEnumerable<string> AskStreamAsync(ChatRequestDto request);
}
