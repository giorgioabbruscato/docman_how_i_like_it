export type DocumentCategory =
  | 'Contract'
  | 'IdentityDocument'
  | 'Certificate'
  | 'Payslip'
  | 'Other';

export interface Document {
  id: string;
  employeeId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  category: DocumentCategory | string;
  uploadedAt: string;
  uploadedBy?: string;
}

export interface UploadDocumentInput {
  employeeId: string;
  category: DocumentCategory | string;
}

export const DOCUMENT_CATEGORIES: DocumentCategory[] = [
  'Contract',
  'IdentityDocument',
  'Certificate',
  'Payslip',
  'Other',
];
