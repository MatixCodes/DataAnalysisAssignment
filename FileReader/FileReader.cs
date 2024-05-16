using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Diagnostics;
using Data.Models;


public class FileReader
{
    public List<DataSet> ReadFileData(string filePath)
    {
        List<DataSet> channelDataList = new List<DataSet>();

        try
        {
            // Read the file data and parse it
            List<List<string>> fileData = ParseFileData(filePath);

            // Determine the column indices of the "channel", "time", "value", and "run number" columns
            int channelColumnIndex = GetColumnIndex(fileData, "channel");
            int timeColumnIndex = GetColumnIndex(fileData, "time");
            int valueColumnIndex = GetColumnIndex(fileData, "value");
            int outingColumnIndex = GetColumnIndex(fileData, "outing"); // Adjust column name as needed

            if (channelColumnIndex == -1 || timeColumnIndex == -1 || valueColumnIndex == -1 || outingColumnIndex == -1)
            {
                throw new InvalidOperationException("One or more required columns are not found in the file.");
            }

            // Group data by unique channels
            var groupedData = fileData.Skip(1) // Skip header row
                                       .GroupBy(row => row[channelColumnIndex]); // Group by channel column index

            // Iterate through each unique channel group
            foreach (var channelGroup in groupedData)
            {
                DataSet channelData = new DataSet();
                channelData.ChannelName = channelGroup.Key;
                channelData.Selected = true;
                // Get run number (assuming it's the same for all rows of a channel)
                channelData.Outing = int.Parse(channelGroup.First()[outingColumnIndex]);

                // Extract time and value arrays for the channel
                List<double> timeList = new List<double>();
                List<double?> valueList = new List<double?>(); // Use List<double?> to allow null values

                foreach (var row in channelGroup)
                {
                    // Parse time
                    timeList.Add(double.Parse(row[timeColumnIndex]));

                    // Parse value or handle NaN and null values
                    double? value = null;
                    if (!string.IsNullOrWhiteSpace(row[valueColumnIndex]))
                    {
                        if (double.TryParse(row[valueColumnIndex], out double parsedValue))
                        {
                            if (!double.IsNaN(parsedValue))
                            {
                                value = parsedValue;
                            }
                        }
                    }
                    valueList.Add(value);
                }

                channelData.TimeArray = timeList.ToArray();
                channelData.ValueArray = valueList.ToArray();

                // Add the channel data object to the list
                channelDataList.Add(channelData);
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.WriteLine($"Error: File not found. {ex.Message}");
            throw; // Rethrow the caught exception
            // You can throw or handle this exception based on your application's requirements
        }
        catch (FormatException ex)
        {
            Debug.WriteLine($"Error: Data format is invalid. {ex.Message}");
            // You can throw or handle this exception based on your application's requirements
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: An unexpected error occurred. {ex.Message}");
            // You can throw or handle this exception based on your application's requirements
        }

        return channelDataList;
    }

    private int GetColumnIndex(List<List<string>> fileData, string columnName)
    {
        // Check each column for the specified header
        for (int columnIndex = 0; columnIndex < fileData[0].Count; columnIndex++)
        {
            if (fileData[0][columnIndex].ToLower() == columnName.ToLower())
            {
                return columnIndex;
            }
        }

        return -1; // Column not found
    }

    private List<List<string>> ParseFileData(string filePath)
    {
        // Implement logic to read and parse the file data into a List<List<string>>
        // For example, read each line of the file, split by tab, and add to the list

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.", filePath);
        }

        List<List<string>> fileData = new List<List<string>>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                List<string> rowData = line.Split('\t').ToList();
                fileData.Add(rowData);
            }
        }

        return fileData;
    }
}
