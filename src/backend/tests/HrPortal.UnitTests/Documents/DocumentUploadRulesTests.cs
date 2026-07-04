using HrPortal.Documents.Application;
using HrPortal.SharedKernel.Results;

namespace HrPortal.UnitTests.Documents;

public sealed class DocumentUploadRulesTests
{
    [Theory]
    [InlineData("application/pdf", 1024)]
    [InlineData("image/jpeg", 1024)]
    [InlineData("image/png", DocumentUploadRules.MaxFileSizeBytes)]
    public void Validate_AllowsPermittedFiles(string contentType, long sizeBytes)
    {
        var result = DocumentUploadRules.Validate(contentType, sizeBytes);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/x-msdownload", 1024)]
    [InlineData("text/html", 1024)]
    public void Validate_RejectsDisallowedMimeTypes(string contentType, long sizeBytes)
    {
        var result = DocumentUploadRules.Validate(contentType, sizeBytes);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not allowed");
    }

    [Fact]
    public void Validate_RejectsOversizedFiles()
    {
        var result = DocumentUploadRules.Validate("application/pdf", DocumentUploadRules.MaxFileSizeBytes + 1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("10 MB");
    }

    [Fact]
    public void Validate_RejectsEmptyFiles()
    {
        var result = DocumentUploadRules.Validate("application/pdf", 0);

        result.IsSuccess.Should().BeFalse();
    }
}
