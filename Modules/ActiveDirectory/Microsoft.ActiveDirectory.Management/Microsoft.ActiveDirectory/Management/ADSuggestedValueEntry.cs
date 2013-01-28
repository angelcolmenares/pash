using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADSuggestedValueEntry
	{
		public object Value
		{
			get;
			set;
		}

		public string ValueDescription
		{
			get;
			set;
		}

		public string ValueDisplayName
		{
			get;
			set;
		}

		public string ValueGUID
		{
			get;
			set;
		}

		public ADSuggestedValueEntry()
		{
			Guid guid = Guid.NewGuid();
			this.ValueGUID = guid.ToString();
		}

		public ADSuggestedValueEntry(object value, string displayName, string description, string valueGuid)
		{
			this.Value = value;
			this.ValueDisplayName = displayName;
			this.ValueDescription = description;
			this.ValueGUID = valueGuid;
		}

		public ADSuggestedValueEntry(object value, string displayName, string description)
			: this(value, displayName, description, Guid.NewGuid().ToString())
		{

		}
	}
}