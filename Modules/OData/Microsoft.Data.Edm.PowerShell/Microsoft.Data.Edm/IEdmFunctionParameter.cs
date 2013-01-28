namespace Microsoft.Data.Edm
{
	internal interface IEdmFunctionParameter : IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEdmFunctionBase DeclaringFunction
		{
			get;
		}

		EdmFunctionParameterMode Mode
		{
			get;
		}

		IEdmTypeReference Type
		{
			get;
		}

	}
}