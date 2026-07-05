import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ProjectForm } from '@/components/projects/project-form';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner } from '@/components/ui/loading-spinner';
import { useCreateProject } from '@/hooks/use-projects';
import { Permission, hasPermission } from '@/lib/auth-permissions';
import { getApiErrorMessage } from '@/lib/utils';
import { useAuthStore } from '@/stores/auth-store';
import type { CreateProjectRequest } from '@/types/project';

export function ProjectCreatePage() {
  const navigate = useNavigate();
  const permissions = useAuthStore((state) => state.permissions);
  const canCreate = hasPermission(permissions, Permission.ProjectCreateTenant);
  const createProject = useCreateProject();
  const [error, setError] = useState<string | null>(null);

  if (!canCreate) {
    return (
      <div className="space-y-6">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">New Project</h2>
          <p className="text-muted-foreground">
            You do not have permission to create projects.
          </p>
        </div>
      </div>
    );
  }

  const handleSubmit = async (data: CreateProjectRequest) => {
    try {
      setError(null);
      const project = await createProject.mutateAsync(data);
      navigate(`/projects/${project.id}`);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to create project.'));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">New Project</h2>
          <p className="text-muted-foreground">Create a new project.</p>
        </div>
        <Link to="/projects">
          <Button type="button" variant="outline">Back to list</Button>
        </Link>
      </div>

      {error && <ErrorBanner message={error} />}

      <Card>
        <CardHeader>
          <CardTitle>Project Details</CardTitle>
        </CardHeader>
        <CardContent>
          <ProjectForm onSubmit={handleSubmit} submitLabel="Create Project" />
        </CardContent>
      </Card>
    </div>
  );
}
