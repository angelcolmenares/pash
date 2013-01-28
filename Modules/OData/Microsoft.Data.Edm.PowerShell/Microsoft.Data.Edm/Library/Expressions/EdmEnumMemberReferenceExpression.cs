using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmEnumMemberReferenceExpression : EdmElement, IEdmEnumMemberReferenceExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmEnumMember referencedEnumMember;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.EnumMemberReference;
			}
		}

		public IEdmEnumMember ReferencedEnumMember
		{
			get
			{
				return this.referencedEnumMember;
			}
		}

		public EdmEnumMemberReferenceExpression(IEdmEnumMember referencedEnumMember)
		{
			EdmUtil.CheckArgumentNull<IEdmEnumMember>(referencedEnumMember, "referencedEnumMember");
			this.referencedEnumMember = referencedEnumMember;
		}
	}
}