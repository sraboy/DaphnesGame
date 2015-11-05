using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Foundation;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;

#region License & Credits
/*
 * Inspiration for this game came from Toddler Touch and Drag:
 * link: https://www.microsoft.com/en-us/store/apps/toddler-touch-and-drag/9wzdncrdkdrh
 * remarks: My 3yr old very quickly adapted to the original game. The levels don't progress
 *          far enough so a couple times through and she had it all figured out. While she
 *          still enjoyed it, I didn't want her just mindlessly moving circles around. This
 *          app is an expansion on the work of Loren Goodman, who does not appear to be
 *          updating the app any longer.
 *
 *
 * Where noted, the following licenses may apply:
 * http://creativecommons.org/licenses/by-sa/3.0/
 * http://creativecommons.org/licenses/by/3.0/
 *
 * Clap Sound:
 * file: 221937__sonsdebarcelona__celebration.wav
 * from: https://freesound.org/people/sonsdebarcelona/sounds/221937/
 * note: only change is filename, which is now clap.wav
 * license: http://creativecommons.org/licenses/by/3.0/
 *          http://creativecommons.org/licenses/by/3.0/legalcode
 *
 * Laser Sound:
 * file: 8-bit Laser.aif
 * adapted from: https://freesound.org/people/timgormly/sounds/170161/
 * note: converted to WAV and changed filename to laser.wav
 * license: http://creativecommons.org/licenses/by/3.0/
 *          http://creativecommons.org/licenses/by/3.0/legalcode
 *
 * Audio Initialization:
 * adapted from: http://stackoverflow.com/questions/10960952/play-two-sounds-simultaneously-in-windows-8-metro-app-c-xaml
 * note: See AudioInit() below.
 * license: http://creativecommons.org/licenses/by-sa/3.0/
 *          http://creativecommons.org/licenses/by-sa/3.0/legalcode
 *
 * KidColors class:
 * adapted from: http://stackoverflow.com/questions/12751008/how-to-enumerate-through-colors-in-winrt
 * license: http://creativecommons.org/licenses/by-sa/3.0/
 *          http://creativecommons.org/licenses/by-sa/3.0/legalcode
*/
#endregion

namespace DaphnesGame
{
    /// <summary>
    /// Main game page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //assume 640x384, a phone in landscape
        private int _gameWidth = 640;
        private int _gameHeight = 384;
        private int _dropShapeBorderWidth = 3;
        private int _currentShapeCount = 0; //used to check for level completion

        private int _currentLevel;
        private int _defaultShapeWidth;
        private int _dropShapeCalculatedWidth;
        private int _dexterityHelper;      //Dragging within X pixels of a drop is "close enough" to match. Can be adjusted for users with less dexterity.
        private MediaElement clap, laser;  //sound effects
        private Random _rnd;
        private Shape _activeDragShape;    //the shape being moved/touched by the user
        private Shape _activeDropShape;    //the matching drop for the active shape
        private List<Shape> _dragShapes;   //all the draggable shapes
        private List<Shape> _dropShapes;   //the non-draggable drop points
        private KidsColorList KidsColors;

