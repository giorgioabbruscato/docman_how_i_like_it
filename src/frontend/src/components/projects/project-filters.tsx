import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import type { ProjectStatus } from '@/types/project';

const STATUSES: (ProjectStatus | '')[] = ['', 'Active', 'OnHold', 'Completed', 'Cancelled'];

interface ProjectFiltersProps {
  search: string;
  customerName: string;
  status: ProjectStatus | '';
  isArchived: boolean;
  onSearchChange: (value: string) => void;
  onCustomerNameChange: (value: string) => void;
  onStatusChange: (value: ProjectStatus | '') => void;
  onIsArchivedChange: (value: boolean) => void;
  onApply: () => void;
  onReset: () => void;
}

export function ProjectFilters({
  search,
  customerName,
  status,
  isArchived,
  onSearchChange,
  onCustomerNameChange,
  onStatusChange,
  onIsArchivedChange,
  onApply,
  onReset,
}: ProjectFiltersProps) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onApply();
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Filters</CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-4">
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Search</label>
            <Input
              placeholder="Project name"
              value={search}
              onChange={(e) => onSearchChange(e.target.value)}
            />
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Customer</label>
            <Input
              placeholder="Customer name"
              value={customerName}
              onChange={(e) => onCustomerNameChange(e.target.value)}
            />
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Status</label>
            <Select
              value={status}
              onChange={(e) => onStatusChange(e.target.value as ProjectStatus | '')}
            >
              {STATUSES.map((value) => (
                <option key={value || 'all'} value={value}>
                  {value || 'All statuses'}
                </option>
              ))}
            </Select>
          </div>
          <div className="flex items-center gap-2 pb-2">
            <input
              id="archived-filter"
              type="checkbox"
              checked={isArchived}
              onChange={(e) => onIsArchivedChange(e.target.checked)}
              className="h-4 w-4 rounded border-border"
            />
            <label htmlFor="archived-filter" className="text-sm">
              Show archived
            </label>
          </div>
          <Button type="submit">Apply</Button>
          <Button type="button" variant="outline" onClick={onReset}>
            Reset
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
