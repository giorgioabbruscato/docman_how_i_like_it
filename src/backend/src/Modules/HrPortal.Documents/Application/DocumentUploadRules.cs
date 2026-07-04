using HrPortal.SharedKernel.Results;

namespace HrPortal.Documents.Application;

public static class DocumentUploadRules
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public static Result Validate(string contentType, long sizeBytes)
    {
        if (sizeBytes <= 0)
            return Result.Failure("File is required.", "VALIDATION");

        if (sizeBytes > MaxFileSizeBytes)
            return Result.Failure("File exceeds the maximum size of 10 MB.", "VALIDATION");

        if (!AllowedContentTypes.Contains(contentType))
            return Result.Failure("File type is not allowed.", "VALIDATION");

        return Result.Success();
    }
}
