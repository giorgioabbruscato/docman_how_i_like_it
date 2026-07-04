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
