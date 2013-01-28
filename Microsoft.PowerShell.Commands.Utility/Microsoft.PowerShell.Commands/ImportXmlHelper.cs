namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Management.Automation;
    using System.Xml;

    internal class ImportXmlHelper : IDisposable
    {
        private readonly PSCmdlet _cmdlet;
        private Deserializer _deserializer;
        private bool _disposed;
        internal FileStream _fs;
        private readonly string _path;
        internal XmlReader _xr;
        private bool isLiteralPath;

        internal ImportXmlHelper(string fileName, PSCmdlet cmdlet, bool isLiteralPath)
        {
            if (fileName == null)
            {
                throw PSTraceSource.NewArgumentNullException("fileName");
            }
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            this._path = fileName;
            this._cmdlet = cmdlet;
            this.isLiteralPath = isLiteralPath;
        }

        private void CleanUp()
        {
            if (this._fs != null)
            {
                this._fs.Close();
                this._fs = null;
            }
        }

        internal void CreateFileStream()
        {
            this._fs = PathUtils.OpenFileStream(this._path, this._cmdlet, this.isLiteralPath);
            this._xr = CreateXmlReader(this._fs);
        }

        private static XmlReader CreateXmlReader(Stream stream)
        {
            TextReader input = new StreamReader(stream);
            if ((input.Peek() == "#< CLIXML"[0]) && !input.ReadLine().Equals("#< CLIXML", StringComparison.Ordinal))
            {
                stream.Seek(0L, SeekOrigin.Begin);
            }
            return XmlReader.Create(input, InternalDeserializer.XmlReaderSettingsForCliXml);
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.CleanUp();
            }
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        internal void Import()
        {
            this.CreateFileStream();
            this._deserializer = new Deserializer(this._xr);
            if (this._cmdlet.PagingParameters.IncludeTotalCount != 0)
            {
                PSObject sendToPipeline = this._cmdlet.PagingParameters.NewTotalCount(0L, 0.0);
                this._cmdlet.WriteObject(sendToPipeline);
            }
            ulong skip = this._cmdlet.PagingParameters.Skip;
            ulong first = this._cmdlet.PagingParameters.First;
            if ((skip == 0L) && (first == ulong.MaxValue))
            {
                ulong num3 = 0L;
                while (!this._deserializer.Done())
                {
                    object obj3 = this._deserializer.Deserialize();
                    num3 += (ulong) 1L;
                    if (num3 >= skip)
                    {
                        if (first == 0L)
                        {
                            return;
                        }
                        this._cmdlet.WriteObject(obj3);
                        first -= (ulong) 1L;
                    }
                }
            }
            else
            {
                ulong num4 = 0L;
                ulong num5 = 0L;
                while (!this._deserializer.Done() && (num5 < first))
                {
                    object obj4 = this._deserializer.Deserialize();
                    PSObject obj5 = obj4 as PSObject;
                    if (obj5 == null)
                    {
                        num4 += (ulong) 1L;
                        if (num4 >= skip)
                        {
                            num5 += (ulong) 1L;
                            this._cmdlet.WriteObject(obj4);
                            continue;
                        }
                    }
                    ICollection baseObject = obj5.BaseObject as ICollection;
                    if (baseObject != null)
                    {
                        foreach (object obj6 in baseObject)
                        {
                            if (num5 >= first)
                            {
                                continue;
                            }
                            num4 += (ulong) 1L;
                            if (num4 >= skip)
                            {
                                num5 += (ulong) 1L;
                                this._cmdlet.WriteObject(obj6);
                            }
                        }
                    }
                    else
                    {
                        num4 += (ulong) 1L;
                        if (num4 >= skip)
                        {
                            num5 += (ulong) 1L;
                            this._cmdlet.WriteObject(obj4);
                        }
                    }
                }
            }
        }

        internal void Stop()
        {
            if (this._deserializer != null)
            {
                this._deserializer.Stop();
            }
        }
    }
}

