using System;
using System.Management.Automation;

namespace Microsoft.DnsClient.Commands
{
	[Cmdlet("Resolve", "DnsName")]
	public class ResolveDnsName : Cmdlet
	{
		private RecordType _Type;

		[Parameter]
		public SwitchParameter CacheOnly
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter DnsOnly
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter DnssecCd
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter DnssecOk
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter LlmnrFallback
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter LlmnrNetbiosOnly
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter LlmnrOnly
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter NetbiosFallback
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter NoHostsFile
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter NoIdn
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter NoRecursion
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter QuickTimeout
		{
			get;
			set;
		}

		[Parameter]
		[ValidateCount(0, 5)]
		public string[] Server
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter TcpOnly
		{
			get;
			set;
		}

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
		[ValidateRange(0, 0xff)]
		public RecordType Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}

		public ResolveDnsName()
		{
		}

		protected override void ProcessRecord()
		{
			if ((this.Type == RecordType.OPT || this.Type == RecordType.DS || this.Type == RecordType.RRSIG || this.Type == RecordType.NSEC || this.Type == RecordType.DNSKEY || this.Type == RecordType.DHCID || this.Type == RecordType.NSEC3 || this.Type == RecordType.NSEC3PARAM) && (Environment.OSVersion.Version.Major < 6 || Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 0))
			{
				throw new PlatformNotSupportedException();
			}
			else
			{
				try
				{
					string reverseLookup = utility.ConvertNumericToReverseLookup(this.Name);
					if (this.Type == RecordType.UNKNOWN)
					{
						this.Type = RecordType.PTR;
					}
					this.Name = reverseLookup;
				}
				catch
				{
					if (this.Type == RecordType.UNKNOWN)
					{
						this.Type = RecordType.UNKNOWN;
					}
				}
				api.QueryParameters dnsOnly = new api.QueryParameters();
				dnsOnly.DnsOnly = this.DnsOnly;
				dnsOnly.CacheOnly = this.CacheOnly;
				dnsOnly.DnssecOk = this.DnssecOk;
				dnsOnly.DnssecCd = this.DnssecCd;
				dnsOnly.NoHostsFile = this.NoHostsFile;
				dnsOnly.LlmnrNetbiosOnly = this.LlmnrNetbiosOnly;
				dnsOnly.LlmnrFallback = this.LlmnrFallback;
				dnsOnly.LlmnrOnly = this.LlmnrOnly;
				dnsOnly.NetbiosFallback = this.NetbiosFallback;
				dnsOnly.NoIdn = this.NoIdn;
				dnsOnly.NoRecursion = this.NoRecursion;
				dnsOnly.QuickTimeout = this.QuickTimeout;
				dnsOnly.TcpOnly = this.TcpOnly;
				api.SendDnsQuery(this, this.Name, this.Type, this.Server, dnsOnly);
				return;
			}
		}
	}
}