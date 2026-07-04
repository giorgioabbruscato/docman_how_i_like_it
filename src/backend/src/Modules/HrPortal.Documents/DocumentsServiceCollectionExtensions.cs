using HrPortal.Documents.Application;
using HrPortal.Documents.Application.Validators;
using HrPortal.Documents.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Documents;

public static class DocumentsServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentsModule(this IServiceCollection services)
    {
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddValidatorsFromAssemblyContaining<UploadDocumentRequestValidator>();
        return services;
    }
}
