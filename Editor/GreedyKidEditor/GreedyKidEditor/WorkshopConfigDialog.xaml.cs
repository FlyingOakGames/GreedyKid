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

        public bool ItemAlreadyExist
        {
            set
            {
                if (value)
                    label9.Visibility = Visibility.Visible;
                else
                    label9.Visibility = Visibility.Hidden;
            }
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
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image file (*.png;*.jpg;*.gif)|*.png;*.jpg;*.gif";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ItemPreviewPath = openFileDialog.FileName;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            bool ok = true;

            // check that nothing is empty and that preview exists
            if (ItemName.Length == 0 || ItemDescription.Length == 0)
            {
                MessageBox.Show(this, "Name and description can't be empty.", "Missing information");
                ok = false;
            }

            if (!System.IO.File.Exists(ItemPreviewPath))
            {
                MessageBox.Show(this, "Preview image can't be found.", "Missing information");
                ok = false;
            }

            if (ok)
                DialogResult = true;
        }
    }
}
