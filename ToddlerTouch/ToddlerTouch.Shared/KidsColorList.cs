using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.UI;

/*
 * ColorList class from: http://stackoverflow.com/questions/12751008/how-to-enumerate-through-colors-in-winrt
 * license: http://creativecommons.org/licenses/by-sa/3.0/     http://creativecommons.org/licenses/by-sa/3.0/legalcode
*/

namespace DaphnesGame
{
    /// <summary>
    /// Provides a list of colors that are different enough and bright enough for use with children.
    /// </summary>
    public class KidsColorList : List<Color>
    {
        private Random rnd;
        public Dictionary<Color, string> ColorNames; 

    /// <summary>
    /// Builds the List of colors, defaulting to the exclusion of colors not inducive to a children's game.
    /// </summary>
    /// <param name="UseAllColors">Set to true if you want to include all color options. False is default.</param>
    /// <remarks>
    /// With UseAllColors=false, these colors are removed: black, whites, grays, some browns/tans, some colors too similar to others.
    /// </remarks>
    public KidsColorList(bool UseAllColors = false)
        {
            BuildList(UseAllColors);
            rnd = new Random();

#if DEBUG
            ColorNames = new Dictionary<Color, string>();
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                ColorNames[(Color) color.GetValue(null)] = color.Name;
            }
#endif
        }

        public Color GetRandomColor()
        {
            return this[rnd.Next(0, this.Count)];
        }
        private void BuildList(bool all = false)
        {
            if(all)
            {
                this.Add(Colors.AntiqueWhite);
                this.Add(Colors.Azure);
                this.Add(Colors.Black);
                this.Add(Colors.DimGray);
                this.Add(Colors.FloralWhite);
                this.Add(Colors.GhostWhite);
                this.Add(Colors.NavajoWhite);
                this.Add(Colors.Gray);
                this.Add(Colors.Transparent);
                this.Add(Colors.SlateGray);
                this.Add(Colors.LightSlateGray);
                this.Add(Colors.LightGray);
                this.Add(Colors.Snow);
                this.Add(Colors.Beige);
                this.Add(Colors.DarkGray);
                this.Add(Colors.DarkSlateGray);
                this.Add(Colors.Khaki);
                this.Add(Colors.Linen);
                this.Add(Colors.LightSeaGreen);
                this.Add(Colors.LightSkyBlue);
                this.Add(Colors.LightSteelBlue);
                this.Add(Colors.OldLace);
                this.Add(Colors.Wheat);
                this.Add(Colors.White);
                this.Add(Colors.WhiteSmoke);
                this.Add(Colors.LightGoldenrodYellow);
                this.Add(Colors.LightYellow);
                this.Add(Colors.Lavender);
                this.Add(Colors.LavenderBlush);
                this.Add(Colors.PeachPuff);
                this.Add(Colors.MintCream);
                this.Add(Colors.Tan);
                this.Add(Colors.BlanchedAlmond);
                this.Add(Colors.BurlyWood);
                this.Add(Colors.DarkSalmon);
                this.Add(Colors.Peru);
                this.Add(Colors.PaleGoldenrod);
                this.Add(Colors.LawnGreen);
                this.Add(Colors.LightGreen);
                this.Add(Colors.Moccasin);
                this.Add(Colors.OliveDrab);
                this.Add(Colors.PapayaWhip);
                this.Add(Colors.Ivory);
                this.Add(Colors.Cornsilk);
                this.Add(Colors.MediumVioletRed);
                this.Add(Colors.Silver);
                this.Add(Colors.MistyRose);
                this.Add(Colors.LemonChiffon);
                this.Add(Colors.AliceBlue);
                this.Add(Colors.Gainsboro);
                this.Add(Colors.LightBlue);
                this.Add(Colors.LightCyan);
                this.Add(Colors.DarkSlateBlue);
                this.Add(Colors.Bisque);
                this.Add(Colors.Chartreuse);
                this.Add(Colors.SandyBrown);
                this.Add(Colors.LightSalmon);
                this.Add(Colors.Honeydew);
            }

            this.Add(Colors.Aqua);
            this.Add(Colors.Aquamarine);
            this.Add(Colors.Blue);
            this.Add(Colors.BlueViolet);
            this.Add(Colors.Brown);
            this.Add(Colors.CadetBlue);
            this.Add(Colors.Chocolate);
            this.Add(Colors.Coral);
            this.Add(Colors.CornflowerBlue);
            this.Add(Colors.Crimson);
            this.Add(Colors.Cyan);
            this.Add(Colors.DarkBlue);
            this.Add(Colors.DarkCyan);
            this.Add(Colors.DarkGoldenrod);
            this.Add(Colors.DarkGreen);
            this.Add(Colors.DarkKhaki);
            this.Add(Colors.DarkMagenta);
            this.Add(Colors.DarkOliveGreen);
            this.Add(Colors.DarkOrange);
            this.Add(Colors.DarkOrchid);
            this.Add(Colors.DarkRed);
            this.Add(Colors.DarkSeaGreen);
            this.Add(Colors.DarkTurquoise);
            this.Add(Colors.DarkViolet);
            this.Add(Colors.DeepPink);
            this.Add(Colors.DeepSkyBlue);
            this.Add(Colors.DodgerBlue);
            this.Add(Colors.Firebrick); 
            this.Add(Colors.ForestGreen);
            this.Add(Colors.Fuchsia);
            this.Add(Colors.Gold);
            this.Add(Colors.Goldenrod);
            this.Add(Colors.Green);
            this.Add(Colors.GreenYellow);
            this.Add(Colors.HotPink);
            this.Add(Colors.IndianRed);
            this.Add(Colors.Indigo);
            this.Add(Colors.LightCoral);
            this.Add(Colors.LightPink);
            this.Add(Colors.Lime);
            this.Add(Colors.LimeGreen);
            this.Add(Colors.Magenta);
            this.Add(Colors.Maroon);
            this.Add(Colors.MediumAquamarine);
            this.Add(Colors.MediumBlue);
            this.Add(Colors.MediumOrchid);
            this.Add(Colors.MediumPurple);
            this.Add(Colors.MediumSeaGreen);
            this.Add(Colors.MediumSlateBlue);
            this.Add(Colors.MediumSpringGreen);
            this.Add(Colors.MediumTurquoise);
            this.Add(Colors.MidnightBlue);
            this.Add(Colors.Navy);
            this.Add(Colors.Olive);
            this.Add(Colors.Orange);
            this.Add(Colors.OrangeRed);
            this.Add(Colors.Orchid);
            this.Add(Colors.PaleGreen);
            this.Add(Colors.PaleTurquoise);
            this.Add(Colors.PaleVioletRed);
            this.Add(Colors.Pink);
            this.Add(Colors.Plum);
            this.Add(Colors.PowderBlue);
            this.Add(Colors.Purple);
            this.Add(Colors.Red);
            this.Add(Colors.RosyBrown);
            this.Add(Colors.RoyalBlue);
            this.Add(Colors.SaddleBrown);
            this.Add(Colors.Salmon);
            this.Add(Colors.SeaGreen);
            this.Add(Colors.SeaShell);
            this.Add(Colors.Sienna);
            this.Add(Colors.SkyBlue);
            this.Add(Colors.SlateBlue);
            this.Add(Colors.SpringGreen);
            this.Add(Colors.SteelBlue);
            this.Add(Colors.Teal);
            this.Add(Colors.Thistle);
            this.Add(Colors.Tomato);
            this.Add(Colors.Turquoise);
            this.Add(Colors.Violet);
            this.Add(Colors.Yellow);
            this.Add(Colors.YellowGreen);
        }

    }
}
