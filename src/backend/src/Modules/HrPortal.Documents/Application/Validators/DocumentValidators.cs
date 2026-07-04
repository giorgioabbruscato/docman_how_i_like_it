using FluentValidation;
using HrPortal.Documents.Application.Dtos;
using HrPortal.Documents.Domain;

namespace HrPortal.Documents.Application.Validators;

public sealed class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    public UploadDocumentRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(c => Enum.TryParse<DocumentCategory>(c, true, out _))
            .WithMessage("Invalid document category.");
    }
}

/// <summary>
/// Validates uploaded file metadata. MIME whitelist and size limits are defined in <see cref="DocumentUploadRules"/>.
/// </summary>
public sealed class DocumentFileMetadataValidator
{
    public static FluentValidation.Results.ValidationResult Validate(string contentType, long sizeBytes)
    {
        var result = DocumentUploadRules.Validate(contentType, sizeBytes);
        if (result.IsSuccess)
            return new FluentValidation.Results.ValidationResult();

        return new FluentValidation.Results.ValidationResult([
            new FluentValidation.Results.ValidationFailure("File", result.Error!)
        ]);
    }
}
