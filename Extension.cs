using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace SchetsEditor
{
    static class Extension
    {
       
        public static string ToString<T>(this List<T> elStack)
        {
            string toReturn = "";
            foreach (T el in elStack)
                toReturn += el.ToString() + "\n";
            return toReturn;
        }

        public static T Pop<T>(this List<T> elements)
        {
            T lastElement = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            return lastElement;
        }

        public static T Peek<T>(this List<T> elements)
        {
            return elements.Last();
        }


        public static List<T> CustomReverse<T>(this List<T> elStack)
        {
            List<T> drawOrder = new List<T>();
            foreach (T el in elStack)
                drawOrder.Add(el);
            return drawOrder;
        }

        public static void DrawElements(this List<DrawInstuction> elStack, Graphics toDrawOn)
        {
            using (toDrawOn)
            {
                foreach (DrawInstuction elToDraw in elStack)
                {
                    switch (elToDraw.elementType)
                    {
                        case ElementType.Pen:
                            Point lastPoint = elToDraw.puntenVanLijn.First();
                            foreach (Point pointOnLine in elToDraw.puntenVanLijn)
                            {
                                toDrawOn.DrawLine(elToDraw.CreatePen(), lastPoint, pointOnLine);
                                lastPoint = pointOnLine;
                            }
                            break;
                        case ElementType.Line:
                            toDrawOn.DrawLine(elToDraw.CreatePen(), elToDraw.startPunt, elToDraw.eindPunt);
                            break;
                        case ElementType.DrawRectangle:
                            toDrawOn.DrawRectangle(elToDraw.CreatePen(), elToDraw.ToRectangle());
                            break;
                        case ElementType.FillRectangle:
                            toDrawOn.FillRectangle(elToDraw.CreateBrush(), elToDraw.ToRectangle());
                            break;
                        case ElementType.DrawEllipse:
                            toDrawOn.DrawEllipse(elToDraw.CreatePen(), elToDraw.ToRectangle());
                            break;
                        case ElementType.FillEllipse:
                            toDrawOn.FillEllipse(elToDraw.CreateBrush(), elToDraw.ToRectangle());
                            break;
                        case ElementType.Tekst:
                            toDrawOn.DrawString(elToDraw.letter.ToString(), elToDraw.font, elToDraw.CreateBrush(), elToDraw.startPunt);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static T FromXML<T>(string xml)
        {
            using (StringReader stringReader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

        public static string ToXML<T>(T obj)
        {
            using (StringWriter stringWriter = new StringWriter(new StringBuilder()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }

        public static ImageFormat getImageFormatFromFile(FileInfo fileInfo)
        {
            switch (fileInfo.Extension.ToLower())
            {
                case ".png":
                    return ImageFormat.Png;
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".pml":
                    //Use Emf as placeholder for our custom paint format. 
                    return ImageFormat.Emf;
                default:
                    return ImageFormat.Png;
            }
        }


        //https://stackoverflow.com/a/10502856/8902440
        public static byte[] ToByteArray(this object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        //https://stackoverflow.com/a/800469/8902440
        public static string GetHash(this byte[] data)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }
    }
}

