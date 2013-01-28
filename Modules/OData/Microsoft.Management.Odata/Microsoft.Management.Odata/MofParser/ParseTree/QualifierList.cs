using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class QualifierList : NodeList<Qualifier>
	{
		internal bool ContainsAbstractQualifier
		{
			get
			{
				return this.ContainsNamedQualifier("abstract");
			}
		}

		internal bool ContainsAssociationQualifier
		{
			get
			{
				return this.ContainsNamedQualifier("association");
			}
		}

		internal QualifierList(Qualifier[] qualifiers) : base(qualifiers)
		{
			Qualifier[] qualifierArray = qualifiers;
			for (int i = 0; i < (int)qualifierArray.Length; i++)
			{
				Qualifier qualifier = qualifierArray[i];
				qualifier.SetParent(this);
			}
		}

		private bool ContainsNamedQualifier(string qualifierName)
		{
			Qualifier qualifier = null;
			return this.TryFindQualifier(qualifierName, out qualifier);
		}

		public override string ToString()
		{
			if (base.Count != 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("[");
				bool flag = false;
				foreach (Qualifier qualifier in this)
				{
					if (!flag)
					{
						flag = true;
					}
					else
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(qualifier);
				}
				stringBuilder.Append("]");
				return stringBuilder.ToString();
			}
			else
			{
				return "";
			}
		}

		internal bool TryFindQualifier(string qualifierName, out Qualifier qualifier)
		{
			bool flag;
			IEnumerator<Qualifier> enumerator = base.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					Qualifier current = enumerator.Current;
					if (current == null || !current.Name.Equals(qualifierName, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					qualifier = current;
					flag = true;
					return flag;
				}
				qualifier = null;
				return false;
			}
			return flag;
		}
	}
}