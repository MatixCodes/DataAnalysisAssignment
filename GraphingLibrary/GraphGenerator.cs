using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using Data.Models;


public class GraphGenerator
{
    private readonly WpfPlot plot;
    private readonly List<DataSet> datasets;
    

    public GraphGenerator(WpfPlot plot)
    {
        this.plot = plot ?? throw new ArgumentNullException(nameof(plot));
        datasets = new List<DataSet>();
    }

    public void CreateSampleGraph(List<DataSet> initialDatasets)
    {
        if (initialDatasets == null)
            throw new ArgumentNullException(nameof(initialDatasets));

        datasets.Clear();
        datasets.AddRange(initialDatasets);
        PlotSelectedDatasets();
        FocusOnData();
        RefreshPlot();
    }

    public void UpdateGraph()
    {
        PlotSelectedDatasets();
        FocusOnData();
        RefreshPlot();
    }

    private void PlotSelectedDatasets()
    {
        plot.Plot.Clear();

        foreach (var dataset in datasets.Where(ds => ds.Selected))
        {
            try
            {
                plot.Plot.Add.Scatter(dataset.TimeArray, dataset.ValueArray)
                            .LegendText = $"Channel {dataset.ChannelName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error plotting dataset: {ex.Message}");
            }
        }
    }

    private void FocusOnData()
    {
        if (!datasets.Any())
            return;

        double minX = datasets.SelectMany(ds => ds.TimeArray).Min();
        double maxX = datasets.SelectMany(ds => ds.TimeArray).Max();
        double minY = datasets.SelectMany(ds => ds.ValueArray).Min() ?? double.NaN;
        double maxY = datasets.SelectMany(ds => ds.ValueArray).Max() ?? double.NaN;

        plot.Plot.Axes.SetLimits(minX, maxX, minY, maxY);
    }

    private void RefreshPlot() => plot.Refresh();


    public enum OperationType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    public void CreateCustomChannel(string channelName, DataSet source1, DataSet source2, OperationType operationType)
    {
        if (source1 == null || source2 == null)
            throw new ArgumentNullException("Source datasets cannot be null");

        // Check if the time arrays of the source datasets have the same length
        if (source1.TimeArray.Length != source2.TimeArray.Length)
            throw new ArgumentException("Source datasets must have the same number of data points");

        // Perform the operation to create the value array for the custom channel
        double?[] customValues = new double?[source1.TimeArray.Length];
        for (int i = 0; i < source1.TimeArray.Length; i++)
        {
            customValues[i] = PerformOperation(source1.ValueArray[i], source2.ValueArray[i], operationType);
        }

        // Create the custom channel dataset
        var customChannel = new DataSet()
        {
            ChannelName = channelName,
            Outing = source1.Outing,
            TimeArray = source2.TimeArray,
            ValueArray = customValues,
            Selected = true
        };

        

        // Add the custom channel to the list of datasets
        datasets.Add(customChannel);
        UpdateGraph();
    }

    private double? PerformOperation(double? value1, double? value2, OperationType operationType)
    {
        switch (operationType)
        {
            case OperationType.Addition:
                return value1 + value2;
            case OperationType.Subtraction:
                return value1 - value2;
            case OperationType.Multiplication:
                return value1 * value2;
            case OperationType.Division:
                return value1 / value2;
            default:
                throw new ArgumentException("Invalid operation type");
        }
    }
}


