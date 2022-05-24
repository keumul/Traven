using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Traven.Commands;
using Traven.Logic.Model;
using Traven.Logic.ViewModel;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.UOW;

namespace Traven.View
{
    /// <summary>
    /// Логика взаимодействия для MindMapMain.xaml
    /// </summary>
    public partial class MindMapMain : System.Windows.Controls.UserControl
    {
        ICommand command;
        ANodeVM _currentMarkedNode = null;
        //ANodeVM _previousMarkedNode = null;
        Dictionary<ANodeVM, Tuple<Line, Grid>> _nodeRegistry;
        AMindMapVM _mindmap;
        Dictionary<int, ANodeVM> _nodes;

        public MindMapMain()
        {
            this._nodeRegistry = new Dictionary<ANodeVM, Tuple<Line, Grid>>();
            this._mindmap = new MindMapNodesVM();
            this._mindmap.removeNodeEvent += removeNodeEventHandler;
            this._mindmap.changeSizeEvent += _mindmap_changeSizeEvent;
            this._mindmap.addNodeEvent += _mindmap_addNodeEvent;
            _nodes = new Dictionary<int, ANodeVM>();
            InitializeComponent();
            this.changeActiveNodeEvent += activeNodeChangedHandler;
        }
        void _mindmap_addNodeEvent(object sender, ANodeVM node)
        {
            node.changeNodeEvent += this.handleNodeChanges;
            node.invalidate();
        }

        void _mindmap_changeSizeEvent(object sender, Size newSize)
        {
            this.MindMapCanvas.Width = newSize.Width;
            this.MindMapCanvas.Height = newSize.Height;
        }

        public void removeNodeEventHandler(object sender, ANodeVM node)
        {
            if (this._nodeRegistry.ContainsKey(node))
            {
                if (this.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item2))
                    this.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item2);

