using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Logic.Model;
using Traven.Repository;
using Traven.UOW;
using Traven.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Traven.Logic.ViewModel.classes
{
    class RechteckVM : IForm
    {
        Grid IForm.getStrokeFromNode(ANodeVM node)
        {
            Grid rechteck = new Grid();

            Rectangle zeichnung = new Rectangle();


            rechteck.Background = new SolidColorBrush(node.getStyle().getBackgroundColor());
            rechteck.Height = node.getRectangle().Height;
            rechteck.Width = node.getRectangle().Width;

            zeichnung.RadiusX = rechteck.Height / 10;
            zeichnung.RadiusY = rechteck.Height / 10;
            SolidColorBrush farbe = new SolidColorBrush(node.getStyle().getColor().Item1);

            if (node.getStyle().getColor().Item2)
            {
                zeichnung.Fill = farbe;
                zeichnung.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
            else
            {
                zeichnung.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                zeichnung.Stroke = farbe;

            }
            if (node.getStyle().getActivated())
                zeichnung.StrokeThickness = 4;
            else
                zeichnung.StrokeThickness = 2;

            rechteck.Children.Add(zeichnung);

            Polygon dreieck = new Polygon();
            PointCollection y = new PointCollection();
            y.Add(new Point(rechteck.Width, rechteck.Height));
            y.Add(new Point(rechteck.Width - 10, rechteck.Height));
            y.Add(new Point(rechteck.Width, rechteck.Height - 10));
            dreieck.Points = y;
            dreieck.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));

            rechteck.Children.Add(dreieck);

            StackPanel textPanel = new StackPanel();
            textPanel.Orientation = Orientation.Horizontal;
            textPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            textPanel.VerticalAlignment = System.Windows.VerticalAlignment.Center;


            if (node.getStyle().getIcon() != null)
            {
                Image bild = new Image();
                bild.Source = node.getStyle().getIcon();

                bild.Width = node.getRectangle().Width / 4;
                bild.Height = node.getRectangle().Width / 4;
                bild.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                bild.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                textPanel.Children.Add(bild);
            }

            TextBlock text = new TextBlock();
            text.FontSize = node.getStyle().getFontsize();
            text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            text.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            text.Text = node.getText();
            textPanel.Children.Add(text);

            rechteck.Children.Add(textPanel);

            return rechteck;
        }

        public override string ToString()
        {
            return "Rechteck";
        }
    }
}
