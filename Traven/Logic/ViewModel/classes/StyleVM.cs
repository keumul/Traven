using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;

namespace Traven.Logic.ViewModel.classes
{
    public class StyleVM : AStyleVM
    {
        Tuple<Color, bool> farbeFill;
        BitmapImage icon;
        Color BackgroundColor = Color.FromArgb(0, 255, 255, 255);
        bool isActive = false;
        int fontSize = 12;

        public override Tuple<Color, bool> getColor()
        {
            return farbeFill;
        }

        public override void setColor(Tuple<Color, bool> color)
        {
            if (this.farbeFill != color)
            {
                this.farbeFill = color;
                changeStyle(this, this);
            }
        }

        public override BitmapImage getIcon()
        {
            return icon;
        }

        public override void setICon(BitmapImage icon)
        {
            this.icon = icon;
        }

        public override void setActivated(bool active)
        {
            if (this.isActive != active)
            {
                this.isActive = active;
                changeStyle(this, this);
            }
        }

        public override void setFontsize(int fontSize)
        {
            if (this.fontSize != fontSize)
            {
                this.fontSize = fontSize;
                changeStyle(this, this);
            }
        }

        public override double getFontsize()
        {
            return fontSize;
        }

        public override bool getActivated()
        {
            return this.isActive;
        }

        public override void setBackgroundColor(Color blub)
        {
            this.BackgroundColor = blub;
        }

        public override Color getBackgroundColor()
        {
            return this.BackgroundColor;
        }
    }
}
