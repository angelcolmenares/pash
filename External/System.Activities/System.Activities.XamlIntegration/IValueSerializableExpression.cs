using System.Windows.Markup;

namespace System.Activities.XamlIntegration
{
	public interface IValueSerializableExpression
	{
		bool CanConvertToString (IValueSerializerContext context);
		string ConvertToString (IValueSerializerContext context);
	}
}
