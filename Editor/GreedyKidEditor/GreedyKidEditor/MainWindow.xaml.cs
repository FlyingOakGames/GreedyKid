using System;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GreedyKidEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PreviewRenderer renderer;
        private Thread rendererThread;

        private Thread mouseThread;        

        private const string _saveFile = "building";

        private Building _building = new Building("New building");

        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            InitializeComponent();

            mouseThread = new Thread(MouseWorker);
            mouseThread.Start();

            IntPtr handle = monoGameRenderPanel.Handle;
            rendererThread = new Thread(new ThreadStart(() => { renderer = new PreviewRenderer(handle, this, _building); renderer.Run(); }));
            rendererThread.Start();

            // load
            if (File.Exists(_saveFile))
            {
                using (FileStream fs = new FileStream(_saveFile, FileMode.Open))
                {
                    using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        using (BinaryReader reader = new BinaryReader(gzipStream))
                        {                            
                            _building.Load(reader);
                        }
                    }
                }
            }

            RefreshLevelListBox();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private void MouseWorker()
        {
            while (true)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { UpdateMouse(); }));               
                Thread.Sleep(33);

                if (_stopThread)
                    break;
            }
            Console.WriteLine("Mouse thread has exited");
        }

        bool _stopThread = false;       

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _stopThread = true;

            renderer.Exit();
            rendererThread.Join();            
        }

        private void UpdateMouse()
        {
            if (renderer == null)
                return;
            if (_stopThread)
                return;

            try
            {
                Point canvasPos = wfHost.PointToScreen(new Point(0, 0));
                Point mousePos = GetCursorPosition();
                mousePos.X = mousePos.X - canvasPos.X;
                mousePos.Y = mousePos.Y - canvasPos.Y;
                // zoom handling
                float zoomX = (float)wfHost.Width / PreviewRenderer.Width;
                float zoomY = (float)wfHost.Height / PreviewRenderer.Height;
                mousePos.X = mousePos.X / zoomX;
                mousePos.Y = mousePos.Y / zoomY;
                Microsoft.Xna.Framework.Point p = new Microsoft.Xna.Framework.Point((int)mousePos.X, (int)mousePos.Y);
                if (renderer != null)
                {
                    renderer.MousePosition = p;
                }

            }
            catch (InvalidOperationException)
            {

            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_MOUSEWHEEL = 0x20A;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // listen for messages that are meant for a hosted Win32 window.
            if (msg == WM_MOUSEWHEEL) // WM_MOUSEWHEEL
            {
                if (renderer != null)
                    renderer.MouseWheelDelta = wParam.ToInt32() >> 16;

                handled = true;   
            }
            else if (msg == WM_KEYDOWN)
            {
                if (wParam.ToInt32() == 32) // space
                {
                    if (renderer != null)
                        renderer.SpaceState = true;
                }
                else if (wParam.ToInt32() == 67) // c
                {
                    if (renderer != null)
                        renderer.CState = true;
                }
                else if (wParam.ToInt32() == 86) // v
                {
                    if (renderer != null)
                        renderer.VState = true;
                }
                else if (wParam.ToInt32() == 66) // b
                {
                    if (renderer != null)
                        renderer.BState = true;
                }
                else if (wParam.ToInt32() == 78) // n
                {
                    if (renderer != null)
                        renderer.NState = true;
                }

                handled = true;   
            }
            else if (msg == WM_KEYUP)
            {
                if (wParam.ToInt32() == 32) // space
                {
                    if (renderer != null)
                        renderer.SpaceState = false;
                }
                else if (wParam.ToInt32() == 67) // c
                {
                    if (renderer != null)
                        renderer.CState = false;
                }
                else if (wParam.ToInt32() == 86) // v
                {
                    if (renderer != null)
                        renderer.VState = false;
                }
                else if (wParam.ToInt32() == 66) // b
                {
                    if (renderer != null)
                        renderer.BState = false;
                }
                else if (wParam.ToInt32() == 78) // n
                {
                    if (renderer != null)
                        renderer.NState = false;
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        private bool _11scale = false;

        private void rendererBackgroundGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int canvasH = (int)e.NewSize.Height;
            int canvasW = (int)e.NewSize.Width;

            AdaptCanvasSize(canvasW, canvasH);
        }

        private void AdaptCanvasSize(int canvasW, int canvasH)
        {
            int margin = 16;

            float ratio = PreviewRenderer.Width / (float)PreviewRenderer.Height;

            float canvasRatio = canvasW / (float)canvasH;

            int max = Math.Min(canvasH, canvasW);

            if (_11scale)
            {
                wfHost.Width = PreviewRenderer.Width;
                wfHost.Height = PreviewRenderer.Height;
            }

            else if (PreviewRenderer.Width < PreviewRenderer.Height)
            {
                // bound to width
                wfHost.Width = Math.Max(2, canvasW - margin * 2);
                wfHost.Height = Math.Max(2, canvasW - margin * 2) / ratio;
                if (wfHost.Height > Math.Max(2, canvasH - margin * 2))
                {
                    wfHost.Width = Math.Max(2, canvasH - margin * 2) * ratio;
                    wfHost.Height = Math.Max(2, canvasH - margin * 2);
                }
            }
            else if (PreviewRenderer.Height < PreviewRenderer.Width)
            {
                // bound to height
                wfHost.Width = Math.Max(2, canvasH - margin * 2) * ratio;
                wfHost.Height = Math.Max(2, canvasH - margin * 2);
                if (wfHost.Width > Math.Max(2, canvasW - margin * 2))
                {
                    wfHost.Width = Math.Max(2, canvasW - margin * 2);
                    wfHost.Height = Math.Max(2, canvasW - margin * 2) / ratio;
                }
            }
            else
            {
                if (canvasH < canvasW)
                {
                    wfHost.Width = Math.Max(2, canvasH - margin * 2);
                    wfHost.Height = Math.Max(2, canvasH - margin * 2);
                }
                else
                {
                    wfHost.Width = Math.Max(2, canvasW - margin * 2);
                    wfHost.Height = Math.Max(2, canvasW - margin * 2);
                }
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Click_1(object sender, ExecutedRoutedEventArgs e)
        {
            using (FileStream fs = new FileStream(_saveFile, FileMode.OpenOrCreate))
            {
                using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress))
                {
                    using (BinaryWriter writer = new BinaryWriter(gzipStream))
                    {
                        _building.Save(writer);
                    }
                }
            }

            System.Media.SystemSounds.Beep.Play();
        }

        private void MenuItem_Click_2(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(@"D:\Projects\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content"))
                dialog.SelectedPath = @"D:\Projects\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content";
            else if (Directory.Exists(@"D:\FlyingOak\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content"))
                dialog.SelectedPath = @"D:\FlyingOak\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            // export
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (FileStream fs = new FileStream(dialog.SelectedPath + "\\" + _saveFile, FileMode.OpenOrCreate))
                {
                    using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress))
                    {
                        using (BinaryWriter writer = new BinaryWriter(gzipStream))
                        {
                            _building.Save(writer, true);
                        }
                    }
                }

                for (int l = 0; l < _building.Levels.Count; l++)
                {
                    using (FileStream fs = new FileStream(dialog.SelectedPath + "\\level_" + l, FileMode.OpenOrCreate))
                    {
                        using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress))
                        {
                            using (BinaryWriter writer = new BinaryWriter(gzipStream))
                            {
                                _building.Levels[l].Save(writer);
                            }
                        }
                    }
                }
            }

            System.Media.SystemSounds.Beep.Play();

            // verification
            if (_building.Levels.Count == 0)
            {
                MessageBox.Show("Warning: There is no level.");
            }
            for (int i = 0; i < _building.Levels.Count; i++)
            {
                int start = _building.Levels[i].HasStart();

                if (start > 1)
                    MessageBox.Show("Warning: Level " + (i + 1) + " has too many starts.");
                else if (start == 0)
                    MessageBox.Show("Warning: Level " + (i + 1) + " has no start.");

                int exit = _building.Levels[i].HasExit();

                if (exit > 1)
                    MessageBox.Show("Warning: Level " + (i + 1) + " has too many exits.");
                else if (exit == 0)
                    MessageBox.Show("Warning: Level " + (i + 1) + " has no exit.");
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            PreviewRenderer.PreviewAnimation = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PreviewRenderer.PreviewAnimation = false;
        }

        private void RefreshLevelListBox()
        {
            levelListBox.Items.Clear();

            // levels
            for (int l = 0; l < _building.Levels.Count; l++)
            {
                if (_building.Levels[l].Name.Length > 0)
                    levelListBox.Items.Add("Level " + (l + 1) + ": " + _building.Levels[l].Name);
                else
                    levelListBox.Items.Add("Level " + (l + 1));
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            _building.Levels.Add(new Level());

            RefreshLevelListBox();

            levelListBox.SelectedIndex = _building.Levels.Count - 1;
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (levelListBox.SelectedIndex >= 0)
            {
                _building.Levels.RemoveAt(levelListBox.SelectedIndex);

                RefreshLevelListBox();
            }
        }

        private void upLevelButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void dwLevelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            if (levelListBox.SelectedIndex >= 0)
            {
                TextInputDialog dialog = new TextInputDialog();
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    string name = dialog.ResponseText;

                    _building.Levels[levelListBox.SelectedIndex].Name = name;

                    if (name.Length > 0)
                        levelListBox.Items[levelListBox.SelectedIndex] = "Level " + (levelListBox.SelectedIndex + 1) + ": " + _building.Levels[levelListBox.SelectedIndex].Name;
                    else
                        levelListBox.Items[levelListBox.SelectedIndex] = "Level " + (levelListBox.SelectedIndex + 1);
                }
            }            
        }

        private void levelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (levelListBox.SelectedIndex >= 0 && renderer.SelectedLevel != levelListBox.SelectedIndex)
            {
                timeBeforeCopSlider.Value = _building.Levels[levelListBox.SelectedIndex].TimeBeforeCop;
                cop1TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Cop1Count.ToString();
                cop2TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Cop2Count.ToString();

                renderer.ResetCamera();
            }

            renderer.SelectedLevel = levelListBox.SelectedIndex;
        }       

        //************************ SPAWN SEQUENCES ************************\\

        private void timeBeforeCopSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.TimeBeforeCop = (int)timeBeforeCopSlider.Value;
                timeBeforeCopLabel.Content = level.TimeBeforeCop.ToString();
            }
        }

        private void cop1ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.Cop1Count++;
                cop1TextBox.Text = level.Cop1Count.ToString();
            }
        }

        private void cop1ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.Cop1Count--;
                level.Cop1Count = Math.Max(level.Cop1Count, 0);
                cop1TextBox.Text = level.Cop1Count.ToString();
            }
        }

        private void cop2ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.Cop2Count++;
                cop2TextBox.Text = level.Cop2Count.ToString();
            }
        }

        private void cop2ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.Cop2Count--;
                level.Cop2Count = Math.Max(level.Cop2Count, 0);
                cop2TextBox.Text = level.Cop2Count.ToString();
            }
        }        
    }
}
