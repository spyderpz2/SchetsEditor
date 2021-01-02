using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class UndoRedoController
    {
        /// <summary>
        /// The list containing all of the current `DrawCommand`s.
        /// </summary>
        private Stack<DrawCommand> UndoList = new Stack<DrawCommand>();
        /// <summary>
        /// The list containing all of the elements that can be redone, only being filled after calling `undo()` method.
        /// </summary>
        private Stack<DrawCommand> RedoList = new Stack<DrawCommand>();

        /// <summary>
        /// Adds the drawing command to the `UndoList` so it can be retrieved later.
        /// </summary>
        /// <param name="command">The drawing command to be added to the list.</param>
        public void addCommand(DrawCommand command)
        {
            // Save the snapshot.
            UndoList.Push(command);
            //Console.WriteLine("hallo?");
            
            // Empty the redo list.
            if (RedoList.Count > 0)
            {
                RedoList = new Stack<DrawCommand>();
            }
        }

        public Stack<DrawCommand> getElements()
        {
            return this.UndoList;
        }

        /// <summary>
        /// Undoes a 'commit' or drawing action made by the user. E.g. it removes the last drawing from the list.
        /// </summary>
        /// <returns>The remaining drawing commands in reverse order e.g. the order they were drawn in</returns>
        public Stack<DrawCommand> undo()
        {
            // Move the most recent change to the redo list.
            if (UndoList.Count != 0)
            {
                RedoList.Push(UndoList.Pop());
            }
            return UndoList.reverse();
        }

        /// <summary>
        /// Redoes a 'commit' or drawing action made by the user. E.g. it adds the n-last drawing from the RedoList to the UndoList.
        /// </summary>
        /// <returns>The remaining drawing commands in the order they were drawn in</returns>
        public Stack<DrawCommand> redo()
        {
            // Move the most recently undone item back to the undo list.
            if (RedoList.Count != 0)
            {
                UndoList.Push(RedoList.Pop());
            }
            return UndoList;
        }

    }

    /// <summary>
    /// Holds all the information to recreate a drawing action by the user.
    /// </summary>
    public struct DrawCommand
    {
        /// <summary>
        /// Assign the values of the draw command to each corresponding variable
        /// </summary>
        /// <param name="elType">The type of the drawing command e.g. FillRectangle would be ElementType.RechthoekDicht</param>
        /// <param name="elKleur">The color of the element to draw.</param>
        /// <param name="elStartPunt">The startingpoint of the element to draw.</param>
        /// <param name="elEindPunt">The endpoint of the element to draw.</param>
        /// <param name="elLijnDikte">The thickness or width of the line(s) of the element to draw.</param>
        public DrawCommand(ElementType elType, Color elKleur, Point elStartPunt, Point elEindPunt, int elLijnDikte = 3)
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
        /// <summary>
        /// Facilitates debugging. 
        /// </summary>
        /// <returns>Returns a custom represention of the current drawing command in a `String`.</returns>
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
        /// <param name="elStack">The stack of `DrawCommand` to be returned as a contious string.</param>
        /// <returns>String the elements in elStack to string with "\n" in between</returns>
        public static string toString(this Stack<DrawCommand> elStack)
        {
            string toReturn = "";
            foreach (DrawCommand el in elStack)
                toReturn += el.ToString() + "\n";
            return toReturn;
        }

        /// <summary>
        /// Reverses the stack of `DrawCommand`. First becomes last element and vice versa.
        /// </summary>
        /// <param name="elStack">The stack of `DrawCommand` to be returned in reverse order.</param>
        /// <returns>`Stack<DrawCommand>`: elStack in reverse order.</returns>
        public static Stack<DrawCommand> reverse(this Stack<DrawCommand> elStack)
        {
            Stack<DrawCommand> drawOrder = new Stack<DrawCommand>();
            foreach (DrawCommand el in elStack)
                drawOrder.Push(el);
            return drawOrder;
        }

        /// <summary>
        /// Redraws the elements in the given stack on the given `Graphics` object.
        /// </summary>
        /// <param name="elStack">The stack of `DrawCommand` to be drawn</param>
        /// <param name="toDrawOn">The `Graphics` object to be drawn on.</param>
        public static void drawElements(this Stack<DrawCommand> elStack, Graphics toDrawOn)
        {
            using (toDrawOn)
            {
                foreach (DrawCommand elToDraw in elStack)
                {
                    switch (elToDraw.elementType)
                    {
                        case ElementType.Lijn:
                            toDrawOn.DrawLine(elToDraw.CreatePen(), elToDraw.startPunt, elToDraw.eindPunt);
                            break;
                        case ElementType.RechthoekOpen:
                            toDrawOn.DrawRectangle(elToDraw.CreatePen(), elToDraw.ToRectangle());
                            break;
                        case ElementType.RechthoekDicht:
                            toDrawOn.FillRectangle(elToDraw.CreateBrush(), elToDraw.ToRectangle());
                            break;
                        case ElementType.ElipseOpen:
                            toDrawOn.DrawEllipse(elToDraw.CreatePen(), elToDraw.ToRectangle());
                            break;
                        case ElementType.ElipseDicht:
                            toDrawOn.FillEllipse(elToDraw.CreateBrush(), elToDraw.ToRectangle());
                            break;
                        default:
                            break;
                    }
                }
            }
        }


    }

}
