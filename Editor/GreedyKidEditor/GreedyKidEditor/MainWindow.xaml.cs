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
using System.Linq;

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

        private string _saveFile = "";

        private const string _latestSave = "latest";

        private Building _building = new Building("New building");

        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            InitializeComponent();

            mouseThread = new Thread(MouseWorker);
            mouseThread.Start();

            IntPtr handle = monoGameRenderPanel.Handle;
            rendererThread = new Thread(new ThreadStart(() => { renderer = new PreviewRenderer(handle, this); renderer.Run(); }));
            rendererThread.Start();

            // check for last saved file
            if (File.Exists(_latestSave))
            {
                using (FileStream fs = new FileStream(_latestSave, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        _saveFile = reader.ReadString();
                    }
                }
            }

            Load();

            if (!File.Exists("Content\\Textures\\level.png")) // devmode 
            {
                exportMenu.Visibility = Visibility.Collapsed;
                exportSeparator.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Title = this.Title + " - DEV MODE";
            }
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
            if (_stopThread || !IsApplicationActive())
            {
                renderer.MousePosition = new Microsoft.Xna.Framework.Point();                
                return;
            }

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

            // delete stuff
            if (renderer.SelectedLevel >= 0 && renderer.RoomToRemoveFloor >= 0 && renderer.RoomToRemove >= 0 &&
                renderer.SelectedLevel < _building.Levels.Count && renderer.RoomToRemoveFloor < _building.Levels[renderer.SelectedLevel].Floors.Count && renderer.RoomToRemove < _building.Levels[renderer.SelectedLevel].Floors[renderer.RoomToRemoveFloor].Rooms.Count &&
                MessageBox.Show(this, "The room and ALL of its content will be removed permanently, are you sure?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _building.Levels[renderer.SelectedLevel].Floors[renderer.RoomToRemoveFloor].Rooms.RemoveAt(renderer.RoomToRemove);

                // empty floor above last non empty floor?
                for (int f = _building.Levels[renderer.SelectedLevel].Floors.Count - 1; f >= 0; f--)
                {
                    if (_building.Levels[renderer.SelectedLevel].Floors[f].Rooms.Count == 0)
                        _building.Levels[renderer.SelectedLevel].Floors.RemoveAt(f);
                    else
                        break;
                }
            }

            renderer.RoomToRemoveFloor = -1;
            renderer.RoomToRemove = -1;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private static bool IsActive(Window wnd)
        {
            // workaround for minimization bug
            // Managed .IsActive may return wrong value
            if (wnd == null) return false;
            return GetForegroundWindow() == new WindowInteropHelper(wnd).Handle;
        }

        private static bool IsApplicationActive()
        {
            foreach (var wnd in Application.Current.Windows.OfType<Window>())
                if (IsActive(wnd)) return true;
            return false;
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
                if (renderer != null && IsApplicationActive())
                    renderer.MouseWheelDelta = wParam.ToInt32() >> 16;

                handled = true;   
            }
            else if (msg == WM_KEYDOWN)
            {
                if (wParam.ToInt32() == 32) // space
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.SpaceState = true;
                }
                else if (wParam.ToInt32() == 67) // c
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.CState = true;
                }
                else if (wParam.ToInt32() == 86) // v
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.VState = true;
                }
                else if (wParam.ToInt32() == 66) // b
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.BState = true;
                }
                else if (wParam.ToInt32() == 78) // n
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.NState = true;
                }

                handled = true;   
            }
            else if (msg == WM_KEYUP)
            {
                if (wParam.ToInt32() == 32) // space
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.SpaceState = false;
                }
                else if (wParam.ToInt32() == 67) // c
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.CState = false;
                }
                else if (wParam.ToInt32() == 86) // v
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.VState = false;
                }
                else if (wParam.ToInt32() == 66) // b
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.BState = false;
                }
                else if (wParam.ToInt32() == 78) // n
                {
                    if (renderer != null && IsApplicationActive())
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
            if (_saveFile == String.Empty)
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "Building file (*.gdk)|*.gdk";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _saveFile = saveFileDialog.FileName;
                }
                else
                    return;
            }

            Save();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Building file (*.gdk)|*.gdk";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _saveFile = saveFileDialog.FileName;

                Save();
            }
            else
                return;
        }

        private void Save()
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

            using (FileStream fs = new FileStream(_latestSave, FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(_saveFile);
                }
            }

            loadedFile.Text = DateTime.Now.ToString("HH:mm") + ": Saved " + _building.Name + " (path to file: " + _saveFile + ")";

            System.Media.SystemSounds.Beep.Play();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Building file (*.gdk)|*.gdk|All files|*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    _saveFile = openFileDialog.FileName;

                    Load();

                    using (FileStream fs = new FileStream(_latestSave, FileMode.OpenOrCreate))
                    {
                        using (BinaryWriter writer = new BinaryWriter(fs))
                        {
                            writer.Write(_saveFile);
                        }
                    }
                }
            }
        }

        private void Load()
        {
            // load
            _building = new Building("New building");
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

                loadedFile.Text = DateTime.Now.ToString("HH:mm") + ": Loaded " + _building.Name + " (path to file: " + _saveFile + ")";
            }
            else if (_saveFile != String.Empty)
            {
                loadedFile.Text = DateTime.Now.ToString("HH:mm") + ": Failed loading your last building (path to file: " + _saveFile + ")";
            }
            else
            {
                loadedFile.Text = DateTime.Now.ToString("HH:mm") + ": New building";
            }

            buildingLabel.Content = _building.Name;
            RefreshLevelListBox();

            PreviewRenderer.Building = _building;
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            _saveFile = String.Empty;
            _building = new Building("New building");
            buildingLabel.Content = _building.Name;
            RefreshLevelListBox();

            PreviewRenderer.Building = _building;
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
                using (FileStream fs = new FileStream(dialog.SelectedPath + "\\building", FileMode.OpenOrCreate))
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

            loadedFile.Text = DateTime.Now.ToString("HH:mm") + ": Exported " + _building.Name + " (path to file: " + dialog.SelectedPath + ")";

            System.Media.SystemSounds.Beep.Play();

            // verification
            if (_building.Levels.Count == 0)
            {
                MessageBox.Show("Warning: There is no level.");
            }
            // entrance / exit
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
            // door connections
            for (int i = 0; i < _building.Levels.Count; i++)
            {
                Level level = _building.Levels[i];

                bool warn1 = false;
                bool warn2 = false;
                bool warn3 = false;
                bool warn4 = false;

                for (int f = 0; f < level.Floors.Count; f++)
                {
                    Floor floor = level.Floors[f];

                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        for (int ff = 0; ff < room.FloorDoors.Count; ff++)
                        {
                            FloorDoor floorDoor = room.FloorDoors[ff];

                            int connections = 0;
                            bool greyDoor = (floorDoor.Color == 0);

                            for (int f2 = 0; f2 < level.Floors.Count; f2++)
                            {
                                Floor floor2 = level.Floors[f2];

                                for (int r2 = 0; r2 < floor2.Rooms.Count; r2++)
                                {
                                    Room room2 = floor2.Rooms[r2];

                                    for (int ff2 = 0; ff2 < room2.FloorDoors.Count; ff2++)
                                    {
                                        FloorDoor floorDoor2 = room2.FloorDoors[ff2];

                                        if (floorDoor != floorDoor2 && floorDoor.Color > 0 && floorDoor2.Color == floorDoor.Color)
                                        {
                                            connections++;
                                        }
                                        else if (floorDoor != floorDoor2 && floorDoor.Color == 0 && floorDoor2.Color == floorDoor.Color && floorDoor2.X == floorDoor.X)
                                        {
                                            connections++;
                                        }
                                    }
                                }
                            }


                            if (connections == 0 && greyDoor && !warn1)
                            {
                                warn1 = true;
                                MessageBox.Show("Warning: Level " + (i + 1) + " has non-connected grey doors.");
                            }
                            else if (connections == 0 && !warn2)
                            {
                                warn2 = true;
                                MessageBox.Show("Warning: Level " + (i + 1) + " has non-connected doors.");
                            }
                            if (connections > 1 && greyDoor && !warn3)
                            {
                                warn3 = true;
                                MessageBox.Show("Warning: Level " + (i + 1) + " has grey doors with too many connections.");
                            }
                            else if (connections > 1 && !warn4)
                            {
                                warn4 = true;
                                MessageBox.Show("Warning: Level " + (i + 1) + " has doors with too many conenctions.");
                            }
                        }
                    }
                }
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
            if (levelListBox.SelectedIndex > 0 && levelListBox.SelectedIndex < _building.Levels.Count)
            {
                Level level = _building.Levels[levelListBox.SelectedIndex];
                _building.Levels[levelListBox.SelectedIndex] = _building.Levels[levelListBox.SelectedIndex - 1];
                _building.Levels[levelListBox.SelectedIndex - 1] = level;

                int selected = levelListBox.SelectedIndex - 1;
                RefreshLevelListBox();
                levelListBox.SelectedIndex = selected;
            }
        }

        private void dwLevelButton_Click(object sender, RoutedEventArgs e)
        {
            if (levelListBox.SelectedIndex >= 0 && levelListBox.SelectedIndex < _building.Levels.Count - 1)
            {
                Level level = _building.Levels[levelListBox.SelectedIndex];
                _building.Levels[levelListBox.SelectedIndex] = _building.Levels[levelListBox.SelectedIndex + 1];
                _building.Levels[levelListBox.SelectedIndex + 1] = level;

                int selected = levelListBox.SelectedIndex + 1;
                RefreshLevelListBox();
                levelListBox.SelectedIndex = selected;
            }
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

        private void renameBuildingButton_Click(object sender, RoutedEventArgs e)
        {
            TextInputDialog dialog = new TextInputDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                _building.Name = dialog.ResponseText;
                buildingLabel.Content = _building.Name;
            }
        }

        private void levelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (levelListBox.SelectedIndex >= 0 && renderer.SelectedLevel != levelListBox.SelectedIndex)
            {
                renderer.SelectedLevel = levelListBox.SelectedIndex;

                timeBeforeCopSlider.Value = _building.Levels[levelListBox.SelectedIndex].TimeBeforeCop;
                cop1TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Cop1Count.ToString();
                cop2TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Cop2Count.ToString();

                timeBeforeSwatSlider.Value = _building.Levels[levelListBox.SelectedIndex].TimeBeforeSwat;
                swat1TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Swat1Count.ToString();

                renderer.ResetCamera();
            }
            else
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

        private void timeBeforeSwatSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.TimeBeforeSwat = (int)timeBeforeSwatSlider.Value;
                timeBeforeSwatLabel.Content = level.TimeBeforeSwat.ToString();
            }
        }

        private void swat1ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.Swat1Count++;
                swat1TextBox.Text = level.Swat1Count.ToString();
            }
        }

        private void swat1ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[renderer.SelectedLevel];

                level.Swat1Count--;
                level.Swat1Count = Math.Max(level.Swat1Count, 0);
                swat1TextBox.Text = level.Swat1Count.ToString();
            }
        }
    }
}
