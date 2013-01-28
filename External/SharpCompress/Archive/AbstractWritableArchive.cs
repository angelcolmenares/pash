using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;

namespace SharpCompress.Archive
{
    public abstract class AbstractWritableArchive<TEntry, TVolume> : AbstractArchive<TEntry, TVolume>
        where TEntry : IArchiveEntry
        where TVolume : IVolume
    {
        private List<TEntry> newEntries = new List<TEntry>();
        private List<TEntry> removedEntries = new List<TEntry>();

        private List<TEntry> modifiedEntries = new List<TEntry>();
        private bool hasModifications;
        private bool anyNotWritable;

#if !PORTABLE
        internal AbstractWritableArchive(ArchiveType type, FileInfo fileInfo, Options options)
            : base(type, fileInfo, options)
        {
        }

		public void AddEntry(int index, string filePath, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("FileInfo does not exist.");
            }
            AddEntry(index, filePath, fileInfo.OpenRead(), fileInfo.Length, fileInfo.LastWriteTime);
        }
#endif

        internal AbstractWritableArchive(ArchiveType type)
            : base(type)
        {
        }

        internal AbstractWritableArchive(ArchiveType type, IEnumerable<Stream> streams, Options options)
            : base(type, streams, options)
        {
            if (streams.Any(x => !x.CanWrite))
            {
                anyNotWritable = true;
            }
        }

        private void CheckWritable()
        {
            if (anyNotWritable)
            {
                throw new ArchiveException("All Archive streams must be Writable to use Archive writing functionality.");
            }
        }

        public override ICollection<TEntry> Entries
        {
            get
            {
                if (hasModifications)
                {
                    return modifiedEntries;
                }
                return base.Entries;
            }
        }

        private void RebuildModifiedCollection()
        {
            hasModifications = true;
            modifiedEntries.Clear();
            modifiedEntries.AddRange(OldEntries.Concat(newEntries));
        }

        private IEnumerable<TEntry> OldEntries
        {
            get
            {
                return base.Entries.Where(x => !removedEntries.Contains(x));
            }
        }

        public void RemoveEntry(TEntry entry)
        {
            CheckWritable();
            if (!removedEntries.Contains(entry))
            {
                removedEntries.Add(entry);
                RebuildModifiedCollection();
            }
        }

		public void AddEntry(int index, string filePath, Stream source,
            long size = 0, DateTime? modified = null)
        {
            CheckWritable();
            newEntries.Add(CreateEntry(index, filePath, source, size, modified));
            RebuildModifiedCollection();
        }

        public void SaveTo(Stream stream, CompressionInfo compressionType)
        {
            SaveTo(stream, compressionType, OldEntries, newEntries);
        }

		protected abstract TEntry CreateEntry(int index, string filePath, Stream source, long size, DateTime? modified);
        protected abstract void SaveTo(Stream stream, CompressionInfo compressionType,
            IEnumerable<TEntry> oldEntries, IEnumerable<TEntry> newEntries);
    }
}
