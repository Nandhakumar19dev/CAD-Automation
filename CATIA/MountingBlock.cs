using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using PARTITF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CATAutomation

{

    internal class CatiaConnect
    {
        INFITF.Application myCATIA;
        static void main(string[] args)
        {
            CatiaConnect program = new CatiaConnect();
            program.Connect();
        }
        public INFITF.Application Connect()
        {
            myCATIA = (INFITF.Application)Marshal.GetActiveObject("CATIA.Application");
            return myCATIA;

        }
    }

    internal class SketchCtxManager : IDisposable
    {
        Sketch sketch;
        public Factory2D fac2d;

        public SketchCtxManager(Sketch sketch)
        {
            this.sketch = sketch;
            fac2d = this.sketch.OpenEdition();
        }

        public void Dispose()
        {
            sketch.CloseEdition();
        }
    }
    internal class CATLCLBase
    {
        public INFITF.Application myCATIA;
        public PartDocument partDocument;
        public Part part;
        public Body body;
        public Document document;

        public CATLCLBase()
        {
            CatiaConnect catiaConnect = new CatiaConnect();
            myCATIA = catiaConnect.Connect();
        }

        public Document getDocument(int documentIndex)
        {
            Document document = myCATIA.Documents.Item(documentIndex);
            return document;
        }

        public PartDocument getPartDocument(Document document)
        {
            PartDocument partDocument = (PartDocument)document;
            return partDocument;

        }

        public Part getPart(PartDocument partDocument)
        {
            Part part = partDocument.Part;
            return part;
        }

        public Body getNthBody(Part part, int index = 1)
        {
            MECMOD.Bodies bdys = part.Bodies;
            MECMOD.Body bdy = bdys.Item(index);
            return bdy;
        }

        public Sketches getSketches(int documentIndex, int bodyIndex)
        {
            this.part = ((PartDocument)getDocument(documentIndex)).Part;
            this.body = part.Bodies.Item(bodyIndex);
            return body.Sketches;
        }
    }
    internal class MountingBlock: CATLCLBase
    {
        public void create_mounting_block(int docIndex, int partIndex)
        {

            
            MECMOD.Sketches skts = getSketches(docIndex, partIndex);
            INFITF.Editor editor = myCATIA.ActiveEditor; 
            INFITF.Reference xypln = (INFITF.Reference)part.OriginElements.PlaneXY;
            
            MECMOD.Sketch skt = skts.Add(xypln);
            
            double xCenter = 20;
            double yCenter = 15;
            using(SketchCtxManager sketchCtxManager = new SketchCtxManager(skt))
            {
                Factory2D fac2d = sketchCtxManager.fac2d;
                fac2d.CreateLine(0, 0, 0, 30); 

                fac2d.CreateLine(0, 30, 20, 30);

                double Radius = 15;
                double stParam = (Math.PI/2 )+ Math.PI;
                double etParam =  (3*Math.PI/2) + Math.PI;

                fac2d.CreateCircle(xCenter, yCenter, Radius, stParam, etParam);

                fac2d.CreateLine(20, 0, 0, 0);
            }


            ShapeFactory shpfac = (ShapeFactory)part.ShapeFactory;
            Pad pad = shpfac.AddNewPad(skt, 20);

            MECMOD.Sketch skt2 = skts.Add(xypln);
            double circleRad = 10/2;
            
            using(SketchCtxManager sketchCtxManager = new SketchCtxManager(skt2))
            {
                Factory2D fac2d1 = sketchCtxManager.fac2d; 
                fac2d1.CreateClosedCircle(xCenter, yCenter, circleRad);
                Pocket pocket = shpfac.AddNewPocket(skt2, -20);
            }

            MECMOD.Sketch skt3 = skts.Add(xypln);
            Pad pad2 = null;
            using (SketchCtxManager sketchCtxManager = new SketchCtxManager(skt3))
            {
                Factory2D fac2d2 = sketchCtxManager.fac2d;
                Line2D line11 = fac2d2.CreateLine(0, 0, 0, 40);
                fac2d2.CreateLine(0, 40, -25, 40);
                Line2D line13 = fac2d2.CreateLine(-25, 40, -25, 0);
                fac2d2.CreateLine(-25, 0, 0, 0);
                pad2 = shpfac.AddNewPad(skt3, 20);
            }
            part.Update();
            Reference refSurf1 = part.CreateReferenceFromName($"Selection_RSur:(Face:(Brp:((Brp:({pad2.get_Name()};2);Brp:({pad.get_Name()};2)));None:();Cf14:());{pad2.get_Name()}_ResultOUT;Z0;G11099)");
            Sketch wall = skts.Add(refSurf1);
            Selection sel = editor.Selection;
            // sel.Add(refSurf1);
            Pad pad3 = null; 
            using (SketchCtxManager sketchCtxManager = new SketchCtxManager(wall))
            {
                Factory2D fac2d3 = sketchCtxManager.fac2d;
                fac2d3.CreateLine(0, 0, 0, 40);
                fac2d3.CreateLine(0, 40, -10, 40);
                fac2d3.CreateLine(-10, 40, -10, 10);
                fac2d3.CreateLine(-10, 10, -25, 10);
                fac2d3.CreateLine(-25, 10, -25, 0);
                fac2d3.CreateLine(-25, 0, 0, 0);

                pad3 = shpfac.AddNewPad(wall, 60);
            }
            part.Update();

            Reference refSurf2 = part.CreateReferenceFromName(
                $"Selection_RSur:(Face:(Brp:({pad3.get_Name()};0:(Brp:({wall.get_Name()};3)));None:();Cf14:());{pad3.get_Name()}_ResultOUT;Z0;G11099)");
            Sketch support = skts.Add(refSurf2);
            sel.Add(refSurf2);
            Pad pad4;
            using (SketchCtxManager sketchCtxManager = new SketchCtxManager(support))
            {
                Factory2D fac2d4 = sketchCtxManager.fac2d;
                fac2d4.CreateLine(0, 0, 30, 60);
                fac2d4.CreateLine(30, 60, 30, 0);
                fac2d4.CreateLine(30, 0, 0, 0);

                pad4 = shpfac.AddNewPad(support, 15);
            }

            part.Update();
            //Reference mirrRef = part.CreateReferenceFromName(
             Console.WriteLine($"Face Mirror Ref... Selection_RSur:(Face:(Brp:((Brp:({pad4.get_Name()};2);Brp:({pad3.get_Name()};0:(Brp:({wall.get_Name()};5)));Brp:({pad2.get_Name()};0:(Brp:({skt3.get_Name()};3)))));None:();Cf14:());{pad4.get_Name()}_ResultOUT;Z0;G11099)");
            // sel.Add(mirrRef);
            sel.Clear();
            object[] face = { "Face", };
            Console.WriteLine("Select a face for mirror..."); 
            sel.SelectElement2(face, "Select a face for mirror...", false);
            Face mirrRef = (Face)sel.Item(1).Value; 
            Mirror mirror = shpfac.AddNewMirror(mirrRef);
            
            part.Update();
        }

        public static void Main(String[] args)
        {
            MountingBlock mountingBlock = new MountingBlock();
            mountingBlock.create_mounting_block(2, 1); 
        }
    }
}
