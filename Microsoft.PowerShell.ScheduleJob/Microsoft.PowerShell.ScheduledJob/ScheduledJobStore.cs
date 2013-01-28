using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.ScheduledJob
{
	internal class ScheduledJobStore
	{
		public static string ScheduledJobsPath 
		{
			get {  return OSHelper.IsUnix ? "Xamarin/PowerShell/ScheduledJobs" : "Microsoft\\Windows\\PowerShell\\ScheduledJobs"; }
		}

		public const string DefinitionFileName = "ScheduledJobDefinition";

		public const string JobRunOutput = "Output";

		public const string ScheduledJobDefExistsFQEID = "ScheduledJobDefExists";

		public ScheduledJobStore()
		{
		}

		private static void AddFullAccessToDirectory(string user, string directoryPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
			DirectorySecurity accessControl = directoryInfo.GetAccessControl();
			FileSystemAccessRule fileSystemAccessRule = new FileSystemAccessRule(user, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
			accessControl.AddAccessRule(fileSystemAccessRule);
			directoryInfo.SetAccessControl(accessControl);
		}

		private static string ConvertDateTimeToJobRunName(DateTime dt)
		{
			object[] year = new object[7];
			year[0] = dt.Year;
			year[1] = dt.Month;
			year[2] = dt.Day;
			year[3] = dt.Hour;
			year[4] = dt.Minute;
			year[5] = dt.Second;
			year[6] = dt.Millisecond;
			return string.Format(CultureInfo.InvariantCulture, "{0:d4}{1:d2}{2:d2}-{3:d2}{4:d2}{5:d2}-{6:d3}", year);
		}

		internal static bool ConvertJobRunNameToDateTime(string jobRunName, out DateTime jobRun)
		{
			if (jobRunName == null || jobRunName.Length != 19)
			{
				jobRun = new DateTime();
				return false;
			}
			else
			{
				int num = 0;
				int num1 = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				bool flag = true;
				try
				{
					num = Convert.ToInt32(jobRunName.Substring(0, 4));
					num1 = Convert.ToInt32(jobRunName.Substring(4, 2));
					num2 = Convert.ToInt32(jobRunName.Substring(6, 2));
					num3 = Convert.ToInt32(jobRunName.Substring(9, 2));
					num4 = Convert.ToInt32(jobRunName.Substring(11, 2));
					num5 = Convert.ToInt32(jobRunName.Substring(13, 2));
					num6 = Convert.ToInt32(jobRunName.Substring(16, 3));
				}
				catch (FormatException formatException)
				{
					flag = false;
				}
				catch (OverflowException overflowException)
				{
					flag = false;
				}
				if (!flag)
				{
					jobRun = new DateTime();
				}
				else
				{
					jobRun = new DateTime(num, num1, num2, num3, num4, num5, num6);
				}
				return flag;
			}
		}

		public static FileStream CreateFileForJobDefinition(string definitionName)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				string str = ScheduledJobStore.CreateFilePathName(definitionName, "ScheduledJobDefinition");
				return File.Create(str);
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static FileStream CreateFileForJobRunItem(string definitionOutputPath, DateTime runStart, ScheduledJobStore.JobRunItem runItem)
		{
			if (!string.IsNullOrEmpty(definitionOutputPath))
			{
				string runFilePathNameFromPath = ScheduledJobStore.GetRunFilePathNameFromPath(definitionOutputPath, runItem, runStart);
				return File.Create(runFilePathNameFromPath);
			}
			else
			{
				throw new PSArgumentException("definitionOutputPath");
			}
		}

		private static string CreateFilePathName(string definitionName, string fileName)
		{
			string jobDefinitionPath = ScheduledJobStore.GetJobDefinitionPath(definitionName);
			string jobRunOutputDirectory = ScheduledJobStore.GetJobRunOutputDirectory(definitionName);
			if (!Directory.Exists(jobDefinitionPath))
			{
				Directory.CreateDirectory(jobDefinitionPath);
				Directory.CreateDirectory(jobRunOutputDirectory);
				object[] objArray = new object[2];
				objArray[0] = jobDefinitionPath;
				objArray[1] = fileName;
				return string.Format(CultureInfo.InstalledUICulture, (OSHelper.IsUnix ? "{0}/{1}.xml" : "{0}\\{1}.xml"), objArray);
			}
			else
			{
				ScheduledJobException scheduledJobException = new ScheduledJobException(StringUtil.Format(ScheduledJobErrorStrings.JobDefFileAlreadyExists, definitionName));
				scheduledJobException.FQEID = "ScheduledJobDefExists";
				throw scheduledJobException;
			}
		}

		private static string GetDirectoryPath()
		{
			string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ScheduledJobsPath);
			if (!Directory.Exists(str))
			{
				Directory.CreateDirectory(str);
			}
			return str;
		}

		public static FileStream GetFileForJobDefinition(string definitionName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				string filePathName = ScheduledJobStore.GetFilePathName(definitionName, "ScheduledJobDefinition");
				return File.Open(filePathName, fileMode, fileAccess, fileShare);
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static FileStream GetFileForJobDefinition(string definitionName, string definitionPath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				if (!string.IsNullOrEmpty(definitionPath))
				{
					object[] objArray = new object[3];
					objArray[0] = definitionPath;
					objArray[1] = definitionName;
					objArray[2] = "ScheduledJobDefinition";
					string str = string.Format(CultureInfo.InvariantCulture, (OSHelper.IsUnix ? "{0}/{1}/{2}.xml" : "{0}\\{1}\\{2}.xml"), objArray);
					return File.Open(str, fileMode, fileAccess, fileShare);
				}
				else
				{
					throw new PSArgumentException("definitionPath");
				}
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static FileStream GetFileForJobRunItem(string definitionName, DateTime runStart, ScheduledJobStore.JobRunItem runItem, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				string runFilePathName = ScheduledJobStore.GetRunFilePathName(definitionName, runItem, runStart);
				return File.Open(runFilePathName, fileMode, fileAccess, fileShare);
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		private static string GetFilePathName(string definitionName, string fileName)
		{
			string jobDefinitionPath = ScheduledJobStore.GetJobDefinitionPath(definitionName);
			object[] objArray = new object[2];
			objArray[0] = jobDefinitionPath;
			objArray[1] = fileName;
			return string.Format(CultureInfo.InvariantCulture, (OSHelper.IsUnix ? "{0}/{1}.xml" : "{0}\\{1}.xml"), objArray);
		}

		public static string GetJobDefinitionLocation()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ScheduledJobsPath);
		}

		private static string GetJobDefinitionPath(string definitionName)
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ScheduledJobsPath, definitionName);
		}

		public static IEnumerable<string> GetJobDefinitions()
		{
			string directoryPath = ScheduledJobStore.GetDirectoryPath();
			IEnumerable<string> strs = Directory.EnumerateDirectories(directoryPath);
			if (strs != null)
			{
				return strs;
			}
			else
			{
				return new Collection<string>();
			}
		}

		public static string GetJobRunOutputDirectory(string definitionName)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				return Path.Combine(ScheduledJobStore.GetJobDefinitionPath(definitionName), "Output");
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static Collection<DateTime> GetJobRunsForDefinition(string definitionName)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				string jobRunOutputDirectory = ScheduledJobStore.GetJobRunOutputDirectory(definitionName);
				return ScheduledJobStore.GetJobRunsForDefinitionPath(jobRunOutputDirectory);
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static Collection<DateTime> GetJobRunsForDefinitionPath(string definitionOutputPath)
		{
			DateTime dateTime;
			string str;
			if (!string.IsNullOrEmpty(definitionOutputPath))
			{
				Collection<DateTime> dateTimes = new Collection<DateTime>();
				IEnumerable<string> strs = Directory.EnumerateDirectories(definitionOutputPath);
				if (strs != null)
				{
					foreach (string str1 in strs)
					{
						int num = str1.LastIndexOf('\\');
						if (num != -1)
						{
							str = str1.Substring(num + 1);
						}
						else
						{
							str = str1;
						}
						string str2 = str;
						if (!ScheduledJobStore.ConvertJobRunNameToDateTime(str2, out dateTime))
						{
							continue;
						}
						dateTimes.Add(dateTime);
					}
				}
				return dateTimes;
			}
			else
			{
				throw new PSArgumentException("definitionOutputPath");
			}
		}

		private static string GetRunDirectory(string definitionName, DateTime runStart)
		{
			string jobRunOutputDirectory = ScheduledJobStore.GetJobRunOutputDirectory(definitionName);
			object[] jobRunName = new object[2];
			jobRunName[0] = jobRunOutputDirectory;
			jobRunName[1] = ScheduledJobStore.ConvertDateTimeToJobRunName(runStart);
			return string.Format(CultureInfo.InvariantCulture, OSHelper.IsUnix ? "{0}/{1}" : "{0}\\{1}", jobRunName);
		}

		private static string GetRunDirectoryFromPath(string definitionOutputPath, DateTime runStart)
		{
			object[] jobRunName = new object[2];
			jobRunName[0] = definitionOutputPath;
			jobRunName[1] = ScheduledJobStore.ConvertDateTimeToJobRunName(runStart);
			return string.Format(CultureInfo.InvariantCulture, OSHelper.IsUnix ? "{0}/{1}" : "{0}\\{1}", jobRunName);
		}

		private static string GetRunFilePathName(string definitionName, ScheduledJobStore.JobRunItem runItem, DateTime runStart)
		{
			string jobRunOutputDirectory = ScheduledJobStore.GetJobRunOutputDirectory(definitionName);
			object[] jobRunName = new object[2];
			jobRunName[0] = jobRunOutputDirectory;
			jobRunName[1] = ScheduledJobStore.ConvertDateTimeToJobRunName(runStart);
			string str = string.Format(CultureInfo.InvariantCulture, OSHelper.IsUnix ? "{0}/{1}" : "{0}\\{1}", jobRunName);
			object[] objArray = new object[2];
			objArray[0] = str;
			objArray[1] = runItem.ToString();
			return string.Format(CultureInfo.InvariantCulture, OSHelper.IsUnix ? "{0}/{1}.xml" : "{0}\\{1}.xml", objArray);
		}

		private static string GetRunFilePathNameFromPath(string outputPath, ScheduledJobStore.JobRunItem runItem, DateTime runStart)
		{
			object[] jobRunName = new object[2];
			jobRunName[0] = outputPath;
			jobRunName[1] = ScheduledJobStore.ConvertDateTimeToJobRunName(runStart);
			string str = string.Format(CultureInfo.InvariantCulture, OSHelper.IsUnix ? "{0}/{1}" : "{0}\\{1}", jobRunName);
			if (!Directory.Exists(str))
			{
				Directory.CreateDirectory(str);
			}
			object[] objArray = new object[2];
			objArray[0] = str;
			objArray[1] = runItem.ToString();
			return string.Format(CultureInfo.InvariantCulture, OSHelper.IsUnix ? "{0}/{1}.xml" : "{0}\\{1}.xml", objArray);
		}

		public static bool IsDefaultUserPath(string definitionPath)
		{
			return definitionPath.Equals(ScheduledJobStore.GetJobDefinitionLocation(), StringComparison.OrdinalIgnoreCase);
		}

		public static void RemoveAllJobRuns(string definitionName)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				Collection<DateTime> jobRunsForDefinition = ScheduledJobStore.GetJobRunsForDefinition(definitionName);
				foreach (DateTime dateTime in jobRunsForDefinition)
				{
					string runDirectory = ScheduledJobStore.GetRunDirectory(definitionName, dateTime);
					Directory.Delete(runDirectory, true);
				}
				return;
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static void RemoveJobDefinition(string definitionName)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				string jobDefinitionPath = ScheduledJobStore.GetJobDefinitionPath(definitionName);
				Directory.Delete(jobDefinitionPath, true);
				return;
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static void RemoveJobRun(string definitionName, DateTime runStart)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				string runDirectory = ScheduledJobStore.GetRunDirectory(definitionName, runStart);
				Directory.Delete(runDirectory, true);
				return;
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public static void RemoveJobRunFromOutputPath(string definitionOutputPath, DateTime runStart)
		{
			if (!string.IsNullOrEmpty(definitionOutputPath))
			{
				string runDirectoryFromPath = ScheduledJobStore.GetRunDirectoryFromPath(definitionOutputPath, runStart);
				Directory.Delete(runDirectoryFromPath, true);
				return;
			}
			else
			{
				throw new PSArgumentException("definitionOutputPath");
			}
		}

		public static void RenameScheduledJobDefDir(string oldDefName, string newDefName)
		{
			if (!string.IsNullOrEmpty(oldDefName))
			{
				if (!string.IsNullOrEmpty(newDefName))
				{
					string jobDefinitionPath = ScheduledJobStore.GetJobDefinitionPath(oldDefName);
					string str = ScheduledJobStore.GetJobDefinitionPath(newDefName);
					Directory.Move(jobDefinitionPath, str);
					return;
				}
				else
				{
					throw new PSArgumentException("newDefName");
				}
			}
			else
			{
				throw new PSArgumentException("oldDefName");
			}
		}

		public static void SetReadAccessOnDefinitionFile(string definitionName, string user)
		{
			string filePathName = ScheduledJobStore.GetFilePathName(definitionName, "ScheduledJobDefinition");
			FileSecurity fileSecurity = new FileSecurity(filePathName, AccessControlSections.Access);
			FileSystemAccessRule fileSystemAccessRule = new FileSystemAccessRule(user, FileSystemRights.Read, AccessControlType.Allow);
			fileSecurity.AddAccessRule(fileSystemAccessRule);
			File.SetAccessControl(filePathName, fileSecurity);
		}

		public static void SetWriteAccessOnJobRunOutput(string definitionName, string user)
		{
			string jobRunOutputDirectory = ScheduledJobStore.GetJobRunOutputDirectory(definitionName);
			ScheduledJobStore.AddFullAccessToDirectory(user, jobRunOutputDirectory);
		}

		public enum JobRunItem
		{
			None,
			Status,
			Results
		}
	}
}