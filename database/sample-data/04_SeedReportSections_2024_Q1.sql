-- =============================================
-- Grant Management AI - Sample Data
-- Step 2: Insert Report Sections for 2024 Q1
-- This creates rich narrative content for AI to learn from
-- =============================================

USE GrantDB;
GO

-- =============================================
-- Grant 1 (Alex Rivera - C16) - 2024 Q1 Report
-- =============================================

DECLARE @Report1_Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId 
    FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'GX-2024-00001' 
    AND r.ReportingYear = 2024 
    AND r.ReportingQuarter = 'Q1'
);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report1_Q1, 'PerformanceNarrative', 'Performance Narrative', 1, 
'Describe your progress toward grant objectives during this reporting period.', 
'Text', 
'During Q1 2024, our community health center made significant progress toward our grant objectives. We successfully expanded our telehealth services, reaching 450 patients who previously faced transportation barriers. Our integrated behavioral health program served 230 patients, representing a 35% increase from the previous quarter. We implemented a new patient portal that improved appointment scheduling efficiency by 40%. Our care team conducted 1,250 preventive care visits, focusing on chronic disease management for diabetes and hypertension. We also strengthened partnerships with local schools to provide health education to 500 students. Despite staffing challenges in dental services, we maintained quality care standards and received positive patient satisfaction scores averaging 4.6 out of 5. Our sliding fee scale program ensured that 65% of patients received affordable care regardless of insurance status.',
2000);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report1_Q1, 'KeyAccomplishments', 'Key Accomplishments', 2, 
'List the major accomplishments achieved during this quarter.', 
'Text', 
'Our key accomplishments this quarter include: First, we launched our telehealth platform serving 450 patients, with 92% reporting high satisfaction. Second, we hired three new behavioral health providers, expanding capacity by 40%. Third, we achieved a 95% childhood immunization rate, exceeding the national average. Fourth, we implemented a chronic disease management program enrolling 380 patients with diabetes or hypertension. Fifth, we secured a partnership with the local YMCA for fitness programs, serving 150 patients. Sixth, we reduced average wait times from 21 days to 12 days through improved scheduling. Finally, we completed staff training on trauma-informed care, enhancing our ability to serve vulnerable populations.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report1_Q1, 'ChallengesBarriers', 'Challenges and Barriers', 3, 
'Describe any challenges or barriers encountered and your mitigation strategies.', 
'Text', 
'We faced several challenges this quarter. Our primary challenge was recruiting dental providers in our rural area. We addressed this by partnering with a dental school to establish a rotation program and offering loan repayment incentives. Second, we experienced a 20% increase in demand for behavioral health services, creating wait times. We mitigated this by hiring two additional counselors and implementing group therapy sessions. Third, our patient portal adoption was slower than expected among elderly patients. We responded by offering one-on-one training sessions and creating simplified instructions in multiple languages. Fourth, supply chain disruptions affected our vaccine inventory. We coordinated with the state health department to secure alternative suppliers. Despite these challenges, we maintained service quality and continued progress toward our objectives.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseNumber)
VALUES 
(@Report1_Q1, 'PatientsServed', 'Total Patients Served', 4, 
'Enter the total number of unique patients served during this quarter.', 
'Number', 2850);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseOptions)
VALUES 
(@Report1_Q1, 'ServicesProvided', 'Services Provided', 5, 
'Select all services provided during this reporting period.', 
'MultiSelect', 
'["Primary Care", "Dental Services", "Behavioral Health", "Enabling Services", "Pharmacy", "Laboratory"]');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseSingle)
VALUES 
(@Report1_Q1, 'TelehealthAdoption', 'Telehealth Services', 6, 
'Are you currently providing telehealth services?', 
'Radio', 
'Yes - Fully Implemented');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report1_Q1, 'StaffingUpdates', 'Staffing Updates', 7, 
'Describe any significant staffing changes or updates.', 
'Text', 
'This quarter we made significant staffing additions to meet growing demand. We hired three behavioral health providers (two LCSWs and one psychologist), expanding our mental health capacity. We also added two community health workers focused on outreach to underserved populations. Our dental department experienced turnover with one dentist departing, but we have two candidates in final interviews. We promoted our nurse practitioner to Clinical Director, providing stronger clinical leadership. All staff completed required training in cultural competency and trauma-informed care. We currently have 45 FTE staff members, up from 42 last quarter.',
1000);

