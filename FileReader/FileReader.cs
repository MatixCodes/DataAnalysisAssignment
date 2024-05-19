using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Diagnostics;
using Data.Models;

public class FileReader : IFileReader
{
    public List<DataSet> ChannelDataList { get; private set; } = new List<DataSet>();

    public void Initialize(string filePath)
    {
        ChannelDataList.Clear();
        ReadFileData(filePath);
    }

    public List<DataSet> GetResults()
    {
        return ChannelDataList;
    }

    private void ReadFileData(string filePath)
    {
        try
        {
            List<List<string>> fileData = ParseFileData(filePath);
            int channelColumnIndex = GetColumnIndex(fileData, "channel");
            int timeColumnIndex = GetColumnIndex(fileData, "time");
            int valueColumnIndex = GetColumnIndex(fileData, "value");
            int outingColumnIndex = GetColumnIndex(fileData, "outing");

            ValidateColumnIndices(channelColumnIndex, timeColumnIndex, valueColumnIndex, outingColumnIndex);

            GroupAndProcessData(fileData, channelColumnIndex, timeColumnIndex, valueColumnIndex, outingColumnIndex);
        }
        catch (FileNotFoundException ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    private List<List<string>> ParseFileData(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        var fileData = new List<List<string>>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                fileData.Add(line.Split('\t').ToList());
        }
        return fileData;
    }

    private int GetColumnIndex(List<List<string>> fileData, string columnName)
    {
        for (int columnIndex = 0; columnIndex < fileData[0].Count; columnIndex++)
        {
            if (fileData[0][columnIndex].Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return columnIndex;
        }
        return -1;
    }

    private void ValidateColumnIndices(params int[] columnIndices)
    {
        if (columnIndices.Any(index => index == -1))
            throw new InvalidOperationException("One or more required columns are not found in the file.");
    }

    private void GroupAndProcessData(List<List<string>> fileData, int channelColumnIndex, int timeColumnIndex, int valueColumnIndex, int outingColumnIndex)
    {
        try
        {
            var groupedData = fileData.Skip(1).GroupBy(row => row[channelColumnIndex]);
            foreach (var channelGroup in groupedData)
            {
                var channelData = new DataSet
                {
                    ChannelName = "Channel " + channelGroup.Key,
                    Selected = false,
                    Outing = int.Parse(channelGroup.First()[outingColumnIndex])
                };

                ExtractTimeAndValueArrays(channelGroup, timeColumnIndex, valueColumnIndex, channelData);
                ChannelDataList.Add(channelData);
            }
        }
        catch (FormatException ex)
        {
            Debug.WriteLine($"Data format error: {ex.Message}");
            throw new InvalidOperationException("Invalid data format in the file.", ex);
        }
    }

    private void ExtractTimeAndValueArrays(IEnumerable<List<string>> channelGroup, int timeColumnIndex, int valueColumnIndex, DataSet channelData)
    {
        var timeList = new List<double>();
        var valueList = new List<double>();

        foreach (var row in channelGroup)
        {
            if (double.TryParse(row[timeColumnIndex], out double time))
                timeList.Add(time);
            else
                throw new InvalidOperationException("Invalid time value encountered.");

            if (double.TryParse(row[valueColumnIndex], out double parsedValue))
                valueList.Add(parsedValue);
            else
                valueList.Add(double.NaN);
        }

        channelData.TimeArray = timeList.ToArray();
        channelData.ValueArray = valueList.ToArray();
    }

    public bool InvalidValuesExist()
    {
        return ChannelDataList.Any(channelData => channelData.ValueArray.Any(double.IsNaN));
    }

    public void RemoveMissingValues()
    {
        foreach (var dataset in ChannelDataList)
            RemoveRowsWithMissingValues(dataset);
    }

    private void RemoveRowsWithMissingValues(DataSet dataset)
    {
        var rowsToRemove = new List<int>();

        for (int i = 0; i < dataset.TimeArray.Length; i++)
        {
            if (double.IsNaN(dataset.ValueArray[i]) || double.IsInfinity(dataset.ValueArray[i]))
                rowsToRemove.Add(i);
        }

        for (int i = rowsToRemove.Count - 1; i >= 0; i--)
        {
            int rowIndex = rowsToRemove[i];
            dataset.TimeArray = dataset.TimeArray.Where((val, idx) => idx != rowIndex).ToArray();
            dataset.ValueArray = dataset.ValueArray.Where((val, idx) => idx != rowIndex).ToArray();
        }
    }

    public void PredictMissingValues()
    {
        foreach (var dataset in GetDatasetsWithMissingValues())
            FillMissingValues(dataset);
    }

    private List<DataSet> GetDatasetsWithMissingValues()
    {
        return ChannelDataList.Where(HasMissingValues).ToList();
    }

    private bool HasMissingValues(DataSet channelData)
    {
        return channelData.ValueArray.Any(double.IsNaN);
    }

    private void FillMissingValues(DataSet channelData)
    {
        var timeList = new List<double>();
        var valueList = new List<double>();

        for (int i = 0; i < channelData.TimeArray.Length; i++)
        {
            double currentTime = channelData.TimeArray[i];
            double currentValue = channelData.ValueArray[i];

            if (double.IsNaN(currentValue))
            {
                double? prevValue = GetPreviousValidValue(channelData.ValueArray, i);
                double? nextValue = GetNextValidValue(channelData.ValueArray, i);

                if (prevValue.HasValue && nextValue.HasValue)
                    valueList.Add(LinearInterpolation(currentTime, channelData.TimeArray[i - 1], channelData.TimeArray[i + 1], prevValue.Value, nextValue.Value));
                else if (prevValue.HasValue)
                    valueList.Add(prevValue.Value);
                else if (nextValue.HasValue)
                    valueList.Add(nextValue.Value);
                else
                    continue;
            }
            else
                valueList.Add(currentValue);

            timeList.Add(currentTime);
        }

        channelData.TimeArray = timeList.ToArray();
        channelData.ValueArray = valueList.ToArray();
    }

    private double? GetPreviousValidValue(double[] values, int currentIndex)
    {
        for (int i = currentIndex - 1; i >= 0; i--)
        {
            if (!double.IsNaN(values[i]))
                return values[i];
        }
        return null;
    }

    private double? GetNextValidValue(double[] values, int currentIndex)
    {
        for (int i = currentIndex + 1; i < values.Length; i++)
        {
            if (!double.IsNaN(values[i]))
                return values[i];
        }
        return null;
    }

    private double LinearInterpolation(double x, double x0, double x1, double y0, double y1)
    {
        return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
    }
}
