using System;
using System.Collections.Generic;
using ScottPlot.WPF;
using Data.Models;
using Xunit;
using ScottPlot.Plottables;
using System.Net;


namespace GraphGeneratorTests
{
    public class GraphGeneratorTests
    {
        private WpfPlot CreateTestPlot()
        {
            var plot = new WpfPlot();
            return plot;
        }

        private List<DataSet> CreateTestDataSets()
        {
            return new List<DataSet>
        {
            new DataSet
            {
                ChannelName = "A",
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 10, 20, 30 },
                Selected = true
            }
        };
        }

        [WpfFact]
        public void CreateGraph_ThrowsArgumentNullException_ForNullInitialDatasets()
        {
            var graphGenerator = new GraphGenerator();
            var plot = CreateTestPlot();

            Assert.Throws<ArgumentNullException>(() => graphGenerator.CreateGraph(null, plot));
        }

        [WpfFact]
        public void CreateGraph_ThrowsArgumentNullException_ForNullWpfPlot()
        {
            var graphGenerator = new GraphGenerator();
            var datasets = CreateTestDataSets();

            Assert.Throws<ArgumentNullException>(() => graphGenerator.CreateGraph(datasets, null));
        }

        [WpfFact]
        public void CreateGraph_CreatesGraphSuccessfully()
        {
            var graphGenerator = new GraphGenerator();
            var datasets = CreateTestDataSets();
            var plot = CreateTestPlot();

            graphGenerator.CreateGraph(datasets, plot);


            Assert.NotNull(plot);
        }

        [WpfFact]
        public void ToggleDataSetVisibility_ThrowsArgumentNullException_ForNullDataset()
        {
            var graphGenerator = new GraphGenerator();

            Assert.Throws<ArgumentNullException>(() => graphGenerator.ToggleDataSetVisibility(null));
        }

        [WpfFact]
        public void ToggleDataSetVisibility_ThrowsArgumentException_ForNonExistentDataset()
        {
            var graphGenerator = new GraphGenerator();
            var datasets = CreateTestDataSets();
            var plot = CreateTestPlot();

            graphGenerator.CreateGraph(datasets, plot);
            var nonExistentDataset = new DataSet { ChannelName = "B", Selected = true };

            Assert.Throws<ArgumentException>(() => graphGenerator.ToggleDataSetVisibility(nonExistentDataset));
        }

        [WpfFact]
        public void ToggleDataSetVisibility_TogglesVisibilitySuccessfully()
        {
            var graphGenerator = new GraphGenerator();
            var datasets = CreateTestDataSets();
            var plot = CreateTestPlot();

            graphGenerator.CreateGraph(datasets, plot);
            var dataset = datasets[0];

            graphGenerator.ToggleDataSetVisibility(dataset);

            Assert.True(plot.Plot.GetPlottables().OfType<Scatter>().First().IsVisible);
        }

        [WpfFact]
        public void HighlightPoints_ThrowsArgumentException_ForInvalidChannelName()
        {
            var graphGenerator = new GraphGenerator();
            var datasets = CreateTestDataSets();
            var plot = CreateTestPlot();

            graphGenerator.CreateGraph(datasets, plot);

            Assert.Throws<ArgumentException>(() => graphGenerator.HighlightPoints("InvalidChannel", ComparisonOperator.GreaterThan, 15));
        }

        [WpfFact]
        public void HighlightPoints_HighlightsPointsSuccessfully()
        {
            var graphGenerator = new GraphGenerator();
            var datasets = CreateTestDataSets();
            var plot = CreateTestPlot();

            graphGenerator.CreateGraph(datasets, plot);
            graphGenerator.HighlightPoints("A", ComparisonOperator.GreaterThan, 15);

            Assert.NotEmpty(plot.Plot.GetPlottables().OfType<Marker>());
        }
    }

}
