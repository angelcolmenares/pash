using Mono.Data.PowerShell.Provider.PathNodeProcessors;

namespace Mono.Data.PowerShell.Paths
{
    public interface IClearItemContent
    {
        void ClearContent(IContext context);
        object ClearContentDynamicParameters(IContext context);
    }
}