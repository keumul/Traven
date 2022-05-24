using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;
using System.Windows.Media.Imaging;

namespace Traven.Logic.ViewModel.interfaces
{
    public interface IStyle
    {
        Tuple<Color, bool> getColor();
        void setColor(Tuple<Color, bool> color);

        void setICon(BitmapImage icon);
        BitmapImage getIcon();

        void setBackgroundColor(Color blub);
        Color getBackgroundColor();

        void setFontsize(int fontSize);
        double getFontsize();

        void setActivated(bool active);
        bool getActivated();
    }
}
