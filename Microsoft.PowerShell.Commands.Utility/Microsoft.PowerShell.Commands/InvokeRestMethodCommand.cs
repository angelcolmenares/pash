namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    [Cmdlet("Invoke", "RestMethod", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217034")]
    public class InvokeRestMethodCommand : WebRequestPSCmdlet
    {
        private RestReturnType CheckReturnType(WebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            RestReturnType detect = RestReturnType.Detect;
            string contentType = ContentHelper.GetContentType(response);
            if (string.IsNullOrEmpty(contentType))
            {
                return RestReturnType.Detect;
            }
            if (ContentHelper.IsJson(contentType))
            {
                return RestReturnType.Json;
            }
            if (ContentHelper.IsXml(contentType))
            {
                detect = RestReturnType.Xml;
            }
            return detect;
        }

        private XmlReaderSettings GetSecureXmlReaderSettings()
        {
            return new XmlReaderSettings { CheckCharacters = false, CloseInput = false, IgnoreProcessingInstructions = true, MaxCharactersFromEntities = 0x400L, DtdProcessing = DtdProcessing.Ignore };
        }

        internal override void ProcessResponse(WebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            MemoryStream stream = new MemoryStream();
            using (BufferingStreamReader reader = new BufferingStreamReader(StreamHelper.GetResponseStream(response)))
            {
                if (base.ShouldWriteToPipeline && !this.TryProcessFeedStream(reader))
                {
                    RestReturnType type = this.CheckReturnType(response);
                    stream = StreamHelper.ReadStream(reader, response.ContentLength, this);
                    Encoding encoding = ContentHelper.GetEncoding(response);
                    object obj2 = null;
                    Exception exRef = null;
                    string json = StreamHelper.DecodeStream(stream, encoding);
                    bool flag = false;
                    if (type == RestReturnType.Json)
                    {
                        flag = this.TryConvertToJson(json, out obj2, ref exRef) || this.TryConvertToXml(json, out obj2, ref exRef);
                    }
                    else
                    {
                        flag = this.TryConvertToXml(json, out obj2, ref exRef) || this.TryConvertToJson(json, out obj2, ref exRef);
                    }
                    if (!flag)
                    {
                        obj2 = json;
                    }
                    base.WriteObject(obj2);
                }
                if (base.ShouldSaveToOutFile)
                {
                    StreamHelper.SaveStreamToFile(StreamHelper.ReadStream(reader, response.ContentLength, this), base.QualifiedOutFile, this);
                }
            }
        }

        private bool TryConvertToJson(string json, out object obj, ref Exception exRef)
        {
            try
            {
                ErrorRecord record;
                obj = JsonObject.ConvertFromJson(json, out record);
                if (record != null)
                {
                    exRef = record.Exception;
                    obj = null;
                }
            }
            catch (ArgumentException exception)
            {
                exRef = exception;
                obj = null;
            }
            catch (InvalidOperationException exception2)
            {
                exRef = exception2;
                obj = null;
            }
            return (null != obj);
        }

        private bool TryConvertToXml(string xml, out object doc, ref Exception exRef)
        {
            try
            {
                XmlReaderSettings secureXmlReaderSettings = this.GetSecureXmlReaderSettings();
                XmlReader reader = XmlReader.Create(new StringReader(xml), secureXmlReaderSettings);
                doc = new XmlDocument();
                ((XmlDocument) doc).PreserveWhitespace = true;
                ((XmlDocument) doc).Load(reader);
            }
            catch (XmlException exception)
            {
                exRef = exception;
                doc = null;
            }
            return (null != doc);
        }

        private bool TryProcessFeedStream(BufferingStreamReader responseStream)
        {
            bool flag = false;
            try
            {
                XmlReaderSettings secureXmlReaderSettings = this.GetSecureXmlReaderSettings();
                XmlReader reader = XmlReader.Create(responseStream, secureXmlReaderSettings);
                for (int i = 0; (i < 10) && reader.Read(); i++)
                {
                    if (string.Equals("rss", reader.Name, StringComparison.OrdinalIgnoreCase) || string.Equals("feed", reader.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    XmlDocument document = new XmlDocument();
                    while (reader.Read())
                    {
                        if ((reader.NodeType == XmlNodeType.Element) && (string.Equals("Item", reader.Name, StringComparison.OrdinalIgnoreCase) || string.Equals("Entry", reader.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            XmlNode sendToPipeline = document.ReadNode(reader);
                            base.WriteObject(sendToPipeline);
                        }
                    }
                }
                return flag;
            }
            catch (XmlException)
            {
            }
            finally
            {
                responseStream.Seek(0L, SeekOrigin.Begin);
            }
            return flag;
        }

        [Parameter]
        public override WebRequestMethod Method
        {
            get
            {
                return base.Method;
            }
            set
            {
                base.Method = value;
            }
        }
    }
}

