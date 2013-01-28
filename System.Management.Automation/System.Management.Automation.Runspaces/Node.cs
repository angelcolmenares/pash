namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal class Node
    {
        internal Collection<Node> actualNodes;
        internal NodeCardinality cardinality;
        internal bool hasInnerText;
        internal string innerText;
        internal bool? isHidden;
        internal int lineNumber;
        internal string name;
        internal int nodeCount;
        internal bool nodeError;
        internal Node[] possibleChildren;

        internal Node(string name, bool hasInnerText, NodeCardinality cardinality, Node[] possibleChildren)
        {
            this.isHidden = null;
            this.name = name;
            this.hasInnerText = hasInnerText;
            this.cardinality = cardinality;
            this.possibleChildren = possibleChildren;
            this.actualNodes = new Collection<Node>();
        }

        internal Node(string name, bool hasInnerText, NodeCardinality cardinality, Node[] possibleChildren, bool? supportsIsHidden) : this(name, hasInnerText, cardinality, possibleChildren)
        {
            this.isHidden = supportsIsHidden;
        }

        internal Node Clone()
        {
            return new Node(this.name, this.hasInnerText, this.cardinality, CloneNodeArray(this.possibleChildren), this.isHidden);
        }

        internal static Node[] CloneNodeArray(Node[] source)
        {
            Node[] nodeArray = new Node[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                nodeArray[i] = source[i].Clone();
            }
            return nodeArray;
        }

        internal delegate void NodeMethod(LoadContext context, Node node);
    }
}

