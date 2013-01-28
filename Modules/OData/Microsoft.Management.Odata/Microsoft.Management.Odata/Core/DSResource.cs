using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Providers;
using System.Linq;

namespace Microsoft.Management.Odata.Core
{
	internal class DSResource
	{
		private Dictionary<string, object> properties;

		private DSResource completeObject;

		internal bool ContainsNonKeyProperties
		{
			get;
			private set;
		}

		internal ResourceType ResourceType
		{
			get;
			private set;
		}

		public DSResource(ResourceType resourceType, bool onlyKeyProperties = false)
		{
			this.properties = new Dictionary<string, object>();
			this.ResourceType = resourceType;
			this.ContainsNonKeyProperties = !onlyKeyProperties;
		}

		public Dictionary<string, object> GetKeyValues()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			foreach (ResourceProperty keyProperty in this.ResourceType.KeyProperties)
			{
				object item = this.properties[keyProperty.Name];
				strs.Add(keyProperty.Name, item);
			}
			return strs;
		}

		public int GetPropertyCount()
		{
			return this.properties.Count;
		}

		public object GetValue(string propertyName, DataServiceQueryProvider.ResultSetCollection resultSets = null)
		{
			object obj = null;
			Func<ResourceProperty, bool> func = null;
			if (!this.ContainsNonKeyProperties && resultSets != null)
			{
				if (this.completeObject == null)
				{
					ReadOnlyCollection<ResourceProperty> properties = this.ResourceType.Properties;
					if (func == null)
					{
						func = (ResourceProperty it) => string.Equals(propertyName, it.Name, StringComparison.Ordinal);
					}
					ResourceProperty resourceProperty = properties.FirstOrDefault<ResourceProperty>(func);
					bool flag = false;
					if (!resourceProperty.IsKeyProperty())
					{
						if (resourceProperty.IsReferenceSetProperty())
						{
							flag = !this.properties.ContainsKey(propertyName);
						}
						else
						{
							flag = true;
						}
					}
					if (flag)
					{
						this.UpdateCompleteObject(resultSets);
					}
				}
				if (this.completeObject != null)
				{
					return this.completeObject.GetValue(propertyName, resultSets);
				}
			}
			if (!this.properties.TryGetValue(propertyName, out obj))
			{
				if (this.ResourceType.Properties.Any<ResourceProperty>((ResourceProperty item) => item.Name == propertyName))
				{
					return null;
				}
				else
				{
					throw new ResourcePropertyNotFoundException(this.ResourceType.Name, propertyName);
				}
			}
			else
			{
				if (obj != null)
				{
					IReferencedResourceSet referencedResourceSet = obj as IReferencedResourceSet;
					if (referencedResourceSet != null)
					{
						Dictionary<string, object> strs = new Dictionary<string, object>();
						foreach (ResourceProperty keyProperty in this.ResourceType.KeyProperties)
						{
							object obj1 = null;
							if (this.properties.TryGetValue(keyProperty.Name, out obj1))
							{
								strs.Add(keyProperty.Name, obj1);
							}
							else
							{
								throw new ResourcePropertyNotFoundException(this.ResourceType.Name, keyProperty.Name);
							}
						}
						List<DSResource> dSResources = referencedResourceSet.Get(strs);
						this.properties[propertyName] = dSResources;
						return dSResources;
					}
				}
				return obj;
			}
		}

		private bool MatchKeyProperties(DSResource other)
		{
			bool flag;
			IEnumerator<ResourceProperty> enumerator = this.ResourceType.KeyProperties.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					ResourceProperty current = enumerator.Current;
					current.GetType();
					if (this.properties[current.Name].Equals(other.properties[current.Name]))
					{
						continue;
					}
					flag = false;
					return flag;
				}
				return true;
			}
			return flag;
		}

		public void SetValue(string propertyName, object value)
		{
			ResourceProperty resourceProperty = this.ResourceType.Properties.FirstOrDefault<ResourceProperty>((ResourceProperty item) => item.Name == propertyName);
			if (resourceProperty != null)
			{
				this.properties[propertyName] = value;
				return;
			}
			else
			{
				throw new ResourcePropertyNotFoundException(this.ResourceType.Name, propertyName);
			}
		}

		internal IDictionary<string, object> TestHookGetProperties()
		{
			return this.properties;
		}

		internal void UpdateCompleteObject(DataServiceQueryProvider.ResultSetCollection resultSets)
		{
			DataServiceQueryProvider.ResultSet resultSet = null;
			Tracer tracer = new Tracer();
			if (resultSets == null || !resultSets.TryGetValue(this.ResourceType.Name, out resultSet))
			{
				tracer.DebugMessage(string.Concat("UpdateCompleteObject: result set ", this.ResourceType.Name, " not found"));
				throw new PowerShellWebServiceException(string.Concat("no result set named ", this.ResourceType.Name));
			}
			else
			{
				this.completeObject = resultSet.FirstOrDefault<DSResource>((DSResource item) => this.MatchKeyProperties(item));
				if (this.completeObject != null)
				{
					tracer.DebugMessage("UpdateCompleteObject: found complete object");
					return;
				}
				else
				{
					tracer.DebugMessage("UpdateCompleteObject: no match for key properties of this object");
					throw new PowerShellWebServiceException(string.Concat("no matching instance in result set named", this.ResourceType.Name));
				}
			}
		}

		public class KeyEqualityComparer : EqualityComparer<DSResource>
		{
			public KeyEqualityComparer()
			{
			}

			public override bool Equals(DSResource r1, DSResource r2)
			{
				if (r1 != null || r2 != null)
				{
					if (r1 == null || r2 == null)
					{
						return false;
					}
					else
					{
						if (r1.ResourceType.Equals(r2.ResourceType))
						{
							return r1.MatchKeyProperties(r2);
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					return true;
				}
			}

			public override int GetHashCode(DSResource resource)
			{
				int hashCode = 0;
				if (resource != null)
				{
					foreach (ResourceProperty keyProperty in resource.ResourceType.KeyProperties)
					{
						hashCode = (hashCode << 1) + resource.properties[keyProperty.Name].GetHashCode();
					}
				}
				return hashCode;
			}
		}
	}
}