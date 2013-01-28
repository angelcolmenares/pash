namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Threading;
    using System.Xml;

    internal class ConfigurationDataFromXML
    {
        internal const string APPBASETOKEN = "applicationbase";
        internal string ApplicationBase;
        internal string AssemblyName;
        internal const string ASSEMBLYTOKEN = "assemblyname";
        internal string ConfigFilePath;
        internal const string CONFIGFILEPATH = "configfilepath";
        internal const string CONFIGFILEPATH_CamelCase = "ConfigFilePath";
        internal Type EndPointConfigurationType;
        internal const string ENDPOINTCONFIGURATIONTYPE = "sessiontype";
        internal string EndPointConfigurationTypeName;
        internal const string HOSTMODE = "hostmode";
        internal string InitializationScriptForOutOfProcessRunspace;
        internal const string INITPARAMETERSTOKEN = "InitializationParameters";
        internal const string MAXPSVERSIONTOKEN = "MaxPSVersion";
        internal const string MAXRCVDCMDSIZETOKEN = "psmaximumreceiveddatasizepercommandmb";
        internal const string MAXRCVDCMDSIZETOKEN_CamelCase = "PSMaximumReceivedDataSizePerCommandMB";
        internal const string MAXRCVDOBJSIZETOKEN = "psmaximumreceivedobjectsizemb";
        internal const string MAXRCVDOBJSIZETOKEN_CamelCase = "PSMaximumReceivedObjectSizeMB";
        internal int? MaxReceivedCommandSizeMB;
        internal int? MaxReceivedObjectSizeMB;
        internal const string MODULESTOIMPORT = "ModulesToImport";
        internal const string NAMETOKEN = "Name";
        internal const string PARAMTOKEN = "Param";
        internal const string PSVERSIONTOKEN = "PSVersion";
        internal const string PSWORKFLOWMODULE = @"%windir%\system32\windowspowershell\v1.0\Modules\PSWorkflow";
        private const string resBaseName = "remotingerroridstrings";
        internal const string SESSIONCONFIGTOKEN = "sessionconfigurationdata";
        internal PSSessionConfigurationData SessionConfigurationData;
        internal const string SHELLCONFIGTYPETOKEN = "pssessionconfigurationtypename";
        internal ApartmentState? ShellThreadApartmentState;
        internal PSThreadOptions? ShellThreadOptions;
        internal string StartupScript;
        internal const string STARTUPSCRIPTTOKEN = "startupscript";
        internal const string THREADAPTSTATETOKEN = "pssessionthreadapartmentstate";
        internal const string THREADOPTIONSTOKEN = "pssessionthreadoptions";
        internal const string VALUETOKEN = "Value";
        internal const string WORKFLOWCOREASSEMBLY = "Microsoft.PowerShell.Workflow.ServiceCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL";
        internal const string WORKFLOWCORETYPENAME = "Microsoft.PowerShell.Workflow.PSWorkflowSessionConfiguration";

        private void AssertValueNotAssigned(string optionName, object originalValue)
        {
            if (originalValue != null)
            {
                throw PSTraceSource.NewArgumentException(optionName, "remotingerroridstrings", "DuplicateInitializationParameterFound", new object[] { optionName, "InitializationParameters" });
            }
        }

        internal static ConfigurationDataFromXML Create(string initializationParameters)
        {
            ConfigurationDataFromXML mxml = new ConfigurationDataFromXML();
            if (!string.IsNullOrEmpty(initializationParameters))
            {
                XmlReaderSettings settings = new XmlReaderSettings {
                    CheckCharacters = false,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    MaxCharactersInDocument = 0x2710L,
                    XmlResolver = null,
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                using (XmlReader reader = XmlReader.Create(new StringReader(initializationParameters), settings))
                {
                    if (reader.ReadToFollowing("InitializationParameters"))
                    {
                        for (bool flag = reader.ReadToDescendant("Param"); flag; flag = reader.ReadToFollowing("Param"))
                        {
                            if (!reader.MoveToAttribute("Name"))
                            {
                                throw PSTraceSource.NewArgumentException(initializationParameters, "remotingerroridstrings", "NoAttributesFoundForParamElement", new object[] { "Name", "Value", "Param" });
                            }
                            string optionName = reader.Value;
                            if (!reader.MoveToAttribute("Value"))
                            {
                                throw PSTraceSource.NewArgumentException(initializationParameters, "remotingerroridstrings", "NoAttributesFoundForParamElement", new object[] { "Name", "Value", "Param" });
                            }
                            string optionValue = reader.Value;
                            mxml.Update(optionName, optionValue);
                        }
                    }
                }
                if (!mxml.MaxReceivedObjectSizeMB.HasValue)
                {
                    mxml.MaxReceivedObjectSizeMB = 0xa00000;
                }
                if (!mxml.MaxReceivedCommandSizeMB.HasValue)
                {
                    mxml.MaxReceivedCommandSizeMB = 0x3200000;
                }
            }
            return mxml;
        }

        internal PSSessionConfiguration CreateEndPointConfigurationInstance()
        {
            try
            {
                return (PSSessionConfiguration) this.EndPointConfigurationType.Assembly.CreateInstance(this.EndPointConfigurationType.FullName);
            }
            catch (TypeLoadException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (MissingMethodException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (TargetInvocationException)
            {
            }
            throw PSTraceSource.NewArgumentException("typeToLoad", "remotingerroridstrings", "UnableToLoadType", new object[] { this.EndPointConfigurationTypeName, "InitializationParameters" });
        }

        private static int? GetIntValueInBytes(string optionValueInMB)
        {
            int? nullable = null;
            try
            {
                double num = (double) LanguagePrimitives.ConvertTo(optionValueInMB, typeof(double), CultureInfo.InvariantCulture);
                nullable = new int?((int) ((num * 1024.0) * 1024.0));
            }
            catch (InvalidCastException)
            {
            }
            if (nullable < 0)
            {
                nullable = null;
            }
            return nullable;
        }

        private void Update(string optionName, string optionValue)
        {
            switch (optionName.ToLower(CultureInfo.InvariantCulture))
            {
                case "applicationbase":
                    this.AssertValueNotAssigned("applicationbase", this.ApplicationBase);
                    this.ApplicationBase = Environment.ExpandEnvironmentVariables(optionValue);
                    return;

                case "assemblyname":
                    this.AssertValueNotAssigned("assemblyname", this.AssemblyName);
                    this.AssemblyName = optionValue;
                    return;

                case "pssessionconfigurationtypename":
                    this.AssertValueNotAssigned("pssessionconfigurationtypename", this.EndPointConfigurationTypeName);
                    this.EndPointConfigurationTypeName = optionValue;
                    return;

                case "startupscript":
                    this.AssertValueNotAssigned("startupscript", this.StartupScript);
                    if (!optionValue.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                    {
                        throw PSTraceSource.NewArgumentException("startupscript", "remotingerroridstrings", "StartupScriptNotCorrect", new object[] { "startupscript" });
                    }
                    this.StartupScript = Environment.ExpandEnvironmentVariables(optionValue);
                    return;

                case "psmaximumreceivedobjectsizemb":
                    this.AssertValueNotAssigned("psmaximumreceivedobjectsizemb", this.MaxReceivedObjectSizeMB);
                    this.MaxReceivedObjectSizeMB = GetIntValueInBytes(optionValue);
                    return;

                case "psmaximumreceiveddatasizepercommandmb":
                    this.AssertValueNotAssigned("psmaximumreceiveddatasizepercommandmb", this.MaxReceivedCommandSizeMB);
                    this.MaxReceivedCommandSizeMB = GetIntValueInBytes(optionValue);
                    return;

                case "pssessionthreadoptions":
                    this.AssertValueNotAssigned("pssessionthreadoptions", this.ShellThreadOptions);
                    this.ShellThreadOptions = new PSThreadOptions?((PSThreadOptions) LanguagePrimitives.ConvertTo(optionValue, typeof(PSThreadOptions), CultureInfo.InvariantCulture));
                    return;

                case "pssessionthreadapartmentstate":
                    this.AssertValueNotAssigned("pssessionthreadapartmentstate", this.ShellThreadApartmentState);
                    this.ShellThreadApartmentState = new ApartmentState?((ApartmentState) LanguagePrimitives.ConvertTo(optionValue, typeof(ApartmentState), CultureInfo.InvariantCulture));
                    return;

                case "sessionconfigurationdata":
                    this.AssertValueNotAssigned("sessionconfigurationdata", this.SessionConfigurationData);
                    this.SessionConfigurationData = PSSessionConfigurationData.Create(optionValue);
                    return;

                case "configfilepath":
                    this.AssertValueNotAssigned("configfilepath", this.ConfigFilePath);
                    this.ConfigFilePath = optionValue.ToString();
                    return;
            }
        }
    }
}

