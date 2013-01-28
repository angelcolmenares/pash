using System;
using System.Collections;
using System.DirectoryServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class ADDNConstraintLinkedAttrSet : ADDNLinkedAttrSet
	{
		private ADDNConstraintLinkedAttrSet.ConstraintType constraint;

		private object constraintData;

		internal ADDNConstraintLinkedAttrSet(ADDNConstraintLinkedAttrSet.ConstraintType constraint, object constraintData, string groupDN, IEnumerable[] members, string primaryGroupDN, DirectorySearcher queryMembersSearcher, bool recursive, ADStoreCtx storeCtx) : base(groupDN, members, primaryGroupDN, queryMembersSearcher, recursive, storeCtx)
		{
			this.constraint = constraint;
			this.constraintData = constraintData;
		}

		internal ADDNConstraintLinkedAttrSet(ADDNConstraintLinkedAttrSet.ConstraintType constraint, object constraintData, string groupDN, DirectorySearcher[] membersSearcher, string primaryGroupDN, DirectorySearcher primaryGroupMembersSearcher, bool recursive, ADStoreCtx storeCtx) : base(groupDN, membersSearcher, primaryGroupDN, primaryGroupMembersSearcher, recursive, storeCtx)
		{
			this.constraint = constraint;
			this.constraintData = constraintData;
		}

		internal override bool MoveNext()
		{
			string str;
			dSPropertyCollection _dSPropertyCollection;
			bool flag = false;
			if (base.MoveNext())
			{
				while (!flag)
				{
					if (this.current != null)
					{
						ADDNConstraintLinkedAttrSet.ConstraintType constraintType = this.constraint;
						switch (constraintType)
						{
							case ADDNConstraintLinkedAttrSet.ConstraintType.ContainerStringMatch:
							{
								if (this.current as SearchResult == null)
								{
									str = ((DirectoryEntry)this.current).Properties["distinguishedName"].Value.ToString();
								}
								else
								{
									str = ((SearchResult)this.current).Properties["distinguishedName"][0].ToString();
								}
								if (!str.EndsWith((string)this.constraintData, StringComparison.Ordinal))
								{
									break;
								}
								flag = true;
								break;
							}
							case ADDNConstraintLinkedAttrSet.ConstraintType.ResultValidatorDelegateMatch:
							{
								ADDNConstraintLinkedAttrSet.ResultValidator resultValidator = this.constraintData as ADDNConstraintLinkedAttrSet.ResultValidator;
								if (resultValidator == null)
								{
									break;
								}
								if (this.current as SearchResult == null)
								{
									_dSPropertyCollection = new dSPropertyCollection(((DirectoryEntry)this.current).Properties);
								}
								else
								{
									_dSPropertyCollection = new dSPropertyCollection(((SearchResult)this.current).Properties);
								}
								flag = resultValidator(_dSPropertyCollection);
								break;
							}
						}
						if (flag || this.MoveNext())
						{
							continue;
						}
						return false;
					}
					else
					{
						return false;
					}
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		internal enum ConstraintType
		{
			ContainerStringMatch,
			ResultValidatorDelegateMatch
		}

		internal delegate bool ResultValidator(dSPropertyCollection resultPropCollection);
	}
}