namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Runtime.CompilerServices;

    [Cmdlet("Invoke", "WebRequest", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217035")]
    public class InvokeWebRequestCommand : WebRequestPSCmdlet
    {
        internal override void ProcessResponse(WebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (base.ShouldWriteToPipeline && (this.UseBasicParsing == 0))
            {
                base.VerifyInternetExplorerAvailable(true);
            }
            MemoryStream contentStream = null;
            using (Stream stream2 = StreamHelper.GetResponseStream(response))
            {
                contentStream = StreamHelper.ReadStream(stream2, response.ContentLength, this);
            }
            if (base.ShouldWriteToPipeline)
            {
                WebResponseObject sendToPipeline = WebResponseObjectFactory.GetResponseObject(response, contentStream, base.Context, (bool) this.UseBasicParsing);
                base.WriteObject(sendToPipeline);
            }
            if (base.ShouldSaveToOutFile)
            {
                StreamHelper.SaveStreamToFile(contentStream, base.QualifiedOutFile, this);
            }
        }

        [Parameter]
        public virtual SwitchParameter UseBasicParsing { get; set; }
    }
}

