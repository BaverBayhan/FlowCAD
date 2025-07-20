using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace FlowCAD_core
{
    internal class ModeDRAW : Mode
    {
        public ModeDRAW() { }

        public static string CreateMuayeneBacasiDimension(Circle firstCircle, Circle secondCircle)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            string idOfDim = "";

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);


                string layerName = "DIMENSIONS";

                // Step 1: Ensure the layer exists
                if (!lt.Has(layerName))
                {
                    lt.UpgradeOpen(); // Allow layer creation
                    LayerTableRecord newLayer = new LayerTableRecord { Name = layerName };
                    newLayer.Color = Color.FromColorIndex(ColorMethod.ByAci, 4); // Set Cyan color for the layer
                    lt.Add(newLayer);
                    tr.AddNewlyCreatedDBObject(newLayer, true);
                }

                // Step 2: Calculate trimmed start and end points
                Vector3d direction = (secondCircle.Center - firstCircle.Center).GetNormal(); // Get unit direction vector
                Point3d trimmedStart = firstCircle.Center + (direction * firstCircle.Radius); // Start at first circle's edge
                Point3d trimmedEnd = secondCircle.Center - (direction * secondCircle.Radius); // End at second circle's edge

                // Calculate midpoint
                double distance = trimmedStart.DistanceTo(trimmedEnd);
                Point3d midpoint = new Point3d(
                    (trimmedStart.X + trimmedEnd.X) / 2,
                    (trimmedStart.Y + trimmedEnd.Y) / 2,
                    0
                );

                // Step 3: Create Aligned Dimension
                AlignedDimension dim = new AlignedDimension(
                    trimmedStart,
                    trimmedEnd,
                    midpoint,
                    $"L = {distance:F2}m \n \\U+2205 200 B.B.",
                    ObjectId.Null
                );

                // Basic properties
                dim.Layer = layerName;
                dim.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, 3); // Green color
                dim.Dimasz = 0.35; // Arrow size
                dim.Dimtxt = 0.4; // Text size

                // Turn off extension lines
                dim.Dimse1 = true; // Suppress extension line 1
                dim.Dimse2 = true; // Suppress extension line 2

                // Set custom arrowheads
                dim.Dimsah = true; // Enable different arrow heads for start and end
                ObjectId id1 = GetArrowObjectId("DIMBLK1", "_NONE");
                dim.Dimblk1 = id1; // Apply no arrow

                dim.Dimblk2 = bt["custom-arrow"]; // Apply custom arrow

                // Adjust text positioning
                dim.Dimtad = 1; // Text above dimension line
                dim.Dimgap = 0.2; // Gap between text and dimension line

                dim.Dimlwd = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight030; // Set line weight

                // Add objects to drawing
                btr.AppendEntity(dim);
                tr.AddNewlyCreatedDBObject(dim, true);

                Xrecord xRecordOfDimension = new Xrecord();
                xRecordOfDimension.Data = new ResultBuffer(
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "out_circle_handle"),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, firstCircle.Handle.ToString()),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "in_circle_handle"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, secondCircle.Handle.ToString()),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "slope_text_id"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "out_flow"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "in_flow"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1")
                );

                ObjectId xRecordId = nod.SetAt(dim.Handle.ToString(), xRecordOfDimension);
                tr.AddNewlyCreatedDBObject(xRecordOfDimension, true);

                tr.Commit();
                idOfDim = dim.Handle.ToString();
            }
            return idOfDim;
        }

        public static AlignedDimension CreateMuayeneBacasiDimension(Circle circle, Point3d point)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);


                string layerName = "DIMENSIONS";

                // Step 1: Ensure the layer exists
                if (!lt.Has(layerName))
                {
                    lt.UpgradeOpen(); // Allow layer creation
                    LayerTableRecord newLayer = new LayerTableRecord { Name = layerName };
                    newLayer.Color = Color.FromColorIndex(ColorMethod.ByAci, 4); // Set Cyan color for the layer
                    lt.Add(newLayer);
                    tr.AddNewlyCreatedDBObject(newLayer, true);
                }

                // Step 2: Calculate trimmed start and end points
                Vector3d direction = (circle.Center - point).GetNormal(); // Get unit direction vector
                Point3d start = point;
                Point3d end = circle.Center - (direction * circle.Radius); // End at second circle's edge

                // Calculate midpoint
                double distance = start.DistanceTo(end);
                Point3d midpoint = new Point3d(
                    (start.X + end.X) / 2,
                    (start.Y + end.Y) / 2,
                    0
                );

                // Step 3: Create Aligned Dimension
                AlignedDimension dim = new AlignedDimension(
                    start,
                    end,
                    midpoint,
                    $"L = {distance:F2}m \n \\U+2205 200 B.B.",
                    ObjectId.Null
                );

                // Basic properties
                dim.Layer = layerName;
                dim.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, 1); // Green color
                dim.Dimasz = 0.25; // Arrow size
                dim.Dimtxt = 0.3; // Text size

                // Turn off extension lines
                dim.Dimse1 = true; // Suppress extension line 1
                dim.Dimse2 = true; // Suppress extension line 2

                // Set custom arrowheads
                dim.Dimsah = true; // Enable different arrow heads for start and end
                ObjectId id1 = GetArrowObjectId("DIMBLK1", "_NONE");
                dim.Dimblk1 = id1; // Apply no arrow

                dim.Dimblk2 = bt["custom-arrow-1"]; // Apply custom arrow

                // Adjust text positioning
                dim.Dimtad = 1; // Text above dimension line
                dim.Dimgap = 0.2; // Gap between text and dimension line

                dim.Dimlwd = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight030; // Set line weight

                // Add objects to drawing
                btr.AppendEntity(dim);
                tr.AddNewlyCreatedDBObject(dim, true);

                Xrecord xRecordOfDimension = new Xrecord();
                xRecordOfDimension.Data = new ResultBuffer(
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "out_circle_handle"),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "PARSEL"),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "in_circle_handle"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, circle.Handle.ToString()),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "slope_text_id"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "out_flow"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "in_flow"),
                   new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1")
                );

                ObjectId xRecordId = nod.SetAt(dim.Handle.ToString(), xRecordOfDimension);
                tr.AddNewlyCreatedDBObject(xRecordOfDimension, true);

                tr.Commit();
                return dim;
            }

        }

        public static void CreateCustomArrowBlockIfNotExists(Database db, Transaction tr, string blockName, short colorCode)
        {
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            // Check if block already exists
            if (bt.Has(blockName))
                return;

            // Create a new block table record for the arrow
            BlockTableRecord btr = new BlockTableRecord
            {
                Name = blockName,
                Origin = Point3d.Origin
            };

            // Add the new block to the block table first
            bt.UpgradeOpen();
            ObjectId btrId = bt.Add(btr);
            tr.AddNewlyCreatedDBObject(btr, true);

            // Define the arrow shape (Triangle)
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(-2, -1, 0);
            Point3d p3 = new Point3d(-2, 1, 0);

            // Create the hatch fill first (so it's behind the outline)
            Polyline hatchBoundary = new Polyline();
            hatchBoundary.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
            hatchBoundary.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
            hatchBoundary.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
            hatchBoundary.Closed = true;
            hatchBoundary.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256); // ByLayer to make it invisible
            hatchBoundary.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.ByLayer;

            // Add the hatch boundary to the block
            btr.AppendEntity(hatchBoundary);
            tr.AddNewlyCreatedDBObject(hatchBoundary, true);

            // Create the hatch
            Hatch hatch = new Hatch();
            hatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            hatch.Color = Color.FromColorIndex(ColorMethod.ByAci, colorCode);

            // Add the boundary to the hatch
            ObjectIdCollection objectIds = new ObjectIdCollection();
            objectIds.Add(hatchBoundary.ObjectId);
            hatch.AppendLoop(HatchLoopTypes.Default, objectIds);

            // Add the hatch to the block
            btr.AppendEntity(hatch);
            tr.AddNewlyCreatedDBObject(hatch, true);

            // Create a polyline for the outline instead of separate lines
            // This approach often shows thickness better in AutoCAD
            Polyline outlinePoly = new Polyline();
            outlinePoly.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
            outlinePoly.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
            outlinePoly.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
            outlinePoly.AddVertexAt(3, new Point2d(p1.X, p1.Y), 0, 0, 0);
            outlinePoly.Closed = true;
            outlinePoly.ConstantWidth = 0.2; // Set a constant width for the polyline
            outlinePoly.Color = Color.FromColorIndex(ColorMethod.ByAci, 3); // Green outline
            outlinePoly.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight080; // Use a valid LineWeight enum value

            // Add the outline polyline to the block
            btr.AppendEntity(outlinePoly);
            tr.AddNewlyCreatedDBObject(outlinePoly, true);

            // Also ensure that lineweights are displayed in AutoCAD
            Application.SetSystemVariable("LWDISPLAY", 1);
        }

        public static void CreateNoArrowBlockIfNotExists(Database db, Transaction tr, string blockName)
        {
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            // Check if block already exists
            if (bt.Has(blockName))
                return;

            // Create a new block table record for the arrow
            BlockTableRecord btr = new BlockTableRecord
            {
                Name = blockName,
                Origin = Point3d.Origin
            };

            // Add the new block to the block table first
            bt.UpgradeOpen();
            ObjectId btrId = bt.Add(btr);
            tr.AddNewlyCreatedDBObject(btr, true);

            // Define the arrow shape (Triangle)
            Polyline noArrow = new Polyline();
            noArrow.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            noArrow.AddVertexAt(1, new Point2d(0, 0), 0, 0, 0);
            noArrow.AddVertexAt(2, new Point2d(0, 0), 0, 0, 0);
            noArrow.Closed = true;
            noArrow.Color = Color.FromColorIndex(ColorMethod.ByAci, 3); // Green Outline
            noArrow.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight050;

            // Add the arrow to the block table record
            btr.AppendEntity(noArrow);
            tr.AddNewlyCreatedDBObject(noArrow, true);
        }

        public static Circle findNearestCircleToSourceCircle(Circle sourceCircle, List<Circle> circles)
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Circle nearestCircle = null;
            double minDistance = double.MaxValue;

            foreach (Circle circle in circles)
            {
                if (circle == sourceCircle)
                    continue;
                double distance = circle.Center.DistanceTo(sourceCircle.Center);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCircle = circle;
                }
            }

            // if circle is the circle with greatest kapak kod
            bool isGreatestCircle = true;
            string sourceCircleIzahatCemberiKapakNo = Utils.fetchXrecordOfObject(sourceCircle.ObjectId).Data.AsArray()[7].Value.ToString();
            int kodNumberSourceCircle = Utils.ExtractNumberFromKod(sourceCircleIzahatCemberiKapakNo);

            foreach (Circle circle in circles)
            {
                if (circle == sourceCircle)
                    continue;

                string circleIzahatCemberiKapakNo = Utils.fetchXrecordOfObject(circle.ObjectId).Data.AsArray()[7].Value.ToString();
                int kodNumberCircle = Utils.ExtractNumberFromKod(circleIzahatCemberiKapakNo);

                if (kodNumberSourceCircle < kodNumberCircle)
                {
                    isGreatestCircle = false;
                    break;
                }
            }


            if (sourceCircle.Center.DistanceTo(nearestCircle.Center) > 50 || isGreatestCircle)
            {
                return null;
            }
            return nearestCircle;
        }

        public static void AlignTextWithDimension(Circle circle, Database db, bool outCircle)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Xrecord xRecord = Utils.fetchXrecordOfObject(circle.ObjectId);
                TypedValue[] xRecordValues = xRecord.Data.AsArray();
                if (xRecordValues[11].Value.ToString() == "-1")
                    return;

                Handle textHandle = new Handle(Convert.ToInt64(xRecordValues[11].Value.ToString(), 16));
                Handle dimHandle;
                if (outCircle)
                {
                    dimHandle = new Handle(Convert.ToInt64(xRecordValues[13].Value.ToString(), 16));
                }
                else
                {
                    dimHandle = new Handle(Convert.ToInt64(xRecordValues[15].Value.ToString(), 16));
                }

                ObjectId textObjectId = db.GetObjectId(false, textHandle, 0);
                ObjectId dimObjectId = db.GetObjectId(false, dimHandle, 0);


                AlignedDimension dim = tr.GetObject(dimObjectId, OpenMode.ForRead) as AlignedDimension;
                DBText text = tr.GetObject(textObjectId, OpenMode.ForWrite) as DBText;

                if (dim == null || text == null)
                    return;

                Point3d startPoint = dim.XLine1Point;
                Point3d endPoint = dim.XLine2Point;

                double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
                text.Rotation = angle;


                double offsetAlongLine = 0.5;
                if (!outCircle)
                {
                    offsetAlongLine *= -7;
                }
                double offsetPerpendicular = 0.2;


                Vector3d alongVector = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0) * offsetAlongLine;
                Vector3d perpVector = new Vector3d(-Math.Sin(angle), Math.Cos(angle), 0) * offsetPerpendicular;


                if (outCircle)
                {
                    text.Position = startPoint + alongVector + perpVector;
                }
                else
                {
                    text.Position = endPoint + alongVector + perpVector;
                }

                text.HorizontalMode = TextHorizontalMode.TextLeft;
                text.VerticalMode = TextVerticalMode.TextBottom;
                text.AlignmentPoint = text.Position;

                tr.Commit();
            }
        }

        private static ObjectId GetArrowObjectId(string arrow, string newArrName)
        {

            ObjectId arrObjId = ObjectId.Null;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            string oldArrName = Application.GetSystemVariable(arrow) as string;
            Application.SetSystemVariable(arrow, newArrName);
            if (oldArrName.Length != 0)
                Application.SetSystemVariable(arrow, oldArrName);
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                arrObjId = bt[newArrName];
                tr.Commit();
            }
            return arrObjId;
        }

        public static DBText GetSlopeTextOfDimension(AlignedDimension dimension)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TypedValue[] xrecordDataMuayeneBacasi = Utils.fetchXrecordOfObject(dimension.ObjectId).Data.AsArray();
                string slopeTextHandleString = xrecordDataMuayeneBacasi[5].Value.ToString();
                if (slopeTextHandleString == "-1")
                {
                    return null;
                }
                ObjectId dimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(slopeTextHandleString, 16)), 0);
                DBText slopeText = (DBText)tr.GetObject(dimensionObjectId, OpenMode.ForWrite);
                tr.Commit();
                return slopeText;
            }
        }

        public static DBText GetInFlowTextOfDimension(AlignedDimension dimension)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TypedValue[] xrecordDataMuayeneBacasi = Utils.fetchXrecordOfObject(dimension.ObjectId).Data.AsArray();
                string inFlowTextHandleString = xrecordDataMuayeneBacasi[9].Value.ToString();
                if (inFlowTextHandleString == "-1")
                {
                    return null;
                }
                ObjectId dimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inFlowTextHandleString, 16)), 0);
                DBText slopeText = (DBText)tr.GetObject(dimensionObjectId, OpenMode.ForWrite);
                tr.Commit();
                return slopeText;
            }
        }

        public static DBText GetOutFlowTextOfDimension(AlignedDimension dimension)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TypedValue[] xrecordDataMuayeneBacasi = Utils.fetchXrecordOfObject(dimension.ObjectId).Data.AsArray();
                string slopeTextHandleString = xrecordDataMuayeneBacasi[7].Value.ToString();
                if (slopeTextHandleString == "-1")
                {
                    return null;
                }
                ObjectId dimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(slopeTextHandleString, 16)), 0);
                DBText slopeText = (DBText)tr.GetObject(dimensionObjectId, OpenMode.ForWrite);
                tr.Commit();
                return slopeText;
            }
        }

        public static void ResetSlopeXrecordDataOfDimension(AlignedDimension dimension, string value)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // Find the XRecord by dimension handle string
                string dimHandleStr = dimension.Handle.ToString();
                if (!nod.Contains(dimHandleStr))
                {
                    ed.WriteMessage($"\nNo XRecord found for dimension handle {dimHandleStr}.");
                    return;
                }

                Xrecord xrec = (Xrecord)tr.GetObject(nod.GetAt(dimHandleStr), OpenMode.ForWrite);
                TypedValue[] data = xrec.Data.AsArray();

                // Find the index of "slope_text_id" and set the next value to "-1"
                for (int i = 0; i < data.Length - 1; i++)
                {
                    if (data[i].Value.ToString() == "slope_text_id")
                    {
                        data[i + 1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, value);
                        break;
                    }
                }

                // Update the XRecord with the modified data
                xrec.Data = new ResultBuffer(data);

                tr.Commit();
            }
        }

        public static void ResetOutFlowXrecordDataOfDimension(AlignedDimension dimension, string value)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // Find the XRecord by dimension handle string
                string dimHandleStr = dimension.Handle.ToString();
                if (!nod.Contains(dimHandleStr))
                {
                    ed.WriteMessage($"\nNo XRecord found for dimension handle {dimHandleStr}.");
                    return;
                }

                Xrecord xrec = (Xrecord)tr.GetObject(nod.GetAt(dimHandleStr), OpenMode.ForWrite);
                TypedValue[] data = xrec.Data.AsArray();
                for (int i = 0; i < data.Length - 1; i++)
                {
                    if (data[i].Value.ToString() == "out_flow")
                    {
                        data[i + 1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, value);
                        break;
                    }
                }

                // Update the XRecord with the modified data
                xrec.Data = new ResultBuffer(data);

                tr.Commit();
            }
        }

        public static void ResetInFlowXrecordDataOfDimension(AlignedDimension dimension, string value)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // Find the XRecord by dimension handle string
                string dimHandleStr = dimension.Handle.ToString();
                if (!nod.Contains(dimHandleStr))
                {
                    ed.WriteMessage($"\nNo XRecord found for dimension handle {dimHandleStr}.");
                    return;
                }

                Xrecord xrec = (Xrecord)tr.GetObject(nod.GetAt(dimHandleStr), OpenMode.ForWrite);
                TypedValue[] data = xrec.Data.AsArray();
                for (int i = 0; i < data.Length - 1; i++)
                {
                    if (data[i].Value.ToString() == "in_flow")
                    {
                        data[i + 1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, value);
                        break;
                    }
                }

                // Update the XRecord with the modified data
                xrec.Data = new ResultBuffer(data);

                tr.Commit();
            }
        }

        public static void CreateAndAlignTextsForParselDimension(string slopeText, string inFlowText, string outFlowText, AlignedDimension dimension)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Format inFlowText
                string formattedInFlow = inFlowText;
                double inFlowVal;
                if (double.TryParse(inFlowText, out inFlowVal))
                    formattedInFlow = inFlowVal.ToString("F2");

                // Format outFlowText
                string formattedOutFlow = outFlowText;
                double outFlowVal;
                if (double.TryParse(outFlowText, out outFlowVal))
                    formattedOutFlow = outFlowVal.ToString("F2");

                // Create slope text
                DBText slopeTextObj = new DBText
                {
                    TextString = string.Format("1/{0}", (slopeText).ToString()),
                    Height = 0.65,
                    Layer = "DIMENSIONS",
                    ColorIndex = 2 // Green
                };
                btr.AppendEntity(slopeTextObj);
                tr.AddNewlyCreatedDBObject(slopeTextObj, true);
                // Create in flow text
                DBText inFlowTextObj = new DBText
                {
                    TextString = formattedInFlow,
                    Height = 0.55,
                    Layer = "DIMENSIONS",
                    ColorIndex = 4
                };
                btr.AppendEntity(inFlowTextObj);
                tr.AddNewlyCreatedDBObject(inFlowTextObj, true);
                // Create out flow text
                DBText outFlowTextObj = new DBText
                {
                    TextString = formattedOutFlow,
                    Height = 0.55,
                    Layer = "DIMENSIONS",
                    ColorIndex = 4 // Green
                };
                btr.AppendEntity(outFlowTextObj);
                tr.AddNewlyCreatedDBObject(outFlowTextObj, true);

                // Align texts with dimension
                AlignTextWithDimension(dimension, outFlowTextObj, AlignmentPosition.START); // For out flow text
                AlignTextWithDimension(dimension, inFlowTextObj, AlignmentPosition.END); // For in flow text
                AlignTextWithDimension(dimension, slopeTextObj, AlignmentPosition.MID); // For slope text

                // Update Xrecord with text object IDs
                ModeDRAW.ResetInFlowXrecordDataOfDimension(dimension, inFlowTextObj.Handle.ToString());
                ModeDRAW.ResetOutFlowXrecordDataOfDimension(dimension, outFlowTextObj.Handle.ToString());
                ModeDRAW.ResetSlopeXrecordDataOfDimension(dimension, slopeTextObj.Handle.ToString());
                tr.Commit();
            }
        }

        private static void AlignTextWithDimension(AlignedDimension dimension, DBText text, AlignmentPosition position)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Point3d startPoint = dimension.XLine1Point;
                Point3d endPoint = dimension.XLine2Point;
                double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
                text.Rotation = angle;

                double offsetAlongLine = 0.5;
                double offsetPerpendicular = 0.2;
                Vector3d alongVector = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0) * offsetAlongLine;
                Vector3d perpVector = new Vector3d(-Math.Sin(angle), Math.Cos(angle), 0) * offsetPerpendicular;

                switch (position)
                {
                    case AlignmentPosition.START:
                        text.Position = startPoint + alongVector + perpVector;
                        break;
                    case AlignmentPosition.END:
                        text.Position = endPoint + (alongVector * -7) + perpVector;
                        break;
                    case AlignmentPosition.MID:
                        Point3d midPoint = new Point3d(
                            (startPoint.X + endPoint.X) / 2,
                            (startPoint.Y + endPoint.Y) / 2,
                            (startPoint.Z + endPoint.Z) / 2
                        );
                        text.Position = midPoint + perpVector * -10;
                        break;
                }

                text.HorizontalMode = TextHorizontalMode.TextLeft;
                text.VerticalMode = TextVerticalMode.TextBottom;
                text.AlignmentPoint = text.Position;
                tr.Commit();
            }
        }


        private enum AlignmentPosition
        {
            START,
            MID,
            END
        }

    }
}

