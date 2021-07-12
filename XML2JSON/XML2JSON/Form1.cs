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
        static string path;
       
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
            public void print(treeNode r, string fp)
            {
                int tab = height(r) + 1; //level num + 1 extra tab 
                string l = "";
                if (r == root)
                {
                    Console.WriteLine("{\n");
                    //l = "";
                    l = "{\n";
                    File.AppendAllText(fp, l);


                }
                l = "";
                for (int k = 0; k < tab; k++)
                {
                    Console.Write("\t");
                    l += "\t";
                }
                Console.Write('"' + r.tagName + '"' + ": ");
                File.AppendAllText(fp, l + '"' + r.tagName + '"' + ": ");
                /*else
                {
                    Console.WriteLine(r.tagName + " parent: " + r.parent.tagName);
                }*/
                if (r.children.Count() == 0)
                {
                    l = "";
                    Console.Write('"' + r.data + '"');
                    
                    File.AppendAllText(fp, l + '"' + r.data + '"' + "\n");
                    int index = r.parent.children.IndexOf(r);
                    if (index + 1 != r.parent.children.Count())
                    {
                        Console.WriteLine(",");
                    }
                }
                else if (r.children.Count() == 1)
                {
                    l = "";
                    Console.WriteLine("[");
                    l += "[\n";
                    File.AppendAllText(fp, l);
                    foreach (treeNode i in r.children)
                    {
                        print(i, fp);
                    }
                    l = "";
                    for (int k = 0; k < tab; k++)
                    {
                        Console.Write("\t");

                    }
                    Console.WriteLine("]");
                    l += "]\n";
                    File.AppendAllText(fp, l);
                }
                else if (r.children.Count() > 0)
                {
                    var flag = false;
                    treeNode x = r.children[0];
                    foreach (treeNode p in r.children)
                    {
                        if(p.tagName != x.tagName)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        l = "";
                  
                        Console.WriteLine("{");
                        l += "{\n";
                        File.AppendAllText(fp, l);
                        foreach (treeNode i in r.children)
                        {
                            print(i, fp);
                        }
                    }
                    else
                    {
                        l = "";
                        Console.WriteLine("[");
                        l += "[\n";
                        File.AppendAllText(fp, l);
                        foreach (treeNode i in r.children)
                        {
                            if(i.children.Count() > 0)
                            {
                                int j = height(i) + 1;
                                l = "";
                                for (int k = 0; k < j; k++)
                                {
                                    Console.Write("\t");
                                    l += "\t";
                                }
                                Console.WriteLine("{");
                                l += "{\n";
                                File.AppendAllText(fp, l);
                                foreach (treeNode f in i.children)
                                {
                                    print(f, fp);
                                }
                                int g = height(i) + 1;
                                l = "";
                                Console.Write("\n");
                                for (int k = 0; k < g; k++)
                                {
                                    Console.Write("\t");
                                    l = "";
                                }
                                Console.Write("}");
                                int index = r.children.IndexOf(i);
                                if (index + 1 != r.children.Count())
                                {
                                    Console.WriteLine(",");
                                }
                                else
                                {
                                    Console.WriteLine();
                                }
                                l += "}\n";
                                File.AppendAllText(fp, l);
                            }
                            else if(i.children.Count() == 0)
                            {
                                int y = height(i) + 1;
                                l = "";
                                for (int k = 0; k < y; k++)
                                {
                                    Console.Write("\t");
                                    l += "\t";
                                }
                                Console.Write('"' + i.data + '"');
                                int index = r.children.IndexOf(i);
                                if (index + 1 != r.children.Count())
                                {
                                    Console.WriteLine(",");
                                }
                                else
                                {
                                    Console.WriteLine();
                                }
                                
                                l += '"' + i.data + '"';
                                File.AppendAllText(fp, l + "\n");
                            }
                        }
                    }
                    l = "";
                    for (int k = 0; k < tab; k++)
                    {
                        Console.Write("\t");
                        l += "\t";
                    }
                    if (flag)
                    {
                        Console.WriteLine("}");
                        l += "}";
                        File.AppendAllText(fp, l + "\n");
                    }
                    else
                    {
                        Console.Write("]");
                        int index = r.parent.children.IndexOf(r);
                        if (index + 1 != r.parent.children.Count())
                        {
                            Console.WriteLine(",");
                        }
                        else
                        {
                            Console.WriteLine();
                        }

                        l += "]";
                        File.AppendAllText(fp, l + "\n");

                    }

                }
                if(r == root)
                {
                    l = "";
                    Console.WriteLine("}");
                    l += "}";
                    File.AppendAllText(fp, l);
                }
                

            }
            public void format(treeNode r, ref Stack<treeNode> s, string fp)
            {
                string l = "";
                if(r == root)
                {
                    Console.WriteLine("<" + r.tagName + ">");
                    l = "";
                    l = "<" + r.tagName + ">";
                    File.AppendAllText(fp, l + "\n");
                    s.Push(r);
                }
                else
                {
                    int i = height(r);
                    l = "";
                    for (int k = 0; k < i; k++)
                    {
                        Console.Write("\t");
                        l += "\t";
                    }
                    if(r.data != "")
                    {
                        Console.WriteLine("<" + r.tagName + ">" + r.data + "</" + r.tagName + ">");
                        l += "<" + r.tagName + ">" + r.data + "</" + r.tagName + ">";
                        File.AppendAllText(fp, l + "\n");
                    }
                    else
                    {
                        Console.WriteLine("<" + r.tagName + ">");
                        l += "<" + r.tagName + ">";
                        File.AppendAllText(fp, l + "\n");
                        s.Push(r);
                    }
                   
                }
                if(r.children.Count() > 0)
                {
                    foreach(treeNode i in r.children)
                    {
                        format(i, ref s, fp);
                    }
                    if (s.Count() != 0)
                    {
                        treeNode n = s.Peek();
                        s.Pop();
                        int f = height(n);
                        l = "";
                        for (int k = 0; k < f; k++)
                        {
                            Console.Write("\t");
                            l += "\t";
                        }
                        Console.WriteLine("</" + n.tagName + ">");
                        l += "</" + n.tagName + ">";
                        File.AppendAllText(fp, l + "\n");
                    }
                }
                
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
        static List<treeNode> nodes = new List<treeNode>();
        xmlTree tree = new xmlTree();
        static Stack<int> Tag = new Stack<int>();
        static Stack<treeNode> st = new Stack<treeNode>();

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
                        /*for (int i = 1; i < input.Length; i++)
                        {
                            index = i;
                            if (input[i] == ' ')
                            {
                                //First white space in the tag
                                flag = true;
                                break;
                            }
                        }*/
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
            sr.Close();
            //tree.format(tree.Root, ref st);
            //tree.print(tree.Root);
            
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
                /*if(listBox2.Items.Count != 0)
                {
                    listBox2.Items.Clear();
                }
                if (listBox3.Items.Count != 0)
                {
                    listBox3.Items.Clear();
                }*/
                richTextBox1.Clear();
                richTextBox2.Clear();
                path = openFileDialog2.FileName;
                label2.Text = path;
                StreamReader sr = new StreamReader(path);
                while (line != null)
                {
                    
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        //listBox2.Items.Add(line);
                        richTextBox1.SelectedText = line + Environment.NewLine;
                        while (line.Contains("\t"))
                        {
                            line = line.Remove(0, 1);
                        }
                        cutter(line);
                        
                    }
                }
                sr.Close();
            }
            path = Path.GetDirectoryName(path) + "/toFormat.xml";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            //File.Create(path);
            foreach (string i in listBox3.Items)
            {
                File.AppendAllText(path, i + "\n");
            }
            createTree(path);
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        //Format button
        private void button8_Click(object sender, EventArgs e)
        {

            /*if (listBox3.Items.Count != 0)
            {
                listBox3.Items.Clear();
            }*/
            richTextBox2.Clear();
            string fp = path;
            fp = Path.GetDirectoryName(fp) + "/Formated.xml";
            if (File.Exists(fp))
            {
                File.Delete(fp);
            }
            tree.format(tree.Root, ref st, fp);
            string line = " ";
            StreamReader sr = new StreamReader(fp);
            while (line != null)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    //listBox3.Items.Add(line);
                    richTextBox2.SelectedText = line + Environment.NewLine;
                }
            }
            sr.Close();
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
            path = Path.GetDirectoryName(path) + "/minified.xml";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            //File.Create(path);
            foreach (string i in listBox3.Items)
            {
                File.AppendAllText(path, i);
            }
            richTextBox2.Clear();
            string line = " ";
            StreamReader sr = new StreamReader(path);
            while (line != null)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    //listBox3.Items.Add(line);
                    richTextBox2.SelectedText = line;
                }
            }
            sr.Close();
        }
        //xml2json
        private void button10_Click(object sender, EventArgs e)
        {
            
            richTextBox2.Clear();
            string fp = path;
            fp = Path.GetDirectoryName(fp) + "/XML2JSON.json";
            if (File.Exists(fp))
            {
                File.Delete(fp);
            }
            tree.print(tree.Root, fp);
            //Console.WriteLine("}");
            File.AppendAllText(fp, "}");
            string line = " ";
            StreamReader sr = new StreamReader(fp);
            while (line != null)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    //listBox3.Items.Add(line);
                    richTextBox2.SelectedText = line + Environment.NewLine;
                }
            }
            sr.Close();
        }
    }
    
}
