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
        List<string> tags = new List<string>();
        String text;
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

        /****************************************************************************/
        /****************************Check Consistency*******************************/
        static bool Check_Consistency(List<string> tags)
        {
            Stack<string> mystack = new Stack<string>();
            mystack.Push(tags[0]);           
            for (int i = 1; i < tags.Count; i++)
            {
                if (tags[i] == "frame")
                {
                    // do nothing
                }

                else if (tags[i] == "<!--")
                {
                    mystack.Push(tags[i]);
                }

                else if (tags[i] == "-->")
                {
                    if (mystack.Count == 0)
                    {
                        return false;
                    }


                    if (mystack.Peek() == "<!--")
                    {
                        mystack.Pop();
                    }
                    else
                    {
                        return false;
                    }
                }

                /*check if it's an end tag*/
                else if (tags[i].Contains('/'))
                {
                    string str = tags[i].TrimStart('/');
                    if (mystack.Count == 0)
                    {
                        return false;
                    }

                    
                    if (str.CompareTo(mystack.Peek()) == 0)
                    {
                        mystack.Pop();
                    }

                    else
                    {
                        return false;
                    }
                }
                else
                {
                    mystack.Push(tags[i]);
                }
            }
            if (mystack.Count != 0)
            {
                return false;
            }
            return true;
        }
        /****************************************************************************/

        //choose file
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            string line = " ";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string str = openFileDialog2.FileName;
                text = File.ReadAllText(str);
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

        private void xml_tags_to_array()
        {
            for (int i = 0; i < text.Count(); i++)
            {
                text = text.TrimStart();
                if (text[i] == '<')
                {
                    if (text[i + 1] == '!' && text[i + 2] == '-' && text[i + 3] == '-')
                    {
                        tags.Add("<!--");
                        for (int j = i + 4; j < text.Count(); j++)
                        {
                            if (text[j] == '-' && text[j + 1] == '-' && text[j + 2] == '>')
                            {
                                tags.Add("-->");
                                text = text.Substring(j + 3);
                                break;
                            }
                        }
                        i = -1;
                        continue;
                    }

                    int firstStringPosition = text.IndexOf("<");
                    int secondStringPosition = text.IndexOf(">");
                    int thirdStringPosition = text.IndexOf(" ");
                    int number = secondStringPosition;
                    if (thirdStringPosition < secondStringPosition && thirdStringPosition > firstStringPosition && thirdStringPosition != -1)
                        number = thirdStringPosition;
                    string stringBetweenTwoStrings = text.Substring(firstStringPosition + 1,
                    number - firstStringPosition - 1);

                    tags.Add(stringBetweenTwoStrings);
                    text = text.Substring(secondStringPosition + 1);
                    i = -1;
                }
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            xml_tags_to_array();
            for (int i = 0; i < tags.Count; i++)
            {
                Console.WriteLine(tags[i]);
            }
            bool status = Check_Consistency(tags);
            Console.WriteLine(status);

        }
    }
}
