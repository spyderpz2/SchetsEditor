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

namespace SchetsEditor
{
    public class SchetsWin : Form
    {   
        MenuStrip menuStrip;
        public UndoRedoController UndoRedoController;
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
        /*protected override void OnFormClosing(FormClosingEventArgs fea)
        {
            fea.Cancel = true;
            base.OnFormClosing(fea);
        }*/
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
                openDialog.Filter = "Paint ML | *.pml | Alle plaatjes | *.png;*.gif;*.bmp;*.jpg;*.jpeg";
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
                        ImageFormat format = Extension.getImageFormatFromFile(new FileInfo(openDialog.FileName));
                        if (format == ImageFormat.Emf)
                        {
                            string tekst = new StreamReader(str).ReadToEnd();
                            DrawStorage final = Extension.FromXML<DrawStorage>(tekst);
                            new SchetsWin(final, openDialog.FileName).Show();

                        }
                        else
                        {
                            Bitmap openedImage = new Bitmap(str);
                            new SchetsWin(new DrawStorage(new List<DrawInstuction>(), new List<DrawInstuction>(),openedImage.Size, openedImage), openDialog.FileName).Show();
                        }
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
                    ImageFormat format = Extension.getImageFormatFromFile(new FileInfo(this.bestandsNaam));
                    Bitmap achtergrond = this.schetscontrol.Schets.baseBitmap;
                    DrawStorage instructions = this.UndoRedoController.getcurrentState(this.schetscontrol.Schets.Afmeting, achtergrond != null ? (Bitmap)this.schetscontrol.Schets.baseBitmap.Clone() : null);
                    if (format == ImageFormat.Emf)
                    {
                        File.WriteAllText(this.bestandsNaam, Extension.ToXML<DrawStorage>(instructions), Encoding.UTF8);
                    }
                    else
                    {
                        byte[] bitmapBytes = tekening.ImageToByteArray(format);
                        File.WriteAllBytes(this.bestandsNaam, bitmapBytes);
                    }
                    this.lastDrawHash = instructions.ToByteArray().GetHash();
                    return;
                }
            }
            
            SaveFile(tekening, "Sla tekening op");
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

        public SchetsWin(DrawStorage openWithSettings = null, string fileName = null)
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
            string[] deKleuren = {"Black",  "White", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan" 
                                 };

            huidigeTool = deTools[0];

            if (openWithSettings != null) //openWithSettings is thus not empty
            {
                this.UndoRedoController = new UndoRedoController(openWithSettings.undo, openWithSettings.redo);
                this.schetscontrol = openWithSettings.backgroundImage != null ? new SchetsControl(openWithSettings.backgroundImage) : new SchetsControl();

                this.bestandsNaam = fileName;
                this.ClientSize = new Size(openWithSettings.dimensions.Width + 100, openWithSettings.dimensions.Height + 100);
                this.schetscontrol.Size = openWithSettings.dimensions;
                this.schetscontrol.Schets.Afmeting = openWithSettings.dimensions;
                openWithSettings.undo.DrawElements(this.schetscontrol.MaakBitmapGraphics());
                this.schetscontrol.Invalidate();
            }
            else
            {
                this.ClientSize = new Size(800, 600);
                UndoRedoController = new UndoRedoController();
                schetscontrol = new SchetsControl();
            }
            
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

        private void maakAktieMenu(string[] kleuren)
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
                this.nieuweSchets(null, null);
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                this.opslaanAls(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                this.opslaan(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.Z)
            {
                this.undo(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                this.redo(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.O)
            {
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
                opslaanDialog.Filter = "Paint ML | *.pml | PNG | *.png | GIF | *.gif | BMP | *.bmp | JPEG | *.jpg; *.jpeg";
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
                            ImageFormat extension = Extension.getImageFormatFromFile(bestandsInfo);
                            Bitmap achtergrond = this.schetscontrol.Schets.baseBitmap;
                            DrawStorage instructions = this.UndoRedoController.getcurrentState(this.schetscontrol.Schets.Afmeting, achtergrond != null ? (Bitmap)this.schetscontrol.Schets.baseBitmap.Clone() : null);

                            if (extension == ImageFormat.Emf)
                            {
                                byte[] xmlBytes = Encoding.UTF8.GetBytes(Extension.ToXML(instructions));
                                myStream.Write(xmlBytes, 0, xmlBytes.Length);
                            }
                            else
                            {
                                byte[] bitmapBytes = tekening.ImageToByteArray(extension);
                                myStream.Write(bitmapBytes, 0, bitmapBytes.Length);
                            }
                            this.lastDrawHash = instructions.ToByteArray().GetHash();

                        }
                        myStream.Close();
                    }
                }
            }
        }
    }

}