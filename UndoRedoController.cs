using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class UndoRedoController
    {

        private Stack<DrawnElement> UndoList = new Stack<DrawnElement>();
        private Stack<DrawnElement> RedoList = new Stack<DrawnElement>();
        public int test = 0;

        public void addState(DrawnElement state)
        {
            // Save the snapshot.
            UndoList.Push(state);
            //Console.WriteLine("hallo?");
            
            // Empty the redo list.
            if (RedoList.Count > 0)
            {
                RedoList = new Stack<DrawnElement>();
            }
        }

        public bool noneUndoRedoYet()
        {
            return this.RedoList.Count == 0;
        }

        public Stack<DrawnElement> getElements()
        {
            return this.UndoList;
        }

        public Stack<DrawnElement> undo()
        {
            // Move the most recent change to the redo list.
            if (UndoList.Count != 0)
            {
                RedoList.Push(UndoList.Pop());
            }
            // Restore the top item in the Undo list.
            return UndoList;
        }

        public Stack<DrawnElement> redo()
        {
            // Move the most recently undone item back to the undo list.
            if (RedoList.Count != 0)
            {
                UndoList.Push(RedoList.Pop());
            }
            // Restore the top item in the Undo list.
            return RedoList;
        }

    }

    public struct DrawnElement
    {
        public DrawnElement(ElementType elType, Color elKleur, Point elStartPunt, Point elEindPunt, int elLijnDikte = 3)
        {
            elementType = elType;
            kleur = elKleur;
            startPunt = elStartPunt;
            eindPunt = elEindPunt;
            lijnDikte = elLijnDikte;
        }

        public ElementType elementType { get; }
        public Color kleur { get; }
        public Point startPunt { get; }
        public Point eindPunt { get; }
        public int lijnDikte { get; }
        public override string ToString() => $"Type: {elementType.ToString()}, kleur: {kleur.ToString()}, lijndikte: {lijnDikte.ToString()}, startpunt: {startPunt.ToString()}, eindpunt: {eindPunt.ToString()};";
        public Pen CreatePen()
        {
            return TweepuntTool.MaakPen(new SolidBrush(kleur), lijnDikte);
        }
        public Rectangle ToRectangle()
        {
            return TweepuntTool.Punten2Rechthoek(startPunt, eindPunt);
        }


    }
    /// <summary>
    /// This enumerate holds all of the possible types of drawings
    /// </summary>
    public enum ElementType
    {
        Lijn,
        RechthoekOpen,
        RechthoekDicht,
        ElipseOpen,
        ElipseDicht
    }

    static class Extension
    {
        /// <summary>
        /// Easily get print the values of the stack of Drawnelement
        /// </summary>
        /// <param name="elStack"></param>
        /// <returns>String the elements in elStack to string with "\n" in between</returns>
        public static string toString(this Stack<DrawnElement> elStack)
        {
            string toReturn = "";
            foreach (DrawnElement el in elStack)
                toReturn += el.ToString() + "\n";
            return toReturn;
        }
        /// <summary>
        /// Reverses the stack of Drawnelement. First becomes last element and vice versa.
        /// </summary>
        /// <param name="elStack"></param>
        /// <returns>`Stack<DrawnElement>`: elStack in reverse order.</returns>
        public static Stack<DrawnElement> reverse(this Stack<DrawnElement> elStack)
        {
            Stack<DrawnElement> drawOrder = new Stack<DrawnElement>();
            foreach (DrawnElement el in elStack)
                drawOrder.Push(el);
            return drawOrder;
        }

    }

}
