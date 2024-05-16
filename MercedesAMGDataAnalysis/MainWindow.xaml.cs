using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScottPlot.WPF;
using Data.Models;
using static GraphGenerator;
using ControlzEx.Standard;
using MercedesAMGDataAnalysis.Views;

namespace MercedesAMGDataAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GraphGenerator graphGenerator;
        private List<Data.Models.DataSet> channelDataList;
        public MainWindow()
        {
            InitializeComponent();


            graphGenerator = new GraphGenerator(wpfPlot1);
            channelDataList = new List<DataSet>();
        }

        private void OpenWindow_Click(object sender, RoutedEventArgs e)
        {
            CustomChannelWindow customChannelWindow = new CustomChannelWindow();
            customChannelWindow.SetComboBoxItems(channelDataList);
            customChannelWindow.Owner = this;
            customChannelWindow.CustomChannelCreated += CustomChannelWindow_CustomChannelCreated;
            customChannelWindow.ShowDialog();
        }


        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Show file dialog to select a file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string selectedFilePath = openFileDialog.FileName;

                // Read data from the selected file
                FileReader fileReader = new FileReader();
                channelDataList = fileReader.ReadFileData(selectedFilePath);
                graphGenerator.CreateSampleGraph(channelDataList);

                // Display the list of channels and checkboxes
                listBoxChannels.ItemsSource = channelDataList;
            }
        }
        private void CheckBox_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var selectedChannel = (DataSet)checkBox.DataContext;
                int index = channelDataList.FindIndex(data => data.ChannelName == selectedChannel.ChannelName);

                if (index >= 0)
                {
                    channelDataList[index].Selected = !channelDataList[index].Selected;
                    graphGenerator.UpdateGraph();
                }
            }
        }
        private void AddCustomChannel_click(object sender, RoutedEventArgs e)
        {
            DataSetResult result = DataSetProcessor.InterpolateDataSets(channelDataList[3], channelDataList[4],1);
            DataSetResult result3 = DataSetProcessor.InterpolateDataSets(channelDataList[3], channelDataList[4],2);
            DataSetResult result2 = DataSetProcessor.ResampleDataSets(channelDataList[3], channelDataList[4]);
            
            

            graphGenerator.CreateCustomChannel("IntterpolatedChannel", result.DataSet1, result.DataSet2, OperationType.Subtraction);
            graphGenerator.CreateCustomChannel("SplineIntterpolatedChannel", result3.DataSet1, result3.DataSet2, OperationType.Subtraction);
            graphGenerator.CreateCustomChannel("ResampledChannel", result2.DataSet1, result2.DataSet2, OperationType.Subtraction);
            graphGenerator.UpdateGraph();
        }

        private void CustomChannelWindow_CustomChannelCreated(object sender, CustomChannelEventArgs e)
        {
            string channelName = e.ChannelName;
            int firstComboBoxSelection = e.FirstComboBoxSelection;
            int secondComboBoxSelection = e.SecondComboBoxSelection;
            OperationType selectedOperator = e.SelectedOperator;

            DataSetResult result = DataSetProcessor.InterpolateDataSets(channelDataList[firstComboBoxSelection], channelDataList[secondComboBoxSelection], 1);
            graphGenerator.CreateCustomChannel(channelName, result.DataSet1, result.DataSet2,selectedOperator);


        }
    }
}
