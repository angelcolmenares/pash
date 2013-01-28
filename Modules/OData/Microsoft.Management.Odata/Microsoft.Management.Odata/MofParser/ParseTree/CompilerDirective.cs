using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal class CompilerDirective : MofProduction
	{
		private readonly string m_name;

		public string Name
		{
			get
			{
				return this.m_name;
			}
		}

		public override MofProduction.ProductionType Type
		{
			get
			{
				return MofProduction.ProductionType.CompilerDirective;
			}
		}

		internal CompilerDirective(DocumentRange location, string name) : base(location)
		{
			this.m_name = name;
		}
	}
}