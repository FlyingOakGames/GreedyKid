using System.Windows;
using System.Windows.Input;

namespace GreedyKidEditor
{
    /// <summary>
    /// Interaction logic for TextInputDialog.xaml
    /// </summary>
    public partial class TextInputDialog : Window
    {
        public TextInputDialog()
        {
            InitializeComponent();
            ResponseTextBox.Focus();
        }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CheckValid())
                DialogResult = true;
        }

        private void ResponseTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CheckValid())
            {
                DialogResult = true;
            }
        }

        private bool CheckValid()
        {
            if (ResponseTextBox.Text.Length == 0)
            {
                MessageBox.Show("Name can not be empty.");
                return false;
            }
            return true;
        }
    }
}
