using System;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;

namespace GreedyKidEditor
{
    public enum BuildingElement
    {
        Building,
        Level,
        Floor,
        Room
    }

    public class BuildingTreeViewItem : TreeViewItem
    {
        public int Level = 0;
        public int Floor = 0;
        public int Room = 0;
        public BuildingElement Type = BuildingElement.Building;

        public BuildingTreeViewItem(BuildingElement type = BuildingElement.Building, int level = 0, int floor = 0, int room = 0)
        {
            Level = level;
            Floor = floor;
            Room = room;
            Type = type;
        }
    }

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

            RefreshTreeView();
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
            //mouseThread.Abort();
            //mouseThread.Join();
            //rendererThread.Abort();
            renderer.Exit();
            rendererThread.Join();            
        }

        private void UpdateMouse()
        {
            if (renderer == null)
                return;
            if (_stopThread)
                return;
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

        private void RefreshTreeView(int selectedLevel = -1, int selectedFloor = -1, int selectedRoom = -1)
        {
            treeView.Items.Clear();

            // root building
            BuildingTreeViewItem buildingItem = new BuildingTreeViewItem();
            buildingItem.Header = "Building: " + _building.Name;
            buildingItem.IsExpanded = true;
            if (selectedLevel == -1)
                buildingItem.IsSelected = true;

            // levels
            for (int l = 0; l < _building.Levels.Count; l++)
            {
                BuildingTreeViewItem levelItem = new BuildingTreeViewItem(BuildingElement.Level, l);
                levelItem.Header = "LEVEL " + (l + 1) + ": " + _building.Levels[l].Name;

                if (l == selectedLevel)
                {
                    levelItem.IsExpanded = true;
                    levelItem.IsSelected = true;
                }

                // floors
                for (int f = 0; f < _building.Levels[l].Floors.Count; f++)
                {
                    BuildingTreeViewItem floorItem = new BuildingTreeViewItem(BuildingElement.Floor, l, f);
                    floorItem.Header = "Floor " + (f + 1) + ": " + _building.Levels[l].Floors[f].Name;

                    if (f == selectedFloor)
                    {
                        floorItem.IsExpanded = true;
                        floorItem.IsSelected = true;
                    }

                    // rooms
                    for (int r = 0; r < _building.Levels[l].Floors[f].Rooms.Count; r++)
                    {
                        BuildingTreeViewItem roomItem = new BuildingTreeViewItem(BuildingElement.Room, l, f, r);
                        roomItem.Header = "Room " + (r + 1) + ": " + _building.Levels[l].Floors[f].Rooms[r].Name;

                        if (r == selectedRoom)
                        {
                            roomItem.IsSelected = true;
                        }

                        floorItem.Items.Add(roomItem);
                    }

                    levelItem.Items.Add(floorItem);
                }

                buildingItem.Items.Add(levelItem);
            }

            treeView.Items.Add(buildingItem);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Building)
                {
                    _building.Levels.Add(new Level());
                    RefreshTreeView();
                }
                else if (selectedItem.Type == BuildingElement.Level)
                {
                    _building.Levels[selectedItem.Level].Floors.Add(new Floor());
                    RefreshTreeView(selectedItem.Level);
                }
                else if (selectedItem.Type == BuildingElement.Floor)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms.Add(new Room());
                    RefreshTreeView(selectedItem.Level, selectedItem.Floor);
                }      
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Level)
                {
                    _building.Levels.RemoveAt(selectedItem.Level);

                    RefreshTreeView(selectedItem.Level - 1);
                }
                else if (selectedItem.Type == BuildingElement.Floor)
                {
                    _building.Levels[selectedItem.Level].Floors.RemoveAt(selectedItem.Floor);

                    RefreshTreeView(selectedItem.Level, selectedItem.Floor - 1);
                }
                else if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms.RemoveAt(selectedItem.Room);

                    RefreshTreeView(selectedItem.Level, selectedItem.Floor, selectedItem.Room - 1);
                }
            }
        }

        private void upLevelButton_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Level)
                {
                    if (selectedItem.Level > 0)
                    {
                        Level level = _building.Levels[selectedItem.Level - 1];
                        _building.Levels[selectedItem.Level - 1] = _building.Levels[selectedItem.Level];
                        _building.Levels[selectedItem.Level] = level;

                        RefreshTreeView(selectedItem.Level - 1);
                    }
                }
                else if (selectedItem.Type == BuildingElement.Floor)
                {
                    if (selectedItem.Floor > 0)
                    {
                        Floor floor = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor - 1];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor - 1] = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor] = floor;

                        RefreshTreeView(selectedItem.Level, selectedItem.Floor - 1);
                    }
                }
                else if (selectedItem.Type == BuildingElement.Room)
                {
                    if (selectedItem.Room > 0)
                    {
                        Room room = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room - 1];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room - 1] = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room] = room;

                        RefreshTreeView(selectedItem.Level, selectedItem.Floor, selectedItem.Room - 1);
                    }                    
                }
            }
        }

        private void dwLevelButton_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Level)
                {
                    if (selectedItem.Level < _building.Levels.Count - 1)
                    {
                        Level level = _building.Levels[selectedItem.Level + 1];
                        _building.Levels[selectedItem.Level + 1] = _building.Levels[selectedItem.Level];
                        _building.Levels[selectedItem.Level] = level;

                        RefreshTreeView(selectedItem.Level + 1);
                    }
                }
                else if (selectedItem.Type == BuildingElement.Floor)
                {
                    if (selectedItem.Floor < _building.Levels[selectedItem.Level].Floors.Count - 1)
                    {
                        Floor floor = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor + 1];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor + 1] = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor] = floor;

                        RefreshTreeView(selectedItem.Level, selectedItem.Floor + 1);
                    }
                }
                else if (selectedItem.Type == BuildingElement.Room)
                {
                    if (selectedItem.Room < _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms.Count - 1)
                    {
                        Room room = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room + 1];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room + 1] = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room];
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room] = room;

                        RefreshTreeView(selectedItem.Level, selectedItem.Floor, selectedItem.Room + 1);
                    }
                }
            }
        }

        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                TextInputDialog dialog = new TextInputDialog();
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    string name = dialog.ResponseText;

                    if (selectedItem.Type == BuildingElement.Building)
                    {
                        _building.Name = name;
                        selectedItem.Header = "Building: " + name;
                    }
                    else if (selectedItem.Type == BuildingElement.Level)
                    {
                        _building.Levels[selectedItem.Level].Name = name;
                        selectedItem.Header = "LEVEL " + (selectedItem.Level + 1) + ": " + name;
                    }
                    else if (selectedItem.Type == BuildingElement.Floor)
                    {
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Name = name;
                        selectedItem.Header = "Floor " + (selectedItem.Floor + 1) + ": " + name;
                    }
                    else if (selectedItem.Type == BuildingElement.Room)
                    {
                        _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].Name = name;
                        selectedItem.Header = "Room " + (selectedItem.Room + 1) + ": " + name;
                    }
                }
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null && renderer != null)
            {
                renderer.SelectedLevel = -1;
                renderer.SelectedFloor = -1;
                renderer.SelectedRoom = -1;

                if (selectedItem.Type != BuildingElement.Building)
                {
                    renderer.SelectedLevel = selectedItem.Level;

                    timeBeforeCopSlider.Value = _building.Levels[selectedItem.Level].TimeBeforeCop;
                    cop1TextBox.Text = _building.Levels[selectedItem.Level].Cop1Count.ToString();
                    cop2TextBox.Text = _building.Levels[selectedItem.Level].Cop2Count.ToString();

                    if (selectedItem.Type == BuildingElement.Room || selectedItem.Type == BuildingElement.Floor)
                    {
                        int floorCount = _building.Levels[selectedItem.Level].Floors.Count;

                        if (floorCount > 4)
                        {
                            if (selectedItem.Floor < 3) // first floors
                                renderer.MoveCamera(0.0f);
                            else if (selectedItem.Floor == floorCount - 1) // last floor
                                renderer.MoveCamera((floorCount - 4) * 40.0f);
                            else
                                renderer.MoveCamera((selectedItem.Floor - 2) * 40.0f);
                        }
                        else
                            renderer.MoveCamera(0.0f);
                    }
                    else
                        renderer.MoveCamera(0.0f);

                    if (selectedItem.Type == BuildingElement.Room)
                    {
                        renderer.SelectedFloor = selectedItem.Floor;
                        renderer.SelectedRoom = selectedItem.Room;

                        Room room = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room];

                        paintTextBox.Text = room.BackgroundColor.ToString();
                        leftMarginTextBox.Text = room.LeftMargin.ToString();
                        rightMarginTextBox.Text = room.RightMargin.ToString();
                        leftDecorationTextBox.Text = room.LeftDecoration.ToString();
                        rightDecorationTextBox.Text = room.RightDecoration.ToString();
                        startCheckBox.IsChecked = (room.HasStart == true);
                        exitCheckBox.IsChecked = (room.HasExit == true);
                        xStart.Value = room.StartX;
                        xExit.Value = room.ExitX;

                        RefreshDetailListBox();
                        RefreshFloorDoorListBox();
                        RefreshRoomDoorListBox();
                        RefreshFurnitureListBox();
                        RefreshRetiredListBox();
                        RefreshNurseListBox();
                        RefreshCopListBox();
                    }
                }
            }
        }

        private void paintButtonUP_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor++;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor = Math.Min(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor, Room.PaintCount - 1);
                    paintTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor.ToString();
                }
            }
        }

        private void paintButtonDown_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor--;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor = Math.Max(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor, 0);
                    paintTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor.ToString();
                }
            }
        }

        private void leftMarginButtonUP_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin++;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin = Math.Min(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin, 30);
                    leftMarginTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin.ToString();
                }
            }
        }

        private void leftMarginButtonDown_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin--;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin = Math.Max(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin, 0);
                    leftMarginTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin.ToString();
                }
            }
        }

        private void rightMarginButtonUP_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin++;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin = Math.Min(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin, 30);
                    rightMarginTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin.ToString();
                }
            }
        }

        private void rightMarginButtonDown_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin--;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin = Math.Max(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin, 0);
                    rightMarginTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin.ToString();
                }
            }
        }

        private void leftDecorationButtonUP_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration++;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration = Math.Min(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration, Room.DecorationCount - 1);
                    leftDecorationTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration.ToString();
                }
            }
        }

        private void leftDecorationButtonDown_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration--;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration = Math.Max(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration, 0);
                    leftDecorationTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration.ToString();
                }
            }
        }

        private void rightDecorationButtonUP_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration++;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration = Math.Min(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration, Room.DecorationCount - 1);
                    rightDecorationTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration.ToString();
                }
            }
        }

        private void rightDecorationButtonDown_Click(object sender, RoutedEventArgs e)
        {
            BuildingTreeViewItem selectedItem = treeView.SelectedItem as BuildingTreeViewItem;

            if (selectedItem != null)
            {
                if (selectedItem.Type == BuildingElement.Room)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration--;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration = Math.Max(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration, 0);
                    rightDecorationTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration.ToString();
                }
            }
        }

        private void exitCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                 renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                 renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];
                room.HasExit = true;
                if (room.ExitX == 0)
                {
                    int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                    room.ExitX = room.LeftMargin * 8 + roomWidth / 2 - 20;
                    xExit.Value = room.ExitX;
                }
            }
        }

        private void exitCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].HasExit = false;
            }
        }

        private void startCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];
                room.HasStart = true;
                if (room.StartX == 0)
                {
                    int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                    room.StartX = room.LeftMargin * 8 + roomWidth / 2 - 20;
                    xStart.Value = room.StartX;
                }
            }
        }

        private void startCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].HasStart = false;
            }
        }

        private void xStart_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].StartX = (int)xStart.Value;
            }
        }

        private void xExit_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].ExitX = (int)xExit.Value;
            }
        }

        //************************ ROOM DETAILS ************************\\

        private void RefreshDetailListBox()
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count && 
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int prevSelection = detailListBox.SelectedIndex;

                detailListBox.Items.Clear();
                for (int i = 0; i < room.Details.Count; i++)
                {
                    detailListBox.Items.Add("Detail " + i);
                }

                if (prevSelection < detailListBox.Items.Count)
                    detailListBox.SelectedIndex = prevSelection;                
                else if (detailListBox.Items.Count > 0)
                    detailListBox.SelectedIndex = 0;
            }
        }

        private void addDetail_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                room.Details.Add(new Detail(room.LeftMargin * 8 + roomWidth / 2 - 16)); // middle of the room

                RefreshDetailListBox();
                detailListBox.SelectedIndex = detailListBox.Items.Count - 1;
            }
        }

        private void removeDetail_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                detailListBox.SelectedIndex >= 0 && detailListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Details.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                
                room.Details.RemoveAt(detailListBox.SelectedIndex);

                RefreshDetailListBox();
                if (detailListBox.SelectedIndex >= 0)
                    detailListBox.SelectedIndex = detailListBox.SelectedIndex - 1;
            }
        }

        private void upDetail_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                detailListBox.SelectedIndex >= 0 && detailListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Details.Count)
            {
                if (detailListBox.SelectedIndex > 0)
                {
                    Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                    Detail detail = room.Details[detailListBox.SelectedIndex];
                    room.Details[detailListBox.SelectedIndex] = room.Details[detailListBox.SelectedIndex - 1];
                    room.Details[detailListBox.SelectedIndex - 1] = detail;

                    int selected = detailListBox.SelectedIndex - 1;

                    RefreshDetailListBox();
                    detailListBox.SelectedIndex = selected;
                }
            }
        }

        private void downDetail_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                detailListBox.SelectedIndex >= 0 && detailListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Details.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                if (detailListBox.SelectedIndex < room.Details.Count - 1)
                {                    
                    Detail detail = room.Details[detailListBox.SelectedIndex];
                    room.Details[detailListBox.SelectedIndex] = room.Details[detailListBox.SelectedIndex + 1];
                    room.Details[detailListBox.SelectedIndex + 1] = detail;

                    int selected = detailListBox.SelectedIndex + 1;

                    RefreshDetailListBox();
                    detailListBox.SelectedIndex = selected;
                }
            }
        }

        private void xDetail_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                detailListBox.SelectedIndex >= 0 && detailListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Details.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Details[detailListBox.SelectedIndex].X = (int)xDetail.Value;
            }
        }

        private void detailListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                detailListBox.SelectedIndex >= 0 && detailListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Details.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                xDetail.Value = room.Details[detailListBox.SelectedIndex].X;
                detailTextBox.Text = room.Details[detailListBox.SelectedIndex].Type.ToString();
            }
        }

        private void detailButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                detailListBox.SelectedIndex >= 0 && detailListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Details.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Details[detailListBox.SelectedIndex].Type++;
                room.Details[detailListBox.SelectedIndex].Type = Math.Min(room.Details[detailListBox.SelectedIndex].Type, Detail.NormalDetailCount + Detail.AnimatedDetailCount - 1);
                detailTextBox.Text = room.Details[detailListBox.SelectedIndex].Type.ToString();
            }
        }

        private void detailButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                detailListBox.SelectedIndex >= 0 && detailListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Details.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Details[detailListBox.SelectedIndex].Type--;
                room.Details[detailListBox.SelectedIndex].Type = Math.Max(room.Details[detailListBox.SelectedIndex].Type, 0);
                detailTextBox.Text = room.Details[detailListBox.SelectedIndex].Type.ToString();
            }
        }

        //************************ FLOOR DOORS ************************\\

        private void RefreshFloorDoorListBox()
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int prevSelection = floorDoorListBox.SelectedIndex;

                floorDoorListBox.Items.Clear();
                for (int i = 0; i < room.FloorDoors.Count; i++)
                {
                    floorDoorListBox.Items.Add("Floor door " + i);
                }

                if (prevSelection < floorDoorListBox.Items.Count)
                    floorDoorListBox.SelectedIndex = prevSelection;
                else if (floorDoorListBox.Items.Count > 0)
                    floorDoorListBox.SelectedIndex = 0;
            }
        }

        private void addFloorDoor_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                room.FloorDoors.Add(new FloorDoor(room.LeftMargin * 8 + roomWidth / 2 - 20)); // middle of the room

                RefreshFloorDoorListBox();
                floorDoorListBox.SelectedIndex = floorDoorListBox.Items.Count - 1;
            }
        }

        private void removeFloorDoor_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                floorDoorListBox.SelectedIndex >= 0 && floorDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].FloorDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];


                room.FloorDoors.RemoveAt(floorDoorListBox.SelectedIndex);

                RefreshFloorDoorListBox();
                if (floorDoorListBox.SelectedIndex >= 0)
                    floorDoorListBox.SelectedIndex = floorDoorListBox.SelectedIndex - 1;
            }
        }

        private void floorDoorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                floorDoorListBox.SelectedIndex >= 0 && floorDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].FloorDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                xFloorDoor.Value = room.FloorDoors[floorDoorListBox.SelectedIndex].X;
                floorDoorTextBox.Text = room.FloorDoors[floorDoorListBox.SelectedIndex].Color.ToString();
            }
        }

        private void xFloorDoor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                floorDoorListBox.SelectedIndex >= 0 && floorDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].FloorDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.FloorDoors[floorDoorListBox.SelectedIndex].X = (int)xFloorDoor.Value;
            }
        }

        private void floorDoorButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                floorDoorListBox.SelectedIndex >= 0 && floorDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].FloorDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.FloorDoors[floorDoorListBox.SelectedIndex].Color++;
                room.FloorDoors[floorDoorListBox.SelectedIndex].Color = Math.Min(room.FloorDoors[floorDoorListBox.SelectedIndex].Color, FloorDoor.DoorCount - 1);
                floorDoorTextBox.Text = room.FloorDoors[floorDoorListBox.SelectedIndex].Color.ToString();
            }
        }

        private void floorDoorButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                floorDoorListBox.SelectedIndex >= 0 && floorDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].FloorDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.FloorDoors[floorDoorListBox.SelectedIndex].Color--;
                room.FloorDoors[floorDoorListBox.SelectedIndex].Color = Math.Max(room.FloorDoors[floorDoorListBox.SelectedIndex].Color, 0);
                floorDoorTextBox.Text = room.FloorDoors[floorDoorListBox.SelectedIndex].Color.ToString();
            }
        }

        //************************ ROOM DOORS ************************\\

        private void RefreshRoomDoorListBox()
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int prevSelection = roomDoorListBox.SelectedIndex;

                roomDoorListBox.Items.Clear();
                for (int i = 0; i < room.RoomDoors.Count; i++)
                {
                    roomDoorListBox.Items.Add("Room door " + i);
                }

                if (prevSelection < roomDoorListBox.Items.Count)
                    roomDoorListBox.SelectedIndex = prevSelection;
                else if (roomDoorListBox.Items.Count > 0)
                    roomDoorListBox.SelectedIndex = 0;
            }
        }

        private void addRoomDoor_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                room.RoomDoors.Add(new RoomDoor(room.LeftMargin * 8 + roomWidth / 2 - 16)); // middle of the room

                RefreshRoomDoorListBox();
                roomDoorListBox.SelectedIndex = roomDoorListBox.Items.Count - 1;
            }
        }

        private void removeRoomDoor_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                roomDoorListBox.SelectedIndex >= 0 && roomDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].RoomDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];


                room.RoomDoors.RemoveAt(roomDoorListBox.SelectedIndex);

                RefreshRoomDoorListBox();
                if (roomDoorListBox.SelectedIndex >= 0)
                    roomDoorListBox.SelectedIndex = roomDoorListBox.SelectedIndex - 1;
            }
        }

        private void roomDoorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                roomDoorListBox.SelectedIndex >= 0 && roomDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].RoomDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                xRoomDoor.Value = room.RoomDoors[roomDoorListBox.SelectedIndex].X;
            }
        }

        private void xRoomDoor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                roomDoorListBox.SelectedIndex >= 0 && roomDoorListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].RoomDoors.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.RoomDoors[roomDoorListBox.SelectedIndex].X = (int)xRoomDoor.Value;
            }
        }

        //************************ FURNITURE ************************\\

        private void RefreshFurnitureListBox()
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int prevSelection = furnitureListBox.SelectedIndex;

                furnitureListBox.Items.Clear();
                for (int i = 0; i < room.Furnitures.Count; i++)
                {
                    furnitureListBox.Items.Add("Furniture " + i);
                }

                if (prevSelection < furnitureListBox.Items.Count)
                    furnitureListBox.SelectedIndex = prevSelection;
                else if (furnitureListBox.Items.Count > 0)
                    furnitureListBox.SelectedIndex = 0;
            }
        }

        private void addFurniture_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                room.Furnitures.Add(new Furniture(room.LeftMargin * 8 + roomWidth / 2 - 16)); // middle of the room

                RefreshFurnitureListBox();
                furnitureListBox.SelectedIndex = furnitureListBox.Items.Count - 1;
            }
        }

        private void removeFurniture_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                furnitureListBox.SelectedIndex >= 0 && furnitureListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Furnitures.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];


                room.Furnitures.RemoveAt(furnitureListBox.SelectedIndex);

                RefreshFurnitureListBox();
                if (furnitureListBox.SelectedIndex >= 0)
                    furnitureListBox.SelectedIndex = furnitureListBox.SelectedIndex - 1;
            }
        }

        private void upFurniture_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                furnitureListBox.SelectedIndex >= 0 && furnitureListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Furnitures.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                if (furnitureListBox.SelectedIndex > 0)
                {
                    Furniture furniture = room.Furnitures[furnitureListBox.SelectedIndex];
                    room.Furnitures[furnitureListBox.SelectedIndex] = room.Furnitures[furnitureListBox.SelectedIndex - 1];
                    room.Furnitures[furnitureListBox.SelectedIndex - 1] = furniture;

                    int selected = furnitureListBox.SelectedIndex - 1;

                    RefreshFurnitureListBox();

                    furnitureListBox.SelectedIndex = selected;
                }
            }
        }

        private void downFurniture_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                furnitureListBox.SelectedIndex >= 0 && furnitureListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Furnitures.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                if (furnitureListBox.SelectedIndex < room.Furnitures.Count - 1)
                {
                    Furniture furniture = room.Furnitures[furnitureListBox.SelectedIndex];
                    room.Furnitures[furnitureListBox.SelectedIndex] = room.Furnitures[furnitureListBox.SelectedIndex + 1];
                    room.Furnitures[furnitureListBox.SelectedIndex + 1] = furniture;

                    int selected = furnitureListBox.SelectedIndex + 1;

                    RefreshFurnitureListBox();

                    furnitureListBox.SelectedIndex = selected;
                }
            }
        }

        private void furnitureListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                furnitureListBox.SelectedIndex >= 0 && furnitureListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Furnitures.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                xFurniture.Value = room.Furnitures[furnitureListBox.SelectedIndex].X;
                furnitureTextBox.Text = room.Furnitures[furnitureListBox.SelectedIndex].Type.ToString();
            }
        }

        private void xFurniture_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                furnitureListBox.SelectedIndex >= 0 && furnitureListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Furnitures.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Furnitures[furnitureListBox.SelectedIndex].X = (int)xFurniture.Value;
            }
        }

        private void furnitureButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                furnitureListBox.SelectedIndex >= 0 && furnitureListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Furnitures.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Furnitures[furnitureListBox.SelectedIndex].Type++;
                room.Furnitures[furnitureListBox.SelectedIndex].Type = Math.Min(room.Furnitures[furnitureListBox.SelectedIndex].Type, Furniture.FurnitureCount - 1);
                furnitureTextBox.Text = room.Furnitures[furnitureListBox.SelectedIndex].Type.ToString();
            }
        }

        private void furnitureButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                furnitureListBox.SelectedIndex >= 0 && furnitureListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Furnitures.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Furnitures[furnitureListBox.SelectedIndex].Type--;
                room.Furnitures[furnitureListBox.SelectedIndex].Type = Math.Max(room.Furnitures[furnitureListBox.SelectedIndex].Type, 0);
                furnitureTextBox.Text = room.Furnitures[furnitureListBox.SelectedIndex].Type.ToString();
            }
        }

        //************************ RETIREDS ************************\\

        private void RefreshRetiredListBox()
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int prevSelection = retiredListBox.SelectedIndex;

                retiredListBox.Items.Clear();
                for (int i = 0; i < room.Retireds.Count; i++)
                {
                    retiredListBox.Items.Add("Floor Retired " + i);
                }

                if (prevSelection < retiredListBox.Items.Count)
                    retiredListBox.SelectedIndex = prevSelection;
                else if (retiredListBox.Items.Count > 0)
                    retiredListBox.SelectedIndex = 0;
            }
        }

        private void addRetired_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                room.Retireds.Add(new Retired(room.LeftMargin * 8 + roomWidth / 2 - 16)); // middle of the room

                RefreshRetiredListBox();
                retiredListBox.SelectedIndex = retiredListBox.Items.Count - 1;
            }
        }

        private void removeRetired_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];


                room.Retireds.RemoveAt(retiredListBox.SelectedIndex);

                RefreshRetiredListBox();
                if (retiredListBox.SelectedIndex >= 0)
                    retiredListBox.SelectedIndex = retiredListBox.SelectedIndex - 1;
            }
        }

        private void retiredListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                xRetired.Value = room.Retireds[retiredListBox.SelectedIndex].X;
                retiredTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Type.ToString();
                retiredLifeTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Life.ToString();
                retiredMoneyTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Money.ToString();
            }
        }

        private void xRetired_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Retireds[retiredListBox.SelectedIndex].X = (int)xRetired.Value;
            }
        }

        private void retiredButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Retireds[retiredListBox.SelectedIndex].Type++;
                room.Retireds[retiredListBox.SelectedIndex].Type = Math.Min(room.Retireds[retiredListBox.SelectedIndex].Type, Retired.RetiredCount - 1);
                retiredTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Type.ToString();
            }
        }

        private void retiredButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Retireds[retiredListBox.SelectedIndex].Type--;
                room.Retireds[retiredListBox.SelectedIndex].Type = Math.Max(room.Retireds[retiredListBox.SelectedIndex].Type, 0);
                retiredTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Type.ToString();
            }
        }

        private void retiredMoneyButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Retireds[retiredListBox.SelectedIndex].Money++;
                room.Retireds[retiredListBox.SelectedIndex].Money = Math.Min(room.Retireds[retiredListBox.SelectedIndex].Money, 10);
                retiredMoneyTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Money.ToString();
            }
        }

        private void retiredMoneyButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Retireds[retiredListBox.SelectedIndex].Money--;
                room.Retireds[retiredListBox.SelectedIndex].Money = Math.Max(room.Retireds[retiredListBox.SelectedIndex].Money, 0);
                retiredMoneyTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Money.ToString();
            }
        }

        private void retiredLifeButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Retireds[retiredListBox.SelectedIndex].Life++;
                room.Retireds[retiredListBox.SelectedIndex].Life = Math.Min(room.Retireds[retiredListBox.SelectedIndex].Life, 3);
                retiredLifeTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Life.ToString();
            }
        }

        private void retiredLifeButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                retiredListBox.SelectedIndex >= 0 && retiredListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Retireds.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Retireds[retiredListBox.SelectedIndex].Life--;
                room.Retireds[retiredListBox.SelectedIndex].Life = Math.Max(room.Retireds[retiredListBox.SelectedIndex].Life, 1);
                retiredLifeTextBox.Text = room.Retireds[retiredListBox.SelectedIndex].Life.ToString();
            }
        }

        //************************ NURSES ************************\\

        private void RefreshNurseListBox()
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int prevSelection = nurseListBox.SelectedIndex;

                nurseListBox.Items.Clear();
                for (int i = 0; i < room.Nurses.Count; i++)
                {
                    nurseListBox.Items.Add("Floor Nurse " + i);
                }

                if (prevSelection < nurseListBox.Items.Count)
                    nurseListBox.SelectedIndex = prevSelection;
                else if (nurseListBox.Items.Count > 0)
                    nurseListBox.SelectedIndex = 0;
            }
        }

        private void addNurse_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                room.Nurses.Add(new Nurse(room.LeftMargin * 8 + roomWidth / 2 - 16)); // middle of the room

                RefreshNurseListBox();
                nurseListBox.SelectedIndex = nurseListBox.Items.Count - 1;
            }
        }

        private void removeNurse_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                nurseListBox.SelectedIndex >= 0 && nurseListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Nurses.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];


                room.Nurses.RemoveAt(nurseListBox.SelectedIndex);

                RefreshNurseListBox();
                if (nurseListBox.SelectedIndex >= 0)
                    nurseListBox.SelectedIndex = nurseListBox.SelectedIndex - 1;
            }
        }

        private void nurseListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                nurseListBox.SelectedIndex >= 0 && nurseListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Nurses.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                xNurse.Value = room.Nurses[nurseListBox.SelectedIndex].X;
                nurseTextBox.Text = room.Nurses[nurseListBox.SelectedIndex].Type.ToString();
                nurseLifeTextBox.Text = room.Nurses[nurseListBox.SelectedIndex].Life.ToString();
            }
        }

        private void xNurse_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                nurseListBox.SelectedIndex >= 0 && nurseListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Nurses.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Nurses[nurseListBox.SelectedIndex].X = (int)xNurse.Value;
            }
        }

        private void nurseButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                nurseListBox.SelectedIndex >= 0 && nurseListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Nurses.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Nurses[nurseListBox.SelectedIndex].Type++;
                room.Nurses[nurseListBox.SelectedIndex].Type = Math.Min(room.Nurses[nurseListBox.SelectedIndex].Type, Nurse.NurseCount - 1);
                nurseTextBox.Text = room.Nurses[nurseListBox.SelectedIndex].Type.ToString();
            }
        }

        private void nurseButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                nurseListBox.SelectedIndex >= 0 && nurseListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Nurses.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Nurses[nurseListBox.SelectedIndex].Type--;
                room.Nurses[nurseListBox.SelectedIndex].Type = Math.Max(room.Nurses[nurseListBox.SelectedIndex].Type, 0);
                nurseTextBox.Text = room.Nurses[nurseListBox.SelectedIndex].Type.ToString();
            }
        }

        private void nurseLifeButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                nurseListBox.SelectedIndex >= 0 && nurseListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Nurses.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Nurses[nurseListBox.SelectedIndex].Life++;
                room.Nurses[nurseListBox.SelectedIndex].Life = Math.Min(room.Nurses[nurseListBox.SelectedIndex].Life, 3);
                nurseLifeTextBox.Text = room.Nurses[nurseListBox.SelectedIndex].Life.ToString();
            }
        }

        private void nurseLifeButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                nurseListBox.SelectedIndex >= 0 && nurseListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Nurses.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Nurses[nurseListBox.SelectedIndex].Life--;
                room.Nurses[nurseListBox.SelectedIndex].Life = Math.Max(room.Nurses[nurseListBox.SelectedIndex].Life, 1);
                nurseLifeTextBox.Text = room.Nurses[nurseListBox.SelectedIndex].Life.ToString();
            }
        }

        //************************ COPS ************************\\

        private void RefreshCopListBox()
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int prevSelection = copListBox.SelectedIndex;

                copListBox.Items.Clear();
                for (int i = 0; i < room.Cops.Count; i++)
                {
                    copListBox.Items.Add("Floor Cop " + i);
                }

                if (prevSelection < copListBox.Items.Count)
                    copListBox.SelectedIndex = prevSelection;
                else if (copListBox.Items.Count > 0)
                    copListBox.SelectedIndex = 0;
            }
        }

        private void addCop_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                int roomWidth = 328 - room.LeftMargin * 8 - room.RightMargin * 8;
                room.Cops.Add(new Cop(room.LeftMargin * 8 + roomWidth / 2 - 16)); // middle of the room

                RefreshCopListBox();
                copListBox.SelectedIndex = copListBox.Items.Count - 1;
            }
        }

        private void removeCop_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                copListBox.SelectedIndex >= 0 && copListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Cops.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];


                room.Cops.RemoveAt(copListBox.SelectedIndex);

                RefreshCopListBox();
                if (copListBox.SelectedIndex >= 0)
                    copListBox.SelectedIndex = copListBox.SelectedIndex - 1;
            }
        }

        private void copListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                copListBox.SelectedIndex >= 0 && copListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Cops.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                xCop.Value = room.Cops[copListBox.SelectedIndex].X;
                copTextBox.Text = room.Cops[copListBox.SelectedIndex].Type.ToString();
            }
        }

        private void xCop_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                copListBox.SelectedIndex >= 0 && copListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Cops.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Cops[copListBox.SelectedIndex].X = (int)xCop.Value;
            }
        }

        private void copButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                copListBox.SelectedIndex >= 0 && copListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Cops.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Cops[copListBox.SelectedIndex].Type++;
                room.Cops[copListBox.SelectedIndex].Type = Math.Min(room.Cops[copListBox.SelectedIndex].Type, Cop.CopCount - 1);
                copTextBox.Text = room.Cops[copListBox.SelectedIndex].Type.ToString();
            }
        }

        private void copButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.SelectedLevel >= 0 && renderer.SelectedLevel < _building.Levels.Count &&
                renderer.SelectedFloor >= 0 && renderer.SelectedFloor < _building.Levels[renderer.SelectedLevel].Floors.Count &&
                renderer.SelectedRoom >= 0 && renderer.SelectedRoom < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms.Count &&
                copListBox.SelectedIndex >= 0 && copListBox.SelectedIndex < _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom].Cops.Count)
            {
                Room room = _building.Levels[renderer.SelectedLevel].Floors[renderer.SelectedFloor].Rooms[renderer.SelectedRoom];

                room.Cops[copListBox.SelectedIndex].Type--;
                room.Cops[copListBox.SelectedIndex].Type = Math.Max(room.Cops[copListBox.SelectedIndex].Type, 0);
                copTextBox.Text = room.Cops[copListBox.SelectedIndex].Type.ToString();
            }
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