                if (this.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item1))
                    this.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item1);

                
            }
            this._nodeRegistry.Remove(node);

            using (UnitOfWork unit = new UnitOfWork())
            {
                Node dbnode = unit.NodeRepository.GetAll().Where(n => n.Id == node.ID).FirstOrDefault();
                RemoveChilds(dbnode);
                removed.Add(dbnode.Id);
            }

            RemoveFromList(removed);
            removed.Clear();
        }

        private List<int> removed = new List<int>();
        
        public void RemoveFromList(List<int> ids)
        {
            using (UnitOfWork unit = new UnitOfWork())
            {
                foreach (int id in ids)
                {
                    //Node dbnode = unit.NodeRepository.GetWithInclude(n => n.Settings, n => n.FatherNode).Where(n => n.Id == id).FirstOrDefault();
                    unit.NodeRepository.Delete(id);
                }
            }
        }
        
        public void RemoveChilds(Node node)
        {
            using (UnitOfWork unit = new UnitOfWork())
            {
                List<Node> childs = unit.NodeRepository.GetAll().Where(n => n.FatherId == node.Id).ToList();
                foreach (Node child in childs)
                {
                    RemoveChilds(child);
                    removed.Add(child.Id);
                }
            }
        }

        private void handleNodeChanges(object sender, ANodeVM node)
        {
            Tuple<Line, Grid> nodeElement = AMindMapVM.getDisplay(node);
            if (this._nodeRegistry.ContainsKey(node))
            {
                if (this.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item2))
                    this.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item2);

                if (this.MindMapCanvas.Children.Contains(this._nodeRegistry[node].Item1))
                    this.MindMapCanvas.Children.Remove(this._nodeRegistry[node].Item1);
            }
            this._nodeRegistry[node] = nodeElement;

            if (nodeElement.Item1 != null)
            {
                Canvas.SetZIndex(nodeElement.Item1, 0);
                this.MindMapCanvas.Children.Add(nodeElement.Item1);
            }

            Canvas.SetLeft(nodeElement.Item2, node.getRectangle().Left);
            Canvas.SetTop(nodeElement.Item2, node.getRectangle().Top);
            Canvas.SetZIndex(nodeElement.Item2, 1);
            nodeElement.Item2.MouseLeftButtonDown += Node_MouseLeftButton;
            nodeElement.Item2.MouseLeftButtonUp += Node_MouseLeftButton;
            this.MindMapCanvas.Children.Add(nodeElement.Item2);

            if (this._currentMarkedNode == null)
                changeActiveNode(this, node);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.MapId == -1)
            {
                this.MindMapCanvas.Clip = null;

                ANodeVM node = new NodeVM();

                node.beginUpdate();
                try
                {
                    node.setForm(new RechteckVM());

                    Point rectStartPoint = new Point(MindMapCanvas.ActualWidth / 2 - 50, this.MindMapCanvas.ActualHeight / 2 - 25);
                    Point rectEndPoint = new Point(MindMapCanvas.ActualWidth / 2 + 50, this.MindMapCanvas.ActualHeight / 2 + 25);

                    node.setRectangle(new Rect(rectStartPoint, rectEndPoint));

                    AStyleVM nodeStyle = new StyleVM();
                    nodeStyle.setColor(Tuple.Create((this.colorRect.Fill as SolidColorBrush).Color, (bool)this.fillCheckBox.IsChecked));
                    node.setStyle(nodeStyle);

                    node.setParent(null);
                    this._mindmap.setMainNode(node, false);
                    node.getStyle().setActivated(true);

                    node.setText(this.nodeText.Text);
                    using (UnitOfWork unit = new UnitOfWork())
                    {
                        Traven.Logic.Model.Settings settings = new Traven.Logic.Model.Settings()
                        {
                            Color = (this.colorRect.Fill as SolidColorBrush).Color.ToString(),
                            Fill = (bool)this.fillCheckBox.IsChecked,
                            Font = (int)this.FontSlider.Value
                            //Icon = node.getForm().getIcon()

                        };
                        unit.SettingsRepository.Create(settings);
                        unit.Save();
                        Node dbnode = new Node()
                        {
                            Text = this.nodeText.Text,
                            X = (float)node.getRectangle().TopLeft.X,
                            Y = (float)node.getRectangle().TopLeft.Y,
                            Wight = (float)node.getRectangle().Width,
                            Height = (float)node.getRectangle().Height,
                            Settings = settings
                        };
                        unit.NodeRepository.Create(dbnode);
                        unit.Save();

                        node.ID = dbnode.Id;

                        _nodes.Add(node.ID, node);

                        MindMap map = new MindMap()
                        {
                            Creator = unit.UserRepository.Get(CurrentUser.Id),
                            Node = dbnode
                        };
                        unit.MindMapRepository.Create(map);
                        unit.Save();
                        CurrentUser.MapId = map.Id;
                    }
                }
                finally
                {
                    node.endUpdate();

                    _currentMarkedNode = node;
                }
            }
            else
            {
                using (UnitOfWork unit = new UnitOfWork())
                {
                    List<MindMap> mindmap = unit.MindMapRepository.GetAll().ToList();
                    MindMap mindMap = unit.MindMapRepository.GetWithInclude(m => m.Node).Where(m => m.Id == CurrentUser.MapId).FirstOrDefault();
                    Node dbnode = unit.NodeRepository.GetWithInclude(n => n.Settings, n => n.FatherNode).Where(n => n.Id == mindMap.Node.Id).FirstOrDefault();
                    ANodeVM node = new NodeVM();
                    node.beginUpdate();
                    Rect rect = new Rect(dbnode.X, dbnode.Y, dbnode.Wight, dbnode.Height);
                    node.setRectangle(rect);
                    AStyleVM style = new StyleVM();
                    Settings settings = unit.SettingsRepository.GetAll().Where(s => s.Id == dbnode.Settings.Id).FirstOrDefault();
                    string color = settings.Color;
                    byte alpha = Convert.ToByte(color.Substring(1, 2), 16);
                    byte red = Convert.ToByte(color.Substring(3, 2), 16);
                    byte green = Convert.ToByte(color.Substring(5, 2), 16);
                    byte blue = Convert.ToByte(color.Substring(7, 2), 16);
                    style.setColor(Tuple.Create(Color.FromArgb(alpha, red, green, blue), settings.Fill));
                    node.setStyle(style);
                    node.setParent(null);
                    this._mindmap.setMainNode(node, false);
                    node.getStyle().setActivated(true);
                    node.setText(dbnode.Text);
                    node.ID = dbnode.Id;
                    node.endUpdate();
                    _currentMarkedNode = node;
                    setChildNode(node);
                }
            }
        }

        private void setChildNode(ANodeVM parent)
        {
            using (UnitOfWork unit = new UnitOfWork())
            {
                List<Node> nodes = unit.NodeRepository.GetWithInclude(n => n.Settings, n => n.FatherNode).Where(n => n.FatherId == parent.ID).ToList();
                foreach (var dbnode in nodes)
                {
                    ANodeVM node = new NodeVM();
                    node.beginUpdate();
                    Rect rect = new Rect(dbnode.X, dbnode.Y, dbnode.Wight, dbnode.Height);
                    node.setRectangle(rect);
                    AStyleVM style = new StyleVM();
                    Settings settings = unit.SettingsRepository.GetAll().Where(s => s.Id == dbnode.Settings.Id).FirstOrDefault();
                    string color = settings.Color;
                    byte alpha = Convert.ToByte(color.Substring(1, 2), 16);
                    byte red = Convert.ToByte(color.Substring(3, 2), 16);
                    byte green = Convert.ToByte(color.Substring(5, 2), 16);
                    byte blue = Convert.ToByte(color.Substring(7, 2), 16);
                    style.setColor(Tuple.Create(Color.FromArgb(alpha, red, green, blue), settings.Fill));
                    node.setStyle(style);
                    node.setParent(parent);
                    this._mindmap.addNode(node);
                    node.getStyle().setActivated(true);
                    node.setText(dbnode.Text);
                    node.ID = dbnode.Id;
                    node.endUpdate();
                    _currentMarkedNode = node;
                    setChildNode(node);
                }
            }
        }

        private void ToolboxAdd()
        {
            if (this.DockPanel.Children.Contains(Toolbox) == false)
            {
                this.DockPanel.Children.Insert(1, Toolbox);
            }
        }
        private void ToolboxAdd_Click(object sender, RoutedEventArgs e)
        {
            ToolboxAdd();
        }
        private void ToolboxRemove()
        {
            if (this.DockPanel.Children.Contains(Toolbox))
            {
                this.DockPanel.Children.Remove(Toolbox);
            }
        }
        private void ToolboxRemove_Click(object sender, RoutedEventArgs e)
        {
            ToolboxRemove();
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            this.FontSizeLabel.Content = "Размер шрифта: " + System.Convert.ToInt32(this.FontSlider.Value).ToString() + "px";
            if (this._currentMarkedNode != null)
                this._currentMarkedNode.getStyle().setFontsize((int)this.FontSlider.Value);
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }
        private void KeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == Key.Escape && this.UserControlStyle == System.Windows.UserControlStyle.None)
            //{
            //    ExitFullscreen();
            //}
        }
        private void ChooseColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = true;
            MyDialog.ShowDialog();
            //Brush
            SolidColorBrush myBrush = new SolidColorBrush();
            myBrush.Color = System.Windows.Media.Color.FromArgb(255, MyDialog.Color.R, MyDialog.Color.G, MyDialog.Color.B);
            colorRect.Fill = myBrush;

            _currentMarkedNode.changeNodeEvent += this.handleNodeChanges;
            _currentMarkedNode.beginUpdate();
            try
            {
                IStyle nodeStyle = _currentMarkedNode.getStyle();
                nodeStyle.setColor(Tuple.Create((this.colorRect.Fill as SolidColorBrush).Color, (bool)this.fillCheckBox.IsChecked));
                _currentMarkedNode.setStyle((AStyleVM)nodeStyle);
            }
            finally
            {
                _currentMarkedNode.endUpdate();
            }
        }
        private void ChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = false;
            MyDialog.ShowDialog();
            SolidColorBrush myBrush = new SolidColorBrush();
            myBrush.Color = System.Windows.Media.Color.FromArgb(255, MyDialog.Color.R, MyDialog.Color.G, MyDialog.Color.B);

            this.MindMapCanvas.Background = myBrush;
            this.Background = myBrush;
        }
        Point click;
        Rect grundRechteck;
        private void Node_MouseLeftButton(object sender, MouseButtonEventArgs e)
        {
            if ((sender is Grid))
            {
                if ((e.GetPosition(MindMapCanvas).X > (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width - 10)) && (e.GetPosition(MindMapCanvas).X < (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width)) && (e.GetPosition(MindMapCanvas).Y > (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height - 10)) && (e.GetPosition(MindMapCanvas).Y < (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height)))
                {
                    Mouse.SetCursor(System.Windows.Input.Cursors.Cross);
                }
            }
            if ((sender is Grid) && (e.LeftButton == MouseButtonState.Pressed))
            {
                if ((e.GetPosition(MindMapCanvas).X > (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width - 10)) && (e.GetPosition(MindMapCanvas).X < (this._currentMarkedNode.getRectangle().X + this._currentMarkedNode.getRectangle().Width)) && (e.GetPosition(MindMapCanvas).Y > (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height - 10)) && (e.GetPosition(MindMapCanvas).Y < (this._currentMarkedNode.getRectangle().Y + this._currentMarkedNode.getRectangle().Height)))
                {
                    size = true;
                    click = e.GetPosition(MindMapCanvas);
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
                nodeStyle.setColor(Tuple.Create((this.colorRect.Fill as SolidColorBrush).Color, (bool)this.fillCheckBox.IsChecked));
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
                nodeStyle.setColor(Tuple.Create((this.colorRect.Fill as SolidColorBrush).Color, (bool)this.fillCheckBox.IsChecked));
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

                    _currentMarkedNode.setText(this.nodeText.Text);
                    using(UnitOfWork unit = new UnitOfWork())
                    {
                        
                        Node dbnode = unit.NodeRepository.GetWithInclude(n => n.Settings).Where(n => n.Id == _currentMarkedNode.ID).FirstOrDefault();
                        dbnode.Text = this.nodeText.Text;

                        unit.NodeRepository.Update(dbnode);
                        unit.Save();
                    }
                }
                finally
                {
                    _currentMarkedNode.endUpdate();
                }
            }
        }
        private void addNode_Click(object sender, RoutedEventArgs e)
        {
            ANodeVM node = new NodeVM();
            node.beginUpdate();
            try
            {
                switch (this.ChooseForm.SelectedIndex)
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
                //Links
                Rect rLeft = new Rect(new Point(_currentMarkedNode.getRectangle().TopLeft.X + 150, _currentMarkedNode.getRectangle().TopLeft.Y), new Point(_currentMarkedNode.getRectangle().BottomRight.X + 150, _currentMarkedNode.getRectangle().BottomRight.Y));
                //Rechts
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
                nodeStyle.setColor(Tuple.Create((this.colorRect.Fill as SolidColorBrush).Color, (bool)this.fillCheckBox.IsChecked));
                node.setStyle(nodeStyle);

                node.setParent(_currentMarkedNode);
                this._mindmap.addNode(node);

                node.setText("meow");


                using (UnitOfWork unit = new UnitOfWork())
                {
                    Traven.Logic.Model.Settings settings = new Traven.Logic.Model.Settings()
                    {
                        Color = (this.colorRect.Fill as SolidColorBrush).Color.ToString(),
                        Fill = (bool)this.fillCheckBox.IsChecked,
                        Font = (int)this.FontSlider.Value
                        //Icon = node.getForm().getIcon()

                    };
                    unit.SettingsRepository.Create(settings);
                    unit.Save();
                    Node father = unit.NodeRepository.GetAll().FirstOrDefault(x => x.Id == _currentMarkedNode.ID);
                    unit.Save();
                    Node dbnode = new Node()
                    {
                        Text = this.nodeText.Text,
                        X = (float)node.getRectangle().TopLeft.X,
                        Y = (float)node.getRectangle().TopLeft.Y,
                        Wight = (float)node.getRectangle().Width,
                        Height = (float)node.getRectangle().Height,
                        FatherId = father.Id,
                        Settings = settings
                    };
                    unit.NodeRepository.Create(dbnode);
                    unit.Save();
                    //Node updated = unit.NodeRepository.GetWithInclude(n => n.Settings, n => n.Father).FirstOrDefault(x => x.Id == dbnode.Id);
                    //dbnode.Father.Id = father.Id;
                    //unit.NodeRepository.Update(dbnode);
                    //unit.Save();
                    node.ID = dbnode.Id;
                    _nodes.Add(node.ID, node);
                }
            }
            finally
            {
                node.endUpdate();
                this.changeActiveNode(this, node);
            }
        }
        private void MenuItem_Click_0(object sender, RoutedEventArgs e)
        {
            using (UnitOfWork unit = new UnitOfWork())
            {
                unit.MindMapRepository.Delete(CurrentUser.MapId);
                unit.Save();
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

                this.removeNOde.IsEnabled = (this._currentMarkedNode.getParent() != null);

                this._currentMarkedNode.getStyle().setActivated(true);

                this.nodeText.Text = this._currentMarkedNode.getText();

                this.FontSlider.Value = this._currentMarkedNode.getStyle().getFontsize();

                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString(this._currentMarkedNode.getStyle().getColor().Item1.ToString());


                this.colorRect.Fill = brush;
                this.fillCheckBox.IsChecked = this._currentMarkedNode.getStyle().getColor().Item2;

                if (this._currentMarkedNode.getForm().ToString() == "Rechteck")
                {
                    this.ChooseForm.SelectedIndex = 0;
                }
                else
                {
                    this.ChooseForm.SelectedIndex = 1;
                }

                this.IconImage.Source = _currentMarkedNode.getStyle().getIcon();



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
                    switch (this.ChooseForm.SelectedIndex)
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

        private void removeNOde_Click(object sender, RoutedEventArgs e)
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
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
                    ExportToPng(path, this.MindMapCanvas);
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

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
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
                this.MindMapCanvas.Children.Clear();
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

        private void AddIcon_Click(object sender, RoutedEventArgs e)
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

                this.IconImage.Source = logo;
                this._currentMarkedNode.getStyle().setICon(logo);

            }

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            using (UnitOfWork unit = new UnitOfWork())
            {
                foreach (KeyValuePair<ANodeVM, Tuple<Line, Grid>> key in this._nodeRegistry)
                {
                    ANodeVM node = key.Key;
                    Node dbnode = unit.NodeRepository.GetAll().Where(n => n.Id == node.ID).FirstOrDefault();
                    Settings settings = unit.NodeRepository.GetWithInclude(n => n.Settings, n => n.FatherNode).Where(n => n.Id == node.ID).FirstOrDefault().Settings;
                    var rect = node.getRectangle();
                    dbnode.X = (float)rect.X;
                    dbnode.Y = (float)rect.Y;
                    dbnode.Wight = (float)rect.Width;
                    dbnode.Height = (float)rect.Height;
                    var style = node.getStyle();
                    var color = style.getColor();
                    settings.Color = color.Item1.ToString();
                    settings.Font = (int)style.getFontsize();
                    settings.Fill = color.Item2;
                    unit.SettingsRepository.Update(settings);
                    unit.NodeRepository.Update(dbnode);
                }
            }
        }
    }
}
