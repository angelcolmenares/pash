using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using System;
using System.Globalization;

namespace Microsoft.Management.Infrastructure.Options
{
	public class CimSessionOptions : IDisposable, ICloneable
	{
		private bool _disposed;

        public CultureInfo Culture
        {
            get;
            set;
        }

		internal string Protocol
		{
			get;
			private set;
		}

        public TimeSpan Timeout
        {
            get;
            set;
        }

        public CultureInfo UICulture
        {
            get;
            set;
        }

		internal Microsoft.Management.Infrastructure.Native.DestinationOptionsHandle DestinationOptionsHandleOnDemand
		{
			get;set;
		}

		internal Microsoft.Management.Infrastructure.Native.DestinationOptionsHandle DestinationOptionsHandle
		{
			get;set;
		}

		public CimSessionOptions() : this(null, false)
		{
		}

		protected CimSessionOptions(string protocol) : this(protocol, true)
		{

		}

		private CimSessionOptions(string protocol, bool validateProtocol)
		{
			if (!validateProtocol || !string.IsNullOrWhiteSpace(protocol))
			{
				this.Protocol = protocol;
				return;
			}
			else
			{
				throw new ArgumentNullException("protocol");
			}
		}

		internal CimSessionOptions(CimSessionOptions optionsToClone)
		{
			if (optionsToClone != null)
			{
				this.Protocol = optionsToClone.Protocol;
				return;
			}
			else
			{
				throw new ArgumentNullException("optionsToClone");
			}
		}

		public virtual void AddDestinationCredentials(CimCredential credential)
		{
			if (credential != null)
			{
				this.AssertNotDisposed();
				return;
			}
			else
			{
				throw new ArgumentNullException("credential");
			}
		}

		internal void AssertNotDisposed()
		{
			if (!this._disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.ToString());
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
					
				}
				this._disposed = true;
				return;
			}
			else
			{
				return;
			}
		}

		public void SetCustomOption(string optionName, string optionValue)
		{
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				this.AssertNotDisposed();
				return;
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		public void SetCustomOption(string optionName, int optionValue)
		{
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				this.AssertNotDisposed();
				return;
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		object System.ICloneable.Clone()
		{
			return new CimSessionOptions(this);
		}
	}
}