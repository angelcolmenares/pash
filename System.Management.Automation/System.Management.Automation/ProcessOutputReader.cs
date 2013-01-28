namespace System.Management.Automation
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation.Internal;

    internal class ProcessOutputReader
    {
        private ProcessStreamReader errorReader;
        private ProcessStreamReader outputReader;
        private Process process;
        private ObjectStream processOutput;
        private string processPath;
        private int readerCount;
        private object readerLock = new object();
        private bool redirectError;
        private bool redirectOutput;

        internal ProcessOutputReader(Process process, string processPath, bool redirectOutput, bool redirectError)
        {
            this.process = process;
            this.processPath = processPath;
            this.redirectOutput = redirectOutput;
            this.redirectError = redirectError;
        }

        internal void Done()
        {
            if (this.outputReader != null)
            {
                this.outputReader.Done();
            }
            if (this.errorReader != null)
            {
                this.errorReader.Done();
            }
        }

        internal object Read()
        {
            return this.processOutput.ObjectReader.Read();
        }

        internal void ReaderDone(bool isOutput)
        {
            int num;
            lock (this.readerLock)
            {
                num = --this.readerCount;
            }
            if (num == 0)
            {
                this.processOutput.ObjectWriter.Close();
            }
        }

        internal void Start()
        {
            this.processOutput = new ObjectStream(0x80);
            lock (this.readerLock)
            {
                if (this.redirectOutput)
                {
                    this.readerCount++;
                    this.outputReader = new ProcessStreamReader(this.process.StandardOutput, this.processPath, true, this.processOutput.ObjectWriter, this);
                    this.outputReader.Start();
                }
                if (this.redirectError)
                {
                    this.readerCount++;
                    this.errorReader = new ProcessStreamReader(this.process.StandardError, this.processPath, false, this.processOutput.ObjectWriter, this);
                    this.errorReader.Start();
                }
            }
        }

        internal void Stop()
        {
            if (this.processOutput != null)
            {
                try
                {
                    this.processOutput.ObjectReader.Close();
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
                try
                {
                    this.processOutput.Close();
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                }
            }
        }
    }
}

