using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class UndoRedoController
    {
        private Stack<Bitmap> UndoList = new Stack<Bitmap>();
        private Stack<Bitmap> RedoList = new Stack<Bitmap>();
        public int test = 0;

        public void addState(Bitmap state)
        {
            // Save the snapshot.
            UndoList.Push(state);
            // Empty the redo list.
            if (RedoList.Count > 0)
            {
                RedoList = new Stack<Bitmap>();
            }
        }

        public bool noneUndoRedoYet()
        {
            return this.RedoList.Count == 0;
        }

        public Bitmap undo()
        {
            // Move the most recent change to the redo list.
            if (UndoList.Count != 0)
            {
                RedoList.Push(UndoList.Pop());
            }
            // Restore the top item in the Undo list.
            return RedoList.Peek();
        }

        public Bitmap redo()
        {
            // Move the most recently undone item back to the undo list.
            if (RedoList.Count != 0)
            {
                UndoList.Push(RedoList.Pop());
            }
            // Restore the top item in the Undo list.
            return UndoList.Peek();
        }

    }
}
