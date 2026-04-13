-- =============================================
-- Grant Management AI - Sample Data
-- Step 2: Populate AI_ApprovedContent table
-- This table stores approved content for AI to use as examples
-- =============================================

USE GrantDB;
GO

-- =============================================
-- Extract approved content from 2024 Q1 reports
-- =============================================

INSERT INTO AI_ApprovedContent (GrantId, ReportId, ProgramTypeCode, SectionName, Content, ApprovalDate, ReviewerRating, GrantType, Keywords)
SELECT 
    r.GrantId,
    r.ReportId,
    g.ProgramTypeCode,
    rs.SectionName,
    rs.ResponseText,
    r.ApprovedDate,
    r.ReviewerRating,
    g.GrantType,
    CASE rs.SectionName
        WHEN 'PerformanceNarrative' THEN 'progress, objectives, services, patients, quality'
        WHEN 'KeyAccomplishments' THEN 'achievements, milestones, success, outcomes'
        WHEN 'ChallengesBarriers' THEN 'challenges, barriers, solutions, mitigation'
        WHEN 'StaffingUpdates' THEN 'staffing, recruitment, training, workforce'
        ELSE 'general'
    END
FROM Reports r
JOIN Grants g ON r.GrantId = g.GrantId
JOIN ReportSections rs ON r.ReportId = rs.ReportId
WHERE r.ReportingYear = 2024
  AND r.ReportingQuarter = 'Q1'
  AND r.Status = 'Approved'
  AND rs.ResponseType = 'Text'
  AND rs.ResponseText IS NOT NULL
  AND LEN(rs.ResponseText) > 100; -- Only substantial content

PRINT 'Populated AI_ApprovedContent from 2024 Q1 reports';
GO

-- Display approved content summary
SELECT 
    g.GrantType,
    ac.ProgramTypeCode,
    ac.SectionName,
    COUNT(*) as ContentCount,
    AVG(ac.ReviewerRating) as AvgRating,
    AVG(LEN(ac.Content)) as AvgLength
FROM AI_ApprovedContent ac
JOIN Grants g ON ac.GrantId = g.GrantId
GROUP BY g.GrantType, ac.ProgramTypeCode, ac.SectionName
ORDER BY g.GrantType, ac.SectionName;
GO
