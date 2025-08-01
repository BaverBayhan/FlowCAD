using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace FlowCAD_core
{
    internal class ModeController
    {
        public static bool isTransitionPossible(Mode currentMode, Mode nextMode)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            if (currentMode is ModeMB)
            {
                if (nextMode is ModeKKY || nextMode is ModeDRAW)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (currentMode is ModeKKY)
            {
                if (nextMode is ModeFMKNA)
                {
                    List<Circle> muayeneBacasiCircles = ModeMB.GetMuayeneBacasiCirclesWithoutIzahatCemberi(db);
                    foreach (Circle circle in muayeneBacasiCircles)
                    {
                        Xrecord xrecord = Utils.fetchXrecordOfObject(circle.ObjectId);
                        TypedValue[] data = xrecord.Data.AsArray();
                        if (data[9].Value.ToString() == "-1")
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    if(nextMode is ModeMB)
                    {
                        return true;
                    }
                    return false;
                }
            }
            else if (currentMode is ModeFMKNA)
            {
                if (nextMode is ModeAKARKOT)
                {
                    List<Circle> izahatCemberiCircles = ModeKKY.GetIzahatCemberiCircles(db);
                    foreach (Circle circle in izahatCemberiCircles)
                    {
                        Xrecord xrecord = Utils.fetchXrecordOfObject(circle.ObjectId);
                        TypedValue[] data = xrecord.Data.AsArray();
                        if (data[7].Value.ToString() == "-1")
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    if (nextMode is ModeMB)
                    {
                        return true;
                    }
                    return false;
                }
            }
            else if (currentMode is ModeAKARKOT)
            {
                if(nextMode is ModeDRAW)
                {
                    List<Circle> muayeneBacasiCircles = ModeMB.GetMuayeneBacasiCirclesWithoutIzahatCemberi(db);
                    foreach (Circle circle in muayeneBacasiCircles)
                    {
                        Xrecord xrecord = Utils.fetchXrecordOfObject(circle.ObjectId);
                        TypedValue[] data = xrecord.Data.AsArray();
                        if (data[11].Value.ToString() == "-1")
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    if (nextMode is ModeMB)
                    {
                        return true;
                    }
                    return false;
                }
            } else if(currentMode is ModeDRAW)
            {
                if (nextMode is ModeCALCULATE)
                {
                    List<Circle> muayeneBacasiCircles = ModeMB.GetMuayeneBacasiCirclesWithoutIzahatCemberi(db);
                    foreach (Circle circle in muayeneBacasiCircles)
                    {
                        Xrecord xrecord = Utils.fetchXrecordOfObject(circle.ObjectId);
                        TypedValue[] data = xrecord.Data.AsArray();
                        if (data[13].Value.ToString() == "-1")
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    if (nextMode is ModeMB)
                    {
                        return true;
                    }
                    return false;
                }
            } else if(currentMode is ModeCALCULATE)
            {
                if(nextMode is ModeMB || nextMode is ModeREVERT)
                {
                    return true;
                } else { return false; }
            }
            else if (currentMode is ModeREVERT)
            {
                if (nextMode is ModeMB || nextMode is ModeCALCULATE || nextMode is ModeDRAW)
                {
                    return true;
                }
                else { return false; }
            }
            else
            {
                return false;
            }
        }
    }
}
