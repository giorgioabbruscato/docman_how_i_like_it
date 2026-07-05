import { useEffect, useState } from 'react';
import { getProjects } from '@/api/projects';
import { getTasks } from '@/api/tasks';
import { EmptyState } from '@/components/ui/loading-spinner';
import { Select } from '@/components/ui/select';
import { Permission, hasPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';
import type { ProjectDto } from '@/types/project';
import type { ProjectTaskDto } from '@/types/task';

interface ProjectTaskSelectorProps {
  projectId: string;
  taskId: string;
  onProjectChange: (projectId: string) => void;
  onTaskChange: (taskId: string) => void;
  disabled?: boolean;
}

export function ProjectTaskSelector({
  projectId,
  taskId,
  onProjectChange,
  onTaskChange,
  disabled,
}: ProjectTaskSelectorProps) {
  const permissions = useAuthStore((state) => state.permissions);
  const canReadProjects = hasPermission(permissions, Permission.ProjectReadTenant);
  const canReadTasks = hasPermission(permissions, Permission.TaskReadTenant);

  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [tasks, setTasks] = useState<ProjectTaskDto[]>([]);
  const [loadingProjects, setLoadingProjects] = useState(false);
  const [loadingTasks, setLoadingTasks] = useState(false);

  useEffect(() => {
    if (!canReadProjects) return;
    setLoadingProjects(true);
    getProjects({ pageSize: 100, isArchived: false })
      .then((result) => setProjects(result.items))
      .catch(() => setProjects([]))
      .finally(() => setLoadingProjects(false));
  }, [canReadProjects]);

  useEffect(() => {
    if (!canReadTasks || !projectId) {
      setTasks([]);
      return;
    }
    setLoadingTasks(true);
    getTasks({ projectId, pageSize: 100 })
      .then((result) => setTasks(result.items))
      .catch(() => setTasks([]))
      .finally(() => setLoadingTasks(false));
  }, [canReadTasks, projectId]);

  if (!canReadProjects) {
    return (
      <EmptyState message="Project selection requires project.read:tenant permission. Contact your administrator to assign projects, or use manual entry with a known project ID when available." />
    );
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <div>
        <label className="mb-1 block text-xs text-muted-foreground">Project</label>
        <Select
          value={projectId}
          onChange={(e) => {
            onProjectChange(e.target.value);
            onTaskChange('');
          }}
          disabled={disabled || loadingProjects}
        >
          <option value="">Select project</option>
          {projects.map((project) => (
            <option key={project.id} value={project.id}>
              {project.name}
            </option>
          ))}
        </Select>
      </div>
      <div>
        <label className="mb-1 block text-xs text-muted-foreground">Task (optional)</label>
        <Select
          value={taskId}
          onChange={(e) => onTaskChange(e.target.value)}
          disabled={disabled || !projectId || loadingTasks || !canReadTasks}
        >
          <option value="">No task</option>
          {tasks.map((task) => (
            <option key={task.id} value={task.id}>
              {task.title}
            </option>
          ))}
        </Select>
      </div>
    </div>
  );
}
