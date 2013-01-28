namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Eventing;
    using System.Diagnostics.Eventing.Reader;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Reflection;
    using System.Resources;
    using System.Xml;

    [Cmdlet("New", "WinEvent", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217469")]
    public sealed class NewWinEventCommand : PSCmdlet
    {
        private ResourceManager _resourceMgr = new ResourceManager("GetEventResources", Assembly.GetExecutingAssembly());
        private const string DataTag = "data";
        private EventDescriptor? eventDescriptor;
        private int id;
        private bool idSpecified;
        private object[] payload;
        private ProviderMetadata providerMetadata;
        private string providerName;
        private const string TemplateTag = "template";
        private byte version;
        private bool versionSpecified;

        protected override void BeginProcessing()
        {
            this.LoadProvider();
            this.LoadEventDescriptor();
            base.BeginProcessing();
        }

        private static EventDescriptor CreateEventDescriptor(ProviderMetadata providerMetaData, EventMetadata emd)
        {
            long keywords = 0L;
            foreach (EventKeyword keyword in emd.Keywords)
            {
                keywords |= keyword.Value;
            }
            byte channel = 0;
            foreach (EventLogLink link in providerMetaData.LogLinks)
            {
                if (string.Equals(link.LogName, emd.LogLink.LogName, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                channel = (byte) (channel + 1);
            }
            return new EventDescriptor((int) emd.Id, emd.Version, channel, (byte) emd.Level.Value, (byte) emd.Opcode.Value, emd.Task.Value, keywords);
        }

        protected override void EndProcessing()
        {
            if (this.providerMetadata != null)
            {
                this.providerMetadata.Dispose();
            }
            base.EndProcessing();
        }

        private void LoadEventDescriptor()
        {
            if (!this.idSpecified)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("EventIdNotSpecified"), new object[0]), "Id");
            }
            List<EventMetadata> list = new List<EventMetadata>();
            foreach (EventMetadata metadata in this.providerMetadata.Events)
            {
                if (metadata.Id == this.id)
                {
                    list.Add(metadata);
                }
            }
            if (list.Count == 0)
            {
                throw new EventWriteException(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("IncorrectEventId"), new object[] { this.id, this.providerName }));
            }
            EventMetadata emd = null;
            if (!this.versionSpecified && (list.Count == 1))
            {
                emd = list[0];
            }
            else
            {
                if (!this.versionSpecified)
                {
                    throw new EventWriteException(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("VersionNotSpecified"), new object[] { this.id, this.providerName }));
                }
                foreach (EventMetadata metadata3 in list)
                {
                    if (metadata3.Version == this.version)
                    {
                        emd = metadata3;
                        break;
                    }
                }
                if (emd == null)
                {
                    throw new EventWriteException(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("IncorrectEventVersion"), new object[] { this.version, this.id, this.providerName }));
                }
            }
            this.VerifyTemplate(emd);
            this.eventDescriptor = new EventDescriptor?(CreateEventDescriptor(this.providerMetadata, emd));
        }

        private void LoadProvider()
        {
            if (string.IsNullOrEmpty(this.providerName))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("ProviderNotSpecified"), new object[0]), "ProviderName");
            }
            using (EventLogSession session = new EventLogSession())
            {
                foreach (string str in session.GetProviderNames())
                {
                    if (string.Equals(str, this.providerName, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            this.providerMetadata = new ProviderMetadata(str);
                            goto Label_00D2;
                        }
                        catch (EventLogException exception)
                        {
                            throw new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("ProviderMetadataUnavailable"), new object[] { str, exception.Message }), exception);
                        }
                    }
                }
            }
        Label_00D2:
            if (this.providerMetadata == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("NoProviderFound"), new object[] { this.providerName }));
            }
        }

        protected override void ProcessRecord()
        {
            using (EventProvider provider = new EventProvider(this.providerMetadata.Id))
            {
                EventDescriptor eventDescriptor = this.eventDescriptor.Value;
                if ((this.payload != null) && (this.payload.Length > 0))
                {
                    for (int i = 0; i < this.payload.Length; i++)
                    {
                        if (this.payload[i] == null)
                        {
                            this.payload[i] = string.Empty;
                        }
                    }
                    provider.WriteEvent(ref eventDescriptor, this.payload);
                }
                else
                {
                    provider.WriteEvent(ref eventDescriptor, new object[0]);
                }
            }
            base.ProcessRecord();
        }

        private bool VerifyTemplate(EventMetadata emd)
        {
            if (emd.Template != null)
            {
                XmlReaderSettings settings = new XmlReaderSettings {
                    CheckCharacters = false,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    MaxCharactersInDocument = 0L,
                    XmlResolver = null,
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                int num = 0;
                using (XmlReader reader = XmlReader.Create(new StringReader(emd.Template), settings))
                {
                    if (reader.ReadToFollowing("template"))
                    {
                        for (bool flag = reader.ReadToDescendant("data"); flag; flag = reader.ReadToFollowing("data"))
                        {
                            num++;
                        }
                    }
                }
                if (((this.payload == null) && (num != 0)) || ((this.payload != null) && (this.payload.Length != num)))
                {
                    string text = string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("PayloadMismatch"), new object[] { this.id, emd.Template });
                    base.WriteWarning(text);
                    return false;
                }
            }
            return true;
        }

        [Parameter(Position=1, Mandatory=true, ParameterSetName="__AllParameterSets")]
        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
                this.idSpecified = true;
            }
        }

        [AllowEmptyCollection, Parameter(Position=2, Mandatory=false, ParameterSetName="__AllParameterSets"), SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Target="Microsoft.PowerShell.Commands", Justification="A string[] is required here because that is the type Powershell supports")]
        public object[] Payload
        {
            get
            {
                return this.payload;
            }
            set
            {
                this.payload = value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ParameterSetName="__AllParameterSets")]
        public string ProviderName
        {
            get
            {
                return this.providerName;
            }
            set
            {
                this.providerName = value;
            }
        }

        [Parameter(Mandatory=false, ParameterSetName="__AllParameterSets")]
        public byte Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
                this.versionSpecified = true;
            }
        }
    }
}

