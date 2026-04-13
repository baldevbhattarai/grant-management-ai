-- =============================================
-- Grant Management AI - Sample Data
-- Step 8: Insert Report Sections for 2024 Q2/Q3/Q4 (all 5 grants)
-- =============================================

USE GrantDB;
GO

DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER, Quarter VARCHAR(10));
INSERT INTO @ReportIds
SELECT r.ReportId, r.ReportingQuarter
FROM Reports r
WHERE r.ReportingYear = 2024 AND r.ReportingQuarter IN ('Q2','Q3','Q4');

-- PerformanceNarrative
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT r.ReportId, 'PerformanceNarrative', 'Performance Narrative', 1,
'Describe your progress toward grant objectives during this reporting period.',
'Text',
CASE r.Quarter
  WHEN 'Q2' THEN 'During Q2 2024, we continued to expand our health services and meet grant objectives. Patient visits increased 12% over Q1 as we launched our summer outreach program. We provided preventive care to 1,050 patients and expanded telehealth services to rural populations. Staff capacity building included training 8 new care coordinators. We strengthened partnerships with local social service organizations to address social determinants of health.'
  WHEN 'Q3' THEN 'Q3 2024 demonstrated strong progress across all program areas. We reached 1,180 patients, our highest quarterly total, driven by expanded hours and a mobile health unit deployment. Behavioral health integration advanced with a co-located psychiatrist joining our team. We met 94% of our UDS performance benchmarks and launched a chronic disease management program serving 210 high-risk patients.'
  WHEN 'Q4' THEN 'Q4 2024 marked the completion of a highly successful program year. We served a total of 4,820 unduplicated patients, exceeding our annual goal of 4,500. Quality improvement initiatives resulted in improved hypertension control rates (from 68% to 74%) and diabetes control rates (from 61% to 69%). We completed infrastructure upgrades enabling expanded services in 2025 and submitted all required UDS data on time.'
END,
2000
FROM @ReportIds r;
GO

DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER, Quarter VARCHAR(10));
INSERT INTO @ReportIds
SELECT r.ReportId, r.ReportingQuarter
FROM Reports r
WHERE r.ReportingYear = 2024 AND r.ReportingQuarter IN ('Q2','Q3','Q4');

-- KeyAccomplishments
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT r.ReportId, 'KeyAccomplishments', 'Key Accomplishments', 2,
'List the major accomplishments achieved during this quarter.',
'Text',
CASE r.Quarter
  WHEN 'Q2' THEN 'Key Q2 accomplishments: Launched summer youth health program serving 320 adolescents. Received PCMH Level 3 recognition. Hired two bilingual medical assistants improving access for Spanish-speaking patients. Implemented EHR patient portal with 380 active users. Completed staff training on trauma-informed care.'
  WHEN 'Q3' THEN 'Key Q3 accomplishments: Deployed mobile health unit completing 14 community visits. Achieved highest quarterly patient volume. Integrated behavioral health with primary care for 95% of high-risk patients. Exceeded HEDIS benchmarks for childhood immunizations. Secured $150K supplemental grant for dental expansion.'
  WHEN 'Q4' THEN 'Key Q4 accomplishments: Exceeded annual patient volume goal. Achieved significant improvements in hypertension and diabetes control rates. Completed facility renovations adding two exam rooms. Submitted UDS report on time with no deficiencies. Onboarded two new providers for 2025 capacity.'
END,
1500
FROM @ReportIds r;
GO

DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER, Quarter VARCHAR(10));
INSERT INTO @ReportIds
SELECT r.ReportId, r.ReportingQuarter
FROM Reports r
WHERE r.ReportingYear = 2024 AND r.ReportingQuarter IN ('Q2','Q3','Q4');

