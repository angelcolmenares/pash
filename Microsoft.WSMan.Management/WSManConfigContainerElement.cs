using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WSMan.Management
{
	public class WSManConfigContainerElement : WSManConfigElement
	{
		private string[] _keys;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] Keys
		{
			get
			{
				return this._keys;
			}
			set
			{
				this._keys = value;
			}
		}

		internal WSManConfigContainerElement(string Name, string TypeNameOfElement, string[] keys)
		{
			this._keys = keys;
			base.Name = Name;
			base.Type = TypeNameOfElement;
		}
	}
}