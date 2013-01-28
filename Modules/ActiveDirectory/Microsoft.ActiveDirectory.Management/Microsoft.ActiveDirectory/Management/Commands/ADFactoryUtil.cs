using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ADFactoryUtil
	{
		private const string _debugCategory = "ADFactoryUtil";

		internal static ADObjectSearcher GetADObjectSearcherFromIdentity(ADEntity identityObj, string searchRoot, bool showDeleted, IADOPathNode structuralObjectFilter, IADOPathNode identityFilter, IdentityResolverDelegate[] identityResolvers, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObjectSearcher identity = null;
			IADOPathNode aDOPathNode = null;
			bool flag = true;
			IdentityResolverDelegate[] identityResolverDelegateArray = identityResolvers;
			int num = 0;
			while (num < (int)identityResolverDelegateArray.Length)
			{
				IdentityResolverDelegate identityResolverDelegate = identityResolverDelegateArray[num];
				if (identityObj.Identity == null)
				{
					identity = identityResolverDelegate(identityObj, searchRoot, cmdletSessionInfo, out flag);
				}
				else
				{
					identity = identityResolverDelegate(identityObj.Identity, searchRoot, cmdletSessionInfo, out flag);
				}
				if (identity == null)
				{
					num++;
				}
				else
				{
					if (!flag)
					{
						aDOPathNode = structuralObjectFilter;
						break;
					}
					else
					{
						IADOPathNode[] filter = new IADOPathNode[2];
						filter[0] = identity.Filter;
						filter[1] = structuralObjectFilter;
						aDOPathNode = ADOPathUtil.CreateAndClause(filter);
						break;
					}
				}
			}
			if (identity == null)
			{
				if (searchRoot != null)
				{
					if (identityFilter != null)
					{
						IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
						aDOPathNodeArray[0] = identityFilter;
						aDOPathNodeArray[1] = structuralObjectFilter;
						aDOPathNode = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
						identity = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchRoot, ADSearchScope.Subtree);
					}
					else
					{
						object[] str = new object[2];
						str[0] = identityObj.ToString();
						str[1] = searchRoot;
						throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, str));
					}
				}
				else
				{
					throw new ADIdentityResolutionException(StringResources.IdentityResolutionPartitionRequired);
				}
			}
			identity.Filter = aDOPathNode;
			identity.ShowDeleted = showDeleted;
			return identity;
		}

		internal static ADObject GetObjectFromIdentitySearcher(ADObjectSearcher searcher, ADEntity identityObj, string searchRoot, AttributeSetRequest attrs, CmdletSessionInfo cmdletSessionInfo, out string[] warningMessages)
		{
			ADObject aDObject;
			bool flag = false;
			warningMessages = new string[0];
			using (searcher)
			{
				searcher.Properties.AddRange(attrs.DirectoryAttributes);
				DebugLogger.LogInfo("ADFactoryUtil", string.Format("GetObjectFromIdentity: Searching for identity using filter: {0} searchbase: {1}", searcher.Filter.GetLdapFilterString(), searcher.SearchRoot));
				aDObject = searcher.FindOne(out flag);
				if (aDObject != null)
				{
					if (flag)
					{
						throw new ADMultipleMatchingIdentitiesException(StringResources.MultipleMatches);
					}
				}
				else
				{
					DebugLogger.LogInfo("ADFactoryUtil", string.Format("GetObjectFromIdentity: Identity not found.", new object[0]));
					object[] str = new object[2];
					str[0] = identityObj.ToString();
					str[1] = searchRoot;
					throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, str));
				}
			}
			string str1 = ADForestPartitionInfo.ExtractPartitionInfo(cmdletSessionInfo.ADRootDSE, aDObject.DistinguishedName, false);
			if (cmdletSessionInfo.CmdletParameters.Contains("Partition"))
			{
				string item = cmdletSessionInfo.CmdletParameters["Partition"] as string;
				if (!ADPathModule.ComparePath(item, str1, ADPathFormat.X500))
				{
					string[] strArrays = new string[1];
					object[] objArray = new object[3];
					objArray[0] = identityObj.ToString();
					objArray[1] = str1;
					objArray[2] = item;
					strArrays[0] = string.Format(CultureInfo.CurrentCulture, StringResources.IdentityInWrongPartition, objArray);
					warningMessages = strArrays;
				}
			}
			cmdletSessionInfo.DefaultPartitionPath = str1;
			return aDObject;
		}
	}
}