import { Component, type ReactNode } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

interface Props {
  title: string;
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

export class WidgetErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  render() {
    if (this.state.hasError) {
      return (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{this.props.title}</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-muted-foreground">Unable to load widget.</CardContent>
        </Card>
      );
    }
    return this.props.children;
  }
}

export function WidgetSkeleton({ title }: { title: string }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
      </CardHeader>
      <CardContent className="text-sm text-muted-foreground">Loading…</CardContent>
    </Card>
  );
}
