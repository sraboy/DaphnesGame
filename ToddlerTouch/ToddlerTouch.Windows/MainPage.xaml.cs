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
        Random rnd;
        //assume 640x384, a phone in landscape
        int width = 640, height = 384;
        int ellipse_base_radius = 40, drop_border = 2, drop_radius;
        int current_ellipse_count = 0; //used to check for level completion
        int level = 1;         //will increase difficulty by increase ellipse_count and adjust sizes/colors
        int dexterity_help = 10; //within 10px of a drop is close enough. can be adjusted for users with less dexterity.

        Ellipse activeEllipse, activeDrop;
        List<Ellipse> Ellipses;
        List<Ellipse> Drops;
        Color last_color;

        public MainPage()
        {
            this.InitializeComponent();

            GameInit();
            GameRun();
        }

        private void GameInit()
        {
            rnd = new Random();
            drop_radius = ellipse_base_radius + (2 * drop_border);

            this.Ellipses = new List<Ellipse>();
            this.Drops = new List<Ellipse>();
        }

        private void GameRun()
        {
            GenerateLevel(level);
        }

        private Color GetRandomColor()
        {
            int a = 255;
            int r = rnd.Next(0, 256);
            int g = rnd.Next(0, 256);
            int b = rnd.Next(0, 256);

            //avoid any grayscale(ish) colors
            //Grayscale is when R, G & B are all the same. I want to avoid
            //gray(ish) colors so +/- 10 should do the trick.
            if (Math.Abs(r - b) < 10 && Math.Abs(r - g) < 10 && Math.Abs(g - b) < 10)
                return GetRandomColor();
                

            Color c = Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
            last_color = c;
            return c;
        }

        private void GenerateLevel(int level)
        {
            int radius_adjustor = 0; //randomize the size of our ellipses

            //level n = n ellipses where n < 7
            //levels 1-7 only increase ellipses
            //starting at level 8, adjust colors
            if (level < 8)
            {
                for (int x = 1; x <= level; x++)
                {
                    SolidColorBrush br = new SolidColorBrush(GetRandomColor());

                    if (rnd.Next(0, 4) < 3) //75% chance of only increasing by 20px
                        radius_adjustor = rnd.Next(20);
                    else
                        radius_adjustor = rnd.Next(30, 40);

                    //generate ellipse
                    Ellipse ellipse = new Ellipse();
                    ellipse.Fill = br;
                    ellipse.Height = ellipse_base_radius + radius_adjustor;
                    ellipse.Width = ellipse_base_radius + radius_adjustor;
                    ellipse.IsTapEnabled = true;
                    ellipse.PointerPressed += Ellipse_PointerPressed;
                    ellipse.PointerMoved += Ellipse_PointerMoved;
                    ellipse.PointerReleased += Ellipse_PointerReleased;
                    this.Ellipses.Add(ellipse);
                    this.canvas.Children.Add(ellipse);
                    Canvas.SetLeft(ellipse, rnd.Next(width - (int)ellipse.Width));
                    Canvas.SetTop(ellipse, rnd.Next(height - (int)ellipse.Height));
                    Canvas.SetZIndex(ellipse, 1); //always keep ellipses on top of their drops


                    //generate drop
                    Ellipse drop = new Ellipse();
                    drop.Fill = new SolidColorBrush(Colors.Transparent);
                    drop.Stroke = br;
                    drop.StrokeThickness = drop_border;
                    drop.Height = drop_radius + radius_adjustor;
                    drop.Width = drop_radius + radius_adjustor;
                    drop.IsTapEnabled = false; //todo: tap provides hint
                    this.Drops.Add(drop);
                    this.canvas.Children.Add(drop);
                    Canvas.SetLeft(drop, rnd.Next(width - (int) drop.Width));
                    Canvas.SetTop(drop, rnd.Next(height - (int) drop.Height));
                    Canvas.SetZIndex(drop, 0); //always keep drops behind their ellipses

                    current_ellipse_count++;
                }
            }
        }

        private void LevelComplete()
        {
            //todo: animation/sound for winning
            level++;
            this.canvas.Children.Clear();
            GameInit();
            GameRun();
        }

        private void CheckDropEllipse()
        {
            double elLeft = Canvas.GetLeft(activeEllipse);
            double elTop = Canvas.GetTop(activeEllipse);
            double drLeft = Canvas.GetLeft(activeDrop);
            double drTop = Canvas.GetTop(activeDrop);

            //auto-grab when "close enough" to the drop
            if (Math.Abs(elLeft - drLeft) <= 5 && Math.Abs(elTop - drTop) <= dexterity_help)
                DropEllipse();
        }
        private void DropEllipse()
        {
            this.activeDrop.Stroke = new SolidColorBrush(Colors.AliceBlue);
            this.activeDrop.Fill = new SolidColorBrush((this.activeEllipse.Fill as SolidColorBrush).Color);
            this.activeDrop.Stroke = new SolidColorBrush((this.activeEllipse.Fill as SolidColorBrush).Color);
            this.canvas.Children.Remove(activeEllipse);
            current_ellipse_count--;

            //todo: check for level completion
            if (current_ellipse_count == 0)
                LevelComplete();
        }

        private void Ellipse_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Ellipse;

            if(activeEllipse != null && activeDrop != null)
            {

                if (e.Pointer.IsInContact //is moving/moveable
                    && el == activeEllipse)
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

            this.activeEllipse = el;
            this.activeDrop = this.Drops.Find(d => (el.Fill as SolidColorBrush).Color == (d.Stroke as SolidColorBrush).Color);
        }
        private void Ellipse_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Ellipse;
            el.ReleasePointerCapture(e.Pointer);
            this.activeEllipse = null;
            this.activeDrop = null;
        }
    }
}
