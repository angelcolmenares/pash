namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class ProcessStreamReader
    {
        private bool isOutput;
        private ProcessOutputReader processOutputReader;
        private string processPath;
        private StreamReader streamReader;
        private Thread thread;
        private PipelineWriter writer;

        internal ProcessStreamReader(StreamReader streamReader, string processPath, bool isOutput, PipelineWriter writer, ProcessOutputReader processOutputReader)
        {
            this.streamReader = streamReader;
            this.processPath = processPath;
            this.isOutput = isOutput;
            this.writer = writer;
            this.processOutputReader = processOutputReader;
        }

        private void AddObjectToWriter(object data, MinishellStream stream)
        {
            try
            {
                ProcessOutputObject obj2 = new ProcessOutputObject(data, stream);
                lock (this.writer)
                {
                    this.writer.Write(obj2);
                }
            }
            catch (PipelineClosedException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal void Done()
        {
            if (this.thread != null)
            {
                this.thread.Join();
            }
        }

        private void ReaderStartProc()
        {
            try
            {
                this.ReaderStartProcHelper();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            finally
            {
                this.processOutputReader.ReaderDone(this.isOutput);
            }
        }

        private void ReaderStartProcHelper()
        {
            string line = this.streamReader.ReadLine();
            if (line != null)
            {
                if (!line.Equals("#< CLIXML", StringComparison.Ordinal))
                {
                    this.ReadText(line);
                }
                else
                {
                    this.ReadXml();
                }
            }
        }

        private void ReadText(string line)
        {
            if (this.isOutput)
            {
                while (line != null)
                {
                    this.AddObjectToWriter(line, MinishellStream.Output);
                    line = this.streamReader.ReadLine();
                }
            }
            else
            {
                ErrorRecord data = new ErrorRecord(new RemoteException(line), "NativeCommandError", ErrorCategory.NotSpecified, line);
                this.AddObjectToWriter(data, MinishellStream.Error);
                char[] buffer = new char[0x1000];
                int charCount = 0;
                while ((charCount = this.streamReader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    StringBuilder builder = new StringBuilder().Append(buffer, 0, charCount);
                    this.AddObjectToWriter(new ErrorRecord(new RemoteException(builder.ToString()), "NativeCommandErrorMessage", ErrorCategory.NotSpecified, null), MinishellStream.Error);
                }
            }
        }

        private void ReadXml()
        {
            try
            {
                Deserializer deserializer = new Deserializer(XmlReader.Create(this.streamReader, InternalDeserializer.XmlReaderSettingsForCliXml));
                while (!deserializer.Done())
                {
                    string str;
                    object obj2 = deserializer.Deserialize(out str);
                    MinishellStream unknown = MinishellStream.Unknown;
                    if (str != null)
                    {
                        unknown = StringToMinishellStreamConverter.ToMinishellStream(str);
                    }
                    if (unknown == MinishellStream.Unknown)
                    {
                        unknown = this.isOutput ? MinishellStream.Output : MinishellStream.Error;
                    }
                    if ((unknown == MinishellStream.Output) || (obj2 != null))
                    {
                        if (unknown == MinishellStream.Error)
                        {
                            if (obj2 is PSObject)
                            {
                                obj2 = ErrorRecord.FromPSObjectForRemoting(PSObject.AsPSObject(obj2));
                            }
                            else
                            {
                                string targetObject = null;
                                try
                                {
                                    targetObject = (string) LanguagePrimitives.ConvertTo(obj2, typeof(string), CultureInfo.InvariantCulture);
                                }
                                catch (PSInvalidCastException)
                                {
                                    continue;
                                }
                                obj2 = new ErrorRecord(new RemoteException(targetObject), "NativeCommandError", ErrorCategory.NotSpecified, targetObject);
                            }
                        }
                        else if (((unknown == MinishellStream.Debug) || (unknown == MinishellStream.Verbose)) || (unknown == MinishellStream.Warning))
                        {
                            try
                            {
                                obj2 = LanguagePrimitives.ConvertTo(obj2, typeof(string), CultureInfo.InvariantCulture);
                            }
                            catch (PSInvalidCastException)
                            {
                                continue;
                            }
                        }
                        this.AddObjectToWriter(obj2, unknown);
                    }
                }
            }
            catch (XmlException exception)
            {
                string cliXmlError = NativeCP.CliXmlError;
                XmlException exception2 = new XmlException(string.Format(null, cliXmlError, new object[] { this.isOutput ? MinishellStream.Output : MinishellStream.Error, this.processPath, exception.Message }), exception);
                ErrorRecord data = new ErrorRecord(exception2, "ProcessStreamReader_CliXmlError", ErrorCategory.SyntaxError, this.processPath);
                this.AddObjectToWriter(data, MinishellStream.Error);
            }
        }

        internal void Start()
        {
            this.thread = new Thread(new ThreadStart(this.ReaderStartProc));
            if (this.isOutput)
            {
                this.thread.Name = string.Format(CultureInfo.InvariantCulture, "{0} :Output Reader", new object[] { this.processPath });
            }
            else
            {
                this.thread.Name = string.Format(CultureInfo.InvariantCulture, "{0} :Error Reader", new object[] { this.processPath });
            }
            this.thread.Start();
        }
    }
}

