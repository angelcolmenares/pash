using System;
using System.IO;

namespace Pscx.Commands.IO.Compression.ArchiveReader
{
    internal class PscxSevenZipReader : SevenZipBaseEx
    {
        internal PscxSevenZipReader(PscxCmdlet command, FileInfo file, ArchiveFormat format) :
            base(command, file, format)
        {
            command.WriteDebug(String.Format("Created {0} reader for {1}.", format, file));
        }
    }
}
