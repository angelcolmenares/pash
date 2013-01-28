using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADOptionalFeature : ADObject
	{
		internal static int FeatureDisableableBit;

		public Guid? FeatureGUID
		{
			get
			{
				return (Guid?)base.GetValue("FeatureGUID");
			}
		}

		public ADOptionalFeatureScope[] FeatureScope
		{
			get
			{
				object value = base.GetValue("FeatureScope");
				if (value != null)
				{
					if ((ADOptionalFeatureScope)value  == ADOptionalFeatureScope.Unknown)
					{
						return (ADOptionalFeatureScope[])value;
					}
					else
					{
						ADOptionalFeatureScope[] aDOptionalFeatureScopeArray = new ADOptionalFeatureScope[1];
						aDOptionalFeatureScopeArray[0] = (ADOptionalFeatureScope)value;
						return aDOptionalFeatureScopeArray;
					}
				}
				else
				{
					ADOptionalFeatureScope[] aDOptionalFeatureScopeArray1 = new ADOptionalFeatureScope[1];
					return aDOptionalFeatureScopeArray1;
				}
			}
		}

		public bool IsDisableable
		{
			get
			{
				return (bool)base.GetValue("IsDisableable");
			}
		}

		public ADDomainMode RequiredDomainMode
		{
			get
			{
				return (ADDomainMode)base.GetValue("RequiredDomainMode");
			}
		}

		public ADForestMode RequiredForestMode
		{
			get
			{
				return (ADForestMode)base.GetValue("RequiredForestMode");
			}
		}

		static ADOptionalFeature()
		{
			ADOptionalFeature.FeatureDisableableBit = 4;
			ADEntity.RegisterMappingTable(typeof(ADOptionalFeature), ADObjectFactory<ADOptionalFeature>.AttributeTable);
		}

		public ADOptionalFeature()
		{
		}

		public ADOptionalFeature(string identity) : base(identity)
		{
		}

		public ADOptionalFeature(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADOptionalFeature(ADObject adobject)
		{
			if (adobject != null)
			{
				base.Identity = adobject;
				if (adobject.IsSearchResult)
				{
					base.SessionInfo = adobject.SessionInfo;
				}
				return;
			}
			else
			{
				throw new ArgumentException("adobject");
			}
		}
	}
}