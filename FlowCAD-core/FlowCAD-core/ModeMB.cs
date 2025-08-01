using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowCAD_core
{
    internal class ModeMB : Mode
    {
        public ModeMB() { }

        private static bool layerChecked = false;

        public static List<Circle> GetAllMuayeneBacasiCircles(Database db)
        {
            List<Circle> circles = new List<Circle>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                foreach (ObjectId objId in btr)
                {
                    Entity entity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (entity is Circle circle)
                    {
                        if (nod.Contains(circle.Handle.ToString()))
                        {
                            Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(circle.Handle.ToString()), OpenMode.ForRead);
                            TypedValue[] values = xRecord.Data.AsArray();
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (values[i].Value.ToString() == "class")
                                {
                                    if (values[i + 1].Value.ToString() == "muayene_bacası")
                                    {
                                        circles.Add(circle);
                                    }
                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }

            return circles;
        }

        public static List<Circle> GetMuayeneBacasiCirclesWithoutIzahatCemberi(Database db)
        {
            List<Circle> circles = new List<Circle>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                foreach (ObjectId objId in btr)
                {
                    Entity entity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (entity is Circle circle)
                    {
                        if (nod.Contains(circle.Handle.ToString()))
                        {
                            Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(circle.Handle.ToString()), OpenMode.ForRead);
                            TypedValue[] values = xRecord.Data.AsArray();
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (values[i].Value.ToString() == "class")
                                {
                                    if (values[i + 1].Value.ToString() == "muayene_bacası" && (values[i+9].Value.ToString()=="-1"))
                                    {
                                        circles.Add(circle);
                                    }
                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }

            return circles;
        }

        public static Dictionary<string, List<Circle>> categorizeMuayeneBacasiCirclesByHatId(List<Circle> circles)
        {
            Dictionary<string, List<Circle>> result = new Dictionary<string, List<Circle>>();
            foreach (Circle circle in circles)
            {
                Xrecord xRecordOfCircle = Utils.fetchXrecordOfObject(circle.ObjectId);
                TypedValue[] xRecordValues = xRecordOfCircle.Data.AsArray();
                for(int i = 0;i < xRecordValues.Length;i++)
                {
                    if(xRecordValues[i].Value.ToString() == "Mb_hat_id")
                    {
                        string hatId = xRecordValues[i+1].Value.ToString();
                        if (result.ContainsKey(hatId))
                        {
                            List<Circle> circlesWithGivenHatId = result[hatId];
                            string kNoId = xRecordValues[i + 3].Value.ToString();
                            placeMuayeneBacasiCircleInHat(circlesWithGivenHatId, circle, kNoId);
                        } else
                        {
                            result[hatId] = new List<Circle>();
                            result[hatId].Add(circle);
                        }
                        break;
                    }
                }
            }
            return result;
        }
        
        private static void placeMuayeneBacasiCircleInHat(List<Circle> circleByHat, Circle circle, string kNoId)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            for (int i = 0; i<circleByHat.Count; i++)
            {
                Circle circleInHat = circleByHat[i];
                int kNoIdOfCircleInHat = Utils.ExtractNumberFromKod(Utils.fetchXrecordOfObject(circleInHat.ObjectId).Data.AsArray()[7].Value.ToString());
                if (Utils.ExtractNumberFromKod(kNoId) < kNoIdOfCircleInHat)
                {
                    circleByHat.Insert(i, circle);
                    return;
                } else if(i == circleByHat.Count - 1)
                {
                    circleByHat.Add(circle);
                    return;
                }
                else continue;
            }
        } 

        public Circle createCircle(Point3d center)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            if (!layerChecked)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                    if (!lt.Has("K_HAT"))
                    {
                        lt.UpgradeOpen();
                        LayerTableRecord ltr = new LayerTableRecord
                        {
                            Name = "K_HAT",
                            Color = Color.FromColorIndex(ColorMethod.ByAci, 2),
                            LineWeight = LineWeight.LineWeight035
                        };
                        lt.Add(ltr);
                        tr.AddNewlyCreatedDBObject(ltr, true);
                    }
                    else
                    {
                        // Force visibility and unlock if it exists
                        LayerTableRecord ltr = (LayerTableRecord)tr.GetObject(lt["K_HAT"], OpenMode.ForWrite);
                        ltr.IsOff = false;
                        ltr.IsFrozen = false;
                        ltr.IsLocked = false;
                        ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 2); // ensure color is visible
                    }

                    tr.Commit();
                }
                layerChecked = true;
            }
            Circle circle = new Circle(center, Vector3d.ZAxis, 0.5)
            {
                Layer = "K_HAT",
                Color = Color.FromColorIndex(ColorMethod.ByAci, 2),
                LineWeight = LineWeight.LineWeight035
            };

            return circle;
        }

        public Xrecord createXrecord(string hatId, string kNoId)
        {
            Xrecord xRecord = new Xrecord();
            xRecord.Data = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "class"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "muayene_bacası"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Unique_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, Guid.NewGuid().ToString()),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Mb_hat_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, hatId),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Mb_kn_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, kNoId),                
                
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Mb_izahat_cemberi_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Mb_akarkot_text_object_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Dim_out_object_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Dim_in_object_ids"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "is_main_hat"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "false")
            );
            return xRecord;
        }

        public static AlignedDimension RetrieveInDimensionOfMuayeneBacasi(Circle muayeneBacasi)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TypedValue[] xrecordDataMuayeneBacasi = Utils.fetchXrecordOfObject(muayeneBacasi.ObjectId).Data.AsArray();
                string inDimensionHandleString = xrecordDataMuayeneBacasi[15].Value.ToString();
                if (inDimensionHandleString == "-1")
                {
                    return null;
                }
                ObjectId dimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inDimensionHandleString, 16)), 0);
                AlignedDimension inDimension = (AlignedDimension)tr.GetObject(dimensionObjectId, OpenMode.ForWrite);
                tr.Commit();
                return inDimension;
            }
        }

        public static AlignedDimension RetrieveOutDimensionOfMuayeneBacasi(Circle muayeneBacasi)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TypedValue[] xrecordDataMuayeneBacasi = Utils.fetchXrecordOfObject(muayeneBacasi.ObjectId).Data.AsArray();
                string outDimensionHandleString = xrecordDataMuayeneBacasi[13].Value.ToString();
                if (outDimensionHandleString == "-1")
                {
                    return null;
                }
                ObjectId dimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(outDimensionHandleString, 16)), 0);
                AlignedDimension inDimension = (AlignedDimension)tr.GetObject(dimensionObjectId, OpenMode.ForWrite);
                tr.Commit();
                return inDimension;
            }
        }

        public static DBText RetrieveAkarkotTextOfMuayeneBacasi(Circle muayeneBacasi)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TypedValue[] xrecordDataMuayeneBacasi = Utils.fetchXrecordOfObject(muayeneBacasi.ObjectId).Data.AsArray();
                string akarkotTextHandleString = xrecordDataMuayeneBacasi[11].Value.ToString();
                if (akarkotTextHandleString == "-1")
                {
                    return null;
                }
                ObjectId dimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(akarkotTextHandleString, 16)), 0);
                DBText akarkotText = (DBText)tr.GetObject(dimensionObjectId, OpenMode.ForWrite);
                tr.Commit();
                return akarkotText;
            }
        }

        public static void ResetInDimensionXrecordDataOfMuayeneBacasi(Circle muayeneBacasi)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // Find the XRecord by dimension handle string
                string muayeneBacasiHandleString = muayeneBacasi.Handle.ToString();
                if (!nod.Contains(muayeneBacasiHandleString))
                {
                    ed.WriteMessage($"\nNo XRecord found for dimension handle {muayeneBacasiHandleString}.");
                    return;
                }

                Xrecord xrec = (Xrecord)tr.GetObject(nod.GetAt(muayeneBacasiHandleString), OpenMode.ForWrite);
                TypedValue[] data = xrec.Data.AsArray();

                // Find the index of "slope_text_id" and set the next value to "-1"
                for (int i = 0; i < data.Length - 1; i++)
                {
                    if (data[i].Value.ToString() == "Dim_in_object_ids")
                    {
                        data[i + 1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1");
                        break;
                    }
                }

                // Update the XRecord with the modified data
                xrec.Data = new ResultBuffer(data);

                tr.Commit();
            }
        }

        public static void ResetOutDimensionXrecordDataOfMuayeneBacasi(Circle muayeneBacasi)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // Find the XRecord by dimension handle string
                string muayeneBacasiHandleString = muayeneBacasi.Handle.ToString();
                if (!nod.Contains(muayeneBacasiHandleString))
                {
                    ed.WriteMessage($"\nNo XRecord found for dimension handle {muayeneBacasiHandleString}.");
                    return;
                }

                Xrecord xrec = (Xrecord)tr.GetObject(nod.GetAt(muayeneBacasiHandleString), OpenMode.ForWrite);
                TypedValue[] data = xrec.Data.AsArray();

                // Find the index of "slope_text_id" and set the next value to "-1"
                for (int i = 0; i < data.Length - 1; i++)
                {
                    if (data[i].Value.ToString() == "Dim_out_object_id")
                    {
                        data[i + 1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1");
                        break;
                    }
                }

                // Update the XRecord with the modified data
                xrec.Data = new ResultBuffer(data);

                tr.Commit();
            }
        }


        public static void ResetAkarKotTextXrecordDataOfMuayeneBacasi(Circle muayeneBacasi)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // Find the XRecord by dimension handle string
                string muayeneBacasiHandleString = muayeneBacasi.Handle.ToString();
                if (!nod.Contains(muayeneBacasiHandleString))
                {
                    ed.WriteMessage($"\nNo XRecord found for dimension handle {muayeneBacasiHandleString}.");
                    return;
                }

                Xrecord xrec = (Xrecord)tr.GetObject(nod.GetAt(muayeneBacasiHandleString), OpenMode.ForWrite);
                TypedValue[] data = xrec.Data.AsArray();

                // Find the index of "slope_text_id" and set the next value to "-1"
                for (int i = 0; i < data.Length - 1; i++)
                {
                    if (data[i].Value.ToString() == "Mb_akarkot_text_object_id")
                    {
                        data[i + 1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1");
                        break;
                    }
                }

                // Update the XRecord with the modified data
                xrec.Data = new ResultBuffer(data);

                tr.Commit();
            }
        }


    }
}
