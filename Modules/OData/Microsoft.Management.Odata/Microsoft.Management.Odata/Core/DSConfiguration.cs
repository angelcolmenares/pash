using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Collections;
using System.Configuration;
using System.Text;

namespace Microsoft.Management.Odata.Core
{
	internal sealed class DSConfiguration : ConfigurationSection
	{
		public const string SectionName = "managementOdata";

		public const int Unlimited = 0;

		internal const int DefaultInvocationLifetime = 0x258;

		internal const int DefaultInvocationResultChars = 0x20000;

		internal const int DefaultSchemaIdleTimeout = 0x258;

		internal const int DefaultRunspaceIdleTimeout = 0x258;

		internal const int DefaultMaxCmdletExecutionTime = 0x258;

		internal const int DefaultMaxResultSetLimit = 0x190;

		internal const int DefaultMaxCmdletExecutionPerRequest = 0x1f4;

		internal const int DefaultMaxUserSchemasInCache = 20;

		internal const int DefaultMaxRunspacesInPerUserCache = 20;

		internal const int DefaultMaxExpandCount = 2;

		internal const int DefaultMaxExpandDepth = 3;

		internal const int MinTimeout = 1;

		internal const int MaxTimeout = 0x24ea00;

		internal const int MinCacheSize = 0;

		internal const int MaxCacheSize = 0x2710;

		internal const int MinCmdletExecution = 1;

		internal const int MaxCmdletExecution = 0x2710;

		internal const int MaxPathLength = 0x104;

		internal const int AssemblyPathLength = 0x3e8;

		internal const int TypeNameLength = 0x3e8;

		internal const int EntitySetLength = 100;

		internal const string DefaultEntitySet = "*";

		[ConfigurationProperty("customAuthorization")]
		public DSConfiguration.CustomAuthzElement CustomAuthorization
		{
			get
			{
				return (DSConfiguration.CustomAuthzElement)base["customAuthorization"];
			}
			set
			{
				base["customAuthorization"] = value;
			}
		}

		[ConfigurationProperty("wcfDataServicesConfig")]
		public DSConfiguration.WcfDataServicesConfig DataServicesConfig
		{
			get
			{
				return (DSConfiguration.WcfDataServicesConfig)base["wcfDataServicesConfig"];
			}
			set
			{
				base["wcfDataServicesConfig"] = value;
			}
		}

		[ConfigurationProperty("commandInvocation")]
		public DSConfiguration.InvocationElement Invocation
		{
			get
			{
				return (DSConfiguration.InvocationElement)base["commandInvocation"];
			}
			set
			{
				base["commandInvocation"] = value;
			}
		}

		[ConfigurationProperty("powershell")]
		public DSConfiguration.PowerShellElement PowerShell
		{
			get
			{
				return (DSConfiguration.PowerShellElement)base["powershell"];
			}
			set
			{
				base["powershell"] = value;
			}
		}

		[ConfigurationProperty("quota")]
		public DSConfiguration.QuotaElement Quotas
		{
			get
			{
				return (DSConfiguration.QuotaElement)base["quota"];
			}
			set
			{
				base["quota"] = value;
			}
		}

		[ConfigurationProperty("resourceMappingFileName", IsRequired=true, DefaultValue="")]
		[StringValidator(MinLength=0, MaxLength=0x104)]
		public string ResourceMappingFileName
		{
			get
			{
				return (string)base["resourceMappingFileName"];
			}
			set
			{
				base["resourceMappingFileName"] = value;
			}
		}

		[ConfigurationProperty("schemaFileName", IsRequired=true, DefaultValue="")]
		[StringValidator(MinLength=0, MaxLength=0x104)]
		public string SchemaFileName
		{
			get
			{
				return (string)base["schemaFileName"];
			}
			set
			{
				base["schemaFileName"] = value;
			}
		}

		public DSConfiguration()
		{
		}

		public int GetResultSetLimit(string entitySet)
		{
			if (this.DataServicesConfig.EntitySets == null || this.DataServicesConfig.EntitySets.Count == 0)
			{
				return 0x190;
			}
			else
			{
				if (this.DataServicesConfig.EntitySets[entitySet] == null)
				{
					if (this.DataServicesConfig.EntitySets["*"] == null)
					{
						return 0x190;
					}
					else
					{
						return this.DataServicesConfig.EntitySets["*"].MaxResults;
					}
				}
				else
				{
					return this.DataServicesConfig.EntitySets[entitySet].MaxResults;
				}
			}
		}

