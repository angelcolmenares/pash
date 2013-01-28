namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal sealed class BufferingXmlReader : XmlReader
    {
        private readonly LinkedList<BufferedNode> bufferedNodes;
        private Stack<XmlBaseDefinition> bufferStartXmlBaseStack;
        private LinkedListNode<BufferedNode> currentAttributeNode;
        private LinkedListNode<BufferedNode> currentBufferedNode;
        private LinkedListNode<BufferedNode> currentBufferedNodeToReport;
        private bool disableInStreamErrorDetection;
        private readonly bool disableXmlBase;
        private readonly Uri documentBaseUri;
        private readonly BufferedNode endOfInputBufferedNode;
        private bool isBuffering;
        private readonly int maxInnerErrorDepth;
        internal readonly string ODataErrorElementName;
        internal readonly string ODataMetadataNamespace;
        internal readonly string ODataNamespace;
        private readonly XmlReader reader;
        private bool removeOnNextRead;
        internal readonly string XmlBaseAttributeName;
        private Stack<XmlBaseDefinition> xmlBaseStack;
        internal readonly string XmlLangAttributeName;
        internal readonly string XmlNamespace;

        internal BufferingXmlReader(XmlReader reader, Uri parentXmlBaseUri, Uri documentBaseUri, bool disableXmlBase, int maxInnerErrorDepth, string odataNamespace)
        {
            this.reader = reader;
            this.documentBaseUri = documentBaseUri;
            this.disableXmlBase = disableXmlBase;
            this.maxInnerErrorDepth = maxInnerErrorDepth;
            XmlNameTable nameTable = this.reader.NameTable;
            this.XmlNamespace = nameTable.Add("http://www.w3.org/XML/1998/namespace");
            this.XmlBaseAttributeName = nameTable.Add("base");
            this.XmlLangAttributeName = nameTable.Add("lang");
            this.ODataMetadataNamespace = nameTable.Add("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            this.ODataNamespace = nameTable.Add(odataNamespace);
            this.ODataErrorElementName = nameTable.Add("error");
            this.bufferedNodes = new LinkedList<BufferedNode>();
            this.currentBufferedNode = null;
            this.endOfInputBufferedNode = BufferedNode.CreateEndOfInput(this.reader.NameTable);
            this.xmlBaseStack = new Stack<XmlBaseDefinition>();
            if (parentXmlBaseUri != null)
            {
                this.xmlBaseStack.Push(new XmlBaseDefinition(parentXmlBaseUri, 0));
            }
        }

        private BufferedNode BufferCurrentReaderNode()
        {
            if (this.reader.EOF)
            {
                return this.endOfInputBufferedNode;
            }
            BufferedNode node = new BufferedNode(this.reader);
            if (this.reader.NodeType == XmlNodeType.Element)
            {
                while (this.reader.MoveToNextAttribute())
                {
                    node.AttributeNodes.AddLast(new BufferedNode(this.reader));
                }
                this.reader.MoveToElement();
            }
            return node;
        }

        public override void Close()
        {
            throw new NotSupportedException();
        }

        private LinkedListNode<BufferedNode> FindAttributeBufferedNode(int index)
        {
            BufferedNode currentElementNode = this.GetCurrentElementNode();
            if ((currentElementNode.NodeType == XmlNodeType.Element) && (currentElementNode.AttributeNodes.Count > 0))
            {
                LinkedListNode<BufferedNode> first = currentElementNode.AttributeNodes.First;
                int num = 0;
                while (first != null)
                {
                    if (num == index)
                    {
                        return first;
                    }
                    num++;
                    first = first.Next;
                }
            }
            return null;
        }

        private LinkedListNode<BufferedNode> FindAttributeBufferedNode(string qualifiedName)
        {
            BufferedNode currentElementNode = this.GetCurrentElementNode();
            if ((currentElementNode.NodeType == XmlNodeType.Element) && (currentElementNode.AttributeNodes.Count > 0))
            {
                for (LinkedListNode<BufferedNode> node2 = currentElementNode.AttributeNodes.First; node2 != null; node2 = node2.Next)
                {
                    BufferedNode node3 = node2.Value;
                    bool flag = !string.IsNullOrEmpty(node3.Prefix);
                    if (!flag && (string.CompareOrdinal(node3.LocalName, qualifiedName) == 0))
                    {
                        return node2;
                    }
                    if (flag && (string.CompareOrdinal(node3.Prefix + ":" + node3.LocalName, qualifiedName) == 0))
                    {
                        return node2;
                    }
                }
            }
            return null;
        }

        private LinkedListNode<BufferedNode> FindAttributeBufferedNode(string localName, string namespaceUri)
        {
            BufferedNode currentElementNode = this.GetCurrentElementNode();
            if ((currentElementNode.NodeType == XmlNodeType.Element) && (currentElementNode.AttributeNodes.Count > 0))
            {
                for (LinkedListNode<BufferedNode> node2 = currentElementNode.AttributeNodes.First; node2 != null; node2 = node2.Next)
                {
                    BufferedNode node3 = node2.Value;
                    if ((string.CompareOrdinal(node3.NamespaceUri, namespaceUri) == 0) && (string.CompareOrdinal(node3.LocalName, localName) == 0))
                    {
                        return node2;
                    }
                }
            }
            return null;
        }

        public override string GetAttribute(int i)
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.GetAttribute(i);
            }
            if ((i < 0) || (i >= this.AttributeCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            LinkedListNode<BufferedNode> node = this.FindAttributeBufferedNode(i);
            if (node != null)
            {
                return node.Value.Value;
            }
            return null;
        }

        public override string GetAttribute(string name)
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.GetAttribute(name);
            }
            LinkedListNode<BufferedNode> node = this.FindAttributeBufferedNode(name);
            if (node != null)
            {
                return node.Value.Value;
            }
            return null;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.GetAttribute(name, namespaceURI);
            }
            LinkedListNode<BufferedNode> node = this.FindAttributeBufferedNode(name, namespaceURI);
            if (node != null)
            {
                return node.Value.Value;
            }
            return null;
        }

        private string GetAttributeWithAtomizedName(string name, string namespaceURI)
        {
            if (this.bufferedNodes.Count > 0)
            {
                for (LinkedListNode<BufferedNode> node = this.GetCurrentElementNode().AttributeNodes.First; node != null; node = node.Next)
                {
                    BufferedNode node2 = node.Value;
                    if (object.ReferenceEquals(namespaceURI, node2.NamespaceUri) && object.ReferenceEquals(name, node2.LocalName))
                    {
                        return node.Value.Value;
                    }
                }
                return null;
            }
            string str = null;
            while (this.reader.MoveToNextAttribute())
            {
                if (object.ReferenceEquals(name, this.reader.LocalName) && object.ReferenceEquals(namespaceURI, this.reader.NamespaceURI))
                {
                    str = this.reader.Value;
                    break;
                }
            }
            this.reader.MoveToElement();
            return str;
        }

        private BufferedNode GetCurrentElementNode()
        {
            if (this.isBuffering)
            {
                return this.currentBufferedNode.Value;
            }
            return this.bufferedNodes.First.Value;
        }

        private bool IsEndOfInputNode(BufferedNode node)
        {
            return object.ReferenceEquals(node, this.endOfInputBufferedNode);
        }

        public override string LookupNamespace(string prefix)
        {
            throw new NotSupportedException();
        }

        private void MoveFromAttributeValueNode()
        {
            if (this.currentAttributeNode != null)
            {
                this.currentBufferedNodeToReport = this.currentAttributeNode;
                this.currentAttributeNode = null;
            }
        }

        public override bool MoveToAttribute(string name)
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.MoveToAttribute(name);
            }
            LinkedListNode<BufferedNode> node = this.FindAttributeBufferedNode(name);
            if (node != null)
            {
                this.currentAttributeNode = null;
                this.currentBufferedNodeToReport = node;
                return true;
            }
            return false;
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.MoveToAttribute(name, ns);
            }
            LinkedListNode<BufferedNode> node = this.FindAttributeBufferedNode(name, ns);
            if (node != null)
            {
                this.currentAttributeNode = null;
                this.currentBufferedNodeToReport = node;
                return true;
            }
            return false;
        }

        public override bool MoveToElement()
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.MoveToElement();
            }
            this.MoveFromAttributeValueNode();
            if (this.isBuffering)
            {
                if (this.currentBufferedNodeToReport.Value.NodeType == XmlNodeType.Attribute)
                {
                    this.currentBufferedNodeToReport = this.currentBufferedNode;
                    return true;
                }
                return false;
            }
            if (this.currentBufferedNodeToReport.Value.NodeType == XmlNodeType.Attribute)
            {
                this.currentBufferedNodeToReport = this.bufferedNodes.First;
                return true;
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.MoveToFirstAttribute();
            }
            BufferedNode currentElementNode = this.GetCurrentElementNode();
            if ((currentElementNode.NodeType == XmlNodeType.Element) && (currentElementNode.AttributeNodes.Count > 0))
            {
                this.currentAttributeNode = null;
                this.currentBufferedNodeToReport = currentElementNode.AttributeNodes.First;
                return true;
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.MoveToNextAttribute();
            }
            LinkedListNode<BufferedNode> currentAttributeNode = this.currentAttributeNode;
            if (currentAttributeNode == null)
            {
                currentAttributeNode = this.currentBufferedNodeToReport;
            }
            if (currentAttributeNode.Value.NodeType == XmlNodeType.Attribute)
            {
                if (currentAttributeNode.Next != null)
                {
                    this.currentAttributeNode = null;
                    this.currentBufferedNodeToReport = currentAttributeNode.Next;
                    return true;
                }
                return false;
            }
            if ((this.currentBufferedNodeToReport.Value.NodeType == XmlNodeType.Element) && (this.currentBufferedNodeToReport.Value.AttributeNodes.Count > 0))
            {
                this.currentBufferedNodeToReport = this.currentBufferedNodeToReport.Value.AttributeNodes.First;
                return true;
            }
            return false;
        }

        public override bool Read()
        {
            if (!this.disableXmlBase && (this.xmlBaseStack.Count > 0))
            {
                XmlNodeType nodeType = this.NodeType;
                if (nodeType == XmlNodeType.Attribute)
                {
                    this.MoveToElement();
                    nodeType = XmlNodeType.Element;
                }
                if ((this.xmlBaseStack.Peek().Depth == this.Depth) && ((nodeType == XmlNodeType.EndElement) || ((nodeType == XmlNodeType.Element) && this.IsEmptyElement)))
                {
                    this.xmlBaseStack.Pop();
                }
            }
            bool flag = this.ReadInternal(this.disableInStreamErrorDetection);
            if ((flag && !this.disableXmlBase) && (this.NodeType == XmlNodeType.Element))
            {
                string attributeWithAtomizedName = this.GetAttributeWithAtomizedName(this.XmlBaseAttributeName, this.XmlNamespace);
                if (attributeWithAtomizedName == null)
                {
                    return flag;
                }
                Uri relativeUri = new Uri(attributeWithAtomizedName, UriKind.RelativeOrAbsolute);
                if (!relativeUri.IsAbsoluteUri)
                {
                    if (this.xmlBaseStack.Count == 0)
                    {
                        if (this.documentBaseUri == null)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomDeserializer_RelativeUriUsedWithoutBaseUriSpecified(attributeWithAtomizedName));
                        }
                        relativeUri = UriUtils.UriToAbsoluteUri(this.documentBaseUri, relativeUri);
                    }
                    else
                    {
                        relativeUri = UriUtils.UriToAbsoluteUri(this.xmlBaseStack.Peek().BaseUri, relativeUri);
                    }
                }
                this.xmlBaseStack.Push(new XmlBaseDefinition(relativeUri, this.Depth));
            }
            return flag;
        }

        public override bool ReadAttributeValue()
        {
            if (this.bufferedNodes.Count <= 0)
            {
                return this.reader.ReadAttributeValue();
            }
            if (this.currentBufferedNodeToReport.Value.NodeType != XmlNodeType.Attribute)
            {
                return false;
            }
            if (this.currentAttributeNode != null)
            {
                return false;
            }
            BufferedNode node = new BufferedNode(this.currentBufferedNodeToReport.Value.Value, this.currentBufferedNodeToReport.Value.Depth, this.NameTable);
            LinkedListNode<BufferedNode> node2 = new LinkedListNode<BufferedNode>(node);
            this.currentAttributeNode = this.currentBufferedNodeToReport;
            this.currentBufferedNodeToReport = node2;
            return true;
        }

        private bool ReadInternal(bool ignoreInStreamErrors)
        {
            bool flag;
            if (this.removeOnNextRead)
            {
                this.currentBufferedNodeToReport = this.currentBufferedNodeToReport.Next;
                this.bufferedNodes.RemoveFirst();
                this.removeOnNextRead = false;
            }
            if (this.isBuffering)
            {
                this.MoveFromAttributeValueNode();
                if (this.currentBufferedNode.Next != null)
                {
                    this.currentBufferedNode = this.currentBufferedNode.Next;
                    this.currentBufferedNodeToReport = this.currentBufferedNode;
                    return true;
                }
                if (ignoreInStreamErrors)
                {
                    flag = this.reader.Read();
                    this.bufferedNodes.AddLast(this.BufferCurrentReaderNode());
                    this.currentBufferedNode = this.bufferedNodes.Last;
                    this.currentBufferedNodeToReport = this.currentBufferedNode;
                    return flag;
                }
                return this.ReadNextAndCheckForInStreamError();
            }
            if (this.bufferedNodes.Count == 0)
            {
                return (ignoreInStreamErrors ? this.reader.Read() : this.ReadNextAndCheckForInStreamError());
            }
            this.currentBufferedNodeToReport = this.bufferedNodes.First;
            BufferedNode node = this.currentBufferedNodeToReport.Value;
            flag = !this.IsEndOfInputNode(node);
            this.removeOnNextRead = true;
            return flag;
        }

        private bool ReadNextAndCheckForInStreamError()
        {
            bool flag = this.ReadInternal(true);
            if ((!this.disableInStreamErrorDetection && (this.NodeType == XmlNodeType.Element)) && (this.LocalNameEquals(this.ODataErrorElementName) && this.NamespaceEquals(this.ODataMetadataNamespace)))
            {
                throw new ODataErrorException(ODataAtomErrorDeserializer.ReadErrorElement(this, this.maxInnerErrorDepth));
            }
            return flag;
        }

        public override void ResolveEntity()
        {
            throw new InvalidOperationException(Microsoft.Data.OData.Strings.ODataException_GeneralError);
        }

        internal void StartBuffering()
        {
            if (this.bufferedNodes.Count == 0)
            {
                this.bufferedNodes.AddFirst(this.BufferCurrentReaderNode());
            }
            else
            {
                this.removeOnNextRead = false;
            }
            this.currentBufferedNode = this.bufferedNodes.First;
            this.currentBufferedNodeToReport = this.currentBufferedNode;
            int count = this.xmlBaseStack.Count;
            switch (count)
            {
                case 0:
                    this.bufferStartXmlBaseStack = new Stack<XmlBaseDefinition>();
                    break;

                case 1:
                    this.bufferStartXmlBaseStack = new Stack<XmlBaseDefinition>();
                    this.bufferStartXmlBaseStack.Push(this.xmlBaseStack.Peek());
                    break;

                default:
                {
                    XmlBaseDefinition[] definitionArray = this.xmlBaseStack.ToArray();
                    this.bufferStartXmlBaseStack = new Stack<XmlBaseDefinition>(count);
                    for (int i = count - 1; i >= 0; i--)
                    {
                        this.bufferStartXmlBaseStack.Push(definitionArray[i]);
                    }
                    break;
                }
            }
            this.isBuffering = true;
        }

        internal void StopBuffering()
        {
            this.isBuffering = false;
            this.removeOnNextRead = true;
            this.currentBufferedNode = null;
            if (this.bufferedNodes.Count > 0)
            {
                this.currentBufferedNodeToReport = this.bufferedNodes.First;
            }
            this.xmlBaseStack = this.bufferStartXmlBaseStack;
            this.bufferStartXmlBaseStack = null;
        }

        [Conditional("DEBUG")]
        private void ValidateInternalState()
        {
        }

        public override int AttributeCount
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.AttributeCount;
                }
                if (this.currentBufferedNodeToReport.Value.AttributeNodes == null)
                {
                    return 0;
                }
                return this.currentBufferedNodeToReport.Value.AttributeNodes.Count;
            }
        }

        public override string BaseURI
        {
            get
            {
                return null;
            }
        }

        public override int Depth
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.Depth;
                }
                return this.currentBufferedNodeToReport.Value.Depth;
            }
        }

        internal bool DisableInStreamErrorDetection
        {
            get
            {
                return this.disableInStreamErrorDetection;
            }
            set
            {
                this.disableInStreamErrorDetection = value;
            }
        }

        public override bool EOF
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.EOF;
                }
                return this.IsEndOfInputNode(this.currentBufferedNodeToReport.Value);
            }
        }

        public override bool HasValue
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.HasValue;
                }
                switch (this.NodeType)
                {
                    case XmlNodeType.Attribute:
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.XmlDeclaration:
                        return true;
                }
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.IsEmptyElement;
                }
                return this.currentBufferedNodeToReport.Value.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.LocalName;
                }
                return this.currentBufferedNodeToReport.Value.LocalName;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.NamespaceURI;
                }
                return this.currentBufferedNodeToReport.Value.NamespaceUri;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.NodeType;
                }
                return this.currentBufferedNodeToReport.Value.NodeType;
            }
        }

        internal Uri ParentXmlBaseUri
        {
            get
            {
                if (this.xmlBaseStack.Count == 0)
                {
                    return null;
                }
                XmlBaseDefinition definition = this.xmlBaseStack.Peek();
                if (definition.Depth == this.Depth)
                {
                    if (this.xmlBaseStack.Count == 1)
                    {
                        return null;
                    }
                    definition = this.xmlBaseStack.Skip<XmlBaseDefinition>(1).First<XmlBaseDefinition>();
                }
                return definition.BaseUri;
            }
        }

        public override string Prefix
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.Prefix;
                }
                return this.currentBufferedNodeToReport.Value.Prefix;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.ReadState;
                }
                if (this.IsEndOfInputNode(this.currentBufferedNodeToReport.Value))
                {
                    return System.Xml.ReadState.EndOfFile;
                }
                if (this.currentBufferedNodeToReport.Value.NodeType != XmlNodeType.None)
                {
                    return System.Xml.ReadState.Interactive;
                }
                return System.Xml.ReadState.Initial;
            }
        }

        public override string Value
        {
            get
            {
                if (this.currentBufferedNodeToReport == null)
                {
                    return this.reader.Value;
                }
                return this.currentBufferedNodeToReport.Value.Value;
            }
        }

        internal Uri XmlBaseUri
        {
            get
            {
                if (this.xmlBaseStack.Count <= 0)
                {
                    return null;
                }
                return this.xmlBaseStack.Peek().BaseUri;
            }
        }

        private sealed class BufferedNode
        {
            private LinkedList<BufferingXmlReader.BufferedNode> attributeNodes;

            private BufferedNode(string emptyString)
            {
                this.NodeType = XmlNodeType.None;
                this.NamespaceUri = emptyString;
                this.LocalName = emptyString;
                this.Prefix = emptyString;
                this.Value = emptyString;
            }

            internal BufferedNode(XmlReader reader)
            {
                this.NodeType = reader.NodeType;
                this.NamespaceUri = reader.NamespaceURI;
                this.LocalName = reader.LocalName;
                this.Prefix = reader.Prefix;
                this.Value = reader.Value;
                this.Depth = reader.Depth;
                this.IsEmptyElement = reader.IsEmptyElement;
            }

            internal BufferedNode(string value, int depth, XmlNameTable nametable)
            {
                string str = nametable.Add(string.Empty);
                this.NodeType = XmlNodeType.Text;
                this.NamespaceUri = str;
                this.LocalName = str;
                this.Prefix = str;
                this.Value = value;
                this.Depth = depth + 1;
                this.IsEmptyElement = false;
            }

            internal static BufferingXmlReader.BufferedNode CreateEndOfInput(XmlNameTable nametable)
            {
                return new BufferingXmlReader.BufferedNode(nametable.Add(string.Empty));
            }

            internal LinkedList<BufferingXmlReader.BufferedNode> AttributeNodes
            {
                get
                {
                    if ((this.NodeType == XmlNodeType.Element) && (this.attributeNodes == null))
                    {
                        this.attributeNodes = new LinkedList<BufferingXmlReader.BufferedNode>();
                    }
                    return this.attributeNodes;
                }
            }

            internal int Depth { get; private set; }

            internal bool IsEmptyElement { get; private set; }

            internal string LocalName { get; private set; }

            internal string NamespaceUri { get; private set; }

            internal XmlNodeType NodeType { get; private set; }

            internal string Prefix { get; private set; }

            internal string Value { get; private set; }
        }

        private sealed class XmlBaseDefinition
        {
            internal XmlBaseDefinition(Uri baseUri, int depth)
            {
                this.BaseUri = baseUri;
                this.Depth = depth;
            }

            internal Uri BaseUri { get; private set; }

            internal int Depth { get; private set; }
        }
    }
}

