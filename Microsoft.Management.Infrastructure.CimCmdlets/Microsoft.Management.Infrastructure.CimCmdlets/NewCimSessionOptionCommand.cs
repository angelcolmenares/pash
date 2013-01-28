using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("New", "CimSessionOption", DefaultParameterSetName="ProtocolTypeSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227969")]
	[OutputType(new Type[] { typeof(CimSessionOptions) })]
	public sealed class NewCimSessionOptionCommand : CimBaseCommand
	{
		internal const string nameNoEncryption = "NoEncryption";

		internal const string nameSkipCACheck = "SkipCACheck";

		internal const string nameSkipCNCheck = "SkipCNCheck";

		internal const string nameSkipRevocationCheck = "SkipRevocationCheck";

		internal const string nameEncodePortInServicePrincipalName = "EncodePortInServicePrincipalName";

		internal const string nameEncoding = "Encoding";

		internal const string nameHttpPrefix = "HttpPrefix";

		internal const string nameMaxEnvelopeSizeKB = "MaxEnvelopeSizeKB";

		internal const string nameProxyAuthentication = "ProxyAuthentication";

		internal const string nameProxyCertificateThumbprint = "ProxyCertificateThumbprint";

		internal const string nameProxyCredential = "ProxyCredential";

		internal const string nameProxyType = "ProxyType";

		internal const string nameUseSsl = "UseSsl";

		internal const string nameImpersonation = "Impersonation";

		internal const string namePacketIntegrity = "PacketIntegrity";

		internal const string namePacketPrivacy = "PacketPrivacy";

		internal const string nameProtocol = "Protocol";

		private SwitchParameter noEncryption;

		private bool noEncryptionSet;

		private SwitchParameter skipCACheck;

		private bool skipCACheckSet;

		private SwitchParameter skipCNCheck;

		private bool skipCNCheckSet;

		private SwitchParameter skipRevocationCheck;

		private bool skipRevocationCheckSet;

		private SwitchParameter encodeportinserviceprincipalname;

		private bool encodeportinserviceprincipalnameSet;

		private PacketEncoding encoding;

		private bool encodingSet;

		private Uri httpprefix;

		private uint maxenvelopesizekb;

		private bool maxenvelopesizekbSet;

		private PasswordAuthenticationMechanism proxyAuthentication;

		private bool proxyauthenticationSet;

		private string proxycertificatethumbprint;

		private PSCredential proxycredential;

		private ProxyType proxytype;

		private bool proxytypeSet;

		private SwitchParameter usessl;

		private bool usesslSet;

		private ImpersonationType impersonation;

		private bool impersonationSet;

		private SwitchParameter packetintegrity;

		private bool packetintegritySet;

		private SwitchParameter packetprivacy;

		private bool packetprivacySet;

		private ProtocolType protocol;

		private CultureInfo uiculture;

		private CultureInfo culture;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public CultureInfo Culture
		{
			get
			{
				return this.culture;
			}
			set
			{
				this.culture = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public SwitchParameter EncodePortInServicePrincipalName
		{
			get
			{
				return this.encodeportinserviceprincipalname;
			}
			set
			{
				this.encodeportinserviceprincipalname = value;
				this.encodeportinserviceprincipalnameSet = true;
				base.SetParameter(value, "EncodePortInServicePrincipalName");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public PacketEncoding Encoding
		{
			get
			{
				return this.encoding;
			}
			set
			{
				this.encoding = value;
				this.encodingSet = true;
				base.SetParameter(value, "Encoding");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public Uri HttpPrefix
		{
			get
			{
				return this.httpprefix;
			}
			set
			{
				this.httpprefix = value;
				base.SetParameter(value, "HttpPrefix");
			}
		}

		[Parameter(ParameterSetName="DcomParameterSet")]
		public ImpersonationType Impersonation
		{
			get
			{
				return this.impersonation;
			}
			set
			{
				this.impersonation = value;
				this.impersonationSet = true;
				base.SetParameter(value, "Impersonation");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public uint MaxEnvelopeSizeKB
		{
			get
			{
				return this.maxenvelopesizekb;
			}
			set
			{
				this.maxenvelopesizekb = value;
				this.maxenvelopesizekbSet = true;
				base.SetParameter(value, "MaxEnvelopeSizeKB");
			}
		}

		[Parameter(ParameterSetName="WSManParameterSet")]
		public SwitchParameter NoEncryption
		{
			get
			{
				return this.noEncryption;
			}
			set
			{
				this.noEncryption = value;
				this.noEncryptionSet = true;
				base.SetParameter(value, "NoEncryption");
			}
		}

		[Parameter(ParameterSetName="DcomParameterSet")]
		public SwitchParameter PacketIntegrity
		{
			get
			{
				return this.packetintegrity;
			}
			set
			{
				this.packetintegrity = value;
				this.packetintegritySet = true;
				base.SetParameter(value, "PacketIntegrity");
			}
		}

		[Parameter(ParameterSetName="DcomParameterSet")]
		public SwitchParameter PacketPrivacy
		{
			get
			{
				return this.packetprivacy;
			}
			set
			{
				this.packetprivacy = value;
				this.packetprivacySet = true;
				base.SetParameter(value, "PacketPrivacy");
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ProtocolTypeSet")]
		public ProtocolType Protocol
		{
			get
			{
				return this.protocol;
			}
			set
			{
				this.protocol = value;
				base.SetParameter(value, "Protocol");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public PasswordAuthenticationMechanism ProxyAuthentication
		{
			get
			{
				return this.proxyAuthentication;
			}
			set
			{
				this.proxyAuthentication = value;
				this.proxyauthenticationSet = true;
				base.SetParameter(value, "ProxyAuthentication");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public string ProxyCertificateThumbprint
		{
			get
			{
				return this.proxycertificatethumbprint;
			}
			set
			{
				this.proxycertificatethumbprint = value;
				base.SetParameter(value, "ProxyCertificateThumbprint");
			}
		}

		[Credential]
		[Parameter(ParameterSetName="WSManParameterSet")]
		public PSCredential ProxyCredential
		{
			get
			{
				return this.proxycredential;
			}
			set
			{
				this.proxycredential = value;
				base.SetParameter(value, "ProxyCredential");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public ProxyType ProxyType
		{
			get
			{
				return this.proxytype;
			}
			set
			{
				this.proxytype = value;
				this.proxytypeSet = true;
				base.SetParameter(value, "ProxyType");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public SwitchParameter SkipCACheck
		{
			get
			{
				return this.skipCACheck;
			}
			set
			{
				this.skipCACheck = value;
				this.skipCACheckSet = true;
				base.SetParameter(value, "SkipCACheck");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public SwitchParameter SkipCNCheck
		{
			get
			{
				return this.skipCNCheck;
			}
			set
			{
				this.skipCNCheck = value;
				this.skipCNCheckSet = true;
				base.SetParameter(value, "SkipCNCheck");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public SwitchParameter SkipRevocationCheck
		{
			get
			{
				return this.skipRevocationCheck;
			}
			set
			{
				this.skipRevocationCheck = value;
				this.skipRevocationCheckSet = true;
				base.SetParameter(value, "SkipRevocationCheck");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public CultureInfo UICulture
		{
			get
			{
				return this.uiculture;
			}
			set
			{
				this.uiculture = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="WSManParameterSet")]
		public SwitchParameter UseSsl
		{
			get
			{
				return this.usessl;
			}
			set
			{
				this.usessl = value;
				this.usesslSet = true;
				base.SetParameter(value, "UseSsl");
			}
		}

		static NewCimSessionOptionCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("NoEncryption", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("SkipCACheck", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("SkipCNCheck", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("SkipRevocationCheck", parameterDefinitionEntries3);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries4 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("EncodePortInServicePrincipalName", parameterDefinitionEntries4);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries5 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("Encoding", parameterDefinitionEntries5);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries6 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("HttpPrefix", parameterDefinitionEntries6);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries7 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("MaxEnvelopeSizeKB", parameterDefinitionEntries7);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries8 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("ProxyAuthentication", parameterDefinitionEntries8);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries9 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("ProxyCertificateThumbprint", parameterDefinitionEntries9);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries10 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries10.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("ProxyCredential", parameterDefinitionEntries10);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries11 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries11.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("ProxyType", parameterDefinitionEntries11);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries12 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries12.Add(new ParameterDefinitionEntry("WSManParameterSet", false));
			strs.Add("UseSsl", parameterDefinitionEntries12);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries13 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries13.Add(new ParameterDefinitionEntry("DcomParameterSet", false));
			strs.Add("Impersonation", parameterDefinitionEntries13);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries14 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries14.Add(new ParameterDefinitionEntry("DcomParameterSet", false));
			strs.Add("PacketIntegrity", parameterDefinitionEntries14);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries15 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries15.Add(new ParameterDefinitionEntry("DcomParameterSet", false));
			strs.Add("PacketPrivacy", parameterDefinitionEntries15);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries16 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries16.Add(new ParameterDefinitionEntry("ProtocolTypeSet", true));
			strs.Add("Protocol", parameterDefinitionEntries16);
			NewCimSessionOptionCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("ProtocolTypeSet", new ParameterSetEntry(1, true));
			strs1.Add("DcomParameterSet", new ParameterSetEntry(0));
			strs1.Add("WSManParameterSet", new ParameterSetEntry(0));
			NewCimSessionOptionCommand.parameterSets = strs1;
		}

		public NewCimSessionOptionCommand() : base(NewCimSessionOptionCommand.parameters, NewCimSessionOptionCommand.parameterSets)
		{
			DebugHelper.WriteLogEx();
		}

		protected override void BeginProcessing()
		{
			this.CmdletOperation = new CmdletOperationBase(this);
			base.AtBeginProcess = false;
		}

		internal DComSessionOptions CreateDComSessionOptions()
		{
			DComSessionOptions dComSessionOption = new DComSessionOptions();
			if (!this.impersonationSet)
			{
				dComSessionOption.Impersonation = ImpersonationType.Impersonate;
			}
			else
			{
				dComSessionOption.Impersonation = this.Impersonation;
				this.impersonationSet = false;
			}
			if (!this.packetintegritySet)
			{
				dComSessionOption.PacketIntegrity = true;
			}
			else
			{
				dComSessionOption.PacketIntegrity = this.packetintegrity;
				this.packetintegritySet = false;
			}
			if (!this.packetprivacySet)
			{
				dComSessionOption.PacketPrivacy = true;
			}
			else
			{
				dComSessionOption.PacketPrivacy = this.PacketPrivacy;
				this.packetprivacySet = false;
			}
			return dComSessionOption;
		}

		internal WSManSessionOptions CreateWSMANSessionOptions()
		{
			WSManSessionOptions wSManSessionOption = new WSManSessionOptions();
			if (!this.noEncryptionSet)
			{
				wSManSessionOption.NoEncryption = false;
			}
			else
			{
				wSManSessionOption.NoEncryption = true;
				this.noEncryptionSet = false;
			}
			if (!this.skipCACheckSet)
			{
				wSManSessionOption.CertCACheck = true;
			}
			else
			{
				wSManSessionOption.CertCACheck = false;
				this.skipCACheckSet = false;
			}
			if (!this.skipCNCheckSet)
			{
				wSManSessionOption.CertCNCheck = true;
			}
			else
			{
				wSManSessionOption.CertCNCheck = false;
				this.skipCNCheckSet = false;
			}
			if (!this.skipRevocationCheckSet)
			{
				wSManSessionOption.CertRevocationCheck = true;
			}
			else
			{
				wSManSessionOption.CertRevocationCheck = false;
				this.skipRevocationCheckSet = false;
			}
			if (!this.encodeportinserviceprincipalnameSet)
			{
				wSManSessionOption.EncodePortInServicePrincipalName = false;
			}
			else
			{
				wSManSessionOption.EncodePortInServicePrincipalName = this.EncodePortInServicePrincipalName;
				this.encodeportinserviceprincipalnameSet = false;
			}
			if (!this.encodingSet)
			{
				wSManSessionOption.PacketEncoding = PacketEncoding.Utf8;
			}
			else
			{
				wSManSessionOption.PacketEncoding = this.Encoding;
			}
			if (this.HttpPrefix != null)
			{
				wSManSessionOption.HttpUrlPrefix = this.HttpPrefix;
			}
			if (!this.maxenvelopesizekbSet)
			{
				wSManSessionOption.MaxEnvelopeSize = 0;
			}
			else
			{
				wSManSessionOption.MaxEnvelopeSize = this.MaxEnvelopeSizeKB;
			}
			if (!string.IsNullOrWhiteSpace(this.ProxyCertificateThumbprint))
			{
				CimCredential cimCredential = new CimCredential(CertificateAuthenticationMechanism.Default, this.ProxyCertificateThumbprint);
				wSManSessionOption.AddProxyCredentials(cimCredential);
			}
			if (this.proxyauthenticationSet)
			{
				this.proxyauthenticationSet = false;
				DebugHelper.WriteLogEx("create credential", 1);
				CimCredential cimCredential1 = base.CreateCimCredentials(this.ProxyCredential, this.ProxyAuthentication, "New-CimSessionOption", "ProxyAuthentication");
				if (cimCredential1 != null)
				{
					try
					{
						DebugHelper.WriteLogEx("Add proxy credential", 1);
						wSManSessionOption.AddProxyCredentials(cimCredential1);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						DebugHelper.WriteLogEx(exception.ToString(), 1);
						throw exception;
					}
				}
			}
			if (!this.proxytypeSet)
			{
				wSManSessionOption.ProxyType = ProxyType.WinHttp;
			}
			else
			{
				wSManSessionOption.ProxyType = this.ProxyType;
				this.proxytypeSet = false;
			}
			if (!this.usesslSet)
			{
				wSManSessionOption.UseSsl = false;
			}
			else
			{
				wSManSessionOption.UseSsl = this.UseSsl;
				this.usesslSet = false;
			}
			wSManSessionOption.DestinationPort = 0;
			return wSManSessionOption;
		}

		protected override void EndProcessing()
		{
		}

		protected override void ProcessRecord()
		{
			CimSessionOptions culture;
			base.CheckParameterSet();
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName == null)
			{
				return;
			}
			else
			{
				if (str == "WSManParameterSet")
				{
					culture = this.CreateWSMANSessionOptions();
				}
				else
				{
					if (str == "DcomParameterSet")
					{
						culture = this.CreateDComSessionOptions();
					}
					else
					{
						if (str == "ProtocolTypeSet")
						{
							ProtocolType protocol = this.Protocol;
							switch (protocol)
							{
								case ProtocolType.Dcom:
								{
									culture = this.CreateDComSessionOptions();
									break;
								}
								case ProtocolType.Wsman:
								{
									culture = this.CreateWSMANSessionOptions();
									break;
								}
								default:
								{
									culture = this.CreateWSMANSessionOptions();
									break;
								}
							}
						}
						else
						{
							return;
						}
					}
				}
				if (culture != null)
				{
					if (this.Culture != null)
					{
						culture.Culture = this.Culture;
					}
					if (this.UICulture != null)
					{
						culture.UICulture = this.UICulture;
					}
					base.WriteObject(culture);
				}
				return;
			}
		}
	}
}