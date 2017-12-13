using GreedyKidEditor.Helpers;
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
using System.Windows.Shapes;

namespace GreedyKidEditor
{
    /// <summary>
    /// Interaction logic for WorkshopConfigDialog.xaml
    /// </summary>
    public partial class WorkshopConfigDialog : Window
    {
        private string[] _languageCodes = new string[]
        {
            "arabic",  
            "bulgarian",   
            "schinese",
            "tchinese",
            "czech",
            "danish",
            "dutch",
            "english",
            "finnish",
            "french",
            "german",
            "greek",
            "hungarian",
            "italian",
            "japanese",  
            "koreana",
            "norwegian", 
            "polish",
            "portuguese",
            "brazilian",
            "romanian",
        };

        public WorkshopConfigDialog()
        {
            InitializeComponent();
        }

        public string ItemName
        {
            set { nameTextBox.Text = value; }
            get { return nameTextBox.Text; }
        }

        public string ItemDescription
        {
            set { descriptionTextBox.Text = value; }
            get { return descriptionTextBox.Text; }
        }

        public string ItemPreviewPath
        {
            set { pathLabel.Content = value; }
            get { return pathLabel.Content.ToString(); }
        }

        public string ItemLanguageCode
        {
            set
            {
                for (int i = 0; i < _languageCodes.Length; i++)
                {
                    if (_languageCodes[i] == value)
                    {
                        languageComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            get
            {
                return _languageCodes[languageComboBox.SelectedIndex];
            }
        }

        public WorkshopItemVisibility ItemVisibility
        {
            set { visibilityComboBox.SelectedIndex = (int)value; }
            get { return (WorkshopItemVisibility)visibilityComboBox.SelectedIndex; }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // openfiledialog            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // check that nothing is empty and that preview exists

            DialogResult = true;
        }
    }
}
