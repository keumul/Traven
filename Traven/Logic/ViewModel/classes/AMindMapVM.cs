using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Windows;

namespace Traven.Logic.ViewModel.classes
{
    public abstract class AMindMapVM
    {
        protected void onAddNode(object sender, ANodeVM node)
        {
            if (this.addNodeEvent != null)
                addNodeEvent(this, node);
        }
        protected void onRemoveNode(object sender, ANodeVM node)
        {
            if (this.removeNodeEvent != null)
                removeNodeEvent(this, node);
        }

        public abstract void setMainNode(ANodeVM node, bool blub);

        public abstract void removeNode(ANodeVM node, bool recursive = false);
        public abstract void addNode(ANodeVM node);
        public delegate void addNodeEventHandler(object sender, ANodeVM node);
        public delegate void removeNodeEventHandler(object sender, ANodeVM node);
        static public Tuple<Line, Grid> getDisplay(ANodeVM element)
        {
            Grid grid = element.getGrid();
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Black);
            line.StrokeThickness = 1;
            if (element.getParent() == null)
            {
                line = null;
            }
            else
            {
                line.X1 = element.getParent().getRectangle().Left + ((element.getParent().getRectangle().Right - element.getParent().getRectangle().Left) * 0.5);
                line.Y1 = element.getParent().getRectangle().Top + ((element.getParent().getRectangle().Bottom - element.getParent().getRectangle().Top) * 0.5);
                line.X2 = element.getRectangle().Left + ((element.getRectangle().Right - element.getRectangle().Left) * 0.5);
                line.Y2 = element.getRectangle().Top + ((element.getRectangle().Bottom - element.getRectangle().Top) * 0.5);
            }
            return Tuple.Create(line, grid);
        }
        public abstract XElement toXML();
        public abstract void fromXML(XElement XML);
        public abstract ANodeVM getMainNode();
        public event addNodeEventHandler addNodeEvent;
        public event removeNodeEventHandler removeNodeEvent;
        public delegate void changedSizeEventHandler(object sender, Size newSize);
        public event changedSizeEventHandler changeSizeEvent;
        protected void changeDrawSize(object sender, Size newSize)
        {
            if ((this.changeSizeEvent != null))
                changeSizeEvent(this, newSize);
        }
        public abstract void setDrawSize(Size newSize);
        public abstract Size getDrawSize();
    }
}
