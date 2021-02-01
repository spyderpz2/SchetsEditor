using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;

namespace SchetsEditor
{
    [Serializable]
    public class DrawStorage
    {
        public DrawStorage(List<DrawInstuction> undoList, List<DrawInstuction> redoList, Size afmeting, Bitmap backImage)
        {
            undo = undoList;
            redo = redoList;
            dimensions = afmeting;
            backgroundImage = backImage;
        }

        public DrawStorage() { }

        public List<DrawInstuction> undo { get; set; }
        public List<DrawInstuction> redo { get; set; }
        public Size dimensions { get; set; }
        [XmlIgnore]
        public Bitmap backgroundImage { get; set; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("backgroundImage")]
        public string backgroundImageSerialized
        {
            get
            { // serialize
                if (backgroundImage == null) { return null; }
                using (MemoryStream ms = new MemoryStream())
                {
                    backgroundImage.Save(ms, ImageFormat.Bmp);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            set
            { // deserialize
                if (value == null) { backgroundImage = null; }
                else
                {
                    byte[] bytes = Convert.FromBase64String(value);
                    MemoryStream mem = new MemoryStream(bytes);
                    backgroundImage = new Bitmap(mem);
                }
            }
        }

        public override string ToString() => $"Undo: {undo.ToString()}, Redo: {redo.ToString()}, meta: {(backgroundImage != null ? backgroundImage.ToString() : "".ToString())};";
    }
}
