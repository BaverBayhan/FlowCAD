using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Autodesk.AutoCAD.Geometry;

[assembly: CommandClass(typeof(FlowCAD_core.Commands))]
namespace FlowCAD_core
{
    public class Commands
    {
        private static Mode mode = new ModeMB();

        static Commands()
        {
            mode = new ModeMB(); // Reset to default MB mode on NETLOAD
        }


        [CommandMethod("resetMode")]
        public void ResetMode()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            mode = new ModeMB();
            ed.WriteMessage("\nMode reset to MB.");
        }

        [CommandMethod("changeMode")]
        public void ChangeMode()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptKeywordOptions options = new PromptKeywordOptions("\nSelect mode : ");
            options.AllowNone = false;
            options.Keywords.Add("MB");
            options.Keywords.Add("KKY");
            options.Keywords.Add("FMKNA");
            options.Keywords.Add("AKARKOT");
            options.Keywords.Add("DRAW");
            options.Keywords.Add("CALCULATE");
            options.Keywords.Add("REVERT");

            PromptResult result = ed.GetKeywords(options);

            if (result.Status != PromptStatus.OK)
                return; 

            switch (result.StringResult.ToLower())
            {
                case "mb":
                    if(ModeController.isTransitionPossible(mode, new ModeMB()))
                    {
                        mode = new ModeMB();
                        ed.WriteMessage("\nSwitched to MB mode.");
                    }
                    else
                    {
                        ed.WriteMessage("\nTransition not possible.");
                    }
                    break;
                case "kky":
                    if(ModeController.isTransitionPossible(mode, new ModeKKY()))
                    {
                        mode = new ModeKKY();
                        ed.WriteMessage("\nSwitched to KKY mode.");
                    }
                    else
                    {
                        ed.WriteMessage("\nTransition not possible.");
                    }
                    break;
                case "fmkna":
                    if(ModeController.isTransitionPossible(mode, new ModeFMKNA()))
                    {
                        mode = new ModeFMKNA();
                        ed.WriteMessage("\nSwitched to FM-KNA mode.");
                    }
                    else
                    {
                        ed.WriteMessage("\nTransition not possible.");
                    }
                    break;
                case "akarkot":
                    if(ModeController.isTransitionPossible(mode, new ModeAKARKOT()))
                    {
                        mode = new ModeAKARKOT();
                        ed.WriteMessage("\nSwitched to AKARKOT mode.");
                    }
                    else
                    {
                        ed.WriteMessage("\nTransition not possible.");
                    }
                    break;
                case "draw":
                    if (ModeController.isTransitionPossible(mode, new ModeDRAW()))
                    {
                        mode = new ModeDRAW();
                        ed.WriteMessage("\nSwitched to DRAW mode.");
                    }
                    else
                    {
                        ed.WriteMessage("\nTransition not possible.");
                    }
                    break;

                case "calculate":
                    if (ModeController.isTransitionPossible(mode, new ModeCALCULATE()))
                    {
                        mode = new ModeCALCULATE();
                        ed.WriteMessage("\nSwitched to CALCULATE mode.");
                    }
                    else
                    {
                        ed.WriteMessage("\nTransition not possible.");
                    }
                    break;

                 case "revert":
                    if (ModeController.isTransitionPossible(mode, new ModeREVERT()))
                    {
                        mode = new ModeREVERT();
                        ed.WriteMessage("\nSwitched to REVERT mode.");
                    }
                    else
                    {
                        mode = new ModeMB();
                        ed.WriteMessage("\nReverted to MB mode.");
                    }
                    break;

            }
        }

        [CommandMethod("createMuayeneBacasi")]
        public void createMuayeneBacasi()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            String hatId = Guid.NewGuid().ToString();
            if (mode is ModeMB modeMB)
            {
                while (true)
                {
                    bool isFirstCircle = true;
                    string latestKnoId = "";
                    PromptPointResult pointResult = ed.GetPoint("\nClick to create a 'muayene bacasi':");
                    if (pointResult.Status != PromptStatus.OK) return;

                    // Convert UCS point to WCS
                    Point3d ucsPoint = pointResult.Value;
                    Point3d wcsPoint = ucsPoint.TransformBy(ed.CurrentUserCoordinateSystem);

                    Circle circle = modeMB.createCircle(pointResult.Value);
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                        Xrecord xRecord = null;
                        if (isFirstCircle)
                        {
                            string biggestKnoId = "A0";
                            foreach (ObjectId objId in btr)
                            {
                                Entity entity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                                if (entity is Circle circleIterated)
                                {
                                    if (nod.Contains(circleIterated.Handle.ToString()))
                                    {
                                        Xrecord xRecordIteratedCircle = (Xrecord)tr.GetObject(nod.GetAt(circleIterated.Handle.ToString()), OpenMode.ForRead);
                                        TypedValue[] values = xRecordIteratedCircle.Data.AsArray();
                                        for (int i = 0; i < values.Length; i++)
                                        {
                                            if (values[i].Value.ToString() == "Mb_kn_id")
                                            {
                                                if (Utils.ExtractNumberFromKod(values[i+1].Value.ToString()) > Utils.ExtractNumberFromKod(biggestKnoId))
                                                {
                                                    biggestKnoId = values[i + 1].Value.ToString();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            xRecord = modeMB.createXrecord(hatId, Utils.IncrementMuayeneBacasiKapakKod(biggestKnoId));
                            isFirstCircle = false;
                        }
                        else
                        {
                            xRecord = modeMB.createXrecord(hatId, Utils.IncrementMuayeneBacasiKapakKod(latestKnoId));
                        }
                        btr.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                        ObjectId xRecordId = nod.SetAt(circle.Handle.ToString(), xRecord);
                        tr.AddNewlyCreatedDBObject(xRecord, true);
                        tr.Commit();
                    }
                }
            }
            else
            {
                ed.WriteMessage("\nThis command is only available in MB mode.");
            }
        }

        [CommandMethod("createIzahatCemberi")]
        public void createIzahatCemberi()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            if (mode is ModeKKY modeKKY)
            {
                List<Circle> circles = ModeMB.GetMuayeneBacasiCirclesWithoutIzahatCemberi(db);
                foreach (Circle circle in circles)
                {
                    Utils.ZoomToCircle(circle, 20);
                    PromptPointResult pointResult = ed.GetPoint("\nClick to create 'izahat cemberi':");
                    if (pointResult.Status != PromptStatus.OK)
                    {
                        return;
                    }
                    Circle izahatCemberi = modeKKY.createCircle(pointResult.Value);
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                        btr.AppendEntity(izahatCemberi);
                        tr.AddNewlyCreatedDBObject(izahatCemberi, true);
                        string kapakNoId = "";
                        // To update circle's xrecord execute a second transaction
                        using (Transaction tr2 = db.TransactionManager.StartTransaction())
                        {
                            Xrecord xRecordToEdit = (Xrecord)tr2.GetObject(nod.GetAt(circle.Handle.ToString()), OpenMode.ForWrite);
                            TypedValue[] updatedValues = xRecordToEdit.Data.AsArray();
                            for (int i = 0; i < updatedValues.Length; i++)
                            {
                                if (updatedValues[i].Value.ToString() == "Mb_izahat_cemberi_id")
                                {
                                    updatedValues[i+1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, izahatCemberi.Handle.ToString());
                                    kapakNoId = updatedValues[i-1].Value.ToString();
                                    break;
                                }
                            }
                            xRecordToEdit.Data = new ResultBuffer(updatedValues);
                            tr2.Commit();
                        }
                        tr.TransactionManager.QueueForGraphicsFlush();
                        doc.Editor.UpdateScreen();

                        DBText izahatText = Utils.CreateTextObject(izahatCemberi.Center, kapakNoId);
                        btr.AppendEntity(izahatText);
                        tr.AddNewlyCreatedDBObject(izahatText, true);
                        tr.TransactionManager.QueueForGraphicsFlush();
                        doc.Editor.UpdateScreen();

                        Xrecord xRecordIzahatCemberi = modeKKY.createXrecord(izahatText.Handle.ToString());
                        ObjectId xRecordId = nod.SetAt(izahatCemberi.Handle.ToString(), xRecordIzahatCemberi);
                        tr.AddNewlyCreatedDBObject(xRecordIzahatCemberi, true);
                        tr.Commit();
                    }
                }
            }
            else
            {
                ed.WriteMessage("\nThis command is only available in KKY mode.");
            }
        }

        [CommandMethod("AssignKapakKot")]
        public void AssignKapakKot()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            if (mode is ModeFMKNA)
            {
                List<Circle> izahatCemberiList = ModeKKY.GetIzahatCemberiCircles(db);

                foreach (Circle izahatCemberi in izahatCemberiList)
                {
                    Utils.ZoomToCircle(izahatCemberi, 7);
                    doc.Editor.Regen();

                    PromptStringOptions stringOptions = new PromptStringOptions("\nEnter text for Kapak Kot:");
                    stringOptions.AllowSpaces = true;
                    PromptResult textResult = ed.GetString(stringOptions);
                    if (textResult.Status != PromptStatus.OK) continue;

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                        // Create a text object
                        DBText kapakKotText;
                        if (double.TryParse(textResult.StringResult, out double value))
                        {
                            string formattedValue = value.ToString("F2");
                            kapakKotText = Utils.CreateTextObject(new Point3d(izahatCemberi.Center.X, izahatCemberi.Center.Y + izahatCemberi.Radius*2, 0), $"({formattedValue})");
                        }
                        else
                        {
                            // Fallback: use the original input if parsing fails
                            kapakKotText = Utils.CreateTextObject(new Point3d(izahatCemberi.Center.X, izahatCemberi.Center.Y + izahatCemberi.Radius*2, 0), $"({textResult.StringResult})");
                        }
                        btr.AppendEntity(kapakKotText);
                        tr.AddNewlyCreatedDBObject(kapakKotText, true);

                        // Update XRecord of `izahat_cemberi` with text object ID
                        if (nod.Contains(izahatCemberi.Handle.ToString()))
                        {
                            Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(izahatCemberi.Handle.ToString()), OpenMode.ForWrite);
                            TypedValue[] updatedValues = xRecord.Data.AsArray();
                            for (int i = 0; i < updatedValues.Length; i++)
                            {
                                if (updatedValues[i].Value.ToString() == "Text_object_kk_id")
                                {
                                    updatedValues[i+1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, kapakKotText.Handle.ToString());
                                    break;
                                }
                            }

                            xRecord.Data = new ResultBuffer(updatedValues);
                        }
                        tr.Commit();
                    }
                }
            }
            else
            {
                doc.Editor.WriteMessage("\nThis command is only available in FM-KNA mode.");
            }

        }

        [CommandMethod("ShowXrecordData")]
        public void ShowXrecordData()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Ask user to select a circle
            PromptEntityResult entityResult = ed.GetEntity("\nSelect a circle to view metadata:");
            if (entityResult.Status != PromptStatus.OK) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectId objId = entityResult.ObjectId;
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                // Check if an Xrecord exists for this object
                if (nod.Contains(objId.Handle.ToString()))
                {
                    Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(objId.Handle.ToString()), OpenMode.ForRead);
                    ResultBuffer rb = xRecord.Data;

                    if (rb != null)
                    {
                        TypedValue[] values = rb.AsArray();
                        ed.WriteMessage("\n--- Xrecord Data ---");
                        for (int i = 0; i < values.Length; i++)
                        {
                            ed.WriteMessage($"\n{values[i].TypeCode}: {values[i].Value}");
                        }
                    }
                    else
                    {
                        ed.WriteMessage("\nNo data found in Xrecord.");
                    }
                }
                else
                {
                    ed.WriteMessage("\nNo Xrecord attached to this object.");
                }

                tr.Commit();
            }
        }

        [CommandMethod("SelectCirclesByHatId")]
        public void SelectCirclesByHatId()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            string targetHatId = "";

            // Ask user to select a circle OR enter hat_id manually
            PromptEntityOptions entityOptions = new PromptEntityOptions("\nSelect a circle to find matching hat_id or press Enter to input manually:");
            entityOptions.SetRejectMessage("\nOnly circles allowed.");
            entityOptions.AddAllowedClass(typeof(Circle), true);

            PromptEntityResult entityResult = ed.GetEntity(entityOptions);

            if (entityResult.Status == PromptStatus.OK)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Circle selectedCircle = tr.GetObject(entityResult.ObjectId, OpenMode.ForRead) as Circle;
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                    if (nod.Contains(selectedCircle.Handle.ToString()))
                    {
                        Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(selectedCircle.Handle.ToString()), OpenMode.ForRead);
                        TypedValue[] values = xRecord.Data.AsArray();
                        for (int i = 0; i < values.Length - 1; i++)
                        {
                            if (values[i].Value.ToString() == "class")
                            {
                                if(values[i + 1].Value.ToString() != "muayene_bacası")
                                {
                                    ed.WriteMessage("\nSelected circle is not a Muayene Bacası.");
                                    tr.Commit();
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                if (entityResult.Status == PromptStatus.OK)
                {
                    // Retrieve hat_id from selected circle
                    ObjectId selectedId = entityResult.ObjectId;

                    if (nod.Contains(selectedId.Handle.ToString()))
                    {
                        Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(selectedId.Handle.ToString()), OpenMode.ForRead);
                        TypedValue[] values = xRecord.Data.AsArray();

                        for (int i = 0; i < values.Length - 1; i++)
                        {
                            if (values[i].Value.ToString() == "Mb_hat_id")
                            {
                                targetHatId = values[i + 1].Value.ToString();
                                break;
                            }
                        }
                    }
                }
                else if (entityResult.Status == PromptStatus.None)
                {
                    // User wants to manually input hat_id
                    PromptIntegerResult hatIdInput = ed.GetInteger("\nEnter Hat ID:");
                    if (hatIdInput.Status == PromptStatus.OK)
                        targetHatId = hatIdInput.Value.ToString();
                }

                if (targetHatId == "")
                {
                    ed.WriteMessage("\nNo valid hat_id found.");
                    return;
                }

                // Search for all circles with the same hat_id
                List<ObjectId> matchingCircles = new List<ObjectId>();

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId objId in btr)
                {
                    Entity entity = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                    if (entity is Circle)
                    {
                        if (nod.Contains(objId.Handle.ToString()))
                        {
                            Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(objId.Handle.ToString()), OpenMode.ForRead);
                            TypedValue[] values = xRecord.Data.AsArray();

                            for (int i = 0; i < values.Length - 1; i++)
                            {
                                if (values[i].Value.ToString() == "Mb_hat_id" && values[i + 1].Value.ToString() == targetHatId)
                                {
                                    matchingCircles.Add(objId);
                                    break;
                                }
                            }
                        }
                    }
                }

                tr.Commit();

                // Highlight matching circles
                if (matchingCircles.Count > 0)
                {
                    ed.SetImpliedSelection(matchingCircles.ToArray());
                    ed.WriteMessage($"\n{matchingCircles.Count} circles found with hat_id {targetHatId}.");
                }
                else
                {
                    ed.WriteMessage("\nNo matching circles found.");
                }
            }
        }

        [CommandMethod("AssignAkarkot")]
        public void assignAkarkot()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            if (mode is ModeAKARKOT )
            {

                List<Circle> baslangicMuayeneBacalariList = new List<Circle>();
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                    List<Circle> muayeneBacasiList = new List<Circle>();
                    List<string> hatIds = new List<string>();

                    foreach (ObjectId objId in btr)
                    {
                        Entity entity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                        if (entity is Circle circle)
                        {
                            string handleKey = circle.Handle.ToString();
                            if (nod.Contains(handleKey))
                            {
                                Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(handleKey), OpenMode.ForRead);
                                if(xRecord == null) continue;
                                TypedValue[] values = xRecord.Data.AsArray();

                                for (int i = 0; i < values.Length - 1; i++)
                                {
                                    if (values[i].Value.ToString() == "class" && values[i + 1].Value.ToString() == "muayene_bacası")
                                    {
                                        if (!hatIds.Contains(values[i+5].Value.ToString()))
                                        {
                                            hatIds.Add(values[i+5].Value.ToString());
                                        }
                                        muayeneBacasiList.Add(circle);
                                    }
                                }
                            }

                        }
                    }

                    Dictionary<string, List<Circle>> muayeneBacasiListByHatIds = new Dictionary<string, List<Circle>>();
                    foreach (string hatId in hatIds)
                    {
                        foreach (Circle muayeneBacasi in muayeneBacasiList)
                        {
                            Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(muayeneBacasi.Handle.ToString()), OpenMode.ForRead);
                            TypedValue[] values = xRecord.Data.AsArray();
                            if (hatId == values[5].Value.ToString())
                            {
                                if (!muayeneBacasiListByHatIds.ContainsKey(hatId))
                                {
                                    muayeneBacasiListByHatIds[hatId] = new List<Circle>();
                                }
                                muayeneBacasiListByHatIds[hatId].Add(muayeneBacasi);
                            }
                        }
                    }

                    foreach (var kvp in muayeneBacasiListByHatIds)
                    {
                        Circle baslangicBacasi = null;
                        foreach(Circle muayeneBacasi in kvp.Value)
                        {
                            if(baslangicBacasi == null)
                            {
                                baslangicBacasi = muayeneBacasi;
                            }

                            Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(muayeneBacasi.Handle.ToString()), OpenMode.ForRead);
                            TypedValue[] values = xRecord.Data.AsArray();
                            string kapakKodOfMuayeneBacasi = values[7].Value.ToString();
                            string akarkotOfMuayeneBacasi = values[11].Value.ToString();

                            Xrecord xRecord1 = (Xrecord)tr.GetObject(nod.GetAt(baslangicBacasi.Handle.ToString()), OpenMode.ForRead);
                            TypedValue[] values1 = xRecord.Data.AsArray();
                            string kapakKodOfAssignedBaslangicBacasi = values[7].Value.ToString();

                            if(Utils.ExtractNumberFromKod(kapakKodOfMuayeneBacasi) < Utils.ExtractNumberFromKod(kapakKodOfAssignedBaslangicBacasi))
                            {
                                baslangicBacasi = muayeneBacasi;
                            }
                        }
                        Xrecord xRecordBaslangicBacasi= (Xrecord)tr.GetObject(nod.GetAt(baslangicBacasi.Handle.ToString()), OpenMode.ForRead);
                        TypedValue[] valuesBaslangicBacasi = xRecordBaslangicBacasi.Data.AsArray();
                        if (valuesBaslangicBacasi[11].Value.ToString() == "-1")
                        {
                            baslangicMuayeneBacalariList.Add(baslangicBacasi);
                        }
                    }
                    ed.WriteMessage($"\n{baslangicMuayeneBacalariList.Count} baslangic muayene bacasi found with no akarkot assigned.");
                    

                    tr.Commit();
                }

                foreach(Circle baslangicBacasi in baslangicMuayeneBacalariList)
                {
                    Utils.ZoomToCircle(baslangicBacasi, 20);
                    doc.Editor.Regen();

                    PromptStringOptions stringOptions = new PromptStringOptions("\nEnter text for Akar Kot:");
                    stringOptions.AllowSpaces = true;
                    PromptResult textResult = ed.GetString(stringOptions);
                    if (textResult.Status != PromptStatus.OK) continue;

                    DBText akarkotText = ModeAKARKOT.createAkarkotTextObject(new Point3d(baslangicBacasi.Center.X, baslangicBacasi.Center.Y - baslangicBacasi.Radius * 2, 0), textResult.StringResult);
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);
                        btr.AppendEntity(akarkotText);
                        tr.AddNewlyCreatedDBObject(akarkotText, true);
                        if (nod.Contains(baslangicBacasi.Handle.ToString()))
                        {
                            Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(baslangicBacasi.Handle.ToString()), OpenMode.ForWrite);
                            TypedValue[] updatedValues = xRecord.Data.AsArray();
                            for (int i = 0; i < updatedValues.Length; i++)
                            {
                                if (updatedValues[i].Value.ToString() == "Mb_akarkot_text_object_id")
                                {
                                    updatedValues[i+1] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, akarkotText.Handle.ToString());
                                    break;
                                }
                            }
                            xRecord.Data = new ResultBuffer(updatedValues);
                        }
                        tr.Commit();
                    }
                }
            }
            else
            {
                ed.WriteMessage("\nThis command is only available in AKARKOT mode.");
            }
        }

        [CommandMethod("drawDimensions")]
        public void drawDimensions()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            if(!(mode is ModeDRAW))
            {
                ed.WriteMessage("\nThis command is only available in DRAW mode.");
                return;
            }

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ModeDRAW.CreateCustomArrowBlockIfNotExists(db, tr, "custom-arrow", 5);
                tr.Commit();
            }

            List<Circle> circles = ModeMB.GetAllMuayeneBacasiCircles(db);
            Dictionary<string, List<Circle>> categorizedCircles = ModeMB.categorizeMuayeneBacasiCirclesByHatId(circles);

            foreach (var kvp in categorizedCircles)
            {
                List<Circle> circlesInHat = kvp.Value;
                Circle firstCircle = circlesInHat[0];
                TypedValue[] firstCircleValues = Utils.fetchXrecordOfObject(firstCircle.ObjectId).Data.AsArray();

                if (firstCircleValues[13].Value.ToString() != "-1")
                {
                    Circle lastCircle = circlesInHat[circlesInHat.Count - 1];
                    TypedValue[] lastCircleValues = Utils.fetchXrecordOfObject(lastCircle.ObjectId).Data.AsArray();
                    if (lastCircleValues[13].Value.ToString() != "-1")
                    {
                        continue;
                    }
                    else
                    {
                        List<Circle> circlesExcludedCurrentHat = circles.Where(circle => !circlesInHat.Contains(circle)).ToList();
                        Circle destinationCircle = ModeDRAW.findNearestCircleToSourceCircle(lastCircle, circlesExcludedCurrentHat);
                        if (destinationCircle == null) continue;
                        string dimId = ModeDRAW.CreateMuayeneBacasiDimension(lastCircle, destinationCircle);
                        executeDrawCommandTransaction(db, lastCircle, destinationCircle, dimId);
                        continue;
                    }
                }
                
                for (int i = 0; i < circlesInHat.Count; i++)
                {
                    Circle circle1 = null;
                    Circle circle2 = null;
                    if (i == circlesInHat.Count - 1)
                    {
                        List<Circle> circlesExcludedCurrentHat = circles.Where(circle => !circlesInHat.Contains(circle)).ToList();
                        if (circlesExcludedCurrentHat.Count == 0) break;

                        circle1 = circlesInHat[i];
                        circle2 = ModeDRAW.findNearestCircleToSourceCircle(circlesInHat[i], circlesExcludedCurrentHat);

                        if (circle2 == null) break;

                        // Prompt user for confirmation
                        Xrecord xRecordOfCircle1 = Utils.fetchXrecordOfObject(circle1.ObjectId);
                        TypedValue[] valuesOfCircle1 = xRecordOfCircle1.Data.AsArray();

                        Xrecord xRecordOfCircle2 = Utils.fetchXrecordOfObject(circle2.ObjectId);
                        TypedValue[] valuesOfCircle2 = xRecordOfCircle2.Data.AsArray();

                        PromptKeywordOptions confirmOptions = new PromptKeywordOptions($"\nThe circle NO: {valuesOfCircle2[7].Value.ToString()} will be selected as the source circle for the last 'muayene bacası'." +
                            $"{valuesOfCircle1[7].Value.ToString()}  Do you want to proceed?");
                        confirmOptions.Keywords.Add("OK");
                        confirmOptions.Keywords.Add("Cancel");
                        confirmOptions.AllowNone = false;

                        PromptResult confirmResult = ed.GetKeywords(confirmOptions);

                        if (confirmResult.Status != PromptStatus.OK || confirmResult.StringResult == "Cancel")
                        {
                            // Request user to select a different circle
                            PromptEntityOptions entityOptions = new PromptEntityOptions("\nPlease select a circle to redirect:");
                            entityOptions.SetRejectMessage("\nOnly circles are allowed.");
                            entityOptions.AddAllowedClass(typeof(Circle), true);

                            PromptEntityResult entityResult = ed.GetEntity(entityOptions);

                            if (entityResult.Status == PromptStatus.OK)
                            {
                                using (Transaction tr = db.TransactionManager.StartTransaction())
                                {
                                    circle2 = tr.GetObject(entityResult.ObjectId, OpenMode.ForRead) as Circle;
                                    tr.Commit();
                                }
                            }
                            else
                            {
                                break; // Exit if no valid circle is selected
                            }
                        }
                    }
                    else
                    {
                        circle1 = circlesInHat[i];
                        circle2 = circlesInHat[i+1];
                    }
                    string dimId = ModeDRAW.CreateMuayeneBacasiDimension(circle1, circle2);
                    executeDrawCommandTransaction(db, circle1, circle2, dimId);
                }
            }

        }

        [CommandMethod("calculateSystem")]
        public void calculateSystem()
        {
            if(!(mode is ModeCALCULATE))
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                ed.WriteMessage("\nThis command is only available in CALCULATE mode.");
                return;
            }
            ModeCALCULATE.calculateSystem();
        }

        [CommandMethod("CheckMode")]
        public void checkMode()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ed.WriteMessage($"\nCurrent mode: {mode.GetType().Name}");
        }

        [CommandMethod("boyProfil")]
        public void boyProfil()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            doc.Editor.UpdateScreen();
            List<Circle> circles = RetrieveHatWithCircles();
            List<BoyProfilCircle> boyProfilCircles = new List<BoyProfilCircle>();
            for (int i=0; i < circles.Count; i++) 
            {
                if(i==circles.Count - 1)
                {
                    boyProfilCircles.Add(BoyProfilUiAutomation.MapInternalCircleToBoyProfilCircle(circles[i], true));
                }
                else
                {
                    boyProfilCircles.Add(BoyProfilUiAutomation.MapInternalCircleToBoyProfilCircle(circles[i], false));
                }
            }
            ed.WriteMessage("starting serialization");
            BoyProfilUiAutomation.SerializeAndSaveBoyProfilCirclesFile(boyProfilCircles);
            BoyProfilUiAutomation.StartBoyProfilMakro();
        }

        [CommandMethod("boyProfildesktop")]
        public void boyProfildesktop()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            try
            {
                // Force AutoCAD to be active and ready
                doc.Editor.UpdateScreen();

                List<Circle> circles = RetrieveHatWithCircles();
                if (circles == null)
                {
                    ed.WriteMessage("\nOperation cancelled or no valid circles found.");
                    return;
                }

                List<BoyProfilCircle> boyProfilCircles = new List<BoyProfilCircle>();
                for (int i = 0; i < circles.Count; i++)
                {
                    boyProfilCircles.Add(BoyProfilUiAutomation.MapInternalCircleToBoyProfilCircle(circles[i], false));
                }

                ed.WriteMessage("starting serialization");
                BoyProfilUiAutomation.SerializeAndSaveBoyProfilCirclesFile(boyProfilCircles);
                BoyProfilUiAutomation.StartBoyProfilMakro();
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError in boyProfil: {ex.Message}");
            }
        }

        [CommandMethod("Revert")]
        public void Revert()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (mode is ModeREVERT)
            {
                PromptKeywordOptions options = new PromptKeywordOptions("\nDimension çizimleri de silinsin mi ? : ");
                options.AllowNone = false;
                options.Keywords.Add("EVET");
                options.Keywords.Add("HAYIR");

                PromptResult result = ed.GetKeywords(options);

                if (result.Status != PromptStatus.OK)
                    return;

                bool isRevertDimensions = false;

                if (result.StringResult == "EVET")
                {
                    isRevertDimensions = true;
                }

                ModeREVERT.RevertPastOperations(isRevertDimensions);

            }
            else
            {
                ed.WriteMessage("\nThis command is only available in REVERT mode.");
            }
        }

        [CommandMethod("ManuelDimension")]
        public void ManuelDimension()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            if (!(mode is ModeDRAW))
            {
                ed.WriteMessage("\nThis command is only available in DRAW mode.");
                return;
            }

            // Ask user to pick a point
            PromptPointOptions pointOptions = new PromptPointOptions("\nSelect a point:");
            PromptPointResult pointResult = ed.GetPoint(pointOptions);

            if (pointResult.Status != PromptStatus.OK)
                return;

            Point3d selectedPoint = pointResult.Value;

            // Ask user to select a circle
            PromptEntityOptions entityOptions = new PromptEntityOptions("\nSelect a circle for dimension");
            entityOptions.SetRejectMessage("\nOnly circles allowed.");
            entityOptions.AddAllowedClass(typeof(Circle), true);

            PromptEntityResult entityResult = ed.GetEntity(entityOptions);

            if (entityResult.Status != PromptStatus.OK)
                return;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Circle selectedCircle = (Circle)tr.GetObject(entityResult.ObjectId, OpenMode.ForRead);
                if (selectedCircle == null)
                {
                    ed.WriteMessage("\nSelected entity is not a circle.");
                    return;
                }
                // Create dimension
                ModeDRAW.CreateCustomArrowBlockIfNotExists(db, tr, "custom-arrow-1", 6);
                AlignedDimension dim = ModeDRAW.CreateMuayeneBacasiDimension(selectedCircle, selectedPoint);
                if (dim == null)
                {
                    ed.WriteMessage("\nFailed to create dimension.");
                    return;
                }
                // Execute transaction to update Xrecord
                executeDrawCommandTransaction(db, selectedCircle, selectedCircle, dim.Handle.ToString());

                // Ask user to in-flow out-flow slope
                PromptStringOptions outFlowOptions = new PromptStringOptions("\nEnter Out-Flow value:");
                outFlowOptions.AllowSpaces = true;
                PromptResult outFlowResult = ed.GetString(outFlowOptions);
                if (outFlowResult.Status != PromptStatus.OK)
                    return;

                PromptStringOptions slopeOptions = new PromptStringOptions("\nEnter Slope value:");
                slopeOptions.AllowSpaces = true;
                PromptResult slopeResult = ed.GetString(slopeOptions);
                if (slopeResult.Status != PromptStatus.OK)
                    return;


                PromptStringOptions inFlowOptions = new PromptStringOptions("\nEnter In-Flow value:");
                inFlowOptions.AllowSpaces = true;
                PromptResult inFlowResult = ed.GetString(inFlowOptions);
                if (inFlowResult.Status != PromptStatus.OK)
                    return;


                ModeDRAW.CreateAndAlignTextsForParselDimension(slopeResult.StringResult, inFlowResult.StringResult, outFlowResult.StringResult, dim); 


                tr.Commit();
            }


        }

        [CommandMethod("ShowHandle")]
        public void ShowHandle()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            // Ask user to select an entity
            PromptEntityOptions entityOptions = new PromptEntityOptions("\nSelect an entity to get its handle:");
            entityOptions.SetRejectMessage("\nOnly entities allowed.");
            PromptEntityResult entityResult = ed.GetEntity(entityOptions);
            if (entityResult.Status == PromptStatus.OK)
            {
                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    Entity selectedEntity = (Entity)tr.GetObject(entityResult.ObjectId, OpenMode.ForRead);
                    ed.WriteMessage($"\nSelected entity handle: {selectedEntity.Handle}");
                    tr.Commit();
                }
            }
        }   

        [CommandMethod("DefineMainHat")]
        public void DefineMainHat()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            List<Circle> circles = RetrieveHatWithCircles();
            if (circles == null || circles.Count == 0)
            {
                ed.WriteMessage("\nNo valid circles found.");
                return;
            }
            
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (Circle circle in circles)
                {
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);
                    Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(circle.Handle.ToString()), OpenMode.ForWrite);
                    TypedValue[] values = xRecord.Data.AsArray();
                    values[17] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, "true");
                    xRecord.Data = new ResultBuffer(values);
                }
                tr.Commit();
            }
            ed.WriteMessage("\nMain hat defined successfully.");
        }


        [CommandMethod("ResetZCoordinate")]
        public void ResetZCoordinate()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            List<ObjectId> circleIds = GetAllCirclesOnLayer(db, "K_HAT");

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId circleId in circleIds)
                {
                    Circle circle = tr.GetObject(circleId, OpenMode.ForWrite) as Circle;
                    if (circle != null)
                    {
                        double z = circle.Center.Z;
                        if (Math.Abs(z) > Tolerance.Global.EqualPoint)
                        {
                            Matrix3d translation = Matrix3d.Displacement(new Vector3d(0, 0, -z));
                            circle.TransformBy(translation);
                        }
                    }
                }
                tr.Commit();
            }

            ed.WriteMessage($"\nReset Z of {circleIds.Count} circles on layer K_HAT.");
        }



        private static List<Circle> RetrieveHatWithCircles()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            string targetHatId = "";

            // Ask user to select a circle OR enter hat_id manually
            PromptEntityOptions entityOptions = new PromptEntityOptions("\nSelect a circle to find matching hat_id or press Enter to input manually:");
            entityOptions.SetRejectMessage("\nOnly circles allowed.");
            entityOptions.AddAllowedClass(typeof(Circle), true);

            PromptEntityResult entityResult = ed.GetEntity(entityOptions);

            Circle selectedCircle = null;

            if (entityResult.Status == PromptStatus.OK)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    selectedCircle = tr.GetObject(entityResult.ObjectId, OpenMode.ForRead) as Circle;
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                    if (nod.Contains(selectedCircle.Handle.ToString()))
                    {
                        Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(selectedCircle.Handle.ToString()), OpenMode.ForRead);
                        TypedValue[] values = xRecord.Data.AsArray();
                        for (int i = 0; i < values.Length - 1; i++)
                        {
                            if (values[i].Value.ToString() == "class")
                            {
                                if (values[i + 1].Value.ToString() != "muayene_bacası")
                                {
                                    ed.WriteMessage("\nSelected circle is not a Muayene Bacası.");
                                    tr.Commit();
                                    return null;
                                }
                            }
                        }
                    }
                }
            }

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                if (entityResult.Status == PromptStatus.OK)
                {
                    // Retrieve hat_id from selected circle
                    ObjectId selectedId = entityResult.ObjectId;

                    if (nod.Contains(selectedId.Handle.ToString()))
                    {
                        Xrecord xRecord = (Xrecord)tr.GetObject(nod.GetAt(selectedId.Handle.ToString()), OpenMode.ForRead);
                        TypedValue[] values = xRecord.Data.AsArray();

                        for (int i = 0; i < values.Length - 1; i++)
                        {
                            if (values[i].Value.ToString() == "Mb_hat_id")
                            {
                                targetHatId = values[i + 1].Value.ToString();
                                break;
                            }
                        }
                    }
                }
                else if (entityResult.Status == PromptStatus.None)
                {
                    // User wants to manually input hat_id
                    PromptIntegerResult hatIdInput = ed.GetInteger("\nEnter Hat ID:");
                    if (hatIdInput.Status == PromptStatus.OK)
                        targetHatId = hatIdInput.Value.ToString();
                }

                if (targetHatId == "")
                {
                    ed.WriteMessage("\nNo valid hat_id found.");
                    return null;
                }

                tr.Commit();
            }

            // Search for all circles with the same hat_id
            List<Circle> allCircles = ModeMB.GetAllMuayeneBacasiCircles(db);
            Dictionary<string, List<Circle>> categorizedCircles = ModeMB.categorizeMuayeneBacasiCirclesByHatId(allCircles);

            foreach (var kvp in categorizedCircles)
            {
                if (kvp.Key == targetHatId)
                {
                    List<Circle> circles = kvp.Value;

                    /* If the last circle in the list has an out dimension, fetch the in-circle from that dimension */
                    /* Circle circle = circles[circles.Count - 1];
                    if (Utils.fetchXrecordOfObject(circle.ObjectId).Data.AsArray()[13].Value.ToString() != "-1")
                    {
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            string outDimHandle = Utils.fetchXrecordOfObject(circle.ObjectId).Data.AsArray()[13].Value.ToString();
                            ObjectId outDimObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(outDimHandle, 16)), 0);
                            TypedValue[] outDimData = ((Xrecord)Utils.fetchXrecordOfObject(outDimObjectId)).Data.AsArray();
                            string inCircleHandle = outDimData[3].Value.ToString();
                            ObjectId inCircleObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inCircleHandle, 16)), 0);
                            Circle inCircle = (Circle)tr.GetObject(inCircleObjectId, OpenMode.ForRead);
                            circles.Add(inCircle);
                        }
                    } */

                    int index = circles.FindIndex(c => c.Handle == selectedCircle.Handle);
                    int lastIndex = index + 14; // automation is allowed to go 15 circles ahead
                    if (lastIndex >= circles.Count)
                    {
                        lastIndex = circles.Count - 1; // Ensure we don't go out of bounds
                    }
                    return circles.GetRange(index, lastIndex - index + 1);
                }
            }
            return null;
        }

        private void executeDrawCommandTransaction(Database db, Circle circle1, Circle circle2, string dimId)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                Xrecord xRecordOfCircle1 = (Xrecord)tr.GetObject(nod.GetAt(circle1.Handle.ToString()), OpenMode.ForWrite);
                Xrecord xRecordOfCircle2 = (Xrecord)tr.GetObject(nod.GetAt(circle2.Handle.ToString()), OpenMode.ForWrite);

                TypedValue[] updatedValue1 = xRecordOfCircle1.Data.AsArray();
                TypedValue[] updatedValue2 = xRecordOfCircle2.Data.AsArray();

                updatedValue1[13] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, dimId);

                string existingDimId = updatedValue2[15].Value.ToString();
                string newDimIdValue = existingDimId == "-1" ? dimId : existingDimId + "%" + dimId;
                updatedValue2[15] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, newDimIdValue);

                xRecordOfCircle1.Data = new ResultBuffer(updatedValue1.Select(v => new TypedValue(v.TypeCode, v.Value)).ToArray());
                xRecordOfCircle2.Data = new ResultBuffer(updatedValue2.Select(v => new TypedValue(v.TypeCode, v.Value)).ToArray());

                ModeDRAW.AlignTextWithDimension(circle1, db, true);

                tr.Commit();
            }
        }

        private static List<ObjectId> GetAllCirclesOnLayer(Database db, string layerName)
        {
            List<ObjectId> circleIds = new List<ObjectId>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (ent is Circle circle && ent.Layer == layerName)
                    {
                        circleIds.Add(objId);
                    }
                }

                tr.Commit();
            }

            return circleIds;
        }

    }
}
