using System.IO;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Common.GZip;

namespace SharpCompress.Archive.GZip
{
    public class GZipArchiveEntry : GZipEntry, IArchiveEntry
    {
        private GZipArchive archive;

        internal GZipArchiveEntry(int index, GZipArchive archive, GZipFilePart part)
            : base(part)
        {
			this.Index = index;
            this.archive = archive;
        }

		public int Index
		{
			get;
			private set;
		}

        public virtual Stream OpenEntryStream()
        {
            return Parts.Single().GetStream();
        }

        #region IArchiveEntry Members

        public void WriteTo(Stream streamToWriteTo)
        {
            if (IsEncrypted)
            {
                throw new PasswordProtectedException("Entry is password protected and cannot be extracted.");
            }
            this.Extract(archive, streamToWriteTo);
        }

        public bool IsComplete
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}
