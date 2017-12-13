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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using GreedyKidEditor.Helpers;

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

#if !DEBUG
        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Exception exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
            if (exception != null)
            {
                ReportCrash(exception);
            }
            Environment.Exit(0);
        }

        public void ReportCrash(Exception exception)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // write a local dump
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.Combine(SaveDirectory, "editor_crash.log"), true))
            {
                writer.WriteLine("----------------------- Boo! Greedy Kid Editor crash log -----------------------" + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Date: " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Editor version: " + version + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Operating system: " + SystemHelper.Name + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Exception Type: " + exception.GetType().ToString() + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Message: " + exception.Message + Environment.NewLine + "StackTrace: " + Environment.NewLine + exception.StackTrace + Environment.NewLine + Environment.NewLine);
                if (exception.InnerException != null)
                {
                    writer.WriteLine("Inner exception Type: " + exception.InnerException.GetType().ToString() + Environment.NewLine + Environment.NewLine);
                    writer.WriteLine("Inner exception: " + exception.InnerException.Message + Environment.NewLine + "StackTrace: " + Environment.NewLine + exception.InnerException.StackTrace + Environment.NewLine + Environment.NewLine);
                }
                writer.WriteLine("---------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            // send to db
            using (System.Net.WebClient client = new System.Net.WebClient())
            {

                string message = "EDITOR " + exception.Message;
                string stackTrace = exception.GetType().ToString() + " -- " + exception.StackTrace;
                if (exception.InnerException != null)
                {
                    message += " (Inner exception: " + exception.InnerException.Message + ")";
                    stackTrace += " (Inner exception: " + exception.InnerException.GetType().ToString() + " -- " + exception.InnerException.StackTrace + ")";
                }

                string secret = HashHelper.SHA1(message + SystemHelper.Name + version).ToLowerInvariant();

                try
                {
                    System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection()
                    {
                        { "secret", secret },
                        { "message", message },
                        { "stracktrace", stackTrace },
                        { "version", version },
                        { "os", SystemHelper.Name },
                    };
                    byte[] response =
                    client.UploadValues("http://flying-oak.com/greedykidcrash.php", data);

                    string result = System.Text.Encoding.UTF8.GetString(response);
                    Console.WriteLine(result);
                }
                catch (System.Net.WebException)
                {
                    // 404
                }
            }
        }

        private string SaveDirectory = "";
#endif

        public MainWindow()
        {
#if !DEBUG
            // crash reporter
            string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (Environment.OSVersion.Platform == PlatformID.MacOSX || (Environment.OSVersion.Platform == PlatformID.Unix && SystemHelper.IsMac))
                SaveDirectory = Path.Combine(userHome, "Library/Application Support/Flying Oak Games/Boo! Greedy Kid");
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
                SaveDirectory = Path.Combine(userHome, ".greedykid");
            else
                SaveDirectory = Path.Combine(userHome, "AppData\\LocalLow\\Flying Oak Games\\Boo! Greedy Kid");

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
#endif

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

#if !DEVMODE
            exportMenu.Visibility = Visibility.Collapsed;
            exportSeparator.Visibility = Visibility.Collapsed;
#else
            this.Title = this.Title + " - DEV MODE";
#endif

            SteamworksReturn steam = SteamworksHelper.Instance.Init();
            if (steam == SteamworksReturn.RestartingThroughSteam)
            {
                MessageBox.Show(this, "Steam must be running, it will now be started and the editor will restart.", "Steam required", MessageBoxButton.OK);
                this.Close();
            }
#if !DEBUG
            else if (steam == SteamworksReturn.CantInit)
            {
                MessageBox.Show(this, "Steam could not be found. Try starting Steam before starting the game. If the error persists, please visit the Steam forum.", "Error", MessageBoxButton.OK);
                this.Close();
            }
#endif
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

        // convert to DPI scaled coordinates
        private void TransformToPixels(Visual visual,
                                      double unitX,
                                      double unitY,
                                      out double pixelX,
                                      out double pixelY)
        {
            Matrix matrix;
            var source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            pixelX = unitX / matrix.M11;
            pixelY = unitY / matrix.M22;
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
                double scaledX = 0;
                double scaledY = 0;
                TransformToPixels(this, mousePos.X, mousePos.Y, out scaledX, out scaledY);
                //loadedFile.Text = "Base = " + (int)mousePos.X + ";" + (int)mousePos.Y + " Scaled = " + scaledX + ";" + scaledY;
                mousePos.X = scaledX / zoomX;
                mousePos.Y = scaledY / zoomY;
                Microsoft.Xna.Framework.Point p = new Microsoft.Xna.Framework.Point((int)mousePos.X, (int)mousePos.Y);
                if (renderer != null)
                {
                    renderer.MousePosition = p;
                    if (Keyboard.IsKeyDown(Key.Space))
                        renderer.SpaceState = true;
                }               
            }
            catch (InvalidOperationException)
            {

            }

            // delete stuff
            if (PreviewRenderer.SelectedLevel >= 0 && renderer.RoomToRemoveFloor >= 0 && renderer.RoomToRemove >= 0 &&
                PreviewRenderer.SelectedLevel < _building.Levels.Count && renderer.RoomToRemoveFloor < _building.Levels[PreviewRenderer.SelectedLevel].Floors.Count && renderer.RoomToRemove < _building.Levels[PreviewRenderer.SelectedLevel].Floors[renderer.RoomToRemoveFloor].Rooms.Count &&
                MessageBox.Show(this, "The room and ALL of its content will be removed permanently, are you sure?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _building.Levels[PreviewRenderer.SelectedLevel].Floors[renderer.RoomToRemoveFloor].Rooms.RemoveAt(renderer.RoomToRemove);

                // empty floor above last non empty floor?
                for (int f = _building.Levels[PreviewRenderer.SelectedLevel].Floors.Count - 1; f >= 0; f--)
                {
                    if (_building.Levels[PreviewRenderer.SelectedLevel].Floors[f].Rooms.Count == 0)
                        _building.Levels[PreviewRenderer.SelectedLevel].Floors.RemoveAt(f);
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
                try
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.MouseWheelDelta = wParam.ToInt32() >> 16;
                }
                catch (OverflowException)
                {
                    if (renderer != null && IsApplicationActive())
                        renderer.MouseWheelDelta = 0;
                }

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

            if (levelListBox.Items.Count > 0)
                levelListBox.SelectedIndex = 0;
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            _saveFile = String.Empty;
            _building = new Building("New building");
            buildingLabel.Content = _building.Name;
            RefreshLevelListBox();

            PreviewRenderer.Building = _building;

            if (levelListBox.Items.Count > 0)
                levelListBox.SelectedIndex = 0;
        }

        private void MenuItem_Click_2(object sender, ExecutedRoutedEventArgs e)
        {
#if DEVMODE
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(@"D:\Projects\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content"))
                dialog.SelectedPath = @"D:\Projects\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content";
            if (Directory.Exists(@"C:\Projects\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content"))
                dialog.SelectedPath = @"C:\Projects\GreedyKid\GreedyKid_Desktop\GreedyKid_Desktop\Content";
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

            CheckExport();
#endif
        }

        private bool CheckExport()
        {
            bool noError = true;

            // verification
            if (_building.Levels.Count == 0)
            {
                MessageBox.Show("Warning: There is no level.");
                noError = false;
            }
            if (_building.Name.Length == 0)
            {
                MessageBox.Show("Warning: Your level must have a name.");
                noError = false;
            }
            // entrance / exit
            for (int i = 0; i < _building.Levels.Count; i++)
            {
                int start = _building.Levels[i].HasStart();

                if (start == 0)
                {
                    MessageBox.Show("Warning: Level " + (i + 1) + " has no start.");
                    noError = false;
                }

                int exit = _building.Levels[i].HasExit();

                if (exit == 0)
                {
                    MessageBox.Show("Warning: Level " + (i + 1) + " has no exit.");
                    noError = false;
                }
            }

            List<Room> _reachableRooms = new List<Room>();

            // door connections
            for (int i = 0; i < _building.Levels.Count; i++)
            {
                _reachableRooms.Clear();

                Level level = _building.Levels[i];

                bool warn1 = false;
                bool warn2 = false;
                bool warn3 = false;
                bool warn4 = false;

                int retireeCount = 0;

                for (int f = 0; f < level.Floors.Count; f++)
                {
                    Floor floor = level.Floors[f];

                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        if (room.HasStart)
                            _reachableRooms.Add(room);

                        retireeCount += room.Retirees.Count;

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
                                noError = false;
                            }
                            else if (connections == 0 && !warn2)
                            {
                                warn2 = true;
                                MessageBox.Show("Warning: Level " + (i + 1) + " has non-connected doors.");
                                noError = false;
                            }
                            if (connections > 1 && greyDoor && !warn3)
                            {
                                warn3 = true;
                                MessageBox.Show("Warning: Level " + (i + 1) + " has grey doors with too many connections.");
                                noError = false;
                            }
                            else if (connections > 1 && !warn4)
                            {
                                warn4 = true;
                                MessageBox.Show("Warning: Level " + (i + 1) + " has doors with too many conenctions.");
                                noError = false;
                            }
                        }
                    }
                }

                //if (retireeCount == 0)
                //    MessageBox.Show("Warning: Level " + (i + 1) + " has no retiree (exit elevator will instantly open).");

                // check reachability
                if (_reachableRooms.Count == 0)
                    continue;

                bool canExit = false;
                int reachableRetiree = 0;

                int currentRoom = -1;
                while (currentRoom < _reachableRooms.Count - 1)
                {
                    currentRoom++;

                    if (_reachableRooms[currentRoom].HasExit)
                        canExit = true;
                    reachableRetiree += _reachableRooms[currentRoom].Retirees.Count;

                    List<Room> newRooms = GetReachableRooms(_reachableRooms[currentRoom], level);

                    for (int n = 0; n < newRooms.Count; n++)
                    {
                        if (!_reachableRooms.Contains(newRooms[n]))
                            _reachableRooms.Add(newRooms[n]);
                    }
                }

                if (retireeCount != reachableRetiree)
                {
                    MessageBox.Show("Warning: Level " + (i + 1) + " can't be completed because some retirees are not reachable.");
                    noError = false;
                }
                else if (!canExit)
                {
                    MessageBox.Show("Warning: Level " + (i + 1) + " can't be completed because the exit is not reachable.");
                    noError = false;
                }

                if (level.Cop1Count + level.Cop2Count == 0 && level.TimeBeforeCop > 0)
                {
                    MessageBox.Show("Warning: Level " + (i + 1) + " has a cop timer but no cop.");
                    noError = false;
                }
                if (level.Swat1Count == 0 && level.TimeBeforeSwat > 0)
                {
                    MessageBox.Show("Warning: Level " + (i + 1) + " has a SWAT timer but no SWAT cop.");
                    noError = false;
                }
                if (level.RobocopCount == 0 && level.TimeBeforeRobocop > 0)
                {
                    MessageBox.Show("Warning: Level " + (i + 1) + " has a robot timer but no robot.");
                    noError = false;
                }
            }

            return noError;
        }

        private List<Room> GetReachableRooms(Room start, Level level)
        {
            List<Room> rooms = new List<Room>();

            for (int i = 0; i < start.FloorDoors.Count; i++)
            {
                FloorDoor floorDoor = start.FloorDoors[i];

                // for each doors, go through levels to find connected door
                for (int f = 0; f < level.Floors.Count; f++)
                {
                    Floor floor = level.Floors[f];

                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        for (int ff = 0; ff < room.FloorDoors.Count; ff++)
                        {
                            FloorDoor floorDoor2 = room.FloorDoors[ff];
                            
                            bool greyDoor = (floorDoor.Color == 0);
                           
                            if (floorDoor != floorDoor2 && floorDoor.Color > 0 && floorDoor2.Color == floorDoor.Color)
                            {
                                if (!rooms.Contains(room))
                                    rooms.Add(room);
                            }
                            else if (floorDoor != floorDoor2 && floorDoor.Color == 0 && floorDoor2.Color == floorDoor.Color && floorDoor2.X == floorDoor.X)
                            {
                                if (!rooms.Contains(room))
                                    rooms.Add(room);
                            }
                        }
                    }
                }
            }

            return rooms;
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            if (CheckExport())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "..\\Content\\Workshop\\" + _building.Identifier;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // export
                using (FileStream fs = new FileStream(path + "\\building", FileMode.OpenOrCreate))
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
                    using (FileStream fs = new FileStream(path + "\\level_" + l, FileMode.OpenOrCreate))
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

                loadedFile.Text = DateTime.Now.ToString("HH:mm") + ": Exported " + _building.Name + " (path to file: " + path + ")";

                System.Media.SystemSounds.Beep.Play();
            }
            else
            {
                MessageBox.Show("The building hasn't been exported because of the listed warning(s).");
            }
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not available yet, but you can share levels exported as test.");
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
            if (_building.Levels.Count < 99)
            {
                _building.Levels.Add(new Level());

                RefreshLevelListBox();

                levelListBox.SelectedIndex = _building.Levels.Count - 1;
            }
            else
                MessageBox.Show("Warning: You can't make more than 99 levels.");
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
            MessageBox.Show("Names are limited to latin characters.");

            TextInputDialog dialog = new TextInputDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                _building.Name = dialog.ResponseText;
                // check text
                _building.Name = Regex.Replace(_building.Name, @"[^\u0020-\u00FF]+", "?");
                if (_building.Name.Length > 25)
                {
                    _building.Name = _building.Name.Substring(0, 25);
                    MessageBox.Show("Names are limited to 25 characters.");
                }
                buildingLabel.Content = _building.Name;
            }
        }

        private void levelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (levelListBox.SelectedIndex >= 0 && PreviewRenderer.SelectedLevel != levelListBox.SelectedIndex)
            {
                PreviewRenderer.SelectedLevel = levelListBox.SelectedIndex;

                timeBeforeCopSlider.Value = _building.Levels[levelListBox.SelectedIndex].TimeBeforeCop;
                cop1TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Cop1Count.ToString();
                cop2TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Cop2Count.ToString();

                timeBeforeSwatSlider.Value = _building.Levels[levelListBox.SelectedIndex].TimeBeforeSwat;
                swat1TextBox.Text = _building.Levels[levelListBox.SelectedIndex].Swat1Count.ToString();

                timeBeforeRobocopSlider.Value = _building.Levels[levelListBox.SelectedIndex].TimeBeforeRobocop;
                robocopTextBox.Text = _building.Levels[levelListBox.SelectedIndex].RobocopCount.ToString();

                if (renderer != null)
                    renderer.ResetCamera();
            }
            else
                PreviewRenderer.SelectedLevel = levelListBox.SelectedIndex;
        }       

        //************************ SPAWN SEQUENCES ************************\\

        private void timeBeforeCopSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.TimeBeforeCop = (int)timeBeforeCopSlider.Value;
                timeBeforeCopLabel.Content = level.TimeBeforeCop.ToString();
            }
        }

        private void cop1ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.Cop1Count++;
                level.Cop1Count = Math.Min(level.Cop1Count, Cop.MaxNormalCop - level.Cop2Count);
                cop1TextBox.Text = level.Cop1Count.ToString();
            }
        }

        private void cop1ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.Cop1Count--;
                level.Cop1Count = Math.Max(level.Cop1Count, 0);
                cop1TextBox.Text = level.Cop1Count.ToString();
            }
        }

        private void cop2ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.Cop2Count++;
                level.Cop2Count = Math.Min(level.Cop2Count, Cop.MaxNormalCop - level.Cop1Count);
                cop2TextBox.Text = level.Cop2Count.ToString();
            }
        }

        private void cop2ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.Cop2Count--;
                level.Cop2Count = Math.Max(level.Cop2Count, 0);
                cop2TextBox.Text = level.Cop2Count.ToString();
            }
        }

        private void timeBeforeSwatSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.TimeBeforeSwat = (int)timeBeforeSwatSlider.Value;
                timeBeforeSwatLabel.Content = level.TimeBeforeSwat.ToString();
            }
        }

        private void swat1ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.Swat1Count++;
                level.Swat1Count = Math.Min(level.Swat1Count, Cop.MaxSwatCop);
                swat1TextBox.Text = level.Swat1Count.ToString();
            }
        }

        private void swat1ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.Swat1Count--;
                level.Swat1Count = Math.Max(level.Swat1Count, 0);
                swat1TextBox.Text = level.Swat1Count.ToString();
            }
        }

        private void timeBeforeRobocopSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.TimeBeforeRobocop = (int)timeBeforeRobocopSlider.Value;
                timeBeforeRobocopLabel.Content = level.TimeBeforeRobocop.ToString();
            }
        }

        private void robocopButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.RobocopCount++;
                level.RobocopCount = Math.Min(level.RobocopCount, Cop.MaxRobocop);
                robocopTextBox.Text = level.RobocopCount.ToString();
            }
        }

        private void robocopButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewRenderer.SelectedLevel >= 0 && PreviewRenderer.SelectedLevel < _building.Levels.Count)
            {
                Level level = _building.Levels[PreviewRenderer.SelectedLevel];

                level.RobocopCount--;
                level.RobocopCount = Math.Max(level.RobocopCount, 0);
                robocopTextBox.Text = level.RobocopCount.ToString();
            }
        }
    }
}
