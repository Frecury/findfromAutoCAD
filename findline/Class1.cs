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
                //开始选择
                TypedValue[] acTypValAr = new TypedValue[2];
                acTypValAr.SetValue(new TypedValue(0, "LWPOLYLINE"), 0);
                acTypValAr.SetValue(new TypedValue(8, "0"), 1);

                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
                acSSPrompt = acDocEd.GetSelection(acSelFtr);
                //acSSPrompt = acDocEd.SelectAll();
                SelectionSet acSSet = acSSPrompt.Value;
                //Application.ShowAlertDialog("Number of objects selected: " + acSSet.Count.ToString());
                acEd.WriteMessage("共找到: " + acSSet.Count.ToString() + "条多段线");  //控制台输出字符串

                PromptStringOptions pStrOpts = new PromptStringOptions("\n要查找的长度: ");
                PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);
                string ThelengthYWS = pStrRes.StringResult;
                int ThelengthYW = Convert.ToInt32(ThelengthYWS);

                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
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
                            view.Height = (Math.Abs(p2.Y - p1.Y)*2);
                            view.Width = (Math.Abs(p2.X - p1.X)*2);
                            view.CenterPoint = new Point2d((p2.X + p1.X) / 2, (p2.Y + p1.Y) / 2);
                            //设为当前视图
                            acEd.SetCurrentView(view);
                            Application.UpdateScreen();
                            break;
                        }
                        //else
                        //{
                        //    Application.ShowAlertDialog("未找到相关内容 ");
                        //}
                    }
                    acTrans.Commit();
                }
            }
            catch
            {
                acEd.WriteMessage("\n取消");  //控制台输出字符串
            }
        }
    }
}
