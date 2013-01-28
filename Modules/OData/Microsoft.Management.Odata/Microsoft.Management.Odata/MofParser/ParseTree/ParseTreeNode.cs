using Microsoft.Management.Odata.MofParser;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal abstract class ParseTreeNode
	{
		private DocumentRange m_location;

		private Dictionary<string, object> m_annotations;

		public object this[string annotationName]
		{
			get
			{
				object obj = null;
				if (this.m_annotations != null)
				{
					this.m_annotations.TryGetValue(annotationName, out obj);
				}
				return obj;
			}
			set
			{
				if (value != null)
				{
					if (this.m_annotations == null)
					{
						this.m_annotations = new Dictionary<string, object>();
					}
					this.m_annotations[annotationName] = value;
				}
				else
				{
					if (this.m_annotations != null)
					{
						this.m_annotations.Remove(annotationName);
						return;
					}
				}
			}
		}

		public DocumentRange Location
		{
			get
			{
				return this.m_location;
			}
		}

		internal ParseTreeNode(DocumentRange location)
		{
			this.m_location = location;
		}
	}
}