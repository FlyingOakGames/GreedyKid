using System;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;

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

        private Building _building = new Building("My Building");

        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            InitializeComponent();

            //RenderOptions.SetBitmapScalingMode(tilesetViewerCanvas, BitmapScalingMode.NearestNeighbor);

            mouseThread = new Thread(MouseWorker);
            mouseThread.Start();

            IntPtr handle = monoGameRenderPanel.Handle;
            rendererThread = new Thread(new ThreadStart(() => { renderer = new PreviewRenderer(handle, this, _building); renderer.Run(); }));
            rendererThread.Start();

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
                levelItem.Header = "Level " + (l + 1) + ": " + _building.Levels[l].Name;

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
                        selectedItem.Header = "Level " + (selectedItem.Level + 1) + ": " + name;
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

                    if (selectedItem.Type == BuildingElement.Room)
                    {
                        renderer.SelectedFloor = selectedItem.Floor;
                        renderer.SelectedRoom = selectedItem.Room;

                        paintTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].BackgroundColor.ToString();
                        leftMarginTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftMargin.ToString();
                        rightMarginTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightMargin.ToString();
                        leftDecorationTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].LeftDecoration.ToString();
                        rightDecorationTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration.ToString();

                        RefreshDetailListBox();
                        RefreshFloorDoorListBox();
                        RefreshRoomDoorListBox();
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
                if (selectedItem.Type == BuildingElement.Floor)
                {
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration--;
                    _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration = Math.Max(_building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration, 0);
                    rightDecorationTextBox.Text = _building.Levels[selectedItem.Level].Floors[selectedItem.Floor].Rooms[selectedItem.Room].RightDecoration.ToString();
                }
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
                room.Details[detailListBox.SelectedIndex].Type = Math.Min(room.Details[detailListBox.SelectedIndex].Type, Detail.DetailCount - 1);
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

                RefreshDetailListBox();
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
    }
}
