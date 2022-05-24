using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System.Xml.Linq;
using Traven.Logic.ViewModel;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;

using Prism.Mvvm;
using Traven.Commands;
using Traven.Logic.Model;
using Traven.View;

namespace Traven.Logic.ViewModel
{
    public class MindMapMainVM : BindableBase
    {
        public ANodeVM _currentMarkedNode = null;
        Dictionary<ANodeVM, Tuple<Line, Grid>> _nodeRegistry;
        public AMindMapVM _mindmap;
        public System.Windows.Window Owner { get; set; }
        public MindMapMain mw = new MindMapMain();
        private MainWindowVM mainWindowVM;

        private MainWindowVM mmv { get; set; }

        public ICommand ToolBoxRemoveCommand { get; set; }
        public ICommand ChooseColorCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand ChangeBackgroundColorCommand { get; set; }
        public ICommand AddNodeCommand { get; set; }
        public ICommand RemoveNodeCommand { get; set; }
        public ICommand AddIconCommand { get; set; }
        public ICommand MenuItem1Command { get; set; }
        public ICommand MenuItem2Command { get; set; }
        public ICommand MenuItem3Command { get; set; }
        public ICommand Window_LoadedCommand { get; set; }
        public ICommand Key_PressedCommand { get; set; }
        public ICommand CanvasCommand { get; set; }
        public ICommand setLoadedViewCommand;
//-------------------------------------------------------
        public ICommand MindMapCommand { get; set; }
//-------------------------------------------------------
        public ICommand SetLoadedViewCommand
        {
            get => setLoadedViewCommand ?? (setLoadedViewCommand = new RelayCommand(parameter => Window_Loaded()));
        }
        private void Menu(object obj)
        {
            mainWindowVM.SetViewModel(VMEnum.Menu);
        }
        public MindMapMainVM(MainWindowVM mainWindowVM)
        {
            mainWindowVM = mainWindowVM;
            MindMapCommand = new RelayCommand(Menu);
            MindMapMain mmm = new MindMapMain();
            var model = mmm.DataContext as MindMapMainVM;
            this._nodeRegistry = new Dictionary<ANodeVM, Tuple<Line, Grid>>();
            this._mindmap = new MindMapNodesVM();
            this._mindmap.removeNodeEvent += removeNodeEventHandler;
            this._mindmap.changeSizeEvent += _mindmap_changeSizeEvent;
            this._mindmap.addNodeEvent += _mindmap_addNodeEvent;
            this.changeActiveNodeEvent += activeNodeChangedHandler;
            this.mainWindowVM = mainWindowVM;
            ToolBoxRemoveCommand = new RelayCommand(ToolboxRemove_Click);
            ChooseColorCommand = new RelayCommand(ChooseColor_Click);
            ExitCommand = new RelayCommand(Exit_Click);
            ChangeBackgroundColorCommand = new RelayCommand(ChangeBackgroundColor_Click);
            AddNodeCommand = new RelayCommand(addNode_Click);
            RemoveNodeCommand = new RelayCommand(removeNode_Click);
            AddIconCommand = new RelayCommand(AddIcon_Click);
            MenuItem1Command = new RelayCommand(MenuItem_Click);
            MenuItem2Command = new RelayCommand(MenuItem_Click_0);
            MenuItem3Command = new RelayCommand(MenuItem_Click_2);
            //Window_LoadedCommand = new RelayCommand(Window_Loaded);

        }

        private void Exit_Click(object obj)
        {
            mainWindowVM.SetViewModel(VMEnum.Menu);
        }

        private void _mindmap_addNodeEvent(object sender, ANodeVM node)
        {
            node.changeNodeEvent += this.handleNodeChanges;
            node.invalidate();
        }

        private void _mindmap_changeSizeEvent(object sender, Size newSize)
        {
            this.mw.MindMapCanvas.Width = newSize.Width;
            this.mw.MindMapCanvas.Height = newSize.Height;
        }

        public void removeNodeEventHandler(object sender, ANodeVM node)
        {
            if (this._nodeRegistry.ContainsKey(node))
            {
                if (this.mw.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item2))
                    this.mw.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item2);