-- ChallengesBarriers
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT r.ReportId, 'ChallengesBarriers', 'Challenges and Barriers', 3,
'Describe any challenges or barriers encountered and your mitigation strategies.',
'Text',
CASE r.Quarter
  WHEN 'Q2' THEN 'Q2 challenges included staff turnover with two medical assistants leaving mid-quarter. We mitigated by cross-training existing staff and accelerating recruitment, filling positions within 6 weeks. Patient no-show rates rose to 18% during summer; we implemented reminder calls and flexible scheduling to reduce this to 14% by quarter end.'
  WHEN 'Q3' THEN 'Q3 challenges included supply chain delays affecting medical supply procurement. We maintained a 60-day buffer stock and sourced alternative vendors. Hurricane preparedness required temporary clinic closure for one day; we implemented telehealth continuity during that period with no disruption to chronic disease management patients.'
  WHEN 'Q4' THEN 'Q4 challenges included year-end insurance verification backlogs affecting revenue cycle. We implemented a pre-visit verification process reducing claim denials by 22%. Flu season surge required extended hours and temporary staffing; we partnered with a locum agency to maintain access without compromising care quality.'
END,
1500
FROM @ReportIds r;
GO

DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER, Quarter VARCHAR(10));
INSERT INTO @ReportIds
SELECT r.ReportId, r.ReportingQuarter
FROM Reports r
WHERE r.ReportingYear = 2024 AND r.ReportingQuarter IN ('Q2','Q3','Q4');

-- PatientsServed
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseNumber)
SELECT r.ReportId, 'PatientsServed', 'Total Patients Served', 4,
'Enter the total number of unique patients served during this quarter.',
'Number',
CASE r.Quarter WHEN 'Q2' THEN 1050 WHEN 'Q3' THEN 1180 ELSE 1140 END
FROM @ReportIds r;
GO

DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER, Quarter VARCHAR(10));
INSERT INTO @ReportIds
SELECT r.ReportId, r.ReportingQuarter
FROM Reports r
WHERE r.ReportingYear = 2024 AND r.ReportingQuarter IN ('Q2','Q3','Q4');

-- ServicesProvided
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseOptions)
SELECT r.ReportId, 'ServicesProvided', 'Services Provided', 5,
'Select all services provided during this reporting period.',
'MultiSelect',
'["Primary Care","Behavioral Health","Dental Services","Enabling Services"]'
FROM @ReportIds r;
GO

DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER, Quarter VARCHAR(10));
INSERT INTO @ReportIds
SELECT r.ReportId, r.ReportingQuarter
FROM Reports r
WHERE r.ReportingYear = 2024 AND r.ReportingQuarter IN ('Q2','Q3','Q4');

-- TelehealthAdoption
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseSingle)
SELECT r.ReportId, 'TelehealthAdoption', 'Telehealth Services', 6,
'Are you currently providing telehealth services?',
'Radio',
'Yes - Fully Implemented'
FROM @ReportIds r;
GO

DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER, Quarter VARCHAR(10));
INSERT INTO @ReportIds
SELECT r.ReportId, r.ReportingQuarter
FROM Reports r
WHERE r.ReportingYear = 2024 AND r.ReportingQuarter IN ('Q2','Q3','Q4');

-- StaffingUpdates
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT r.ReportId, 'StaffingUpdates', 'Staffing Updates', 7,
'Describe any significant staffing changes or updates.',
'Text',
CASE r.Quarter
  WHEN 'Q2' THEN 'Q2 staffing updates: Hired two bilingual medical assistants. Two MA departures managed through cross-training. Total FTE count maintained at 18.5. Initiated recruitment for a second full-time dentist.'
  WHEN 'Q3' THEN 'Q3 staffing updates: Welcomed a co-located psychiatrist (0.5 FTE). Promoted senior MA to Care Coordinator role. Completed all required HIPAA and compliance training. Total FTE: 19.5.'
  WHEN 'Q4' THEN 'Q4 staffing updates: Onboarded two new providers starting January 2025. Completed annual performance reviews. Maintained zero clinical vacancy rate. Total FTE at year-end: 20.0.'
END,
1000
FROM @ReportIds r;
GO

SELECT r.ReportingQuarter, COUNT(rs.SectionId) as Sections
FROM Reports r JOIN ReportSections rs ON r.ReportId = rs.ReportId
WHERE r.ReportingYear = 2024
GROUP BY r.ReportingQuarter
ORDER BY r.ReportingQuarter;
GO
