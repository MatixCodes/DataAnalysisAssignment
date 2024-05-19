using Data.Models;
using ScottPlot.WPF;

namespace GraphGeneration.Test
{
    public class GraphGeneratorBasicTests
    {
        
        [WpfFact]
        public void CreateSampleGraph_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var plot = new WpfPlot();
            var graphGenerator = new GraphGenerator(plot);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => graphGenerator.CreateGraph(null));
        }
      
    } 
}