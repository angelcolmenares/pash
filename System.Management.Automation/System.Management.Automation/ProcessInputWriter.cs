namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class ProcessInputWriter
    {
        private InternalCommand command;
        private NativeCommandIOFormat inputFormat;
        private ArrayList inputList = new ArrayList();
        private Thread inputThread;
        private bool stopping;
        private StreamWriter streamWriter;

        internal ProcessInputWriter(InternalCommand command)
        {
            this.command = command;
        }

        internal void Add(object input)
        {
            this.inputList.Add(input);
        }

        private void ConvertToString()
        {
            PipelineProcessor processor = new PipelineProcessor();
            processor.Add(this.command.Context.CreateCommand("out-string", false));
            object[] c = (object[]) processor.Execute(this.inputList.ToArray());
            this.inputList = new ArrayList(c);
        }

        internal void Done()
        {
            if (this.inputThread != null)
            {
                this.inputThread.Join();
            }
        }

        internal void Start(Process process, NativeCommandIOFormat inputFormat)
        {
            Encoding variableValue = this.command.Context.GetVariableValue(SpecialVariables.OutputEncodingVarPath) as Encoding;
            if (variableValue == null)
            {
                variableValue = Encoding.ASCII;
            }
            this.streamWriter = new StreamWriter(process.StandardInput.BaseStream, variableValue);
            this.inputFormat = inputFormat;
            if (inputFormat == NativeCommandIOFormat.Text)
            {
                this.ConvertToString();
            }
            this.inputThread = new Thread(new ThreadStart(this.WriterThreadProc));
            this.inputThread.Start();
        }

        internal void Stop()
        {
            this.stopping = true;
        }

        private void WriterThreadProc()
        {
            try
            {
                if (this.inputFormat == NativeCommandIOFormat.Text)
                {
                    this.WriteTextInput();
                }
                else
                {
                    this.WriteXmlInput();
                }
            }
            catch (IOException)
            {
            }
        }

        private void WriteTextInput()
        {
            try
            {
                foreach (object obj2 in this.inputList)
                {
                    if (this.stopping)
                    {
                        return;
                    }
                    string str = PSObject.ToStringParser(this.command.Context, obj2);
                    this.streamWriter.Write(str);
                }
            }
            finally
            {
                this.streamWriter.Close();
            }
        }

        private void WriteXmlInput()
        {
            try
            {
                this.streamWriter.WriteLine("#< CLIXML");
                XmlWriter writer = new XmlTextWriter(this.streamWriter);
                Serializer serializer = new Serializer(writer);
                foreach (object obj2 in this.inputList)
                {
                    if (this.stopping)
                    {
                        return;
                    }
                    serializer.Serialize(obj2);
                }
                serializer.Done();
            }
            finally
            {
                this.streamWriter.Close();
            }
        }

        internal int Count
        {
            get
            {
                return this.inputList.Count;
            }
        }
    }
}