        public int GameWidth
        {
            get
            {
                return _gameWidth;
            }

            set
            {
                _gameWidth = value;
            }
        }
        public int GameHeight
        {
            get
            {
                return _gameHeight;
            }

            set
            {
                _gameHeight = value;
            }
        }
        public int CurrentLevel
        {
            get
            {
                return _currentLevel;
            }

            set
            {
                _currentLevel = value;
            }
        }
        public List<Shape> DragShapes
        {
            get
            {
                return _dragShapes;
            }

            set
            {
                _dragShapes = value;
            }
        }
        public List<Shape> DropShapes
        {
            get
            {
                return _dropShapes;
            }

            set
            {
                _dropShapes = value;
            }
        }
        public Shape ActiveDragShape
        {
            get
            {
                return _activeDragShape;
            }

            set
            {
                _activeDragShape = value;
            }
        }
        public Shape ActiveDropShape
        {
            get
            {
                return _activeDropShape;
            }

            set
            {
                _activeDropShape = value;
            }
        }
        public int CurrentShapeCount
        {
            get
            {
                return _currentShapeCount;
            }

            set
            {
                _currentShapeCount = value;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            AdvertisingInit();
            AudioInit();

            GameInit();
            GameStart(1); //starts off with CurrentLevel = 1
        }
        private void AdvertisingInit()
        {
            ////set up AdMediator: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/mt219682.aspx
            ////test it: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/mt219690.aspx
            ////test values: https://msdn.microsoft.com/en-us/library/advertising-windows-test-mode-values(v=msads.10).aspx

            ////Canvas test = new Canvas();
            ////adViewBox = new Viewbox();
            ////adCover = new TextBox();
            ////AdMediator_8D2C93 = new AdMediatorControl();

            ////test.Children.Add(adCover);
            ////test.Children.Add(AdMediator_8D2C93);

            ////canvas.Children.Add(adViewBox);
            ////canvas.Children.Add(AdMediator_8D2C93);
            ////canvas.Children.Add(adCover);

            //Canvas.SetLeft(AdMediator_8D2C93, 50);
            //Canvas.SetTop(AdMediator_8D2C93, 50);

            //adViewBox.Width = 300;
            //adViewBox.Height = 50;
            //adViewBox.IsTapEnabled = false;
            //Canvas.SetLeft(adViewBox, (GameWidth / 2) - (adViewBox.Width / 2));
            //Canvas.SetTop(adViewBox, 0);
            //Canvas.SetZIndex(adViewBox, -1);
            ////adViewBox.Child = test;

            //adCover.Width = 300;
            //adCover.Height = 50;
            //adCover.IsTapEnabled = false;
            //adCover.IsReadOnly = true;
            //adCover.Background = new SolidColorBrush(Colors.Transparent);
            //adCover.BorderThickness = new Windows.UI.Xaml.Thickness(2);
            //Canvas.SetLeft(adCover, Canvas.GetLeft(adViewBox));
            //Canvas.SetTop(adCover, Canvas.GetTop(adCover));
            //Canvas.SetZIndex(adCover, 0);

            //AdMediator_8D2C93.AdSdkError += AdMediator_8D2C93_AdSdkError;
            //AdMediator_8D2C93.AdMediatorFilled += AdMediator_8D2C93_AdMediatorFilled;
            //AdMediator_8D2C93.AdMediatorError += AdMediator_8D2C93_AdMediatorError;
            //AdMediator_8D2C93.AdSdkEvent += AdMediator_8D2C93_AdSdkEvent;
            //AdMediator_8D2C93.Width = 728;
            //AdMediator_8D2C93.Height = 90;
            //AdMediator_8D2C93.IsTapEnabled = false;
            //AdMediator_8D2C93.IsDoubleTapEnabled = false;
            //AdMediator_8D2C93.IsRightTapEnabled = false;
        }
        private void GameInit()
        {
            KidsColors = new KidsColorList();

            _dexterityHelper = (int) (GameWidth * .03);          //3% of default is 18px, works for ~3y/o
            _defaultShapeWidth = (int) (GameWidth * .1);         //10% of the screen's width
            _dropShapeCalculatedWidth = _defaultShapeWidth + (2 * _dropShapeBorderWidth); //make it big enough to fit the shape
            DragShapes = new List<Shape>();
            DropShapes = new List<Shape>();
            canvas.IsDoubleTapEnabled = true;
            canvas.DoubleTapped += Canvas_DoubleTapped;
        }
        private void GameStart(int start_level = 0)
        {
            if (start_level != 0)
                CurrentLevel = start_level;

            GenerateLevel();
        }
        private void Canvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
#if DEBUG
            List<Shape> combined = new List<Shape>();
            combined.AddRange(DropShapes);
            combined.AddRange(DragShapes);
            foreach (Shape sh in combined)
            {
                //sh.IsDoubleTapEnabled = true;
                //sh.DoubleTapped += Shape_DoubleTapped;
                TextBlock stats = new TextBlock();
                stats.Text = "Left: " + Canvas.GetLeft(sh).ToString() + "\nTop: " + Canvas.GetTop(sh).ToString();
                stats.FontSize = 10;
                canvas.Children.Add(stats);
                Canvas.SetLeft(stats, Canvas.GetLeft(sh));
                Canvas.SetTop(stats, Canvas.GetTop(sh));
                Canvas.SetZIndex(stats, 100);
            }
#endif
        }

