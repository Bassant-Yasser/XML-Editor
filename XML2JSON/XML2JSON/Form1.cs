using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace XML2JSON
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //title
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog2.Filter = "Text Files (.txt)|*.txt";
        }


        //Arrow drawing 
        private void Form1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(Brushes.White, this.ClientRectangle);

            Pen p = new Pen(Color.Black, 10);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(p, 30, 30, 80, 30);
            p.Dispose();
        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            Invalidate();
        }



        //choose file
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            string line = " ";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string str = openFileDialog2.FileName;
                MessageBox.Show(str);
                StreamReader sr = new StreamReader(str);
                while (line != null)
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        listBox2.Items.Add(line);
                    }
                }
                sr.Close();
            }
        }
    }
}
