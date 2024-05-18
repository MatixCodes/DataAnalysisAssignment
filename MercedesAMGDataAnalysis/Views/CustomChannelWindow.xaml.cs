using Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MercedesAMGDataAnalysis.Views
{
    public partial class CustomChannelWindow : Window
    {
        public event EventHandler<CustomChannelEventArgs> CustomChannelCreated;
        public List<string> ChannelDataList { get; private set; }
        private List<DataSet> dataSets;
        private DataSetValidator dsValidator = new DataSetValidator();

        

        public CustomChannelWindow()
        {
            InitializeComponent();
            PopulateAlignmentComboBox();

            FirstChannelComboBox.SelectionChanged += ChannelSelectionComboBoxs_SelectionChanged;
            SecondChannelComboBox.SelectionChanged += ChannelSelectionComboBoxs_SelectionChanged;
        }

        private void PopulateAlignmentComboBox()
        {
            CorrectionTypeComboBox.ItemsSource = EnumHelper.GetEnumDescriptions<DataConversionType>(DataConversionType.None);
            
        }

        public void SetData(List<DataSet> dataSetList)
        {
            dataSets = dataSetList;
            // Set items source for the ComboBoxes
            FirstChannelComboBox.ItemsSource = dataSetList;
            SecondChannelComboBox.ItemsSource = dataSetList;

            // Set display member path to indicate which property should be displayed
            FirstChannelComboBox.DisplayMemberPath = "ChannelName";
            SecondChannelComboBox.DisplayMemberPath = "ChannelName";


        }
        private void CheckDataButton_Click(object sender, RoutedEventArgs e)
        {
            int firstChannelComboBoxIndex = FirstChannelComboBox.SelectedIndex;
            int secondChannelComboBoxIndex = SecondChannelComboBox.SelectedIndex;

            if ((firstChannelComboBoxIndex != -1 && secondChannelComboBoxIndex != -1)&& (firstChannelComboBoxIndex != secondChannelComboBoxIndex))
            {
                bool timesAlign = dsValidator.DoTimesAlign(dataSets[firstChannelComboBoxIndex], dataSets[secondChannelComboBoxIndex]);

                if (!timesAlign)
                {
                    AlignmentLabel.Visibility = Visibility.Visible;
                    AlignmentLabel.Content = "Please select a correction method";
                    CorrectionTypeComboBox.Visibility = Visibility.Visible;
                    AddCustomChannelButton.Visibility = Visibility.Visible;
                    SetCorrectionTypeComboBoxValue(DataConversionType.LinearInterpolation);
                }
                if (timesAlign)
                {
                    AlignmentLabel.Visibility = Visibility.Visible;
                    AlignmentLabel.Content = "Data is ready";
                    CorrectionTypeComboBox.Visibility = Visibility.Collapsed;
                    AddCustomChannelButton.Visibility = Visibility.Visible;
                    SetCorrectionTypeComboBoxValue(DataConversionType.None);
                }
            }
            else
            {
                ErrorLabel.Content = "Select two channels before checking data!";
                return;
            }
        }

        private void CreateCustomChannelButton_Click(object sender, RoutedEventArgs e)
        {
            
            string channelName = ChannelNameTextBox.Text;
            int firstComboBoxIndex = FirstChannelComboBox.SelectedIndex;
            int secondComboBoxIndex = SecondChannelComboBox.SelectedIndex;
            OperationType selectedOperator = GetSelectedOperator();
            DataConversionType selectedConversionType;


            if (channelName.Length != 0)
            {
                if (GetSelectedOperator() != OperationType.None)
                {
                    if (CorrectionTypeComboBox.SelectedIndex == -1 || CorrectionTypeComboBox.SelectedItem == null)
                    {
                        selectedConversionType = DataConversionType.None;
                    }
                    else
                    {
                        string selectedDescription = CorrectionTypeComboBox.SelectedItem.ToString();
                        selectedConversionType = EnumHelper.GetEnumValueFromDescription<DataConversionType>(selectedDescription);
                    }


                    var args = new CustomChannelEventArgs(channelName, firstComboBoxIndex, secondComboBoxIndex,selectedOperator, selectedConversionType);
                    CustomChannelCreated?.Invoke(this, args);
                    this.Close();
                }
                else
                {
                    ErrorLabel.Content = "Operator selection required!";
                }
            }
            else
            {
                ErrorLabel.Content = "Name field required!";
            }
        }

        public void SetCorrectionTypeComboBoxValue(DataConversionType conversionType)
        {
            if (conversionType == DataConversionType.None)
            {
                CorrectionTypeComboBox.SelectedItem = null;
            }
            else
            {
                string description = EnumHelper.GetEnumDescription(conversionType);
                CorrectionTypeComboBox.SelectedItem = description;
            }
        }

        private OperationType GetSelectedOperator()
        {
            if (PlusRadioButton.IsChecked == true)
            {
                return OperationType.Addition;
            }
            else if (MinusRadioButton.IsChecked == true)
            {
                return OperationType.Subtraction;
            }
            else if (MultiplyRadioButton.IsChecked == true)
            {
                return OperationType.Multiplication;
            }
            else if (DivideRadioButton.IsChecked == true)
            {
                return OperationType.Division;
            }
            
            return OperationType.None;
        }

        private void ChannelSelectionComboBoxs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AlignmentLabel.Visibility = Visibility.Collapsed;
            CorrectionTypeComboBox.Visibility = Visibility.Collapsed;
            AddCustomChannelButton.Visibility = Visibility.Collapsed;
        }
    }



    public class CustomChannelEventArgs : EventArgs
    {
        public string ChannelName { get; }
        public int FirstComboBoxSelection { get; }
        public int SecondComboBoxSelection { get; }
        public OperationType SelectedOperator { get; }

        public DataConversionType DataConversionType { get; }

        public CustomChannelEventArgs(string channelName, int firstComboBoxSelection, int secondComboBoxSelection, OperationType selectedOperator,DataConversionType dataConversionType)
        {
            ChannelName = channelName;
            FirstComboBoxSelection = firstComboBoxSelection;
            SecondComboBoxSelection = secondComboBoxSelection;
            SelectedOperator = selectedOperator;
            DataConversionType = dataConversionType;
        }
    }
    

    public static class EnumHelper
    {
        public static List<string> GetEnumDescriptions<T>(params T[] excludeValues)
        {
            var excludedList = excludeValues.Select(e => e.ToString()).ToList();
            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Where(field => !excludedList.Contains(field.Name))
                            .Select(field => field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name)
                            .ToList();
        }
        public static string GetEnumDescription<T>(T value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
        public static T GetEnumValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();

            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
        }
    }
    

}

