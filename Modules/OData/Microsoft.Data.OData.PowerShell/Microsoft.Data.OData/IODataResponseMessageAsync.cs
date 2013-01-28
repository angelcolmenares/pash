using System.IO;

namespace Microsoft.Data.OData
{
    using System.Threading.Tasks;

    internal interface IODataResponseMessageAsync : IODataResponseMessage
    {
        Task<Stream> GetStreamAsync();
    }
}