		public static DSConfiguration GetSection(Configuration config)
		{
			DSConfiguration section;
			DSConfiguration dSConfiguration;
			try
			{
				if (config == null)
				{
					section = ConfigurationManager.GetSection("managementOdata") as DSConfiguration;
				}
				else
				{
					section = config.GetSection("managementOdata") as DSConfiguration;
				}
				if (section != null)
				{
					var hasErrors = false; // section.ElementInformation.Errors != null -- Not Implemeted;
					if (hasErrors)
					{
						IEnumerator enumerator = section.ElementInformation.Errors.GetEnumerator();
						try
						{
							if (enumerator.MoveNext())
							{
								Exception current = (Exception)enumerator.Current;
								throw new InvalidConfigurationException(Resources.ConfigError, current);
							}
						}
						finally
						{
							IDisposable disposable = enumerator as IDisposable;
							if (disposable != null)
							{
								disposable.Dispose();
							}
						}
					}
					section.Trace();
					TraceHelper.Current.ValidDataServiceConfiguration();
					dSConfiguration = section;
				}
				else
				{
					if (config != null)
					{
						TraceHelper.Current.DebugMessage(string.Concat("Configuration file ", config.FilePath, " does not contain managementOdata section."));
					}
					object[] objArray = new object[1];
					objArray[0] = "managementOdata";
					string exceptionMessage = ExceptionHelpers.GetExceptionMessage(Resources.ConfigMissing, objArray);
					throw new InvalidConfigurationException(exceptionMessage);
				}
			}
			catch (ConfigurationErrorsException configurationErrorsException1)
			{
				ConfigurationErrorsException configurationErrorsException = configurationErrorsException1;
				throw new InvalidConfigurationException(Resources.ConfigError, configurationErrorsException);
			}
			return dSConfiguration;
		}

		internal string ToTraceMessage(StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine("Current DSConfiguration");
			stringBuilder.AppendLine("Unit for all time related fields is 'Second'");
			foreach (ConfigurationProperty property in this.Properties)
			{
				stringBuilder.AppendLine(string.Concat(property.Name, ": ", base[property].ToString()));
			}
			return stringBuilder.ToString();
		}

		internal void Trace()
		{
			if (TraceHelper.IsEnabled(5))
			{
				TraceHelper.Current.DebugMessage(this.ToTraceMessage(new StringBuilder()));
			}
		}

		public class CustomAuthzElement : ConfigurationElement
		{
			[ConfigurationProperty("assembly", IsRequired=true, DefaultValue="")]
			[StringValidator(MinLength=0, MaxLength=0x3e8)]
			public string Assembly
			{
				get
				{
					return (string)base["assembly"];
				}
				set
				{
					base["assembly"] = value;
				}
			}

			[ConfigurationProperty("type", IsRequired=true, DefaultValue="")]
			[StringValidator(MinLength=0, MaxLength=0x3e8)]
			public string Type
			{
				get
				{
					return (string)base["type"];
				}
				set
				{
					base["type"] = value;
				}
			}

			public CustomAuthzElement()
			{
			}

			public override string ToString()
			{
				string[] type = new string[5];
				type[0] = "{ type: ";
				type[1] = this.Type;
				type[2] = "\nassembly: ";
				type[3] = this.Assembly;
				type[4] = " }";
				return string.Concat(type);
			}
		}

		public class InvocationElement : ConfigurationElement
		{
			[ConfigurationProperty("enabled", IsRequired=false, DefaultValue=false)]
			public bool Enabled
			{
				get
				{
					return (bool)base["enabled"];
				}
				set
				{
					base["enabled"] = value;
				}
			}

			[ConfigurationProperty("lifetimeSec", IsRequired=false, DefaultValue=0x258)]
			[IntegerValidator(MinValue=1, MaxValue=0x24ea00)]
			public int Lifetime
			{
				get
				{
					return (int)base["lifetimeSec"];
				}
				set
				{
					base["lifetimeSec"] = value;
				}
			}

			[ConfigurationProperty("maxActiveInvocationsPerUser", IsRequired=false, DefaultValue=20)]
			public int MaxPerUser
			{
				get
				{
					return (int)base["maxActiveInvocationsPerUser"];
				}
				set
				{
					base["maxActiveInvocationsPerUser"] = value;
				}
			}

			[ConfigurationProperty("maxInvocationResponseChars", IsRequired=false, DefaultValue=0x20000)]
			public int MaxResponseChars
			{
				get
				{
					return (int)base["maxInvocationResponseChars"];
				}
				set
				{
					base["maxInvocationResponseChars"] = value;
				}
			}

			public InvocationElement()
			{
			}

			public override string ToString()
			{
				object[] enabled = new object[9];
				enabled[0] = "{ enabled: ";
				enabled[1] = this.Enabled;
				enabled[2] = "\nlifetime: ";
				enabled[3] = this.Lifetime;
				enabled[4] = "\nmax response chars: ";
				enabled[5] = this.MaxResponseChars;
				enabled[6] = "\nmax invocations/user: ";
				enabled[7] = this.MaxPerUser;
				enabled[8] = " }";
				return string.Concat(enabled);
			}
		}

