-- =============================================
-- Grant Management AI - Sample Data
-- Step 2: Insert Report Sections for Remaining 2024 Q1 Reports
-- =============================================

USE GrantDB;
GO

-- =============================================
-- Grant 4 (Emily Davis - H80 School-Based) - 2024 Q1 Report
-- =============================================

DECLARE @Report4_Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId 
    FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'GX-2024-00004' 
    AND r.ReportingYear = 2024 
    AND r.ReportingQuarter = 'Q1'
);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report4_Q1, 'PerformanceNarrative', 'Performance Narrative', 1, 
'Describe your progress toward grant objectives during this reporting period.', 
'Text', 
'Our school-based health center made significant strides in Q1 2024 serving adolescent populations. We provided comprehensive health services to 890 students across three high schools, addressing physical and mental health needs. Our mental health services expanded dramatically, serving 320 students through individual counseling and group therapy sessions. We implemented a peer education program training 45 student leaders to promote health awareness among their peers. Our sports physicals program served 280 student athletes, ensuring safe participation in athletics. We partnered with school nurses to provide coordinated care for students with chronic conditions like asthma and diabetes, serving 95 students. Our reproductive health education reached 450 students with age-appropriate information. We conducted health screenings identifying and treating 120 students with previously undiagnosed conditions. Our culturally sensitive approach and school-based accessibility resulted in high utilization rates and positive health outcomes for underserved youth.',
2000);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report4_Q1, 'KeyAccomplishments', 'Key Accomplishments', 2, 
'List the major accomplishments achieved during this quarter.', 
'Text', 
'Key achievements this quarter include: First, we expanded mental health services to serve 320 students, a 60% increase from last quarter. Second, we launched a peer education program training 45 student health ambassadors. Third, we provided 280 sports physicals ensuring safe athletic participation. Fourth, we implemented a chronic disease management program for 95 students with asthma and diabetes. Fifth, we conducted comprehensive health screenings for 450 students, identifying 120 with previously undiagnosed conditions. Sixth, we established a partnership with local mental health agencies providing referrals for intensive services. Seventh, we achieved 98% immunization compliance among enrolled students. Finally, we received recognition from the school district for our innovative approach to adolescent health.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report4_Q1, 'ChallengesBarriers', 'Challenges and Barriers', 3, 
'Describe any challenges or barriers encountered and your mitigation strategies.', 
'Text', 
'We faced several challenges serving adolescent populations. Stigma around mental health services prevented some students from seeking help. We addressed this through peer education programs and normalizing mental health discussions in schools. Parental consent requirements created barriers for some students needing confidential services. We worked with school administrators and legal counsel to establish appropriate protocols balancing parental involvement with adolescent privacy. Limited clinic hours during school days restricted access. We extended hours to include after-school and early morning appointments. Some students lacked health insurance creating financial barriers. We enrolled eligible students in public programs and expanded our sliding fee scale. Transportation challenges affected access to off-site specialty care. We coordinated with school transportation and provided referral assistance. Despite these challenges, we maintained high service utilization and positive health outcomes.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseNumber)
VALUES 
(@Report4_Q1, 'PatientsServed', 'Total Patients Served', 4, 
'Enter the total number of unique patients served during this quarter.', 
'Number', 890);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseOptions)
VALUES 
(@Report4_Q1, 'ServicesProvided', 'Services Provided', 5, 
'Select all services provided during this reporting period.', 
'MultiSelect', 
'["Primary Care", "Mental Health Counseling", "Health Education", "Sports Physicals", "Immunizations", "Reproductive Health"]');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseSingle)
VALUES 
(@Report4_Q1, 'TelehealthAdoption', 'Telehealth Services', 6, 
'Are you currently providing telehealth services?', 
'Radio', 
'Yes - Partially Implemented');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report4_Q1, 'StaffingUpdates', 'Staffing Updates', 7, 
'Describe any significant staffing changes or updates.', 
'Text', 
'We expanded our team to meet growing demand for adolescent health services. We hired a licensed clinical social worker specializing in adolescent mental health. We added a nurse practitioner with pediatric and adolescent medicine expertise. We recruited a health educator to lead our peer education program and coordinate health promotion activities. We hired a part-time psychiatrist providing consultation and medication management for complex cases. All staff completed training in adolescent development, trauma-informed care, and confidentiality requirements for minor patients. We currently have 12 FTE staff members dedicated to school-based health services.',
1000);

PRINT 'Inserted sections for Grant 4 - 2024 Q1';
GO

-- =============================================
-- Grant 5 (David Wilson - C18 Homeless) - 2024 Q1 Report
-- =============================================

