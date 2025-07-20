using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;

namespace FlowCAD_core
{
    internal class ModeREVERT : Mode
    {
        public static void RevertPastOperations(bool isRevertDimensions)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            List<Circle> muayeneBacasiCircles = ModeMB.GetAllMuayeneBacasiCircles(db);
            Dictionary<string, List<Circle>> groupedCircles = ModeMB.categorizeMuayeneBacasiCirclesByHatId(muayeneBacasiCircles);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (List<Circle> hatCircles in groupedCircles.Values)
                {
                    for(int i = 0; i<hatCircles.Count; i++)
                    {
                        AlignedDimension outDimensionOfCircle = ModeMB.RetrieveOutDimensionOfMuayeneBacasi(hatCircles[i]);
                        if (outDimensionOfCircle == null)
                        {
                            ed.WriteMessage("\n No out dimension found for the first muayene bacası circle.");
                            continue;
                        }
                        EraseSlopeText(ed, outDimensionOfCircle);
                        EraseInFlowText(ed, outDimensionOfCircle);
                        if (i != 0)
                        {
                            EraseOutFlowText(ed, outDimensionOfCircle);
                        } 
                    }
                }
                tr.Commit();
            }

            if (isRevertDimensions)
            {
                foreach (var circle in muayeneBacasiCircles)
                {
                    AlignedDimension dim = ModeMB.RetrieveOutDimensionOfMuayeneBacasi(circle);
                    if (dim != null)
                    {
                        using (var tr2 = db.TransactionManager.StartTransaction())
                        {
                            var dimForWrite = tr2.GetObject(dim.ObjectId, OpenMode.ForWrite, false) as AlignedDimension;
                            if (dimForWrite != null)
                            {
                                dimForWrite.Erase();
                            }
                            tr2.Commit();
                        }
                    }
                    else
                    {
                        ed.WriteMessage("\n No out dimension found for a muayene bacası circle.");
                    }
                }

                foreach (List<Circle> hatCircles in groupedCircles.Values)
                {
                    for (int i = 0; i<hatCircles.Count; i++)
                    {
                        ModeMB.ResetInDimensionXrecordDataOfMuayeneBacasi(hatCircles[i]);
                        ModeMB.ResetOutDimensionXrecordDataOfMuayeneBacasi(hatCircles[i]);
                        if (i != 0)
                        {
                            ModeMB.ResetAkarKotTextXrecordDataOfMuayeneBacasi(hatCircles[i]);
                        }
                    }
                }
            }

        }

        private static void EraseSlopeText(Editor ed, AlignedDimension outDimensionOfCircle)
        {
            DBText slopeText = ModeDRAW.GetSlopeTextOfDimension(outDimensionOfCircle);
            if (slopeText != null)
            {
                slopeText.Erase();
                ModeDRAW.ResetSlopeXrecordDataOfDimension(outDimensionOfCircle, "-1");
            }
            else
            {
                ed.WriteMessage("\n No slope text found for the out dimension of the first muayene bacası circle.");
            }
        }

        private static void EraseOutFlowText(Editor ed, AlignedDimension outDimensionOfCircle)
        {
            DBText outFlowText = ModeDRAW.GetOutFlowTextOfDimension(outDimensionOfCircle);
            if (outFlowText != null)
            {
                outFlowText.Erase();
                ModeDRAW.ResetOutFlowXrecordDataOfDimension(outDimensionOfCircle, "-1");
            }
            else
            {
                ed.WriteMessage("\n No out flow text found for the out dimension of the first muayene bacası circle.");
            }
        }

        private static void EraseInFlowText(Editor ed, AlignedDimension outDimensionOfCircle)
        {
            DBText inFlowText = ModeDRAW.GetInFlowTextOfDimension(outDimensionOfCircle);
            if (inFlowText != null)
            {
                inFlowText.Erase();
                ModeDRAW.ResetInFlowXrecordDataOfDimension(outDimensionOfCircle, "-1");
            }
            else
            {
                ed.WriteMessage("\n No in flow text found for the out dimension of the first muayene bacası circle.");
            }
        }

    }
}
