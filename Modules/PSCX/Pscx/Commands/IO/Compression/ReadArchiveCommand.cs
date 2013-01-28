using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell.Commands;

using Pscx.Commands.IO.Compression.ArchiveReader;
using Pscx.IO;
using SharpCompress.Archive;

namespace Pscx.Commands.IO.Compression
{
    /// <summary>
    /// Enumerates archives and writes an object for each distinct entry in the archive.
    /// </summary>
    [OutputType(typeof(ArchiveEntry))]
	[Cmdlet(VerbsCommunications.Read, PscxNouns.Archive,
        DefaultParameterSetName = ParameterSetPath)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ReadArchiveCommand : PscxInputObjectPathCommandBase
	{
        public ReadArchiveCommand()
        {
            //this.Format = null;
        }

        //[Parameter(Position = 0, ParameterSetName = ParameterSetObject)]
        //[Parameter(Position = 1, ParameterSetName = ParameterSetPath)]
        //public ArchiveFormat? Format { get; set; }

        /// <summary>
        /// Show progress for reading archives.
        /// </summary>
        [Parameter]
        public SwitchParameter ShowProgress { get; set; }

        /// <summary>
        /// If present, write out an ArchiveEntry object for each directory/folder entry in the archive.
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeDirectories { get; set; }

        protected override bool OnValidatePscxPath(string parameterName, IPscxPathSettings settings)
        {
            if (parameterName == "LiteralPath")
            {
                settings.ShouldExist = true;
            }
            return true;
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            RegisterInputType<FileInfo>(ProcessArchive);
            RegisterPathInputType<FileInfo>();
        }

        protected virtual void ProcessArchive(FileInfo fileInfo)
        {
            var entries = new List<ArchiveEntry>();
            
			var archive = ArchiveFactory.Open (fileInfo);

            // don't need password to dump header/entries
            /*using (var extractor = new SevenZipExtractor(archive.FullName))
            {
            */
				int total = archive.Entries.Count();
                ProgressRecord progress = null;
                foreach (IArchiveEntry info in archive.Entries)
                {
                    if (ShowProgress)
                    {
                        progress = new ProgressRecord(1, "Scanning...", fileInfo.FullName)
                                           {
                                               CurrentOperation = info.FilePath,
                                               PercentComplete = (int) Math.Floor(((float) info.Index/total)*100),
                                               RecordType = ProgressRecordType.Processing
                                           };
                        WriteProgress(progress);
                    }

                    if (this.IncludeDirectories || !info.IsDirectory)
                    {
                        entries.Add(new ArchiveEntry(info, fileInfo.FullName, ToArchiveFormat (archive.Type)));
                    }
                }
                if (ShowProgress && progress != null)
                {
                    progress.PercentComplete = 100;
                    progress.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progress);
                }
            //}

            WriteObject(entries, enumerateCollection: false); // true? which is better for piping to expand-archive?

            //ArchiveFormat format = this.Format;           
            //if ((format != ArchiveFormat.Unknown) ||
            //    SevenZipBase.TryGetFormat(this, archive, ref format))
            //{
            //    using (var reader = new PscxSevenZipReader(this, archive, format))
            //    {
            //        reader.ShowScanProgress = ShowProgress;

            //        foreach (ArchiveEntry entry in reader)
            //        {
            //            // only dump out the zero-length folder entries
            //            // if instructed to.
            //            if (this.IncludeDirectories || !entry.IsFolder)
            //            {
            //                WriteObject(entry);
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    // todo: localize
            //    // unknown file extension / format and no -Format override specified
            //    ErrorHandler.HandleError(true,
            //        new ErrorRecord(new PSArgumentException("Unknown file extension or invalid archive. You may override the format used with the -Format parameter."),
            //            "UnknownArchiveFormat", ErrorCategory.ReadError, archive));
            //}
        }

		private static ArchiveFormat ToArchiveFormat (SharpCompress.Common.ArchiveType type)
		{
			switch (type) {
			case SharpCompress.Common.ArchiveType.GZip:
				return ArchiveFormat.GZip;
			case SharpCompress.Common.ArchiveType.Zip:
				return ArchiveFormat.Zip;
			case SharpCompress.Common.ArchiveType.Rar:
				return ArchiveFormat.Rar;
			case SharpCompress.Common.ArchiveType.Tar:
				return ArchiveFormat.Tar;
			case SharpCompress.Common.ArchiveType.SevenZip:
				return ArchiveFormat.SevenZip;
			default: 
				return ArchiveFormat.Unknown;
			}
		}

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            var archive = new FileInfo(pscxPath.ProviderPath);
            ProcessArchive(archive);
        }
    }
}
