using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ProviderUtils
	{
		private const string _debugCategory = "ProviderUtils";

		internal const string ADProviderName = "ActiveDirectory";

		public ProviderUtils()
		{
		}

		internal static string GetCurrentDriveLocation(SessionState sessionState, ADSessionInfo sessionInfo)
		{
			string str = null;
			ADDriveInfo current = sessionState.Drive.Current as ADDriveInfo;
			if (current != null)
			{
				string currentLocation = current.CurrentLocation;
				str = ADPathModule.MakePath(current.RootWithoutAbsolutePathToken, currentLocation, current.FormatType);
				if (current.FormatType == ADPathFormat.Canonical && str != string.Empty)
				{
					if (sessionInfo.Server != null)
					{
						//TODO: Review sessionInfo.Server;
					}
					str = ADPathModule.ConvertPath(sessionInfo, str, ADPathFormat.Canonical, ADPathFormat.X500);
				}
			}
			return str;
		}

		internal static ADSessionInfo GetCurrentDriveSessionInfo(SessionState sessionState)
		{
			ADSessionInfo aDSessionInfo = null;
			ADDriveInfo current = sessionState.Drive.Current as ADDriveInfo;
			if (current != null)
			{
				aDSessionInfo = current.SessionInfo.Copy();
			}
			return aDSessionInfo;
		}

		internal static string GetCurrentPartitionPath(SessionState sessionState)
		{
			ADRootDSE rootDSE;
			string str = null;
			ADDriveInfo current = sessionState.Drive.Current as ADDriveInfo;
			if (current != null)
			{
				using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(current.SessionInfo))
				{
					rootDSE = aDObjectSearcher.GetRootDSE();
					rootDSE.SessionInfo = current.SessionInfo;
				}
				string currentDriveLocation = ProviderUtils.GetCurrentDriveLocation(sessionState, current.SessionInfo);
				if (currentDriveLocation != string.Empty)
				{
					try
					{
						str = ADForestPartitionInfo.ExtractAndValidatePartitionInfo(rootDSE, currentDriveLocation);
					}
					catch (ArgumentException argumentException1)
					{
						ArgumentException argumentException = argumentException1;
						object[] objArray = new object[1];
						objArray[0] = currentDriveLocation;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ProviderUtilInvalidDrivePath, objArray), argumentException);
					}
				}
				else
				{
					return string.Empty;
				}
			}
			return str;
		}

		internal static bool IsCurrentDriveAD(SessionState sessionState)
		{
			PSDriveInfo current = sessionState.Drive.Current;
			ProviderInfo provider = current.Provider;
			bool flag = string.Compare(provider.Name, "ActiveDirectory", StringComparison.OrdinalIgnoreCase) == 0;
			return flag;
		}
	}
}