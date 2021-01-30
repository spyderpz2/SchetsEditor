using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;


namespace SchetsEditor
{
    public interface ISchetsTool
    {

        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p, UndoRedoController u = null);
        void Letter(SchetsControl s, char c, UndoRedoController u = null);
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Brush kwast;
        protected bool isPen;
        /// <summary>
        /// The Stack containing all of the coordinates of the drawing by Pentool.
        /// </summary>
        protected List<Point> penLijn = new List<Point>();

        public virtual void MuisVast(SchetsControl s, Point p)
        {
            startpunt = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p, UndoRedoController u = null)
        {
            kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c, UndoRedoController u = null);
    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c, UndoRedoController u)
        {
            if (c >= 32)
            {
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz = gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
                gr.DrawString(tekst, font, kwast,
                                              this.startpunt, StringFormat.GenericTypographic);
                u.addInstruction(new DrawInstuction(ElementType.Tekst, s.PenKleur, startpunt, font, c));
                //Checks whether the input was a space and sets the size manually.
                startpunt.X += c == 32 ? 10 : (int)sz.Width;
                s.Invalidate();
            }
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
                                , new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            kwast = Brushes.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
            s.Invalidate();
        }
        public override void MuisLos(SchetsControl s, Point p, UndoRedoController u = null)
        {
            base.MuisLos(s, p, u);
            this.Compleet(u, s.MaakBitmapGraphics(), this.startpunt, p);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c, UndoRedoController u = null)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2, UndoRedoController u = null);

        public virtual void Compleet(UndoRedoController u, Graphics g, Point p1, Point p2)
        {
            this.Bezig(g, p1, p2, u);
        }
    }
    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2, UndoRedoController u = null)
        {
            g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
            if (u != null)
            {
                u.addInstruction(new DrawInstuction(ElementType.DrawRectangle, ((SolidBrush)kwast).Color, p1, p2, 3));
            }
        }
    }

    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(UndoRedoController u, Graphics g, Point p1, Point p2)
        {
            g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
            u.addInstruction(new DrawInstuction(ElementType.FillRectangle, ((SolidBrush)kwast).Color, p1, p2, 3));
        }
    }

    public class EllipseTool : TweepuntTool
    {
        public override string ToString() { return "ellipse"; }

        public override void Bezig(Graphics g, Point p1, Point p2, UndoRedoController u = null)
        {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
            if (u != null)
            {
                u.addInstruction(new DrawInstuction(ElementType.DrawEllipse, ((SolidBrush)kwast).Color, p1, p2, 3));
            }
        }
    }
    public class VolEllipseTool : EllipseTool
    {
        public override string ToString() { return "volellipse"; }

        public override void Compleet(UndoRedoController u, Graphics g, Point p1, Point p2)

        {

            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
            u.addInstruction(new DrawInstuction(ElementType.FillEllipse, ((SolidBrush)kwast).Color, p1, p2, 3));
        }
    }
    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }
        public override void Bezig(Graphics g, Point p1, Point p2, UndoRedoController u = null)
        {
            //Check whether the line is drawn with the Pentool, then push the points to the stack penLijn.
            if (this.isPen)
            {
                this.penLijn.Add(p1); this.penLijn.Add(p2);
            }
            g.DrawLine(MaakPen(this.kwast, 3), p1, p2);

            //Check whether the UndoRedoController is passed to the function, then add the definitive instruction to it.
            if (u != null)
            {
                if (this.penLijn.Count > 0) //Check whether there are points in the stack, if so then it was drawn with the Pentool.
                {
                    ///Could maybe split the penLijn to multiple smaller points.
                    u.addInstruction(new DrawInstuction(ElementType.Pen, ((SolidBrush)this.kwast).Color, this.penLijn, 3));
                    this.penLijn = new List<Point>();
                }
                else //This means its just a normal straight line.
                {
                    u.addInstruction(new DrawInstuction(ElementType.Line, ((SolidBrush)this.kwast).Color, p1, p2, 3));
                }
            }
        }
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            //Set the global value of isPen to be what it is, true. Then reset.
            this.isPen = true;
            this.MuisLos(s, p);
            this.MuisVast(s, p);
            this.isPen = false;
        }
    }

    public class GumTool : PenTool
    {
        public override string ToString() { return "gum"; }
        public override void MuisLos(SchetsControl s, Point p, UndoRedoController u = null)
        {
            if(u != null)
            {
                //We have to reverse a temporary list because we want to delete the last added item.
                List<DrawInstuction> reverseElementList = new List<DrawInstuction>(u.UndoList);
                reverseElementList.Reverse();
                
                Graphics g = s.CreateGraphics();
                GraphicsPath gp = new GraphicsPath();
                bool visible = false;
                foreach(DrawInstuction uu in reverseElementList)
                {
                    //Here we check the elementtype and if it is visible, 
                    //we do this so that only the border of ellipses become clickable instead of the whole circle to make it more user friendly
                    switch (uu.elementType)
                    {
                        case ElementType.DrawEllipse:
                            gp.AddEllipse(uu.ToRectangle());
                            visible = gp.IsOutlineVisible(p.X, p.Y, uu.CreatePen());
                            break;
                        case ElementType.FillEllipse:
                            gp.AddEllipse(uu.ToRectangle());
                            visible = gp.IsVisible(p.X, p.Y);
                            break;
                        case ElementType.DrawRectangle:
                            gp.AddRectangle(uu.ToRectangle());
                            visible = gp.IsOutlineVisible(p.X, p.Y, uu.CreatePen());
                            break;
                        case ElementType.FillRectangle:
                            gp.AddRectangle(uu.ToRectangle());
                            visible = gp.IsVisible(p.X, p.Y);
                            break;
                        case ElementType.Line:
                            gp.AddLine(uu.startPunt, uu.eindPunt);
                            visible = gp.IsOutlineVisible(p.X, p.Y, uu.CreatePen());
                            break;
                        case ElementType.Pen:
                            Point lastPoint = uu.puntenVanLijn.Peek();
                            foreach (Point pointOnLine in uu.puntenVanLijn)
                            {
                                gp.AddLine(pointOnLine, lastPoint);
                                lastPoint = pointOnLine;
                            }
                            visible = gp.IsOutlineVisible(p.X, p.Y, uu.CreatePen());
                            break;
                    }
                    // Here we have to only check visible because everything is checked by the switch.
                    if (visible)
                     {
                        u.UndoList.Remove(uu);
                        u.RedoList.Add(uu);
                        break;
                     }
                    visible = false;
                }
                s.Schets.Schoon();
                u.UndoList.DrawElements(s.MaakBitmapGraphics());

                s.Refresh();
            }
        }
       
    }
}