		public class PowerShellElement : ConfigurationElement
		{
			[ConfigurationProperty("quota")]
			public DSConfiguration.PowerShellQuotaElement Quotas
			{
				get
				{
					return (DSConfiguration.PowerShellQuotaElement)base["quota"];
				}
				set
				{
					base["quota"] = value;
				}
			}

			[ConfigurationProperty("sessionConfiguration")]
			public DSConfiguration.SessionConfigElement SessionConfig
			{
				get
				{
					return (DSConfiguration.SessionConfigElement)base["sessionConfiguration"];
				}
				set
				{
					base["sessionConfiguration"] = value;
				}
			}

			public PowerShellElement()
			{
			}

			public override string ToString()
			{
				string[] str = new string[5];
				str[0] = "{ session config: ";
				str[1] = this.SessionConfig.ToString();
				str[2] = "\nquotas: ";
				str[3] = this.Quotas.ToString();
				str[4] = " }";
				return string.Concat(str);
			}
		}

		public class PowerShellQuotaElement : ConfigurationElement
		{
			[ConfigurationProperty("maxCmdletsPerRequest", IsRequired=false, DefaultValue=0x1f4)]
			[IntegerValidator(MinValue=1, MaxValue=0x2710)]
			public int MaxCmdletsPerRequest
			{
				get
				{
					return (int)base["maxCmdletsPerRequest"];
				}
				set
				{
					base["maxCmdletsPerRequest"] = value;
				}
			}

			[ConfigurationProperty("maxCmdletExecutionTimeSec", IsRequired=false, DefaultValue=0x258)]
			[IntegerValidator(MinValue=1, MaxValue=0x24ea00)]
			public int MaxExecutionTime
			{
				get
				{
					return (int)base["maxCmdletExecutionTimeSec"];
				}
				set
				{
					base["maxCmdletExecutionTimeSec"] = value;
				}
			}

			[ConfigurationProperty("maxRunspacesInPerUserCache", IsRequired=false, DefaultValue=20)]
			[IntegerValidator(MinValue=0, MaxValue=0x2710)]
			public int MaxRunspaces
			{
				get
				{
					return (int)base["maxRunspacesInPerUserCache"];
				}
				set
				{
					base["maxRunspacesInPerUserCache"] = value;
				}
			}

			[ConfigurationProperty("runspaceCacheTimeoutSec", IsRequired=false, DefaultValue=0x258)]
			[IntegerValidator(MinValue=1, MaxValue=0x24ea00)]
			public int RunspaceTimeout
			{
				get
				{
					return (int)base["runspaceCacheTimeoutSec"];
				}
				set
				{
					base["runspaceCacheTimeoutSec"] = value;
				}
			}

			public PowerShellQuotaElement()
			{
			}

			public override string ToString()
			{
				object[] runspaceTimeout = new object[9];
				runspaceTimeout[0] = "{ runspace timeout: ";
				runspaceTimeout[1] = this.RunspaceTimeout;
				runspaceTimeout[2] = "\nmax runspaces: ";
				runspaceTimeout[3] = this.MaxRunspaces;
				runspaceTimeout[4] = "\nmax execution time: ";
				runspaceTimeout[5] = this.MaxExecutionTime;
				runspaceTimeout[6] = "\nmax cmdlets: ";
				runspaceTimeout[7] = this.MaxCmdletsPerRequest;
				runspaceTimeout[8] = " }";
				return string.Concat(runspaceTimeout);
			}
		}

		public class QuotaElement : ConfigurationElement
		{
			[ConfigurationProperty("maxUserSchemasInCache", IsRequired=false, DefaultValue=20)]
			[IntegerValidator(MinValue=0, MaxValue=0x2710)]
			public int MaxUserSchemas
			{
				get
				{
					return (int)base["maxUserSchemasInCache"];
				}
				set
				{
					base["maxUserSchemasInCache"] = value;
				}
			}

			[ConfigurationProperty("userSchemaCacheTimeoutSec", IsRequired=false, DefaultValue=0x258)]
			[IntegerValidator(MinValue=1, MaxValue=0x24ea00)]
			public int UserSchemaTimeout
			{
				get
				{
					return (int)base["userSchemaCacheTimeoutSec"];
				}
				set
				{
					base["userSchemaCacheTimeoutSec"] = value;
				}
			}

			public QuotaElement()
			{
			}

			public override string ToString()
			{
				object[] userSchemaTimeout = new object[5];
				userSchemaTimeout[0] = "{ schema timeout: ";
				userSchemaTimeout[1] = this.UserSchemaTimeout;
				userSchemaTimeout[2] = "\nmax: ";
				userSchemaTimeout[3] = this.MaxUserSchemas;
				userSchemaTimeout[4] = " }";
				return string.Concat(userSchemaTimeout);
			}
		}

