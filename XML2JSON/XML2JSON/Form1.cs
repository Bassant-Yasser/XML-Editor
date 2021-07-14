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
        static string decompressed;
        static string path;
        static string savedFile;
        static string p;
        static string pc;
        static List<treeNode> nodes = new List<treeNode>();
        xmlTree tree = new xmlTree();
        xmlTree ctree = new xmlTree();
        static Stack<int> Tag = new Stack<int>();
        static Stack<treeNode> st = new Stack<treeNode>();

        List<string> tags = new List<string>();
        List<int> errors_index = new List<int>();
        String text, text2, t;
        int flag = 0;
        bool status = true;
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        //create xml file of a cutted data in each line
        void cutter(string l, string p)
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
                    File.AppendAllText(p, line + "\n");
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
                if (tags[i] == "frame/")
                {
                    // do nothing
                }
                else if (tags[i] == "?xml")
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
            public bool RemoveAllChildren()
            {
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    children[i].parent = null;
                    children.RemoveAt(i);
                }
                return true;
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
            public void Clear()
            {
                // remove all the children from each node
                // so nodes can be garbage collected
                foreach (treeNode node in nodes)
                {
                    node.parent = null;
                    node.RemoveAllChildren();
                }

                // now remove all the nodes from the tree and set root to null
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    nodes.RemoveAt(i);
                }
                root = null;
            }
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
            public void p(treeNode r)
            {
                if(r == root)
                {
                    Console.WriteLine(r.tagName);
                }
                if(r.children.Count() > 0)
                {
                    foreach(treeNode i in r.children)
                    {
                        Console.WriteLine(i.tagName + "\t" + i.data + "\t" + i.attr + "\t" + "parent: " + i.parent.tagName);
                    }
                    foreach(treeNode j in r.children)
                    {
                        p(j);
                    }
                }
            }
            #endregion
        }
        //create tree with 
        void createTree(string file)
        {
            treeNode n = tree.createNode("null", "");
            tree.addRoot(n);//add tree root to the tree
            nodes.Add(n);
            Tag.Push(nodes.Count() - 1);//add the last opened tag to deal with it to add children or data to it 
            string input = " ";
            StreamReader sr = new StreamReader(file);
            while (input != null)
            {
                input = sr.ReadLine();
                if (input != null)
                {
                    //comment or self closing tag ba5od el tag kloh
                    if (input[0] == '<' && (input[1] == '!' || input[1] == '?' || input[input.Length - 2] == '/'))
                    {
                        treeNode c = tree.createNode(input, "");//decalre a new tag
                        tree.addNode(nodes[Tag.Peek()], c);
                        continue;
                    }
                    //opening tag
                    if (input[0] == '<' && (input[1] != '/'))// || input[1] != '!' || input[1] != '?' || input[input.Length - 2] != '/'))
                    {
                        var index = input.Length - 1;
                        var flag = false;
                        // Searching for the space to get the tag name only from the line
                        if (input.Contains(" "))
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
                            //Console.WriteLine(nodes.Count());
                        }
                        else
                        {
                            string att = input.Substring(index + 1, (input.Length - index - 2));
                            nodes.Add(tree.createNode(tag, att));//decalre a new tag 
                            //Console.WriteLine(nodes.Count());
                        }
                        if (Tag.Count() == 0)
                        {
                           
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
            sr.Close();
        }
        void cTree(string file)
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
                        if (input.Contains(" "))
                        {
                            index = input.IndexOf(" ");
                            //First white space in the tag
                            flag = true;
                            break;
                        }
                        string tag = input.Substring(1, index - 1);
                        if (!flag)
                        {
                            nodes.Add(ctree.createNode(tag, ""));//decalre a new tag 
                        }
                        else
                        {
                            string att = input.Substring(index + 1, (input.Length - index - 2));
                            nodes.Add(ctree.createNode(tag, att));//decalre a new tag 
                        }
                        if (Tag.Count() == 0)
                        {
                            ctree.addRoot(nodes[nodes.Count() - 1]);//add tree root to the tree
                        }
                        else
                        {
                            ctree.addNode(nodes[Tag.Peek()], nodes[nodes.Count() - 1]);//add a child to the last opened tag
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
                        ctree.addData(nodes[Tag.Peek()], input);//add data to the last opened tag
                    }
                }
            }
            sr.Close();
        }
        public void print(xmlTree tree, treeNode r)
        {
            int tab = tree.height(r) + 1; //level num + 1 extra tab 
            if (r == tree.Root)
            {
                //Console.WriteLine("{");
                richTextBox2.SelectedText = "{" + Environment.NewLine;
            }
            for (int k = 0; k < tab; k++)
            {
                //Console.Write("\t");
                richTextBox2.SelectedText = "\t";
            }
            //Console.Write('"' + r.tagName + '"' + ": ");
            richTextBox2.SelectedText = '"' + r.tagName + '"' + ": ";
            if (r.children.Count() == 0)
            {
                //Console.Write('"' + r.data + '"');
                richTextBox2.SelectedText = '"' + r.data + '"';
                int index = r.parent.children.IndexOf(r);
                if (index + 1 != r.parent.children.Count())
                {
                    //Console.WriteLine(",");
                    richTextBox2.SelectedText = "," + Environment.NewLine;
                }
            }
            else if (r.children.Count() == 1)
            {
                //Console.WriteLine("[");
                richTextBox2.SelectedText = "[" + Environment.NewLine;
                foreach (treeNode i in r.children)
                {
                    print(tree, i);
                }
                for (int k = 0; k < tab; k++)
                {
                    //Console.Write("\t");
                    richTextBox2.SelectedText = "\t";
                }
                //Console.WriteLine("]");
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
                    //Console.WriteLine("{");
                    richTextBox2.SelectedText = "{" + Environment.NewLine;
                    foreach (treeNode i in r.children)
                    {
                        print(tree, i);
                    }
                }
                else
                {
                    //Console.WriteLine("[");
                    richTextBox2.SelectedText = "[" + Environment.NewLine;
                    foreach (treeNode i in r.children)
                    {
                        if (i.children.Count() > 0)
                        {
                            int j = tree.height(i) + 1;
                            for (int k = 0; k < j; k++)
                            {
                                //Console.Write("\t");
                                richTextBox2.SelectedText = "\t";
                            }
                            //Console.WriteLine("{");
                            richTextBox2.SelectedText = "{" + Environment.NewLine;
                            foreach (treeNode f in i.children)
                            {
                                print(tree, f);
                            }
                            int g = tree.height(i) + 1;
                            //Console.Write("\n");
                            richTextBox2.SelectedText = Environment.NewLine;
                            for (int k = 0; k < g; k++)
                            {
                                //Console.Write("\t");
                                richTextBox2.SelectedText = "\t";
                            }
                            //Console.Write("}");
                            richTextBox2.SelectedText = "}";
                            int index = r.children.IndexOf(i);
                            if (index + 1 != r.children.Count())
                            {
                                //Console.WriteLine(",");
                                richTextBox2.SelectedText = "," + Environment.NewLine;
                            }
                            else
                            {
                                //Console.WriteLine();
                                richTextBox2.SelectedText = Environment.NewLine;
                            }
                        }
                        else if (i.children.Count() == 0)
                        {
                            int y = tree.height(i) + 1;
                            for (int k = 0; k < y; k++)
                            {
                                //Console.Write("\t");
                                richTextBox2.SelectedText = "\t";
                            }
                            //Console.Write('"' + i.data + '"');
                            richTextBox2.SelectedText = '"' + i.data + '"';
                            int index = r.children.IndexOf(i);
                            if (index + 1 != r.children.Count())
                            {
                                //Console.WriteLine(",");
                                richTextBox2.SelectedText = "," + Environment.NewLine;
                            }
                            else
                            {
                                //Console.WriteLine();
                                richTextBox2.SelectedText = Environment.NewLine;
                            }
                        }
                    }
                }
                for (int k = 0; k < tab; k++)
                {
                    //Console.Write("\t");
                    richTextBox2.SelectedText = "\t";
                }
                if (flag)
                {
                    //Console.WriteLine("}");
                    richTextBox2.SelectedText = "}" + Environment.NewLine;
                }
                else
                {
                    //Console.Write("]");
                    richTextBox2.SelectedText = "]";
                    int index = r.parent.children.IndexOf(r);
                    if (index + 1 != r.parent.children.Count())
                    {
                        //Console.WriteLine(",");
                        richTextBox2.SelectedText = "," + Environment.NewLine;
                    }
                    else
                    {
                        //Console.WriteLine();
                        richTextBox2.SelectedText = Environment.NewLine;
                    }
                }
            }
            if (r == tree.Root)
            {               
                //Console.WriteLine("}");
                richTextBox2.SelectedText = "}" + Environment.NewLine;
            }
        }
        public void format(xmlTree tree, treeNode r, ref Stack<treeNode> s)
        {
            if (r == tree.Root)
            {
            }
            else if (r.tagName.Contains("<"))
            {
                int i = tree.height(r) - 1;
                for (int k = 0; k < i; k++)
                {
                    richTextBox2.SelectedText = "\t";
                }
                richTextBox2.SelectedText = r.tagName + Environment.NewLine;
            }
            else
            {
                int i = tree.height(r) - 1;
                for (int k = 0; k < i; k++)
                {
                    richTextBox2.SelectedText = "\t";
                }
                if (r.data != "")
                {
                    richTextBox2.SelectedText = "<" + r.tagName + ">" + r.data + "</" + r.tagName + ">" + Environment.NewLine;
                }
                else
                {
                    richTextBox2.SelectedText = "<" + r.tagName + ">" + Environment.NewLine;
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
                    int f = tree.height(n) - 1;
                    for (int k = 0; k < f; k++)
                    {
                        richTextBox2.SelectedText = "\t";
                    }
                    richTextBox2.SelectedText = "</" + n.tagName + ">" + Environment.NewLine;
                }
            }
        }
        void reset()
        {
            
            tree.Clear();
            ctree.Clear();
            path = "";
            savedFile = "";
            minifiedoutput = "";
            decompressed = "";
            richTextBox1.Clear();
            richTextBox2.Clear();
            status = true;
            nodes.Clear();
            Tag.Clear();
            st.Clear();
            tags.Clear();
            errors_index.Clear();
            text = "";
            text2 = "";
            t = "";
            flag = 0;
            
            p = "";
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
            reset();
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            string line = " ";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog2.FileName;
                text = File.ReadAllText(path);
                text2 = File.ReadAllText(path);
                t = File.ReadAllText(path);
                label2.Text = path;
                p = Path.GetDirectoryName(path) + "/cut.xml";
                if (File.Exists(p))
                {
                    File.Delete(p);
                }
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
                        cutter(line, p);
                    }
                }
                sr.Close();
            }
            //xml_tags_to_array();
            //status = Check_Consistency(tags);
            if (status)
            {
                createTree(p);
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        //Format button
        private void button8_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            textBox2.Clear();
            richTextBox2.Visible = true;
            textBox2.Visible = false;
            format(tree, tree.Root, ref st);
        }
        //save file
        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
            SaveFileDialog1.InitialDirectory = @"C:\Users\Lenovo\Desktop";
            SaveFileDialog1.RestoreDirectory = true;
            SaveFileDialog1.Filter = "All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                savedFile = saveFileDialog1.FileName;
            }
            if (File.Exists(savedFile))
            {
                File.Delete(savedFile);
            }
            File.AppendAllText(savedFile, richTextBox2.Text);
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
            richTextBox2.Clear();
            textBox2.Clear();
            richTextBox2.Visible = false;
            textBox2.Visible = true;

            minifiedoutput = "";

            string line = " ";
            StreamReader sr = new StreamReader(p);
            while (line != null)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    for(int i = 0; i < line.Length; i++)
                    {
                        if(line[i] != ' ' && line[i] != '\t') 
                            minifiedoutput += line[i];
                    }
                }
            }
            textBox2.Text = minifiedoutput;
            sr.Close();
        }
        //xml2json
        private void button10_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            textBox2.Clear();
            richTextBox2.Visible = true;
            textBox2.Visible = false;
            nodes.Clear();
            Tag.Clear();
            st.Clear();
            cTree(p);
            print(ctree, ctree.Root);
        }
        //fix errors
        private void button11_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            textBox2.Clear();
            richTextBox2.Visible = true;
            textBox2.Visible = false;
            Fix_XML_Errors(1);
        }
        //show errors
        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            textBox2.Clear();
            richTextBox2.Visible = true;
            textBox2.Visible = false;
            Fix_XML_Errors(0);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        //compress
        private void button5_Click(object sender, EventArgs e)
        {
            minifiedoutput = "";
            string line = " ";
            StreamReader sr = new StreamReader(p);
            while (line != null)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    minifiedoutput += line;
                }
            }
            sr.Close();
            path = Path.GetDirectoryName(path) + "/Compressed.xml";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            List<short> compressed;

            compressed = Compress(minifiedoutput);
            File.AppendAllText(path, string.Join("", compressed));
            decompressed = Decompress(compressed);
            Console.WriteLine(decompressed);
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        //decompress
        private void button12_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            textBox2.Clear();
            richTextBox2.Visible = true;
            textBox2.Visible = false;
            //decompressed = "";
            pc = Path.GetDirectoryName(path) + "/decompressed.xml";
            if (File.Exists(pc))
            {
                File.Delete(pc);
            }
            cutter(decompressed, pc);
            tree.Clear();
            createTree(pc);
            st.Clear();
            nodes.Clear();
            format(tree, tree.Root, ref st);
        }
        //check consistency
        private void button6_Click(object sender, EventArgs e)
        {
            xml_tags_to_array();
            status = Check_Consistency(tags);
            label5.Visible = true;
            if (status)
            {
                label5.Text = "File is consistent";
                label5.ForeColor = Color.Black;
                button7.Enabled = false;
                button11.Enabled = false;
            }
            else
            {
                label5.Text = "File is  not consistent";
                label5.ForeColor = Color.Red;
                //label5.Font = ;
                button7.Enabled = true;
                button11.Enabled = true;
            }
            //Console.WriteLine(status);
        }
    }
    
}
