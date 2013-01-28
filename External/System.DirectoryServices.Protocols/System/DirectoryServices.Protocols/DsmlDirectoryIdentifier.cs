using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class DsmlDirectoryIdentifier : DirectoryIdentifier
	{
		private Uri uri;

		public Uri ServerUri
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.uri;
			}
		}

		public DsmlDirectoryIdentifier(Uri serverUri)
		{
			if (serverUri != null)
			{
				if (string.Compare(serverUri.Scheme, "http", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(serverUri.Scheme, "https", StringComparison.OrdinalIgnoreCase) == 0)
				{
					this.uri = serverUri;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("DsmlNonHttpUri"));
				}
			}
			else
			{
				throw new ArgumentNullException("serverUri");
			}
		}
	}
}