                if (this.mw.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item1))
                    this.mw.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item1);
            }
            this._nodeRegistry.Remove(node);
        }

        private void handleNodeChanges(object sender, ANodeVM node)
        {
            Tuple<Line, Grid> nodeElement = AMindMapVM.getDisplay(node);
            if (this._nodeRegistry.ContainsKey(node))
            {
                if (this.mw.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item2))
                    this.mw.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item2);

                if (this.mw.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item1))
                    this.mw.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item1);
            }
            this._nodeRegistry[node] = nodeElement;

            if (nodeElement.Item1 != null)
            {
                Canvas.SetZIndex(nodeElement.Item1, 0);
                this.mw.MindMapCanvas.Children.Add(nodeElement.Item1);
            }

            Canvas.SetLeft(nodeElement.Item2, node.getRectangle().Left);
            Canvas.SetTop(nodeElement.Item2, node.getRectangle().Top);
            Canvas.SetZIndex(nodeElement.Item2, 1);
            nodeElement.Item2.MouseLeftButtonDown += Node_MouseLeftButton;
            nodeElement.Item2.MouseLeftButtonUp += Node_MouseLeftButton;
            this.mw.MindMapCanvas.Children.Add(nodeElement.Item2);

            if (this._currentMarkedNode == null)
                changeActiveNode(this, node);
        }

        private void Window_Loaded()
        {
            this.mw.MindMapCanvas.Clip = null;

            ANodeVM node = new NodeVM();

            node.beginUpdate();
            try
            {
                node.setForm(new RechteckVM());

                Point rectStartPoint = new Point(mw.MindMapCanvas.ActualWidth / 2 - 50, mw.MindMapCanvas.ActualHeight / 2 - 25);
                Point rectEndPoint = new Point(mw.MindMapCanvas.ActualWidth / 2 + 50, mw.MindMapCanvas.ActualHeight / 2 + 25);

                node.setRectangle(new Rect(rectStartPoint, rectEndPoint));

                AStyleVM nodeStyle = new StyleVM();
                nodeStyle.setColor(Tuple.Create((mw.colorRect.Fill as SolidColorBrush).Color, (bool)mw.fillCheckBox.IsChecked));
                node.setStyle(nodeStyle);

                node.setParent(null);
                this._mindmap.setMainNode(node, false);
                node.getStyle().setActivated(true);

                node.setText(this.mw.nodeText.Text);

            }
            finally
            {
                node.endUpdate();

                _currentMarkedNode = node;
            }

        }
        private void ToolboxAdd()
        {
            if (this.mw.DockPanel.Children.Contains(mw.Toolbox) == false)
            {
                this.mw.DockPanel.Children.Insert(1, mw.Toolbox);
            }
        }
        private void ToolboxAdd_Click(object sender)
        {
            ToolboxAdd();
        }
        private void ToolboxRemove()
        {
            if (this.mw.DockPanel.Children.Contains(mw.Toolbox))
            {
                this.mw.DockPanel.Children.Remove(mw.Toolbox);
            }
        }
        private void ToolboxRemove_Click(object sender)
        {
            ToolboxRemove();
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mw.FontSizeLabel.Content = "Размер шрифта: " + System.Convert.ToInt32(this.mw.FontSlider.Value).ToString() + "px";
            if (this._currentMarkedNode != null)
                this._currentMarkedNode.getStyle().setFontsize((int)this.mw.FontSlider.Value);
        }
        private void ChooseColor_Click(object sender)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = true;
            MyDialog.ShowDialog();
            //Brush
            SolidColorBrush myBrush = new SolidColorBrush();
            myBrush.Color = System.Windows.Media.Color.FromArgb(255, MyDialog.Color.R, MyDialog.Color.G, MyDialog.Color.B);
            mw.colorRect.Fill = myBrush;
            _currentMarkedNode.changeNodeEvent += this.handleNodeChanges;
            _currentMarkedNode.beginUpdate();
            try
            {
                IStyle nodeStyle = _currentMarkedNode.getStyle();
                nodeStyle.setColor(Tuple.Create((this.mw.colorRect.Fill as SolidColorBrush).Color, (bool)this.mw.fillCheckBox.IsChecked));
                _currentMarkedNode.setStyle((AStyleVM)nodeStyle);
            }
            finally
            {
                _currentMarkedNode.endUpdate();
            }
        }

        private void ChangeBackgroundColor_Click(object sender)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = false;
            MyDialog.ShowDialog();
            SolidColorBrush myBrush = new SolidColorBrush();
            myBrush.Color = System.Windows.Media.Color.FromArgb(255, MyDialog.Color.R, MyDialog.Color.G, MyDialog.Color.B);

            this.mw.MindMapCanvas.Background = myBrush;
            this.mw.Background = myBrush;
        }
        Point click;
        Rect grundRechteck;
        private void Node_MouseLeftButton(object sender, MouseButtonEventArgs e)
        {
            if ((sender is Grid))
            {
                if ((e.GetPosition(mw.MindMapCanvas).X > (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width - 10)) &&
                    (e.GetPosition(mw.MindMapCanvas).X < (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width)) &&
                    (e.GetPosition(mw.MindMapCanvas).Y > (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height - 10)) &&
                    (e.GetPosition(mw.MindMapCanvas).Y < (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height)))
                {
                    Mouse.SetCursor(System.Windows.Input.Cursors.Cross);
                }
            }
            if ((sender is Grid) && (e.LeftButton == MouseButtonState.Pressed))
            {
                if ((e.GetPosition(mw.MindMapCanvas).X > (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width - 10)) &&
                    (e.GetPosition(mw.MindMapCanvas).X < (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width)) &&
                    (e.GetPosition(mw.MindMapCanvas).Y > (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height - 10)) &&
                    (e.GetPosition(mw.MindMapCanvas).Y < (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height)))
                {
                    size = true;
                    click = e.GetPosition(mw.MindMapCanvas);
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeAll;
                    grundRechteck = this._currentMarkedNode.getRectangle();
                }
                else
                    this.dragging = true;
                Grid tempGrid = sender as Grid;
                this.dragStart = e.GetPosition(tempGrid);

                foreach (KeyValuePair<ANodeVM, Tuple<Line, Grid>> tempKeyValuePair in this._nodeRegistry)
                {
                    if (tempKeyValuePair.Value.Item2 == tempGrid)
                    {
                        this.changeActiveNode(this, tempKeyValuePair.Key);
                        break;
                    }
                }
            }
            else if ((sender is Grid) && (e.LeftButton == MouseButtonState.Released))
            {
                this.dragging = false;
                this.size = false;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
            if (e.LeftButton == MouseButtonState.Released)
            {
                this.dragging = false;
                this.size = false;
            }
        }
        private bool dragging = false;
        private Point dragStart;
        bool size = false;
        private void MindMapCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is IInputElement)
            {
                Rect tempRect;
                if (dragging)
                {
                    tempRect = this._currentMarkedNode.getRectangle();
                    tempRect.X = e.GetPosition((IInputElement)sender).X - this.dragStart.X;
                    tempRect.Y = e.GetPosition((IInputElement)sender).Y - this.dragStart.Y;
                    this._currentMarkedNode.setRectangle(tempRect);

                }
                else if (size)
                {
                    tempRect = this._currentMarkedNode.getRectangle();

                    if ((e.GetPosition((IInputElement)sender).X - this.click.X + grundRechteck.Width) >= 29)
                        tempRect.Width = e.GetPosition((IInputElement)sender).X - this.click.X + grundRechteck.Width;
                    else
                    {

                        tempRect.Width = 30;
                    }
                    if ((e.GetPosition((IInputElement)sender).Y - this.click.Y + grundRechteck.Height) >= 29)
                        tempRect.Height = e.GetPosition((IInputElement)sender).Y - this.click.Y + grundRechteck.Height;
                    else
                        tempRect.Height = 30;
                    this._currentMarkedNode.setRectangle(tempRect);
                }

            }
        }

        private void fillCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _currentMarkedNode.changeNodeEvent += this.handleNodeChanges;
            _currentMarkedNode.beginUpdate();
            try
            {
                IStyle nodeStyle = _currentMarkedNode.getStyle();
                nodeStyle.setColor(Tuple.Create((this.mw.colorRect.Fill as SolidColorBrush).Color, (bool)this.mw.fillCheckBox.IsChecked));
                _currentMarkedNode.setStyle((AStyleVM)nodeStyle);

            }
            finally
            {
                _currentMarkedNode.endUpdate();
            }
        }
        private void fillCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _currentMarkedNode.changeNodeEvent += this.handleNodeChanges;
            _currentMarkedNode.beginUpdate();
            try
            {
                IStyle nodeStyle = _currentMarkedNode.getStyle();
                nodeStyle.setColor(Tuple.Create((this.mw.colorRect.Fill as SolidColorBrush).Color, (bool)this.mw.fillCheckBox.IsChecked));
                _currentMarkedNode.setStyle((AStyleVM)nodeStyle);

            }
            finally
            {
                _currentMarkedNode.endUpdate();
            }
        }
        private void nodeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentMarkedNode != null)
            {
                _currentMarkedNode.changeNodeEvent += this.handleNodeChanges;
                _currentMarkedNode.beginUpdate();
                try
                {

                    _currentMarkedNode.setText(this.mw.nodeText.Text);

                }
                finally
                {
                    _currentMarkedNode.endUpdate();
                }
            }
        }
        private void addNode_Click(object sender)
        {
            ANodeVM node = new NodeVM();
            node.beginUpdate();
            try
            {
                switch (this.mw.ChooseForm.SelectedIndex)
                {
                    case 0:
                        node.setForm(new RechteckVM());
                        break;
                    case 1:
                        node.setForm(new EllipseVM());
                        break;
                    default:
                        break;
                }
                Rect rLeft = new Rect(new Point(_currentMarkedNode.getRectangle().TopLeft.X + 150, _currentMarkedNode.getRectangle().TopLeft.Y), new Point(_currentMarkedNode.getRectangle().BottomRight.X + 150, _currentMarkedNode.getRectangle().BottomRight.Y));

                Rect rRight = new Rect(new Point(_currentMarkedNode.getRectangle().TopLeft.X - 150, _currentMarkedNode.getRectangle().TopLeft.Y), new Point(_currentMarkedNode.getRectangle().BottomRight.X - 150, _currentMarkedNode.getRectangle().BottomRight.Y));

                Rect rTop = new Rect(new Point(_currentMarkedNode.getRectangle().TopLeft.X, _currentMarkedNode.getRectangle().TopLeft.Y - 150), new Point(_currentMarkedNode.getRectangle().BottomRight.X, _currentMarkedNode.getRectangle().BottomRight.Y - 150));

                Rect rBot = new Rect(new Point(_currentMarkedNode.getRectangle().TopLeft.X, _currentMarkedNode.getRectangle().TopLeft.Y + 150), new Point(_currentMarkedNode.getRectangle().BottomRight.X, _currentMarkedNode.getRectangle().BottomRight.Y + 150));
                Rect r = new Rect();
                Random random = new Random();
                switch (random.Next(0, 4))
                {
                    case 0:
                        r = rLeft;
                        break;
                    case 1:
                        r = rRight;
                        break;
                    case 2:
                        r = rTop;
                        break;
                    case 3:
                        r = rBot;
                        break;
                }
                node.setRectangle(r);

                AStyleVM nodeStyle = new StyleVM();
                nodeStyle.setColor(Tuple.Create((this.mw.colorRect.Fill as SolidColorBrush).Color, (bool)this.mw.fillCheckBox.IsChecked));
                node.setStyle(nodeStyle);

                node.setParent(_currentMarkedNode);
                this._mindmap.addNode(node);

                node.setText("Новый узел");
            }
            finally
            {
                node.endUpdate();
                this.changeActiveNode(this, node);
            }
        }

        protected void changeActiveNode(object sender, ANodeVM node)
        {
            if ((this.changeActiveNodeEvent != null))
                changeActiveNodeEvent(this, node);
        }

        public delegate void changedActiveNodeEventHandler(object sender, ANodeVM node);

        public event changedActiveNodeEventHandler changeActiveNodeEvent;
        private void activeNodeChangedHandler(object sender, ANodeVM node)
        {
            if (this._currentMarkedNode != node)
            {
                if (this._currentMarkedNode != null)
                    this._currentMarkedNode.getStyle().setActivated(false);
                this._currentMarkedNode = node;

                this.mw.removeNOde.IsEnabled = (this._currentMarkedNode.getParent() != null);

                this._currentMarkedNode.getStyle().setActivated(true);

                this.mw.nodeText.Text = this._currentMarkedNode.getText();

                this.mw.FontSlider.Value = this._currentMarkedNode.getStyle().getFontsize();

                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString(this._currentMarkedNode.getStyle().getColor().Item1.ToString());


                this.mw.colorRect.Fill = brush;
                this.mw.fillCheckBox.IsChecked = this._currentMarkedNode.getStyle().getColor().Item2;

                if (this._currentMarkedNode.getForm().ToString() == "Прямоугольник")
                {
                    this.mw.ChooseForm.SelectedIndex = 0;
                }
                else
                {
                    this.mw.ChooseForm.SelectedIndex = 1;
                }

                this.mw.IconImage.Source = _currentMarkedNode.getStyle().getIcon();
            }
        }
        private void ChooseForm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentMarkedNode != null)
            {
                _currentMarkedNode.changeNodeEvent += this.handleNodeChanges;
                _currentMarkedNode.beginUpdate();
                try
                {
                    switch (this.mw.ChooseForm.SelectedIndex)
                    {
                        case 0:
                            _currentMarkedNode.setForm(new RechteckVM());
                            break;
                        case 1:
                            _currentMarkedNode.setForm(new EllipseVM());
                            break;
                        default:
                            break;
                    }
                }
                finally
                {
                    _currentMarkedNode.endUpdate();
                }
            }

        }

        private void removeNode_Click(object sender)
        {
            ANodeVM an = _currentMarkedNode;
            if (this._currentMarkedNode.getParent() != null)
                changeActiveNode(this, _currentMarkedNode.getParent());
            this._mindmap.removeNode(an, true);

        }

        private void MindMapCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this._mindmap.setDrawSize(e.NewSize);
        }

        private void MenuItem_Click(object sender)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Mindmap"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension

            dlg.Filter = "Picture documents (.png)|*.png|Xml Document(.xml)|*.xml"; // Filter files by extension


            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            //Process save file dialog box results

            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                Uri path = new Uri(filename);
                if (dlg.FileName.Contains(".xml"))
                    ExportXml(path, _mindmap.toXML().ToString());
                else
                    ExportToPng(path, this.mw.MindMapCanvas);
            }
        }

        public void ExportXml(Uri path, String xml)
        {
            FileStream outStream = new FileStream(path.LocalPath, FileMode.Create);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            outStream.Write(bytes, 0, bytes.Length);
            outStream.Close();
        }
        public void ExportToPng(Uri path, Canvas surface)
        {
            if (path == null) return;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(surface.ActualWidth, surface.ActualHeight);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }

        private void MenuItem_Click_0(object sender)
        {
            string filename = @"C:\Users\Katerina\Desktop\BSTU\4s2k\курсовая\KateApp\StartedMindMap.xml";

            this._currentMarkedNode = null;
            this._nodeRegistry.Clear();
            this.mw.MindMapCanvas.Children.Clear();
            FileStream input = new FileStream(filename, FileMode.Open);
            byte[] xxx = new byte[input.Length];
            input.Read(xxx, 0, (int)input.Length);
            string xml = System.Text.Encoding.UTF8.GetString(xxx);
            input.Close();
            XElement hgsfg = XElement.Parse(xml);
            _mindmap.fromXML(hgsfg);
            this.changeActiveNode(this, _mindmap.getMainNode());

        }

        private void MenuItem_Click_2(object sender)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Mindmap"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension

            dlg.Filter = "Xml Document(.xml)|*.xml"; // Filter files by extension


            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            //Process save file dialog box results

            if (result == true)
            {
                this._currentMarkedNode = null;
                this._nodeRegistry.Clear();
                this.mw.MindMapCanvas.Children.Clear();
                FileStream input = new FileStream(dlg.FileName, FileMode.Open);
                byte[] xxx = new byte[input.Length];
                input.Read(xxx, 0, (int)input.Length);
                string xml = System.Text.Encoding.UTF8.GetString(xxx);
                input.Close();
                XElement hgsfg = XElement.Parse(xml);
                _mindmap.fromXML(hgsfg);
                this.changeActiveNode(this, _mindmap.getMainNode());
            }
        }

        private void AddIcon_Click(object sender)
        {

            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".png";
            dlg.Filter = "Pictures (.png)|*.png";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                //FileNameTextBox.Text = filename;

                //Image finalImage = new Image();

                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.UriSource = new Uri(filename);
                logo.EndInit();

                this.mw.IconImage.Source = logo;
                this._currentMarkedNode.getStyle().setICon(logo);

            }

        }
    }
}
