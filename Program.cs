using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Security;
using System.Collections.Generic;

namespace SchetsEditor
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                string fileName = args[0];
                //Check file exists
                if (File.Exists(fileName))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    if (fileName.ToLower().Contains(".jpg") || fileName.ToLower().Contains(".jpeg") || fileName.ToLower().Contains(".png") || fileName.ToLower().Contains(".bmp") || fileName.ToLower().Contains(".gif"))
                    {
                        try
                        {
                            var sr = new StreamReader(fileName);
                            Stream str = sr.BaseStream;
                            Bitmap openedImage = new Bitmap(str);
                            //new SchetsWin(openedImage).Show();
                            Application.Run(new SchetsWin(new DrawStorage(new List<DrawInstuction>(), new List<DrawInstuction>(), openedImage.Size, openedImage)));

                        }
                        catch (SecurityException ex)
                        {
                            MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                            $"Details:\n\n{ex.StackTrace}");
                            Application.Run(new SchetsWin());
                        }
                    }
                    else
                    {
                        MessageBox.Show("Dit bestandstype wordt niet ondersteund.");
                        Application.Run(new SchetsWin());
                    }
                }
                //The file does not exist
                else
                {
                    MessageBox.Show("The file does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new SchetsWin());
                }
                //Application.Run(new SchetsWin());
            }
            else
            {
                Application.Run(new SchetsWin());
            }

        }
    }
}