        #region Game Setup Helpers
        //private Color GetRandomColor()
        //{
        //    return AllColors[_rnd.Next(AllColors.Count - 1)];


        //    /********************************************************
        //    partial solution to generating sufficiently-different colors
        //    ********************************************************/
        //    //int r = 0, g = 0, b = 0;
        //    //int rgb = rnd.Next(0, 9);
        //    //Color clr;
        //    //rgb %= 3;
        //    //switch (rgb)
        //    //{
        //    //    case 0:
        //    //        clr = Colors.Red;
        //    //        clr.G += (byte)rnd.Next(0, 20);
        //    //        clr.B += (byte) rnd.Next(0, 20);
        //    //        //r = rnd.Next(100, 256);
        //    //        //g = rnd.Next(0, r - 50);
        //    //        //b = rnd.Next(0, r - 50);
        //    //        break;
        //    //    case 1:
        //    //        clr = Colors.Blue;
        //    //        clr.G += (byte) rnd.Next(0, 20);
        //    //        clr.R += (byte) rnd.Next(0, 20);
        //    //        //g = rnd.Next(100, 256);
        //    //        //r = rnd.Next(0, g - 50);
        //    //        //b = rnd.Next(0, g - 50);
        //    //        break;
        //    //    case 2:
        //    //        clr = Colors.Green;
        //    //        clr.R += (byte) rnd.Next(0, 20);
        //    //        clr.B += (byte) rnd.Next(0, 20);
        //    //        //b = rnd.Next(100, 256);
        //    //        //g = rnd.Next(0, b - 50);
        //    //        //r = rnd.Next(0, b - 50);
        //    //        break;
        //    //}

        //    /********************************************************
        //    CTRL-K,U below for (working) dynamic generation
        //    ********************************************************/
        //    //int a = 255;
        //    //int r = _rnd.Next(0, 256);
        //    //int g = _rnd.Next(0, 256);
        //    //int b = _rnd.Next(0, 256);

        //    ////avoids any grayscale(ish) colors (+/- 10 across rgb)
        //    ////and bland earthy colors +/-50 across rgb
        //    ////just bright colors +/- 75
        //    //if (Math.Abs(r - b) < 75 && Math.Abs(r - g) < 75 && Math.Abs(g - b) < 75)
        //    //    return GetRandomColor();

        //    //Color c = Color.FromArgb((byte) a, (byte) r, (byte) g, (byte) b);
        //    //return c;
        //}

