using System;

namespace Microsoft.WSMan.Management
{
	public class WSManConfigLeafElement : WSManConfigElement
	{
		private object _SourceOfValue;

		private object _value;

		public object SourceOfValue
		{
			get
			{
				return this._SourceOfValue;
			}
			set
			{
				this._SourceOfValue = value;
			}
		}

		public object Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = value;
			}
		}

		internal WSManConfigLeafElement()
		{
		}

		internal WSManConfigLeafElement(string Name, object Value, string TypeNameOfElement, object SourceOfValue = null)
		{
			this._value = Value;
			this._SourceOfValue = SourceOfValue;
			base.Name = Name;
			base.Type = TypeNameOfElement;
		}
	}
}