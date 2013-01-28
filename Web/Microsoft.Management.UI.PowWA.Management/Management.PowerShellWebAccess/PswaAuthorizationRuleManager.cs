using Microsoft.Management.PowerShellWebAccess.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.Management.PowerShellWebAccess
{
	public sealed class PswaAuthorizationRuleManager
	{
		private string powwaConfigFilePath;

		private string powwaAuthorizationRuleFilePath;

		private string powwaAuthorizationRuleSchemaResourcePath;

		private string powwaConfigFileResourcePath;

		private string powwaAuthorizationRuleFileResourcePath;

		internal IActiveDirectoryHelper activeDirectoryHelper;

		private readonly static PswaAuthorizationRuleManager instance;

		internal EventHandler<TestRuleInvalidRuleEventArgs> TestRuleInvalidRule;

		internal EventHandler<GetRuleByNameNotFoundEventArgs> GetRuleByNameNotFound;

		internal EventHandler<GetRuleByIdNotFoundEventArgs> GetRuleByIdNotFound;

		public static PswaAuthorizationRuleManager Instance
		{
			get
			{
				return PswaAuthorizationRuleManager.instance;
			}
		}

		static PswaAuthorizationRuleManager()
		{
			PswaAuthorizationRuleManager.instance = new PswaAuthorizationRuleManager();
		}

		private PswaAuthorizationRuleManager()
		{
			string root = "/Users/bruno/Projects/PowerShell/Web/PowershellWebAccess";
			this.powwaConfigFilePath = Environment.ExpandEnvironmentVariables(Path.Combine (root, "data/powwa.config.xml"));
			this.powwaAuthorizationRuleSchemaResourcePath = "Microsoft.Management.PowerShellWebAccess.data.AuthorizationRuleSchema.xsd";
			this.powwaConfigFileResourcePath = "Microsoft.Management.PowerShellWebAccess.data.powwa.config.xml";
			this.powwaAuthorizationRuleFileResourcePath = "Microsoft.Management.PowerShellWebAccess.data.AuthorizationRules.xml";
			this.activeDirectoryHelper = new ActiveDirectoryHelper();
		}

		internal PswaAuthorizationRule AddRule(SortedList<int, PswaAuthorizationRule> allRules, string name, string user, PswaUserType userType, string destination, PswaDestinationType destinationType, string configuration, string canonicalDestination)
		{
			string str;
			string str1;
			bool flag;
			int nextAvailableId = this.GetNextAvailableId(allRules);
			PswaAuthorizationRule pswaAuthorizationRule = new PswaAuthorizationRule();
			pswaAuthorizationRule.Id = nextAvailableId;
			PswaAuthorizationRule pswaAuthorizationRule1 = pswaAuthorizationRule;
			if (name != null)
			{
				str = name;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = nextAvailableId;
				str = string.Format(CultureInfo.CurrentCulture, Resources.AuthorizationRuleName_Format, objArray);
			}
			pswaAuthorizationRule1.RuleName = str;
			this.AddRuleSetUserInfo(pswaAuthorizationRule, user, userType);
			this.AddRuleSetDestinationInfo(pswaAuthorizationRule, destination, canonicalDestination, destinationType);
			PswaAuthorizationRule pswaAuthorizationRule2 = pswaAuthorizationRule;
			if (configuration == "*")
			{
				str1 = "*";
			}
			else
			{
				if (string.IsNullOrEmpty(configuration))
				{
					str1 = "Microsoft.PowerShell";
				}
				else
				{
					str1 = configuration;
				}
			}
			pswaAuthorizationRule2.ConfigurationName = str1;
			PswaAuthorizationRule pswaAuthorizationRule3 = pswaAuthorizationRule;
			if (destinationType == PswaDestinationType.All)
			{
				flag = false;
			}
			else
			{
				flag = destination != pswaAuthorizationRule.DestinationCanonicalForm;
			}
			pswaAuthorizationRule3.IsCanonicalDestinationSid = flag;
			allRules.Add(pswaAuthorizationRule.Id, pswaAuthorizationRule);
			return pswaAuthorizationRule;
		}

		private void AddRuleSetDestinationInfo(PswaAuthorizationRule newRule, string destination, string canonicalDestination, PswaDestinationType destinationType)
		{
			bool flag = false;
			newRule.DestinationType = destinationType;
			if (destinationType != PswaDestinationType.Computer)
			{
				if (destinationType != PswaDestinationType.ComputerGroup)
				{
					if (destinationType == PswaDestinationType.All)
					{
						newRule.Destination = "*";
						newRule.DestinationCanonicalForm = "*";
						newRule.IsComputerGroupLocal = false;
					}
					return;
				}
				else
				{
					if (this.CheckSamAccountFormat(destination))
					{
						string str = null;
						string stringSid = this.activeDirectoryHelper.ConvertAccountNameToStringSid(destination, out flag, out str);
						string str1 = null;
						if (this.activeDirectoryHelper.CheckComputerTypeMatch(flag, stringSid, destinationType, str, out str1))
						{
							newRule.Destination = destination;
							newRule.DestinationCanonicalForm = stringSid;
							newRule.IsComputerGroupLocal = flag;
							return;
						}
						else
						{
							throw new ArgumentException(str1);
						}
					}
					else
					{
						throw new ArgumentException(Resources.InvalidUserAndGroupNameFormat);
					}
				}
			}
			else
			{
				newRule.Destination = destination;
				newRule.DestinationCanonicalForm = canonicalDestination;
				newRule.IsComputerGroupLocal = false;
				return;
			}
		}

		private void AddRuleSetUserInfo(PswaAuthorizationRule newRule, string user, PswaUserType userType)
		{
			bool flag = false;
			newRule.UserType = userType;
			if (userType == PswaUserType.All)
			{
				newRule.User = "*";
				newRule.UserCanonicalForm = newRule.User;
				newRule.IsUserGroupLocal = false;
				return;
			}
			else
			{
				string str = null;
				if (this.CheckSamAccountFormat(user))
				{
					newRule.User = user;
					string stringSid = this.activeDirectoryHelper.ConvertAccountNameToStringSid(user, out flag, out str);
					newRule.IsUserGroupLocal = flag;
					string str1 = null;
					if (this.activeDirectoryHelper.CheckUserTypeMatch(newRule.IsUserGroupLocal, stringSid, userType, str, out str1))
					{
						newRule.UserCanonicalForm = stringSid;
						return;
					}
					else
					{
						throw new ArgumentException(str1);
					}
				}
				else
				{
					throw new ArgumentException(Resources.InvalidUserAndGroupNameFormat);
				}
			}
		}

		private PswaAuthorizationRule CheckAllowAllRule(PswaAuthorizationRule[] rules)
		{
			PswaAuthorizationRule[] pswaAuthorizationRuleArray = rules;
			int num = 0;
			while (num < (int)pswaAuthorizationRuleArray.Length)
			{
				PswaAuthorizationRule pswaAuthorizationRule = pswaAuthorizationRuleArray[num];
				if (!(pswaAuthorizationRule.UserCanonicalForm == "*") || !(pswaAuthorizationRule.DestinationCanonicalForm == "*") || !(pswaAuthorizationRule.ConfigurationName == "*"))
				{
					num++;
				}
				else
				{
					PswaAuthorizationRule pswaAuthorizationRule1 = pswaAuthorizationRule;
					return pswaAuthorizationRule1;
				}
			}
			return null;
		}

		private bool CheckSamAccountFormat(string accountName)
		{
			int num = accountName.IndexOf('\\');
			if (num <= 0)
			{
				return false;
			}
			else
			{
				return num < accountName.Length - 1;
			}
		}

		internal void GetComputerFqdnAndSid(string computerName, out string computerFqdn, out string computerSid)
		{
			bool flag = false;
			computerFqdn = this.activeDirectoryHelper.GetFqdn(computerName);
			computerSid = null;
			if (computerFqdn != null)
			{
				try
				{
					string str = null;
					string str1 = this.activeDirectoryHelper.ConvertComputerName(computerFqdn, true);
					computerSid = this.activeDirectoryHelper.ConvertAccountNameToStringSid(str1, out flag, out str);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!exception.Data.Contains("ErrorCode") || (int)exception.Data["ErrorCode"] != 0x534)
					{
						throw;
					}
					else
					{
						computerSid = null;
					}
				}
			}
		}

		private int GetNextAvailableId(SortedList<int, PswaAuthorizationRule> ruleList)
		{
			int num = 0;
			while (num < ruleList.Keys.Count)
			{
				if (num == ruleList.Keys[num])
				{
					num++;
				}
				else
				{
					return num;
				}
			}
			return ruleList.Keys.Count;
		}

		internal PswaAuthorizationRule[] GetRule(SortedList<int, PswaAuthorizationRule> allRules, string[] names)
		{
			ArrayList arrayLists = new ArrayList();
			string[] strArrays = names;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				Func<PswaAuthorizationRule, bool> func = null;
				int count = arrayLists.Count;
				IList<PswaAuthorizationRule> values = allRules.Values;
				if (func == null)
				{
					func = (PswaAuthorizationRule n) => string.Compare(strArrays[i], n.RuleName, StringComparison.CurrentCultureIgnoreCase) == 0;
				}
				foreach (PswaAuthorizationRule pswaAuthorizationRule in values.Where<PswaAuthorizationRule>(func))
				{
					arrayLists.Add(pswaAuthorizationRule);
				}
				if (arrayLists.Count == count)
				{
					this.OnGetRuleByNameNotFound(strArrays[i]);
				}
			}
			return (PswaAuthorizationRule[])arrayLists.ToArray(typeof(PswaAuthorizationRule));
		}

		internal PswaAuthorizationRule[] GetRule(SortedList<int, PswaAuthorizationRule> allRules, int[] id)
		{
			ArrayList arrayLists = new ArrayList();
			if (id == null)
			{
				return allRules.Values.ToArray<PswaAuthorizationRule>();
			}
			else
			{
				int[] numArray = id;
				for (int i = 0; i < (int)numArray.Length; i++)
				{
					int num = numArray[i];
					if (!allRules.ContainsKey(num))
					{
						this.OnGetRuleByIdNotFound(num);
					}
					else
					{
						arrayLists.Add(allRules[num]);
					}
				}
				return (PswaAuthorizationRule[])arrayLists.ToArray(typeof(PswaAuthorizationRule));
			}
		}

		internal bool IsCurrentComputerDomainJoined()
		{
			return this.activeDirectoryHelper.IsCurrentComputerDomainJoined();
		}

		internal bool LoadConfigFile(ArrayList errorMessageReport)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.PreserveWhitespace = false;
			if (File.Exists(this.powwaConfigFilePath))
			{
				try
				{
					this.TryLoadConfigFile(xmlDocument);
				}
				catch (XmlException xmlException1)
				{
					XmlException xmlException = xmlException1;
					object[] fileName = new object[4];
					fileName[0] = Path.GetFileName(this.powwaConfigFilePath);
					fileName[1] = xmlException.Message;
					fileName[2] = "powwa.config_corrupt.xml";
					fileName[3] = this.powwaConfigFilePath;
					errorMessageReport.Add(new DataFileLoadError(DataFileLoadError.ErrorStatus.Warning, string.Format(CultureInfo.CurrentCulture, Resources.LoadFile_ConfigFileBad, fileName)));
					try
					{
						this.RestoreConfigFile();
						this.TryLoadConfigFile(xmlDocument);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						errorMessageReport.Add(new DataFileLoadError(DataFileLoadError.ErrorStatus.Error, exception));
						bool flag = false;
						return flag;
					}
				}
				return true;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = this.powwaConfigFilePath;
				errorMessageReport.Add(new DataFileLoadError(DataFileLoadError.ErrorStatus.Error, new Exception(string.Format(CultureInfo.CurrentCulture, Resources.LoadFile_ConfigFileMissing, objArray))));
				return false;
			}
		}

		internal SortedList<int, PswaAuthorizationRule> LoadFromFile(ArrayList errorMessageReport)
		{
			if (this.LoadConfigFile(errorMessageReport))
			{
				if (!Path.IsPathRooted(this.powwaAuthorizationRuleFilePath))
				{
					this.powwaAuthorizationRuleFilePath = Path.Combine(Directory.GetParent(this.powwaConfigFilePath).FullName, this.powwaAuthorizationRuleFilePath);
				}
				XmlDocument xmlDocument = this.LoadRuleFile(errorMessageReport);
				if (xmlDocument != null)
				{
					SortedList<int, PswaAuthorizationRule> nums = this.ParseRuleList(xmlDocument, errorMessageReport);
					return nums;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		private XmlDocument LoadRuleFile(ArrayList errorMessageReport)
		{
			if (File.Exists(this.powwaAuthorizationRuleFilePath))
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
				var stream = typeof(PswaAuthorizationRule).Assembly.GetManifestResourceStream(this.powwaAuthorizationRuleSchemaResourcePath);
				using (StreamReader streamReader = new StreamReader(stream))
				{
					XmlSchema xmlSchema = XmlSchema.Read(streamReader, null);
					xmlReaderSetting.Schemas.Add(xmlSchema);
					xmlReaderSetting.ValidationType = ValidationType.Schema;
				}
				try
				{
					this.TryLoadRuleFile(xmlDocument, xmlReaderSetting);
				}
				catch (Exception exception3)
				{
					Exception exception = exception3;
					if (exception as XmlException != null || exception as XmlSchemaValidationException != null)
					{
						object[] fileName = new object[4];
						fileName[0] = Path.GetFileName(this.powwaAuthorizationRuleFilePath);
						fileName[1] = exception.Message;
						fileName[2] = "Authorization_corrupt.xml";
						fileName[3] = this.powwaAuthorizationRuleFilePath;
						errorMessageReport.Add(new DataFileLoadError(DataFileLoadError.ErrorStatus.Warning, string.Format(CultureInfo.CurrentCulture, Resources.LoadFile_RuleFileBad, fileName)));
						try
						{
							this.RestoreRuleFile();
							this.TryLoadRuleFile(xmlDocument, xmlReaderSetting);
						}
						catch (Exception exception2)
						{
							Exception exception1 = exception2;
							errorMessageReport.Add(new DataFileLoadError(DataFileLoadError.ErrorStatus.Error, exception1));
							XmlDocument xmlDocument1 = null;
							return xmlDocument1;
						}
					}
					else
					{
						throw exception;
					}
				}
				return xmlDocument;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = this.powwaAuthorizationRuleFilePath;
				errorMessageReport.Add(new DataFileLoadError(DataFileLoadError.ErrorStatus.Error, new Exception(string.Format(CultureInfo.CurrentCulture, Resources.LoadFile_RuleFileMissing, objArray))));
				return null;
			}
		}

		private void OnGetRuleByIdNotFound(int id)
		{
			EventHandler<GetRuleByIdNotFoundEventArgs> getRuleByIdNotFound = this.GetRuleByIdNotFound;
			if (getRuleByIdNotFound != null)
			{
				getRuleByIdNotFound(this, new GetRuleByIdNotFoundEventArgs(id));
			}
		}

		private void OnGetRuleByNameNotFound(string name)
		{
			EventHandler<GetRuleByNameNotFoundEventArgs> getRuleByNameNotFound = this.GetRuleByNameNotFound;
			if (getRuleByNameNotFound != null)
			{
				getRuleByNameNotFound(this, new GetRuleByNameNotFoundEventArgs(name));
			}
		}

		private void OnTestRuleInvalidRule(PswaAuthorizationRule rule, Exception exception)
		{
			EventHandler<TestRuleInvalidRuleEventArgs> testRuleInvalidRule = this.TestRuleInvalidRule;
			if (testRuleInvalidRule != null)
			{
				testRuleInvalidRule(this, new TestRuleInvalidRuleEventArgs(rule, exception));
			}
		}

		private void OnTestRuleRuleMatch(PswaAuthorizationRule rule)
		{
			EventHandler<TestRuleRuleMatchEventArgs> eventHandler = this.TestRuleRuleMatch;
			if (eventHandler != null)
			{
				eventHandler(this, new TestRuleRuleMatchEventArgs(rule));
			}
		}

		private SortedList<int, PswaAuthorizationRule> ParseRuleList(XmlDocument doc, ArrayList errorMessageReport)
		{
			SortedList<int, PswaAuthorizationRule> nums;
			XmlNodeList elementsByTagName = doc.GetElementsByTagName("Rule");
			SortedList<int, PswaAuthorizationRule> nums1 = new SortedList<int, PswaAuthorizationRule>();
			try
			{
				foreach (XmlElement xmlElement in elementsByTagName)
				{
					PswaAuthorizationRule pswaAuthorizationRule = new PswaAuthorizationRule();
					pswaAuthorizationRule.DeserializeFromXmlElement(xmlElement);
					int id = pswaAuthorizationRule.Id;
					nums1.Add(id, pswaAuthorizationRule);
				}
				return nums1;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				errorMessageReport.Add(new DataFileLoadError(DataFileLoadError.ErrorStatus.Error, exception));
				nums = null;
			}
			return nums;
		}

		internal PswaAuthorizationRule RemoveRule(SortedList<int, PswaAuthorizationRule> allRules, int id)
		{
			PswaAuthorizationRule item = null;
			if (allRules.ContainsKey(id))
			{
				item = allRules[id];
				allRules.Remove(id);
			}
			return item;
		}

		private void RestoreConfigFile()
		{
			if (File.Exists(this.powwaConfigFilePath))
			{
				string str = string.Concat(this.powwaConfigFilePath, "\\..\\powwa.config_corrupt.xml");
				File.WriteAllText(str, File.ReadAllText(this.powwaConfigFilePath));
				this.RestrictAdminAccess(str);
			}
			using (StreamReader streamReader = new StreamReader(typeof(PswaAuthorizationRule).Assembly.GetManifestResourceStream(this.powwaConfigFileResourcePath)))
			{
				File.WriteAllText(this.powwaConfigFilePath, streamReader.ReadToEnd());
			}
		}

		private void RestoreRuleFile()
		{
			if (File.Exists(this.powwaAuthorizationRuleFilePath))
			{
				string str = string.Concat(this.powwaAuthorizationRuleFilePath, "\\..\\AuthorizationRules_corrupt.xml");
				File.WriteAllText(str, File.ReadAllText(this.powwaAuthorizationRuleFilePath));
				this.RestrictAdminAccess(str);
			}
			using (StreamReader streamReader = new StreamReader(typeof(PswaAuthorizationRule).Assembly.GetManifestResourceStream(this.powwaAuthorizationRuleFileResourcePath)))
			{
				File.WriteAllText(this.powwaAuthorizationRuleFilePath, streamReader.ReadToEnd());
			}
		}

		private void RestrictAdminAccess(string path)
		{
			FileSecurity fileSecurity = new FileSecurity();
			fileSecurity.SetAccessRuleProtection(true, false);
			SecurityIdentifier securityIdentifier = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
			FileSystemRights fileSystemRight = FileSystemRights.FullControl;
			AccessControlType accessControlType = AccessControlType.Allow;
			FileSystemAccessRule fileSystemAccessRule = new FileSystemAccessRule(securityIdentifier, fileSystemRight, accessControlType);
			fileSecurity.AddAccessRule(fileSystemAccessRule);
			File.SetAccessControl(path, fileSecurity);
		}

		internal void SaveToFile(SortedList<int, PswaAuthorizationRule> ruleList)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<Rules />");
			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", null, null);
			xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);
			XmlComment xmlComment = xmlDocument.CreateComment(Resources.AuthorizationRuleFile_WarningComment);
			xmlDocument.InsertBefore(xmlComment, xmlDocument.DocumentElement);
			foreach (PswaAuthorizationRule value in ruleList.Values)
			{
				XmlNode xmlNodes = xmlDocument.ImportNode(value.SerializeToXmlElement(), true);
				xmlDocument.DocumentElement.AppendChild(xmlNodes);
			}
			try
			{
				xmlDocument.Save(this.powwaAuthorizationRuleFilePath);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				throw new Exception(Resources.SaveFileFailed, exception);
			}
		}

		internal void SetFilePath(string configPath)
		{
			this.powwaConfigFilePath = configPath;
		}

		private PswaAuthorizationRule[] TestDomainGroupRule(IEnumerable rules, string userCanonicalForm, string computerCanonicalForm, string configurationName, List<string> userDomainGroupSid, List<string> computerDomainGroupSid, MatchingWildcard parts)
		{
			ArrayList arrayLists = new ArrayList();
			foreach (PswaAuthorizationRule rule in rules)
			{
				try
				{
					bool flag = false;
					bool flag1 = false;
					bool flag2 = false;
					if (parts.HasFlag(MatchingWildcard.User) || rule.UserType == PswaUserType.All || rule.UserType == PswaUserType.User && string.Compare(userCanonicalForm, rule.UserCanonicalForm, StringComparison.OrdinalIgnoreCase) == 0 || rule.UserType == PswaUserType.UserGroup && userDomainGroupSid.Contains<string>(rule.UserCanonicalForm, StringComparer.OrdinalIgnoreCase))
					{
						flag = true;
					}
					if (parts.HasFlag(MatchingWildcard.Destination) || rule.DestinationType == PswaDestinationType.All || rule.DestinationType == PswaDestinationType.Computer && string.Compare(computerCanonicalForm, rule.DestinationCanonicalForm, StringComparison.OrdinalIgnoreCase) == 0 || rule.DestinationType == PswaDestinationType.ComputerGroup && computerDomainGroupSid.Contains<string>(rule.DestinationCanonicalForm, StringComparer.OrdinalIgnoreCase))
					{
						flag1 = true;
					}
					if (parts.HasFlag(MatchingWildcard.Configuration) || rule.ConfigurationName == "*" || string.Compare(rule.ConfigurationName, configurationName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag2 = true;
					}
					if (flag && flag1 && flag2)
					{
						arrayLists.Add(rule);
						this.OnTestRuleRuleMatch(rule);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.OnTestRuleInvalidRule(rule, exception);
				}
			}
			return (PswaAuthorizationRule[])arrayLists.ToArray(typeof(PswaAuthorizationRule));
		}

		private PswaAuthorizationRule[] TestLocalGroupRule(IEnumerable rules, string userCanonicalForm, string computerCanonicalForm, string configurationName, List<string> userDomainGroupSid, List<string> computerDomainGroupSid, bool returnAllMatches, MatchingWildcard parts)
		{
			ArrayList arrayLists = new ArrayList();
			Dictionary<string, string> strs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, string> strs1 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (PswaAuthorizationRule rule in rules)
			{
				try
				{
					bool flag = false;
					bool flag1 = false;
					bool flag2 = false;
					if (parts.HasFlag(MatchingWildcard.User) || rule.UserType == PswaUserType.All || rule.UserType == PswaUserType.User && userCanonicalForm == rule.UserCanonicalForm || rule.UserType == PswaUserType.UserGroup && !rule.IsUserGroupLocal && userDomainGroupSid.Contains<string>(rule.UserCanonicalForm, StringComparer.OrdinalIgnoreCase) || rule.UserType == PswaUserType.UserGroup && rule.IsUserGroupLocal && this.activeDirectoryHelper.IsAccountInGroup(rule.UserCanonicalForm, userDomainGroupSid, userCanonicalForm, strs))
					{
						flag = true;
					}
					if (parts.HasFlag(MatchingWildcard.Destination) || rule.DestinationType == PswaDestinationType.All || rule.DestinationType == PswaDestinationType.Computer && computerCanonicalForm == rule.DestinationCanonicalForm || rule.DestinationType == PswaDestinationType.ComputerGroup && !rule.IsComputerGroupLocal && computerDomainGroupSid.Contains<string>(rule.DestinationCanonicalForm, StringComparer.OrdinalIgnoreCase) || rule.DestinationType == PswaDestinationType.ComputerGroup && rule.IsComputerGroupLocal && this.activeDirectoryHelper.IsAccountInGroup(rule.DestinationCanonicalForm, computerDomainGroupSid, computerCanonicalForm, strs1))
					{
						flag1 = true;
					}
					if (parts.HasFlag(MatchingWildcard.Configuration) || rule.ConfigurationName == "*" || string.Compare(rule.ConfigurationName, configurationName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag2 = true;
					}
					if (flag && flag1 && flag2)
					{
						arrayLists.Add(rule);
						this.OnTestRuleRuleMatch(rule);
						if (!returnAllMatches)
						{
							break;
						}
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.OnTestRuleInvalidRule(rule, exception);
				}
			}
			return (PswaAuthorizationRule[])arrayLists.ToArray(typeof(PswaAuthorizationRule));
		}

		private PswaAuthorizationRule[] TestNonGroupRule(IEnumerable rules, string userCanonicalForm, string computerCanonicalForm, string configurationName, MatchingWildcard parts)
		{
			ArrayList arrayLists = new ArrayList();
			foreach (PswaAuthorizationRule rule in rules)
			{
				try
				{
					bool flag = false;
					bool flag1 = false;
					bool flag2 = false;
					if (parts.HasFlag(MatchingWildcard.User) || rule.UserType == PswaUserType.All || string.Compare(userCanonicalForm, rule.UserCanonicalForm, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag = true;
					}
					if (parts.HasFlag(MatchingWildcard.Destination) || rule.DestinationType == PswaDestinationType.All || string.Compare(computerCanonicalForm, rule.DestinationCanonicalForm, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag1 = true;
					}
					if (parts.HasFlag(MatchingWildcard.Configuration) || rule.ConfigurationName == "*" || string.Compare(rule.ConfigurationName, configurationName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag2 = true;
					}
					if (flag && flag1 && flag2)
					{
						arrayLists.Add(rule);
						this.OnTestRuleRuleMatch(rule);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.OnTestRuleInvalidRule(rule, exception);
				}
			}
			return (PswaAuthorizationRule[])arrayLists.ToArray(typeof(PswaAuthorizationRule));
		}

		internal PswaAuthorizationRule[] TestRule(PswaAuthorizationRule[] rules, string user, Uri connectionUri, string configuration, bool returnAllMatches, MatchingWildcard wildcardParts = 0)
		{
			return this.TestRule(rules, user, connectionUri.Host, configuration, returnAllMatches, wildcardParts);
		}

		internal PswaAuthorizationRule[] TestRule(PswaAuthorizationRule[] rules, string user, string computer, string configuration, bool returnAllMatches, MatchingWildcard wildcardParts = 0)
		{
			string stringSid;
			string str;
			ArrayList arrayLists = new ArrayList();
			PswaAuthorizationRule[] pswaAuthorizationRuleArray = rules;
			if (!returnAllMatches)
			{
				PswaAuthorizationRule pswaAuthorizationRule = this.CheckAllowAllRule(pswaAuthorizationRuleArray);
				if (pswaAuthorizationRule != null)
				{
					PswaAuthorizationRule[] pswaAuthorizationRuleArray1 = new PswaAuthorizationRule[1];
					pswaAuthorizationRuleArray1[0] = pswaAuthorizationRule;
					return pswaAuthorizationRuleArray1;
				}
			}
			string str1 = null;
			bool flag = false;
			if (wildcardParts.HasFlag(MatchingWildcard.User))
			{
				stringSid = "*";
			}
			else
			{
				stringSid = this.activeDirectoryHelper.ConvertAccountNameToStringSid(user, out flag, out str1);
			}
			string str2 = stringSid;
			bool flag1 = true;
			string stringSid1 = computer;
			if (!wildcardParts.HasFlag(MatchingWildcard.Destination))
			{
				string str3 = this.TryParseDestinationIpAddress(stringSid1);
				if (str3 == null)
				{
					if (this.activeDirectoryHelper.IsCurrentComputerDomainJoined())
					{
						try
						{
							string str4 = this.activeDirectoryHelper.ConvertComputerName(computer, false);
							stringSid1 = this.activeDirectoryHelper.ConvertAccountNameToStringSid(str4, out flag1, out str1);
						}
						catch (Exception exception)
						{
						}
					}
				}
				else
				{
					stringSid1 = str3;
				}
			}
			if (string.IsNullOrEmpty(configuration))
			{
				str = "Microsoft.PowerShell";
			}
			else
			{
				str = configuration;
			}
			string str5 = str;
			ArrayList arrayLists1 = new ArrayList();
			ArrayList arrayLists2 = new ArrayList();
			ArrayList arrayLists3 = new ArrayList();
			PswaAuthorizationRule[] pswaAuthorizationRuleArray2 = pswaAuthorizationRuleArray;
			for (int i = 0; i < (int)pswaAuthorizationRuleArray2.Length; i++)
			{
				PswaAuthorizationRule pswaAuthorizationRule1 = pswaAuthorizationRuleArray2[i];
				if (pswaAuthorizationRule1.UserType == PswaUserType.UserGroup || pswaAuthorizationRule1.DestinationType == PswaDestinationType.ComputerGroup)
				{
					if (pswaAuthorizationRule1.IsUserGroupLocal || pswaAuthorizationRule1.IsComputerGroupLocal)
					{
						arrayLists3.Add(pswaAuthorizationRule1);
					}
					else
					{
						arrayLists1.Add(pswaAuthorizationRule1);
					}
				}
				else
				{
					arrayLists2.Add(pswaAuthorizationRule1);
				}
			}
			PswaAuthorizationRule[] pswaAuthorizationRuleArray3 = this.TestNonGroupRule(arrayLists2, str2, stringSid1, str5, wildcardParts);
			if (!returnAllMatches)
			{
				if ((int)pswaAuthorizationRuleArray3.Length > 0)
				{
					return pswaAuthorizationRuleArray3;
				}
			}
			else
			{
				arrayLists.AddRange(pswaAuthorizationRuleArray3);
			}
			List<string> strs = new List<string>();
			List<string> accountDomainGroupSid = new List<string>();
			try
			{
				if (!flag && !wildcardParts.HasFlag(MatchingWildcard.User))
				{
					strs = this.activeDirectoryHelper.GetAccountDomainGroupSid(str2);
				}
				if (!flag1 && !wildcardParts.HasFlag(MatchingWildcard.Destination))
				{
					accountDomainGroupSid = this.activeDirectoryHelper.GetAccountDomainGroupSid(stringSid1);
				}
			}
			catch (ArgumentException argumentException)
			{
			}
			PswaAuthorizationRule[] pswaAuthorizationRuleArray4 = this.TestDomainGroupRule(arrayLists1, str2, stringSid1, str5, strs, accountDomainGroupSid, wildcardParts);
			if (!returnAllMatches)
			{
				if ((int)pswaAuthorizationRuleArray4.Length > 0)
				{
					return pswaAuthorizationRuleArray4;
				}
			}
			else
			{
				arrayLists.AddRange(pswaAuthorizationRuleArray4);
			}
			PswaAuthorizationRule[] pswaAuthorizationRuleArray5 = this.TestLocalGroupRule(arrayLists3, str2, stringSid1, str5, strs, accountDomainGroupSid, returnAllMatches, wildcardParts);
			if (!returnAllMatches)
			{
				if ((int)pswaAuthorizationRuleArray5.Length > 0)
				{
					return pswaAuthorizationRuleArray5;
				}
			}
			else
			{
				arrayLists.AddRange(pswaAuthorizationRuleArray5);
			}
			return (PswaAuthorizationRule[])arrayLists.ToArray(typeof(PswaAuthorizationRule));
		}

		private void TryLoadConfigFile(XmlDocument configDoc)
		{
			configDoc.Load(this.powwaConfigFilePath);
			XmlNodeList elementsByTagName = configDoc.GetElementsByTagName("AuthorizationRuleFilePath");
			if (elementsByTagName.Count == 0 || string.IsNullOrEmpty(elementsByTagName[0].InnerText.Trim()))
			{
				object[] objArray = new object[2];
				objArray[0] = "AuthorizationRuleFilePath";
				objArray[1] = this.powwaConfigFilePath;
				throw new XmlException(string.Format(CultureInfo.CurrentCulture, Resources.LoadFile_ConfigFileMissingAuthorizationFile, objArray));
			}
			else
			{
				this.powwaAuthorizationRuleFilePath = Environment.ExpandEnvironmentVariables(elementsByTagName[0].InnerText.Trim());
				try
				{
					Path.GetFileName(this.powwaAuthorizationRuleFilePath);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					throw new XmlException(argumentException.Message);
				}
				return;
			}
		}

		private void TryLoadRuleFile(XmlDocument doc, XmlReaderSettings settings)
		{
			XmlReader xmlReader = XmlReader.Create(this.powwaAuthorizationRuleFilePath, settings);
			using (xmlReader)
			{
				doc.Load(xmlReader);
			}
		}

		internal string TryParseDestinationIpAddress(string destination)
		{
			IPAddress pAddress = null;
			if (!IPAddress.TryParse(destination, out pAddress))
			{
				return null;
			}
			else
			{
				return pAddress.ToString();
			}
		}

		internal event EventHandler<TestRuleRuleMatchEventArgs> TestRuleRuleMatch;
	}
}