namespace HrPortal.Workflows.Domain;

internal static class WorkflowEntityExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