        /// <summary>
        /// Returns a List of Shapes with matching colors and a sized drop.
        /// </summary>
        /// <typeparam name="T">The specific Shape type (Ellipse, Polygon, Rectangle, etc)</typeparam>
        /// <param name="staticWidthAdjustment">If set, will add this number to the default shape width rather than randomize shape sizes.</param>
        /// <returns></returns>
        private List<Shape> BuildDragDropShapePair<T>(int? staticWidthAdjustment = null) where T : Shape, new()
        {
            SolidColorBrush br = new SolidColorBrush(KidsColors.GetRandomColor());

            if (staticWidthAdjustment == null) //get a randomized shape width
            {
                if (_rnd.Next(0, 4) < 3) //75% chance of only increasing by 20px
                    staticWidthAdjustment = _rnd.Next(20);
                else
                    staticWidthAdjustment = _rnd.Next(30, 40);
            }
            //else all are the same size: _minimum_shape_width + width_adjustor

            //generate shape
            T shape = new T();
            shape.Fill = br;
            shape.Height = _defaultShapeWidth + (double) staticWidthAdjustment;
            shape.Width = _defaultShapeWidth + (double) staticWidthAdjustment;
            shape.IsTapEnabled = true;
            shape.PointerPressed += Shape_PointerPressed;
            shape.PointerMoved += Shape_PointerMoved;
            shape.PointerReleased += Shape_PointerReleased;

            //generate drop
            T drop = new T();
            drop.Fill = new SolidColorBrush(Colors.Transparent);
            drop.Stroke = br;
            drop.StrokeThickness = _dropShapeBorderWidth;
            drop.Height = _dropShapeCalculatedWidth + (double) staticWidthAdjustment;
            drop.Width = _dropShapeCalculatedWidth + (double) staticWidthAdjustment;
            drop.IsTapEnabled = false;

            return new List<Shape> { shape, drop };
        }
        private void GenerateLevel()
        {
            _rnd = new Random();

            switch (CurrentLevel)
            {
                case 1:
                    LevelOne();
                    break;
                case 2:
                    LevelTwo();
                    break;
                case 3:
                    LevelThree();
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                default:
                    GameStart(1); //restarts
                    break;
            }

            // Always keep shape on top of their drops. Drops have a default ZIndex of 0.
            foreach (Shape sh in DragShapes)
                Canvas.SetZIndex(sh, 1);
        }
        private List<Shape> LevelOne()
        {
            List<Shape> newShapes;
            newShapes = BuildDragDropShapePair<Ellipse>();

            foreach (Shape sh in newShapes)
            {
                if (sh.IsTapEnabled == true) //shape, not drop
                {
                    DragShapes.Add(sh);
                    canvas.Children.Add(sh);

                    Canvas.SetLeft(sh, _rnd.Next((GameWidth / 2) - (int) sh.Width)); //keeps it on the left half of the screen
                    Canvas.SetTop(sh, _rnd.Next(GameHeight - (int) sh.Height));
                    CurrentShapeCount++;
                }
                else
                {
                    DropShapes.Add(sh);
                    canvas.Children.Add(sh);

                    Canvas.SetLeft(sh, _rnd.Next(GameWidth / 2, GameWidth - (int) sh.Width)); //keeps it on the bottom half of the screen
                    Canvas.SetTop(sh, _rnd.Next(GameHeight - (int) sh.Height));
                }

            }

            return newShapes;
        }
        private List<Shape> LevelTwo()
        {
            List<Shape> newShapes;
            newShapes = BuildDragDropShapePair<Ellipse>();

            foreach (Shape el in newShapes)
            {
                if (el.IsTapEnabled == true) //shape, not drop
                {
                    DragShapes.Add(el);
                    canvas.Children.Add(el);

                    do
                    {
                        Canvas.SetLeft(el, _rnd.Next((GameWidth / 3) * 2, GameWidth - (int) el.Width)); //far right third of screen
                        Canvas.SetTop(el, _rnd.Next(0, (GameHeight / 2) - (int) el.Height)); //keeps it on the bottom half of the screen
                    } while (CheckForOverlaps(el));

                    CurrentShapeCount++;
                }
                else
                {
                    DropShapes.Add(el);
                    canvas.Children.Add(el);

                    do
                    {
                        Canvas.SetLeft(el, _rnd.Next(0, (GameWidth / 3) - (int) el.Width)); //far left third of screen
                        Canvas.SetTop(el, _rnd.Next(GameHeight / 2, GameHeight - (int) el.Height)); //keeps it on the top half of the screen
                    } while (CheckForOverlaps(el));
                }

            }

            newShapes.AddRange(LevelOne()); //another simple left/right ellipse pair

            return newShapes;
        }
        private List<Shape> LevelThree()
        {
            List<Shape> newShapes;
            newShapes = BuildDragDropShapePair<Rectangle>();

            foreach (Rectangle polyShape in newShapes)
            {
                //polyShape.Points.Add(new Point(0, 100)); //bottomLeft);
                //polyShape.Points.Add(new Point(20, 100)); //bottomRight);
                //polyShape.Points.Add(new Point(30, 3)); //top);
                //polyShape.Height = 100;
                //polyShape.Width = 100;

                canvas.Children.Add(polyShape);

                Canvas.SetLeft(polyShape, _rnd.Next(0, GameWidth - (int) polyShape.Width));
                Canvas.SetTop(polyShape, _rnd.Next(0, GameHeight - (int) polyShape.Height));

                if (polyShape.IsTapEnabled == true) //shape, not drop
                {
                    DragShapes.Add(polyShape);
                    CurrentShapeCount++;
                }
                else
                    DropShapes.Add(polyShape);

            }

            newShapes.AddRange(LevelOne());
            newShapes.AddRange(LevelTwo());

            //todo: tapping on shapes will grow/shrink its drop to help show the difference

            return newShapes;
        }
        #endregion

