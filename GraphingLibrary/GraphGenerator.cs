using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using Data.Models;
using System.Diagnostics;

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
}


