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
        #region Variables
        static string minifiedoutput;
        static string path;
        static List<treeNode> nodes = new List<treeNode>();
        xmlTree tree = new xmlTree();
        static Stack<int> Tag = new Stack<int>();
        static Stack<treeNode> st = new Stack<treeNode>();

        List<string> tags = new List<string>();
        List<int> errors_index = new List<int>();
        String text, text2;
        int flag = 0;
        #endregion
        public Form1()
        {
            InitializeComponent();
        }
        void cutter(string l)
        {
            var start = 0;
            var end = 0;
            string line; 
            char d = ' ';
            for (int i = 0; i < l.Length; i++)
            {
                if (l[i] == '<')
                {
                    start = i;
                    for (; i < l.Length; i++)
                    {
                        if (l[i] == '>')
                        {
                            end = i;
                            break;
                        }
                    }
                }
                else
                {
                    start = i;
                    for (; i < l.Length; i++)
                    {
                        if (l[i] == '<')
                        {
                            end = i - 1;
                            i--;
                            break;
                        }
                        else
                        {
                            end = i;
                        }

                    }
                }
                line = l.Substring(start, end - start + 1);
                if (line[0] != '<')
                    for (int j = 0; i < line.Length; i++)
                    {
                        if (line[i] == '"')
                            line.Replace(line[i], d);
                    }
                if (line[0] != ' ' || line[1] != ' ' || line[2] != ' ')
                {
                    listBox3.Items.Add(line);
                }
                
                
            }

        }

        /****************************************************************************/
        /**************************Compress & Decompress*****************************/
        public static List<short> Compress(string uncompressed)
        {
            // build the dictionary
            Dictionary<string, short> dictionary = new Dictionary<string, short>();
            for (short i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            string w = string.Empty;
            List<short> compressed = new List<short>();

            foreach (char c in uncompressed)
            {
                string wc = w + c;
                if (dictionary.ContainsKey(wc))
                {
                    w = wc;
                }
                else
                {
                    // write w to output
                    compressed.Add(dictionary[w]);
                    // wc is a new sequence; add it to the dictionary
                    dictionary.Add(wc, (short)dictionary.Count);
                    w = c.ToString();
                }
            }

            // write remaining output if necessary
            if (!string.IsNullOrEmpty(w))
                compressed.Add(dictionary[w]);

            return compressed;
        }

        /*****************************Decompress********************************/
        public static string Decompress(List<short> compressed)
        {
            // build the dictionary
            Dictionary<short, string> dictionary = new Dictionary<short, string>();
            for (short i = 0; i < 256; i++)
                dictionary.Add(i, ((char)i).ToString());

            string w = dictionary[compressed[0]];
            compressed.RemoveAt(0);
            StringBuilder decompressed = new StringBuilder(w);

            foreach (short k in compressed)
            {
                string entry = null;
                if (dictionary.ContainsKey(k))
                    entry = dictionary[k];
                else if (k == dictionary.Count)
                    entry = w + w[0];

                decompressed.Append(entry);

                // new sequence; add it to the dictionary
                dictionary.Add((short)dictionary.Count, w + entry[0]);
                w = entry;
            }

            return decompressed.ToString();
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


        public class treeNode
        {
            #region fields
            public string tagName;
            public string attr;
            public string data; 
            public List<treeNode> children;
            public treeNode parent;
            #endregion
            #region constructor
            public treeNode()
            {
                tagName = "";
                attr = "";
                data = "";
                parent = null;
                children = new List<treeNode>();
            }
            #endregion
            #region methods
            public bool addChild(treeNode child)
            {
                if (children.Contains(child))
                {
                    return false;
                }
                else if (child == this)
                {
                    return false;
                }
                else
                {
                    children.Add(child);
                    child.parent = this;
                    return true;
                }
            }
            #endregion
        }
        public class xmlTree
        {
            #region fields
            treeNode root;
            List<treeNode> nodes = new List<treeNode>();
            #endregion
            #region constructor
            public xmlTree()
            {
                root = new treeNode();
                nodes.Add(root);
            }
            #endregion
            #region properties
            public int count
            {
                get { return nodes.Count; }
            }
            public treeNode Root
            {
                get { return root; }
            }
            #endregion
            #region methods
            public void addNode(treeNode p, treeNode node)
            {
                node.parent = p;
                nodes.Add(node);
                node.parent.addChild(node);
            }
            public treeNode createNode(string name, string att)
            {
                treeNode n = new treeNode();
                n.tagName = name;
                n.attr = att;
                return n;
            }
            public void addRoot(treeNode n)
            {
                root = n;
            }
            public void addData(treeNode n, string d)
            {
                n.data += d;
            }
            
            public int height(treeNode node)
            {
                if (node == root)
                {
                    return 0;
                }
                return (1 + height(node.parent));
            }
            #endregion

        }

        void createTree(string file)
        {
            string input = " ";
            StreamReader sr = new StreamReader(file);
            while (input != null)
            {
                input = sr.ReadLine();
                if (input != null)
                {
                    if (input[0] == '<' && (input[1] == '!' || input[1] == '?'))
                    {
                        continue;
                    }
                    if (input[0] == '<' && input[1] != '/')
                    {

                        var index = input.Length - 1;
                        var flag = false;
                        // Searching for the space to get the tag name only from the line
                        if(input.Contains(" "))
                        {
                            index = input.IndexOf(" ");
                            //First white space in the tag
                            flag = true;
                            break;
                        }
         
                        string tag = input.Substring(1, index - 1);
                        if (!flag)
                        {
                            nodes.Add(tree.createNode(tag, ""));//decalre a new tag 
                        }
                        else
                        {
                            string att = input.Substring(index + 1, (input.Length - index - 2));
                            nodes.Add(tree.createNode(tag, att));//decalre a new tag 
                        }
                        
                        if (Tag.Count() == 0)
                        {
                            tree.addRoot(nodes[nodes.Count() - 1]);//add tree root to the tree
                        }
                        else
                        {
                            tree.addNode(nodes[Tag.Peek()], nodes[nodes.Count() - 1]);//add a child to the last opened tag
                        }
                        Tag.Push(nodes.Count() - 1);//add the last opened tag to deal with it to add children or data to it 
                        if (input[input.Length - 2] == '/')
                        {
                            Tag.Pop();//self closing tag 
                        }
                    }
                    //Closing tag
                    else if (input[0] == '<' && input[1] == '/')
                    {
                        Tag.Pop();//remove the last opend tag (closed) to deal with the next tag to it 
                    }
                    //Data
                    else
                    {
                        tree.addData(nodes[Tag.Peek()], input);//add data to the last opened tag

                    }
                }
                
            }
            
        }


        public void print(xmlTree tree, treeNode r)
        {
            int tab = tree.height(r) + 1; //level num + 1 extra tab 
            if (r == tree.Root)
            {
                Console.WriteLine("{");
                richTextBox2.SelectedText = "{" + Environment.NewLine;
            }
            for (int k = 0; k < tab; k++)
            {
                Console.Write("\t");
                richTextBox2.SelectedText = "\t";
            }
            Console.Write('"' + r.tagName + '"' + ": ");
            richTextBox2.SelectedText = '"' + r.tagName + '"' + ": ";
            if (r.children.Count() == 0)
            {
                Console.Write('"' + r.data + '"');
                richTextBox2.SelectedText = '"' + r.data + '"';
                int index = r.parent.children.IndexOf(r);
                if (index + 1 != r.parent.children.Count())
                {
                    Console.WriteLine(",");
                    richTextBox2.SelectedText = "," + Environment.NewLine;
                }
            }
            else if (r.children.Count() == 1)
            {
                Console.WriteLine("[");
                richTextBox2.SelectedText = "[" + Environment.NewLine;
                foreach (treeNode i in r.children)
                {
                    print(tree, i);
                }
                for (int k = 0; k < tab; k++)
                {
                    Console.Write("\t");
                    richTextBox2.SelectedText = "\t";
                }
                Console.WriteLine("]");
                richTextBox2.SelectedText = "]" + Environment.NewLine;
            }
            else if (r.children.Count() > 0)
            {
                var flag = false;
                treeNode x = r.children[0];
                foreach (treeNode p in r.children)
                {
                    if (p.tagName != x.tagName)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    Console.WriteLine("{");
                    richTextBox2.SelectedText = "{" + Environment.NewLine;
                    foreach (treeNode i in r.children)
                    {
                        print(tree, i);
                    }
                }
                else
                {
                    Console.WriteLine("[");
                    richTextBox2.SelectedText = "[" + Environment.NewLine;
                    foreach (treeNode i in r.children)
                    {
                        if (i.children.Count() > 0)
                        {
                            int j = tree.height(i) + 1;
                            for (int k = 0; k < j; k++)
                            {
                                Console.Write("\t");
                                richTextBox2.SelectedText = "\t";
                            }
                            Console.WriteLine("{");
                            richTextBox2.SelectedText = "{" + Environment.NewLine;
                            foreach (treeNode f in i.children)
                            {
                                print(tree, f);
                            }
                            int g = tree.height(i) + 1;
                            Console.Write("\n");
                            richTextBox2.SelectedText = Environment.NewLine;
                            for (int k = 0; k < g; k++)
                            {
                                Console.Write("\t");
                                richTextBox2.SelectedText = "\t";
                            }
                            Console.Write("}");
                            richTextBox2.SelectedText = "}";
                            int index = r.children.IndexOf(i);
                            if (index + 1 != r.children.Count())
                            {
                                Console.WriteLine(",");
                                richTextBox2.SelectedText = "," + Environment.NewLine;
                            }
                            else
                            {
                                Console.WriteLine();
                                richTextBox2.SelectedText = Environment.NewLine;
                            }
                        }
                        else if (i.children.Count() == 0)
                        {
                            int y = tree.height(i) + 1;
                            for (int k = 0; k < y; k++)
                            {
                                Console.Write("\t");
                                richTextBox2.SelectedText = "\t";
                            }
                            Console.Write('"' + i.data + '"');
                            richTextBox2.SelectedText = '"' + i.data + '"';
                            int index = r.children.IndexOf(i);
                            if (index + 1 != r.children.Count())
                            {
                                Console.WriteLine(",");
                                richTextBox2.SelectedText = "," + Environment.NewLine;
                            }
                            else
                            {
                                Console.WriteLine();
                                richTextBox2.SelectedText = Environment.NewLine;
                            }
                        }
                    }
                }
                for (int k = 0; k < tab; k++)
                {
                    Console.Write("\t");
                    richTextBox2.SelectedText = "\t";
                }
                if (flag)
                {
                    Console.WriteLine("}");
                    richTextBox2.SelectedText = "}" + Environment.NewLine;
                }
                else
                {
                    Console.Write("]");
                    richTextBox2.SelectedText = "]";
                    int index = r.parent.children.IndexOf(r);
                    if (index + 1 != r.parent.children.Count())
                    {
                        Console.WriteLine(",");
                        richTextBox2.SelectedText = "," + Environment.NewLine;
                    }
                    else
                    {
                        Console.WriteLine();
                        richTextBox2.SelectedText = Environment.NewLine;
                    }
                }
            }
            if (r == tree.Root)
            {
                Console.WriteLine("}");
                richTextBox2.SelectedText = "}" + Environment.NewLine;
            }
        }


        public void format(xmlTree tree , treeNode r, ref Stack<treeNode> s)
        {
            string l = "";
            if (r == tree.Root)
            {
                richTextBox2.SelectedText = "<" + r.tagName + ">" + Environment.NewLine;
                //Console.WriteLine("<" + r.tagName + ">");
                l = "";
                l = "<" + r.tagName + ">";
                s.Push(r);
            }
            else
            {
                int i = tree.height(r);
                l = "";
                for (int k = 0; k < i; k++)
                {
                    //Console.Write("\t");
                    l += "\t";
                }
                if (r.data != "")
                {
                    
                    //Console.WriteLine("<" + r.tagName + ">" + r.data + "</" + r.tagName + ">");
                    l += "<" + r.tagName + ">" + r.data + "</" + r.tagName + ">";
                    richTextBox2.SelectedText = l + Environment.NewLine;
                }
                else
                {
                    //Console.WriteLine("<" + r.tagName + ">");
                    l += "<" + r.tagName + ">";
                    richTextBox2.SelectedText = l + Environment.NewLine;
                    s.Push(r);
                }

            }
            if (r.children.Count() > 0)
            {
                foreach (treeNode i in r.children)
                {
                    format(tree, i, ref s);
                }
                if (s.Count() != 0)
                {
                    treeNode n = s.Peek();
                    s.Pop();
                    int f = tree.height(n);
                    l = "";
                    for (int k = 0; k < f; k++)
                    {
                        //Console.Write("\t");
                        l += "\t";
                    }
                    //Console.WriteLine("</" + n.tagName + ">");
                    l += "</" + n.tagName + ">";
                    richTextBox2.SelectedText = l + Environment.NewLine;
                }
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

        /****************************************************************************/
        /*******************************Fix Errors**********************************/
        private void Fix_XML_Errors(int fix_show_errors)    //if show errors -> 0, fix errors -> 1
        {
            Stack<string> stack = new Stack<string>(); // holds the tags
            Stack<int> spaces = new Stack<int>(); // spaces at the begining of a line tag

            int number = 0, errors = 0;
            flag = 1;

            string[] strings = text2.Split('\n');
            for (int i = 0; i < strings.Count(); i++)
            {
                if (strings[i].TrimStart()[0] == '<' && strings[i].TrimStart()[1] != '/')   //opening tag 
                {
                    int length = strings[i].TrimEnd().Count();
                    if (strings[i].TrimStart()[1] == '!' && strings[i].TrimEnd()[length - 1] == '>' && strings[i].TrimEnd()[length - 2] == '-')
                    {
                        // ignore
                        number += 2;
                        richTextBox2.SelectedText += strings[i];
                        continue;
                    }
                    if (strings[i].TrimStart()[1] == '?' && strings[i].TrimEnd()[length - 1] == '>' && strings[i].TrimEnd()[length - 2] == '?')
                    {
                        // ignore
                        number++;
                        richTextBox2.SelectedText += strings[i];
                        continue;
                    }
                    if (strings[i].TrimEnd()[length - 1] == '>' && strings[i].TrimEnd()[length - 2] == '/')
                    {
                        // ignore self closing tag
                        number++;
                        richTextBox2.SelectedText += strings[i];
                        continue;
                    }
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
                            for (int m = 0; m < myList.Count(); m++)
                            {
                                richTextBox2.SelectedText += myList.ToArray()[m];
                                if (m != myList.Count() - 1)
                                    richTextBox2.SelectedText += '\n';
                            }
                        }

                        if (fix_show_errors == 0)
                        {
                            richTextBox2.SelectionColor = Color.Red;
                            richTextBox2.SelectedText += strings[i - 1];
                        }

                        strings[i - 1] = strings[i - 1].TrimEnd();
                        strings[i - 1] += "</" + tags[number - 1] + ">";
                        Console.WriteLine(strings[i - 1]);

                        if (fix_show_errors == 1)
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

                                if (fix_show_errors == 1)
                                {
                                    richTextBox2.SelectedText += strings[i];
                                }

                                if (fix_show_errors == 0)
                                {
                                    richTextBox2.SelectionColor = Color.Black;
                                    richTextBox2.SelectedText += strings[i];
                                }

                            }
                            else
                            {
                                stack.Pop();
                                errors++;

                                if (fix_show_errors == 0)
                                {
                                    richTextBox2.SelectionColor = Color.Red;
                                    richTextBox2.SelectedText += strings[i];
                                }

                                int secondStringPosition = strings[i].IndexOf("/");
                                strings[i] = strings[i].Substring(0, secondStringPosition - 1);
                                strings[i] += "</";
                                strings[i] += tags[number - 1];
                                strings[i] += '>';
                                Console.WriteLine(strings[i]);

                                if (fix_show_errors == 1)
                                {
                                    richTextBox2.SelectedText += strings[i] + Environment.NewLine;
                                }
                            }
                            number++;
                            spaces.Pop();

                        }
                    }
                    if (flag == 0)
                    {
                        if (fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += strings[i];
                        }

                        if (fix_show_errors == 0)
                        {
                            richTextBox2.SelectionColor = Color.Black;
                            richTextBox2.SelectedText += strings[i];
                        }
                    }
                }
                else if (strings[i].TrimStart()[0] == '<' && strings[i].TrimStart()[1] == '/')
                {
                    int count = Count_Spaces(strings[i]);
                    if (count == spaces.Peek() && tags[number].Trim('/') == stack.Peek())
                    {
                        number++;
                        spaces.Pop();
                        stack.Pop();

                        if (fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += strings[i];
                        }

                        if (fix_show_errors == 0)
                        {
                            richTextBox2.SelectionColor = Color.Black;
                            richTextBox2.SelectedText += strings[i];
                        }

                    }
                    else if (count < spaces.Peek()) //zy halt el followers
                    {
                        errors++;
                        strings[i - 1] += '\n';
                        for (int k = 0; k < spaces.Peek(); k++)
                        {
                            strings[i - 1] += ' ';
                        }
                        strings[i - 1] += "</" + stack.Peek() + '>';
                        Console.WriteLine(strings[i - 1]);


                        String str = "";
                        for (int k = 0; k < spaces.Peek(); k++)
                        {
                            str += " ";
                        }
                        str += "</" + stack.Peek() + '>';

                        if (fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += str + Environment.NewLine;
                        }


                        spaces.Pop();
                        stack.Pop();
                        if (stack.Count() != 0)
                        {
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
                                if (stack.Count() == 0)
                                {
                                    break;
                                }
                            }
                        }
                        

                        if (fix_show_errors == 1)
                        {
                            richTextBox2.SelectedText += strings[i];
                        }

                        if (fix_show_errors == 0)
                        {
                            richTextBox2.SelectionColor = Color.Red;
                            richTextBox2.SelectedText += strings[i];
                        }

                        spaces.Pop();
                        if (stack.Count != 0)
                            stack.Pop();
                        number++;
                    }
                    else if (tags[number] != stack.Peek())
                    {
                        number++;
                        stack.Pop();
                        errors++;
                    }
                }
                else
                {
                    if (fix_show_errors == 1)
                    {
                        richTextBox2.SelectedText += strings[i];
                    }

                    if (fix_show_errors == 0)
                    {
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
            for (int i = 0; i < str.Count(); i++)
            {
                if (str[i] != ' ')
                    return count;
                count++;
            }
            return count;
        }

        //title
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog2.Filter = "XML Files (.xml)|*.xml";
        }
        //choose file
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            string line = " ";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.Clear();
                richTextBox2.Clear();
                path = openFileDialog2.FileName;
                text = File.ReadAllText(path);
                text2 = File.ReadAllText(path);
                label2.Text = path;
                StreamReader sr = new StreamReader(path);
                while (line != null)
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        richTextBox1.SelectedText = line + Environment.NewLine;
                        int c = Count_Spaces(line);
                        line = line.Remove(0, c);
                        while (line.Contains("\t"))
                        {
                            line = line.Remove(0, 1);
                        }
                        cutter(line);
                        
                    }
                }
                sr.Close();
            }
            path = Path.GetDirectoryName(path) + "/Formatit.xml";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            //File.Create(path);
            foreach (string i in listBox3.Items)
            {
                File.AppendAllText(path, i + "\n");
            }
            xml_tags_to_array();
            bool status = Check_Consistency(tags);
            //Console.WriteLine(status);
            
            if (status)
            {
                createTree(path);
            }
            
            
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        //Format button
        private void button8_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            format(tree, tree.Root, ref st);




        }
        //save file
        private void button4_Click(object sender, EventArgs e)
        {
            
         
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        //Minify
        private void button9_Click(object sender, EventArgs e)
        {
            
            foreach(string i in listBox3.Items)
            {
                richTextBox2.SelectedText = i;
                minifiedoutput += i;
            }
        }
        //xml2json
        private void button10_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            print(tree, tree.Root);
        }
        //fix errors
        private void button11_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            Fix_XML_Errors(1);
        }
        //show errors
        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            Fix_XML_Errors(0);
        }
        
        //compress
        private void button5_Click(object sender, EventArgs e)
        {
            path = Path.GetDirectoryName(path) + "/Formatit.xml";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            List<short> compressed;

            compressed = Compress(minifiedoutput);
            File.AppendAllText(path, string.Join("", compressed));
            string decompressed = Decompress(compressed);
            Console.WriteLine(decompressed);
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        //check consistency
        private void button6_Click(object sender, EventArgs e)
        {
            xml_tags_to_array();
            for (int i = 0; i < tags.Count; i++)
            {
                //Console.WriteLine(tags[i]);
            }
            bool status = Check_Consistency(tags);
            //Console.WriteLine(status);
        }
    }   
}
