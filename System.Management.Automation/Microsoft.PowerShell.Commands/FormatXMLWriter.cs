namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Xml;

    internal static class FormatXMLWriter
    {
        internal static void WriteToPS1XML(PSCmdlet cmdlet, List<ExtendedTypeDefinition> typeDefinitions, string filepath, bool force, bool noclobber, bool writeScritBlock, bool isLiteralPath)
        {
            StreamWriter writer;
            FileStream stream;
            FileInfo info;
            PathUtils.MasterStreamOpen(cmdlet, filepath, "ascii", true, false, force, noclobber, out stream, out writer, out info, isLiteralPath);
            XmlWriter writer2 = XmlWriter.Create(writer);
            FormatXMLHelper.WriteToXML(writer2, typeDefinitions, writeScritBlock);
            writer2.Close();
            writer.Close();
            stream.Close();
        }
    }
}

