using System.Text.RegularExpressions;
using SimpleBackup.Abstractions;

namespace SimpleBackup.Engine.Compressors
{
    public sealed class ArchiveNameService(IDateTimeService dateTimeService, IFileSystemService fileSystemService) : IArchiveNameService
    {
        public const string ARCHIVE = nameof(ARCHIVE);
        public const string DATE_TIME_PATTERN = "yyyy_MM_dd_HH_mm_ss";
        private const string DIRECTORY_PATTERN = "????_??_??_??_??_??";
        private static readonly Regex _dateTimeRegex = new Regex(@"\d{4}(_\d{2}){5}");

        public string ConstructArchiveFolderName()
        {
            return $"{ARCHIVE}_{dateTimeService.Now.ToString(DATE_TIME_PATTERN)}";
        }

        public TimeSpan GetTimePassedFromLatesFinishedArchive(string mainFolder)
        {
            DateTime latestArchiveTime = GetNewestFinishedArchiveTime(mainFolder);
            return dateTimeService.Now - latestArchiveTime;
        }

        private DateTime GetNewestFinishedArchiveTime(string directoryWithArchives)
        {
            IReadOnlyCollection<string> archiveFolders = fileSystemService.GetFolders(directoryWithArchives, $"{ARCHIVE}_{DIRECTORY_PATTERN}.{IArchiveNameService.FINISHED}");

            if (archiveFolders.Count == 0)
            {
                return DateTime.MinValue;
            }

            var latestDate = DateTime.MinValue;
            foreach (string folder in archiveFolders)
            {
                string folderName = Path.GetFileName(folder);
                var match = _dateTimeRegex.Match(folderName);
                int[] dateTimeparts = match.Value.Split('_').Select(s => Convert.ToInt32(s)).ToArray();
                var date = new DateTime(dateTimeparts[0], dateTimeparts[1], dateTimeparts[2], dateTimeparts[3], dateTimeparts[4], dateTimeparts[5]);

                if (date > latestDate)
                {
                    latestDate = date;
                }
            }

            return latestDate;
        }
    }
}
