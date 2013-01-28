using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class CimCmdletDefinitionContext
	{
		private const string QueryLanguageKey = "QueryDialect";

		private const int FallbackDefaultThrottleLimit = 15;

		private readonly IDictionary<string, string> _privateData;

		private bool? _useEnumerateInstancesInsteadOfWql;

		private Uri _resourceUri;

		private bool _resourceUriHasBeenCalculated;

		private CimOperationFlags? _schemaConformanceLevel;

		public bool ClientSideShouldProcess
		{
			get
			{
				return this._privateData.ContainsKey("ClientSideShouldProcess");
			}
		}

		public bool ClientSideWriteVerbose
		{
			get
			{
				return this._privateData.ContainsKey("ClientSideWriteVerbose");
			}
		}

		public string CmdletizationClassName
		{
			get;
			private set;
		}

		public string CmdletizationClassVersion
		{
			get;
			private set;
		}

		public Version CmdletizationModuleVersion
		{
			get;
			private set;
		}

		public int DefaultThrottleLimit
		{
			get
			{
				string str = null;
				int num = 0;
				if (this._privateData.TryGetValue("DefaultThrottleLimit", out str))
				{
					if (LanguagePrimitives.TryConvertTo<int>(str, CultureInfo.InvariantCulture, out num))
					{
						return num;
					}
					else
					{
						return 15;
					}
				}
				else
				{
					return 15;
				}
			}
		}

		public bool ExposeCimNamespaceParameter
		{
			get
			{
				return this._privateData.ContainsKey("CimNamespaceParameter");
			}
		}

		public Uri ResourceUri
		{
			get
			{
				string str = null;
				Uri uri = null;
				if (!this._resourceUriHasBeenCalculated)
				{
					if (this._privateData != null && this._privateData.TryGetValue("ResourceUri", out str) && Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out uri))
					{
						this._resourceUri = uri;
					}
					this._resourceUriHasBeenCalculated = true;
				}
				return this._resourceUri;
			}
		}

		public CimOperationFlags SchemaConformanceLevel
		{
			get
			{
				string str = null;
				if (!this._schemaConformanceLevel.HasValue)
				{
					CimOperationFlags cimOperationFlag = CimOperationFlags.None;
					if (this._privateData != null && this._privateData.TryGetValue("TypeInformation", out str))
					{
						if (!str.Equals("Basic", StringComparison.OrdinalIgnoreCase))
						{
							if (!str.Equals("Full", StringComparison.OrdinalIgnoreCase))
							{
								if (!str.Equals("None", StringComparison.OrdinalIgnoreCase))
								{
									if (str.Equals("Standard", StringComparison.OrdinalIgnoreCase))
									{
										cimOperationFlag = CimOperationFlags.StandardTypeInformation;
									}
								}
								else
								{
									cimOperationFlag = CimOperationFlags.NoTypeInformation;
								}
							}
							else
							{
								cimOperationFlag = CimOperationFlags.FullTypeInformation;
							}
						}
						else
						{
							cimOperationFlag = CimOperationFlags.BasicTypeInformation;
						}
					}
					this._schemaConformanceLevel = new CimOperationFlags?(cimOperationFlag);
				}
				return this._schemaConformanceLevel.Value;
			}
		}

		public bool SkipTestConnection
		{
			get
			{
				return this._privateData.ContainsKey("SkipTestConnection");
			}
		}

		public bool SupportsShouldProcess
		{
			get;
			private set;
		}

		public bool UseEnumerateInstancesInsteadOfWql
		{
			get
			{
				string str = null;
				if (!this._useEnumerateInstancesInsteadOfWql.HasValue)
				{
					bool flag = false;
					if (this._privateData != null && this._privateData.TryGetValue("QueryDialect", out str) && str.Equals("None", StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
					}
					this._useEnumerateInstancesInsteadOfWql = new bool?(flag);
				}
				return this._useEnumerateInstancesInsteadOfWql.Value;
			}
		}

		internal CimCmdletDefinitionContext(string cmdletizationClassName, string cmdletizationClassVersion, Version cmdletizationModuleVersion, bool supportsShouldProcess, IDictionary<string, string> privateData)
		{
			this.CmdletizationClassName = cmdletizationClassName;
			this.CmdletizationClassVersion = cmdletizationClassVersion;
			this.CmdletizationModuleVersion = cmdletizationModuleVersion;
			this.SupportsShouldProcess = supportsShouldProcess;
			this._privateData = privateData;
		}
	}
}