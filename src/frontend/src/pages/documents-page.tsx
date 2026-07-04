import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { deleteDocument, downloadDocument, fetchDocuments, uploadDocument } from '@/api/documents';
import { fetchEmployees } from '@/api/employees';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { hasAnyRole, HR_OR_ADMIN_ROLES, MANAGER_OR_ABOVE_ROLES } from '@/lib/auth-roles';
import {
  confirmAction,
  formatDateTime,
  formatFileSize,
  getApiErrorMessage,
  triggerBlobDownload,
} from '@/lib/utils';
import { useAuthStore } from '@/stores/auth-store';
import { DOCUMENT_CATEGORIES } from '@/types/document';
import type { Document } from '@/types/document';
import type { Employee } from '@/types/employee';

const uploadSchema = z.object({
  employeeId: z.string().min(1, 'Required'),
  category: z.string().min(1, 'Required'),
});

type UploadForm = z.infer<typeof uploadSchema>;

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;

export function DocumentsPage() {
  const user = useAuthStore((state) => state.user);
  const isManagerOrAbove = hasAnyRole(user?.roles ?? [], ...MANAGER_OR_ABOVE_ROLES);
  const isHrOrAdmin = hasAnyRole(user?.roles ?? [], ...HR_OR_ADMIN_ROLES);

  const [documents, setDocuments] = useState<Document[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<UploadForm>({
    resolver: zodResolver(uploadSchema),
    defaultValues: { category: 'Other' },
  });

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      if (isManagerOrAbove) {
        const [documentData, employeeData] = await Promise.all([
          fetchDocuments(),
          fetchEmployees(),
        ]);
        setDocuments(documentData);
        setEmployees(employeeData);
      } else {
        const employeeData = await fetchEmployees().catch(() => []);
        setEmployees(employeeData);
      }
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to load documents.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [isManagerOrAbove]);

  const onSubmit = async (data: UploadForm) => {
    if (!selectedFile) {
      setError('Please select a file to upload.');
      return;
    }
    if (selectedFile.size > MAX_FILE_SIZE_BYTES) {
      setError('File size exceeds the maximum of 10 MB.');
      return;
    }

    try {
      setError(null);
      await uploadDocument(
        { employeeId: data.employeeId, category: data.category },
        selectedFile,
      );
      reset({ category: 'Other' });
      setSelectedFile(null);
      if (fileInputRef.current) fileInputRef.current.value = '';
      if (isManagerOrAbove) await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to upload document.'));
    }
  };

  const handleDownload = async (doc: Document) => {
    try {
      setError(null);
      const blob = await downloadDocument(doc.id);
      triggerBlobDownload(blob, doc.fileName);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to download document.'));
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirmAction('Delete this document permanently?')) return;
    try {
      setError(null);
      await deleteDocument(id);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to delete document.'));
    }
  };

  const employeeName = (id: string) => {
    const employee = employees.find((e) => e.id === id);
    return employee ? `${employee.firstName} ${employee.lastName}` : id;
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Documents</h2>
        <p className="text-muted-foreground">Upload and manage employee documents.</p>
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Upload Document</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div>
                {isManagerOrAbove ? (
                  <Select {...register('employeeId')}>
                    <option value="">Select employee</option>
                    {employees.map((employee) => (
                      <option key={employee.id} value={employee.id}>
                        {employee.firstName} {employee.lastName}
                      </option>
                    ))}
                  </Select>
                ) : (
                  <>
                    <Input placeholder="Employee ID (UUID)" {...register('employeeId')} />
                    <p className="mt-1 text-xs text-muted-foreground">
                      Enter your employee record ID. Contact HR if you do not have it.
                    </p>
                  </>
                )}
                {errors.employeeId && (
                  <p className="mt-1 text-xs text-red-600">{errors.employeeId.message}</p>
                )}
              </div>
              <Select {...register('category')}>
                {DOCUMENT_CATEGORIES.map((category) => (
                  <option key={category} value={category}>
                    {category}
                  </option>
                ))}
              </Select>
              <div>
                <Input
                  ref={fileInputRef}
                  type="file"
                  accept=".pdf,.jpg,.jpeg,.png,.docx,application/pdf,image/jpeg,image/png,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                  onChange={(e) => setSelectedFile(e.target.files?.[0] ?? null)}
                />
                <p className="mt-1 text-xs text-muted-foreground">
                  Allowed: PDF, JPEG, PNG, DOCX. Max 10 MB.
                </p>
              </div>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Uploading...' : 'Upload Document'}
              </Button>
            </form>
          </CardContent>
        </Card>

        {isManagerOrAbove && (
          <Card>
            <CardHeader>
              <CardTitle>Document List</CardTitle>
            </CardHeader>
            <CardContent>
              {loading ? (
                <LoadingSpinner label="Loading documents" />
              ) : documents.length === 0 ? (
                <EmptyState message="No documents found." />
              ) : (
                <ul className="divide-y divide-border">
                  {documents.map((doc) => (
                    <li key={doc.id} className="flex items-start justify-between gap-2 py-3">
                      <div>
                        <p className="font-medium">{doc.fileName}</p>
                        <p className="text-sm text-muted-foreground">
                          {employeeName(doc.employeeId)} · {doc.category}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {formatFileSize(doc.sizeBytes)} · {formatDateTime(doc.uploadedAt)}
                        </p>
                      </div>
                      <div className="flex gap-2">
                        <Button size="sm" variant="outline" onClick={() => handleDownload(doc)}>
                          Download
                        </Button>
                        {isHrOrAdmin && (
                          <Button size="sm" variant="destructive" onClick={() => handleDelete(doc.id)}>
                            Delete
                          </Button>
                        )}
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
