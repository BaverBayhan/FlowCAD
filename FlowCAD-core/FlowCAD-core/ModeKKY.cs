using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace FlowCAD_core
{
    internal class ModeKKY : Mode
    {
        public ModeKKY() { }

        private static bool layerChecked = false;

        public static List<Circle> GetIzahatCemberiCircles(Database db)
        {
            List<Circle> izahatCemberiList = new List<Circle>();

            // Retrieve izahat_cemberi circles with no kapak_kot assigned
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                foreach (ObjectId objId in btr)
                {
                    Entity entity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (entity is Circle circle)
                    {
                        Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(circle.Handle.ToString()), OpenMode.ForRead);
                        TypedValue[] values = xRecord.Data.AsArray();

                        for (int i = 0; i < values.Length - 1; i++)
                        {
                            if ((values[i].Value.ToString() == "class" && values[i + 1].Value.ToString() == "mb_kky") && values[i+7].Value.ToString() == "-1")
                            {
                                izahatCemberiList.Add(circle);
                            }
                        }
                    }
                }
                tr.Commit(); // End first transaction
            }

            return izahatCemberiList;
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

                    if (!lt.Has("AS_DugumNoktasi_IzahatCemb"))
                    {
                        lt.UpgradeOpen();
                        LayerTableRecord ltr = new LayerTableRecord
                        {
                            Name = "AS_DugumNoktasi_IzahatCemb",
                            Color = Color.FromColorIndex(ColorMethod.ByAci, 2),
                            LineWeight = LineWeight.LineWeight035
                        };
                        lt.Add(ltr);
                        tr.AddNewlyCreatedDBObject(ltr, true);
                    }

                    tr.Commit();
                }
                layerChecked = true;
            }
            Circle circle = new Circle(center, Vector3d.ZAxis, 2)
            {
                Layer = "AS_DugumNoktasi_IzahatCemb",
                Color = Color.FromColorIndex(ColorMethod.ByAci, 2),
                LineWeight = LineWeight.LineWeight035
            };
            return circle;
        }

        public Xrecord createXrecord(string textObjectKnId)
        {
            Xrecord xRecord = new Xrecord();
            xRecord.Data = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "class"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "mb_kky"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Unique_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, Guid.NewGuid().ToString()),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Mb_unique_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Text_object_kk_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "-1"),

                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Text_object_kn_id"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, textObjectKnId)
            );
            return xRecord;
        }
    }
}
