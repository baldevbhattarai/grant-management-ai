namespace GrantManagement.Core.Interfaces;

public interface IVectorSearchService
{
    /// <summary>Ensure the vector collection exists (called at startup).</summary>
    Task EnsureCollectionAsync();

    /// <summary>Index or update a single report section.</summary>
    Task UpsertSectionAsync(Guid sectionId, string text, Guid grantId, Guid reportId,
        int reportingYear, string reportingQuarter, string sectionName);

    /// <summary>Index all sections in bulk (initial setup).</summary>
    Task BulkUpsertAsync(IEnumerable<VectorSectionDto> sections);

    /// <summary>Semantic search filtered to a specific grant.</summary>
    Task<List<VectorSearchResult>> SearchAsync(float[] queryVector, Guid grantId, int topN = 3, float minScore = 0.5f);

    /// <summary>Delete a section vector by ID.</summary>
    Task DeleteSectionAsync(Guid sectionId);
}

public record VectorSectionDto(
    Guid SectionId,
    string Text,
    Guid GrantId,
    Guid ReportId,
    int ReportingYear,
    string ReportingQuarter,
    string SectionName);

public record VectorSearchResult(
    Guid SectionId,
    float Score,
    string ResponseText,
    string SectionName,
    int ReportingYear,
    string ReportingQuarter,
    Guid? ReportId = null);
