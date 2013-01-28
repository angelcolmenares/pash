using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmDocumentation : IEdmDocumentation
	{
		private readonly string summary;

		private readonly string description;

		public string Description
		{
			get
			{
				return this.description;
			}
		}

		public string Summary
		{
			get
			{
				return this.summary;
			}
		}

		public EdmDocumentation(string summary, string description)
		{
			this.summary = summary;
			this.description = description;
		}
	}
}