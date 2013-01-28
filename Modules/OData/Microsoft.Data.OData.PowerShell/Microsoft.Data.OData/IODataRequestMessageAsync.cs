namespace Microsoft.Data.OData
{
	using System.IO;
    using System.Threading.Tasks;

    internal interface IODataRequestMessageAsync : IODataRequestMessage
    {
        Task<Stream> GetStreamAsync();
    }
}

