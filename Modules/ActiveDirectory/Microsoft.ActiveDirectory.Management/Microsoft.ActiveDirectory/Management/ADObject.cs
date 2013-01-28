using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Collections;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADObject : ADEntity
	{
		private const string _debugCategory = "ADObject";

		internal static string[] DefaultProperties;

		internal static string[] IdentityPropertyNames;

		public string DistinguishedName
		{
			get
			{
				return (string)base.GetValue("distinguishedName");
			}
			set
			{
				if (!base.IsSearchResult)
				{
					base.SetValue("distinguishedName", value);
					return;
				}
				else
				{
					DebugLogger.LogWarning("ADObject", "Not allowed to change the DistinguishedName of a search result object");
					throw new NotSupportedException();
				}
			}
		}

		internal override string IdentifyingString
		{
			get
			{
				if (this.DistinguishedName == null)
				{
					return this.Identity.ToString();
				}
				else
				{
					return this.DistinguishedName;
				}
			}
		}

		public string Name
		{
			get
			{
				return (string)base.GetValue("name");
			}
		}

		public string ObjectClass
		{
			get
			{
				object obj = base.GetValue("objectClass");
				if (obj is string) return (string)obj;
				object[] arr = (object[])obj;
				if (arr.Length == 0) return "top";
				return (string)arr[arr.GetUpperBound (0)];
			}
			set
			{
				if (!base.IsSearchResult)
				{
					base.SetValue("objectClass", new object[] { value });
					return;
				}
				else
				{
					DebugLogger.LogWarning("ADObject", "Not allowed to change the ObjectClass of a search result object");
					throw new NotSupportedException();
				}
			}
		}

		public Guid? ObjectGuid
		{
			get
			{
				object obj = base.GetValue ("objectGUID");
				if (obj is Guid) return (Guid)obj;
				return new Guid?(new Guid((byte[])obj));
			}
			set
			{
				if (!base.IsSearchResult)
				{
					base.SetValue("objectGUID", value.GetValueOrDefault ().ToByteArray ());
					return;
				}
				else
				{
					DebugLogger.LogWarning("ADObject", "Not allowed to change the ObjectGuid of a search result object");
					throw new NotSupportedException();
				}
			}
		}

		internal ADPropertyValueCollection ObjectTypes
		{
			get
			{
				return base.InternalProperties["ADPSH-Internal-ObjectTypes"];
			}
			set
			{
				base.InternalProperties.SetValue("ADPSH-Internal-ObjectTypes", value);
			}
		}

		static ADObject()
		{
			string[] strArrays = new string[4];
			strArrays[0] = "distinguishedName";
			strArrays[1] = "name";
			strArrays[2] = "objectClass";
			strArrays[3] = "objectGUID";
			ADObject.DefaultProperties = strArrays;
			string[] strArrays1 = new string[4];
			strArrays1[0] = "distinguishedName";
			strArrays1[1] = "name";
			strArrays1[2] = "objectClass";
			strArrays1[3] = "objectGUID";
			ADObject.IdentityPropertyNames = strArrays1;
			ADEntity.RegisterMappingTable(typeof(ADObject), ADObjectFactory<ADObject>.AttributeTable);
		}

		public ADObject()
		{
		}

		public ADObject(string identity)
		{
			base.Identity = identity;
		}

		public ADObject(Guid? objectGuid)
		{
			base.Identity = objectGuid;
		}

		internal ADObject(string distinguishedName, string objectClass)
		{
			base.Add("distinguishedName", distinguishedName);
			base.Add("objectClass", objectClass);
		}

		internal ADObject(string distinguishedName, string objectClass, Guid? objectGuid)
		{
			base.Add("distinguishedName", distinguishedName);
			base.Add("objectClass", objectClass);
			base.Add("objectGUID", objectGuid);
		}

		internal override bool? IsOfType(string objectType)
		{
			bool? nullable;
			if (this.ObjectTypes != null)
			{
				IEnumerator enumerator = this.ObjectTypes.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						string current = (string)enumerator.Current;
						if (string.Compare(current, objectType, StringComparison.OrdinalIgnoreCase) != 0)
						{
							continue;
						}
						nullable = new bool?(true);
						return nullable;
					}
					return new bool?(false);
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return nullable;
			}
			else
			{
				bool? nullable1 = null;
				return nullable1;
			}
		}

		public override string ToString()
		{
			if (!base.IsSearchResult)
			{
				if (this.Identity == null)
				{
					if (string.IsNullOrEmpty(this.DistinguishedName))
					{
						return base.ToString();
					}
					else
					{
						return this.DistinguishedName;
					}
				}
				else
				{
					return this.Identity.ToString();
				}
			}
			else
			{
				return this.DistinguishedName;
			}
		}
	}
}