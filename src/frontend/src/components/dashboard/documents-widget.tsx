import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { fetchDocuments } from '@/api/documents';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import { WidgetSkeleton } from './widget-primitives';

export function DocumentsWidget() {
  const canRead = useHasPermission(Permission.DocumentReadSelf);
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard-documents'],
    queryFn: fetchDocuments,
    enabled: canRead,
  });

  if (!canRead) return null;
  if (isLoading) return <WidgetSkeleton title="Recent documents" />;

  const recent = [...(data ?? [])]
    .sort((a, b) => new Date(b.uploadedAt).getTime() - new Date(a.uploadedAt).getTime())
    .slice(0, 5);

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-base">Recent documents</CardTitle>
        <Link to="/documents" className="text-sm text-primary hover:underline">
          View all
        </Link>
      </CardHeader>
      <CardContent>
        {recent.length === 0 ? (
          <p className="text-sm text-muted-foreground">No documents yet.</p>
        ) : (
          <ul className="space-y-2">
            {recent.map((doc) => (
              <li key={doc.id} className="flex items-center justify-between gap-2 text-sm">
                <span className="truncate font-medium">{doc.fileName}</span>
                <span className="shrink-0 text-muted-foreground">{doc.category}</span>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
