using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmEnumMemberReferenceExpression : IEdmExpression, IEdmElement
	{
		IEdmEnumMember ReferencedEnumMember
		{
			get;
		}

	}
}