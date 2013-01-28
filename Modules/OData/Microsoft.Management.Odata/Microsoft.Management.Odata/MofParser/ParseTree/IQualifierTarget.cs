namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal interface IQualifierTarget
	{
		NodeList<Qualifier> Qualifiers
		{
			get;
		}

	}
}