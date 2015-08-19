using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DaphnesGame
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //assume 640x384, a phone in landscape

        int ellipse_radius = 40, drop_border = 2, drop_radius;
        int ellipse_count = 0; //will scale up based on level
        int level = 1;         //will increase difficulty by increase ellipse_count and adjust sizes/colors
        //int max_level = 50;    //todo: figure out leveling
        bool moving = false;   //used to check whether or not an ellipse is being moved
                               //todo: custom Ellipse object since multi-touch could cause this to become false erroneously

        Ellipse activeEllipse, activeDrop;

        List<Ellipse> Ellipses;
        List<Ellipse> Drops;

        private void GenerateLevel(int level)
        {
            //level n = n ellipses where n < 7
            //levels 1-7 only increase ellipses
            //starting at level 8, adjust colors

            if (level < 8)
            {
                //GenerateColorArray() for SolidColorBrush[ColorArray[x]]
                Color c = Colors.Red;  //debug

                for (int x = 1; x <= level; x++)
                {
                    Ellipse ellipse = new Ellipse();

                    ellipse.Fill = new SolidColorBrush(c);
                    ellipse.Height = ellipse_radius;
                    ellipse.Width = ellipse_radius;
                    ellipse.IsTapEnabled = true;

                    ellipse.PointerPressed += Ellipse_PointerPressed;
                    ellipse.PointerMoved += Ellipse_PointerMoved;
                    ellipse.PointerReleased += Ellipse_PointerReleased;

                    this.Ellipses.Add(ellipse);
                    this.canvas.Children.Add(ellipse);
                    Canvas.SetLeft(ellipse, canvas.Width / 10);
                    Canvas.SetTop(ellipse, canvas.Height / 10);
                    Canvas.SetZIndex(ellipse, 1); //always keep ellipses on top of their drops


                    Ellipse drop = new Ellipse();
                    drop.Fill = new SolidColorBrush(Colors.Transparent);

                    drop.Stroke = new SolidColorBrush(c);
                    drop.StrokeThickness = drop_border;
                    drop.Height = drop_radius;
                    drop.Width = drop_radius;
                    drop.IsTapEnabled = false; //todo: tap provides hint

                    this.Drops.Add(drop);
                    this.canvas.Children.Add(drop);
                    Canvas.SetLeft(drop, canvas.Width * .33);
                    Canvas.SetTop(drop, canvas.Height * .33);
                    Canvas.SetZIndex(drop, 0); //always keep drops behind their ellipses
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            GameInit();
            GameRun();
        }

        private void GameInit()
        {
            drop_radius = ellipse_radius + (2 * drop_border);

            this.Ellipses = new List<Ellipse>();
            this.Drops = new List<Ellipse>();
        }

        private void GameRun()
        {
            GenerateLevel(level);

        }

        private void CheckDropEllipse()
        {
            double elLeft = Canvas.GetLeft(activeEllipse);
            double elTop = Canvas.GetTop(activeEllipse);
            double drLeft = Canvas.GetLeft(activeDrop);
            double drTop = Canvas.GetTop(activeDrop);

            //auto-grab when "close enough" to the drop
            if (Math.Abs(elLeft - drLeft) <= 5 && Math.Abs(elTop - drTop) <= 5)
                DropEllipse();
        }

        private void DropEllipse()
        {
            this.activeDrop.Stroke = new SolidColorBrush(Colors.AliceBlue);
            this.activeDrop.Fill = new SolidColorBrush((this.activeEllipse.Fill as SolidColorBrush).Color);
            this.activeDrop.Stroke = new SolidColorBrush((this.activeEllipse.Fill as SolidColorBrush).Color);
            this.canvas.Children.Remove(activeEllipse);
            ellipse_count--;
        }

        private void Ellipse_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Ellipse;

            if(activeEllipse != null && activeDrop != null)
            {

                //if (moving && el == activeEllipse)
                if (e.Pointer.IsInContact && el == activeEllipse)
                {      
                    PointerPoint pointer_pos_in_canvas = e.GetCurrentPoint(el.Parent as Canvas);

                    Canvas.SetLeft(el, pointer_pos_in_canvas.Position.X - (el.Width / 2));
                    Canvas.SetTop(el, pointer_pos_in_canvas.Position.Y - (el.Height / 2));

                    CheckDropEllipse();
                }
            }
        }

        private void Ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Ellipse;
            el.CapturePointer(e.Pointer);
            //moving = true;

            this.activeEllipse = el;
            this.activeDrop = this.Drops.Find(d => (el.Fill as SolidColorBrush).Color == (d.Stroke as SolidColorBrush).Color);
        }

        private void Ellipse_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Ellipse;
            el.ReleasePointerCapture(e.Pointer);
            //moving = false;
            this.activeEllipse = null;
            this.activeDrop = null;
        }
    }
}
