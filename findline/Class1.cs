using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace findline
{
    public class Class1
    {
        public Document acDoc;
        public Editor acEd;
        public Class1()
        {
            acDoc = Application.DocumentManager.MdiActiveDocument;//获取当前的活动文档 
        }
        public ObjectId SelectEntity(string message, Type enttype)
        {
            PromptEntityOptions Options = new PromptEntityOptions(message);
            if (enttype != null)
            {
                Options.SetRejectMessage("选择的图元类型不对，请重新选择" + enttype.ToString() + "类型图元\n");
                Options.AddAllowedClass(enttype, true);
            }
            PromptEntityResult res = acDoc.Editor.GetEntity(Options);
            // if (res.Status != PromptStatus.OK) return null;
            return res.ObjectId;
        }

        string Thechosentype = " ";
        string CThechosentype = " ";
        int a = 0;

        [CommandMethod("fd")]//设计的新命令
        public void te1()
        {

            Class1 ncad = new Class1();
            Database acCurDb = ncad.acDoc.Database;//获取当前的活动数据库
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult acSSPrompt;  //选择集的结果对象   
            Editor acEd = acDoc.Editor;//当前的编辑器对象，命令行对象

            ViewTableRecord view = acEd.GetCurrentView();
            Point2d pt1 = view.CenterPoint;
            double Height = view.Height;
            double Width = view.Width;

            try
            {
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    //开始选择
                    //PromptEntityOptions pEntOpts = new PromptEntityOptions("\n 请选择要查找的对象");
                    //PromptEntityResult pEntRes = ncad.acDoc.Editor.GetEntity(pEntOpts);
                    //pEntOpts.AddAllowedClass(null, true);
                    //ObjectId obj2 = pEntRes.ObjectId;

                    ObjectId objid = ncad.SelectEntity("请选择对象类型", null);
                    Entity Chosenentity = acTrans.GetObject(objid, OpenMode.ForWrite) as Entity;
                    string Thelayername = Chosenentity.Layer;

                    Type chosentype = Chosenentity.GetType();
                    string chosentypes = chosentype.ToString();
                    if (chosentypes == "Autodesk.AutoCAD.DatabaseServices.Polyline")
                    {
                        Thechosentype = "LWPOLYLINE";
                        CThechosentype = "多段线";
                    }
                    else if (chosentypes == "Autodesk.AutoCAD.DatabaseServices.Hatch")
                    {
                        Thechosentype = "HATCH";
                        CThechosentype = "填充";
                    }
                    TypedValue[] acTypValAr = new TypedValue[2];
                    acTypValAr.SetValue(new TypedValue(0, Thechosentype), 0);
                    acTypValAr.SetValue(new TypedValue(8, Thelayername), 1);

                    SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
                    acSSPrompt = acDocEd.GetSelection(acSelFtr);
                    //acSSPrompt = acDocEd.SelectAll();
                    SelectionSet acSSet = acSSPrompt.Value;
                    //Application.ShowAlertDialog("Number of objects selected: " + acSSet.Count.ToString());
                    acEd.WriteMessage("共找到: " + acSSet.Count.ToString() + CThechosentype);  //控制台输出字符串

                    PromptStringOptions pStrOpts = new PromptStringOptions("\n要查找的长度/面积: ");
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);
                    //string ThelengthYWS = pStrRes.StringResult;
                    //int ThelengthYW = Convert.ToInt32(ThelengthYWS);
                    //string TheareaYWS = pStrRes.StringResult;
                    //int TheareaYW = Convert.ToInt32(TheareaYWS);

                    if (chosentypes == "Autodesk.AutoCAD.DatabaseServices.Polyline")
                    {
                        string ThelengthYWS = pStrRes.StringResult;
                        int ThelengthYW = Convert.ToInt32(ThelengthYWS);
                        foreach (ObjectId Objid in acSSet.GetObjectIds())
                        {
                            //BlockTableRecord acBlkTblRec = acTrans.GetObject((ObjectId)Objid, OpenMode.ForRead) as BlockTableRecord;
                            Polyline polyline = acTrans.GetObject((ObjectId)Objid, OpenMode.ForRead) as Polyline;
                            double findlength = polyline.Length;
                            int Findlength = Convert.ToInt32(findlength);

                            while (Findlength == ThelengthYW)
                            {
                                //高亮显示
                                Entity current_entity = (Entity)acTrans.GetObject(Objid, OpenMode.ForWrite, true);
                                List<ObjectId> listSet = new List<ObjectId>();
                                listSet.Add(Objid);
                                acEd.SetImpliedSelection(listSet.ToArray());
                                //缩放图形到屏幕正中
                                Point3d p1 = polyline.StartPoint;
                                Point3d p2 = polyline.EndPoint;
                                view.Height = (Math.Abs(p2.Y - p1.Y) * 2);
                                view.Width = (Math.Abs(p2.X - p1.X) * 2);
                                view.CenterPoint = new Point2d((p2.X + p1.X) / 2, (p2.Y + p1.Y) / 2);
                                //设为当前视图
                                acEd.SetCurrentView(view);
                                Application.UpdateScreen();
                                a = 1;
                                break;
                            }

                        }
                        while (a == 0)
                        {
                            Application.ShowAlertDialog("未找到相关内容 ");
                            break;
                        }
                        acTrans.Commit();
                    }
                    else if (chosentypes == "Autodesk.AutoCAD.DatabaseServices.Hatch")
                    {
                        string TheareaYWS = pStrRes.StringResult;
                        int TheareaYW = Convert.ToInt32(TheareaYWS);
                        foreach (ObjectId Objid in acSSet.GetObjectIds())
                        {
                            Hatch hatch = acTrans.GetObject((ObjectId)Objid, OpenMode.ForRead) as Hatch;
                            double findArea = hatch.Area;
                            int FindArea = Convert.ToInt32(findArea);

                            while (FindArea == TheareaYW)
                            {
                                //高亮显示
                                Entity current_entity = (Entity)acTrans.GetObject(Objid, OpenMode.ForWrite, true);
                                List<ObjectId> listSet = new List<ObjectId>();
                                listSet.Add(Objid);
                                acEd.SetImpliedSelection(listSet.ToArray());
                                //Point2d centerpoint = hatch.Origin;
                                ////缩放图形到屏幕正中
                                ////Point2d p1 = centerpoint.X;
                                ////Point3d p2 = centerpoint.Y;
                                ////view.Height = (Math.Abs(p2.Y - p1.Y) * 2);
                                ////view.Width = (Math.Abs(p2.X - p1.X) * 2);
                                //view.CenterPoint = centerpoint;
                                ////设为当前视图
                                //acEd.SetCurrentView(view);
                                //Application.UpdateScreen();
                                break;
                            }

                        }
                        while (a == 0)
                        {
                            Application.ShowAlertDialog("未找到相关内容 ");
                            break;
                        }
                        acTrans.Commit();
                    }
                }
            }
            catch
            {
                acEd.WriteMessage("\n取消");  //控制台输出字符串
            }
        }


        //#region 查找填充
        //[CommandMethod("fdh")]//设计的新命令
        //public  void te2()
        //{
        //    Class1 ncad = new Class1();
        //    Database acCurDb = ncad.acDoc.Database;//获取当前的活动数据库
        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;
        //    Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;
        //    PromptSelectionResult acSSPrompt;  //选择集的结果对象   
        //    Editor acEd = acDoc.Editor;//当前的编辑器对象，命令行对象

        //    ViewTableRecord view = acEd.GetCurrentView();
        //    Point2d pt1 = view.CenterPoint;
        //    double Height = view.Height;
        //    double Width = view.Width;

        //    try
        //    {
        //        //开始选择
        //        TypedValue[] acTypValAr = new TypedValue[2];
        //        acTypValAr.SetValue(new TypedValue(0, "HATCH"), 0);
        //        acTypValAr.SetValue(new TypedValue(8, "0"), 1);

        //        SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
        //        acSSPrompt = acDocEd.GetSelection(acSelFtr);
        //        //acSSPrompt = acDocEd.SelectAll();
        //        SelectionSet acSSet = acSSPrompt.Value;
        //        //Application.ShowAlertDialog("Number of objects selected: " + acSSet.Count.ToString());
        //        acEd.WriteMessage("共找到: " + acSSet.Count.ToString() + "个填充");  //控制台输出字符串

        //        PromptStringOptions pStrOpts = new PromptStringOptions("\n要查找的面积: ");
        //        PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);
        //        string TheareaYWS = pStrRes.StringResult;
        //        int TheareaYW = Convert.ToInt32(TheareaYWS);

        //        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //        {
        //            foreach (ObjectId Objid in acSSet.GetObjectIds())
        //            {
        //                //BlockTableRecord acBlkTblRec = acTrans.GetObject((ObjectId)Objid, OpenMode.ForRead) as BlockTableRecord;
        //                Hatch hatch = acTrans.GetObject((ObjectId)Objid, OpenMode.ForRead) as Hatch;
        //                double findArea = hatch.Area;
        //                int FindArea = Convert.ToInt32(findArea);

        //                while (FindArea == TheareaYW)
        //                {
        //                    //高亮显示
        //                    Entity current_entity = (Entity)acTrans.GetObject(Objid, OpenMode.ForWrite, true);
        //                    List<ObjectId> listSet = new List<ObjectId>();
        //                    listSet.Add(Objid);
        //                    acEd.SetImpliedSelection(listSet.ToArray());
        //                    //Point2d centerpoint = hatch.Origin;
        //                    ////缩放图形到屏幕正中
        //                    ////Point2d p1 = centerpoint.X;
        //                    ////Point3d p2 = centerpoint.Y;
        //                    ////view.Height = (Math.Abs(p2.Y - p1.Y) * 2);
        //                    ////view.Width = (Math.Abs(p2.X - p1.X) * 2);
        //                    //view.CenterPoint = centerpoint;
        //                    ////设为当前视图
        //                    //acEd.SetCurrentView(view);
        //                    //Application.UpdateScreen();
        //                    break;
        //                }
        //                //else
        //                //{
        //                //    Application.ShowAlertDialog("未找到相关内容 ");
        //                //}
        //            }
        //            acTrans.Commit();
        //        }
        //    }
        //    catch
        //    {
        //        acEd.WriteMessage("\n取消");  //控制台输出字符串
        //    }
        //}
        //#endregion
    }
}
