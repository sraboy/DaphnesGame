using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
//using Microsoft.Advertising.WinRT.UI;
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
        //todo: could speed up overlaps and implement multitouch by creating our own shape objects
            // overlaps: each shape would have a list of shapes it's already checked itself against
            // multitouch: instead of having single activedrop/activedrag, make the object active

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
        private enum Locations
        {
            LeftRight = 0,
            LeftRightThirds = 1,
            Random = 2
        }

        private Dictionary<Shape, Shape> DragDropShapes;

        public MainPage()
        {
            InitializeComponent();
            //AdvertisingInit();
            AudioInit();

            GameInit();  
            GameStart(1); //starts off with CurrentLevel = 1

        }
  
        private void GameInit()
        {
            KidsColors = new KidsColorList();

            _dexterityHelper = (int) (GameWidth * .03);          //3% of default is 18px, works for ~3y/o
            _defaultShapeWidth = (int) (GameWidth * .1);         //10% of the screen's width
            _dropShapeCalculatedWidth = _defaultShapeWidth + (2 * _dropShapeBorderWidth); //make it big enough to fit the shape

            canvas.IsDoubleTapEnabled = true;
            canvas.DoubleTapped += Canvas_DoubleTapped;

            DragDropShapes = new Dictionary<Shape, Shape>();
        }
        private void GameStart(int start_level = 0)
        {
            if (start_level != 0)
                CurrentLevel = start_level;

            GenerateLevel();
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

        ///// <summary>
        ///// Returns a List of Shapes with matching colors and a sized drop.
        ///// </summary>
        ///// <typeparam name="T">The specific Shape type (Ellipse, Polygon, Rectangle, etc)</typeparam>
        ///// <param name="staticWidthAdjustment">If set, will add this number to the default shape width rather than randomize shape sizes.</param>
        ///// <param name="colors">Custom list of colors from which to choose instead of using the built-in "kid-friendly" list.</param>
        ///// /// <returns>The newly-created list of shapes</returns>
        //private List<Shape> BuildDragDropShapePair<T>(int? staticWidthAdjustment = null, List<Color> colors = null) where T : Shape, new()
        //{
        //    SolidColorBrush br;
        //    if (colors == null)
        //        br = new SolidColorBrush(KidsColors.GetRandomColor());
        //    else
        //        br = new SolidColorBrush(colors[_rnd.Next(0, colors.Count)]);

        //    if(staticWidthAdjustment == null) //get a randomized shape width
        //    { 
        //        if (_rnd.Next(0, 10) < 9) //90% chance of only increasing up to 20px
        //            staticWidthAdjustment = _rnd.Next(6) * 4; //0, 4, 8, 12, 16, 20 -- want a bit more variation
        //        else
        //            staticWidthAdjustment = _rnd.Next(6, 11) * 4; //10% chance of increase 20-40 pixels
        //    }
        //    //else all are the same size: _minimum_shape_width + width_adjustor

        //    //generate shape
        //    T shape = new T();
        //    shape.Fill = br;
        //    shape.Height = _defaultShapeWidth + (double) staticWidthAdjustment;
        //    shape.Width = _defaultShapeWidth + (double) staticWidthAdjustment;
        //    shape.IsTapEnabled = true;
        //    shape.PointerPressed += Shape_PointerPressed;
        //    shape.PointerMoved += Shape_PointerMoved;
        //    shape.PointerReleased += Shape_PointerReleased;
            
        //    //generate drop
        //    T drop = new T();
        //    drop.Fill = new SolidColorBrush(Colors.Transparent);
        //    drop.Stroke = br;
        //    drop.StrokeThickness = _dropShapeBorderWidth;
        //    drop.Height = _dropShapeCalculatedWidth + (double) staticWidthAdjustment;
        //    drop.Width = _dropShapeCalculatedWidth + (double) staticWidthAdjustment;
        //    drop.IsTapEnabled = false; 

        //    return new List<Shape> { shape, drop };
        //}
        private void GenerateLevel()
        {
            _rnd = new Random();

            switch (CurrentLevel)
            {
                case 1:
                    SetShapesOnCanvas<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>());
                    break;
                case 2:
                    SetShapesOnCanvas<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>());
                    SetShapesOnCanvas<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>());
                    break;
                case 3:
                    SetShapesOnCanvas<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>());
                    SetShapesOnCanvas<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>());
                    SetShapesOnCanvas<Rectangle>(Locations.LeftRight, BuildDragDropShapePair<Rectangle>());
                    break;
                case 4:
                    Color colorOne = KidsColors[_rnd.Next(KidsColors.Count)];
                    Color colorTwo = KidsColors[_rnd.Next(KidsColors.Count)];
                    SetShapesOnCanvas<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>(null, new List<Color> { colorOne, colorTwo }));
                    SetShapesOnCanvas<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>(null, new List<Color> { colorOne, colorTwo }));
                    SetShapesOnCanvas<Rectangle>(Locations.LeftRight, BuildDragDropShapePair<Rectangle>(null, new List<Color> { colorOne, colorTwo }));
                    SetShapesOnCanvas<Rectangle>(Locations.LeftRight, BuildDragDropShapePair<Rectangle>(null, new List<Color> { colorOne, colorTwo }));
                    break;
                case 5:
                case 6:
                case 7:
                default:
                    GameStart(1); //restarts
                    break;
            }
        }

        /// <summary>
        /// Returns a List of Shapes with matching colors and a sized drop.
        /// </summary>
        /// <typeparam name="T">The specific Shape type (Ellipse, Polygon, Rectangle, etc)</typeparam>
        /// <param name="staticWidthAdjustment">If set, will add this number to the default shape width rather than randomize shape sizes.</param>
        /// <param name="colors">Custom list of colors from which to choose instead of using the built-in "kid-friendly" list.</param>
        /// /// <returns>The newly-created list of shapes</returns>
        //public List<Shape> BuildDragDropShapePair<T>(int? staticWidthAdjustment = null, List<Color> colors = null) where T : Shape, new()
        //{
        //    SolidColorBrush br;
        //    if (colors == null)
        //        br = new SolidColorBrush(KidsColors.GetRandomColor());
        //    else
        //        br = new SolidColorBrush(colors[_rnd.Next(0, colors.Count)]);

        //    if (staticWidthAdjustment == null) //get a randomized shape width
        //    {
        //        if (_rnd.Next(0, 10) < 9) //90% chance of only increasing up to 20px
        //            staticWidthAdjustment = _rnd.Next(6) * 4; //0, 4, 8, 12, 16, 20 -- want a bit more variation
        //        else
        //            staticWidthAdjustment = _rnd.Next(6, 11) * 4; //10% chance of increase 20-40 pixels
        //    }
        //    //else all are the same size: _minimum_shape_width + width_adjustor

        //    //generate shape
        //    T shape = new T();
        //    shape.Fill = br;
        //    shape.Height = _defaultShapeWidth + (double) staticWidthAdjustment;
        //    shape.Width = _defaultShapeWidth + (double) staticWidthAdjustment;
        //    shape.IsTapEnabled = true;
        //    shape.PointerPressed += Shape_PointerPressed;
        //    shape.PointerMoved += Shape_PointerMoved;
        //    shape.PointerReleased += Shape_PointerReleased;

        //    //generate drop
        //    T drop = new T();
        //    drop.Fill = new SolidColorBrush(Colors.Transparent);
        //    drop.Stroke = br;
        //    drop.StrokeThickness = _dropShapeBorderWidth;
        //    drop.Height = _dropShapeCalculatedWidth + (double) staticWidthAdjustment;
        //    drop.Width = _dropShapeCalculatedWidth + (double) staticWidthAdjustment;
        //    drop.IsTapEnabled = false;

        //    return new List<Shape> { shape, drop };
        //}
        public KeyValuePair<T, T> BuildDragDropShapePair<T>(int? staticWidthAdjustment = null, List<Color> colors = null) where T : Shape, new()
        {
            SolidColorBrush br;
            if (colors == null)
                br = new SolidColorBrush(KidsColors.GetRandomColor());
            else
                br = new SolidColorBrush(colors[_rnd.Next(0, colors.Count)]);

            //making them too large results in too many overlaps and causes the app to hang
            //because CheckIfOverlap() keeps returning true. Reduced to multiple of 3 below, 
            //instead of 4 to reduce overlaps.
            if (staticWidthAdjustment == null) //get a randomized shape width
            {
                if (_rnd.Next(0, 10) < 9) //90% chance of only increasing up to 15px
                    staticWidthAdjustment = _rnd.Next(6) * 3; //0, 3, 6, 9, 12, 15 -- don't want a bunch of +/- 1px differences
                else
                    staticWidthAdjustment = _rnd.Next(6, 11) * 4; //10% chance of increase 20-40 pixels
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
            
            return new KeyValuePair<T, T>(shape, drop);
        }
        //private void SetShapesOnCanvas<T>(Locations loc, List<Shape> newShapes) where T : Shape, new()
        //{
        //    double dragLeft = 0, 
        //           dragTop = 0, 
        //           dropLeft = 0, 
        //           dropTop = 0;

        //    //Shape test;
        //    //foreach(Object ob in newShapes)
        //    //{
        //    //    if (ob.GetType() == typeof(Rectangle))
        //    //    {
        //    //        test = ob as Shape;
        //    //        dragLeft = _rnd.Next((GameWidth / 2) - (int) test.Width); //keeps it on the left half of the screen
        //    //        dragTop = _rnd.Next(GameHeight - (int) test.Height);
        //    //        dropLeft = _rnd.Next(GameWidth / 2, GameWidth - (int) test.Width); //keeps it on the right half of the screen
        //    //        dropTop = _rnd.Next(GameHeight - (int) test.Height);
        //    //    }
        //    //}

        //    foreach (T sh in newShapes)
        //    {
        //        do
        //        {
        //            switch (loc)
        //            {
        //                case Locations.LeftRight:
        //                    dragLeft = _rnd.Next((GameWidth / 2) - (int) sh.Width); //keeps it on the left half of the screen
        //                    dragTop = _rnd.Next(GameHeight - (int) sh.Height);
        //                    dropLeft = _rnd.Next(GameWidth / 2, GameWidth - (int) sh.Width); //keeps it on the right half of the screen
        //                    dropTop = _rnd.Next(GameHeight - (int) sh.Height);
        //                    break;
        //                case Locations.LeftRightThirds:
        //                    dragLeft = _rnd.Next(0, (GameWidth / 3) - (int) sh.Width); //far left third of scree
        //                    dragTop = _rnd.Next(GameHeight - (int) sh.Height);
        //                    dropLeft = _rnd.Next((GameWidth / 3) * 2, GameWidth - (int) sh.Width); //far right third of screen
        //                    dropTop = _rnd.Next(GameHeight - (int) sh.Height);
        //                    break;
        //                case Locations.Random:
        //                    dragLeft = _rnd.Next(0, GameWidth - (int) sh.Width);
        //                    dragTop = _rnd.Next(0, GameHeight - (int) sh.Width);
        //                    dropLeft = _rnd.Next(0, GameWidth - (int) sh.Width);
        //                    dropTop = _rnd.Next(0, GameHeight - (int) sh.Width);
        //                    break;
        //            }

        //            if (sh.IsTapEnabled == true) //shape, not drop
        //            {
        //                DragShapes.Add(sh);
        //                Canvas.SetLeft(sh, dragLeft);
        //                Canvas.SetTop(sh, dragTop);
        //            }
        //            else
        //            {
        //                DropShapes.Add(sh);
        //                Canvas.SetLeft(sh, dropLeft);
        //                Canvas.SetTop(sh, dropTop);
        //            }
        //        } while (CheckIfOverlaps(sh));

        //        if (sh.IsTapEnabled == true) //shape, not drop
        //            CurrentShapeCount++;

        //        canvas.Children.Add(sh);
        //    }
        //}
        private void SetShapesOnCanvas<T>(Locations loc, KeyValuePair<T, T> newShapes) where T : Shape, new()
        {
            double dragLeft = 0,
                   dragTop = 0,
                   dropLeft = 0,
                   dropTop = 0;

            do
            {
                switch (loc)
                {
                    case Locations.LeftRight:
                        dragLeft = _rnd.Next((GameWidth / 2) - (int) newShapes.Key.Width); //keeps it on the left half of the screen
                        dragTop = _rnd.Next(GameHeight - (int) newShapes.Key.Height);
                        dropLeft = _rnd.Next(GameWidth / 2, GameWidth - (int) newShapes.Value.Width); //keeps it on the right half of the screen
                        dropTop = _rnd.Next(GameHeight - (int) newShapes.Value.Height);
                        break;
                    case Locations.LeftRightThirds:
                        dragLeft = _rnd.Next(0, (GameWidth / 3) - (int) newShapes.Key.Width); //far left third of scree
                        dragTop = _rnd.Next(GameHeight - (int) newShapes.Value.Height);
                        dropLeft = _rnd.Next((GameWidth / 3) * 2, GameWidth - (int) newShapes.Key.Width); //far right third of screen
                        dropTop = _rnd.Next(GameHeight - (int) newShapes.Value.Height);
                        break;
                    case Locations.Random:
                        dragLeft = _rnd.Next(0, GameWidth - (int) newShapes.Key.Width);
                        dragTop = _rnd.Next(0, GameHeight - (int) newShapes.Key.Width);
                        dropLeft = _rnd.Next(0, GameWidth - (int) newShapes.Value.Width);
                        dropTop = _rnd.Next(0, GameHeight - (int) newShapes.Value.Width);
                        break;
                }


                //DragShapes.Add(newShapes.Key);
                Canvas.SetLeft(newShapes.Key, dragLeft);
                Canvas.SetTop(newShapes.Key, dragTop);
       
                //DropShapes.Add(newShapes.Value);
                Canvas.SetLeft(newShapes.Value, dropLeft);
                Canvas.SetTop(newShapes.Value, dropTop);

                Canvas.SetZIndex(newShapes.Key, 1); //DragShapes stay on top

            } while (CheckIfOverlaps(newShapes.Key) || CheckIfOverlaps(newShapes.Value));

            DragDropShapes.Add(newShapes.Key, newShapes.Value);
                
            CurrentShapeCount++;

            canvas.Children.Add(newShapes.Key);
            canvas.Children.Add(newShapes.Value);
        }
        //private List<Shape> LevelOne()
        //{
        //    return PutShapes<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>());
        //    //List<Shape> newShapes;
        //    //newShapes = BuildDragDropShapePair<Ellipse>();

        //    //foreach (Shape sh in newShapes)
        //    //{
        //    //    if (sh.IsTapEnabled == true) //shape, not drop
        //    //    {
        //    //        DragShapes.Add(sh);
        //    //        canvas.Children.Add(sh);
        //    //        do
        //    //        {
        //    //            Canvas.SetLeft(sh, _rnd.Next((GameWidth / 2) - (int) sh.Width)); //keeps it on the left half of the screen
        //    //            Canvas.SetTop(sh, _rnd.Next(GameHeight - (int) sh.Height));
        //    //        } while (CheckIfOverlaps(sh));
        //    //        CurrentShapeCount++;
        //    //    }
        //    //    else
        //    //    {
        //    //        DropShapes.Add(sh);
        //    //        canvas.Children.Add(sh);

        //    //        do
        //    //        {
        //    //            Canvas.SetLeft(sh, _rnd.Next(GameWidth / 2, GameWidth - (int) sh.Width)); //keeps it on the right half of the screen
        //    //            Canvas.SetTop(sh, _rnd.Next(GameHeight - (int) sh.Height));
        //    //        } while (CheckIfOverlaps(sh));
        //    //    }

        //    //}

        //    //return newShapes;
        //}
        //private List<Shape> LevelTwo()
        //{
        //    List<Shape> newShapes;
        //    newShapes = BuildDragDropShapePair<Ellipse>();

        //    //foreach (Shape sh in newShapes)
        //    //{
        //    //    if (sh.IsTapEnabled == true) //shape, not drop
        //    //    {
        //    //        DragShapes.Add(sh);
        //    //        canvas.Children.Add(sh);

        //    //        do
        //    //        {
        //    //            //Canvas.SetLeft(el, _rnd.Next((GameWidth / 3) * 2, GameWidth - (int) el.Width)); //far right third of screen
        //    //            //Canvas.SetTop(el, _rnd.Next(0, (GameHeight / 2) - (int) el.Height)); //keeps it on the bottom half of the screen
        //    //            Canvas.SetLeft(sh, _rnd.Next(GameWidth / 2, GameWidth - (int) sh.Width)); //keeps it on the right half of the screen
        //    //            Canvas.SetTop(sh, _rnd.Next(GameHeight - (int) sh.Height));
        //    //        } while (CheckIfOverlaps(sh));

        //    //        CurrentShapeCount++;
        //    //    }
        //    //    else
        //    //    {
        //    //        DropShapes.Add(sh);
        //    //        canvas.Children.Add(sh);

        //    //        do
        //    //        {
        //    //            //Canvas.SetLeft(sh, _rnd.Next(0, (GameWidth / 3) - (int) sh.Width)); //far left third of screen
        //    //            //Canvas.SetTop(sh, _rnd.Next(GameHeight / 2, GameHeight - (int) sh.Height)); //keeps it on the top half of the screen
        //    //            Canvas.SetLeft(sh, _rnd.Next((GameWidth / 2) - (int) sh.Width)); //keeps it on the left half of the screen
        //    //            Canvas.SetTop(sh, _rnd.Next(GameHeight - (int) sh.Height));
        //    //        } while (CheckIfOverlaps(sh));
        //    //    }

        //    //}

        //    newShapes.AddRange(PutShapes<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>())); //another simple left/right ellipse pair
        //    newShapes.AddRange(PutShapes<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>()));
        //    return newShapes;

        //    //return newShapes;
        //}
        //private List<Shape> LevelThree()
        //{
        //    List<Shape> newShapes;
        //    newShapes = BuildDragDropShapePair<Rectangle>();

        //    //foreach (Rectangle polyShape in newShapes)
        //    //{
        //    //    canvas.Children.Add(polyShape);

        //    //    do
        //    //    {
        //    //        Canvas.SetLeft(polyShape, _rnd.Next(0, GameWidth - (int) polyShape.Width));
        //    //        Canvas.SetTop(polyShape, _rnd.Next(0, GameHeight - (int) polyShape.Height));
        //    //    } while (CheckIfOverlaps(polyShape));

        //    //    if (polyShape.IsTapEnabled == true) //shape, not drop
        //    //    {
        //    //        DragShapes.Add(polyShape);
        //    //        CurrentShapeCount++;
        //    //    }
        //    //    else
        //    //        DropShapes.Add(polyShape);

        //    //}

        //    //newShapes.AddRange(LevelOne());
        //    //newShapes.AddRange(LevelTwo());

        //    //todo: tapping on shapes will grow/shrink its drop to help show the difference

        //    //return newShapes;
        //    newShapes.AddRange(PutShapes<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>()));
        //    newShapes.AddRange(PutShapes<Ellipse>(Locations.LeftRight, BuildDragDropShapePair<Ellipse>()));
        //    newShapes.AddRange(PutShapes<Rectangle>(Locations.LeftRight, BuildDragDropShapePair<Rectangle>()));
        //    return newShapes;
        //}
        //private List<Shape> LevelFour()
        //{
        //    List<Shape> ellipses = LevelTwo();
        //    List<Shape> newShapes = LevelTwo();

        //    foreach (Ellipse el in ellipses)
        //    {

        //    }

        //    return newShapes;
        //}
        private bool CheckIfOverlaps(Shape newShapeToCheck)
        {
            foreach (KeyValuePair<Shape, Shape> kvpair in DragDropShapes)
            {
                bool overlapXaxis = false;
                bool overlapYaxis = false;
                Shape oldShape;

                for (int i = 0; i < 2; i++)
                {
                    switch(i)
                    {
                        case 0:
                            oldShape = kvpair.Key;
                            break;
                        case 1:  //fall through so compiler knows oldShape won't be null below
                        default:
                            oldShape = kvpair.Value;
                            break;
                    }

                    if (newShapeToCheck != oldShape)
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

                        overlapXaxis = (newShapeToCheckRight > oldShapeLeft && //overlaps on the left side
                                       newShapeToCheckRight < oldShapeRight) ||
                                       (newShapeToCheckLeft > oldShapeLeft &&  //overlaps on the right side
                                       newShapeToCheckLeft < oldShapeRight);

                        // ACHTUNG! (0,0) is in top-left so (0,20) is higher than (0,30)
                        overlapYaxis = (newShapeToCheckBottom > oldShapeTop &&    //overlaps above
                                       newShapeToCheckBottom < oldShapeBottom) ||
                                       (newShapeToCheckTop > oldShapeTop &&       //overlaps below
                                       newShapeToCheckTop < oldShapeBottom);

                        if (overlapXaxis && overlapYaxis)
                            return true;
                        else if (((newShapeToCheckLeft <= oldShapeLeft && newShapeToCheckRight >= oldShapeRight) ||  //it's wider so overlapXaxis will always be false
                                 (newShapeToCheckLeft >= oldShapeLeft && newShapeToCheckRight <= oldShapeRight)) &&  //it's narrower so overlapXaxis will always be false
                                 overlapYaxis == true)
                            return true;
                        else if (((newShapeToCheckTop <= oldShapeTop && newShapeToCheckBottom >= oldShapeBottom) ||  //it's taller so overlapYaxis will always be false
                                 (newShapeToCheckTop >= oldShapeTop && newShapeToCheckBottom <= oldShapeBottom)) &&  //it's shorter so overlapYaxis will always be false
                                 overlapXaxis == true)
                            return true;
                        else if ((newShapeToCheckTop <= oldShapeTop && newShapeToCheckBottom >= oldShapeBottom &&   //it's wider and taller and it's covering the other
                                 newShapeToCheckLeft <= oldShapeLeft && newShapeToCheckRight >= oldShapeRight) ||
                                 (newShapeToCheckTop >= oldShapeTop && newShapeToCheckBottom <= oldShapeBottom &&   //it's narrower and shorter and it's covered by the other
                                 newShapeToCheckLeft >= oldShapeLeft && newShapeToCheckRight <= oldShapeRight))
                            return true;
                    }
                }
            }

            return false;
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
#if DEBUG
            foreach (TextBlock tb in canvas.Children.OfType<TextBlock>().ToList<TextBlock>())
                canvas.Children.Remove(tb);
#endif


            //DragShapes.Clear();
            //DropShapes.Clear();
            DragDropShapes.Clear();
            GameStart(++CurrentLevel);
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
            //AnimateShape(ActiveDragShape);

            if (CurrentShapeCount == 0)
                LevelComplete();
        }
        private void AnimateShape(Shape sh)
        {
            if (sh.GetType() == typeof(Ellipse))
            {
                int originaHeight = (int) sh.Height;
                for(int flips = 0; flips < 10; flips++)
                { 
                    //shrink
                    for (int x = (int)sh.Height; x > 0; x--)
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        sh.Height -= 1;
                        while (true)
                        {
                            //some other processing to do possible
                            UpdateLayout();
                            if (stopwatch.ElapsedMilliseconds >= 10)
                            {
                                break;
                            }
                        }
                    }

                    //grow
                    for (int x = 0; x < originaHeight; x++)
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        sh.Height += 1;
                        while (true)
                        {
                            //some other processing to do possible
                            UpdateLayout();
                            if (stopwatch.ElapsedMilliseconds >= 10)
                            {
                                break;
                            }
                        }
                    }
                }
            }
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
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                laser.Play();
            });

            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(250));
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

                if (e.Pointer.IsInContact && sh == ActiveDragShape) //is moving/moveable
                {      
                    PointerPoint pointer_pos_in_canvas = e.GetCurrentPoint(sh.Parent as Canvas);
                    double newLeft = pointer_pos_in_canvas.Position.X - (sh.Width / 2);
                    double newTop = pointer_pos_in_canvas.Position.Y - (sh.Height / 2);

                    //don't go more than halfway off the screen
                    if (newLeft < (0 - sh.Width / 2)) //left
                        Canvas.SetLeft(sh, 0);
                    else if (newLeft > (GameWidth - (sh.Width / 2))) //right
                        Canvas.SetLeft(sh, (GameWidth - (sh.Width / 2)));
                    else
                        Canvas.SetLeft(sh, newLeft);

                    if (newTop < (0 - sh.Height / 2)) //top
                        Canvas.SetTop(sh, 0);
                    else if (newTop > (GameHeight - (sh.Height / 2)))
                        Canvas.SetTop(sh, (GameHeight - (sh.Height / 2)));
                    else
                        Canvas.SetTop(sh, newTop);

                    //Canvas.SetLeft(sh, pointer_pos_in_canvas.Position.X - (sh.Width / 2));
                    //Canvas.SetTop(sh, pointer_pos_in_canvas.Position.Y - (sh.Height / 2));

                    CheckForMatchingDrop();
                }
            }
        }
        private void Shape_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Shape;
            el.CapturePointer(e.Pointer);

            ActiveDragShape = el;

            Shape drop = ActiveDropShape;
            DragDropShapes.TryGetValue(el, out drop);
            ActiveDropShape = drop;

        }
        private void Shape_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var el = sender as Shape;
            el.ReleasePointerCapture(e.Pointer);
            ActiveDragShape = null;
            ActiveDropShape = null;
        }
        private void Canvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