		public class SessionConfigElement : ConfigurationElement
		{
			[ConfigurationProperty("assembly", IsRequired=true, DefaultValue="")]
			[StringValidator(MinLength=0, MaxLength=0x3e8)]
			public string Assembly
			{
				get
				{
					return (string)base["assembly"];
				}
				set
				{
					base["assembly"] = value;
				}
			}

			[ConfigurationProperty("type", IsRequired=true, DefaultValue="")]
			[StringValidator(MinLength=0, MaxLength=0x3e8)]
			public string Type
			{
				get
				{
					return (string)base["type"];
				}
				set
				{
					base["type"] = value;
				}
			}

			public SessionConfigElement()
			{
			}

			public override string ToString()
			{
				string[] type = new string[5];
				type[0] = "{ type: ";
				type[1] = this.Type;
				type[2] = "\nassembly: ";
				type[3] = this.Assembly;
				type[4] = " }";
				return string.Concat(type);
			}
		}

		[ConfigurationCollection(typeof(WcfConfigElement), AddItemName="EntitySet", CollectionType=ConfigurationElementCollectionType.BasicMap)]
		public class WcfConfigCollection : ConfigurationElementCollection
		{
			public DSConfiguration.WcfConfigElement this[string name]
			{
				get
				{
					return (DSConfiguration.WcfConfigElement)base.BaseGet(name);
				}
			}

			public WcfConfigCollection()
			{
			}

			public void BaseAdd(ConfigurationElement element)
			{
				base.BaseAdd(element);
			}

			protected override ConfigurationElement CreateNewElement()
			{
				return new DSConfiguration.WcfConfigElement();
			}

			protected override object GetElementKey(ConfigurationElement element)
			{
				return ((DSConfiguration.WcfConfigElement)element).Name;
			}
		}

		public class WcfConfigElement : ConfigurationElement
		{
			[ConfigurationProperty("maxResults", IsRequired=true)]
			public int MaxResults
			{
				get
				{
					return (int)base["maxResults"];
				}
				set
				{
					base["maxResults"] = value;
				}
			}

			[ConfigurationProperty("name", IsRequired=true, IsKey=true, DefaultValue="")]
			[StringValidator(MinLength=0, MaxLength=100)]
			public string Name
			{
				get
				{
					return (string)base["name"];
				}
				set
				{
					base["name"] = value;
				}
			}

			public WcfConfigElement()
			{
			}

			public WcfConfigElement(string name, int maxResults)
			{
				this.Name = name;
				this.MaxResults = maxResults;
			}
		}

		public class WcfDataServicesConfig : ConfigurationElement
		{
			[ConfigurationProperty("enablePublicServerOverride", IsRequired=false, DefaultValue=false)]
			public bool EnablePublicServerOverride
			{
				get
				{
					return (bool)base["enablePublicServerOverride"];
				}
				set
				{
					base["enablePublicServerOverride"] = value;
				}
			}

			[ConfigurationProperty("entitySets", IsDefaultCollection=false, IsRequired=false)]
			public DSConfiguration.WcfConfigCollection EntitySets
			{
				get
				{
					return (DSConfiguration.WcfConfigCollection)base["entitySets"];
				}
			}

			[ConfigurationProperty("maxExpandCount", IsRequired=false, DefaultValue=2)]
			[IntegerValidator(MinValue=1, MaxValue=0x7fffffff)]
			public int MaxExpandCount
			{
				get
				{
					return (int)base["maxExpandCount"];
				}
				set
				{
					base["maxExpandCount"] = value;
				}
			}

			[ConfigurationProperty("maxExpandDepth", IsRequired=false, DefaultValue=3)]
			[IntegerValidator(MinValue=1, MaxValue=0x7fffffff)]
			public int MaxExpandDepth
			{
				get
				{
					return (int)base["maxExpandDepth"];
				}
				set
				{
					base["maxExpandDepth"] = value;
				}
			}

			public WcfDataServicesConfig()
			{
			}

			public override string ToString()
			{
				object[] maxExpandCount = new object[9];
				maxExpandCount[0] = "{ maxExpandCount: ";
				maxExpandCount[1] = this.MaxExpandCount;
				maxExpandCount[2] = "\nmaxExpandDepth: ";
				maxExpandCount[3] = this.MaxExpandDepth;
				maxExpandCount[4] = " \nenablePublicServerOverride";
				maxExpandCount[5] = this.EnablePublicServerOverride;
				maxExpandCount[6] = "\nEntity Sets count: ";
				maxExpandCount[7] = this.EntitySets.Count;
				maxExpandCount[8] = "}";
				return string.Concat(maxExpandCount);
			}
		}
	}
}