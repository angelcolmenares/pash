using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.Management
{
	public class ManagementNamedValueCollection : NameObjectCollectionBase
	{
		public object this[string name]
		{
			get
			{
				return base.BaseGet(name);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementNamedValueCollection()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ManagementNamedValueCollection(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public void Add(string name, object value)
		{
			try
			{
				base.BaseRemove(name);
			}
			catch
			{
			}
			base.BaseAdd(name, value);
			this.FireIdentifierChanged();
		}

		public ManagementNamedValueCollection Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = new ManagementNamedValueCollection();
			foreach (string str in this)
			{
				object obj = base.BaseGet(str);
				if (obj == null)
				{
					managementNamedValueCollection.Add(str, null);
				}
				else
				{
					Type type = obj.GetType();
					if (!type.IsByRef)
					{
						managementNamedValueCollection.Add(str, obj);
					}
					else
					{
						try
						{
							object obj1 = ((ICloneable)obj).Clone();
							managementNamedValueCollection.Add(str, obj1);
						}
						catch
						{
							throw new NotSupportedException();
						}
					}
				}
			}
			return managementNamedValueCollection;
		}

		private void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		internal IWbemContext GetContext()
		{
			IWbemContext wbemContext = null;
			if (0 < this.Count)
			{
				int num = 0;
				try
				{
					wbemContext = (IWbemContext)(new WbemContext());
					IEnumerator enumerator = this.GetEnumerator();
					try
					{
						do
						{
							if (!enumerator.MoveNext())
							{
								break;
							}
							string current = (string)enumerator.Current;
							object obj = base.BaseGet(current);
							num = wbemContext.SetValue_(current, 0, ref obj);
						}
						while (((long)num & (long)-2147483648) == (long)0);
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
				catch
				{
				}
			}
			return wbemContext;
		}

		public void Remove(string name)
		{
			base.BaseRemove(name);
			this.FireIdentifierChanged();
		}

		public void RemoveAll()
		{
			base.BaseClear();
			this.FireIdentifierChanged();
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}