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
        List<int> errors_index = new List<int>();
        String text, text2;
        int flag = 0;
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
            richTextBox1.Clear();
            string line = " ";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string str = openFileDialog2.FileName;
                text = File.ReadAllText(str);
                text2 = File.ReadAllText(str);

                StreamReader sr = new StreamReader(str);
                while (line != null)
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        line = line + '\r' + '\n';
                        richTextBox1.Text += line; 
                    }
                }
                sr.Close();
            }
        }

        private void Fix_XML_Errors(int fix_show_errors)    //if show errors -> 0, fix errors -> 1
        {
            Stack<string> stack = new Stack<string>();
            Stack<int> spaces = new Stack<int>();

            int number = 0, errors = 0;
            flag = 1;

            string[] strings = text2.Split('\n');
            for (int i = 0; i < strings.Count(); i++)
            {
                if (strings[i].TrimStart()[0] == '<' && strings[i].TrimStart()[1] != '/')   //opening tag 
                {

                    int flag = 0;
                    int count = Count_Spaces(strings[i]);
                    if (spaces.Count == 0 || count > spaces.Peek())
                    {
                        spaces.Push(count);  //0 1 2  
                    }     
                    else
                    {
                        errors++;
                        stack.Pop();

                        Console.Write(richTextBox2.Lines);
                        List<string> myList = richTextBox2.Lines.ToList();
                        if (myList.Count > 0)
                        {
                            myList.RemoveAt(myList.Count - 2);
                            Console.WriteLine(myList);
                            richTextBox2.Clear();
                            for(int m = 0; m < myList.Count(); m++)
                            {
                                richTextBox2.SelectedText += myList.ToArray()[m];
                                if (m != myList.Count() - 1)
                                    richTextBox2.SelectedText += '\n';
                            }
                        }
                        
                        if(fix_show_errors == 0)
                        {
                            Font font = new Font("Tahoma", 8, FontStyle.Regular);
                            richTextBox2.SelectionFont = font;
                            richTextBox2.SelectionColor = Color.Red;
                            richTextBox2.SelectedText += strings[i - 1];
                        }

                        strings[i - 1] = strings[i - 1].TrimEnd();
                        strings[i - 1] += "</" + tags[number - 1] + ">";
                        Console.WriteLine(strings[i - 1]);
                        
                        if(fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += strings[i - 1] + Environment.NewLine;
                        }
                        flag = 1;
                    }
                    stack.Push(tags[number]);
                    number++;

                    string line = strings[i].TrimStart();
                    for (int j = 1; j < line.Count(); j++)
                    {
                        if (line[j] == '<' && line[j + 1] == '/') //closing fe nafs elsatr
                        {
                            flag = 1;
                            if (tags[number].Trim('/') == stack.Peek())
                            {
                                stack.Pop();

                                if(fix_show_errors == 1)
                                {
                                    richTextBox2.SelectedText += strings[i];
                                }

                                if(fix_show_errors == 0)
                                {
                                    Font font = new Font("Tahoma", 8, FontStyle.Regular);
                                    richTextBox2.SelectionFont = font;
                                    richTextBox2.SelectionColor = Color.Black;
                                    richTextBox2.SelectedText += strings[i];
                                }

                            }
                            else
                            {
                                stack.Pop();
                                errors++;

                                if(fix_show_errors == 0)
                                {
                                    Font font = new Font("Tahoma", 8, FontStyle.Regular);
                                    richTextBox2.SelectionFont = font;
                                    richTextBox2.SelectionColor = Color.Red;
                                    richTextBox2.SelectedText += strings[i];
                                }

                                int secondStringPosition = strings[i].IndexOf("/");
                                strings[i] = strings[i].Substring(0, secondStringPosition - 1);
                                strings[i] += "</";
                                strings[i] += tags[number-1];
                                strings[i] += '>';
                                Console.WriteLine(strings[i]);

                                if(fix_show_errors == 1)
                                {
                                    richTextBox2.SelectedText += strings[i] + Environment.NewLine;
                                }
                            }   
                            number++;
                            spaces.Pop();
                            
                        }
                    }
                    if(flag == 0)
                    {
                        if(fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += strings[i];
                        }

                        if(fix_show_errors == 0)
                        {
                            Font font = new Font("Tahoma", 8, FontStyle.Regular);
                            richTextBox2.SelectionFont = font;
                            richTextBox2.SelectionColor = Color.Black;
                            richTextBox2.SelectedText += strings[i];
                        }
                    }     
                }
                else if(strings[i].TrimStart()[0] == '<' && strings[i].TrimStart()[1] == '/')
                {
                    int count = Count_Spaces(strings[i]);
                    if(count == spaces.Peek() && tags[number].Trim('/') == stack.Peek())
                    {
                        number++;
                        spaces.Pop();
                        stack.Pop();

                        if(fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += strings[i];
                        }

                        if(fix_show_errors == 0)
                        {
                            Font font = new Font("Tahoma", 8, FontStyle.Regular);
                            richTextBox2.SelectionFont = font;
                            richTextBox2.SelectionColor = Color.Black;
                            richTextBox2.SelectedText += strings[i];
                        }

                    }
                    else if(count < spaces.Peek()) //zy halt el followers
                    {
                        errors++;
                        strings[i-1] += '\n';
                        for(int k = 0; k < spaces.Peek(); k++)
                        {
                            strings[i-1] += ' ';
                        }
                        strings[i-1] += "</" + stack.Peek() + '>';
                        Console.WriteLine(strings[i-1]);


                        String str = "";
                        for(int k = 0; k < spaces.Peek(); k++)
                        {
                            str += " ";
                        }
                        str += "</" + stack.Peek() + '>';

                        if(fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += str + Environment.NewLine;
                        }
                        

                        spaces.Pop();
                        stack.Pop();
                        while (stack.Peek() != tags[number].Trim('/'))
                        {
                            errors++;
                            String str2 = "";
                            for (int k = 0; k < spaces.Peek(); k++)
                            {
                                str2 += " ";
                            }
                            str2 += "</" + stack.Peek() + '>';

                            if (fix_show_errors == 1)
                            {
                                richTextBox2.SelectedText += str2 + Environment.NewLine;
                            }
                            stack.Pop();
                            spaces.Pop();
                        }

                        if(fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += strings[i];
                        }

                        if(fix_show_errors == 0)
                        {
                            Font font = new Font("Tahoma", 8, FontStyle.Regular);
                            richTextBox2.SelectionFont = font;
                            richTextBox2.SelectionColor = Color.Red;
                            richTextBox2.SelectedText += strings[i];
                        }

                        spaces.Pop();
                        if(stack.Count != 0)
                             stack.Pop();
                        number++;
                    }
                    else if(tags[number] != stack.Peek())
                    {
                        number++;
                        stack.Pop();
                        errors++;
                    }
                }
                else
                {
                    if(fix_show_errors == 1)
                    {
                        richTextBox2.SelectedText += strings[i];
                    }

                    if(fix_show_errors == 0)
                    {
                        Font font = new Font("Tahoma", 8, FontStyle.Regular);
                        richTextBox2.SelectionFont = font;
                        richTextBox2.SelectionColor = Color.Black;
                        richTextBox2.SelectedText += strings[i];
                    }
                }
            }
            Console.WriteLine(errors);
        }

        private int Count_Spaces(string str)
        {
            int count = 0;
            for(int i = 0; i < str.Count(); i++)
            {
                if (str[i] != ' ')
                    return count;
                count++;
            }
            return count;
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

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void button8_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            Fix_XML_Errors(1);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            Fix_XML_Errors(0);
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
