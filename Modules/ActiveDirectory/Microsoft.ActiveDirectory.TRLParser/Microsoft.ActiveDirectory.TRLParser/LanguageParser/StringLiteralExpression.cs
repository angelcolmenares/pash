using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class StringLiteralExpression : Expression
	{
		public string Value
		{
			get;
			set;
		}

		public StringLiteralExpression(string value)
		{
			this.Value = value;
		}

		public override bool Compare(Expression other)
		{
			StringLiteralExpression stringLiteralExpression = other as StringLiteralExpression;
			if (stringLiteralExpression != null)
			{
				return StringComparer.Ordinal.Equals(this.Value, stringLiteralExpression.Value);
			}
			else
			{
				return false;
			}
		}

		public override ExpressionStruct GetStruct()
		{
			ExpressionStruct @struct = base.GetStruct();
			@struct.type = ExpressionType.Literal;
			@struct.LiteralExpression.literal = this.Value;
			return @struct;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("\"");
			stringBuilder.Append(this.Value);
			stringBuilder.Append("\"");
			return stringBuilder.ToString();
		}

		public override void Validate(object context)
		{
			if (this.Value != null)
			{
				this.Value.Validate();
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Value";
				throw new PolicyValidationException(SR.GetString("POLICY0005", objArray));
			}
		}
	}
}