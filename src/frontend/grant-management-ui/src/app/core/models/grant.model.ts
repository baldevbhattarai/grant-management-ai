export interface Grant {
  grantId: string;
  grantNumber: string;
  grantType: string;
  programName: string;
  programTypeCode: number;
  focusAreas: string | null;
  fundingAmount: number | null;
  status: string;
  userName: string;
  organizationName: string;
}
