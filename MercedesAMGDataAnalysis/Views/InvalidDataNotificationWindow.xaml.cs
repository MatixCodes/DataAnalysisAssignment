using System;
using System.Windows;


namespace MercedesAMGDataAnalysis.Views
{
    public partial class InvalidDataNotificationWindow : Window
    {
        public event EventHandler RemoveValuesClicked;
        public event EventHandler PredictValuesClicked;
        public InvalidDataNotificationWindow()
        {
            InitializeComponent();
        }


        private void RemoveValuesButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveValuesClicked?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        private void PredictValuesButton_Click(object sender, RoutedEventArgs e)
        {
            PredictValuesClicked?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        private void IgnoreValuesButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    
}
