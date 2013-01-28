using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal abstract class MofProduction : ParseTreeNode
	{
		private object m_parent;

		public MofSpecification ContainingSpecification
		{
			get
			{
				MofSpecification mParent = this.m_parent as MofSpecification;
				if (mParent == null)
				{
					ClassDeclaration classDeclaration = this.m_parent as ClassDeclaration;
					if (classDeclaration == null)
					{
						throw new InvalidOperationException();
					}
					else
					{
						return classDeclaration.ContainingSpecification;
					}
				}
				else
				{
					return mParent;
				}
			}
		}

		public object Parent
		{
			get
			{
				return this.m_parent;
			}
		}

		public abstract MofProduction.ProductionType Type
		{
			get;
		}

		internal MofProduction(DocumentRange location) : base(location)
		{
		}

		internal void SetParent(object parent)
		{
			this.m_parent = parent;
		}

		public enum ProductionType
		{
			None,
			ClassDeclaration,
			CompilerDirective,
			InstanceDeclaration,
			QualifierDeclaration
		}
	}
}