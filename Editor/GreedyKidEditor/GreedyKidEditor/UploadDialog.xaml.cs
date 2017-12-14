using System;
using System.Diagnostics;
using System.Windows;

namespace GreedyKidEditor
{
    /// <summary>
    /// Interaction logic for UploadDialog.xaml
    /// </summary>
    public partial class UploadDialog : Window
    {
        public UploadDialog()
        {
            InitializeComponent();

            var uploading = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {                
                Stopwatch sw = new Stopwatch();
                sw.Start();
                long time = 0;
                while (Helpers.SteamworksHelper.Instance.UploadStatus == Helpers.WorkshopUploadStatus.Uploading)
                {
                    // burn some CPU
                    if (time <= sw.ElapsedMilliseconds - 50)
                    {
                        time += 50;
                        Application.Current.Dispatcher.Invoke(new Action(() => { UpdateProgress(time); }));
                    }
                }
                Application.Current.Dispatcher.Invoke(new Action(() => { ProcessResult(); }));                
            },
            System.Threading.CancellationToken.None,
            System.Threading.Tasks.TaskCreationOptions.None,
            System.Threading.Tasks.TaskScheduler.Default);
        }

        public void Reset()
        {
            progressBar.Value = 0.0;
            textBlock.Visibility = Visibility.Hidden;
            textBlock2.Visibility = Visibility.Hidden;
            button.Visibility = Visibility.Hidden;
            label.Content = "Please wait while your building is being uploaded...";
            label2.Content = "";
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (Helpers.SteamworksHelper.Instance.UploadStatus == Helpers.WorkshopUploadStatus.Uploading)
                e.Cancel = true;

            base.OnClosing(e);
        }

        private void UpdateProgress(long totalTime)
        {
            if (totalTime == -1)
                progressBar.Value = 100.0;
            else
            {
                double d = (totalTime / 10000.0) * 100.0;
                progressBar.Value = Math.Min(95.0, Math.Log(d, 10.0) * 50.0);
            }
        }

        private void ProcessResult()
        {
            MainWindow.SaveAsOrSave();

            UpdateProgress(-1);

            button.Visibility = Visibility.Visible;

            Helpers.WorkshopUploadReturn result = Helpers.SteamworksHelper.Instance.UploadResult;
            Helpers.WorkshopUploadStatus status = Helpers.SteamworksHelper.Instance.UploadStatus;

            if (status == Helpers.WorkshopUploadStatus.Error)
            {
                label.Content = "The upload failed...";

                // if error
                switch (result)
                {
                    case Helpers.WorkshopUploadReturn.Banned:
                        label2.Content = "You don't own Boo! Greedy Kid on Steam or you are banned from the Workshop.";
                        break;
                    case Helpers.WorkshopUploadReturn.NotEnoughSpace:
                        label2.Content = "You have reached your building count limit.";
                        break;
                    case Helpers.WorkshopUploadReturn.NotLoggedIn:
                        label2.Content = "You are not logged in Steam.";
                        break;
                    case Helpers.WorkshopUploadReturn.Timeout:
                        label2.Content = "Timeout: retry later.";
                        break;
                    case Helpers.WorkshopUploadReturn.UnknownError:
                        label2.Content = "Unknown error: verify that all information and files are correct and/or retry later.";
                        break;
                }
            }
            else
            {
                label.Content = "Success!";

                if (result == Helpers.WorkshopUploadReturn.NeedLegalAgreement)
                {
                    textBlock.Visibility = Visibility.Visible;
                    hyperlink.NavigateUri = new System.Uri("http://steamcommunity.com/sharedfiles/filedetails/?id=" + Helpers.SteamworksHelper.Instance.UploadID);
                }
                else
                {
                    textBlock2.Visibility = Visibility.Visible;
                    hyperlink2.NavigateUri = new System.Uri("http://steamcommunity.com/sharedfiles/filedetails/?id=" + Helpers.SteamworksHelper.Instance.UploadID);
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://steamcommunity.com/sharedfiles/filedetails/?id=" + Helpers.SteamworksHelper.Instance.UploadID);
            }
            catch { }
        }

        private void hyperlink2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://steamcommunity.com/sharedfiles/filedetails/?id=" + Helpers.SteamworksHelper.Instance.UploadID);
            }
            catch { }
        }
    }
}
