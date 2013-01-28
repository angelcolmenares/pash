using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class IdentityResolverMethods
	{
		internal static ADObjectSearcher BuildAggregatedSearchFilterIdentityResolver(IdentityResolverDelegate[] identityDelegates, ADOperator op, object identity, string searchRoot, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			ADObjectSearcher item;
			List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>((int)identityDelegates.Length);
			IdentityResolverDelegate[] identityResolverDelegateArray = identityDelegates;
			for (int i = 0; i < (int)identityResolverDelegateArray.Length; i++)
			{
				bool V_1;
				IdentityResolverDelegate identityResolverDelegate = identityResolverDelegateArray[i];
				item = identityResolverDelegate(identity, searchRoot, cmdletSessionInfo, out V_1);
				if (item != null)
				{
					aDOPathNodes.Add(item.Filter);
				}
			}
			item = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchRoot, ADSearchScope.Subtree);
			if (aDOPathNodes.Count <= 1)
			{
				if (aDOPathNodes.Count != 1)
				{
					item = null;
				}
				else
				{
					item.Filter = aDOPathNodes[0];
				}
			}
			else
			{
				if (ADOperator.Or != op)
				{
					item.Filter = ADOPathUtil.CreateAndClause(aDOPathNodes.ToArray());
				}
				else
				{
					item.Filter = ADOPathUtil.CreateOrClause(aDOPathNodes.ToArray());
				}
			}
			useSearchFilter = true;
			return item;
		}

		internal static ADObjectSearcher BuildDNBaseSearcher(string DN, CmdletSessionInfo cmdletSessionInfo)
		{
			return SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, DN, ADSearchScope.Base);
		}

		internal static ADObjectSearcher BuildGenericSearcher(string[] identityLdapAttributes, object identity, string searchRoot, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			useSearchFilter = true;
			string str = identity as string;
			if (!string.IsNullOrEmpty(str))
			{
				ADObjectSearcher item = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchRoot, ADSearchScope.Subtree);
				List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>((int)identityLdapAttributes.Length);
				string[] strArrays = identityLdapAttributes;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str1 = strArrays[i];
					if (str1 != "distinguishedName")
					{
						aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, str1, str));
					}
					else
					{
						string str2 = Utils.EscapeDNForFilter(str);
						aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, str1, str2));
					}
				}
				if (aDOPathNodes.Count <= 1)
				{
					item.Filter = aDOPathNodes[0];
				}
				else
				{
					item.Filter = ADOPathUtil.CreateOrClause(aDOPathNodes.ToArray());
				}
				return item;
			}
			else
			{
				return null;
			}
		}

		internal static ADObjectSearcher BuildGuidBaseSearcher(Guid? guidObject, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = string.Concat("<GUID=", guidObject, ">");
			return SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, str, ADSearchScope.Base);
		}

		internal static ADObjectSearcher BuildObjectGuidSearcher(Guid? guidObject, string searchBase, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchBase, ADSearchScope.Subtree);
			Guid value = guidObject.Value;
			aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectGUID", value.ToByteArray());
			return aDObjectSearcher;
		}

		internal static ADObjectSearcher BuildSidBaseSearcher(SecurityIdentifier sidObject, string searchRoot, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchRoot, ADSearchScope.Subtree);
			aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", sidObject);
			return aDObjectSearcher;
		}

		internal static ADObjectSearcher DistinguishedNameIdentityResolver(object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			useSearchFilter = false;
			if (identityObject != null)
			{
				string str = identityObject as string;
				if (string.IsNullOrEmpty(str) || !ADPathModule.IsValidPath(str, ADPathFormat.X500) || !ADForestPartitionInfo.IsDNUnderPartition(cmdletSessionInfo.ADRootDSE, str, false))
				{
					ADObject aDObject = identityObject as ADObject;
					if (aDObject == null || string.IsNullOrEmpty(aDObject.DistinguishedName))
					{
						return null;
					}
					else
					{
						return IdentityResolverMethods.BuildDNBaseSearcher(aDObject.DistinguishedName, cmdletSessionInfo);
					}
				}
				else
				{
					return IdentityResolverMethods.BuildDNBaseSearcher(str, cmdletSessionInfo);
				}
			}
			else
			{
				throw new ArgumentNullException("identityObject");
			}
		}

		internal static IdentityResolverDelegate GetAggregatedIdentityResolver(ADOperator op, IdentityResolverDelegate[] identityDelegates)
		{
			return (object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter) => IdentityResolverMethods.BuildAggregatedSearchFilterIdentityResolver(identityDelegates, op, identityObject, searchBase, cmdletSessionInfo, out useSearchFilter);
		}

		internal static IdentityResolverDelegate GetCustomIdentityResolver(IdentityResolverDelegate function)
		{
			return function;
		}

		internal static IdentityResolverDelegate GetGenericIdentityResolver(string[] identityLdapAttributes)
		{
			return (object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter) => IdentityResolverMethods.BuildGenericSearcher(identityLdapAttributes, identityObject, searchBase, cmdletSessionInfo, out useSearchFilter);
		}

		internal static ADObjectSearcher GuidIdentityResolver(object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			useSearchFilter = false;
			if (identityObject != null)
			{
				Guid? nullable = null;
				string str = identityObject as string;
				if (string.IsNullOrEmpty(str) || !Utils.TryParseGuid(str, out nullable))
				{
					if (!(identityObject is Guid))
					{
						ADObject aDObject = identityObject as ADObject;
						if (aDObject != null)
						{
							Guid? objectGuid = aDObject.ObjectGuid;
							if (objectGuid.HasValue)
							{
								return IdentityResolverMethods.BuildGuidBaseSearcher(aDObject.ObjectGuid, cmdletSessionInfo);
							}
						}
						return null;
					}
					else
					{
						return IdentityResolverMethods.BuildGuidBaseSearcher(new Guid?((Guid)identityObject), cmdletSessionInfo);
					}
				}
				else
				{
					return IdentityResolverMethods.BuildGuidBaseSearcher(nullable, cmdletSessionInfo);
				}
			}
			else
			{
				throw new ArgumentNullException("identityObject");
			}
		}

		internal static ADObjectSearcher GuidSearchFilterIdentityResolver(object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			useSearchFilter = true;
			if (identityObject != null)
			{
				Guid? nullable = null;
				string str = identityObject as string;
				if (string.IsNullOrEmpty(str) || !Utils.TryParseGuid(str, out nullable))
				{
					if (!(identityObject is Guid))
					{
						ADObject aDObject = identityObject as ADObject;
						if (aDObject != null)
						{
							Guid? objectGuid = aDObject.ObjectGuid;
							if (objectGuid.HasValue)
							{
								return IdentityResolverMethods.BuildObjectGuidSearcher(aDObject.ObjectGuid, searchBase, cmdletSessionInfo);
							}
						}
						return null;
					}
					else
					{
						return IdentityResolverMethods.BuildObjectGuidSearcher(new Guid?((Guid)identityObject), searchBase, cmdletSessionInfo);
					}
				}
				else
				{
					return IdentityResolverMethods.BuildObjectGuidSearcher(nullable, searchBase, cmdletSessionInfo);
				}
			}
			else
			{
				throw new ArgumentNullException("identityObject");
			}
		}

		internal static ADObjectSearcher SamAccountNameIdentityResolver(object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			useSearchFilter = true;
			if (identityObject != null)
			{
				ADPrincipal aDPrincipal = identityObject as ADPrincipal;
				if (aDPrincipal == null || aDPrincipal.SamAccountName == null)
				{
					return null;
				}
				else
				{
					ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchBase, ADSearchScope.Subtree);
					aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "sAMAccountName", aDPrincipal.SamAccountName);
					return aDObjectSearcher;
				}
			}
			else
			{
				throw new ArgumentNullException("identityObject");
			}
		}

		internal static ADObjectSearcher SidIdentityResolver(object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			useSearchFilter = true;
			if (identityObject != null)
			{
				SecurityIdentifier securityIdentifier = null;
				string str = identityObject as string;
				if (string.IsNullOrEmpty(str) || !Utils.TryParseSid(str, out securityIdentifier))
				{
					securityIdentifier = identityObject as SecurityIdentifier;
					if (securityIdentifier == null)
					{
						ADPrincipal aDPrincipal = identityObject as ADPrincipal;
						if (aDPrincipal == null || !(aDPrincipal.SID != null))
						{
							return null;
						}
						else
						{
							return IdentityResolverMethods.BuildSidBaseSearcher(aDPrincipal.SID, searchBase, cmdletSessionInfo);
						}
					}
					else
					{
						return IdentityResolverMethods.BuildSidBaseSearcher(securityIdentifier, searchBase, cmdletSessionInfo);
					}
				}
				else
				{
					return IdentityResolverMethods.BuildSidBaseSearcher(securityIdentifier, searchBase, cmdletSessionInfo);
				}
			}
			else
			{
				throw new ArgumentNullException("identityObject");
			}
		}
	}
}