PRINT 'Inserted sections for Grant 1 - 2024 Q1';
GO

-- =============================================
-- Grant 2 (Jordan Park - C17 Migrant) - 2024 Q1 Report
-- =============================================

DECLARE @Report2_Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId 
    FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'GX-2024-00002' 
    AND r.ReportingYear = 2024 
    AND r.ReportingQuarter = 'Q1'
);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report2_Q1, 'PerformanceNarrative', 'Performance Narrative', 1, 
'Describe your progress toward grant objectives during this reporting period.', 
'Text', 
'Our migrant health center achieved substantial progress in Q1 2024. We launched a mobile health unit that traveled to 12 agricultural sites, providing on-site care to 380 migrant workers who previously lacked access to healthcare. Our bilingual outreach team conducted 450 home visits, connecting families with essential services. We established a chronic disease management program specifically designed for our migrant population, enrolling 150 patients with diabetes or hypertension. Our partnership with local food banks addressed food insecurity affecting 200 patient families. We provided culturally appropriate health education in Spanish and indigenous languages to 300 community members. Our dental services expanded to evening hours, serving 100 additional patients. We achieved a 90% childhood immunization rate, exceeding our target of 85%. Our patient satisfaction scores remained high at 4.7 out of 5, reflecting our commitment to culturally competent care.',
2000);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report2_Q1, 'KeyAccomplishments', 'Key Accomplishments', 2, 
'List the major accomplishments achieved during this quarter.', 
'Text', 
'Our key milestones this quarter include: First, we successfully launched our mobile health unit, completing 48 site visits and serving 380 migrant workers. Second, we hired two bilingual community health workers who conducted 450 outreach visits and improved our cultural competency. Third, we established a partnership with the local food bank to address food insecurity affecting 200 patient families. Fourth, we implemented a chronic disease management program enrolling 150 patients with diabetes or hypertension. Fifth, we secured additional funding for dental services, allowing us to expand hours and serve 100 additional patients. Sixth, we achieved a 90% immunization rate for children under 5. Finally, we developed health education materials in three languages, reaching 300 community members with preventive care information.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report2_Q1, 'ChallengesBarriers', 'Challenges and Barriers', 3, 
'Describe any challenges or barriers encountered and your mitigation strategies.', 
'Text', 
'We encountered several challenges serving our migrant population. Transportation barriers prevented many patients from accessing our main clinic. We addressed this by deploying our mobile health unit to agricultural work sites and offering evening hours. Language barriers affected communication with patients speaking indigenous languages. We hired interpreters and developed multilingual health education materials. Seasonal migration patterns created continuity of care challenges. We implemented a robust referral system and electronic health record sharing with partner clinics in other regions. Many patients lacked health insurance, creating financial barriers. We expanded our sliding fee scale program and connected patients with enrollment assistance for public programs. Despite these challenges, we maintained high-quality care and continued expanding access to underserved migrant communities.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseNumber)
VALUES 
(@Report2_Q1, 'PatientsServed', 'Total Patients Served', 4, 
'Enter the total number of unique patients served during this quarter.', 
'Number', 1650);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseOptions)
VALUES 
(@Report2_Q1, 'ServicesProvided', 'Services Provided', 5, 
'Select all services provided during this reporting period.', 
'MultiSelect', 
'["Primary Care", "Dental Services", "Behavioral Health", "Enabling Services", "Outreach Services", "Case Management"]');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseSingle)
VALUES 
(@Report2_Q1, 'TelehealthAdoption', 'Telehealth Services', 6, 
'Are you currently providing telehealth services?', 
'Radio', 
'Planned for Implementation');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report2_Q1, 'StaffingUpdates', 'Staffing Updates', 7, 
'Describe any significant staffing changes or updates.', 
'Text', 
'We made strategic staffing additions to better serve our migrant population. We hired two bilingual community health workers fluent in Spanish and indigenous languages. We added a mobile health unit nurse practitioner and medical assistant to staff our new mobile clinic. We recruited a dentist willing to work evening hours to accommodate agricultural workers. Our outreach coordinator position was filled by a former migrant worker with deep community connections. All staff completed cultural competency training focused on migrant health issues. We currently have 28 FTE staff members, representing a 15% increase from last quarter.',
1000);

PRINT 'Inserted sections for Grant 2 - 2024 Q1';
GO

-- =============================================
-- Grant 3 (Morgan Chen - C16 Rural) - 2024 Q1 Report
-- =============================================

