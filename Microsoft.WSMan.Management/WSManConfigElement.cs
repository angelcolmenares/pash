using System;

namespace Microsoft.WSMan.Management
{
	public class WSManConfigElement
	{
		private string _name;

		private string _typenameofelement;

		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		public string Type
		{
			get
			{
				return this._typenameofelement;
			}
			set
			{
				this._typenameofelement = value;
			}
		}

		public string TypeNameOfElement
		{
			get
			{
				return this._typenameofelement;
			}
			set
			{
				this._typenameofelement = value;
			}
		}

		internal WSManConfigElement()
		{
		}

		internal WSManConfigElement(string name, string typenameofelement)
		{
			this._name = name;
			this._typenameofelement = typenameofelement;
		}
	}
}