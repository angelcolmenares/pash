using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract]
	public abstract class Mergable<T> : IResolvable, IMergable<T>, IMergable, IExtensibleDataObject
	where T : Mergable<T>
	{
		private Dictionary<string, object> propertyStore;

		private Dictionary<string, object> PropertyStore
		{
			get
			{
				if (this.propertyStore == null)
				{
					this.propertyStore = new Dictionary<string, object>();
				}
				return this.propertyStore;
			}
		}

		ExtensionDataObject System.Runtime.Serialization.IExtensibleDataObject.ExtensionData
		{
			get;set;
		}

		protected Mergable()
		{
		}

		protected TValue Convert<TValue>()
		{
			DataContractSerializer dataContractSerializer = new DataContractSerializer(this.GetType());
			DataContractSerializer dataContractSerializer1 = new DataContractSerializer(typeof(TValue));
			MemoryStream memoryStream = new MemoryStream();
			dataContractSerializer.WriteObject(memoryStream, this);
			memoryStream.Position = (long)0;
			return (TValue)dataContractSerializer1.ReadObject(memoryStream);
		}

		protected Nullable<TValue> GetField<TValue>(string fieldName)
		where TValue : struct
		{
			object obj = null;
			if (!this.PropertyStore.TryGetValue(fieldName, out obj))
			{
				Nullable<TValue> nullable = null;
				return nullable;
			}
			else
			{
				return new Nullable<TValue>((TValue)obj);
			}
		}

		protected TValue GetValue<TValue>(string fieldName)
		{
			object obj = null;
			if (!this.PropertyStore.TryGetValue(fieldName, out obj))
			{
				TValue tValue = default(TValue);
				return tValue;
			}
			else
			{
				return (TValue)obj;
			}
		}

		void Microsoft.Samples.WindowsAzure.ServiceManagement.IMergable.Merge(object other)
		{
			((Microsoft.Samples.WindowsAzure.ServiceManagement.IMergable<T>)this).Merge((T)other);
		}

		void Microsoft.Samples.WindowsAzure.ServiceManagement.IMergable<T>.Merge(T other)
		{
			object obj = null;
			Mergable<T> mergable = other;
			foreach (KeyValuePair<string, object> propertyStore in mergable.PropertyStore)
			{
				if (this.PropertyStore.TryGetValue(propertyStore.Key, out obj))
				{
					IMergable mergable1 = obj as IMergable;
					if (mergable1 != null)
					{
						mergable1.Merge(propertyStore.Value);
						continue;
					}
				}
				this.PropertyStore[propertyStore.Key] = propertyStore.Value;
			}
		}

		public virtual object ResolveType()
		{
			return this;
		}

		protected void SetField<TValue>(string fieldName, Nullable<TValue> value)
		where TValue : struct
		{
			if (value.HasValue)
			{
				this.PropertyStore[fieldName] = value.Value;
			}
		}

		protected void SetValue<TValue>(string fieldName, TValue value)
		{
			this.PropertyStore[fieldName] = value;
		}
	}
}