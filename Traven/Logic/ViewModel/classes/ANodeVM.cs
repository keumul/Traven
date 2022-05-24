using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Traven.Logic.ViewModel.classes
{
    public abstract class ANodeVM
    {
        public int ID { get; set; }
        private bool _onUpdateing = false;
        public void beginUpdate()
        {
            this._onUpdateing = true;
        }
        public void endUpdate()
        {
            this._onUpdateing = false;
            this.changeNode(this, this);
        }
        public abstract Rect getRectangle();
        public abstract void setRectangle(Rect rectangle);
        public abstract ANodeVM getParent();
        public abstract void setParent(ANodeVM parent);
        public abstract string getText();
        public abstract void setText(string text);
        public abstract IStyle getStyle();
        public virtual void setStyle(AStyleVM style)
        {
            style.changeStyleEvent += this.onChangedStyle;
        }
        protected virtual void onChangedStyle(object sender, IStyle node)
        {
            this.changeNode(sender, this);
        }
        public abstract void setForm(IForm form);
        public abstract IForm getForm();
        public Grid getGrid()
        {
            return this.getForm().getStrokeFromNode(this);
        }
        public abstract XElement toXML();
        public abstract void fromXML(XElement XML);

        public delegate void changedNodeEventHandler(object sender, ANodeVM node);

        public event changedNodeEventHandler changeNodeEvent;
        protected void changeNode(object sender, ANodeVM node)
        {
            if ((this.changeNodeEvent != null) && (!this._onUpdateing))
                changeNodeEvent(this, node);
        }
        public void invalidate()
        {
            this.changeNode(this, this);
        }
    }
}
