using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace Traven.Logic.ViewModel.classes
{
    public class EllipseVM : IForm
    {
        Grid IForm.getStrokeFromNode(ANodeVM node)
        {
            Grid ellipse = new Grid();
            ellipse.Background = new SolidColorBrush(node.getStyle().getBackgroundColor());
            ellipse.Height = node.getRectangle().Height;
            ellipse.Width = node.getRectangle().Width;
            Rectangle rectangle = new Rectangle();
            rectangle.RadiusX = ellipse.Width;
            rectangle.RadiusY = ellipse.Height / 2;
            SolidColorBrush farbe = new SolidColorBrush(node.getStyle().getColor().Item1);

            if (node.getStyle().getColor().Item2)
            {
                rectangle.Fill = farbe;
                rectangle.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            }
            else
            {
                rectangle.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                rectangle.Stroke = farbe;
            }

            rectangle.StrokeThickness = 2;
            ellipse.Children.Add(rectangle);

            if (node.getStyle().getActivated())
                rectangle.StrokeThickness = 4;
            else
                rectangle.StrokeThickness = 2;

            Polygon dreieck = new Polygon();
            PointCollection y = new PointCollection();
            y.Add(new Point(ellipse.Width, ellipse.Height));
            y.Add(new Point(ellipse.Width - 10, ellipse.Height));
            y.Add(new Point(ellipse.Width, ellipse.Height - 10));
            dreieck.Points = y;
            dreieck.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));

            ellipse.Children.Add(dreieck);


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
            ellipse.Children.Add(textPanel);

            return ellipse;
        }

        public override string ToString()
        {
            return "Ellipse";
        }
    }

}