#if DEBUG
            foreach (TextBlock tb in canvas.Children.OfType<TextBlock>().ToList<TextBlock>())
                canvas.Children.Remove(tb);

            foreach (KeyValuePair<Shape, Shape> pair in DragDropShapes)
            {
                //sh.IsDoubleTapEnabled = true;
                //sh.DoubleTapped += Shape_DoubleTapped;
                TextBlock dragStats = new TextBlock();
                Color dragColor = ((SolidColorBrush) pair.Key.Fill).Color;

                dragStats.Text = "Left: " + Canvas.GetLeft(pair.Key).ToString() + "\nTop: " + Canvas.GetTop(pair.Key).ToString() + "\nColor: " + KidsColors.ColorNames[((SolidColorBrush) pair.Key.Fill).Color];
                dragStats.FontSize = 10;
                canvas.Children.Add(dragStats);
                Canvas.SetLeft(dragStats, Canvas.GetLeft(pair.Key));
                Canvas.SetTop(dragStats, Canvas.GetTop(pair.Key));
                Canvas.SetZIndex(dragStats, 100);
                CheckIfOverlaps(pair.Key);

                TextBlock dropStats = new TextBlock();
                Color dropColor = ((SolidColorBrush) pair.Value.Fill).Color;

                dropStats.Text = "Left: " + Canvas.GetLeft(pair.Value).ToString() + "\nTop: " + Canvas.GetTop(pair.Value).ToString() + "\nColor: " + KidsColors.ColorNames[((SolidColorBrush) pair.Value.Fill).Color];
                dropStats.FontSize = 10;
                canvas.Children.Add(dropStats);
                Canvas.SetLeft(dropStats, Canvas.GetLeft(pair.Value));
                Canvas.SetTop(dropStats, Canvas.GetTop(pair.Value));
                Canvas.SetZIndex(dropStats, 100);
                CheckIfOverlaps(pair.Value);
            }
#endif
        }
        #endregion
    }
}
