using Data.Models;
using System;
using System.Collections.Generic;
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

namespace MercedesAMGDataAnalysis.Views
{
    public partial class CustomChannelWindow : Window
    {
        public event EventHandler<CustomChannelEventArgs> CustomChannelCreated;

        public CustomChannelWindow()
        {
            InitializeComponent();
            
        }

        public void SetComboBoxItems(List<DataSet> dataSetList)
        {
            // Set items source for the ComboBoxes
            FirstComboBox.ItemsSource = dataSetList;
            SecondComboBox.ItemsSource = dataSetList;

            // Set display member path to indicate which property should be displayed
            FirstComboBox.DisplayMemberPath = "ChannelName";
            SecondComboBox.DisplayMemberPath = "ChannelName";

            
        }


        private void AddCustomChannel_Click(object sender, RoutedEventArgs e)
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

            var args = new CustomChannelEventArgs(channelName, firstComboBoxIndex, secondComboBoxIndex, selectedOperator);
            CustomChannelCreated?.Invoke(this, args);
            this.Close();
        }
    }

    public class CustomChannelEventArgs : EventArgs
    {
        public string ChannelName { get; }
        public int FirstComboBoxSelection { get; }
        public int SecondComboBoxSelection { get; }
        public OperationType SelectedOperator { get; }

        public CustomChannelEventArgs(string channelName, int firstComboBoxSelection, int secondComboBoxSelection, OperationType selectedOperator)
        {
            ChannelName = channelName;
            FirstComboBoxSelection = firstComboBoxSelection;
            SecondComboBoxSelection = secondComboBoxSelection;
            SelectedOperator = selectedOperator;
        }
    }


}