        #region Game Play / UI
        /// <summary>
        /// Clean up the game and progress to the next level.
        /// </summary>
        private void LevelComplete()
        {
            //todo: animation for winning
            PlayClap();

            List<Shape> shapes = canvas.Children.OfType<Shape>().ToList();
            shapes.AddRange(canvas.Children.OfType<Ellipse>().ToList());
            shapes.AddRange(canvas.Children.OfType<Polygon>().ToList());
            shapes.AddRange(canvas.Children.OfType<Rectangle>().ToList());

            foreach (Object ob in shapes)
                canvas.Children.Remove(ob as UIElement);

            DragShapes.Clear();
            DropShapes.Clear();

            GameStart(++CurrentLevel);
        }
        /// <summary>
        /// Check to see if an shape overlaps with any other shapes/drops.
        /// </summary>
        /// <param name="newShapeToCheck">The shape to check against the others</param>
        /// <returns></returns>
        private bool CheckForOverlaps(Shape newShapeToCheck)
        {
            List<Shape> combined = new List<Shape>();
            combined.AddRange(DragShapes);
            combined.AddRange(DropShapes);

            foreach (Shape oldShape in combined)
            {
                if (oldShape != newShapeToCheck)
                {
                    double oldShapeLeft = Canvas.GetLeft(oldShape);
                    double oldShapeRight = oldShapeLeft + oldShape.Width;
                    double oldShapeTop = Canvas.GetTop(oldShape);
                    double oldShapeBottom = oldShapeTop + oldShape.Height;

                    double newShapeToCheckLeft = Canvas.GetLeft(newShapeToCheck);
                    double newShapeToCheckRight = newShapeToCheckLeft + newShapeToCheck.Width;
                    double newShapeToCheckTop = Canvas.GetTop(newShapeToCheck);
                    double newShapeToCheckBottom = newShapeToCheckTop + newShapeToCheck.Height;

                    //Point oldEllipseCenter = new Point(Canvas.GetLeft(oldShape) + (oldShape.Width / 2), Canvas.GetTop(oldShape) + (oldShape.Height / 2));
                    //Point newEllipseToCheckCenter = new Point(Canvas.GetLeft(newShapeToCheck) + (newShapeToCheck.Width / 2), Canvas.GetTop(newShapeToCheck) + (newShapeToCheck.Height / 2));

                    bool noOverlapXaxis = newShapeToCheckRight < oldShapeLeft || //too far left
                                          newShapeToCheckLeft > oldShapeRight;   //too far right

                    // ACHTUNG! (0,0) is in top-left so (0,20) is higher than (0,30)
                    bool noOverlapYaxis = newShapeToCheckBottom < oldShapeTop || //too high
                                          newShapeToCheckTop > oldShapeBottom;   //too low

                    return !(noOverlapXaxis || noOverlapYaxis);
                }
            }

            //We only get here if oldEllipse and newEllipse are the same and there's only 
            //one ellipse in total, before its matching drop was created
            return false;
        }
        /// <summary>
        /// Check if the active ellipse is over its matching drop. Call DropEllipse if it is.
        /// </summary>
        /// <remarks>
        /// It will automatically grab and drop the ellipse if it's "close enough", as defined by dexterity_help.
        /// </remarks>
        private void CheckForMatchingDrop()
        {
            double elLeft = Canvas.GetLeft(ActiveDragShape);
            double elTop = Canvas.GetTop(ActiveDragShape);
            double drLeft = Canvas.GetLeft(ActiveDropShape);
            double drTop = Canvas.GetTop(ActiveDropShape);

            //auto-grab when "close enough" to the drop
            if (Math.Abs(elLeft - drLeft) <= 5 && Math.Abs(elTop - drTop) <= _dexterityHelper)
                ForceShapeToDrop();
        }
        /// <summary>
        /// Drops the active ellipse in its 'drop container' and updates the UI appropriately.
        /// </summary>
        private void ForceShapeToDrop()
        {
#if DEBUG
            Shape sh = ActiveDropShape;
            var left = Canvas.GetLeft(sh);
            var right = left + sh.Width;
            var top = Canvas.GetTop(sh);
            var bottom = top + sh.Height;
#endif

            PlayLaser();

            //put a ring around the shape so the completed drop stands out
            ActiveDragShape.Stroke = new SolidColorBrush(Colors.AntiqueWhite);
            ActiveDragShape.StrokeThickness = _dropShapeBorderWidth;

            //disable the shape
            ActiveDragShape.ReleasePointerCaptures();
            ActiveDragShape.IsTapEnabled = false;
            ActiveDragShape.PointerPressed -= Shape_PointerPressed;
            ActiveDragShape.PointerMoved -= Shape_PointerMoved;
            ActiveDragShape.PointerReleased -= Shape_PointerReleased;

            //center it in the drop
            Canvas.SetLeft(ActiveDragShape, Canvas.GetLeft(ActiveDropShape) + _dropShapeBorderWidth);
            Canvas.SetTop(ActiveDragShape, Canvas.GetTop(ActiveDropShape) + _dropShapeBorderWidth);

            CurrentShapeCount--;

            if (CurrentShapeCount == 0)
                LevelComplete();
        }
        #endregion
        #region Async Audio Methods
        private async void AudioInit()
        {
            var package = Windows.ApplicationModel.Package.Current;
            var installedLocation = package.InstalledLocation;
            var storageFile = await installedLocation.GetFileAsync("Assets\\clap.wav");

            if (storageFile != null)
            {
                var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                clap = new MediaElement();
                clap.AutoPlay = false;
                clap.SetSource(stream, storageFile.ContentType);
                clap.AudioCategory = AudioCategory.BackgroundCapableMedia;
                storageFile = null;
            }

            storageFile = await installedLocation.GetFileAsync("Assets\\laser.wav");
            if (storageFile != null)
            {
                var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                laser = new MediaElement();
                laser.AutoPlay = false;
                laser.SetSource(stream, storageFile.ContentType);
                laser.AudioCategory = AudioCategory.BackgroundCapableMedia;
            }
        }
        private async void PlayClap()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                clap.Play();
            });
        }
        private async void PlayLaser()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                laser.Play();
            });
        }
        #endregion
        #region Events
        private void Shape_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var sh = sender as Shape;
#if DEBUG
            var left = Canvas.GetLeft(sh);
            var right = left + sh.Width;
            var top = Canvas.GetTop(sh);
            var bottom = top + sh.Height;
#endif
            if (ActiveDragShape != null && ActiveDropShape != null)
            {

                if (e.Pointer.IsInContact //is moving/moveable
                    && sh == ActiveDragShape)
                {
                    PointerPoint pointer_pos_in_canvas = e.GetCurrentPoint(sh.Parent as Canvas);

                    Canvas.SetLeft(sh, pointer_pos_in_canvas.Position.X - (sh.Width / 2));
                    Canvas.SetTop(sh, pointer_pos_in_canvas.Position.Y - (sh.Height / 2));

                    CheckForMatchingDrop();
                }
            }
        }
        private void Shape_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Shape;
            el.CapturePointer(e.Pointer);

            ActiveDragShape = el;
            ActiveDropShape = DropShapes.Find(d => (el.Fill as SolidColorBrush).Color == (d.Stroke as SolidColorBrush).Color);
        }
        private void Shape_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Shape;
            el.ReleasePointerCapture(e.Pointer);
            ActiveDragShape = null;
            ActiveDropShape = null;
        }
        #endregion
    }
}