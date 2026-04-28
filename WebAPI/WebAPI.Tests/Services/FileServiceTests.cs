using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using WebAPI.Services;

namespace WebAPI.Tests.Services
{

    public class FileServiceTests : IDisposable
    {
        private readonly FileStorageService _fileStorageService;
        private readonly string _testDirectory;
        private readonly Mock<IWebHostEnvironment> mockEnv;

        public FileServiceTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);

            mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.WebRootPath).Returns(_testDirectory);
            _fileStorageService = new FileStorageService(mockEnv.Object);
        }

        [Fact]
        public async Task Save_ValidFile()
        {
            // Arrange
            var fileName = "testfile.txt";
            var content = "This is file content";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));

            IFormFile file = fileMock.Object;
            int taskId = 1;
            int companyId = 100;

            //Act 
            var (storedFileName, filePath) = await _fileStorageService.SaveAsync(file, taskId, companyId);

            //Assert
            storedFileName.Should().EndWith(".txt");
            filePath.Should().Contain($"uploads/{taskId}/{companyId}/");

            var absolutePath = _fileStorageService.GetAbsolutePath(filePath);
            File.Exists(absolutePath).Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_WithEmptyFile_ShouldThrowException()
        {
            //Arrange
            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.FileName).Returns("empty.txt");
            fileMock.Setup(f => f.Length).Returns(0);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            IFormFile file = fileMock.Object;
            int taskId = 1;
            int companyId = 100;

            //Act
            var thrown = await Assert.ThrowsAsync<ArgumentException>(() => _fileStorageService.SaveAsync(file, companyId, taskId));

            //Assert
            Assert.NotNull(thrown);
            Assert.Contains("File is empty", thrown.Message);
        }

        [Fact]
        public async Task SaveAsync_WithFileTooLarge_ShouldThrowException()
        {
            //Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("largefile.txt");
            fileMock.Setup(f => f.Length).Returns(25 * 1024 * 1024); // 25 MB
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            IFormFile file = fileMock.Object;
            int taskId = 1;
            int companyId = 100;

            //Act 
            var thrown = await Assert.ThrowsAsync<ArgumentException>(() => _fileStorageService.SaveAsync(file, companyId, taskId));

            //Assert
            Assert.NotNull(thrown);
            Assert.Contains("File exceeds maximum size", thrown.Message);
        }

        [Fact]
        public async Task SaveAsync_WithInvalidContentType_ShouldThrowException()
        {
            //Arrange 
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file.exe");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.ContentType).Returns("application/x-msdownload");

            IFormFile file = fileMock.Object;
            int taskId = 1;
            int companyId = 100;

            //Act
            var thrown = await Assert.ThrowsAsync<ArgumentException>(() => _fileStorageService.SaveAsync(file, companyId, taskId));

            //Assert 
            Assert.NotNull(thrown);
            Assert.Contains("File type 'application/x-msdownload' is not allowed", thrown.Message);
        }

        [Theory]
        [InlineData("image/jpeg")]
        [InlineData("application/pdf")]
        [InlineData("image/png")]
        public async Task SaveAsync_WithAllowedContentTypes_ShouldSucceed(string contentType)
        {
            //Arrange
            var stream = new MemoryStream(new byte[1024]);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));

            IFormFile file = fileMock.Object;
            int taskId = 1;
            int companyId = 100;

            //Act
            var (storedFileName, filePath) = await _fileStorageService.SaveAsync(fileMock.Object, companyId, taskId);

            //Assert
            storedFileName.Should().NotBeNullOrEmpty();
            filePath.Should().NotBeNullOrEmpty();

        }

        [Fact]
        public void Delete_ExistingFile_ShouldDeleteFile()
        {
            //Arrange
            var filePath = Path.Combine(_testDirectory, "text.txt");
            File.WriteAllText(filePath, "Test content");
            var relativeFilePath = "text.txt";

            //Act
            _fileStorageService.Delete(relativeFilePath);

            //Assert
            File.Exists(filePath).Should().BeFalse();
        }

        [Fact]
        public void Delete_NonExistingFile_ShouldNotThrow()
        {
            //Arrange
            var act = () => _fileStorageService.Delete(Path.Combine(_testDirectory, "nonexistent.txt"));
            act.Should().NotThrow();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

    }
}
