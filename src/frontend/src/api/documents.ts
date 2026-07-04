import { apiClient } from '@/lib/api-client';
import type { Document, UploadDocumentInput } from '@/types/document';

export async function fetchDocuments(): Promise<Document[]> {
  const { data } = await apiClient.get<Document[]>('/v1/documents');
  return data;
}

export async function fetchDocument(id: string): Promise<Document> {
  const { data } = await apiClient.get<Document>(`/v1/documents/${id}`);
  return data;
}

export async function uploadDocument(
  input: UploadDocumentInput,
  file: File,
): Promise<Document> {
  const formData = new FormData();
  formData.append('employeeId', input.employeeId);
  formData.append('category', input.category);
  formData.append('file', file);

  const { data } = await apiClient.post<Document>('/v1/documents', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return data;
}

export async function downloadDocument(id: string): Promise<Blob> {
  const { data } = await apiClient.get<Blob>(`/v1/documents/${id}/download`, {
    responseType: 'blob',
  });
  return data;
}

export async function deleteDocument(id: string): Promise<void> {
  await apiClient.delete(`/v1/documents/${id}`);
}
