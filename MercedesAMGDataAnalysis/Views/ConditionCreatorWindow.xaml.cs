using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace MercedesAMGDataAnalysis.Views
{
    public partial class ConditonCreatorWindow : Window
    {
        private List<DataSet> dataSets;
        public event EventHandler<ConditionCreatedEventArgs> ConditionCreated;
        public ConditonCreatorWindow()
        {
            InitializeComponent();
        }

        public void SetData(List<DataSet> dataSetList)
        {
            dataSets = dataSetList;
            // Set items source for the ComboBoxes
            ConditionChannelComboBox.ItemsSource = dataSetList;


            // Set display member path to indicate which property should be displayed
            ConditionChannelComboBox.DisplayMemberPath = "ChannelName";
        }

        private void CreateConditionButton_Click(object sender, RoutedEventArgs e)
        {
            ComparisonOperator selectedOperator = GetSelectedComparisonOperator();
            string text = ThresholdTextBox.Text;

            if (ConditionChannelComboBox.SelectedIndex != -1)
            {

                if (selectedOperator != ComparisonOperator.None)
                {
                    if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
                    {
                        var args = new ConditionCreatedEventArgs(dataSets[ConditionChannelComboBox.SelectedIndex].ChannelName, value, selectedOperator);
                        ConditionCreated?.Invoke(this, args);
                        this.Close();
                    }
                    else
                    {
                        ErrorLabel.Content = "Threshold value missng!";
                        return;
                    }

                }
                else
                {
                    ErrorLabel.Content = "Selct a comparison operator!";
                    return;
                }
            }
            else
            {
                ErrorLabel.Content = "Select a channel!";
                return;
            }
        }

        private void ThresholdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Get the TextBox content and caret position
            TextBox textBox = sender as TextBox;
            string text = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Check if the input is a valid number with at most one decimal point
            if (!(double.TryParse(text, out double number) || (text.Length == 1 && text == "-") || (text.Count(c => c == '.') <= 1 && text.All(c => char.IsDigit(c) || c == '.'))))
            {
                e.Handled = true;
            }
        }
        private ComparisonOperator GetSelectedComparisonOperator()
        {
            if (SmallerThanRadioButton.IsChecked == true)
            {
                return ComparisonOperator.LessThan;
            }
            else if (BiggerThanRadioButtton.IsChecked == true)
            {
                return ComparisonOperator.GreaterThan;
            }
            else if (EqualsRadioButton.IsChecked == true)
            {
                return ComparisonOperator.EqualTo;
            }

            return ComparisonOperator.None;
        }
    }



    public class ConditionCreatedEventArgs : EventArgs
    {
        public string ChannelName { get; }
        public float Threshold { get; }
        public ComparisonOperator SelectedOperator { get; }



        public ConditionCreatedEventArgs(string channelName, float threshold, ComparisonOperator selectedOperator)
        {
            ChannelName = channelName;
            Threshold = threshold;
            SelectedOperator = selectedOperator;
        }
    }
}
