using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Data.Models;
using MercedesAMGDataAnalysis.Views;


namespace MercedesAMGDataAnalysis
{
    public partial class MainWindow : Window
    {
        private readonly IGraphGenerator graphGenerator;
        private readonly IFileReader fileReader;
        private List<DataSet> channelDataList;
        public MainWindow(IGraphGenerator graphGenerator, IFileReader fileReader)
        {
            InitializeComponent();
            this.graphGenerator = graphGenerator;
            this.fileReader = fileReader;
            channelDataList = new List<DataSet>();
        }

        #region EventHandlers

        private void OpenChannelCreationWindow_Click(object sender, RoutedEventArgs e)
        {
            CustomChannelWindow customChannelWindow = new CustomChannelWindow();
            customChannelWindow.Owner = this;
            customChannelWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            customChannelWindow.SetData(channelDataList);

            customChannelWindow.CustomChannelCreated += CustomChannelWindow_CustomChannelCreated;
            customChannelWindow.ShowDialog();
        }

        private void OpenConditionCreatorWindow_Click(object sender, RoutedEventArgs e)
        {
            ConditonCreatorWindow conditionCreatorWindow = new ConditonCreatorWindow();
            conditionCreatorWindow.Owner = this;
            conditionCreatorWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            conditionCreatorWindow.SetData(channelDataList);
            conditionCreatorWindow.ConditionCreated += ConditionCreatorWindow_ConditionCreated;
            conditionCreatorWindow.ShowDialog();
        }

        private void OpenInvalidDataNotificationWindow()
        {
            InvalidDataNotificationWindow invalidDataNotificationWindow = new InvalidDataNotificationWindow();
            invalidDataNotificationWindow.Owner = this;
            invalidDataNotificationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            invalidDataNotificationWindow.RemoveValuesClicked += NotificationWindow_RemoveValuesClicked;
            invalidDataNotificationWindow.PredictValuesClicked += NotificationWindow_PredictValuesClicked;
            invalidDataNotificationWindow.ShowDialog();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Show file dialog to select a file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string selectedFilePath = openFileDialog.FileName;


               fileReader.Initialize(selectedFilePath);


                if (fileReader.InvalidValuesExist())
                {
                    OpenInvalidDataNotificationWindow();
                }

                channelDataList = fileReader.GetResults();
                graphGenerator.CreateGraph(channelDataList,wpfPlot1);


                listBoxChannels.ItemsSource = channelDataList;
                CreateChannel_Button.Visibility = Visibility.Visible;
                CreateCondition_Button.Visibility = Visibility.Visible;

                GenerateRequiredChannelAndCondition("Channel 7", channelDataList[4], channelDataList[3], OperationType.Subtraction, DataConversionType.LinearInterpolation, ComparisonOperator.LessThan, 0);
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
                    graphGenerator.ToggleDataSetVisibility(selectedChannel);
                }
            }
        }
        private void CustomChannelWindow_CustomChannelCreated(object sender, CustomChannelEventArgs e)
        {
            string channelName = e.ChannelName;
            int firstComboBoxSelection = e.FirstComboBoxSelection;
            int secondComboBoxSelection = e.SecondComboBoxSelection;
            OperationType selectedOperator = e.SelectedOperator;
            DataConversionType dataConversionType = e.DataConversionType;

            DataSet result = DataSetProcessor.ConvertData(channelName, channelDataList[firstComboBoxSelection], channelDataList[secondComboBoxSelection], selectedOperator, dataConversionType);
            channelDataList.Add(result);
            graphGenerator.CreateCustomChannel(result);

            RefreshListBoxChannels();
        }

        private void ConditionCreatorWindow_ConditionCreated(object sender, ConditionCreatedEventArgs e)
        {
            string channelName = e.ChannelName;
            ComparisonOperator selectedOperator = e.SelectedOperator;
            float threshold = e.Threshold;

            graphGenerator.HighlightPoints(channelName, selectedOperator, threshold);
            RefreshListBoxChannels();
        }

        private void NotificationWindow_PredictValuesClicked(object? sender, EventArgs e)
        {
            fileReader.PredictMissingValues();
        }

        private void NotificationWindow_RemoveValuesClicked(object? sender, EventArgs e)
        {
            fileReader.RemoveMissingValues();
        }
        #endregion

        #region Private Methods
        private void RefreshListBoxChannels()
        {
            listBoxChannels.ItemsSource = null;
            listBoxChannels.ItemsSource = channelDataList;
        }

        private void GenerateRequiredChannelAndCondition(string channelName, DataSet ds1, DataSet ds2, OperationType operation, DataConversionType conversionType, ComparisonOperator compOperator, int threshold)
        {
            DataSet result = DataSetProcessor.ConvertData(channelName, ds1, ds2, operation, conversionType);
            channelDataList.Add(result);
            graphGenerator.CreateCustomChannel(result);
            graphGenerator.HighlightPoints(channelName, compOperator, threshold);
            RefreshListBoxChannels();
            graphGenerator.HighlightPoints("Channel 2", ComparisonOperator.LessThan, -0.5);
        }
        #endregion
    }
}
