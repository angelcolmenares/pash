using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Management.Odata.PS
{
	internal class PSParameterSet
	{
		public string Name
		{
			get;
			private set;
		}

		public PSParameterSet.ParameterDictionary Parameters
		{
			get;
			private set;
		}

		internal PSParameterSet(string name)
		{
			this.Name = name;
			this.Parameters = new PSParameterSet.ParameterDictionary();
		}

		public StringBuilder ToTraceMessage(StringBuilder builder)
		{
			builder.AppendLine(string.Concat("\nPSParameterSet = ", this.Name));
			builder.AppendLine(string.Concat("Parameter\nCount = ", this.Parameters.Count));
			this.Parameters.Keys.ToList<string>().ForEach((string item) => {
				string[] type = new string[8];
				type[0] = "Name = ";
				type[1] = item;
				type[2] = " Type = ";
				type[3] = this.Parameters[item].Type;
				type[4] = " IsSwitch = ";
				bool isSwitch = this.Parameters[item].IsSwitch;
				type[5] = isSwitch.ToString();
				type[6] = " IsMandatory = ";
				bool isMandatory = this.Parameters[item].IsMandatory;
				type[7] = isMandatory.ToString();
				builder.AppendLine(string.Concat(type));
			}
			);
			return builder;
		}

		internal class ParameterDictionary : Dictionary<string, PSParameterInfo>
		{
			public ParameterDictionary() : base(StringComparer.OrdinalIgnoreCase)
			{
			}

			public ParameterDictionary(PSParameterSet.ParameterDictionary other) : base(other, StringComparer.OrdinalIgnoreCase)
			{
			}

			public void Add(string name)
			{
				this.Add(name, new PSParameterInfo());
			}

			public void Add(string name, string type)
			{
				this.Add(name, new PSParameterInfo(false, false, type));
			}

			public void Add(string name, PSParameterInfo parameterInfo)
			{
				name.ThrowIfNullOrEmpty("name", Resources.ParameterSetNameNullOrEmpty, new object[0]);
				object[] objArray = new object[1];
				objArray[0] = name;
				parameterInfo.ThrowIfNull("parameterInfo", Resources.NullPassedForCmdletParameter, objArray);
				try
				{
					base.Add(name, parameterInfo);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					object[] objArray1 = new object[1];
					objArray1[0] = name;
					string str = string.Format(CultureInfo.CurrentCulture, Resources.DuplicateParameterInParameterSet, objArray1);
					throw new InvalidSchemaException(str, argumentException);
				}
			}
		}
	}
}