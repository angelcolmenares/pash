using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class PropertyMapEntry
	{
		private string _propertyName;

		private string[] _ADAttribute;

		private string[] _ADAMAttribute;

		public string[] ADAMAttribute
		{
			get
			{
				return this._ADAMAttribute;
			}
		}

		public string[] ADAttribute
		{
			get
			{
				return this._ADAttribute;
			}
		}

		public string PropertyName
		{
			get
			{
				return this._propertyName;
			}
		}

		public PropertyMapEntry(string PropertyName, string ADAttribute, string ADAMAttribute)
		{
			this._propertyName = PropertyName;
			string[] aDAttribute = new string[1];
			aDAttribute[0] = ADAttribute;
			this._ADAttribute = aDAttribute;
			string[] aDAMAttribute = new string[1];
			aDAMAttribute[0] = ADAMAttribute;
			this._ADAMAttribute = aDAMAttribute;
		}

		public PropertyMapEntry(string PropertyName, string[] ADAttribute, string[] ADAMAttribute)
		{
			this._propertyName = PropertyName;
			this._ADAttribute = ADAttribute;
			this._ADAMAttribute = ADAMAttribute;
		}
	}
}