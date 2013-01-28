namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Runtime.InteropServices;

    internal static class WebResponseObjectFactory
    {
        internal static WebResponseObject GetResponseObject(WebResponse response, MemoryStream contentStream, ExecutionContext executionContext, bool useBasicParsing = false)
        {
            if (WebResponseHelper.IsText(response))
            {
                if (!useBasicParsing)
                {
                    return new HtmlWebResponseObject(response, contentStream, executionContext);
                }
                return new BasicHtmlWebResponseObject(response, contentStream);
            }
            return new WebResponseObject(response, contentStream);
        }
    }
}

