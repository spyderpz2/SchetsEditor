using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {   
        MenuStrip menuStrip;
        public UndoRedoController UndoRedoController = new UndoRedoController();
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast;

        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );
        private string bestandsNaam = null;
        private void veranderAfmeting(object o, EventArgs ea)
        {
            //Zet hier de dimensies van het bord waar je daadwerkelijk op tekent
            schetscontrol.Size = new Size ( this.ClientSize.Width  - 70
                                          , this.ClientSize.Height - 80);
            paneel.Location = new Point(64, this.ClientSize.Height - 30);
        }

        private void klikToolMenu(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
        }

        private void klikToolButton(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
        }

        private void nieuweSchets(object obj, EventArgs ea)
        {
            new SchetsWin().Show();
        }

        private void opslaan(object obj, EventArgs ea)
        {
            Bitmap tekening = this.schetscontrol.Schets.tekening;
            
            if (this.bestandsNaam != null)
            {
                if (File.Exists(this.bestandsNaam))
                {
                    byte[] bitmapBytes = tekening.ToByteArray(this.getImageFormatFromFile(new FileInfo(this.bestandsNaam)));
                    File.WriteAllBytes(this.bestandsNaam, bitmapBytes);
                    return;
                }
            }

            SaveFile(tekening, "Sla tekening op");
        }

        private ImageFormat getImageFormatFromFile(FileInfo fileInfo)
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
                default:
                    return ImageFormat.Png;
            }
        }

        private void opslaanAls(object obj, EventArgs ea)
        {
            Bitmap tekening = this.schetscontrol.Schets.tekening;
            SaveFile(tekening, "Sla tekening op als");
        }

        private void afsluiten(object obj, EventArgs ea)
        {
            this.Close();
        }

        private void undo(object obj, EventArgs ea)
        {
            if (this.UndoRedoController.noneUndoRedoYet())
            {
                this.UndoRedoController.addState((Bitmap)this.schetscontrol.Schets.tekening.Clone());
            }


            this.schetscontrol.Schets.bitmap = this.UndoRedoController.undo();
            this.schetscontrol.Refresh();


        }

        private void redo(object obj, EventArgs ea) 
        {
            this.schetscontrol.Schets.bitmap = this.UndoRedoController.redo();
            this.schetscontrol.Refresh();
        }


        public SchetsWin()
        {
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(SchetsWinKeyDown);

            ISchetsTool[] deTools = { new PenTool()         
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan" 
                                 };

            this.ClientSize = new Size(800, 600);
            huidigeTool = deTools[0];

            schetscontrol = new SchetsControl();
            
            //schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {
                                           this.UndoRedoController.addState((Bitmap)this.schetscontrol.Schets.tekening.Clone());

                                           vast = true;  
                                           huidigeTool.MuisVast(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {
                                           if (vast)
                                           {
                                               huidigeTool.MuisLos(schetscontrol, mea.Location);
                                               vast = false;
                                           }
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar);
                                           if (kpea.KeyChar >= 32)
                                           {
                                               this.UndoRedoController.addState((Bitmap)this.schetscontrol.Schets.tekening.Clone());
                                           }
                                       };
            schetscontrol.Location = new Point(60, 30);
            schetscontrol.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
            this.Controls.Add(schetscontrol);

            menuStrip = new MenuStrip();
            
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakAktieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
            menuStrip.Visible = true;
            this.Controls.Add(menuStrip);
        }

        private void maakFileMenu()
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
            menu.DropDownItems.Add("Nieuw", null, this.nieuweSchets);
            menu.DropDownItems.Add("Opslaan", null, this.opslaan);
            menu.DropDownItems.Add("Opslaan als...", null, this.opslaanAls);
            menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }

        private void maakToolMenu(ICollection<ISchetsTool> tools)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools)
            {   ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakAktieMenu(String[] kleuren)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Aktie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
            ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
            foreach (string k in kleuren)
                submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
            menu.DropDownItems.Add(submenu);
            menuStrip.Items.Add(menu);
        }

        private void maakToolButtons(ICollection<ISchetsTool> tools)
        {
            int t = 0;
            foreach (ISchetsTool tool in tools)
            {
                RadioButton b = new RadioButton();
                b.Appearance = Appearance.Button;
                b.Size = new Size(45, 62);
                b.Location = new Point(10, 30 + t * 62);
                b.Tag = tool;
                b.Text = tool.ToString();
                b.Image = (Image)resourcemanager.GetObject(tool.ToString());
                b.TextAlign = ContentAlignment.TopCenter;
                b.ImageAlign = ContentAlignment.BottomCenter;
                b.Click += this.klikToolButton;
                this.Controls.Add(b);
                if (t == 0) b.Select();
                t++;
            }
        }

        private void maakAktieButtons(String[] kleuren)
        {   
            paneel = new Panel();
            paneel.Size = new Size(600, 24);
            this.Controls.Add(paneel);
            
            Button b; Label l; ComboBox cbb;
            b = new Button(); 
            b.Text = "Clear";  
            b.Location = new Point(  0, 0); 
            b.Click += schetscontrol.Schoon; 
            paneel.Controls.Add(b);
            
            b = new Button(); 
            b.Text = "Rotate"; 
            b.Location = new Point( 80, 0); 
            b.Click += schetscontrol.Roteer; 
            paneel.Controls.Add(b);
            
            l = new Label();  
            l.Text = "Penkleur:"; 
            l.Location = new Point(180, 3); 
            l.AutoSize = true;               
            paneel.Controls.Add(l);
            
            cbb = new ComboBox(); cbb.Location = new Point(240, 0); 
            cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
            cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            paneel.Controls.Add(cbb);
        }

        private void SchetsWinKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                // Your code to execute when shortcut Ctrl+N happens here
                Console.WriteLine("new window?");
                this.nieuweSchets(null, null);
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                // Your code to execute when shortcut Ctrl+S & Shift happens here
                Console.WriteLine("save as?");
                this.opslaanAls(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                // Your code to execute when shortcut Ctrl+S happens here
                Console.WriteLine("save?");
                this.opslaan(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.Z)
            {
                // Your code to execute when shortcut Ctrl+S happens here
                Console.WriteLine("undo?");
                this.undo(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                // Your code to execute when shortcut Ctrl+Y happens here
                Console.WriteLine("redo?");
                this.redo(null, null);
            }
        }


        private void SaveFile(Bitmap tekening, String filterBoxTitle)
        {

            Stream myStream;
            using (SaveFileDialog opslaanDialog = new SaveFileDialog())
            {
                opslaanDialog.Filter = "PNG | *.png | GIF | *.gif | BMP | *.bmp | JPEG | *.jpg; *.jpeg";
                opslaanDialog.FilterIndex = 1;
                opslaanDialog.RestoreDirectory = true;
                opslaanDialog.DefaultExt = ".png";
                opslaanDialog.ValidateNames = true;
                opslaanDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                opslaanDialog.Title = filterBoxTitle;

                if (this.bestandsNaam != null)
                {
                    Console.WriteLine("is al opgeslagen");
                    opslaanDialog.FileName = this.bestandsNaam;
                    opslaanDialog.InitialDirectory = @"" + this.bestandsNaam;
                }

                if (opslaanDialog.ShowDialog() == DialogResult.OK)
                {
                    using (myStream = opslaanDialog.OpenFile())
                    {
                        if (myStream != null)
                        {
                            FileInfo bestandsInfo = new FileInfo(opslaanDialog.FileName);
                            this.bestandsNaam = opslaanDialog.FileName;
                            this.Text = bestandsInfo.Name;
                            byte[] bitmapBytes = tekening.ToByteArray(this.getImageFormatFromFile(bestandsInfo));
                            myStream.Write(bitmapBytes, 0, bitmapBytes.Length);
                        }
                        myStream.Close();

                    }
                }
            }

        }



    }

    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }

}
