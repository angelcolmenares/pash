using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Management.Odata.PS
{
	internal class PSEntityMetadata : EntityMetadata
	{
		public Dictionary<CommandType, PSCmdletInfo> Cmdlets
		{
			get;
			private set;
		}

		public Dictionary<string, PSEntityMetadata.ReferenceSetCmdlets> CmdletsForReferenceSets
		{
			get;
			private set;
		}

		public PSEntityMetadata() : base(0)
		{
			this.Cmdlets = new Dictionary<CommandType, PSCmdletInfo>();
			this.CmdletsForReferenceSets = new Dictionary<string, PSEntityMetadata.ReferenceSetCmdlets>();
		}

		public void AddCmdlet(CommandType commandType, string cmdletName)
		{
			try
			{
				this.Cmdlets.Add(commandType, new PSCmdletInfo(cmdletName));
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				object[] str = new object[1];
				str[0] = commandType.ToString();
				string str1 = string.Format(CultureInfo.CurrentCulture, Resources.DuplicateCommandInEntityMetadata, str);
				throw new InvalidSchemaException(str1, argumentException);
			}
		}

		public void AddCmdletsForReference(string property, PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType propertyType, PSReferenceSetCmdletInfo addRefCmdlet, PSReferenceSetCmdletInfo removeRefCmdlet, PSReferenceSetCmdletInfo getRefCmdlet = null)
		{
			this.CmdletsForReferenceSets.Add(property, new PSEntityMetadata.ReferenceSetCmdlets(propertyType, addRefCmdlet, removeRefCmdlet, getRefCmdlet, false));
		}

		public override StringBuilder ToTraceMessage(string entityName, StringBuilder builder)
		{
			builder.AppendLine(string.Concat("\n\tEntityMetadata for ", entityName));
			builder.AppendLine(string.Concat("\tCmdlet Count = ", this.Cmdlets.Count));
			this.Cmdlets.Keys.ToList<CommandType>().ForEach((CommandType type) => builder = this.Cmdlets[type].ToTraceMessage(string.Concat("PSCmdletInfo for ", type.ToString()), builder));
			return builder;
		}

		public class ReferenceSetCmdlets
		{
			public Dictionary<CommandType, PSReferenceSetCmdletInfo> Cmdlets
			{
				get;
				private set;
			}

			public bool GetRefHidden
			{
				get;
				private set;
			}

			public PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType PropertyType
			{
				get;
				private set;
			}

			public ReferenceSetCmdlets(PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType propertyType, PSReferenceSetCmdletInfo addRefCmdlet, PSReferenceSetCmdletInfo removeRefCmdlet, PSReferenceSetCmdletInfo getRefCmdlet = null, bool getRefHidden = false)
			{
				this.Cmdlets = new Dictionary<CommandType, PSReferenceSetCmdletInfo>();
				this.PropertyType = propertyType;
				if (addRefCmdlet != null)
				{
					this.Cmdlets.Add(CommandType.AddReference, addRefCmdlet);
				}
				if (removeRefCmdlet != null)
				{
					this.Cmdlets.Add(CommandType.RemoveReference, removeRefCmdlet);
				}
				if (getRefCmdlet != null)
				{
					this.Cmdlets.Add(CommandType.GetReference, getRefCmdlet);
					this.GetRefHidden = getRefHidden;
				}
			}

			public enum ReferencePropertyType
			{
				KeyOnly,
				Instance
			}
		}
	}
}