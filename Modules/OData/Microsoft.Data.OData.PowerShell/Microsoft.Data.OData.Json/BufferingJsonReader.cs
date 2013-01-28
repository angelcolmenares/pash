namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class BufferingJsonReader : JsonReader
    {
        private BufferedNode bufferedNodesHead;
        private BufferedNode currentBufferedNode;
        private bool disableInStreamErrorDetection;
        private bool isBuffering;
        private readonly int maxInnerErrorDepth;
        private bool parsingInStreamError;
        private readonly bool removeDuplicateProperties;
        private bool removeOnNextRead;

        internal BufferingJsonReader(TextReader reader, bool removeDuplicateProperties, int maxInnerErrorDepth) : base(reader)
        {
            this.removeDuplicateProperties = removeDuplicateProperties;
            this.maxInnerErrorDepth = maxInnerErrorDepth;
            this.bufferedNodesHead = null;
            this.currentBufferedNode = null;
        }

        internal object BookmarkCurrentPosition()
        {
            return this.currentBufferedNode;
        }

        internal void MoveToBookmark(object bookmark)
        {
            BufferedNode node = bookmark as BufferedNode;
            this.currentBufferedNode = node;
        }

        public override bool Read()
        {
            return this.ReadInternal();
        }

        private bool ReadInternal()
        {
            bool flag;
            if (this.removeOnNextRead)
            {
                if (this.bufferedNodesHead.Next == this.bufferedNodesHead)
                {
                    this.bufferedNodesHead = null;
                }
                else
                {
                    this.bufferedNodesHead.Previous.Next = this.bufferedNodesHead.Next;
                    this.bufferedNodesHead.Next.Previous = this.bufferedNodesHead.Previous;
                    this.bufferedNodesHead = this.bufferedNodesHead.Next;
                }
                this.removeOnNextRead = false;
            }
            if (this.isBuffering)
            {
                if (this.currentBufferedNode.Next != this.bufferedNodesHead)
                {
                    this.currentBufferedNode = this.currentBufferedNode.Next;
                    return true;
                }
                if (this.parsingInStreamError)
                {
                    flag = base.Read();
                    BufferedNode node = new BufferedNode(base.NodeType, base.Value) {
                        Previous = this.bufferedNodesHead.Previous,
                        Next = this.bufferedNodesHead
                    };
                    this.bufferedNodesHead.Previous.Next = node;
                    this.bufferedNodesHead.Previous = node;
                    this.currentBufferedNode = node;
                    return flag;
                }
                return this.ReadNextAndCheckForInStreamError();
            }
            if (this.bufferedNodesHead == null)
            {
                return (this.parsingInStreamError ? base.Read() : this.ReadNextAndCheckForInStreamError());
            }
            flag = this.bufferedNodesHead.NodeType != JsonNodeType.EndOfInput;
            this.removeOnNextRead = true;
            return flag;
        }

        private bool ReadNextAndCheckForInStreamError()
        {
            bool flag3;
            this.parsingInStreamError = true;
            try
            {
                bool flag = this.ReadInternal();
                if (base.NodeType == JsonNodeType.StartObject)
                {
                    bool isBuffering = this.isBuffering;
                    BufferedNode currentBufferedNode = null;
                    if (this.isBuffering)
                    {
                        currentBufferedNode = this.currentBufferedNode;
                    }
                    else
                    {
                        this.StartBuffering();
                    }
                    if (this.removeDuplicateProperties)
                    {
                        this.RemoveDuplicateProperties();
                    }
                    else
                    {
                        this.TryReadErrorAndThrow();
                    }
                    if (isBuffering)
                    {
                        this.currentBufferedNode = currentBufferedNode;
                    }
                    else
                    {
                        this.StopBuffering();
                    }
                }
                flag3 = flag;
            }
            finally
            {
                this.parsingInStreamError = false;
            }
            return flag3;
        }

        private void RemoveDuplicateProperties()
        {
            Stack<ObjectRecordPropertyDeduplicationRecord> stack = new Stack<ObjectRecordPropertyDeduplicationRecord>();
            do
            {
                if (this.currentBufferedNode.NodeType == JsonNodeType.StartObject)
                {
                    stack.Push(new ObjectRecordPropertyDeduplicationRecord());
                    BufferedNode currentBufferedNode = this.currentBufferedNode;
                    this.TryReadErrorAndThrow();
                    this.currentBufferedNode = currentBufferedNode;
                }
                else if (this.currentBufferedNode.NodeType == JsonNodeType.EndObject)
                {
                    ObjectRecordPropertyDeduplicationRecord record = stack.Pop();
                    if (record.CurrentPropertyRecord != null)
                    {
                        record.CurrentPropertyRecord.LastPropertyValueNode = this.currentBufferedNode.Previous;
                    }
                    foreach (List<PropertyDeduplicationRecord> list in record.Values)
                    {
                        if (list.Count > 1)
                        {
                            PropertyDeduplicationRecord record2 = list[0];
                            for (int i = 1; i < list.Count; i++)
                            {
                                PropertyDeduplicationRecord record3 = list[i];
                                record3.PropertyNode.Previous.Next = record3.LastPropertyValueNode.Next;
                                record3.LastPropertyValueNode.Next.Previous = record3.PropertyNode.Previous;
                                record2.PropertyNode.Previous.Next = record3.PropertyNode;
                                record3.PropertyNode.Previous = record2.PropertyNode.Previous;
                                record2.LastPropertyValueNode.Next.Previous = record3.LastPropertyValueNode;
                                record3.LastPropertyValueNode.Next = record2.LastPropertyValueNode.Next;
                                record2 = record3;
                            }
                        }
                    }
                    if (stack.Count == 0)
                    {
                        return;
                    }
                }
                else if (this.currentBufferedNode.NodeType == JsonNodeType.Property)
                {
                    List<PropertyDeduplicationRecord> list2;
                    ObjectRecordPropertyDeduplicationRecord record4 = stack.Peek();
                    if (record4.CurrentPropertyRecord != null)
                    {
                        record4.CurrentPropertyRecord.LastPropertyValueNode = this.currentBufferedNode.Previous;
                    }
                    record4.CurrentPropertyRecord = new PropertyDeduplicationRecord(this.currentBufferedNode);
                    string key = (string) this.currentBufferedNode.Value;
                    if (!record4.TryGetValue(key, out list2))
                    {
                        list2 = new List<PropertyDeduplicationRecord>();
                        record4.Add(key, list2);
                    }
                    list2.Add(record4.CurrentPropertyRecord);
                }
            }
            while (this.ReadInternal());
        }

        private void SkipValueInternal()
        {
            int num = 0;
            do
            {
                switch (this.currentBufferedNode.NodeType)
                {
                    case JsonNodeType.StartObject:
                    case JsonNodeType.StartArray:
                        num++;
                        break;

                    case JsonNodeType.EndObject:
                    case JsonNodeType.EndArray:
                        num--;
                        break;
                }
                this.ReadInternal();
            }
            while (num > 0);
        }

        internal void StartBuffering()
        {
            if (this.bufferedNodesHead == null)
            {
                this.bufferedNodesHead = new BufferedNode(base.NodeType, base.Value);
            }
            else
            {
                this.removeOnNextRead = false;
            }
            if (this.currentBufferedNode == null)
            {
                this.currentBufferedNode = this.bufferedNodesHead;
            }
            this.isBuffering = true;
        }

        internal void StopBuffering()
        {
            this.isBuffering = false;
            this.removeOnNextRead = true;
            this.currentBufferedNode = null;
        }

        private void TryReadErrorAndThrow()
        {
            ODataError error = null;
            if (!this.DisableInStreamErrorDetection)
            {
                this.ReadInternal();
                bool flag = false;
                while (this.currentBufferedNode.NodeType == JsonNodeType.Property)
                {
                    string strB = (string) this.currentBufferedNode.Value;
                    if ((string.CompareOrdinal("error", strB) != 0) || flag)
                    {
                        return;
                    }
                    flag = true;
                    this.ReadInternal();
                    if (!this.TryReadErrorPropertyValue(out error))
                    {
                        return;
                    }
                }
                if (flag)
                {
                    throw new ODataErrorException(error);
                }
            }
        }

        private bool TryReadErrorPropertyValue(out ODataError error)
        {
            error = null;
            if (this.currentBufferedNode.NodeType != JsonNodeType.StartObject)
            {
                return false;
            }
            this.ReadInternal();
            error = new ODataError();
            ODataJsonReaderUtils.ErrorPropertyBitMask none = ODataJsonReaderUtils.ErrorPropertyBitMask.None;
            while (this.currentBufferedNode.NodeType == JsonNodeType.Property)
            {
                string str2;
                ODataInnerError error2;
                string str3 = (string) this.currentBufferedNode.Value;
                if (str3 == null)
                {
                    goto Label_00CC;
                }
                if (!(str3 == "code"))
                {
                    if (str3 == "message")
                    {
                        goto Label_0090;
                    }
                    if (str3 == "innererror")
                    {
                        goto Label_00A8;
                    }
                    goto Label_00CC;
                }
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.Code) && this.TryReadErrorStringPropertyValue(out str2))
                {
                    error.ErrorCode = str2;
                    goto Label_00CE;
                }
                return false;
            Label_0090:
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.Message) && this.TryReadMessagePropertyValue(error))
                {
                    goto Label_00CE;
                }
                return false;
            Label_00A8:
                if (!ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.InnerError))
                {
                    return false;
                }
                if (!this.TryReadInnerErrorPropertyValue(out error2, 0))
                {
                    return false;
                }
                error.InnerError = error2;
                goto Label_00CE;
            Label_00CC:
                return false;
            Label_00CE:
                this.ReadInternal();
            }
            this.ReadInternal();
            return (none != ODataJsonReaderUtils.ErrorPropertyBitMask.None);
        }

        private bool TryReadErrorStringPropertyValue(out string stringValue)
        {
            this.ReadInternal();
            stringValue = this.currentBufferedNode.Value as string;
            if (this.currentBufferedNode.NodeType != JsonNodeType.PrimitiveValue)
            {
                return false;
            }
            if (this.currentBufferedNode.Value != null)
            {
                return (stringValue != null);
            }
            return true;
        }

        private bool TryReadInnerErrorPropertyValue(out ODataInnerError innerError, int recursionDepth)
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref recursionDepth, this.maxInnerErrorDepth);
            this.ReadInternal();
            if (this.currentBufferedNode.NodeType != JsonNodeType.StartObject)
            {
                innerError = null;
                return false;
            }
            this.ReadInternal();
            innerError = new ODataInnerError();
            ODataJsonReaderUtils.ErrorPropertyBitMask none = ODataJsonReaderUtils.ErrorPropertyBitMask.None;
            while (this.currentBufferedNode.NodeType == JsonNodeType.Property)
            {
                string str2;
                string str3;
                string str4;
                ODataInnerError error;
                string str5 = (string) this.currentBufferedNode.Value;
                if (str5 == null)
                {
                    goto Label_0125;
                }
                if (!(str5 == "message"))
                {
                    if (str5 == "type")
                    {
                        goto Label_00B6;
                    }
                    if (str5 == "stacktrace")
                    {
                        goto Label_00D9;
                    }
                    if (str5 == "internalexception")
                    {
                        goto Label_0100;
                    }
                    goto Label_0125;
                }
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.MessageValue) && this.TryReadErrorStringPropertyValue(out str2))
                {
                    innerError.Message = str2;
                    goto Label_012B;
                }
                return false;
            Label_00B6:
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.TypeName) && this.TryReadErrorStringPropertyValue(out str3))
                {
                    innerError.TypeName = str3;
                    goto Label_012B;
                }
                return false;
            Label_00D9:
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.StackTrace) && this.TryReadErrorStringPropertyValue(out str4))
                {
                    innerError.StackTrace = str4;
                    goto Label_012B;
                }
                return false;
            Label_0100:
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.InnerError) && this.TryReadInnerErrorPropertyValue(out error, recursionDepth))
                {
                    innerError.InnerError = error;
                    goto Label_012B;
                }
                return false;
            Label_0125:
                this.SkipValueInternal();
            Label_012B:
                this.ReadInternal();
            }
            return true;
        }

        internal bool TryReadInStreamErrorPropertyValue(out ODataError error)
        {
            bool flag;
            error = null;
            this.StartBuffering();
            this.parsingInStreamError = true;
            try
            {
                flag = this.TryReadErrorPropertyValue(out error);
            }
            finally
            {
                this.StopBuffering();
                this.parsingInStreamError = false;
            }
            return flag;
        }

        private bool TryReadMessagePropertyValue(ODataError error)
        {
            this.ReadInternal();
            if (this.currentBufferedNode.NodeType != JsonNodeType.StartObject)
            {
                return false;
            }
            this.ReadInternal();
            ODataJsonReaderUtils.ErrorPropertyBitMask none = ODataJsonReaderUtils.ErrorPropertyBitMask.None;
            while (this.currentBufferedNode.NodeType == JsonNodeType.Property)
            {
                string str2;
                string str3;
                string str4 = (string) this.currentBufferedNode.Value;
                if (str4 == null)
                {
                    goto Label_009D;
                }
                if (!(str4 == "lang"))
                {
                    if (str4 == "value")
                    {
                        goto Label_007B;
                    }
                    goto Label_009D;
                }
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.MessageLanguage) && this.TryReadErrorStringPropertyValue(out str2))
                {
                    error.MessageLanguage = str2;
                    goto Label_009F;
                }
                return false;
            Label_007B:
                if (ODataJsonReaderUtils.ErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.MessageValue) && this.TryReadErrorStringPropertyValue(out str3))
                {
                    error.Message = str3;
                    goto Label_009F;
                }
                return false;
            Label_009D:
                return false;
            Label_009F:
                this.ReadInternal();
            }
            return true;
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

        public override JsonNodeType NodeType
        {
            get
            {
                if (this.bufferedNodesHead == null)
                {
                    return base.NodeType;
                }
                if (this.isBuffering)
                {
                    return this.currentBufferedNode.NodeType;
                }
                return this.bufferedNodesHead.NodeType;
            }
        }

        public override object Value
        {
            get
            {
                if (this.bufferedNodesHead == null)
                {
                    return base.Value;
                }
                if (this.isBuffering)
                {
                    return this.currentBufferedNode.Value;
                }
                return this.bufferedNodesHead.Value;
            }
        }

        private sealed class BufferedNode
        {
            private readonly JsonNodeType nodeType;
            private readonly object nodeValue;

            internal BufferedNode(JsonNodeType nodeType, object value)
            {
                this.nodeType = nodeType;
                this.nodeValue = value;
                this.Previous = this;
                this.Next = this;
            }

            internal BufferingJsonReader.BufferedNode Next { get; set; }

            internal JsonNodeType NodeType
            {
                get
                {
                    return this.nodeType;
                }
            }

            internal BufferingJsonReader.BufferedNode Previous { get; set; }

            internal object Value
            {
                get
                {
                    return this.nodeValue;
                }
            }
        }

        private sealed class ObjectRecordPropertyDeduplicationRecord : Dictionary<string, List<BufferingJsonReader.PropertyDeduplicationRecord>>
        {
            internal BufferingJsonReader.PropertyDeduplicationRecord CurrentPropertyRecord { get; set; }
        }

        private sealed class PropertyDeduplicationRecord
        {
            private BufferingJsonReader.BufferedNode lastPropertyValueNode;
            private readonly BufferingJsonReader.BufferedNode propertyNode;

            internal PropertyDeduplicationRecord(BufferingJsonReader.BufferedNode propertyNode)
            {
                this.propertyNode = propertyNode;
            }

            internal BufferingJsonReader.BufferedNode LastPropertyValueNode
            {
                get
                {
                    return this.lastPropertyValueNode;
                }
                set
                {
                    this.lastPropertyValueNode = value;
                }
            }

            internal BufferingJsonReader.BufferedNode PropertyNode
            {
                get
                {
                    return this.propertyNode;
                }
            }
        }
    }
}

