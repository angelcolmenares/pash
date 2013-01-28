using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADGetPropertiesCmdletBase<P, F, O, ROF, RO> : ADCmdletBase<P>, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADEntity, new()
	where ROF : ADFactory<RO>, new()
	where RO : ADEntity, new()
	{
		private const string _debugCategory = "ADGetPropertiesCmdletBase";

		protected F _factory;

		protected ROF _returnObjectFactory;

		private int _pageSize;

		internal virtual bool AutoRangeRetrieve
		{
			get
			{
				return true;
			}
		}

		internal abstract IdentityLookupMode IdentityLookupMode
		{
			get;
		}

		internal abstract string SourceProperty
		{
			get;
		}

		internal virtual SourcePropertyType SourcePropertyType
		{
			get
			{
				return SourcePropertyType.IdentityInfo;
			}
		}

		public ADGetPropertiesCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
			this._returnObjectFactory = Activator.CreateInstance<ROF>();
			this._pageSize = 0x100;
			base.ProcessRecordPipeline.Clear();
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADGetPropertiesCmdletBaseProcessCSRoutine));
		}

		protected bool ADGetPropertiesCmdletBaseProcessCSRoutine()
		{
			if (this._cmdletParameters.Contains("Identity"))
			{
				O item = (O)(this._cmdletParameters["Identity"] as O);
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				this._returnObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				if (this.IdentityLookupMode != IdentityLookupMode.DirectoryMode || this.SourcePropertyType != SourcePropertyType.LinkedDN)
				{
					if (this.IdentityLookupMode != IdentityLookupMode.FactoryMode || this.SourcePropertyType != SourcePropertyType.IdentityInfo)
					{
						if (this.IdentityLookupMode == IdentityLookupMode.DirectoryMode && this.SourcePropertyType == SourcePropertyType.IdentityInfo)
						{
							string identityObjectDN = this.GetIdentityObjectDN();
							if (identityObjectDN != null)
							{
								ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, identityObjectDN, ADSearchScope.Base);
								using (aDObjectSearcher)
								{
									aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
									aDObjectSearcher.Properties.Add(this.SourceProperty);
									ADObject aDObject = aDObjectSearcher.FindOne();
									this.WritePropertiesToOutput(item, aDObject);
								}
							}
						}
					}
					else
					{
						string[] sourceProperty = new string[1];
						sourceProperty[0] = this.SourceProperty;
						O extendedObjectFromIdentity = this._factory.GetExtendedObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath, sourceProperty);
						this.WritePropertiesToOutput(item, extendedObjectFromIdentity);
					}
				}
				else
				{
					string str = this.GetIdentityObjectDN();
					if (str != null)
					{
						AttributeSetRequest attributeSetRequest = this._returnObjectFactory.ConstructAttributeSetRequest(null);
						ADObjectSearcher structuralObjectFilter = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, str, ADSearchScope.Base);
						using (structuralObjectFilter)
						{
							structuralObjectFilter.AttributeScopedQuery = this.SourceProperty;
							structuralObjectFilter.Filter = this._returnObjectFactory.StructuralObjectFilter;
							structuralObjectFilter.Properties.AddRange(attributeSetRequest.DirectoryAttributes);
							structuralObjectFilter.AutoRangeRetrieve = this.AutoRangeRetrieve;
							IEnumerable<ADObject> aDObjects = structuralObjectFilter.FindAll();
							if (aDObjects != null)
							{
								foreach (ADObject aDObject1 in aDObjects)
								{
									if (aDObject1 == null)
									{
										continue;
									}
									RO rO = this._returnObjectFactory.Construct(aDObject1, attributeSetRequest);
									base.WriteObject(rO);
								}
							}
						}
					}
				}
			}
			return true;
		}

		protected internal virtual string ExtractIdentityInfoFromSourcePropertyValue(string sourcePropertyValue, out bool isExtractedIdentityDN)
		{
			isExtractedIdentityDN = false;
			return sourcePropertyValue;
		}

		protected internal virtual string GetIdentityObjectDN()
		{
			O item = (O)(this._cmdletParameters["Identity"] as O);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath);
			if (directoryObjectFromIdentity == null)
			{
				return null;
			}
			else
			{
				return directoryObjectFromIdentity.DistinguishedName;
			}
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			if (this._cmdletParameters.Contains("Identity"))
			{
				return this._cmdletParameters["Identity"];
			}
			else
			{
				return null;
			}
		}

		private void WriteObjectsInBatch(List<IADOPathNode> filterList)
		{
			IADOPathNode item;
			if (filterList.Count != 1)
			{
				item = ADOPathUtil.CreateOrClause(filterList.ToArray());
			}
			else
			{
				item = filterList[0];
			}
			int? nullable = null;
			IEnumerable<RO> extendedObjectFromFilter = this._returnObjectFactory.GetExtendedObjectFromFilter(item, this.GetCmdletSessionInfo().DefaultPartitionPath, ADSearchScope.Subtree, null, nullable, new int?(this._pageSize), false);
			int num = 0;
			if (extendedObjectFromFilter != null)
			{
				foreach (RO rO in extendedObjectFromFilter)
				{
					base.WriteObject(rO);
					num++;
				}
			}
		}

		protected internal virtual void WritePropertiesToOutput(O inputIdentity, ADEntity constructedIdentityObject)
		{
			bool flag = false;
			if (constructedIdentityObject == null)
			{
				DebugLogger.LogInfo("ADGetPropertiesCmdletBase", string.Format("Identity: {0} not found", inputIdentity));
			}
			else
			{
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				ADPropertyValueCollection item = constructedIdentityObject[this.SourceProperty];
				RO rO = Activator.CreateInstance<RO>();
				if (item == null)
				{
					DebugLogger.LogInfo("ADGetPropertiesCmdletBase", string.Format("Could  not find property: {0} for identity: {1}", this.SourceProperty, inputIdentity));
					return;
				}
				else
				{
					List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>(this._pageSize);
					foreach (string str in item)
					{
						try
						{
							string str1 = this.ExtractIdentityInfoFromSourcePropertyValue(str, out flag);
							if (str1 != null)
							{
								rO.Identity = str1;
								if (!flag)
								{
									RO extendedObjectFromIdentity = this._returnObjectFactory.GetExtendedObjectFromIdentity(rO, cmdletSessionInfo.DefaultPartitionPath);
									base.WriteObject(extendedObjectFromIdentity);
								}
								else
								{
									IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", Utils.EscapeDNForFilter(str1));
									aDOPathNodes.Add(aDOPathNode);
									if (aDOPathNodes.Count >= this._pageSize)
									{
										this.WriteObjectsInBatch(aDOPathNodes);
										aDOPathNodes.Clear();
									}
								}
							}
							else
							{
								DebugLogger.LogInfo("ADGetPropertiesCmdletBase", string.Format("ExtractIdentityInfoFromSourcePropertyValue returned NULL for propertyValue: {0} - skipping this value", str));
							}
						}
						catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
						{
							ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
							base.WriteError(ADUtilities.GetErrorRecord(aDIdentityNotFoundException, "ADGetPropertiesCmdletBase:WritePropertiesToOutput", str));
						}
					}
					if (aDOPathNodes.Count > 0)
					{
						this.WriteObjectsInBatch(aDOPathNodes);
						aDOPathNodes.Clear();
						return;
					}
				}
			}
		}
	}
}