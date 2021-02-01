using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class UndoRedoController
    {
        
        public List<DrawInstuction> UndoList = new List<DrawInstuction>();
        
        public List<DrawInstuction> RedoList = new List<DrawInstuction>();
        
        public void addInstruction(DrawInstuction instruction)
        {
            // Save the snapshot.
            UndoList.Add(instruction);

            // Empty the redo list.
            if (RedoList.Count > 0)
            {
                RedoList = new List<DrawInstuction>();
            }
        }

        public UndoRedoController(List<DrawInstuction> undo, List<DrawInstuction> redo)
        {
            UndoList = undo;
            RedoList = redo;
        }

        public UndoRedoController() { }

        public List<DrawInstuction> getElements()
        {
            return this.UndoList;
        }
        public DrawStorage getcurrentState(Size afmetingen, Bitmap backgroundImage = null)
        {
            //Bitmap backImage = (backgroundImage != null) ? backgroundImage : null;
            return new DrawStorage(this.UndoList, this.RedoList, afmetingen, (backgroundImage != null) ? backgroundImage : null);
        }
        
        public List<DrawInstuction> undo()
        {
            // Move the most recent change to the redo list.
            if (UndoList.Count != 0)
            {
                RedoList.Add(UndoList.Pop());
            }
            return UndoList.CustomReverse();
        }

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
}