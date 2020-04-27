using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Drawing;

namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class TargetMap
    {
        [DataMember] public string Name = ""; // map name
        [DataMember] public string Label = ""; // map label displayed to user
        [DataMember] public string Source = ""; // source of map
        [DataMember] public string Type = ""; // dendogram, pathway...
        [DataMember] public string ImageType; // extension type of image for map
        [DataMember] public string ImageFile = ""; // image file (without directory)
        [DataMember] public Rectangle Bounds; // image bounds
        [DataMember] public bool MarkBounds = false; // if true put dummy points in rectangle boundaries  
        [DataMember] public string CoordsFile = ""; // coords file (without directory)
        [DataMember] public Dictionary<int, List<TargetMapCoords>> Coords; // list of coordinates indexed by target id
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class TargetMapCoords
    {
        [DataMember] public int TargetId; // Entrez gene id for target
        [DataMember] public int X;
        [DataMember] public int Y;
        [DataMember] public int X2;
        [DataMember] public int Y2;
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class Rectangle
    {
        [DataMember] public int X;
        [DataMember] public int Y;
        [DataMember] public int Width;
        [DataMember] public int Height;

        public Rectangle()
        {
        }

        public Rectangle(System.Drawing.Rectangle rectangle)
        {
            //rectangle is a non-nullable type
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
        }

        public static explicit operator System.Drawing.Rectangle(Rectangle serviceRectangle)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle();
            if (serviceRectangle != null)
            {
                rectangle.X = serviceRectangle.X;
                rectangle.Y = serviceRectangle.Y;
                rectangle.Width = serviceRectangle.Width;
                rectangle.Height = serviceRectangle.Height;
            }
            return rectangle;
        }

    }
}
