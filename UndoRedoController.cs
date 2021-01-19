using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace SchetsEditor
{
    public class UndoRedoController
    {
        /// <summary>
        /// The list containing all of the current `DrawInstuction`s.
        /// </summary>
        private List<DrawInstuction> UndoList = new List<DrawInstuction>();
        /// <summary>
        /// The list containing all of the elements that can be redone, only being filled after calling `undo()` method.
        /// </summary>
        private List<DrawInstuction> RedoList = new List<DrawInstuction>();

        /// <summary>
        /// Adds the drawing instruction to the `UndoList` so it can be retrieved later.
        /// </summary>
        /// <param name="instruction">The drawing instruction to be added to the list.</param>
        public void addInstruction(DrawInstuction instruction)
        {
            // Save the snapshot.
            UndoList.Add(instruction);
            //Console.WriteLine("hallo?");
            
            // Empty the redo list.
            if (RedoList.Count > 0)
            {
                RedoList = new List<DrawInstuction>();
            }
        }

        public List<DrawInstuction> getElements()
        { 
            return this.UndoList;
        }

        public Dictionary<String, List<DrawInstuction>> getAll()
        {
            Dictionary<String, List<DrawInstuction>> undoRedo = new Dictionary<string, List<DrawInstuction>>();
            undoRedo.Add("undo", this.UndoList);
            undoRedo.Add("redo", this.RedoList);
            return undoRedo;
        }

        /// <summary>
        /// Undoes a 'commit' or drawing action made by the user. E.g. it removes the last drawing from the list.
        /// </summary>
        /// <returns>The remaining drawing instructions in Reverse order e.g. the order they were drawn in</returns>
        public List<DrawInstuction> undo()
        {
            // Move the most recent change to the redo list.
            if (UndoList.Count != 0)
            {
                RedoList.Add(UndoList.Pop());
            }
            return UndoList.CustomReverse();
        }

        /// <summary>
        /// Redoes a 'commit' or drawing action made by the user. E.g. it adds the n-last drawing from the RedoList to the UndoList.
        /// </summary>
        /// <returns>The remaining drawing instructions in the order they were drawn in</returns>
        public List<DrawInstuction> redo()
        {
            // Move the most recently undone item back to the undo list.
            if (RedoList.Count != 0)
            {
                UndoList.Add(RedoList.Pop());
            }
            return UndoList;
        }

    }

    /// <summary>
    /// Holds all the information to recreate a drawing action by the user.
    /// </summary>
    [Serializable]
    public struct DrawInstuction
    {
        /// <summary>
        /// Assign the values of the draw instruction to each corresponding variable
        /// </summary>
        /// <param name="elType">The type of the drawing instruction e.g. DrawRectangle would be ElementType.FillRectangle</param>
        /// <param name="elKleur">The color of the element to draw.</param>
        /// <param name="elStartPunt">The startingpoint of the element to draw.</param>
        /// <param name="elEindPunt">The endpoint of the element to draw.</param>
        /// <param name="elLijnDikte">The thickness or width of the line(s) of the element to draw.</param>
        public DrawInstuction(ElementType elType, Color elKleur, Point elStartPunt, Point elEindPunt, int elLijnDikte = 3) : this()
        {
            elementType = elType;
            kleur = elKleur;
            startPunt = elStartPunt;
            eindPunt = elEindPunt;
            lijnDikte = elLijnDikte;
        }

        public DrawInstuction(ElementType elType, Color elKleur, Point elStartPunt, Font elFont, char elChar) : this()
        {
            elementType = elType;
            kleur = elKleur;
            startPunt = elStartPunt;
            font = elFont;
            letter = elChar;
        }

        public DrawInstuction(ElementType elType, Color elKleur, List<Point> elPunten, int elLijnDikte = 3) : this()
        {
            elementType = elType;
            kleur = elKleur;
            puntenVanLijn = elPunten;
            lijnDikte = elLijnDikte;    
        }


        public ElementType elementType { get; set; }
        //public Color kleur { get; set; }
        public Point startPunt { get; set; }
        public Point eindPunt { get; set; }
        public int lijnDikte { get; set; }
        public char letter { get; set; }
        public List<Point> puntenVanLijn { get; set; }

        //color should be ignored by xml serializer because it can't normally be serialized.
        [XmlIgnore]
        public Color kleur { get; set; }
        //Fix the color serialization. Taken from: https://stackoverflow.com/a/12101050/8902440
        [XmlElement("kleur"), Browsable(false)]
        public int kleurAsArgb
        {
            get { return kleur.ToArgb(); }
            set { kleur = Color.FromArgb(value); }
        }

        //font should be ingored by xml serializer because it can't normally be serialized.
        [XmlIgnore()]
        public Font font { get; set; }
        //Fix the font serialization. Taken from: https://stackoverflow.com/a/34934422/8902440
        [Browsable(false)]
        public string FontSerialize
        {
            get { return TypeDescriptor.GetConverter(typeof(Font)).ConvertToInvariantString(font); }
            set { font = TypeDescriptor.GetConverter(typeof(Font)).ConvertFromInvariantString(value) as Font; }
        }

        /// <summary>
        /// Facilitates debugging. 
        /// </summary>
        /// <returns>Returns a custom represention of the current drawing instruction in a `String`.</returns>
        public override string ToString() => $"Type: {elementType.ToString()}, kleur: {kleur.ToString()}, lijndikte: {lijnDikte.ToString()}, startpunt: {startPunt.ToString()}, eindpunt: {eindPunt.ToString()};";
        /// <summary>
        /// Easily create a brush of the current element for the redrawal of it.
        /// </summary>
        /// <returns>Returns a `SolidBrush` of the elements color</returns>
        public Brush CreateBrush() => new SolidBrush(kleur);
        /// <summary>
        /// Easily create a `Pen` of the current element for the redrawal of it. Using the Preexisting `TweepuntTool.MaakPen()` method.
        /// </summary>
        /// <returns>Returns a `Pen` of the elements color</returns>
        public Pen CreatePen() => TweepuntTool.MaakPen(CreateBrush(), lijnDikte);
        /// <summary>
        /// Easily create a `Rectangle` of the current element for the redrawal of it. Using the Preexisting `TweepuntTool.Punten2Rechthoek()` method.
        /// </summary>
        /// <returns>Returns a `Rectangle` of the elements color</returns>
        public Rectangle ToRectangle() => TweepuntTool.Punten2Rechthoek(startPunt, eindPunt);
    }

    /// <summary>
    /// This enumerate holds all of the possible types of drawings
    /// </summary>
    public enum ElementType
    {
        Pen,
        Line,
        DrawRectangle,
        FillRectangle,
        DrawEllipse,
        FillEllipse,
        Tekst
    }

    static class Extension
    {
        /// <summary>
        /// Easily get print the values of the stack of Drawnelement
        /// </summary>
        /// <param name="elStack">The stack of `DrawInstuction` to be returned as a contious string.</param>
        /// <returns>String the elements in elStack to string with "\n" in between</returns>
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
            return elements[elements.Count - 1];
        }


        /// <summary>
        /// Reverses the stack of `DrawInstuction`. First becomes last element and vice versa.
        /// </summary>
        /// <param name="elStack">The stack of `DrawInstuction` to be returned in Reverse order.</param>
        /// <returns>`Stack<DrawInstuction>`: elStack in Reverse order.</returns>
        public static List<T> CustomReverse<T>(this List<T> elStack)
        {
            List<T> drawOrder = new List<T>();
            foreach (T el in elStack)
                drawOrder.Add(el);
            return drawOrder;
        }

        /// <summary>
        /// Redraws the elements in the given stack on the given `Graphics` object.
        /// </summary>
        /// <param name="elStack">The stack of `DrawInstuction` to be drawn</param>
        /// <param name="toDrawOn">The `Graphics` object to be drawn on.</param>
        public static void DrawElements(this List<DrawInstuction> elStack, Graphics toDrawOn)
        {
            using (toDrawOn)
            {
                foreach (DrawInstuction elToDraw in elStack)
                {
                    switch (elToDraw.elementType)
                    {
                        case ElementType.Pen:
                            Point lastPoint = elToDraw.puntenVanLijn.Peek();
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
