using System;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class ClaimProperty
	{
		public string PropertyName
		{
			get;
			set;
		}

		public ClaimPropertyType PropertyType
		{
			get;
			set;
		}

		public ClaimProperty(ClaimPropertyType type)
		{
			this.PropertyType = type;
		}

		public virtual bool Compare(ClaimProperty other)
		{
			if (other != null)
			{
				if (this.PropertyType == other.PropertyType)
				{
					if (StringComparer.Ordinal.Equals(this.PropertyName, other.PropertyName))
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.PropertyType.ToString());
			return stringBuilder.ToString();
		}

		public virtual void Validate()
		{
		}
	}
}