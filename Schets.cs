using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public class Schets
    {
        public Bitmap bitmap;
        private Bitmap baseBitmap;
        public Schets(Bitmap openMetBitmap = null)
        {
            bitmap = openMetBitmap != null ? openMetBitmap : new Bitmap(1, 1);
            if (openMetBitmap != null)
            {
                bitmap = openMetBitmap;
                baseBitmap = (Bitmap)openMetBitmap.Clone();
                //this.VeranderAfmeting(new Size(openMetBitmap.Width, openMetBitmap.Height));
            }
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
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
                    Console.WriteLine("fix dit ff"); 
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
                this.tekening = (Bitmap)baseBitmap.Clone();
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
