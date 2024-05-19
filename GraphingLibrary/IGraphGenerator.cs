using Data.Models;
using ScottPlot.WPF;

using System.Collections.Generic;



public interface IGraphGenerator
{
    void CreateGraph(List<DataSet> initialDatasets,WpfPlot wpfPlot);
    void ToggleDataSetVisibility(DataSet dataset);
    void CreateCustomChannel(DataSet newChannel);
    void HighlightPoints(string channelName, ComparisonOperator comparisonOperator, double thresholdValue);
}

