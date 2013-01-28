namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Text;
    using System.Xml;

    public sealed class PSSessionConfigurationData
    {
        private List<string> _modulesToImport;
        private string _privateData;
        internal const string InProcActivityToken = "InProcActivity";
        public static bool IsServerManager;
        internal const string ModulesToImportToken = "modulestoimport";
        private const string NameToken = "Name";
        private const string ParamToken = "Param";
        internal const string PrivateDataToken = "PrivateData";
        private const string ResBaseName = "remotingerroridstrings";
        private const string SessionConfigToken = "SessionConfigurationData";
        private const string ValueToken = "Value";

        private PSSessionConfigurationData()
        {
        }

        private static void AssertValueNotAssigned(string optionName, object originalValue)
        {
            if (originalValue != null)
            {
                throw PSTraceSource.NewArgumentException(optionName, "remotingerroridstrings", "DuplicateInitializationParameterFound", new object[] { optionName, "SessionConfigurationData" });
            }
        }

        internal static PSSessionConfigurationData Create(string configurationData)
        {
            PSSessionConfigurationData data = new PSSessionConfigurationData();
            if (!string.IsNullOrEmpty(configurationData))
            {
                configurationData = Unescape(configurationData);
                XmlReaderSettings settings = new XmlReaderSettings {
                    CheckCharacters = false,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    MaxCharactersInDocument = 0x2710L,
                    XmlResolver = null,
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                using (XmlReader reader = XmlReader.Create(new StringReader(configurationData), settings))
                {
                    if (reader.ReadToFollowing("SessionConfigurationData"))
                    {
                        for (bool flag = reader.ReadToDescendant("Param"); flag; flag = reader.ReadToFollowing("Param"))
                        {
                            if (!reader.MoveToAttribute("Name"))
                            {
                                throw PSTraceSource.NewArgumentException(configurationData, "remotingerroridstrings", "NoAttributesFoundForParamElement", new object[] { "Name", "Value", "Param" });
                            }
                            string a = reader.Value;
                            if (string.Equals(a, "PrivateData", StringComparison.OrdinalIgnoreCase))
                            {
                                if (reader.ReadToFollowing("PrivateData"))
                                {
                                    string str2 = reader.ReadOuterXml();
                                    AssertValueNotAssigned("PrivateData", data._privateData);
                                    data._privateData = str2;
                                }
                            }
                            else
                            {
                                if (!reader.MoveToAttribute("Value"))
                                {
                                    throw PSTraceSource.NewArgumentException(configurationData, "remotingerroridstrings", "NoAttributesFoundForParamElement", new object[] { "Name", "Value", "Param" });
                                }
                                string optionValue = reader.Value;
                                data.Update(a, optionValue);
                            }
                        }
                    }
                }
                data.CreateCollectionIfNecessary();
            }
            return data;
        }

        private void CreateCollectionIfNecessary()
        {
            if (this._modulesToImport == null)
            {
                this._modulesToImport = new List<string>();
            }
        }

        private static string Unescape(string s)
        {
            StringBuilder builder = new StringBuilder(s);
            builder.Replace("&amp;", "&");
            builder.Replace("&lt;", "<");
            builder.Replace("&gt;", ">");
            builder.Replace("&quot;", "\"");
            builder.Replace("&apos;", "'");
            return builder.ToString();
        }

        private void Update(string optionName, string optionValue)
        {
            string str2;
            if (((str2 = optionName.ToLower(CultureInfo.InvariantCulture)) != null) && (str2 == "modulestoimport"))
            {
                AssertValueNotAssigned("modulestoimport", this._modulesToImport);
                this._modulesToImport = new List<string>();
                foreach (string str in optionValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    this._modulesToImport.Add(str.Trim());
                }
            }
        }

        public List<string> ModulesToImport
        {
            get
            {
                return this._modulesToImport;
            }
        }

        public string PrivateData
        {
            get
            {
                return this._privateData;
            }
            internal set
            {
                this._privateData = value;
            }
        }
    }
}

