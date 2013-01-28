using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal abstract class Expression
	{
		protected Expression()
		{
		}

		public abstract bool Compare(Expression other);

		public virtual ExpressionStruct GetStruct()
		{
			ExpressionStruct expressionStruct = new ExpressionStruct();
			return expressionStruct;
		}

		public abstract void Validate(object context);
	}
}