using System;
using System.Windows;

namespace FBDEdit
{
    public partial class PropertiesWindow : Window
    {
        private Item item;
        public PropertiesWindow(Item item)
        {
            this.item = item;
            InitializeComponent();
            InputsTextBox.Text = item.Inputs.ToString();
            OutputsTextBox.Text = item.Outputs.ToString();
            InputNamesBox.SelectedIndex = Convert.ToInt32(item.InputNames);
            OutputNamesBox.SelectedIndex = Convert.ToInt32(item.OutputNames);
            TypeBox.SelectedIndex = (int)item.Type;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            item.Inputs = int.Parse(InputsTextBox.Text);
            item.Outputs = int.Parse(OutputsTextBox.Text);
            item.InputNames = InputNamesBox.SelectedIndex == 1;
            item.OutputNames = OutputNamesBox.SelectedIndex == 1;
            item.Type = (ItemType)TypeBox.SelectedIndex;            // must be last
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).RemoveItem(item);
            Close();
        }
    }
}
