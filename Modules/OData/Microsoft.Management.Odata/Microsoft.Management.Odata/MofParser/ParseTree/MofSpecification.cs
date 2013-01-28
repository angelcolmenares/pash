using System;
using System.Text;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class MofSpecification
	{
		private readonly MofProduction[] m_productions;

		private string m_containingFile;

		public string ContainingFile
		{
			get
			{
				return this.m_containingFile;
			}
		}

		public MofProduction[] Productions
		{
			get
			{
				return this.m_productions;
			}
		}

		internal MofSpecification(MofProduction[] productions)
		{
			this.m_productions = productions;
			MofProduction[] mProductions = this.m_productions;
			for (int i = 0; i < (int)mProductions.Length; i++)
			{
				MofProduction mofProduction = mProductions[i];
				if (mofProduction.Parent == null)
				{
					mofProduction.SetParent(this);
				}
			}
		}

		internal void SetParent(string containingFile)
		{
			this.m_containingFile = containingFile;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			MofProduction[] productions = this.Productions;
			for (int i = 0; i < (int)productions.Length; i++)
			{
				MofProduction mofProduction = productions[i];
				stringBuilder.Append(mofProduction.ToString());
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}
	}
}