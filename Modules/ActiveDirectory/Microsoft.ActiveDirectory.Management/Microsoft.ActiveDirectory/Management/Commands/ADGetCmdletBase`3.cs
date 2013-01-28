using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADGetCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADEntity, new()
	{
		private const string _debugCategory = "ADGetCmdletBase";

		protected internal F _factory;

		protected internal string[] _propertiesRequested;

		private bool _filterProcessed;

		private bool _showDeleted;

		private ADGetCmdletBase<P, F, O>.OutputFilterDelegate _outputFilterFunction;

		public ADGetCmdletBase<P, F, O>.OutputFilterDelegate OutputFilterFunction
		{
			get
			{
				return this._outputFilterFunction;
			}
			set
			{
				this._outputFilterFunction = value;
			}
		}

		public ADGetCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
			this.PopulateDefaultParameters();
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.ADGetCmdletBaseBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADGetCmdletBaseProcessCSRoutine));
		}

		private bool ADGetCmdletBaseBeginCSRoutine()
		{
			IADOPathNode ldapFilterADOPathNode = null;
			bool flag = false;
			this._showDeleted = this._cmdletParameters.GetSwitchParameterBooleanValue("IncludeDeletedObjects");
			if (this._cmdletParameters.Contains("Filter") || this._cmdletParameters.Contains("LDAPFilter"))
			{
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				this.ValidateParameters();
				this.BuildPropertySet();
			}
			if (!this._cmdletParameters.Contains("Filter"))
			{
				if (this._cmdletParameters.Contains("LDAPFilter"))
				{
					ldapFilterADOPathNode = new LdapFilterADOPathNode(this._cmdletParameters["LDAPFilter"] as string);
					flag = true;
				}
			}
			else
			{
				string item = this._cmdletParameters["Filter"] as string;
				try
				{
					var obj = this._factory;
					ConvertSearchFilterDelegate convertSearchFilterDelegate = new ConvertSearchFilterDelegate(obj.BuildSearchFilter);
					VariableExpressionConverter variableExpressionConverter = new VariableExpressionConverter(new EvaluateVariableDelegate(this.EvaluateFilterVariable));
					QueryParser queryParser = new QueryParser(item, variableExpressionConverter, convertSearchFilterDelegate);
					ldapFilterADOPathNode = queryParser.FilterExpressionTree;
				}
				catch (ADFilterParsingException aDFilterParsingException1)
				{
					ADFilterParsingException aDFilterParsingException = aDFilterParsingException1;
					base.ThrowTerminatingError(base.ConstructErrorRecord(aDFilterParsingException));
				}
				flag = true;
			}
			this._filterProcessed = flag;
			if (flag)
			{
				this.OutputSearchResults(ldapFilterADOPathNode);
			}
			return true;
		}

		protected bool ADGetCmdletBaseProcessCSRoutine()
		{
			if (!this._cmdletParameters.Contains("Identity"))
			{
				if (!this._filterProcessed)
				{
					object[] objArray = new object[1];
					objArray[0] = "Identity,Filter,LDAPFilter";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequiredMultiple, objArray));
				}
			}
			else
			{
				O item = (O)(this._cmdletParameters["Identity"] as O);
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				this.ValidateParameters();
				this.BuildPropertySet();
				O extendedObjectFromIdentity = this._factory.GetExtendedObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath, this._propertiesRequested, this._showDeleted);
				base.WriteObject(extendedObjectFromIdentity);
			}
			return true;
		}

		internal IADOPathNode AppendObjectClassFilter(IADOPathNode filter)
		{
			IADOPathNode[] structuralObjectFilter = new IADOPathNode[2];
			structuralObjectFilter[0] = filter;
			structuralObjectFilter[1] = this._factory.StructuralObjectFilter;
			return ADOPathUtil.CreateAndClause(structuralObjectFilter);
		}

		protected internal void BuildPropertySet()
		{
			if (!this._cmdletParameters.Contains("Properties"))
			{
				this._propertiesRequested = null;
			}
			else
			{
				object item = this._cmdletParameters["Properties"];
				if (item as object[] == null)
				{
					string[] strArrays = new string[1];
					strArrays[0] = this._cmdletParameters["Properties"] as string;
					this._propertiesRequested = strArrays;
				}
				else
				{
					this._propertiesRequested = this._cmdletParameters["Properties"] as string[];
				}
			}
			if (this._showDeleted)
			{
				if (this._propertiesRequested != null)
				{
					bool flag = false;
					string[] strArrays1 = this._propertiesRequested;
					int num = 0;
					while (num < (int)strArrays1.Length)
					{
						string str = strArrays1[num];
						if (string.Compare(str, "Deleted", StringComparison.OrdinalIgnoreCase) != 0)
						{
							num++;
						}
						else
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Array.Resize<string>(ref this._propertiesRequested, (int)this._propertiesRequested.Length + 1);
						this._propertiesRequested[(int)this._propertiesRequested.Length - 1] = "Deleted";
					}
				}
				else
				{
					string[] strArrays2 = new string[1];
					strArrays2[0] = "Deleted";
					this._propertiesRequested = strArrays2;
					return;
				}
			}
		}

		internal object EvaluateFilterVariable(string varName)
		{
			return base.GetVariableValue(varName);
		}

		protected internal override string GetDefaultPartitionPath()
		{
			string item = this._cmdletParameters["SearchBase"] as string;
			if (string.IsNullOrEmpty(item))
			{
				return base.GetDefaultPartitionPath();
			}
			else
			{
				return ADForestPartitionInfo.ExtractAndValidatePartitionInfo(this.GetRootDSE(), item);
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

		internal virtual void OutputSearchResults(IADOPathNode filter)
		{
			string item;
			ADSessionInfo sessionInfo = base.GetSessionInfo();
			int? nullable = (int?)this._cmdletParameters["ResultSetSize"];
			int? nullable1 = null;
			ADSearchScope aDSearchScope = (ADSearchScope)this._cmdletParameters["SearchScope"];
			bool flag = false;
			if (this._cmdletParameters.Contains("SearchBase"))
			{
				item = this._cmdletParameters["SearchBase"] as string;
				flag = true;
			}
			else
			{
				item = this.GetDefaultQueryPath();
			}
			if (!flag || !(string.Empty == item) || sessionInfo.ConnectedToGC)
			{
				if (item == null || !sessionInfo.ConnectedToGC && item == string.Empty)
				{
					object[] objArray = new object[1];
					objArray[0] = "SearchBase";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
				}
				else
				{
					if (this._cmdletParameters.Contains("ResultPageSize"))
					{
						nullable1 = new int?((int)this._cmdletParameters["ResultPageSize"]);
					}
					IEnumerable<O> extendedObjectFromFilter = this._factory.GetExtendedObjectFromFilter(filter, item, aDSearchScope, this._propertiesRequested, nullable, nullable1, this._showDeleted);
					if (extendedObjectFromFilter != null)
					{
						if (this._outputFilterFunction != null)
						{
							foreach (O o in extendedObjectFromFilter)
							{
								if (!this._outputFilterFunction(o))
								{
									continue;
								}
								base.WriteObject(o);
							}
						}
						else
						{
							foreach (O o1 in extendedObjectFromFilter)
							{
								base.WriteObject(o1);
							}
						}
					}
					return;
				}
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = "SearchBase";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.EmptySearchBaseNotSupported, objArray1));
			}
		}

		protected internal virtual void PopulateDefaultParameters()
		{
			this._cmdletParameters["SearchScope"] = ADSearchScope.Subtree;
			this._cmdletParameters["ResultPageSize"] = 0x100;
			this._cmdletParameters["ResultSetSize"] = ParameterDefaults.ResultSetSize;
		}

		protected internal virtual void ValidateParameters()
		{
			this.GetCmdletSessionInfo();
			if (!this._cmdletParameters.Contains("Identity") || !string.IsNullOrEmpty(this.GetDefaultPartitionPath()))
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Partition";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}

		public delegate bool OutputFilterDelegate(O output);
	}
}