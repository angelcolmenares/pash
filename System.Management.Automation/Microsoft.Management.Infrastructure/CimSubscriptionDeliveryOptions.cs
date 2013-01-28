using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Options
{
	public class CimSubscriptionDeliveryOptions : IDisposable, ICloneable
	{
		private SubscriptionDeliveryOptionsHandle _subscriptionDeliveryOptionsHandle;

		private bool _disposed;

		internal SubscriptionDeliveryOptionsHandle SubscriptionDeliveryOptionsHandle
		{
			get
			{
				this.AssertNotDisposed();
				return this._subscriptionDeliveryOptionsHandle;
			}
		}

		public CimSubscriptionDeliveryOptions() : this((CimSubscriptionDeliveryType)2)
		{
		}

		public CimSubscriptionDeliveryOptions(CimSubscriptionDeliveryType types)
		{
			this.Initialize(types);
		}

		public CimSubscriptionDeliveryOptions(CimSubscriptionDeliveryOptions optionsToClone)
		{
			SubscriptionDeliveryOptionsHandle subscriptionDeliveryOptionsHandle = null;
			if (optionsToClone != null)
			{
				MiResult miResult = MiResult.OK; //TODO: SubscriptionDeliveryOptionsMethods.Clone(optionsToClone.SubscriptionDeliveryOptionsHandle, out subscriptionDeliveryOptionsHandle);
				CimException.ThrowIfMiResultFailure(miResult);
				this._subscriptionDeliveryOptionsHandle = subscriptionDeliveryOptionsHandle;
				return;
			}
			else
			{
				throw new ArgumentNullException("optionsToClone");
			}
		}

		public void AddCredentials(string optionName, CimCredential optionValue, uint flags)
		{
			if (string.IsNullOrWhiteSpace(optionName) || optionValue == null)
			{
				throw new ArgumentNullException("optionName");
			}
			else
			{
				if (optionValue != null)
				{
					this.AssertNotDisposed();
					MiResult miResult = MiResult.OK; //TODO: SubscriptionDeliveryOptionsMethods.AddCredentials(this._subscriptionDeliveryOptionsHandle, optionName, optionValue.GetCredential(), flags);
					CimException.ThrowIfMiResultFailure(miResult);
					return;
				}
				else
				{
					throw new ArgumentNullException("optionValue");
				}
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
					this._subscriptionDeliveryOptionsHandle.Dispose();
					this._subscriptionDeliveryOptionsHandle = null;
				}
				this._disposed = true;
				return;
			}
			else
			{
				return;
			}
		}

		private void Initialize(CimSubscriptionDeliveryType types)
		{
			SubscriptionDeliveryOptionsHandle subscriptionDeliveryOptionsHandle = null;
			MiResult miResult = ApplicationMethods.NewSubscriptionDeliveryOptions(CimApplication.Handle, (MiSubscriptionDeliveryType)types, out subscriptionDeliveryOptionsHandle);
			CimException.ThrowIfMiResultFailure(miResult);
			this._subscriptionDeliveryOptionsHandle = subscriptionDeliveryOptionsHandle;
		}

		public void SetDateTime(string optionName, DateTime optionValue, uint flags)
		{
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				this.AssertNotDisposed();
				MiResult miResult = MiResult.OK; //TODO: SubscriptionDeliveryOptionsMethods.SetDateTime(this._subscriptionDeliveryOptionsHandle, optionName, optionValue, flags);
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		public void SetDateTime(string optionName, TimeSpan optionValue, uint flags)
		{
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				this.AssertNotDisposed();
				MiResult miResult = MiResult.OK; //TODO: SubscriptionDeliveryOptionsMethods.SetDateTime(this._subscriptionDeliveryOptionsHandle, optionName, optionValue, flags);
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		public void SetInterval(string optionName, TimeSpan optionValue, uint flags)
		{
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				this.AssertNotDisposed();
				MiResult miResult = MiResult.OK; //TODO: SubscriptionDeliveryOptionsMethods.SetInterval(this._subscriptionDeliveryOptionsHandle, optionName, optionValue, flags);
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		public void SetNumber(string optionName, uint optionValue, uint flags)
		{
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				this.AssertNotDisposed();
				MiResult miResult = MiResult.OK; //TODO: SubscriptionDeliveryOptionsMethods.SetNumber(this._subscriptionDeliveryOptionsHandle, optionName, optionValue, flags);
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		public void SetString(string optionName, string optionValue, uint flags)
		{
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				this.AssertNotDisposed();
				MiResult miResult = MiResult.OK; //TODO: SubscriptionDeliveryOptionsMethods.SetString(this._subscriptionDeliveryOptionsHandle, optionName, optionValue, flags);
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		object System.ICloneable.Clone()
		{
			return new CimSubscriptionDeliveryOptions(this);
		}
	}
}