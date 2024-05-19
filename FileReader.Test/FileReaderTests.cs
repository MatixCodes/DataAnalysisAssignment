using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Data.Models;



namespace FileReaderTests
{
    public class FileReaderTests
    {
        [Fact]
        public void Initialize_FileNotFound_ThrowsFileNotFoundException()
        {
            var fileReader = new FileReader();
            Assert.Throws<FileNotFoundException>(() => fileReader.Initialize("nonexistentfile.txt"));
        }

        [Fact]
        public void Initialize_MissingRequiredColumn_ThrowsInvalidOperationException()
        {
            var fileReader = new FileReader();
            string filePath = CreateTempFile("channel\ttime\tmissing_column\touting\nA\t1\t10\t1");
            Assert.Throws<InvalidOperationException>(() => fileReader.Initialize(filePath));
            File.Delete(filePath);
        }

        [Fact]
        public void Initialize_InvalidDataFormat_ThrowsInvalidOperationException()
        {
            var fileReader = new FileReader();
            string filePath = CreateTempFile("channel\ttime\tvalue\touting\nA\tinvalid_time\t10\t1");
            Assert.Throws<InvalidOperationException>(() => fileReader.Initialize(filePath));
            File.Delete(filePath);
        }

        [Fact]
        public void GetResults_ReturnsCorrectDataSets()
        {
            var fileReader = new FileReader();
            string filePath = CreateTempFile("channel\ttime\tvalue\touting\nA\t1\t10\t1\nA\t2\t20\t1\nB\t1\t5\t2\nB\t2\t15\t2");
            fileReader.Initialize(filePath);

            List<DataSet> results = fileReader.GetResults();
            Assert.Equal(2, results.Count);
            Assert.Equal("Channel A", results[0].ChannelName);
            Assert.Equal("Channel B", results[1].ChannelName);

            File.Delete(filePath);
        }

        [Fact]
        public void InvalidValuesExist_DetectsInvalidValues()
        {
            var fileReader = new FileReader();
            string filePath = CreateTempFile("channel\ttime\tvalue\touting\nA\t1\t10\t1\nA\t2\tinvalid_value\t1");
            fileReader.Initialize(filePath);

            bool result = fileReader.InvalidValuesExist();
            Assert.True(result);

            File.Delete(filePath);
        }

        [Fact]
        public void RemoveMissingValues_RemovesRowsWithMissingValues()
        {
            var fileReader = new FileReader();
            string filePath = CreateTempFile("channel\ttime\tvalue\touting\nA\t1\t10\t1\nA\t2\tNaN\t1\nA\t3\t30\t1");
            fileReader.Initialize(filePath);

            // Check initial state
            var initialResults = fileReader.GetResults();
            Assert.Equal(3, initialResults[0].TimeArray.Length);
            Assert.Equal(3, initialResults[0].ValueArray.Length);

            fileReader.RemoveMissingValues();

            var results = fileReader.GetResults();
            Assert.Equal(2, results[0].TimeArray.Length);
            Assert.Equal(2, results[0].ValueArray.Length);
            Assert.Equal(1, results[0].TimeArray[0]);
            Assert.Equal(10, results[0].ValueArray[0]);
            Assert.Equal(3, results[0].TimeArray[1]);
            Assert.Equal(30, results[0].ValueArray[1]);

            File.Delete(filePath);
        }

        [Fact]
        public void PredictMissingValues_FillsMissingValues()
        {
            var fileReader = new FileReader();
            string filePath = CreateTempFile("channel\ttime\tvalue\touting\nA\t1\t10\t1\nA\t2\tNaN\t1\nA\t3\t30\t1");
            fileReader.Initialize(filePath);
            fileReader.PredictMissingValues();

            var results = fileReader.GetResults();
            Assert.Equal(3, results[0].TimeArray.Length);
            Assert.Equal(20, results[0].ValueArray[1]);

            File.Delete(filePath);
        }

        private string CreateTempFile(string content)
        {
            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, content);
            return tempFilePath;
        }
    }

}

