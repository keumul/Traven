using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Traven.Logic.ViewModel.classes
{
    class NodeVM : ANodeVM
    {
        Rect grundform = new Rect(0, 0, 0, 0);
        ANodeVM parent;
        String text = "";
        IStyle stil;
        IForm shape = new RechteckVM();
        public override Rect getRectangle()
        {
            if (grundform != null)
                return grundform;
            else
                return new Rect(0, 0, 0, 0);
        }
        public override void setRectangle(Rect rectangle)
        {
            rectangle.X = Math.Max(0, rectangle.X);
            rectangle.Y = Math.Max(0, rectangle.Y);
            this.grundform = rectangle;
            changeNode(this, this);
        }
        public override ANodeVM getParent()
        {
            return parent;
        }
        public override void setParent(ANodeVM parent)
        {
            this.parent = parent;
        }
        public override string getText()
        {
            return text + "";
        }
        public override void setText(string text)
        {
            this.text = text;
            changeNode(this, this);
        }
        public override IStyle getStyle()
        {

            return stil;
        }
        public override void setStyle(AStyleVM style)
        {
            this.stil = style;
            base.setStyle(style);
        }
        public override void setForm(IForm form)
        {
            this.shape = form;
            changeNode(this, this);
        }
        public override IForm getForm()
        {
            return shape;
        }
        public override XElement toXML()
        {
            try
            {
                XElement blub = new XElement("node",

                                    new XElement("X", getRectangle().X),
                                    new XElement("Y", getRectangle().Y),
                                    new XElement("width", getRectangle().Width),
                                    new XElement("height", getRectangle().Height),
                                    new XElement("stil", getStyle().ToString()),
                                    new XElement("text", getText()),
                                    new XElement("form", getForm().ToString())
                                    );
                if (this.getStyle() != null)
                {
                    blub.Add(new XElement("style",
                                    new XElement("farbe", (getStyle().getColor() == null ? Colors.Black : getStyle().getColor().Item1)),
                                    new XElement("fill", (getStyle().getColor() == null ? false : getStyle().getColor().Item2)),
                                    new XElement("font", getStyle().getFontsize()),
                                    //new XElement("aktiv", getStyle().getActivated()),
                                    new XElement("icon", (getStyle().getIcon() == null ? null : getStyle().getIcon().BaseUri))
                            ));
                }
                return blub;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }

        public override void fromXML(XElement XML)
        {
            XElement blub = XML;
            double x = (double)blub.Element("X");
            this.setRectangle(new Rect((double)blub.Element("X"), (double)blub.Element("Y"), (double)blub.Element("width"), (double)blub.Element("height")));
            setText(blub.Element("text").Value);
            StyleVM thiss = new StyleVM();
            setStyle(thiss);



            Color fff = (Color)ColorConverter.ConvertFromString(blub.Element("style").Element("farbe").Value);
            bool fill = bool.Parse(blub.Element("style").Element("fill").Value);
            thiss.setColor(Tuple.Create(fff, fill));

            thiss.setFontsize(int.Parse(blub.Element("style").Element("font").Value));
            String form = blub.Element("form").Value;
            switch (form)
            {
                case "Ellipse":
                    setForm(new EllipseVM());
                    break;
                default:
                    setForm(new RechteckVM());
                    break;
            }
        }
    }
}
