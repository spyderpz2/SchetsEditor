using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public class Schets
    {
        public Bitmap bitmap;
        public Bitmap baseBitmap;
        public Schets(Bitmap openMetBitmap = null)
        {
            //bitmap = openMetBitmap != null ? openMetBitmap : new Bitmap(1, 1);
            if (openMetBitmap != null)
            {

                bitmap = (Bitmap)openMetBitmap.Clone();
                baseBitmap = (Bitmap)openMetBitmap.Clone();


                //this.VeranderAfmeting(new Size(openMetBitmap.Width, openMetBitmap.Height));
                openMetBitmap.Dispose();
            }
            else
            {
                bitmap = new Bitmap(1, 1);
                baseBitmap = null;
            }
        }
        public Graphics BitmapGraphics
        {
            get 
            {
                /*using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    return g;
                }*/

                return Graphics.FromImage(bitmap);
                /*Graphics graph;
                Console.WriteLine("size: " + bitmap.Size);
                try
                {
                    graph = Graphics.FromImage(bitmap);

                    return graph;
                }
                catch (OutOfMemoryException e)
                {
                    Console.WriteLine(e.ToString());
                   // graph = Graphics.from
                    //throw;
                }*/

            }
        }

        public Size Afmeting
        {
            get { return bitmap.Size; }
            set { VeranderAfmeting(value); }
        }

        public Bitmap tekening
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        public void VeranderAfmeting(Size sz)
        {
            if (this.baseBitmap != null)
            {
                if (this.baseBitmap.Width < sz.Width || this.baseBitmap.Height < sz.Height)
                {
                    //Console.WriteLine("fix dit ff"); 
                }

            }

            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
            {
                Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                         , Math.Max(sz.Height, bitmap.Size.Height)
                                         );
                Graphics gr = Graphics.FromImage(nieuw);
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                //Hier kunnen we eventueel de background color bepalen...
                gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
                gr.DrawImage(bitmap, 0, 0);
                bitmap = nieuw;
            }
        }
        public void Teken(Graphics gr)
        {
            gr.DrawImage(bitmap, 0, 0);
        }
        public void Schoon()
        {
            if (baseBitmap != null)
            {
                //this.bitmap = (Bitmap)baseBitmap.Clone();
                Graphics gr = Graphics.FromImage(bitmap);
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
                gr.DrawImage(this.baseBitmap, new Point(0, 0));
                //Graphics gr = Graphics.FromImage(baseBitmap);
                //gr.SmoothingMode = SmoothingMode.AntiAlias;
                //gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            } else
            {
                Graphics gr = Graphics.FromImage(bitmap);
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            }
        }

        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }
    }
}
