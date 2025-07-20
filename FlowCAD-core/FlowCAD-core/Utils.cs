using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;

namespace FlowCAD_core
{
    internal class Utils
    {
        public static void ZoomToCircle(Circle circle, int zoomFactor)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Extents3d extents = new Extents3d(
                circle.Center - new Vector3d(circle.Radius * zoomFactor, circle.Radius * zoomFactor, 0),
                circle.Center + new Vector3d(circle.Radius * zoomFactor, circle.Radius * zoomFactor, 0)
            );

            ViewTableRecord view = ed.GetCurrentView();
            view.CenterPoint = new Point2d(circle.Center.X, circle.Center.Y);
            view.Height = extents.MaxPoint.Y - extents.MinPoint.Y;
            view.Width = extents.MaxPoint.X - extents.MinPoint.X;
            ed.SetCurrentView(view);
        }

        public static DBText CreateTextObject(Point3d position, string textContent)
        {
            DBText text = new DBText
            {
                Position = position,
                TextString = textContent,
                Height = 1,
                Rotation = 0,
                WidthFactor = 1,
                Justify = AttachmentPoint.MiddleCenter, 
                Color = Color.FromColorIndex(ColorMethod.ByAci, 30) 
            };

            text.Annotative = AnnotativeStates.False;
            text.HorizontalMode = TextHorizontalMode.TextMid;
            text.VerticalMode = TextVerticalMode.TextVerticalMid;
            text.AlignmentPoint = position;

            return text;
        }

        public static string IncrementMuayeneBacasiKapakKod(string kod)
        {
            if (string.IsNullOrEmpty(kod) || !kod.StartsWith("A"))
                throw new ArgumentException("Invalid kod format. Must start with 'A'.");

            // Extract numeric part
            string numberPart = kod.Substring(1);

            if (!int.TryParse(numberPart, out int number))
                throw new ArgumentException("Invalid numeric part in kod.");

            // Increment and return new kod
            return $"A{number + 1}";
        }

        public static int ExtractNumberFromKod(string kod)
        {
            if (string.IsNullOrEmpty(kod) || !kod.StartsWith("A"))
                throw new ArgumentException("Invalid kod format. Must start with 'A'.");

            // Extract numeric part and convert to integer
            if (int.TryParse(kod.Substring(1), out int number))
                return number;

            throw new ArgumentException("Invalid numeric part in kod.");
        }

        public static Xrecord fetchXrecordOfObject(ObjectId objId)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                if (nod.Contains(objId.Handle.ToString()))
                {
                    Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(objId.Handle.ToString()), OpenMode.ForRead);
                    tr.Commit();
                    return xRecord;
                }
                else
                {
                    tr.Commit();
                    return null;
                }
            }
        }

    }
}
