using GrantManagement.Core.DTOs;
using GrantManagement.Core.Interfaces;
using GrantManagement.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/ai/documents")]
public class AIDocumentController(
    IDocumentProcessingService docService,
    IDocumentRepository docRepo) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".pdf", ".docx", ".txt"];

    /// <summary>Upload and index a document (PDF, DOCX, or TXT) for a grant.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<UploadedDocumentDto>> Upload(
        IFormFile file,
        [FromQuery] Guid grantId,
        [FromQuery] Guid userId)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest($"Unsupported file type. Allowed: {string.Join(", ", AllowedExtensions)}");

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("File exceeds the maximum size of 10MB");

        if (grantId == Guid.Empty)
            return BadRequest("grantId is required");

        try
        {
            await using var stream = file.OpenReadStream();
            var doc = await docService.ProcessAndIndexAsync(
                stream, file.FileName, file.ContentType ?? "application/octet-stream",
                file.Length, grantId, userId);

            return Ok(ToDto(doc));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>List all uploaded documents for a grant.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UploadedDocumentDto>>> GetDocuments([FromQuery] Guid grantId)
    {
        if (grantId == Guid.Empty)
            return BadRequest("grantId is required");

        var docs = await docRepo.GetByGrantAsync(grantId);
        return Ok(docs.Select(ToDto).ToList());
    }

    /// <summary>Delete an uploaded document and its vectors.</summary>
    [HttpDelete("{documentId}")]
    public async Task<IActionResult> Delete(Guid documentId)
    {
        await docService.DeleteAsync(documentId);
        return NoContent();
    }

    private static UploadedDocumentDto ToDto(Core.Entities.AIUploadedDocument d) =>
        new(d.DocumentId, d.FileName, d.ContentType, d.FileSizeBytes, d.ChunkCount, d.UploadedAt, d.IsIndexed);
}
