using System.Management.Automation.Provider;
using Mono.Data.PowerShell.Provider.PathNodeProcessors;

namespace Mono.Data.PowerShell.Paths
{
    public interface ISetItemContent
    {
        IContentWriter GetContentWriter(IContext context);
        object GetContentWriterDynamicParameters(IContext context);
    }
}