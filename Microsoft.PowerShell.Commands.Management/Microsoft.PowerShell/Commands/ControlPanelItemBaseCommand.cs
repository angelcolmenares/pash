using Microsoft.PowerShell.Commands.Management;
using Microsoft.Win32;
using Shell32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	public abstract class ControlPanelItemBaseCommand : PSCmdlet
	{
		private const string RegionCanonicalName = "Microsoft.RegionAndLanguage";

		private const string ControlPanelShellFolder = "shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}";

		private const string TestHeadlessServerScript = "\r\n$result = $false\r\n$serverManagerModule = Get-Module -ListAvailable | ? {$_.Name -eq 'ServerManager'}\r\nif ($serverManagerModule -ne $null)\r\n{\r\n    Import-Module ServerManager\r\n    $Gui = (Get-WindowsFeature Server-Gui-Shell).Installed\r\n    if ($Gui -eq $false)\r\n    {\r\n        $result = $true\r\n    }\r\n}\r\n$result\r\n";

		private static string VerbActionOpenName;

		private readonly static string[] ControlPanelItemFilterList;

		internal readonly Dictionary<string, string> CategoryMap;

		internal string[] CategoryNames;

		internal string[] RegularNames;

		internal string[] CanonicalNames;

		internal ControlPanelItem[] ControlPanelItems;

		private List<ShellFolderItem> _allControlPanelItems;

		internal static Shell CreateShellImpl ()
		{
			if (OSHelper.IsWindows)
				return (Shell)Activator.CreateInstance (Type.GetTypeFromCLSID (new Guid ("13709620-C279-11CE-A49E-444553540000")));
			if (OSHelper.IsMacOSX) {
				return new OSXShell();
			}
			throw new NotSupportedException("Linux Configuration Shell is not implemented");
		}

		internal List<ShellFolderItem> AllControlPanelItems
		{
			get
			{
				if (this._allControlPanelItems == null)
				{
					this._allControlPanelItems = new List<ShellFolderItem>();
					string str = "shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0";
					IShellDispatch4 variable = CreateShellImpl ();
					Folder2 variable1 = (Folder2)variable.NameSpace(str);
					FolderItems3 variable2 = (FolderItems3)variable1.Items();
					bool flag = this.IsServerCoreOrHeadLessServer();
					foreach (ShellFolderItem variable3 in variable2)
					{
						if (flag)
						{
							bool flag1 = false;
							string[] controlPanelItemFilterList = ControlPanelItemBaseCommand.ControlPanelItemFilterList;
							int num = 0;
							while (num < (int)controlPanelItemFilterList.Length)
							{
								string str1 = controlPanelItemFilterList[num];
								if (!str1.Equals(variable3.Name, StringComparison.OrdinalIgnoreCase))
								{
									num++;
								}
								else
								{
									flag1 = true;
									break;
								}
							}
							if (flag1)
							{
								continue;
							}
						}
						if (!this.ContainVerbOpen(variable3))
						{
							continue;
						}
						this._allControlPanelItems.Add(variable3);
					}
				}
				return this._allControlPanelItems;
			}
		}

		static ControlPanelItemBaseCommand()
		{
			ControlPanelItemBaseCommand.VerbActionOpenName = null;
			string[] strArrays = new string[2];
			strArrays[0] = "Folder Options";
			strArrays[1] = "Taskbar and Start Menu";
			ControlPanelItemBaseCommand.ControlPanelItemFilterList = strArrays;
		}

		protected ControlPanelItemBaseCommand()
		{
			this.CategoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string[] strArrays = new string[1];
			strArrays[0] = "*";
			this.CategoryNames = strArrays;
			string[] strArrays1 = new string[1];
			strArrays1[0] = "*";
			this.RegularNames = strArrays1;
			string[] strArrays2 = new string[1];
			strArrays2[0] = "*";
			this.CanonicalNames = strArrays2;
			this.ControlPanelItems = new ControlPanelItem[0];
		}

		protected override void BeginProcessing()
		{
			OperatingSystem oSVersion = Environment.OSVersion;
			PlatformID platform = oSVersion.Platform;
			Version version = oSVersion.Version;
			if (!platform.Equals(PlatformID.Win32NT) || version.Major >= 6 && (version.Major != 6 || version.Minor >= 2))
			{
				return;
			}
			else
			{
				object[] name = new object[1];
				name[0] = base.CommandInfo.Name;
				string str = string.Format(CultureInfo.InvariantCulture, ControlPanelResources.ControlPanelItemCmdletNotSupported, name);
				throw new PSNotSupportedException(str);
			}
		}

		private static bool CompareVerbActionOpen(string verbActionName)
		{
			string str;
			bool flag = false;
			if (!string.IsNullOrEmpty(verbActionName))
			{
				if (ControlPanelItemBaseCommand.VerbActionOpenName == null)
				{
					new List<ShellFolderItem>();
					string str1 = "shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0";
					IShellDispatch4 variable = CreateShellImpl ();
					Folder2 variable1 = (Folder2)variable.NameSpace(str1);
					FolderItems3 variable2 = (FolderItems3)variable1.Items();
					foreach (ShellFolderItem variable3 in variable2)
					{
						string str2 = (string)((dynamic)variable3.ExtendedProperty("System.ApplicationName"));
						if (str2 != null)
						{
							str = str2.Substring(0, str2.IndexOf("\0", StringComparison.OrdinalIgnoreCase));
						}
						else
						{
							str = null;
						}
						str2 = str;
						if (str2 == null || !str2.Equals("Microsoft.RegionAndLanguage", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						ControlPanelItemBaseCommand.VerbActionOpenName = verbActionName;
						break;
					}
				}
				flag = ControlPanelItemBaseCommand.VerbActionOpenName.Equals(verbActionName, StringComparison.OrdinalIgnoreCase);
			}
			return flag;
		}

		private bool ContainVerbOpen(ShellFolderItem item)
		{
			bool flag = false;
			FolderItemVerbs variable = item.Verbs();
			foreach (FolderItemVerb variable1 in variable)
			{
				if (variable1.Name == null || !variable1.Name.Equals(ControlPanelResources.VerbActionOpen, StringComparison.OrdinalIgnoreCase) && !ControlPanelItemBaseCommand.CompareVerbActionOpen(variable1.Name))
				{
					continue;
				}
				flag = true;
				break;
			}
			return flag;
		}

		internal void GetCategoryMap()
		{
			if (this.CategoryMap.Count == 0)
			{
				IShellDispatch4 variable = CreateShellImpl();
				Folder2 variable1 = (Folder2)variable.NameSpace("shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}");
				FolderItems3 variable2 = (FolderItems3)variable1.Items();
				foreach (ShellFolderItem variable3 in variable2)
				{
					string path = variable3.Path;
					string str = path.Substring(path.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
					this.CategoryMap.Add(str, variable3.Name);
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal List<ShellFolderItem> GetControlPanelItemByCanonicalName(List<ShellFolderItem> controlPanelItems, bool withCategoryFilter)
		{
			bool flag = false;
			bool flag1 = false;
			string str;
			string noControlPanelItemFoundForGivenCanonicalNameWithCategory;
			string noControlPanelItemFoundWithNullCanonicalNameWithCategory;
			List<ShellFolderItem> collection = new List<ShellFolderItem>();
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (this.CanonicalNames != null)
			{
				string[] canonicalNames = this.CanonicalNames;
				for (int i = 0; i < (int)canonicalNames.Length; i++)
				{
					string str1 = canonicalNames[i];
					WildcardPattern wildcardPattern = new WildcardPattern(str1, WildcardOptions.IgnoreCase);
					foreach (ShellFolderItem controlPanelItem in controlPanelItems)
					{
						string path = controlPanelItem.Path;
						string str2 = (string)((dynamic)controlPanelItem.ExtendedProperty("System.ApplicationName"));
						if (str2 != null)
						{
							str = str2.Substring(0, str2.IndexOf("\0", StringComparison.OrdinalIgnoreCase));
						}
						else
						{
							str = null;
						}
						str2 = str;
						if (str2 != null)
						{
							if (!wildcardPattern.IsMatch(str2))
							{
								continue;
							}
							if (!strs.Contains(path))
							{
								flag1 = true;
								strs.Add(path);
								collection.Add(controlPanelItem);
							}
							else
							{
								flag1 = true;
							}
						}
						else
						{
							if (!str1.Equals("*", StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
							flag1 = true;
							if (strs.Contains(path))
							{
								continue;
							}
							strs.Add(path);
							collection.Add(controlPanelItem);
						}
					}
					if (!flag1 && !WildcardPattern.ContainsWildcardCharacters(str1))
					{
						if (withCategoryFilter)
						{
							noControlPanelItemFoundForGivenCanonicalNameWithCategory = ControlPanelResources.NoControlPanelItemFoundForGivenCanonicalNameWithCategory;
						}
						else
						{
							noControlPanelItemFoundForGivenCanonicalNameWithCategory = ControlPanelResources.NoControlPanelItemFoundForGivenCanonicalName;
						}
						string str3 = noControlPanelItemFoundForGivenCanonicalNameWithCategory;
						string str4 = StringUtil.Format(str3, str1);
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str4), "NoControlPanelItemFoundForGivenCanonicalName", ErrorCategory.InvalidArgument, str1);
						base.WriteError(errorRecord);
					}
				}
				return collection;
			}
			else
			{
				foreach (ShellFolderItem variable in controlPanelItems)
				{
					string str5 = (string)((dynamic)variable.ExtendedProperty("System.ApplicationName"));
					if (str5 != null)
					{
						continue;
					}
					flag = true;
					collection.Add(variable);
				}
				if (!flag)
				{
					if (withCategoryFilter)
					{
						noControlPanelItemFoundWithNullCanonicalNameWithCategory = ControlPanelResources.NoControlPanelItemFoundWithNullCanonicalNameWithCategory;
					}
					else
					{
						noControlPanelItemFoundWithNullCanonicalNameWithCategory = ControlPanelResources.NoControlPanelItemFoundWithNullCanonicalName;
					}
					string str6 = noControlPanelItemFoundWithNullCanonicalNameWithCategory;
					ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str6), "", ErrorCategory.InvalidArgument, this.CanonicalNames);
					base.WriteError(errorRecord1);
				}
				return collection;
			}
		}

		internal List<ShellFolderItem> GetControlPanelItemByCategory(List<ShellFolderItem> controlPanelItems)
		{
			List<ShellFolderItem> collection = new List<ShellFolderItem>();
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string[] categoryNames = this.CategoryNames;
			for (int i = 0; i < (int)categoryNames.Length; i++)
			{
				string str = categoryNames[i];
				bool flag = false;
				WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
			Label0:
				foreach (ShellFolderItem controlPanelItem in controlPanelItems)
				{
					string path = controlPanelItem.Path;
					int[] numArray = (int[])((dynamic)controlPanelItem.ExtendedProperty("System.ControlPanel.Category"));
					int[] numArray1 = numArray;
					int num = 0;
					while (num < (int)numArray1.Length)
					{
						int num1 = numArray1[num];
						string str1 = (string)LanguagePrimitives.ConvertTo(num1, typeof(string), CultureInfo.InvariantCulture);
						string item = this.CategoryMap[str1];
						if (!wildcardPattern.IsMatch(item))
						{
							num++;
						}
						else
						{
							if (!strs.Contains(path))
							{
								flag = true;
								strs.Add(path);
								collection.Add(controlPanelItem);
								goto Label0;
							}
							else
							{
								flag = true;
								goto Label0;
							}
						}
					}
				}
				if (!flag && !WildcardPattern.ContainsWildcardCharacters(str))
				{
					string str2 = StringUtil.Format(ControlPanelResources.NoControlPanelItemFoundForGivenCategory, str);
					ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str2), "NoControlPanelItemFoundForGivenCategory", ErrorCategory.InvalidArgument, str);
					base.WriteError(errorRecord);
				}
			}
			return collection;
		}

		internal List<ShellFolderItem> GetControlPanelItemByName(List<ShellFolderItem> controlPanelItems, bool withCategoryFilter)
		{
			bool flag = false;
			string noControlPanelItemFoundForGivenNameWithCategory;
			List<ShellFolderItem> collection = new List<ShellFolderItem>();
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string[] regularNames = this.RegularNames;
			for (int i = 0; i < (int)regularNames.Length; i++)
			{
				string str = regularNames[i];
				WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
				foreach (ShellFolderItem controlPanelItem in controlPanelItems)
				{
					string name = controlPanelItem.Name;
					string path = controlPanelItem.Path;
					if (!wildcardPattern.IsMatch(name))
					{
						continue;
					}
					if (!strs.Contains(path))
					{
						flag = true;
						strs.Add(path);
						collection.Add(controlPanelItem);
					}
					else
					{
						flag = true;
					}
				}
				if (!flag && !WildcardPattern.ContainsWildcardCharacters(str))
				{
					if (withCategoryFilter)
					{
						noControlPanelItemFoundForGivenNameWithCategory = ControlPanelResources.NoControlPanelItemFoundForGivenNameWithCategory;
					}
					else
					{
						noControlPanelItemFoundForGivenNameWithCategory = ControlPanelResources.NoControlPanelItemFoundForGivenName;
					}
					string str1 = noControlPanelItemFoundForGivenNameWithCategory;
					string str2 = StringUtil.Format(str1, str);
					ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str2), "NoControlPanelItemFoundForGivenName", ErrorCategory.InvalidArgument, str);
					base.WriteError(errorRecord);
				}
			}
			return collection;
		}

		internal List<ShellFolderItem> GetControlPanelItemsByInstance(List<ShellFolderItem> controlPanelItems)
		{
			bool flag = false;
			List<ShellFolderItem> collection = new List<ShellFolderItem>();
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			ControlPanelItem[] controlPanelItemArray = this.ControlPanelItems;
			for (int i = 0; i < (int)controlPanelItemArray.Length; i++)
			{
				ControlPanelItem controlPanelItem = controlPanelItemArray[i];
				foreach (ShellFolderItem variable in controlPanelItems)
				{
					string path = variable.Path;
					if (!controlPanelItem.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					if (!strs.Contains(path))
					{
						flag = true;
						strs.Add(path);
						collection.Add(variable);
						break;
					}
					else
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					string str = StringUtil.Format(ControlPanelResources.NoControlPanelItemFoundForGivenInstance, controlPanelItem.GetType().Name);
					ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "NoControlPanelItemFoundForGivenInstance", ErrorCategory.InvalidArgument, controlPanelItem);
					base.WriteError(errorRecord);
				}
			}
			return collection;
		}

		private bool IsServerCoreOrHeadLessServer ()
		{
			bool flag = false;
			if (OSHelper.IsWindows) {
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey ("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
				using (registryKey) {
					string value = (string)registryKey.GetValue ("InstallationType", "");
					if (!value.Equals ("Server Core")) {
						if (value.Equals ("Server")) {
							System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create ();
							using (powerShell) {
								powerShell.AddScript ("\r\n$result = $false\r\n$serverManagerModule = Get-Module -ListAvailable | ? {$_.Name -eq 'ServerManager'}\r\nif ($serverManagerModule -ne $null)\r\n{\r\n    Import-Module ServerManager\r\n    $Gui = (Get-WindowsFeature Server-Gui-Shell).Installed\r\n    if ($Gui -eq $false)\r\n    {\r\n        $result = $true\r\n    }\r\n}\r\n$result\r\n");
								Collection<PSObject> pSObjects = powerShell.Invoke (new object[0]);
								if (LanguagePrimitives.IsTrue (PSObject.Base (pSObjects [0]))) {
									flag = true;
								}
							}
						}
					} else {
						flag = true;
					}
				}
			}
			else { flag = !OSHelper.IsMacOSX; }
			return flag;
		}
	}
}