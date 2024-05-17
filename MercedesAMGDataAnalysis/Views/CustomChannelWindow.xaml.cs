using Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
        }

        private void PopulateAlignmentComboBox()
        {
            CorrectionTypeComboBox.ItemsSource = EnumHelper.GetEnumDescriptions<DataConversionType>(DataConversionType.None);
        }

        public void SetData(List<DataSet> dataSetList)
        {
            dataSets = dataSetList;
            // Set items source for the ComboBoxes
            FirstComboBox.ItemsSource = dataSetList;
            SecondComboBox.ItemsSource = dataSetList;

            // Set display member path to indicate which property should be displayed
            FirstComboBox.DisplayMemberPath = "ChannelName";
            SecondComboBox.DisplayMemberPath = "ChannelName";


        }
        private void CheckDataButton_Click(object sender, RoutedEventArgs e)
        {
            int firstComboBoxIndex = FirstComboBox.SelectedIndex;
            int secondComboBoxIndex = SecondComboBox.SelectedIndex;
            bool timesAlign = dsValidator.DoTimesAlign(dataSets[firstComboBoxIndex], dataSets[secondComboBoxIndex]);

            if (!timesAlign)
            {
                AlignmentLabel.Visibility = Visibility.Visible;
                AlignmentLabel.Content = "Data does not align.Please select a correction method";
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

        private void CreateCustomChannelButton_Click(object sender, RoutedEventArgs e)
        {
            string channelName = ChannelNameTextBox.Text;
            int firstComboBoxIndex = FirstComboBox.SelectedIndex;
            int secondComboBoxIndex = SecondComboBox.SelectedIndex;
            OperationType selectedOperator = OperationType.Subtraction; // Default value

            if (PlusRadioButton.IsChecked == true)
            {
                selectedOperator = OperationType.Addition;
            }
            else if (MinusRadioButton.IsChecked == true)
            {
                selectedOperator = OperationType.Subtraction;
            }
            else if (MultiplyRadioButton.IsChecked == true)
            {
                selectedOperator = OperationType.Multiplication;
            }
            else if (DivideRadioButton.IsChecked == true)
            {
                selectedOperator = OperationType.Division;
            }

            DataConversionType selectedConversionType;
            if (CorrectionTypeComboBox.SelectedIndex == -1 || CorrectionTypeComboBox.SelectedItem == null)
            {
                selectedConversionType = DataConversionType.None;
            }
            else
            {
                string selectedDescription = CorrectionTypeComboBox.SelectedItem.ToString();
                selectedConversionType = EnumHelper.GetEnumValueFromDescription<DataConversionType>(selectedDescription);
            }


            var args = new CustomChannelEventArgs(channelName, firstComboBoxIndex, secondComboBoxIndex, selectedOperator,selectedConversionType);
            CustomChannelCreated?.Invoke(this, args);
            this.Close();
            
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

