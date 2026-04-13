using GrantManagement.Core.DTOs;
using GrantManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportRepository reportRepo) : ControllerBase
{
    /// <summary>Get all reports for a grant</summary>
    [HttpGet("grant/{grantId:guid}")]
    public async Task<ActionResult<List<ReportDto>>> GetByGrant(Guid grantId)
    {
        var reports = await reportRepo.GetByGrantIdAsync(grantId);
        var dtos = reports.Select(r => new ReportDto
        {
            ReportId = r.ReportId,
            GrantId = r.GrantId,
            GrantNumber = string.Empty, // populated in detail call
            ReportingYear = r.ReportingYear,
            ReportingQuarter = r.ReportingQuarter,
            ReportType = r.ReportType,
            Status = r.Status,
            SubmittedDate = r.SubmittedDate,
            ApprovedDate = r.ApprovedDate,
            ReviewerRating = r.ReviewerRating
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>Get a single report with all sections</summary>
    [HttpGet("{reportId:guid}")]
    public async Task<ActionResult<ReportDto>> GetById(Guid reportId)
    {
        var report = await reportRepo.GetByIdWithSectionsAsync(reportId);
        if (report is null) return NotFound();

        return Ok(new ReportDto
        {
            ReportId = report.ReportId,
            GrantId = report.GrantId,
            GrantNumber = report.Grant.GrantNumber,
            ReportingYear = report.ReportingYear,
            ReportingQuarter = report.ReportingQuarter,
            ReportType = report.ReportType,
            Status = report.Status,
            SubmittedDate = report.SubmittedDate,
            ApprovedDate = report.ApprovedDate,
            ReviewerRating = report.ReviewerRating,
            Sections = report.Sections.Select(s => new ReportSectionDto
            {
                SectionId = s.SectionId,
                SectionName = s.SectionName,
                SectionTitle = s.SectionTitle,
                SectionOrder = s.SectionOrder,
                QuestionText = s.QuestionText,
                ResponseType = s.ResponseType,
                ResponseText = s.ResponseText,
                ResponseNumber = s.ResponseNumber,
                IsRequired = s.IsRequired,
                MaxLength = s.MaxLength
            }).ToList()
        });
    }

    /// <summary>Update a single report section's response</summary>
    [HttpPut("sections/{sectionId:guid}")]
    public async Task<IActionResult> UpdateSection(Guid sectionId, [FromBody] UpdateSectionRequest request)
    {
        var section = await reportRepo.GetSectionByIdAsync(sectionId);
        if (section is null) return NotFound();

        section.ResponseText = request.ResponseText;
        section.ResponseNumber = request.ResponseNumber;
        section.ResponseSingle = request.ResponseSingle;
        section.ResponseOptions = request.ResponseOptions;

        await reportRepo.UpdateSectionAsync(section);
        return NoContent();
    }
}
