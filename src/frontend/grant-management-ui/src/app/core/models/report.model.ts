export interface Report {
  reportId: string;
  grantId: string;
  grantNumber: string;
  reportingYear: number;
  reportingQuarter: string;
  reportType: string;
  status: string;
  submittedDate: string | null;
  approvedDate: string | null;
  reviewerRating: number | null;
  sections: ReportSection[];
}

export interface ReportSection {
  sectionId: string;
  sectionName: string;
  sectionTitle: string;
  sectionOrder: number;
  questionText: string;
  responseType: string; // Text | Number | MultiSelect | Radio
  responseText: string | null;
  responseNumber: number | null;
  isRequired: boolean;
  maxLength: number | null;
}

export interface UpdateSectionRequest {
  responseText?: string | null;
  responseNumber?: number | null;
  responseSingle?: string | null;
  responseOptions?: string | null;
}
