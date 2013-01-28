using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Mono.Data.OData.Provider
{
    public class ODataDynamicParameters
    {
        [Parameter()]
        public int Top { get; set; }
        [Parameter()]
        [Alias( "SortBy" )]
        public string OrderBy { get; set; }
        [Parameter()]
        public SwitchParameter Descending { get; set; }
        [Parameter()]
        public int Skip { get; set; }
        [Parameter()]
        public string Expand { get; set; }
        [Parameter()]
        public string[] Select { get; set; }
    }
}