DECLARE @Report5_Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId 
    FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'GX-2024-00005' 
    AND r.ReportingYear = 2024 
    AND r.ReportingQuarter = 'Q1'
);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report5_Q1, 'PerformanceNarrative', 'Performance Narrative', 1, 
'Describe your progress toward grant objectives during this reporting period.', 
'Text', 
'Our Health Care for the Homeless program achieved substantial progress in Q1 2024 serving our most vulnerable populations. We provided comprehensive healthcare to 680 individuals experiencing homelessness through our street medicine program and shelter-based clinics. Our integrated behavioral health and substance abuse services served 290 patients, addressing the complex needs of this population. We established a medical respite program providing 45 patients with safe recovery environments after hospitalization. Our housing support services connected 38 patients with permanent supportive housing, addressing the root cause of health disparities. We partnered with homeless shelters and soup kitchens to provide on-site care, reaching 420 individuals who avoid traditional healthcare settings. Our harm reduction program distributed naloxone kits and provided overdose prevention education, potentially saving lives. We conducted street outreach to 150 unsheltered individuals, building trust and connecting them with services. Our trauma-informed, low-barrier approach resulted in high engagement and improved health outcomes for people experiencing homelessness.',
2000);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report5_Q1, 'KeyAccomplishments', 'Key Accomplishments', 2, 
'List the major accomplishments achieved during this quarter.', 
'Text', 
'Major accomplishments include: First, we launched a medical respite program providing safe recovery for 45 patients after hospitalization. Second, we connected 38 patients with permanent supportive housing, addressing health and housing simultaneously. Third, we established partnerships with 6 homeless shelters providing on-site healthcare. Fourth, we served 290 patients through integrated behavioral health and substance abuse services. Fifth, we distributed 200 naloxone kits through our harm reduction program, with 8 reported overdose reversals. Sixth, we conducted 150 street outreach visits building relationships with unsheltered individuals. Seventh, we achieved 85% follow-up rate for patients discharged from emergency departments. Finally, we trained 50 shelter staff in recognizing health emergencies and connecting residents with care.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report5_Q1, 'ChallengesBarriers', 'Challenges and Barriers', 3, 
'Describe any challenges or barriers encountered and your mitigation strategies.', 
'Text', 
'Serving people experiencing homelessness presents unique challenges. Many patients lack identification documents needed for enrollment. We implemented flexible enrollment procedures accepting alternative forms of identification. Mental illness and substance abuse complicated treatment adherence. We provided integrated behavioral health services and medication-assisted treatment on-site. Patients frequently missed appointments due to unstable housing. We adopted a flexible, walk-in approach and conducted street outreach. Limited medical respite beds restricted our ability to serve patients needing post-hospitalization care. We partnered with local hotels to expand capacity. Many patients had complex trauma histories requiring specialized care. We trained all staff in trauma-informed approaches and hired providers with expertise in homeless healthcare. Despite these challenges, we maintained high engagement rates and achieved positive health outcomes through our patient-centered, harm-reduction approach.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseNumber)
VALUES 
(@Report5_Q1, 'PatientsServed', 'Total Patients Served', 4, 
'Enter the total number of unique patients served during this quarter.', 
'Number', 680);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseOptions)
VALUES 
(@Report5_Q1, 'ServicesProvided', 'Services Provided', 5, 
'Select all services provided during this reporting period.', 
'MultiSelect', 
'["Primary Care", "Behavioral Health", "Substance Abuse Treatment", "Case Management", "Housing Support", "Street Outreach"]');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseSingle)
VALUES 
(@Report5_Q1, 'TelehealthAdoption', 'Telehealth Services', 6, 
'Are you currently providing telehealth services?', 
'Radio', 
'No - Not Applicable');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report5_Q1, 'StaffingUpdates', 'Staffing Updates', 7, 
'Describe any significant staffing changes or updates.', 
'Text', 
'We strengthened our team with providers experienced in homeless healthcare. We hired a street medicine physician leading our outreach efforts. We added two behavioral health clinicians specializing in trauma and substance abuse. We recruited a housing navigator connecting patients with permanent supportive housing. We hired a nurse practitioner for our medical respite program. We added two community health workers with lived experience of homelessness, bringing valuable peer perspectives. All staff completed training in trauma-informed care, harm reduction, and Housing First principles. We currently have 22 FTE staff members dedicated to serving people experiencing homelessness.',
1000);

PRINT 'Inserted sections for Grant 5 - 2024 Q1';
GO

PRINT 'Completed 2024 Q1 report sections for all 5 grants';
GO

