using System.IO;
using SharpCompress.Common;

namespace SharpCompress.Archive
{
    public static class AbstractWritableArchiveExtensions
    {

       public static void SaveTo<TEntry, TVolume>(this AbstractWritableArchive<TEntry, TVolume> writableArchive,
             Stream stream, CompressionType compressionType)
          where TEntry : IArchiveEntry
          where TVolume : IVolume
       {
          writableArchive.SaveTo(stream, new CompressionInfo { Type = compressionType });
       }
#if !PORTABLE
        public static void AddEntry<TEntry, TVolume>(this AbstractWritableArchive<TEntry, TVolume> writableArchive,
		    int index, string entryPath, string filePath)
            where TEntry : IArchiveEntry
            where TVolume : IVolume
        {
			writableArchive.AddEntry(index, entryPath, new FileInfo(filePath));
        }

        public static void SaveTo<TEntry, TVolume>(this AbstractWritableArchive<TEntry, TVolume> writableArchive,
            string filePath, CompressionType compressionType)
            where TEntry : IArchiveEntry
            where TVolume : IVolume
        {
           writableArchive.SaveTo(new FileInfo(filePath), new CompressionInfo { Type = compressionType });
        }

        public static void SaveTo<TEntry, TVolume>(this AbstractWritableArchive<TEntry, TVolume> writableArchive,
             FileInfo fileInfo, CompressionType compressionType)
            where TEntry : IArchiveEntry
            where TVolume : IVolume
        {
            using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write))
            {
                writableArchive.SaveTo(stream, new CompressionInfo { Type = compressionType });
            }
        }

        public static void SaveTo<TEntry, TVolume>(this AbstractWritableArchive<TEntry, TVolume> writableArchive,
             string filePath, CompressionInfo compressionInfo)
           where TEntry : IArchiveEntry
           where TVolume : IVolume
        {
           writableArchive.SaveTo(new FileInfo(filePath), compressionInfo);
        }

        public static void SaveTo<TEntry, TVolume>(this AbstractWritableArchive<TEntry, TVolume> writableArchive,
             FileInfo fileInfo, CompressionInfo compressionInfo)
           where TEntry : IArchiveEntry
           where TVolume : IVolume
        {
           using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write))
           {
              writableArchive.SaveTo(stream, compressionInfo);
           }
        }

        public static void AddAllFromDirectory<TEntry, TVolume>(this AbstractWritableArchive<TEntry, TVolume> writableArchive,
            string filePath, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
            where TEntry : IArchiveEntry
            where TVolume : IVolume
        {
			int i = 1;
#if THREEFIVE
            foreach (var path in Directory.GetFiles(filePath, searchPattern, searchOption))
#else
            foreach (var path in Directory.EnumerateFiles(filePath, searchPattern, searchOption))
#endif

            {
                writableArchive.AddEntry(i++, path.Substring(filePath.Length), new FileInfo(path));
            }
        }
#endif
    }
}
