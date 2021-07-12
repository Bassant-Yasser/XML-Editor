using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML2JSON
{
    /*public class treeNode<T>
    {
        #region fields
        T tagName;
        T attr;
        T data; //also in case of self closing tags 
        List<treeNode<T>> children;
        treeNode<T> parent;
        #endregion

        #region constructors

        //constructor
        public treeNode(T name, T a, T d, treeNode<T> parent)
        {
            this.tagName = name;
            this.attr = a;
            this.data = d;
            this.parent = parent;
            children = new List<treeNode<T>>();
        }
        #region properties
        public T tagName
        {
            get { return tagName; }
        }
        public T attr
        {
            get { return attr; }
        }
        public T data
        {
            get { return data; }
        }
        public treeNode<T> parent
        {
            get { return parent; }
            set { parent = data; }
        }
        public IList<treeNode<T>> children
        {
            get { return children.AsReadOnly(); }
        }
        #endregion
        #region methods
        public bool addChild(treeNode<T> child)
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
    }*/

    public class xmlTree<T>
    {
        #region fields
        treeNode<T> root;
        List<treeNode<T>> nodes = new List<treeNode<T>>();
        #endregion
        #region constructor
        public xmlTree(T n, T a, T d)
        {
            root = new treeNode<T>(n, a, d, null);
            nodes.Add(root);
        }
        #endregion
        #region properties
        public int count
        {
            get { return nodes.Count; }
        }
        public treeNode<T> Root
        {
            get { return root; }
        }
        #endregion
        #region methods
        public bool addNode(treeNode<T> node)
        {
            if (node == null || node.parent == null
                || !nodes.Contains(node.parent){
                return false;
            }
            else if (node.parent.children.Contains(node))
            {
                return false;
            }
            else
            {
                nodes.Add(node);
                return node.parent.addChild(node);
            }
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("root: ");
            if (root != null)
            {
                builder.Append(root.tagName +   );
            }
            else
            {
                builder.Append("null");
            }
            for (int i; i < count; i++)
            {
                builder.Append(nodes[i].ToString());
                if (i < count - 1)
                {
                    builder.Append(",");
                }
            }
            return builder.ToString();
        }
        #endregion

    }

    /*class Program
    {
        static void Main(string[] args)
        {
            Stack<int> tags = new Stack<int>();
            List<string> lines = new List<string>();

            //kda 3nde tree feha root node fadeah
            xmlTree tree = new xmlTree();
            treeNode root = new treeNode(" ", " ", " ");




        }
    }*/


    //start of implementing the xml tree from the xml file
	
	inFile.open("output1.txt");
	if (!inFile) {
		cout << "Unable to open file";
		exit(1); // terminate with error
}

while (getline(inFile, input))
{


    //Opening Tag			// We can take Tag name and Attribute from this line
    if (input[0] == '<' && (input[1] == '!' || input[1] == '?'))
    {
        continue;
    }
    if (input[0] == '<' && input[1] != '/')
    {

        int index;
        // Searching for the space to get the tag name only from the line
        for (int i = 1; i < input.length(); i++)
        {
            index = i;
            if (input[i] == ' ')
            {
                //First white space in the tag
                break;
            }
        }
        string tag = input.substr(1, index - 1);
        string att = input.substr(index + 1, (input.size() - index - 2));
        nodes.push_back(tree.add_node(tag, att));//decalre a new tag 
        if (tags.size() == 0)
        {
            tree.add_root(nodes[nodes.size() - 1]);//add thre root to the tree
        }
        else
        {
            tree.add_child(nodes[tags.top()], nodes[nodes.size() - 1]);//add a child to the last opened tag
        }
        tags.push(nodes.size() - 1);//add the last opened tag to deal with it to add children or data to it 
        if (input[input.length() - 2] == '/')
            tags.pop();//self closing tag 
    }
    //Closing tag
    else if (input[0] == '<' && input[1] == '/')
    {
        tags.pop();//remove the last opend tag (closed) to deal with the next tag to it 
    }
    //Data
    else
    {
        tree.add_data(nodes[tags.top()], input);//add data to the last opened tag

    }
}


	//end of impementation the xml tree from the xml file
}
