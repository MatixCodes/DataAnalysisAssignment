using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using Data.Models;

public class GraphGenerator : IGraphGenerator
{
    private WpfPlot plot;
    private readonly List<DataSet> datasets;
    private readonly Dictionary<string, Scatter> scatterPlots;
    private readonly Dictionary<string, List<IPlottable>> markersAndAnnotations;

    public GraphGenerator()
    {
        plot = new WpfPlot();
        datasets = new List<DataSet>();
        scatterPlots = new Dictionary<string, Scatter>();
        markersAndAnnotations = new Dictionary<string, List<IPlottable>>();
    }

    public void CreateGraph(List<DataSet> initialDatasets, WpfPlot wpfPlot)
    {
        if (initialDatasets == null)
            throw new ArgumentNullException(nameof(initialDatasets));
        
        if (wpfPlot == null)
            throw new ArgumentNullException(nameof(wpfPlot));

        plot = wpfPlot;
        datasets.Clear();
        datasets.AddRange(initialDatasets);
        InitializeScatterPlots();
        UpdateGraph();
    }

    private void InitializeScatterPlots()
    {
        scatterPlots.Clear();
        markersAndAnnotations.Clear();
        plot.Plot.Clear();

        foreach (var dataset in datasets)
        {
            AddScatterPlot(dataset);
            markersAndAnnotations[dataset.ChannelName] = new List<IPlottable>();
        }
    }

    private void AddScatterPlot(DataSet dataset)
    {
        var scatter = plot.Plot.Add.Scatter(dataset.TimeArray, dataset.ValueArray);
        scatter.LegendText = $"Channel {dataset.ChannelName}";
        scatterPlots[dataset.ChannelName] = scatter;
    }

    public void ToggleDataSetVisibility(DataSet dataset)
    {
        if (dataset == null)
            throw new ArgumentNullException(nameof(dataset));

        if (!scatterPlots.ContainsKey(dataset.ChannelName))
            throw new ArgumentException($"Dataset with channel name {dataset.ChannelName} does not exist.");

        scatterPlots[dataset.ChannelName].IsVisible = dataset.Selected;

        if (markersAndAnnotations.ContainsKey(dataset.ChannelName))
        {
            foreach (var marker in markersAndAnnotations[dataset.ChannelName])
            {
                marker.IsVisible = dataset.Selected;
            }
        }
        RefreshPlot();
    }

    public void UpdateGraph()
    {
        foreach (var dataset in datasets)
        {
            ToggleDataSetVisibility(dataset);
        }
        FocusOnData();
        RefreshPlot();
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
        if (newChannel == null)
            throw new ArgumentNullException(nameof(newChannel));

        datasets.Add(newChannel);
        AddScatterPlot(newChannel);
        markersAndAnnotations[newChannel.ChannelName] = new List<IPlottable>();
        UpdateGraph();
    }

    private void HighlightPoint(string channelName, double x, double y, string legendText, bool isVisible)
    {
        if (!markersAndAnnotations.ContainsKey(channelName))
        {
            markersAndAnnotations[channelName] = new List<IPlottable>();
        }

        var marker = plot.Plot.Add.Marker(x, y);
        marker.LegendText = legendText;
        marker.IsVisible = isVisible;
        markersAndAnnotations[channelName].Add(marker);

        var txt = plot.Plot.Add.Text(x.ToString("F1"), x, y);
        txt.IsVisible = isVisible;
        markersAndAnnotations[channelName].Add(txt);
    }

    public void HighlightPoints(string channelName, ComparisonOperator comparisonOperator, double thresholdValue)
    {
        var dataset = datasets.FirstOrDefault(ds => ds.ChannelName == channelName);
        if (dataset == null)
            throw new ArgumentException($"No dataset found with channel name {channelName}");

        bool foundFixedPoint = false;
        bool foundLinePoint = false;
        bool isVisible = dataset.Selected;

        double? fixedPointX = null;
        double? fixedPointY = null;
        int fixedPointIndex = -1;

        for (int i = 0; i < dataset.ValueArray.Length; i++)
        {
            bool conditionMet = comparisonOperator switch
            {
                ComparisonOperator.LessThan => dataset.ValueArray[i] < thresholdValue,
                ComparisonOperator.GreaterThan => dataset.ValueArray[i] > thresholdValue,
                ComparisonOperator.EqualTo => dataset.ValueArray[i] == thresholdValue,
                _ => throw new ArgumentException($"Invalid comparison operator {comparisonOperator}")
            };

            if (conditionMet && !foundFixedPoint)
            {
                fixedPointX = dataset.TimeArray[i];
                fixedPointY = dataset.ValueArray[i];
                fixedPointIndex = i;
                foundFixedPoint = true;
            }
        }

        for (int i = 0; i < dataset.TimeArray.Length - 1; i++)
        {
            if (!foundLinePoint)
            {
                double x0 = dataset.TimeArray[i];
                double y0 = dataset.ValueArray[i];
                double x1 = dataset.TimeArray[i + 1];
                double y1 = dataset.ValueArray[i + 1];

                if ((y0 - thresholdValue) * (y1 - thresholdValue) <= 0)
                {
                    double interpolatedTime = InterpolateCrossing(x0, y0, x1, y1, thresholdValue);

                    if (fixedPointX != null && fixedPointY != null && fixedPointX.Value > interpolatedTime)
                    {
                        HighlightPoint(channelName, fixedPointX.Value, fixedPointY.Value, $"{comparisonOperator} {thresholdValue} (Channel {channelName}) - Fixed Point", isVisible);
                        HighlightPoint(channelName, interpolatedTime, thresholdValue, $"{comparisonOperator} {thresholdValue} (Channel {channelName}) - Interpolated", isVisible);
                        foundLinePoint = true;
                    }
                    else if (fixedPointX == null || fixedPointY == null)
                    {
                        HighlightPoint(channelName, interpolatedTime, thresholdValue, $"{comparisonOperator} {thresholdValue} (Channel {channelName}) - Interpolated", isVisible);
                        foundLinePoint = true;
                    }
                }
            }
        }

        if (fixedPointX != null && fixedPointY != null && foundFixedPoint && !foundLinePoint)
        {
            HighlightPoint(channelName, fixedPointX.Value, fixedPointY.Value, $"{comparisonOperator} {thresholdValue} (Channel {channelName}) - Fixed Point", isVisible);
        }

        RefreshPlot();
    }

    private double InterpolateCrossing(double x0, double y0, double x1, double y1, double thresholdValue)
    {
        return x0 + (x1 - x0) * (thresholdValue - y0) / (y1 - y0);
    }
}
