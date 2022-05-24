using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Traven.Logic.ViewModel.classes
{
    public abstract class AStyleVM : IStyle
    {
        //public Tuple<Color, bool> color => null;

        //public Tuple<Color, bool> Color;

        protected void changeStyle(object sender, IStyle node)
        {
            if ((this.changeStyleEvent != null))
                changeStyleEvent(this, node);
        }

        public abstract Tuple<Color, bool> getColor();
        public abstract void setColor(Tuple<Color, bool> color);

        public delegate void changedStyleEventHandler(object sender, IStyle node);

        public event changedStyleEventHandler changeStyleEvent;

        public abstract void setICon(BitmapImage icon);
        public abstract BitmapImage getIcon();

        public abstract void setActivated(bool active);


        public abstract void setFontsize(int fontSize);

        public abstract double getFontsize();


        public abstract bool getActivated();


        public abstract void setBackgroundColor(Color blub);

        public abstract Color getBackgroundColor();
    }
}
