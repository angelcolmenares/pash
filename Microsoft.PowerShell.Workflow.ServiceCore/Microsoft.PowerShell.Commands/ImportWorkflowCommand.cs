using Microsoft.PowerShell.Workflow;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Remoting;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xaml;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Import", "PSWorkflow", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210606")]
	public class ImportWorkflowCommand : PSCmdlet
	{
		private const int FunctionCacheSize = 0x3e8;

		private const string functionNamePattern = "^[a-zA-Z0-9-_]*$";

		private const string FunctionBodyTemplate = "\r\n\r\n                $PSInputCollection = New-Object 'System.Collections.Generic.List[PSObject]'\r\n            }}\r\n\r\n            process {{\r\n                 if ($PSBoundParameters.ContainsKey('InputObject'))\r\n                 {{\r\n                     $PSInputCollection.Add($InputObject)\r\n                 }}\r\n            }}\r\n            \r\n            end {{\r\n\r\n                {0}\r\n                if ($PSBoundParameters['PSCredential'])\r\n                {{\r\n                    $CredentialTransform = New-Object System.Management.Automation.CredentialAttribute\r\n                    $LocalCredential = $CredentialTransform.Transform($ExecutionContext, $PSCredential)\r\n                    $PSBoundParameters['PSCredential'] = [system.management.automation.pscredential]$LocalCredential\r\n\r\n                    if (!$PSBoundParameters['PSComputerName'] -and !$PSBoundParameters['PSConnectionURI'])\r\n                    {{\r\n                        $PSBoundParameters['PSComputerName'] =  New-Object string @(,'localhost')\r\n                    }}\r\n                }}\r\n\r\n                # Extract the job name if specified\r\n                $jobName = ''\r\n                if ($PSBoundParameters['JobName'])\r\n                {{\r\n                    $jobName = $PSBoundParameters['JobName']\r\n                    [void] $PSBoundParameters.Remove('JobName');\r\n                }}\r\n\r\n                # Extract the PSParameterCollection if specified\r\n                [hashtable[]] $jobSpecifications = @()\r\n                $parametersCollection = $null;\r\n                if ($PSBoundParameters['PSParameterCollection'])\r\n                {{\r\n                    $parameterSCollection = $PSBoundParameters['PSParameterCollection']\r\n                    [void] $PSBoundParameters.Remove('PSParameterCollection');\r\n                }}\r\n\r\n                # Remove the InputObject parameter from the bound parameters collection\r\n                if ($PSBoundParameters['InputObject'])\r\n                {{\r\n                    [void] $PSBoundParameters.Remove('InputObject');\r\n                }}\r\n\r\n                # Remove parameters consumed by this function or PowerShell itself\r\n                $null = $PSBoundParameters.Remove('AsJob')\r\n                $null = $psBoundParameters.Remove('WarningVariable')\r\n                $null = $psBoundParameters.Remove('ErrorVariable')\r\n                $null = $psBoundParameters.Remove('OutVariable')\r\n                $null = $psBoundParameters.Remove('OutBuffer')\r\n                \r\n                # Add parameter to add the path of the workflow module, needed by Import-LocalizedData\r\n                # which uses this as a base path to find localized content files.\r\n                $psBoundParameters['PSWorkflowRoot'] = '{2}'\r\n\r\n                try\r\n                {{\r\n                     $psBoundParameters['PSSenderInfo'] = $PSSenderInfo\r\n                }}\r\n                catch\r\n                {{\r\n                     # Avoid the strict mode runtime exception\r\n                }}\r\n\r\n                $psBoundParameters['PSCurrentDirectory'] = $pwd.Path\r\n\r\n                # Process author-specified metadata which is set using\r\n                # the Private member in the module manifest\r\n                $myCommand = $MyInvocation.MyCommand\r\n                $myModule = $myCommand.Module\r\n                if ($myModule)\r\n                {{\r\n                    # The function was defined in a module so look for \r\n                    # the PrivateData member\r\n                    [Hashtable] $privateData = $myModule.PrivateData -as [Hashtable]\r\n                        \r\n                    if ($privateData)\r\n                    {{\r\n                        # Extract the nested hashtable corresponding to this\r\n                        # command\r\n                        [hashtable] $authorMetadata = $privateData[$myCommand.Name]\r\n                        if ($authorMetadata)\r\n                        {{\r\n                            # Copy the author-supplied hashtable so we can safely\r\n                            # modify it.\r\n                            $authorMetadata = @{{}} + $authorMetadata \r\n                            if ($psBoundParameters['PSPrivateMetadata'])\r\n                            {{\r\n                                # merge in the user-supplied metadata\r\n                                foreach ($pair in $psPrivateMetadata.GetEnumerator())\r\n                                {{\r\n                                    $authorMetadata[$pair.Key] = $pair.Value\r\n                                }}\r\n                            }}\r\n                            # and update the bound parameter to include the merged data\r\n                            $psBoundParameters['PSPrivateMetadata'] = $authorMetadata\r\n                        }}\r\n                    }}\r\n                }}\r\n\r\n                # Add in the input collection if there wasn't one explicitly passed\r\n                # which can only happen through PSParameterCollection               \r\n                if (! $PSBoundParameters['PSInputCollection'])\r\n                {{\r\n                    $PSBoundParameters['PSInputCollection'] = $PSInputCollection\r\n                }}\r\n\r\n                $errorAction = \"Continue\"\r\n                if ($PSBoundParameters['ErrorAction'] -eq \"SilentlyContinue\")\r\n                {{\r\n                    $errorAction = \"SilentlyContinue\"\r\n                }}\r\n\r\n                if($PSBoundParameters['ErrorAction'] -eq \"Ignore\")\r\n                {{\r\n                    $PSBoundParameters['ErrorAction'] = \"SilentlyContinue\"\r\n                    $errorAction = \"SilentlyContinue\"\r\n                }}\r\n\r\n                if ($PSBoundParameters['ErrorAction'] -eq \"Inquire\")\r\n                {{\r\n                    $PSBoundParameters['ErrorAction'] = \"Continue\"\r\n                    $errorAction = \"Continue\"\r\n                }}\r\n\r\n                $warningAction = \"Continue\"\r\n                if ($PSBoundParameters['WarningAction'] -eq \"SilentlyContinue\" -or $PSBoundParameters['WarningAction'] -eq \"Ignore\")\r\n                {{\r\n                    $warningAction = \"SilentlyContinue\"\r\n                }}\r\n\r\n                if ($PSBoundParameters['WarningAction'] -eq \"Inquire\")\r\n                {{\r\n                    $PSBoundParameters['WarningAction'] = \"Continue\"\r\n                    $warningAction = \"Continue\"\r\n                }}\r\n\r\n                #  Create the final parameter collection...\r\n                $finalParameterCollection = $null\r\n                if ($PSParameterCollection -ne $null)\r\n                {{\r\n                    $finalParameterCollection = $PSParameterCollection \r\n                }}\r\n                else\r\n                {{\r\n                    $finalParameterCollection = $PSBoundParameters\r\n                }}\r\n\r\n                try\r\n                {{\r\n                    # Start the workflow and return the job object...        \r\n                    $job = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::StartWorkflowApplication(\r\n                                        $PSCmdlet,\r\n                                        $jobName,\r\n                                        '{1}',\r\n                                        $AsJob,\r\n                                        $parameterCollectionProcessed,\r\n                                        $finalParameterCollection)\r\n                }}\r\n                catch\r\n                {{\r\n                    # extract exception from the error record\r\n                    $e = $_.Exception\r\n                    # this is probably a method invocation exception so we want the inner exception\r\n                    # if it exists\r\n                    if ($e -is [System.Management.Automation.MethodException] -and $e.InnerException)\r\n                    {{\r\n                        $e = $e.InnerException\r\n                    }}\r\n\r\n                    $msg = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::UnableToStartWorkflowMessageMessage -f `\r\n                        $MyInvocation.MyCommand.Name, $e.Message\r\n\r\n                    $newException = New-Object System.Management.Automation.RuntimeException $msg, $e\r\n\r\n                    throw (New-Object System.Management.Automation.ErrorRecord $newException, StartWorkflow.InvalidArgument, InvalidArgument, $finalParameterCollection)\r\n                }}\r\n\r\n                if (-not $AsJob -and $job -ne $null)\r\n                {{\r\n                    try\r\n                    {{\r\n                        Receive-Job -Job $job -Wait -Verbose -Debug -ErrorAction $errorAction -WarningAction $warningAction\r\n\r\n                        $PSCmdlet.InvokeCommand.HasErrors = $job.State -eq 'failed'\r\n                    }}\r\n                    finally\r\n                    {{\r\n                        if($job.State -ne \"Suspended\" -and $job.State -ne \"Stopped\")\r\n                        {{\r\n                            Remove-Job -Job $job -Force\r\n                        }}\r\n                        else\r\n                        {{\r\n                            $job\r\n                        }}\r\n                    }}\r\n                }}\r\n                else\r\n                {{\r\n                    $job\r\n                }}\r\n            }}\r\n";

		private readonly static PowerShellTraceSource Tracer;

		private static Tracer _structuredTracer;

		private string[] _path;

		private string[] _dependentWorkflow;

		private string[] _dependentAssemblies;

		private bool _force;

		private readonly static ConcurrentDictionary<string, ImportWorkflowCommand.FunctionDetails> FunctionCache;

		internal static ConcurrentDictionary<string, WorkflowRuntimeCompilation> compiledAssemblyCache;

		[Alias(new string[] { "PSDependentAssemblies" })]
		[Parameter(Position=2)]
		public string[] DependentAssemblies
		{
			get
			{
				return this._dependentAssemblies;
			}
			set
			{
				this._dependentAssemblies = value;
			}
		}

		[Alias(new string[] { "PSDependentWorkflow" })]
		[Parameter(Position=1)]
		public string[] DependentWorkflow
		{
			get
			{
				return this._dependentWorkflow;
			}
			set
			{
				this._dependentWorkflow = value;
			}
		}

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this._force;
			}
			set
			{
				this._force = value;
			}
		}

		public static string InvalidPSParameterCollectionAdditionalErrorMessage
		{
			get
			{
				return Resources.ParameterCollectionOnlyUsedWithAsJobAndJobName;
			}
		}

		public static string InvalidPSParameterCollectionEntryErrorMessage
		{
			get
			{
				return Resources.AsJobandJobNameNotAllowed;
			}
		}

		public static string ParameterErrorMessage
		{
			get
			{
				return Resources.OnlyOneDefaultParameterCollectionAllowed;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public string[] Path
		{
			get
			{
				return this._path;
			}
			set
			{
				this._path = value;
			}
		}

		public static string UnableToStartWorkflowMessageMessage
		{
			get
			{
				return Resources.UnableToStartWorkflow;
			}
		}

		static ImportWorkflowCommand()
		{
			ImportWorkflowCommand.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			ImportWorkflowCommand._structuredTracer = new Tracer();
			ImportWorkflowCommand.FunctionCache = new ConcurrentDictionary<string, ImportWorkflowCommand.FunctionDetails>(StringComparer.OrdinalIgnoreCase);
			ImportWorkflowCommand.compiledAssemblyCache = new ConcurrentDictionary<string, WorkflowRuntimeCompilation>();
		}

		public ImportWorkflowCommand()
		{
		}

		internal static string AddCommonWfParameters(bool innerfunction, string workflowAttributes)
		{
			string[] strArrays = new string[21];
			strArrays[0] = "[string[]] $PSComputerName";
			strArrays[1] = "[ValidateNotNullOrEmpty()] $PSCredential";
			strArrays[2] = "[uint32] $PSConnectionRetryCount";
			strArrays[3] = "[uint32] $PSConnectionRetryIntervalSec";
			strArrays[4] = "[ValidateRange(1, 2147483)][uint32] $PSRunningTimeoutSec";
			strArrays[5] = "[ValidateRange(1, 2147483)][uint32] $PSElapsedTimeoutSec";
			strArrays[6] = "[bool] $PSPersist";
			strArrays[7] = "[ValidateNotNullOrEmpty()] [System.Management.Automation.Runspaces.AuthenticationMechanism] $PSAuthentication";
			strArrays[8] = "[ValidateNotNullOrEmpty()][System.Management.AuthenticationLevel] $PSAuthenticationLevel";
			strArrays[9] = "[ValidateNotNullOrEmpty()] [string] $PSApplicationName";
			strArrays[10] = "[uint32] $PSPort";
			strArrays[11] = "[switch] $PSUseSSL";
			strArrays[12] = "[ValidateNotNullOrEmpty()] [string] $PSConfigurationName";
			strArrays[13] = "[ValidateNotNullOrEmpty()][string[]] $PSConnectionURI";
			strArrays[14] = "[switch] $PSAllowRedirection";
			strArrays[15] = "[ValidateNotNullOrEmpty()][System.Management.Automation.Remoting.PSSessionOption] $PSSessionOption";
			strArrays[16] = "[ValidateNotNullOrEmpty()] [string] $PSCertificateThumbprint";
			strArrays[17] = "[hashtable] $PSPrivateMetadata";
			strArrays[18] = "[switch] $AsJob";
			strArrays[19] = "[string] $JobName";
			strArrays[20] = "$InputObject";
			string[] strArrays1 = strArrays;
			object[] objArray = new object[1];
			objArray[0] = workflowAttributes;
			string str = string.Format(CultureInfo.InvariantCulture, "\r\n                {0}\r\n                param (\r\n                    {{0}}", objArray);
			if (!innerfunction)
			{
				str = string.Concat(str, "\r\n                    [hashtable[]] $PSParameterCollection");
			}
			string[] strArrays2 = strArrays1;
			for (int i = 0; i < (int)strArrays2.Length; i++)
			{
				string str1 = strArrays2[i];
				if (!str1.Equals("$InputObject"))
				{
					str = string.Concat(str, ",\r\n                    ", str1);
				}
				else
				{
					str = string.Concat(str, ",\r\n                    [Parameter(ValueFromPipeline=$true)]", str1);
				}
			}
			str = string.Concat(str, "\r\n                    )");
			return str;
		}

		internal static string CompileDependentWorkflowsToAssembly(string[] dependentWorkflows, Dictionary<string, string> requiredAssemblies)
		{
			PSWorkflowConfigurationProvider configuration = PSWorkflowRuntime.Instance.Configuration;
			string empty = string.Empty;
			List<int> nums = new List<int>();
			string[] strArrays = dependentWorkflows;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				nums.Add(str.GetHashCode());
			}
			nums.Sort();
			foreach (int num in nums)
			{
				empty = string.Concat(empty, num.ToString(CultureInfo.InvariantCulture));
			}
			WorkflowRuntimeCompilation workflowRuntimeCompilation = null;
			if (ImportWorkflowCommand.compiledAssemblyCache.ContainsKey(empty))
			{
				ImportWorkflowCommand.compiledAssemblyCache.TryGetValue(empty, out workflowRuntimeCompilation);
			}
			if (workflowRuntimeCompilation == null)
			{
				try
				{
					workflowRuntimeCompilation = new WorkflowRuntimeCompilation();
					workflowRuntimeCompilation.Compile(new List<string>(dependentWorkflows), requiredAssemblies);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ImportWorkflowCommand.Tracer.TraceException(exception);
					throw;
				}
				if (ImportWorkflowCommand.compiledAssemblyCache.Keys.Count >= configuration.CompiledAssemblyCacheLimit)
				{
					ImportWorkflowCommand.compiledAssemblyCache.Clear();
				}
				ImportWorkflowCommand.compiledAssemblyCache.TryAdd(empty, workflowRuntimeCompilation);
			}
			if (!workflowRuntimeCompilation.BuildReturnedCode || !File.Exists(workflowRuntimeCompilation.AssemblyPath))
			{
				object[] buildLogPath = new object[1];
				buildLogPath[0] = workflowRuntimeCompilation.BuildLogPath;
				string str1 = string.Format(CultureInfo.CurrentUICulture, Resources.CompilationErrorWhileBuildingWorkflows, buildLogPath);
				throw new InvalidDataException(str1);
			}
			else
			{
				return workflowRuntimeCompilation.AssemblyPath;
			}
		}

		private static Dictionary<string, object> ConvertToParameterDictionary(Hashtable h)
		{
			if (h != null)
			{
				Dictionary<string, object> strs = new Dictionary<string, object>();
				foreach (object key in h.Keys)
				{
					strs.Add((string)key, h[key]);
				}
				return strs;
			}
			else
			{
				return null;
			}
		}

		internal static Activity ConvertXamlToActivity(string xaml)
		{
			Activity activity;
			ImportWorkflowCommand.Tracer.WriteMessage("Trying to convert Xaml into Activity.");
			if (!string.IsNullOrEmpty(xaml))
			{
				StringReader stringReader = new StringReader(xaml);
				try
				{
					activity = ActivityXamlServices.Load(stringReader);
				}
				finally
				{
					stringReader.Dispose();
				}
				return activity;
			}
			else
			{
				ArgumentNullException argumentNullException = new ArgumentNullException("xaml", Resources.XamlNotNull);
				ImportWorkflowCommand.Tracer.TraceException(argumentNullException);
				throw argumentNullException;
			}
		}

		internal static Activity ConvertXamlToActivity(string xaml, string[] dependentWorkflows, Dictionary<string, string> requiredAssemblies, ref string compiledAssemblyPath, ref Assembly compiledAssembly, ref string compiledAssemblyName)
		{
			Activity activity;
			ImportWorkflowCommand._structuredTracer.ImportedWorkflowFromXaml(Guid.Empty, string.Empty);
			ImportWorkflowCommand.Tracer.WriteMessage("Trying to convert Xaml into Activity and using additional xamls assembly.");
			if (!string.IsNullOrEmpty(xaml))
			{
				XamlXmlReaderSettings xamlXmlReaderSetting = new XamlXmlReaderSettings();
				if (string.IsNullOrEmpty(compiledAssemblyPath))
				{
					if (dependentWorkflows != null && (int)dependentWorkflows.Length > 0)
					{
						string assembly = ImportWorkflowCommand.CompileDependentWorkflowsToAssembly(dependentWorkflows, requiredAssemblies);
						xamlXmlReaderSetting.LocalAssembly = Assembly.LoadFrom(assembly);
						compiledAssemblyPath = assembly;
						compiledAssembly = Assembly.LoadFrom(compiledAssemblyPath);
						compiledAssemblyName = compiledAssembly.GetName().Name;
						xamlXmlReaderSetting.LocalAssembly = compiledAssembly;
					}
				}
				else
				{
					if (compiledAssembly == null && string.IsNullOrEmpty(compiledAssemblyName))
					{
						compiledAssembly = Assembly.LoadFrom(compiledAssemblyPath);
						compiledAssemblyName = compiledAssembly.GetName().Name;
					}
					xamlXmlReaderSetting.LocalAssembly = compiledAssembly;
				}
				using (StringReader stringReader = new StringReader(xaml))
				{
					XamlXmlReader xamlXmlReader = new XamlXmlReader(stringReader, xamlXmlReaderSetting);
					activity = ActivityXamlServices.Load(xamlXmlReader);
				}
				ImportWorkflowCommand._structuredTracer.ImportedWorkflowFromXaml(Guid.Empty, string.Empty);
				return activity;
			}
			else
			{
				ArgumentNullException argumentNullException = new ArgumentNullException("xaml", Resources.XamlNotNull);
				ImportWorkflowCommand.Tracer.TraceException(argumentNullException);
				ImportWorkflowCommand._structuredTracer.ErrorImportingWorkflowFromXaml(Guid.Empty, argumentNullException.Message);
				throw argumentNullException;
			}
		}

		public static string CreateFunctionFromXaml(string name, string xaml, Dictionary<string, string> requiredAssemblies, string[] dependentWorkflows, string dependentAssemblyPath, Dictionary<string, ParameterAst> parameterValidation, string modulePath, bool scriptWorkflow, string workflowAttributes)
		{
			bool flag = false;
			if (Regex.IsMatch(name, "^[a-zA-Z0-9-_]*$"))
			{
				WorkflowJobDefinition workflowJobDefinition = null;
				Activity activityFromCache = null;
				if (scriptWorkflow)
				{
					activityFromCache = DefinitionCache.Instance.GetActivityFromCache(xaml, out workflowJobDefinition);
				}
				if (activityFromCache == null)
				{
					workflowJobDefinition = new WorkflowJobDefinition(typeof(WorkflowJobSourceAdapter), name, null, modulePath, dependentWorkflows, dependentAssemblyPath, xaml);
					workflowJobDefinition.IsScriptWorkflow = scriptWorkflow;
					activityFromCache = DefinitionCache.Instance.CompileActivityAndSaveInCache(workflowJobDefinition, null, requiredAssemblies, out flag);
				}
				DynamicActivity dynamicActivity = activityFromCache as DynamicActivity;
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder1 = new StringBuilder();
				Guid instanceId = workflowJobDefinition.InstanceId;
				string str = instanceId.ToString();
				object[] objArray = new object[2];
				objArray[0] = name;
				objArray[1] = str;
				ImportWorkflowCommand.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Generating function for name: {0}, WFGuid: {1}", objArray));
				List<string> strs = new List<string>();
				string str1 = "";
				if (dynamicActivity != null)
				{
					foreach (DynamicActivityProperty property in dynamicActivity.Properties)
					{
						if (typeof(OutArgument).IsAssignableFrom(property.Type) || property.Name.Equals("PSComputerName", StringComparison.OrdinalIgnoreCase) || property.Name.Equals("PSPrivateMetadata", StringComparison.OrdinalIgnoreCase) || property.Name.Equals("InputObject", StringComparison.OrdinalIgnoreCase) || property.Name.Equals("AsJob", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						string str2 = (string)LanguagePrimitives.ConvertTo(property.Type.GetGenericArguments()[0], typeof(string), CultureInfo.InvariantCulture);
						string str3 = "";
						string str4 = "";
						if (parameterValidation == null || !parameterValidation.ContainsKey(property.Name))
						{
							if (typeof(InArgument).IsAssignableFrom(property.Type) && property.Attributes != null)
							{
								foreach (Attribute attribute in property.Attributes)
								{
									if (attribute.TypeId.GetType() != typeof(RequiredArgumentAttribute))
									{
										continue;
									}
									str3 = "[Parameter(Mandatory=$true)] ";
								}
							}
						}
						else
						{
							ParameterAst item = parameterValidation[property.Name];
							foreach (AttributeBaseAst attributeBaseAst in item.Attributes)
							{
								stringBuilder.Append(attributeBaseAst.ToString());
								stringBuilder.Append("\n                    ");
								AttributeAst attributeAst = attributeBaseAst as AttributeAst;
								if (attributeAst == null || !string.Equals(attributeBaseAst.TypeName.Name, "Parameter", StringComparison.OrdinalIgnoreCase))
								{
									if (string.Equals(attributeBaseAst.TypeName.FullName, "System.Management.Automation.CredentialAttribute", StringComparison.OrdinalIgnoreCase))
									{
										continue;
									}
									stringBuilder1.Append(attributeBaseAst.ToString());
									stringBuilder1.Append("\n                    ");
								}
								else
								{
									string str5 = "[Parameter(";
									bool flag1 = true;
									foreach (NamedAttributeArgumentAst namedArgument in attributeAst.NamedArguments)
									{
										if (string.Equals(namedArgument.ArgumentName, "Mandatory", StringComparison.OrdinalIgnoreCase))
										{
											continue;
										}
										if (!string.Equals(namedArgument.ArgumentName, "ValueFromPipeline", StringComparison.OrdinalIgnoreCase) || !string.Equals(namedArgument.Argument.Extent.Text, "$true", StringComparison.OrdinalIgnoreCase))
										{
											if (!flag1)
											{
												str5 = string.Concat(str5, ",");
											}
											flag1 = false;
											str5 = string.Concat(str5, namedArgument.ToString());
										}
										else
										{
											throw new PSInvalidOperationException(Resources.ValueFromPipelineNotSupported);
										}
									}
									str5 = string.Concat(str5, ")]");
									stringBuilder1.Append(str5);
									stringBuilder1.Append("\n                    ");
								}
							}
							if (item.DefaultValue != null)
							{
								str4 = string.Concat(" = ", item.DefaultValue.ToString());
								strs.Add(string.Concat("'", property.Name, "'"));
							}
						}
						object[] objArray1 = new object[4];
						objArray1[0] = str3;
						objArray1[1] = str2;
						objArray1[2] = property.Name;
						objArray1[3] = str4;
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}[{1}] ${2}{3},\n                ", objArray1));
						object[] objArray2 = new object[3];
						objArray2[0] = str2;
						objArray2[1] = property.Name;
						objArray2[2] = str4;
						stringBuilder1.Append(string.Format(CultureInfo.InvariantCulture, "[{0}] ${1}{2},\n                ", objArray2));
					}
					if (strs.Count <= 0)
					{
						str1 = "\r\n                    # None of the workflow parameters had default values\r\n                    $parametersWithDefaults = @()";
					}
					else
					{
						str1 = string.Concat("\r\n                        # Update any parameters that had default values in the workflow\r\n                        $parametersWithDefaults = @(", string.Join(", ", strs.ToArray()), ")\n");
					}
					string str6 = str1;
					string[] strArrays = new string[6];
					strArrays[0] = str6;
					strArrays[1] = "\r\n                    trap { break }\r\n                    $parameterCollectionProcessed = $false\r\n                    $PSParameterCollectionDefaultsMember = $null\r\n                    if ($PSBoundParameters.ContainsKey('PSParameterCollection'))\r\n                    {\r\n                        # validate parameters used with PSParameterCollection\r\n                        foreach ($pa in $PSBoundParameters.Keys)\r\n                        {\r\n                            if ($pa -eq 'JobName' -or $pa -eq 'AsJob' -or $pa -eq 'InputObject' -or $pa -eq 'PSParameterCollection')\r\n                            {\r\n                                continue\r\n                            }\r\n                            $msg = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::InvalidPSParameterCollectionAdditionalErrorMessage;\r\n                            throw (New-Object System.Management.Automation.ErrorRecord $msg, StartWorkflow.InvalidArgument, InvalidArgument, $PSParameterCollection)\r\n                        }\r\n                        $parameterCollectionProcessed = $true\r\n\r\n                        # See if there is a defaults collection, indicated by '*'\r\n                        foreach ($collection in $PSParameterCollection)\r\n                        {\r\n                            if ($collection['PSComputerName'] -eq '*' )\r\n                            {\r\n                                if ($PSParameterCollectionDefaultsMember -ne $null)\r\n                                {\r\n                                    $msg = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::ParameterErrorMessage;\r\n                                    throw ( New-Object System.Management.Automation.ErrorRecord $msg, StartWorkflow.InvalidArgument, InvalidArgument, $PSParameterCollection)\r\n                                }\r\n                                $PSParameterCollectionDefaultsMember = $collection;\r\n                                foreach($parameter in $parametersWithDefaults)\r\n                                {\r\n                                    if(! $collection.ContainsKey($parameter))\r\n                                    {\r\n                                        $collection[$parameter] = (Get-Variable $parameter).Value\r\n                                    }\r\n                                }\r\n                            }\r\n                        }\r\n\r\n                        $PSParameterCollection = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::MergeParameterCollection(\r\n                                        $PSParameterCollection, $PSParameterCollectionDefaultsMember)\r\n\r\n                        # canonicalize each collection...\r\n                        $PSParameterCollection = foreach ( $c in $PSParameterCollection) {\r\n                            if($c.containskey('AsJob') -or $c.containsKey('JobName') -or $c.containsKey('PSParameterCollection') -or $c.containsKey('InputObject'))\r\n                            {\r\n                                    $msg = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::InvalidPSParameterCollectionEntryErrorMessage;\r\n                                    throw ( New-Object System.Management.Automation.ErrorRecord $msg, StartWorkflow.InvalidArgument, InvalidArgument, $PSParameterCollection)\r\n                            }\r\n                            & \"";
					strArrays[2] = name;
					strArrays[3] = "\" @c\r\n                        }\r\n\r\n                        # If there was no '*' collection, added the paramter defaults\r\n                        # to each individual collection if the parameter isn't already there... \r\n                        if (-not $PSParameterCollectionDefaultsMember)\r\n                        {\r\n                            foreach ($collection in $PSParameterCollection)\r\n                            {\r\n                                foreach($parameter in $parametersWithDefaults)\r\n                                {\r\n                                    if(! $collection.ContainsKey($parameter))\r\n                                    {\r\n                                        $collection[$parameter] = (Get-Variable $parameter).Value\r\n                                    }\r\n                                }\r\n                            }\r\n                        }\r\n                    }\r\n                    else\r\n                    {\r\n                        # no PSParameterCollection so add the default values to PSBoundParameters\r\n                        foreach($parameter in $parametersWithDefaults)\r\n                        {\r\n                            if(! $PSBoundParameters.ContainsKey($parameter))\r\n                            {\r\n                                $PSBoundParameters[$parameter] = (Get-Variable $parameter).Value\r\n                            }\r\n                        }\r\n\r\n                        $PSBoundParameters = & \"";
					strArrays[4] = name;
					strArrays[5] = "\" @PSBoundParameters\r\n                    }\r\n                    ";
					str1 = string.Concat(strArrays);
				}
				modulePath = ImportWorkflowCommand.EscapeSingleQuotedString(modulePath);
				string str7 = ImportWorkflowCommand.AddCommonWfParameters(false, workflowAttributes);
				string str8 = ImportWorkflowCommand.AddCommonWfParameters(true, workflowAttributes);
				string str9 = stringBuilder1.ToString();
				str9 = ImportWorkflowCommand.EscapeCurlyBracketString(str9);
				object[] objArray3 = new object[1];
				objArray3[0] = str9;
				string str10 = string.Format(CultureInfo.InvariantCulture, str7, objArray3);
				string str11 = stringBuilder.ToString();
				str11 = ImportWorkflowCommand.EscapeCurlyBracketString(str11);
				object[] objArray4 = new object[1];
				objArray4[0] = string.Concat(str11, "$PSInputCollection");
				string str12 = string.Format(CultureInfo.InvariantCulture, str8, objArray4);
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.Append(str10);
				stringBuilder2.Append("\n        begin {{\n");
				stringBuilder2.Append(string.Concat("                function ", name, "  {{\n"));
				stringBuilder2.Append(str12);
				stringBuilder2.Append("           $PSBoundParameters\n              }}\n");
				stringBuilder2.Append("\r\n\r\n                $PSInputCollection = New-Object 'System.Collections.Generic.List[PSObject]'\r\n            }}\r\n\r\n            process {{\r\n                 if ($PSBoundParameters.ContainsKey('InputObject'))\r\n                 {{\r\n                     $PSInputCollection.Add($InputObject)\r\n                 }}\r\n            }}\r\n            \r\n            end {{\r\n\r\n                {0}\r\n                if ($PSBoundParameters['PSCredential'])\r\n                {{\r\n                    $CredentialTransform = New-Object System.Management.Automation.CredentialAttribute\r\n                    $LocalCredential = $CredentialTransform.Transform($ExecutionContext, $PSCredential)\r\n                    $PSBoundParameters['PSCredential'] = [system.management.automation.pscredential]$LocalCredential\r\n\r\n                    if (!$PSBoundParameters['PSComputerName'] -and !$PSBoundParameters['PSConnectionURI'])\r\n                    {{\r\n                        $PSBoundParameters['PSComputerName'] =  New-Object string @(,'localhost')\r\n                    }}\r\n                }}\r\n\r\n                # Extract the job name if specified\r\n                $jobName = ''\r\n                if ($PSBoundParameters['JobName'])\r\n                {{\r\n                    $jobName = $PSBoundParameters['JobName']\r\n                    [void] $PSBoundParameters.Remove('JobName');\r\n                }}\r\n\r\n                # Extract the PSParameterCollection if specified\r\n                [hashtable[]] $jobSpecifications = @()\r\n                $parametersCollection = $null;\r\n                if ($PSBoundParameters['PSParameterCollection'])\r\n                {{\r\n                    $parameterSCollection = $PSBoundParameters['PSParameterCollection']\r\n                    [void] $PSBoundParameters.Remove('PSParameterCollection');\r\n                }}\r\n\r\n                # Remove the InputObject parameter from the bound parameters collection\r\n                if ($PSBoundParameters['InputObject'])\r\n                {{\r\n                    [void] $PSBoundParameters.Remove('InputObject');\r\n                }}\r\n\r\n                # Remove parameters consumed by this function or PowerShell itself\r\n                $null = $PSBoundParameters.Remove('AsJob')\r\n                $null = $psBoundParameters.Remove('WarningVariable')\r\n                $null = $psBoundParameters.Remove('ErrorVariable')\r\n                $null = $psBoundParameters.Remove('OutVariable')\r\n                $null = $psBoundParameters.Remove('OutBuffer')\r\n                \r\n                # Add parameter to add the path of the workflow module, needed by Import-LocalizedData\r\n                # which uses this as a base path to find localized content files.\r\n                $psBoundParameters['PSWorkflowRoot'] = '{2}'\r\n\r\n                try\r\n                {{\r\n                     $psBoundParameters['PSSenderInfo'] = $PSSenderInfo\r\n                }}\r\n                catch\r\n                {{\r\n                     # Avoid the strict mode runtime exception\r\n                }}\r\n\r\n                $psBoundParameters['PSCurrentDirectory'] = $pwd.Path\r\n\r\n                # Process author-specified metadata which is set using\r\n                # the Private member in the module manifest\r\n                $myCommand = $MyInvocation.MyCommand\r\n                $myModule = $myCommand.Module\r\n                if ($myModule)\r\n                {{\r\n                    # The function was defined in a module so look for \r\n                    # the PrivateData member\r\n                    [Hashtable] $privateData = $myModule.PrivateData -as [Hashtable]\r\n                        \r\n                    if ($privateData)\r\n                    {{\r\n                        # Extract the nested hashtable corresponding to this\r\n                        # command\r\n                        [hashtable] $authorMetadata = $privateData[$myCommand.Name]\r\n                        if ($authorMetadata)\r\n                        {{\r\n                            # Copy the author-supplied hashtable so we can safely\r\n                            # modify it.\r\n                            $authorMetadata = @{{}} + $authorMetadata \r\n                            if ($psBoundParameters['PSPrivateMetadata'])\r\n                            {{\r\n                                # merge in the user-supplied metadata\r\n                                foreach ($pair in $psPrivateMetadata.GetEnumerator())\r\n                                {{\r\n                                    $authorMetadata[$pair.Key] = $pair.Value\r\n                                }}\r\n                            }}\r\n                            # and update the bound parameter to include the merged data\r\n                            $psBoundParameters['PSPrivateMetadata'] = $authorMetadata\r\n                        }}\r\n                    }}\r\n                }}\r\n\r\n                # Add in the input collection if there wasn't one explicitly passed\r\n                # which can only happen through PSParameterCollection               \r\n                if (! $PSBoundParameters['PSInputCollection'])\r\n                {{\r\n                    $PSBoundParameters['PSInputCollection'] = $PSInputCollection\r\n                }}\r\n\r\n                $errorAction = \"Continue\"\r\n                if ($PSBoundParameters['ErrorAction'] -eq \"SilentlyContinue\")\r\n                {{\r\n                    $errorAction = \"SilentlyContinue\"\r\n                }}\r\n\r\n                if($PSBoundParameters['ErrorAction'] -eq \"Ignore\")\r\n                {{\r\n                    $PSBoundParameters['ErrorAction'] = \"SilentlyContinue\"\r\n                    $errorAction = \"SilentlyContinue\"\r\n                }}\r\n\r\n                if ($PSBoundParameters['ErrorAction'] -eq \"Inquire\")\r\n                {{\r\n                    $PSBoundParameters['ErrorAction'] = \"Continue\"\r\n                    $errorAction = \"Continue\"\r\n                }}\r\n\r\n                $warningAction = \"Continue\"\r\n                if ($PSBoundParameters['WarningAction'] -eq \"SilentlyContinue\" -or $PSBoundParameters['WarningAction'] -eq \"Ignore\")\r\n                {{\r\n                    $warningAction = \"SilentlyContinue\"\r\n                }}\r\n\r\n                if ($PSBoundParameters['WarningAction'] -eq \"Inquire\")\r\n                {{\r\n                    $PSBoundParameters['WarningAction'] = \"Continue\"\r\n                    $warningAction = \"Continue\"\r\n                }}\r\n\r\n                #  Create the final parameter collection...\r\n                $finalParameterCollection = $null\r\n                if ($PSParameterCollection -ne $null)\r\n                {{\r\n                    $finalParameterCollection = $PSParameterCollection \r\n                }}\r\n                else\r\n                {{\r\n                    $finalParameterCollection = $PSBoundParameters\r\n                }}\r\n\r\n                try\r\n                {{\r\n                    # Start the workflow and return the job object...        \r\n                    $job = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::StartWorkflowApplication(\r\n                                        $PSCmdlet,\r\n                                        $jobName,\r\n                                        '{1}',\r\n                                        $AsJob,\r\n                                        $parameterCollectionProcessed,\r\n                                        $finalParameterCollection)\r\n                }}\r\n                catch\r\n                {{\r\n                    # extract exception from the error record\r\n                    $e = $_.Exception\r\n                    # this is probably a method invocation exception so we want the inner exception\r\n                    # if it exists\r\n                    if ($e -is [System.Management.Automation.MethodException] -and $e.InnerException)\r\n                    {{\r\n                        $e = $e.InnerException\r\n                    }}\r\n\r\n                    $msg = [Microsoft.PowerShell.Commands.ImportWorkflowCommand]::UnableToStartWorkflowMessageMessage -f `\r\n                        $MyInvocation.MyCommand.Name, $e.Message\r\n\r\n                    $newException = New-Object System.Management.Automation.RuntimeException $msg, $e\r\n\r\n                    throw (New-Object System.Management.Automation.ErrorRecord $newException, StartWorkflow.InvalidArgument, InvalidArgument, $finalParameterCollection)\r\n                }}\r\n\r\n                if (-not $AsJob -and $job -ne $null)\r\n                {{\r\n                    try\r\n                    {{\r\n                        Receive-Job -Job $job -Wait -Verbose -Debug -ErrorAction $errorAction -WarningAction $warningAction\r\n\r\n                        $PSCmdlet.InvokeCommand.HasErrors = $job.State -eq 'failed'\r\n                    }}\r\n                    finally\r\n                    {{\r\n                        if($job.State -ne \"Suspended\" -and $job.State -ne \"Stopped\")\r\n                        {{\r\n                            Remove-Job -Job $job -Force\r\n                        }}\r\n                        else\r\n                        {{\r\n                            $job\r\n                        }}\r\n                    }}\r\n                }}\r\n                else\r\n                {{\r\n                    $job\r\n                }}\r\n            }}\r\n");
				object[] objArray5 = new object[3];
				objArray5[0] = str1;
				objArray5[1] = str;
				objArray5[2] = modulePath;
				string str13 = string.Format(CultureInfo.InvariantCulture, stringBuilder2.ToString(), objArray5);
				str13 = Regex.Replace(str13, "^ *\\#.*$", "", RegexOptions.Multiline);
				return str13;
			}
			else
			{
				object[] objArray6 = new object[1];
				objArray6[0] = name;
				throw new PSArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.WorkflowNameInvalid, objArray6));
			}
		}

		internal static string EscapeCurlyBracketString(string stringContent)
		{
			if (!string.IsNullOrEmpty(stringContent))
			{
				StringBuilder stringBuilder = new StringBuilder(stringContent.Length);
				string str = stringContent;
				for (int i = 0; i < str.Length; i++)
				{
					char chr = str[i];
					stringBuilder.Append(chr);
					if (ImportWorkflowCommand.SpecialCharacters.IsCurlyBracket(chr))
					{
						stringBuilder.Append(chr);
					}
				}
				return stringBuilder.ToString();
			}
			else
			{
				return string.Empty;
			}
		}

		internal static string EscapeSingleQuotedString(string stringContent)
		{
			if (!string.IsNullOrEmpty(stringContent))
			{
				StringBuilder stringBuilder = new StringBuilder(stringContent.Length);
				string str = stringContent;
				for (int i = 0; i < str.Length; i++)
				{
					char chr = str[i];
					stringBuilder.Append(chr);
					if (ImportWorkflowCommand.SpecialCharacters.IsSingleQuote(chr))
					{
						stringBuilder.Append(chr);
					}
				}
				return stringBuilder.ToString();
			}
			else
			{
				return string.Empty;
			}
		}

		private static ImportWorkflowCommand.FunctionDetails GenerateFunctionFromXaml(string name, string xaml, Dictionary<string, string> requiredAssemblies, string[] dependentWorkflows, string dependentAssemblyPath, string resolvedPath)
		{
			if (name != null)
			{
				string directoryName = System.IO.Path.GetDirectoryName(resolvedPath);
				string str = ImportWorkflowCommand.CreateFunctionFromXaml(name, xaml, requiredAssemblies, dependentWorkflows, dependentAssemblyPath, null, directoryName, false, "[CmdletBinding()]");
				ImportWorkflowCommand.FunctionDetails functionDetail = new ImportWorkflowCommand.FunctionDetails();
				functionDetail.Name = name;
				functionDetail.FunctionDefinition = str;
				functionDetail.Xaml = xaml;
				ImportWorkflowCommand.FunctionDetails functionDetail1 = functionDetail;
				return functionDetail1;
			}
			else
			{
				ArgumentNullException argumentNullException = new ArgumentNullException("name");
				ImportWorkflowCommand.Tracer.TraceException(argumentNullException);
				throw argumentNullException;
			}
		}

		public static Hashtable[] MergeParameterCollection(Hashtable[] parameterCollection, Hashtable defaultsParameterCollection)
		{
			string baseObject;
			if (defaultsParameterCollection != null)
			{
				List<Hashtable> hashtables = new List<Hashtable>();
				Hashtable[] hashtableArrays = parameterCollection;
				for (int i = 0; i < (int)hashtableArrays.Length; i++)
				{
					Hashtable hashtables1 = hashtableArrays[i];
					if (hashtables1.ContainsKey("PSComputerName"))
					{
						object item = hashtables1["PSComputerName"];
						PSObject pSObject = item as PSObject;
						if (pSObject == null)
						{
							baseObject = item as string;
						}
						else
						{
							baseObject = pSObject.BaseObject as string;
						}
						if (baseObject != null && baseObject.Equals("*"))
						{
							continue;
						}
					}
					foreach (object key in defaultsParameterCollection.Keys)
					{
						if (key.Equals("PSComputerName") || hashtables1.ContainsKey(key))
						{
							continue;
						}
						hashtables1.Add(key, defaultsParameterCollection[key]);
					}
					hashtables.Add(hashtables1);
				}
				return hashtables.ToArray();
			}
			else
			{
				return parameterCollection;
			}
		}

		protected override void ProcessRecord()
		{
			Collection<string> resolvedProviderPathFromPSPath;
			Collection<string> strs;
			string fileNameWithoutExtension;
			string empty = string.Empty;
			List<string> strs1 = new List<string>();
			if (this._dependentWorkflow != null)
			{
				string[] strArrays = this._dependentWorkflow;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					try
					{
						ProviderInfo providerInfo = null;
						resolvedProviderPathFromPSPath = base.SessionState.Path.GetResolvedProviderPathFromPSPath(str, out providerInfo);
					}
					catch (ItemNotFoundException itemNotFoundException1)
					{
						ItemNotFoundException itemNotFoundException = itemNotFoundException1;
						ErrorRecord errorRecord = new ErrorRecord(itemNotFoundException, "Workflow_XamlFileNotFound", ErrorCategory.OpenError, str);
						base.WriteError(errorRecord);
						ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord);
						continue;
					}
					if (resolvedProviderPathFromPSPath == null || resolvedProviderPathFromPSPath.Count <= 0)
					{
						object[] objArray = new object[1];
						objArray[0] = str;
						string str1 = string.Format(CultureInfo.CurrentUICulture, Resources.NoMatchingWorkflowWasFound, objArray);
						FileNotFoundException fileNotFoundException = new FileNotFoundException(str1);
						ErrorRecord errorRecord1 = new ErrorRecord(fileNotFoundException, "Workflow_NoMatchingWorkflowXamlFileFound", ErrorCategory.ResourceUnavailable, str);
						base.WriteError(errorRecord1);
						ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord1);
					}
					else
					{
						if (resolvedProviderPathFromPSPath.Count != 1 || string.Compare(System.IO.Path.GetExtension(resolvedProviderPathFromPSPath[0]), ".dll", StringComparison.OrdinalIgnoreCase) != 0)
						{
							foreach (string str2 in resolvedProviderPathFromPSPath)
							{
								string extension = System.IO.Path.GetExtension(str2);
								if (string.Compare(extension, ".xaml", StringComparison.OrdinalIgnoreCase) == 0)
								{
									try
									{
										string str3 = File.ReadAllText(str2);
										strs1.Add(str3);
									}
									catch (AccessViolationException accessViolationException1)
									{
										AccessViolationException accessViolationException = accessViolationException1;
										ErrorRecord errorRecord2 = new ErrorRecord(accessViolationException, "Workflow_XAMLfileNotAccessible", ErrorCategory.PermissionDenied, str);
										base.WriteError(errorRecord2);
										ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord2);
									}
								}
								else
								{
									object[] objArray1 = new object[1];
									objArray1[0] = extension;
									InvalidOperationException invalidOperationException = new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Resources.InvalidWorkflowExtension, objArray1));
									ErrorRecord errorRecord3 = new ErrorRecord(invalidOperationException, "Workflows_InvalidWorkflowFileExtension", ErrorCategory.InvalidOperation, str2);
									base.WriteError(errorRecord3);
									ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord3);
								}
							}
						}
						else
						{
							empty = resolvedProviderPathFromPSPath[0];
						}
					}
				}
			}
			if (this._path != null)
			{
				string[] strArrays1 = this._path;
				for (int j = 0; j < (int)strArrays1.Length; j++)
				{
					string str4 = strArrays1[j];
					try
					{
						ProviderInfo providerInfo1 = null;
						strs = base.SessionState.Path.GetResolvedProviderPathFromPSPath(str4, out providerInfo1);
					}
					catch (ItemNotFoundException itemNotFoundException3)
					{
						ItemNotFoundException itemNotFoundException2 = itemNotFoundException3;
						ErrorRecord errorRecord4 = new ErrorRecord(itemNotFoundException2, "Workflow_XamlFileNotFound", ErrorCategory.OpenError, str4);
						base.WriteError(errorRecord4);
						ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord4);
						continue;
					}
					if (strs == null || strs.Count <= 0)
					{
						object[] objArray2 = new object[1];
						objArray2[0] = str4;
						string str5 = string.Format(CultureInfo.CurrentUICulture, Resources.NoMatchingWorkflowWasFound, objArray2);
						FileNotFoundException fileNotFoundException1 = new FileNotFoundException(str5);
						ErrorRecord errorRecord5 = new ErrorRecord(fileNotFoundException1, "Workflow_NoMatchingWorkflowXamlFileFound", ErrorCategory.ResourceUnavailable, str4);
						base.WriteError(errorRecord5);
						ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord5);
					}
					else
					{
						foreach (string str6 in strs)
						{
							string empty1 = string.Empty;
							string extension1 = System.IO.Path.GetExtension(str6);
							if (string.Compare(extension1, ".xaml", StringComparison.OrdinalIgnoreCase) == 0)
							{
								ImportWorkflowCommand.FunctionDetails orAdd = null;
								try
								{
									object[] objArray3 = new object[1];
									objArray3[0] = str6;
									string str7 = string.Format(CultureInfo.CurrentUICulture, Resources.ImportingWorkflowFrom, objArray3);
									base.WriteVerbose(str7);
									fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(str6);
									ImportWorkflowCommand._structuredTracer.ImportingWorkflowFromXaml(Guid.Empty, fileNameWithoutExtension);
									ImportWorkflowCommand.FunctionCache.TryGetValue(str6, out orAdd);
									if (orAdd == null || this._force)
									{
										empty1 = File.ReadAllText(str6);
									}
									else
									{
										this.UpdateFunctionFromXaml(orAdd);
										ImportWorkflowCommand._structuredTracer.ImportedWorkflowFromXaml(Guid.Empty, fileNameWithoutExtension);
										continue;
									}
								}
								catch (AccessViolationException accessViolationException3)
								{
									AccessViolationException accessViolationException2 = accessViolationException3;
									ErrorRecord errorRecord6 = new ErrorRecord(accessViolationException2, "Workflow_XAMLfileNotAccessible", ErrorCategory.PermissionDenied, str4);
									base.WriteError(errorRecord6);
									ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord6);
									continue;
								}
								Dictionary<string, string> strs2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
								if (this.DependentAssemblies != null && (int)this.DependentAssemblies.Length > 0)
								{
									string[] dependentAssemblies = this.DependentAssemblies;
									for (int k = 0; k < (int)dependentAssemblies.Length; k++)
									{
										string str8 = dependentAssemblies[k];
										strs2.Add(System.IO.Path.GetFileNameWithoutExtension(str8), str8);
									}
								}
								ImportWorkflowCommand.FunctionDetails functionDetail = ImportWorkflowCommand.GenerateFunctionFromXaml(fileNameWithoutExtension, empty1, strs2, strs1.ToArray(), empty, str6);
								if (orAdd == null)
								{
									if (ImportWorkflowCommand.FunctionCache.Count == 0x3e8)
									{
										ImportWorkflowCommand.FunctionCache.Clear();
									}
									orAdd = ImportWorkflowCommand.FunctionCache.GetOrAdd(str6, functionDetail);
								}
								else
								{
									ImportWorkflowCommand.FunctionCache.TryUpdate(str6, functionDetail, orAdd);
									orAdd = functionDetail;
								}
								this.UpdateFunctionFromXaml(orAdd);
								ImportWorkflowCommand._structuredTracer.ImportedWorkflowFromXaml(Guid.Empty, fileNameWithoutExtension);
							}
							else
							{
								object[] objArray4 = new object[1];
								objArray4[0] = extension1;
								InvalidOperationException invalidOperationException1 = new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Resources.InvalidWorkflowExtension, objArray4));
								ErrorRecord errorRecord7 = new ErrorRecord(invalidOperationException1, "Workflows_InvalidWorkflowFileExtension", ErrorCategory.InvalidOperation, str6);
								base.WriteError(errorRecord7);
								ImportWorkflowCommand.Tracer.TraceErrorRecord(errorRecord7);
							}
						}
					}
				}
			}
		}

		public static ContainerParentJob StartWorkflowApplication(PSCmdlet command, string jobName, string workflowGuid, bool startAsync, bool parameterCollectionProcessed, Hashtable[] parameters)
		{
			Guid guid = Guid.NewGuid();
			ImportWorkflowCommand._structuredTracer.BeginStartWorkflowApplication(guid);
			if (!string.IsNullOrEmpty(workflowGuid))
			{
				if (command != null)
				{
					if (parameterCollectionProcessed)
					{
						StringBuilder stringBuilder = new StringBuilder();
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder.Append(string.Concat("commandName ='", command.MyInvocation.MyCommand.Name, "'\n"));
						stringBuilder.Append(string.Concat("jobName ='", jobName, "'\n"));
						stringBuilder.Append(string.Concat("workflowGUID = ", workflowGuid, "\n"));
						stringBuilder.Append(string.Concat("startAsync ", startAsync.ToString(), "\n"));
						if (parameters != null)
						{
							Hashtable[] hashtableArrays = parameters;
							for (int i = 0; i < (int)hashtableArrays.Length; i++)
							{
								Hashtable hashtables = hashtableArrays[i];
								stringBuilder.Append("@{");
								bool flag = true;
								foreach (DictionaryEntry dictionaryEntry in hashtables)
								{
									if (dictionaryEntry.Key == null)
									{
										continue;
									}
									if (flag)
									{
										flag = false;
									}
									else
									{
										stringBuilder.Append("'; ");
									}
									stringBuilder.Append(dictionaryEntry.Key.ToString());
									stringBuilder.Append("='");
									if (dictionaryEntry.Value == null)
									{
										continue;
									}
									if (string.Equals(dictionaryEntry.Key.ToString(), "PSComputerName", StringComparison.OrdinalIgnoreCase))
									{
										stringBuilder1.Append(LanguagePrimitives.ConvertTo<string>(dictionaryEntry.Value));
									}
									stringBuilder.Append(dictionaryEntry.Value.ToString());
								}
								stringBuilder.Append("}\n ");
							}
						}
						ImportWorkflowCommand._structuredTracer.ParameterSplattingWasPerformed(stringBuilder.ToString(), stringBuilder1.ToString());
					}
					JobDefinition definition = DefinitionCache.Instance.GetDefinition(new Guid(workflowGuid));
					if (definition != null)
					{
						List<Dictionary<string, object>> dictionaries = new List<Dictionary<string, object>>();
						if (parameters == null || (int)parameters.Length == 0)
						{
							dictionaries.Add(new Dictionary<string, object>());
						}
						else
						{
							if ((int)parameters.Length != 1 || parameterCollectionProcessed)
							{
								Hashtable[] hashtableArrays1 = parameters;
								for (int j = 0; j < (int)hashtableArrays1.Length; j++)
								{
									Hashtable hashtables1 = hashtableArrays1[j];
									if (hashtables1 != null)
									{
										Dictionary<string, object> parameterDictionary = ImportWorkflowCommand.ConvertToParameterDictionary(hashtables1);
										dictionaries.Add(parameterDictionary);
									}
								}
							}
							else
							{
								Hashtable hashtables2 = parameters[0];
								Dictionary<string, object> strs = new Dictionary<string, object>();
								foreach (object key in parameters[0].Keys)
								{
									strs.Add((string)key, hashtables2[key]);
								}
								dictionaries.Add(strs);
							}
						}
						JobInvocationInfo jobInvocationInfo = new JobInvocationInfo(definition, dictionaries);
						jobInvocationInfo.Name = jobName;
						jobInvocationInfo.Command = command.MyInvocation.InvocationName;
						ImportWorkflowCommand._structuredTracer.BeginCreateNewJob(guid);
						ContainerParentJob containerParentJob = command.JobManager.NewJob(jobInvocationInfo) as ContainerParentJob;
						ImportWorkflowCommand._structuredTracer.EndCreateNewJob(guid);
						PSSQMAPI.IncrementWorkflowExecuted(definition.Command);
						if (!startAsync)
						{
							foreach (PSWorkflowJob childJob in containerParentJob.ChildJobs)
							{
								childJob.SynchronousExecution = true;
							}
							containerParentJob.StartJob();
						}
						else
						{
							if (!PSSessionConfigurationData.IsServerManager)
							{
								foreach (PSWorkflowJob pSWorkflowJob in containerParentJob.ChildJobs)
								{
									pSWorkflowJob.EnableStreamUnloadOnPersistentState();
								}
							}
							containerParentJob.StartJobAsync();
						}
						ImportWorkflowCommand._structuredTracer.EndStartWorkflowApplication(guid);
						ImportWorkflowCommand._structuredTracer.TrackingGuidContainerParentJobCorrelation(guid, containerParentJob.InstanceId);
						return containerParentJob;
					}
					else
					{
						object[] cacheSize = new object[1];
						cacheSize[0] = DefinitionCache.Instance.CacheSize;
						InvalidOperationException invalidOperationException = new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Resources.InvalidWorkflowDefinitionState, cacheSize));
						ImportWorkflowCommand.Tracer.TraceException(invalidOperationException);
						throw invalidOperationException;
					}
				}
				else
				{
					ArgumentNullException argumentNullException = new ArgumentNullException("command");
					ImportWorkflowCommand.Tracer.TraceException(argumentNullException);
					throw argumentNullException;
				}
			}
			else
			{
				ArgumentNullException argumentNullException1 = new ArgumentNullException("workflowGuid");
				ImportWorkflowCommand.Tracer.TraceException(argumentNullException1);
				throw argumentNullException1;
			}
		}

		private void UpdateFunctionFromXaml(ImportWorkflowCommand.FunctionDetails details)
		{
			string str = string.Concat("function:\\script:", details.Name);
			ScriptBlock scriptBlock = null;
			PSLanguageMode languageMode = base.SessionState.LanguageMode;
			try
			{
				base.SessionState.LanguageMode = PSLanguageMode.FullLanguage;
				scriptBlock = ScriptBlock.Create(details.FunctionDefinition);
			}
			finally
			{
				base.SessionState.LanguageMode = languageMode;
			}
			WorkflowInfo workflowInfo = new WorkflowInfo(details.Name, "", scriptBlock, details.Xaml, null);
			base.SessionState.InvokeProvider.Item.Set(str, workflowInfo);
		}

		private class FunctionDetails
		{
			internal string FunctionDefinition
			{
				get;
				set;
			}

			internal string Name
			{
				get;
				set;
			}

			internal string Xaml
			{
				get;
				set;
			}

			public FunctionDetails()
			{
			}
		}

		internal static class SpecialCharacters
		{
			public const char quoteSingleLeft = '‘';

			public const char quoteSingleRight = '’';

			public const char quoteSingleBase = '‚';

			public const char quoteReversed = '‛';

			public static bool IsCurlyBracket(char c)
			{
				if (c == '{')
				{
					return true;
				}
				else
				{
					return c == '}';
				}
			}

			public static bool IsSingleQuote(char c)
			{
				if (c == '‘' || c == '’' || c == '‚' || c == '‛')
				{
					return true;
				}
				else
				{
					return c == '\'';
				}
			}
		}
	}
}