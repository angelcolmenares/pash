using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class ValueInitializerList : NodeList<ValueInitializer>
	{
		internal ValueInitializerList(ValueInitializer[] valueInitializers) : base(valueInitializers)
		{
			ValueInitializer[] valueInitializerArray = valueInitializers;
			for (int i = 0; i < (int)valueInitializerArray.Length; i++)
			{
				ValueInitializer valueInitializer = valueInitializerArray[i];
				valueInitializer.SetParent(this);
			}
		}
	}
}