DECLARE @Report3_Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId 
    FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'GX-2024-00003' 
    AND r.ReportingYear = 2024 
    AND r.ReportingQuarter = 'Q1'
);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report3_Q1, 'PerformanceNarrative', 'Performance Narrative', 1, 
'Describe your progress toward grant objectives during this reporting period.', 
'Text', 
'Our rural health center made excellent progress in Q1 2024 despite geographic challenges. We expanded telehealth services to reach 520 patients across five counties, reducing travel burdens for rural residents. Our chronic disease management program enrolled 290 patients, providing coordinated care for diabetes, hypertension, and heart disease. We partnered with local pharmacies to improve medication access, serving 180 patients through our pharmacy network. Our dental services treated 340 patients, including 120 children through school-based programs. We implemented a care coordination program that reduced hospital readmissions by 25%. Our behavioral health integration served 175 patients, addressing the mental health crisis in rural areas. We conducted community health screenings at 8 rural events, reaching 400 residents. Our patient-centered medical home model achieved recognition, demonstrating our commitment to quality care in underserved rural communities.',
2000);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report3_Q1, 'KeyAccomplishments', 'Key Accomplishments', 2, 
'List the major accomplishments achieved during this quarter.', 
'Text', 
'Major accomplishments this quarter include: First, we expanded telehealth to 520 patients across five counties, with 94% satisfaction rates. Second, we achieved Patient-Centered Medical Home Level 3 recognition, validating our quality improvement efforts. Third, we reduced hospital readmissions by 25% through our care coordination program. Fourth, we established partnerships with three rural pharmacies, improving medication access for 180 patients. Fifth, we launched school-based dental services reaching 120 children in underserved areas. Sixth, we implemented a diabetes prevention program enrolling 85 pre-diabetic patients. Seventh, we secured broadband internet upgrades enabling better telehealth connectivity. Finally, we trained all staff in rural health best practices and social determinants of health.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report3_Q1, 'ChallengesBarriers', 'Challenges and Barriers', 3, 
'Describe any challenges or barriers encountered and your mitigation strategies.', 
'Text', 
'Rural healthcare delivery presents unique challenges. Geographic isolation and long travel distances limited patient access. We addressed this by expanding telehealth services and establishing satellite clinics in two remote communities. Broadband internet limitations affected telehealth quality. We partnered with the state broadband initiative to upgrade connectivity and provided mobile hotspots to patients. Recruiting and retaining healthcare providers in rural areas remained difficult. We offered loan repayment, housing assistance, and professional development opportunities. Many patients faced economic hardships affecting their ability to afford care. We expanded our sliding fee scale and connected patients with financial assistance programs. Limited public transportation created access barriers. We coordinated with local transit services and provided transportation vouchers. Despite these challenges, we maintained high-quality care and continued expanding access to rural populations.',
1500);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseNumber)
VALUES 
(@Report3_Q1, 'PatientsServed', 'Total Patients Served', 4, 
'Enter the total number of unique patients served during this quarter.', 
'Number', 2100);

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseOptions)
VALUES 
(@Report3_Q1, 'ServicesProvided', 'Services Provided', 5, 
'Select all services provided during this reporting period.', 
'MultiSelect', 
'["Primary Care", "Dental Services", "Behavioral Health", "Chronic Disease Management", "Telehealth", "Care Coordination"]');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseSingle)
VALUES 
(@Report3_Q1, 'TelehealthAdoption', 'Telehealth Services', 6, 
'Are you currently providing telehealth services?', 
'Radio', 
'Yes - Fully Implemented');

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
VALUES 
(@Report3_Q1, 'StaffingUpdates', 'Staffing Updates', 7, 
'Describe any significant staffing changes or updates.', 
'Text', 
'We strengthened our rural health workforce this quarter. We recruited a family medicine physician through the National Health Service Corps, adding critical primary care capacity. We hired a psychiatric nurse practitioner to address mental health needs in our rural community. We added two care coordinators to support our chronic disease management program. We recruited a dental hygienist to expand preventive dental services. Our telehealth coordinator position was filled by a nurse with extensive rural health experience. We provided loan repayment assistance to three providers, supporting retention. All staff completed training in rural health challenges and telehealth best practices. We currently have 38 FTE staff members.',
1000);

PRINT 'Inserted sections for Grant 3 - 2024 Q1';
GO


