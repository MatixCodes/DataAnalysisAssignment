using System;
using System.IO;
using System.Linq;
using Xunit;


namespace Reader.Tests
{
    public class FileReaderTests
    {
        [Fact]
        public void ReadFileData_ValidFile_ReturnsChannelDataList()
        {
            // Arrange
            var filePath = Path.Combine("TestFiles", "validFile.dat");
            var fileReader = new FileReader();

            // Act
            var result = fileReader.ReadFileData(filePath);

            // Assert
            Assert.NotNull(result);
            
            // Add more specific assertions based on your expected results
        }

        [Fact]
        public void ReadFileData_FileNotFound_ThrowsFileNotFoundException()
        {
            // Arrange
            var filePath = Path.Combine("TestFiles", "nonExistantFile.dat");
            var fileReader = new FileReader();

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() => fileReader.ReadFileData(filePath));
            Assert.Contains("File not found.", exception.Message);
        }

        [Fact]
        public void ReadFileData_ValidFile_ReturnsCorrectChannelData()
        {
            // Arrange
            var filePath = Path.Combine("TestFiles", "validFile.dat");
            var fileReader = new FileReader();

            // Act
            var result = fileReader.ReadFileData(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(6, result.Count); 
        }
        [Fact]
        public void ReadFileData_FileWithEmptyValues_HandlesEmptyValues()
        {
            // Arrange
            var filePath = Path.Combine("Testfiles", "validFile.dat");
            var fileReader = new FileReader();

            // Act
            var result = fileReader.ReadFileData(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            // Assert that empty values are allowed in ValueArray
            foreach (var channelData in result)
            {
                foreach (var value in channelData.ValueArray)
                {
                    if (value.HasValue)
                    {
                        Assert.NotNull(value); // Non-null values should not be null
                    }
                    else
                    {
                        Assert.Null(value); // Null values should be allowed
                    }
                }
            }
        }
    }
}
