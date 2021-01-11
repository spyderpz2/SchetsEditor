using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.Drawing.Imaging;
using System.IO;
using System.Security;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

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
        string lastDrawHash;


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

        private void openFile(object obj, EventArgs ea)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Alle plaatjes | *.png;*.gif;*.bmp;*.jpg;*.jpeg";
                openDialog.FilterIndex = 1;
                openDialog.RestoreDirectory = true;
                openDialog.ValidateNames = true;
                openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openDialog.Title = "Open een plaatje om te bewerken";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sr = new StreamReader(openDialog.FileName);
                        Stream str = sr.BaseStream;
                        Bitmap openedImage = new Bitmap(str);
                        new SchetsWin(openedImage).Show();
                    }
                    catch (SecurityException ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                }
            }

            
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
                    this.lastDrawHash = this.UndoRedoController.getElements().ToByteArray().GetHash();
                    return;
                }
            }
            this.lastDrawHash = this.UndoRedoController.getElements().ToByteArray().GetHash();

            SaveFile(tekening, "Sla tekening op");
        }


        //Not finished yet.
        private void ser()
        {
            Stack<DrawInstuction> obj = this.UndoRedoController.getElements();//new DrawInstuction(ElementType.RechthoekOpen, Color.Black, new Point(50, 50), new Point(100, 100), 4);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@"C:\Users\Michiel Schouten\Desktop\hallo.txt", FileMode.Append, FileAccess.Write);

            formatter.Serialize(stream, obj);
            stream.Close();
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
            SaveFile(tekening, "&Sla tekening op als...");
        }

        private void afsluiten(object obj, EventArgs ea)
        {               
            //Check if the hashes are the same or not, thus whether the current drawing was saved.
            if (this.UndoRedoController.getElements().ToByteArray().GetHash() != this.lastDrawHash)
            {       
                Console.WriteLine("er waren wel wijzigingen");
                string message = "Sommige wijzigingen zijn nog niet opgeslagen, wil je deze opslaan alvorens af te sluiten?";
                string title = "Onopgeslagen werk";
                MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
                DialogResult result = MessageBox.Show(message, title, buttons,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RightAlign, false);
                if (result == DialogResult.Yes)
                    this.opslaan(null, null);
                else if (result == DialogResult.No)
                    this.Close();
                else
                    Console.WriteLine("nou niks"); 
            }
            else
            {
                Console.WriteLine("geen wijzigingen");
                this.Close();
            }
        }

        private void undo(object obj, EventArgs ea)
        {
            this.schetscontrol.Schets.Schoon();
            this.UndoRedoController.undo().DrawElements(this.schetscontrol.MaakBitmapGraphics());
            this.schetscontrol.Refresh();
        }

        private void redo(object obj, EventArgs ea) 
        {
            this.schetscontrol.Schets.Schoon();
            this.UndoRedoController.redo().DrawElements(this.schetscontrol.MaakBitmapGraphics());
            this.schetscontrol.Refresh();
        }


        public SchetsWin(Bitmap openMetBitmap = null)
        {
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(SchetsWinKeyDown);

            ISchetsTool[] deTools = { new PenTool()         
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new EllipseTool()
                                    , new VolEllipseTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan" 
                                 };

            this.ClientSize = new Size(800, 600);
            huidigeTool = deTools[0];

            schetscontrol = openMetBitmap != null ? new SchetsControl(openMetBitmap) : new SchetsControl();
            
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {
                                           vast = true;  
                                           huidigeTool.MuisVast(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {  if (vast)
                                          huidigeTool.MuisDrag(schetscontrol, mea.Location);
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {
                                           if (vast)
                                           {
                                               huidigeTool.MuisLos(schetscontrol, mea.Location, this.UndoRedoController);
                                               vast = false;
                                           }
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   huidigeTool.Letter(schetscontrol, kpea.KeyChar, this.UndoRedoController);
                                       };
            schetscontrol.Location = new Point(60, 30);
            schetscontrol.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
            this.Controls.Add(schetscontrol);

            


            menuStrip = new MenuStrip();
            menuStrip.Items.Add(this.maakFileMenu());
            this.maakToolMenu(deTools);
            this.maakAktieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
            menuStrip.Visible = true;
            this.Controls.Add(menuStrip);
        }

        private ToolStripMenuItem maakFileMenu()
        {   
            ToolStripMenuItem TFile = new ToolStripMenuItem("&File");
            TFile.DropDownItems.AddRange( new ToolStripItem[] { 
                maakItem("&New", new EventHandler(nieuweSchets), Keys.Control | Keys.Shift | Keys.N), 
                maakItem("&Open", new EventHandler(openFile), Keys.Control | Keys.O),
                maakItem("&Save", new EventHandler(opslaan), Keys.Control | Keys.S),
                maakItem("Save &As...", new EventHandler(opslaanAls), Keys.Control | Keys.Shift | Keys.S),
                maakItem("E&xit", new EventHandler(afsluiten), Keys.Control | Keys.W)
            } );
            return TFile;

        }

        private static ToolStripMenuItem maakItem(String name, EventHandler toClick, Keys shortcut)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(name);
            item.Click += toClick;
            item.ShortcutKeys = shortcut;
            return item;
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
                b.Size = new Size(45, 70);
                b.Location = new Point(10, 30 + t * 70);
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
            else if (e.Control && e.KeyCode == Keys.O)
            {
                Console.WriteLine("Open file");
                this.openFile(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.W)
            {
                this.afsluiten(null, null);
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
