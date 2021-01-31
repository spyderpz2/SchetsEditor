using System;
using System.Drawing;
using System.Windows.Forms;

namespace SchetsEditor
{
    public class Hoofdscherm : Form
    {
        MenuStrip menuStrip;
        //lol
        public Hoofdscherm()
        {   this.ClientSize = new Size(800, 600);
            menuStrip = new MenuStrip();
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakHelpMenu();
            this.Text = "Schets editor";
            this.IsMdiContainer = false;
            this.MainMenuStrip = menuStrip;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(HoofdschermKeyDown);
        }

        private void maakFileMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("File");
            menu.DropDownItems.Add("Nieuw", null, this.nieuw);
            menu.DropDownItems.Add("Exit", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }
        private void maakHelpMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("Help");
            menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
            menuStrip.Items.Add(menu);
        }
        private void about(object o, EventArgs ea)
        {   MessageBox.Show("Schets versie 1.0\n(c) UU Informatica 2010"
                           , "Over \"Schets\""
                           , MessageBoxButtons.OK
                           , MessageBoxIcon.Information
                           );
        }

        private void nieuw(object sender, EventArgs e)
        {   SchetsWin s = new SchetsWin();
            //s.MdiParent = this;
            s.Show();
        }
        private void afsluiten(object sender, EventArgs e)
        {   this.Close();
        }

        private void HoofdschermKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                // Your code to execute when shortcut Ctrl+N happens here
                Console.WriteLine("new window?");
                this.nieuw(null, null);
            }
        }
    }
}
