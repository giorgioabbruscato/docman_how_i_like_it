using HrPortal.Documents.Domain;

namespace HrPortal.UnitTests.Documents;

public sealed class DocumentDomainTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var tenantId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var document = Document.Create(
            tenantId,
            employeeId,
            "contract.pdf",
            "application/pdf",
            1024,
            $"{tenantId}/employee/documents/contract.pdf",
            DocumentCategory.Contract);

        document.FileName.Should().Be("contract.pdf");
        document.Category.Should().Be(DocumentCategory.Contract);
        document.SizeBytes.Should().Be(1024);
    }
}
