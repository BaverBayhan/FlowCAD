using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace FlowCAD_core
{
    internal class ModeCALCULATE : Mode
    {

        public ModeCALCULATE() { }

        private static readonly double MIN_BACA_DERINLIGI = 1.65;
        private static readonly double MAX_SLOPE_DENOMINATOR_FOR_200_BB = 200;

        private static void makeHatCalculation(string hatId, List<Circle> circlesInHat, Dictionary<string, List<Circle>> categorizedCircles)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            /* Calculate hat */
            for (int i = 0; i <= circlesInHat.Count - 1; i++)
            {
                Circle outCircle = circlesInHat[i];
                Circle inCircle;

                if (i != circlesInHat.Count - 1)
                {
                    inCircle = circlesInHat[i + 1];
                }
                else
                {
                    TypedValue[] outCircleData = ((Xrecord)Utils.fetchXrecordOfObject(outCircle.ObjectId)).Data.AsArray();
                    string outCircleOutDimensionHandleString = outCircleData[13].Value.ToString();
                    if (outCircleOutDimensionHandleString == "-1")
                    {
                        return;
                    }
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        ObjectId outCircleOutDimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(outCircleOutDimensionHandleString, 16)), 0);
                        AlignedDimension outCircleOutDimension = (AlignedDimension)tr.GetObject(outCircleOutDimensionObjectId, OpenMode.ForRead);
                        TypedValue[] outCircleOutDimensionData = ((Xrecord)Utils.fetchXrecordOfObject(outCircleOutDimensionObjectId)).Data.AsArray();
                        string inCircleInAnotherHatHandleString = outCircleOutDimensionData[3].Value.ToString();
                        ObjectId inCircleInAnotherHatObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inCircleInAnotherHatHandleString, 16)), 0);
                        inCircle = (Circle)inCircleInAnotherHatObjectId.GetObject(OpenMode.ForWrite);
                    }

                }


                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                    Xrecord outCircleXrecord = (Xrecord)tr.GetObject(nod.GetAt(outCircle.Handle.ToString()), OpenMode.ForWrite);
                    Xrecord inCircleXrecord = (Xrecord)tr.GetObject(nod.GetAt(inCircle.Handle.ToString()), OpenMode.ForWrite);

                    TypedValue[] outCircleXrecordValues = outCircleXrecord.Data.AsArray();
                    TypedValue[] inCircleXrecordValues = inCircleXrecord.Data.AsArray();

                    /* Retrieve Dimension information */
                    string dimensionHandleString = outCircleXrecordValues[13].Value.ToString();
                    ObjectId dimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(dimensionHandleString, 16)), 0);
                    AlignedDimension dimension = (AlignedDimension)tr.GetObject(dimensionObjectId, OpenMode.ForRead);
                    double distanceValue = dimension.Measurement;
                    Xrecord dimensionXrecord = (Xrecord)tr.GetObject(nod.GetAt(dimension.Handle.ToString()), OpenMode.ForWrite);
                    TypedValue[] dimensionXrecordValues = dimensionXrecord.Data.AsArray();

                    /* Assign akarkot of out circle */
                    string akarkotOfOutCircle;
                    string akarkotTextHandle;
                    if (i>0)
                    {
                        string inDimensions1 = outCircleXrecordValues[15].Value.ToString();
                        if (inDimensions1.Contains("%"))
                        {
                            List<string> inDimensionHandles = inDimensions1.Split('%').ToList();
                            List<double> inDimensionInFlowValues = new List<double>();
                            foreach (string inDimensionHandle in inDimensionHandles)
                            {
                                ObjectId inDimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inDimensionHandle, 16)), 0);
                                AlignedDimension inDimension = (AlignedDimension)tr.GetObject(inDimensionObjectId, OpenMode.ForRead);
                                TypedValue[] inDimensionXrecordValues = ((Xrecord)tr.GetObject(nod.GetAt(inDimension.Handle.ToString()), OpenMode.ForWrite)).Data.AsArray();
                                string inDimensionInFlowTextHandle = inDimensionXrecordValues[9].Value.ToString();
                                ObjectId inDimensionInFlowTextObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inDimensionInFlowTextHandle, 16)), 0);
                                DBText inDimensionInFlowText = (DBText)tr.GetObject(inDimensionInFlowTextObjectId, OpenMode.ForRead);
                                string inDimensionInFlow = inDimensionInFlowText.TextString;
                                inDimensionInFlowValues.Add(Convert.ToDouble(inDimensionInFlow));
                            }
                            double minInfowValue = inDimensionInFlowValues.Min();
                            DBText akarkotTextOfOutCircle = ModeAKARKOT.createAkarkotTextAlignedWithDimension(dimension, minInfowValue.ToString(), true);
                            btr.AppendEntity(akarkotTextOfOutCircle);
                            tr.AddNewlyCreatedDBObject(akarkotTextOfOutCircle, true);
                            outCircleXrecordValues[11] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, akarkotTextOfOutCircle.Handle.ToString());
                            ResultBuffer updatedOutCircleResultBuffer = new ResultBuffer(outCircleXrecordValues);
                            outCircleXrecord.Data = updatedOutCircleResultBuffer;
                            akarkotOfOutCircle = minInfowValue.ToString();
                            akarkotTextHandle = akarkotTextOfOutCircle.Handle.ToString();

                        }
                        else
                        {
                            ObjectId inDimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inDimensions1, 16)), 0);
                            AlignedDimension inDimension = (AlignedDimension)tr.GetObject(inDimensionObjectId, OpenMode.ForRead);
                            TypedValue[] inDimensionXrecordValues = ((Xrecord)tr.GetObject(nod.GetAt(inDimension.Handle.ToString()), OpenMode.ForWrite)).Data.AsArray();
                            string inDimensionInFlowTextHandle = inDimensionXrecordValues[9].Value.ToString();
                            ObjectId inDimensionInFlowTextObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inDimensionInFlowTextHandle, 16)), 0);
                            DBText inDimensionInFlowText = (DBText)tr.GetObject(inDimensionInFlowTextObjectId, OpenMode.ForRead);
                            string inDimensionInFlow = inDimensionInFlowText.TextString;
                            DBText akarkotTextOfOutCircle = ModeAKARKOT.createAkarkotTextAlignedWithDimension(dimension, inDimensionInFlow, true);
                            btr.AppendEntity(akarkotTextOfOutCircle);
                            tr.AddNewlyCreatedDBObject(akarkotTextOfOutCircle, true);
                            outCircleXrecordValues[11] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, akarkotTextOfOutCircle.Handle.ToString());
                            ResultBuffer updatedOutCircleResultBuffer = new ResultBuffer(outCircleXrecordValues);
                            outCircleXrecord.Data = updatedOutCircleResultBuffer;
                            akarkotOfOutCircle = inDimensionInFlow;
                            akarkotTextHandle = akarkotTextOfOutCircle.Handle.ToString();
                        }
                    }
                    else
                    {
                        /* Retrieve akarkot of out circle for first circle in hat*/
                        string akarkotOfOutCircleHandleString = outCircleXrecordValues[11].Value.ToString();
                        ObjectId akarkotTextObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(akarkotOfOutCircleHandleString, 16)), 0);
                        DBText akarkotText = (DBText)tr.GetObject(akarkotTextObjectId, OpenMode.ForRead);
                        akarkotOfOutCircle = akarkotText.TextString;
                        akarkotTextHandle = akarkotText.Handle.ToString();
                    }

                    /* Retrieve kapak kot of in circle */
                    string IzahatCemberiOfInCircleHandleString = inCircleXrecordValues[9].Value.ToString();
                    ed.WriteMessage(IzahatCemberiOfInCircleHandleString);
                    ObjectId IzahatCemberiObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(IzahatCemberiOfInCircleHandleString, 16)), 0);
                    Circle izahateCemberiInCircle = (Circle)tr.GetObject(IzahatCemberiObjectId, OpenMode.ForRead);
                    ed.WriteMessage(izahateCemberiInCircle.Handle.ToString());
                    TypedValue[] izahatCemberiInCircleXrecord = ((Xrecord)tr.GetObject(nod.GetAt(izahateCemberiInCircle.Handle.ToString()), OpenMode.ForWrite)).Data.AsArray();
                    string kapakKotOfInCircleHandleString = izahatCemberiInCircleXrecord[7].Value.ToString();
                    ObjectId kapakKotOfInCircleObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(kapakKotOfInCircleHandleString, 16)), 0);
                    DBText kapakKotOfInCircleText = (DBText)tr.GetObject(kapakKotOfInCircleObjectId, OpenMode.ForRead);
                    string kapakKotOfInCircle = kapakKotOfInCircleText.TextString;


                    /* assign out-flow to dimension*/
                    dimensionXrecordValues[7] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, akarkotTextHandle);
                    ResultBuffer updatedDimensionResultBuffer = new ResultBuffer(dimensionXrecordValues);
                    dimensionXrecord.Data = updatedDimensionResultBuffer;

                    /* Calculate for circle */
                    ed.WriteMessage("akarkot of out circle : "+akarkotOfOutCircle + "\n");
                    double akarkotOfOutCircleDouble = Convert.ToDouble(akarkotOfOutCircle);
                    double kapakKotOfInCircleDouble = Convert.ToDouble(kapakKotOfInCircle.Substring(1, kapakKotOfInCircle.Length-2));
                    double slopeDenominatorValue = Math.Floor(distanceValue / ((akarkotOfOutCircleDouble-kapakKotOfInCircleDouble)+MIN_BACA_DERINLIGI));

                    if (slopeDenominatorValue < 0)
                    {
                        slopeDenominatorValue = MAX_SLOPE_DENOMINATOR_FOR_200_BB;
                    }
                    double akarkotInCircle = akarkotOfOutCircleDouble - ((1/slopeDenominatorValue) * distanceValue);
                    ed.WriteMessage("akarkot in circle : "+akarkotInCircle.ToString() + "\n");


                    /* Update Drawings and Db */
                    DBText akarkotTextOfInCircle = ModeAKARKOT.createAkarkotTextAlignedWithDimension(dimension, akarkotInCircle.ToString("F2"), false);
                    btr.AppendEntity(akarkotTextOfInCircle);
                    tr.AddNewlyCreatedDBObject(akarkotTextOfInCircle, true);

                    // Assign in-flow to dimension
                    dimensionXrecordValues[9] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, akarkotTextOfInCircle.Handle.ToString());
                    ResultBuffer updatedDimensionResultBuffer0 = new ResultBuffer(dimensionXrecordValues);
                    dimensionXrecord.Data = updatedDimensionResultBuffer0;

                    // Calculate the midpoint of the dimension line
                    Point3d midpoint = new Point3d(
                        (dimension.XLine1Point.X + dimension.XLine2Point.X) / 2,
                        (dimension.XLine1Point.Y + dimension.XLine2Point.Y) / 2,
                        0
                    );

                    // Calculate the direction vector (perpendicular to dimension line)
                    Vector3d direction = (dimension.XLine2Point - dimension.XLine1Point).GetPerpendicularVector().GetNormal();
                    Vector3d offset = direction * -1.0;  // Adjust offset as needed for vertical distance below

                    // Calculate dimension line direction (not the perpendicular)
                    Vector3d dimLineDirection = (dimension.XLine2Point - dimension.XLine1Point).GetNormal();

                    // Calculate rotation angle in radians
                    double rotationAngle = Math.Atan2(dimLineDirection.Y, dimLineDirection.X);

                    // New text position (slightly below the dimension line midpoint)
                    Point3d textPosition = midpoint + offset;
                    

                    DBText slopeText = new DBText
                    {
                        Position = textPosition,
                        TextString = string.Format("1/{0}", (slopeDenominatorValue).ToString()),
                        Height = 0.65,
                        WidthFactor = 1,
                        Rotation = rotationAngle,  // Apply rotation to align with dimension line
                        Justify = AttachmentPoint.MiddleCenter,
                        Color = Color.FromColorIndex(ColorMethod.ByAci, 2)
                    };
                    slopeText.Annotative = AnnotativeStates.False;
                    slopeText.HorizontalMode = TextHorizontalMode.TextMid;
                    slopeText.VerticalMode = TextVerticalMode.TextVerticalMid;
                    slopeText.AlignmentPoint = textPosition;

                    btr.AppendEntity(slopeText);
                    tr.AddNewlyCreatedDBObject(slopeText, true);

                    dimensionXrecordValues[5] = new TypedValue((int)DxfCode.ExtendedDataAsciiString, slopeText.Handle.ToString());
                    ResultBuffer updatedDimensionResultBuffer1 = new ResultBuffer(dimensionXrecordValues);
                    dimensionXrecord.Data = updatedDimensionResultBuffer1;
                    tr.Commit();
                }
            }
        }

        private static void retrieveDependentHatsOfMainHat(List<Circle> circlesInHat, Dictionary<string, List<Circle>> categorizedCircles, Queue<List<Circle>> dependentHats)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            /* Case 1 : mid-circles contains in-dimensions from other hats*/
            for (int i = 0; i <= circlesInHat.Count - 1; i++)
            {
                Xrecord xrecord = Utils.fetchXrecordOfObject(circlesInHat[i].ObjectId);
                TypedValue[] xrecordValues = xrecord.Data.AsArray();
                string inDimensionHandleString = xrecordValues[15].Value.ToString();
                string inCircleHatId = xrecordValues[5].Value.ToString();
                if (inDimensionHandleString.Contains("%"))
                {
                    List<string> inDimensionHandles = inDimensionHandleString.Split('%').ToList();
                    foreach (string inDimensionHandle in inDimensionHandles)
                    {
                        string outCircleHatId;
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {                        
                            ObjectId inDimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inDimensionHandle, 16)), 0);
                            AlignedDimension inDimension = (AlignedDimension)tr.GetObject(inDimensionObjectId, OpenMode.ForRead);
                            TypedValue[] inDimensionXrecordValues = ((Xrecord)Utils.fetchXrecordOfObject(inDimension.ObjectId)).Data.AsArray();
                            string outCircleHandleString = inDimensionXrecordValues[1].Value.ToString();
                            if (outCircleHandleString == "PARSEL")
                            {
                                continue;
                            }
                            ObjectId outCircleObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(outCircleHandleString, 16)), 0);
                            Circle outCircle = (Circle)tr.GetObject(outCircleObjectId, OpenMode.ForRead);
                            TypedValue[] outCircleXrecordValues = ((Xrecord)Utils.fetchXrecordOfObject(outCircle.ObjectId)).Data.AsArray();
                            outCircleHatId = outCircleXrecordValues[5].Value.ToString();
                            tr.Commit();
                        }

                        if (categorizedCircles.ContainsKey(outCircleHatId) && inCircleHatId != outCircleHatId)
                        {
                            List<Circle> dependentHat = categorizedCircles[outCircleHatId];
                            dependentHats.Enqueue(dependentHat);
                            retrieveDependentHatsOfMainHat(dependentHat, categorizedCircles, dependentHats);
                        }
                    }
                }
            }

            /* Case 2 : init-circle contains an in-dimension from other hats*/
            Xrecord xrecordOfInitCircle = Utils.fetchXrecordOfObject(circlesInHat[0].ObjectId);
            TypedValue[] xrecordValuesOfInitCircle = xrecordOfInitCircle.Data.AsArray();
            string inDimensionHandleStringOfInitCircle = xrecordValuesOfInitCircle[15].Value.ToString();
            if (inDimensionHandleStringOfInitCircle != "-1")
            {
                string outCircleHatId;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    ObjectId inDimensionObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(inDimensionHandleStringOfInitCircle, 16)), 0);
                    AlignedDimension inDimension = (AlignedDimension)tr.GetObject(inDimensionObjectId, OpenMode.ForRead);
                    TypedValue[] inDimensionXrecordValues = ((Xrecord)Utils.fetchXrecordOfObject(inDimension.ObjectId)).Data.AsArray();
                    string outCircleHandleString = inDimensionXrecordValues[1].Value.ToString();
                    ObjectId outCircleObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(outCircleHandleString, 16)), 0);
                    Circle outCircle = (Circle)tr.GetObject(outCircleObjectId, OpenMode.ForRead);
                    TypedValue[] outCircleXrecordValues = ((Xrecord)Utils.fetchXrecordOfObject(outCircle.ObjectId)).Data.AsArray();
                    outCircleHatId = outCircleXrecordValues[5].Value.ToString();
                    tr.Commit();
                }

                if (categorizedCircles.ContainsKey(outCircleHatId))
                {
                    List<Circle> dependentHat = categorizedCircles[outCircleHatId];
                    dependentHats.Enqueue(dependentHat);
                    ed.WriteMessage("recursive call for dependent hat : ");
                    retrieveDependentHatsOfMainHat(dependentHat, categorizedCircles, dependentHats);
                }

            }
        }


        public static void calculateSystem()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            List<Circle> allCircles = ModeMB.GetAllMuayeneBacasiCircles(db);
            Dictionary<string, List<Circle>> categorizedCircles = ModeMB.categorizeMuayeneBacasiCirclesByHatId(allCircles);
            List<string> calculatedHatIdList = new List<string>();
            try
            {
                foreach (KeyValuePair<string, List<Circle>> entry in categorizedCircles)
                {
                    Queue<List<Circle>> listOfDependentHatsQueue = new Queue<List<Circle>>();
                    retrieveDependentHatsOfMainHat(entry.Value, categorizedCircles, listOfDependentHatsQueue);
                    while (listOfDependentHatsQueue.Count != 0)
                    {
                        List<Circle> dependentHat = listOfDependentHatsQueue.Dequeue();
                        TypedValue[] dependentHatXrecordValues = ((Xrecord)Utils.fetchXrecordOfObject(dependentHat[0].ObjectId)).Data.AsArray();
                        string dependentHatId = dependentHatXrecordValues[5].Value.ToString();
                        // dependent hat calculation
                        if (!calculatedHatIdList.Contains(dependentHatId))
                        {
                            makeHatCalculation(dependentHatId, dependentHat, categorizedCircles);
                            calculatedHatIdList.Add(dependentHatId);
                        }

                    }
                    // main hat calculation
                    if (!calculatedHatIdList.Contains(entry.Key))
                    {
                        makeHatCalculation(entry.Key, entry.Value, categorizedCircles);
                        calculatedHatIdList.Add(entry.Key);
                    }

                }

            }
            catch (Exception e)
            {
                ed.WriteMessage(e.StackTrace);
                throw e;
            }
        }

    }
}