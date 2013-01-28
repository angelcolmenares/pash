using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class WSManProviderClientCertificateParameters
	{
		private string _issuer;

		private string _subject;

		private Uri _uri;

		private bool _enabled;

		[Parameter]
		public bool Enabled
		{
			get
			{
				return this._enabled;
			}
			set
			{
				this._enabled = value;
			}
		}

		[Parameter(Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public string Issuer
		{
			get
			{
				return this._issuer;
			}
			set
			{
				this._issuer = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Subject
		{
			get
			{
				return this._subject;
			}
			set
			{
				this._subject = value;
			}
		}

		[Parameter]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URI")]
		[ValidateNotNullOrEmpty]
		public Uri URI
		{
			get
			{
				return this._uri;
			}
			set
			{
				this._uri = value;
			}
		}

		public WSManProviderClientCertificateParameters()
		{
			this._subject = "*";
			this._uri = new Uri("*", UriKind.RelativeOrAbsolute);
			this._enabled = true;
		}
	}
}