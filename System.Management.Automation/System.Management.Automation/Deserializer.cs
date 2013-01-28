namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Deserializer
    {
        private readonly DeserializationContext _context;
        private readonly InternalDeserializer _deserializer;
        private bool _done;
        private readonly XmlReader _reader;
        private const string DeserializationTypeNamePrefix = "Deserialized.";

        internal Deserializer(XmlReader reader) : this(reader, new DeserializationContext())
        {
        }

        internal Deserializer(XmlReader reader, DeserializationContext context)
        {
            if (reader == null)
            {
                throw PSTraceSource.NewArgumentNullException("reader");
            }
            this._reader = reader;
            this._context = context;
            this._deserializer = new InternalDeserializer(this._reader, this._context);
            try
            {
                this.Start();
            }
            catch (XmlException exception)
            {
                ReportExceptionForETW(exception);
                throw;
            }
        }

        internal static void AddDeserializationPrefix(ref string type)
        {
            if (!type.StartsWith("Deserialized.", StringComparison.OrdinalIgnoreCase))
            {
                type = type.Insert(0, "Deserialized.");
            }
        }

        internal object Deserialize()
        {
            string str;
            return this.Deserialize(out str);
        }

        internal object Deserialize(out string streamName)
        {
            object obj2;
            if (this.Done())
            {
                throw PSTraceSource.NewInvalidOperationException("Serialization", "ReadCalledAfterDone", new object[0]);
            }
            try
            {
                obj2 = this._deserializer.ReadOneObject(out streamName);
            }
            catch (XmlException exception)
            {
                ReportExceptionForETW(exception);
                throw;
            }
            return obj2;
        }

        internal bool Done()
        {
            if (!this._done)
            {
                if (DeserializationOptions.NoRootElement == (this._context.options & DeserializationOptions.NoRootElement))
                {
                    this._done = this._reader.EOF;
                }
                else if (this._reader.NodeType == XmlNodeType.EndElement)
                {
                    try
                    {
                        this._reader.ReadEndElement();
                    }
                    catch (XmlException exception)
                    {
                        ReportExceptionForETW(exception);
                        throw;
                    }
                    this._done = true;
                }
            }
            return this._done;
        }

        internal static bool IsDeserializedInstanceOfType(object o, Type type)
        {
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            if (o != null)
            {
                PSObject obj2 = o as PSObject;
                if (obj2 != null)
                {
                    IEnumerable<string> internalTypeNames = obj2.InternalTypeNames;
                    if (internalTypeNames != null)
                    {
                        foreach (string str in internalTypeNames)
                        {
                            if (((str.Length == ("Deserialized.".Length + type.FullName.Length)) && str.StartsWith("Deserialized.", StringComparison.OrdinalIgnoreCase)) && str.EndsWith(type.FullName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal static bool IsInstanceOfType(object o, Type type)
        {
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            if (o == null)
            {
                return false;
            }
            if (!type.IsAssignableFrom(PSObject.Base(o).GetType()))
            {
                return IsDeserializedInstanceOfType(o, type);
            }
            return true;
        }

        internal static Collection<string> MaskDeserializationPrefix(Collection<string> typeNames)
        {
            bool flag = false;
            Collection<string> collection = new Collection<string>();
            foreach (string str in typeNames)
            {
                if (str.StartsWith("Deserialized.", StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    collection.Add(str.Substring("Deserialized.".Length));
                }
                else
                {
                    collection.Add(str);
                }
            }
            if (flag)
            {
                return collection;
            }
            return null;
        }

        internal static string MaskDeserializationPrefix(string typeName)
        {
            if (typeName == null)
            {
                return null;
            }
            if (typeName.StartsWith("Deserialized.", StringComparison.OrdinalIgnoreCase))
            {
                typeName = typeName.Substring("Deserialized.".Length);
            }
            return typeName;
        }

        private static void ReportExceptionForETW(XmlException exception)
        {
            PSEtwLog.LogAnalyticError(PSEventId.Serializer_XmlExceptionWhenDeserializing, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { exception.LineNumber, exception.LinePosition, exception.ToString() });
        }

        private void Start()
        {
            this._reader.Read();
            string version = "1.1.0.1";
            if (DeserializationOptions.NoRootElement == (this._context.options & DeserializationOptions.NoRootElement))
            {
                this._done = this._reader.EOF;
            }
            else
            {
                this._reader.MoveToContent();
                string attribute = this._reader.GetAttribute("Version");
                if (attribute != null)
                {
                    version = attribute;
                }
                if (!this._deserializer.ReadStartElementAndHandleEmpty("Objs"))
                {
                    this._done = true;
                }
            }
            this._deserializer.ValidateVersion(version);
        }

        internal void Stop()
        {
            this._deserializer.Stop();
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this._deserializer.TypeTable;
            }
            set
            {
                this._deserializer.TypeTable = value;
            }
        }
    }
}

