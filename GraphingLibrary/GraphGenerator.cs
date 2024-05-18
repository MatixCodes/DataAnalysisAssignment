using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using Data.Models;
using System.Diagnostics;
using ScottPlot.Colormaps;

public class GraphGenerator
{
    private readonly WpfPlot plot;
    private readonly List<DataSet> datasets;
    

    public GraphGenerator(WpfPlot plot)
    {
        this.plot = plot ?? throw new ArgumentNullException(nameof(plot));
        datasets = new List<DataSet>();
    }

    public void CreateGraph(List<DataSet> initialDatasets)
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
        double minY = datasets.SelectMany(ds => ds.ValueArray).Min();
        double maxY = datasets.SelectMany(ds => ds.ValueArray).Max();

        plot.Plot.Axes.SetLimits(minX, maxX, minY, maxY);
    }

    private void RefreshPlot() => plot.Refresh();

    public void CreateCustomChannel(DataSet newChannel)
    {
        datasets.Add(newChannel);
        UpdateGraph();
    }

    public void HighlightPoints(string channelName, ComparisonOperator comparisonOperator, double thresholdValue)
    {
        var dataset = datasets.FirstOrDefault(ds => ds.ChannelName == channelName && ds.Selected);
        if (dataset == null)
            return;

        bool foundFirstPoint = false;
        bool foundInterpolatedPoint = false;

        // Find the first exact point where the condition is met
        for (int i = 0; i < dataset.ValueArray.Length; i++)
        {
            bool conditionMet = false;
            switch (comparisonOperator)
            {
                case ComparisonOperator.LessThan:
                    conditionMet = dataset.ValueArray[i] < thresholdValue;
                    break;
                case ComparisonOperator.GreaterThan:
                    conditionMet = dataset.ValueArray[i] > thresholdValue;
                    break;
                case ComparisonOperator.EqualTo:
                    conditionMet = dataset.ValueArray[i] == thresholdValue;
                    break;
                default:
                    break;
            }
            if (conditionMet && !foundFirstPoint)
            {
                HighlightPoint(dataset.TimeArray[i], dataset.ValueArray[i], $"{comparisonOperator} {thresholdValue} ({channelName}) - Fixed Point");
                foundFirstPoint = true;
            }
        }

        // Find the first interpolated point where the line crosses the threshold value
        for (int i = 0; i < dataset.TimeArray.Length - 1; i++)
        {
            if (!foundInterpolatedPoint &&
                ((dataset.ValueArray[i] < thresholdValue && dataset.ValueArray[i + 1] > thresholdValue) ||
                 (dataset.ValueArray[i] > thresholdValue && dataset.ValueArray[i + 1] < thresholdValue)))
            {
                double interpolatedTime = InterpolateCrossing(dataset.TimeArray[i], dataset.ValueArray[i], dataset.TimeArray[i + 1], dataset.ValueArray[i + 1], thresholdValue);
                HighlightPoint(interpolatedTime, thresholdValue, $"{comparisonOperator} {thresholdValue} ({channelName}) - Interpolated");
                foundInterpolatedPoint = true;
            }
        }
    }

    private double InterpolateCrossing(double x0, double y0, double x1, double y1, double thresholdValue)
    {
        return x0 + (x1 - x0) * (thresholdValue - y0) / (y1 - y0);
    }

    public enum ComparisonOperator
    {
    LessThan,
    GreaterThan,
    EqualTo
    }
    private void AddMarker(double x, double y, string legendText)
    {
        plot.Plot.Add.Marker(x, y).LegendText = legendText;
    }

    private void AddText(double x, double y)
    {
        string formattedTime = x.ToString("F1"); // Format time to one decimal place
        var txt = plot.Plot.Add.Text(formattedTime, x, y);
        txt.LabelFontSize = 16;
        txt.LabelBorderColor = Colors.Black;
        txt.LabelBorderWidth = 1;
        txt.LabelPadding = 2;
        txt.LabelBold = true;
    }

    private void HighlightPoint(double x, double y, string legendText)
    {
        AddMarker(x, y, legendText);
        AddText(x, y);
    }

}


