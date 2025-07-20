using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace FlowCAD_core
{
    internal class ModeAKARKOT : Mode
    {
        public ModeAKARKOT() { }

        public static DBText createAkarkotTextObject(Point3d position, string textContent)
        {
            if (double.TryParse(textContent, out double value))
            {
                textContent= value.ToString("F2");
            }
            DBText text = new DBText
            {
                Position = position,
                TextString = textContent,
                Height = 0.55,
                Rotation = Math.PI / 2,
                WidthFactor = 1,
                Justify = AttachmentPoint.MiddleCenter,
                Color = Color.FromColorIndex(ColorMethod.ByAci, 4)
            };

            text.Annotative = AnnotativeStates.False;
            text.HorizontalMode = TextHorizontalMode.TextLeft;
            text.VerticalMode = TextVerticalMode.TextVerticalMid;
            text.AlignmentPoint = position;

            return text;
        }

        public static DBText createAkarkotTextAlignedWithDimension(AlignedDimension dimension, string textContent, bool isOut)
        {
            Point3d startPoint = dimension.XLine1Point;
            Point3d endPoint = dimension.XLine2Point;

            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);


            double offsetAlongLine = 0.5;
            if (!isOut)
            {
                offsetAlongLine *= -9;
            }
            double offsetPerpendicular = 0.2;


            Vector3d alongVector = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0) * offsetAlongLine;
            Vector3d perpVector = new Vector3d(-Math.Sin(angle), Math.Cos(angle), 0) * offsetPerpendicular;

            Point3d position;
            if (isOut)
            {
                position = startPoint + alongVector + perpVector;
            }
            else
            {
                position = endPoint + alongVector + perpVector;
            }
            
            if (double.TryParse(textContent, out double value))
            {
                textContent= value.ToString("F2");
            }

            DBText text = new DBText
            {
                Position = position,
                TextString = textContent,
                Height = 0.55,
                Rotation = angle,
                WidthFactor = 1,
                Justify = AttachmentPoint.MiddleCenter,
                Color = Color.FromColorIndex(ColorMethod.ByAci, 4)
            };
            text.HorizontalMode = TextHorizontalMode.TextLeft;
            text.VerticalMode = TextVerticalMode.TextBottom;
            text.AlignmentPoint = text.Position;
            return text;
        }
    }
}
