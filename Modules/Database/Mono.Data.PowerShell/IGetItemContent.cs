using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Provider;
using System.Text;
using Mono.Data.PowerShell.Provider.PathNodeProcessors;

namespace Mono.Data.PowerShell.Paths
{
    public interface IGetItemContent
    {
        IContentReader GetContentReader(IContext context);
        object GetContentReaderDynamicParameters(IContext context);
    }
}
