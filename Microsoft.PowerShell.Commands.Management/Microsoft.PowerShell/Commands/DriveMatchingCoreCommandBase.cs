using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	public class DriveMatchingCoreCommandBase : CoreCommandBase
	{
		public DriveMatchingCoreCommandBase()
		{
		}

		internal List<PSDriveInfo> GetMatchingDrives(string driveName, string[] providerNames, string scope)
		{
			List<PSDriveInfo> pSDriveInfos = new List<PSDriveInfo>();
			if (providerNames == null || (int)providerNames.Length == 0)
			{
				string[] strArrays = new string[1];
				strArrays[0] = "*";
				providerNames = strArrays;
			}
			string[] strArrays1 = providerNames;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i];
				object[] objArray = new object[1];
				objArray[0] = str;
				CoreCommandBase.tracer.WriteLine("ProviderName: {0}", objArray);
				bool flag = string.IsNullOrEmpty(str);
				bool flag1 = WildcardPattern.ContainsWildcardCharacters(str);
				bool flag2 = string.IsNullOrEmpty(driveName);
				bool flag3 = WildcardPattern.ContainsWildcardCharacters(driveName);
				if (!flag && !flag1)
				{
					base.SessionState.Provider.Get(str);
				}
				if (!flag2 && !flag3)
				{
					if (!string.IsNullOrEmpty(scope))
					{
						base.SessionState.Drive.GetAtScope(driveName, scope);
					}
					else
					{
						base.SessionState.Drive.Get(driveName);
					}
				}
				WildcardPattern wildcardPattern = null;
				PSSnapinQualifiedName instance = null;
				if (!flag)
				{
					instance = PSSnapinQualifiedName.GetInstance(str);
					if (instance == null)
					{
						goto Label0;
					}
					wildcardPattern = new WildcardPattern(instance.ShortName, WildcardOptions.IgnoreCase);
				}
				WildcardPattern wildcardPattern1 = null;
				if (!flag2)
				{
					wildcardPattern1 = new WildcardPattern(driveName, WildcardOptions.IgnoreCase);
				}
				foreach (PSDriveInfo allAtScope in base.SessionState.Drive.GetAllAtScope(scope))
				{
					bool flag4 = flag2;
					if (!base.SuppressWildcardExpansion)
					{
						if (wildcardPattern1.IsMatch(allAtScope.Name))
						{
							flag4 = true;
						}
					}
					else
					{
						if (string.Equals(allAtScope.Name, driveName, StringComparison.OrdinalIgnoreCase))
						{
							flag4 = true;
						}
					}
					if (!flag4 || !flag && !allAtScope.Provider.IsMatch(wildcardPattern, instance))
					{
						continue;
					}
					pSDriveInfos.Add(allAtScope);
				}
            Label0:
                continue;
			}
			pSDriveInfos.Sort();
			return pSDriveInfos;
		}
	}
}