/*
	Copyright (c) 2012 Code Owls LLC

	Permission is hereby granted, free of charge, to any person obtaining a copy 
	of this software and associated documentation files (the "Software"), to 
	deal in the Software without restriction, including without limitation the 
	rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
	sell copies of the Software, and to permit persons to whom the Software is 
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in 
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
	IN THE SOFTWARE. 
*/
using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Mono.Data.PowerShell.Paths.Processors;

namespace Mono.Data.OData.Provider
{
    [CmdletProvider("OData", ProviderCapabilities.ShouldProcess | ProviderCapabilities.Filter | ProviderCapabilities.Include)]
    public class ODataProvider : Mono.Data.PowerShell.Provider.Provider
    {
        private ODataDrive Drive
        {
            get
            {
                var drive = this.PSDriveInfo as ODataDrive;
                if (null == drive)
                {
                    drive = ProviderInfo.Drives[0] as ODataDrive;
                }
                return drive;
            }
        }

        protected override IPathNodeProcessor PathNodeProcessor
        {
            get { return new ODataPathNodeProcessor(this.Drive); }
        }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
			var uri = new Uri(drive.Root);
			/* drive.Root.Replace('/', '\\') */
            var newDrive = new PSDriveInfo(
                drive.Name,
                drive.Provider,
				uri.Host,
                drive.Description,
                drive.Credential);
            return new ODataDrive(uri, newDrive );
        }

        // overridden to prevent the powershell runtime from mucking up the URI with \'s
        //protected override string MakePath(string parent, string child)
        //{
        //    child = child.TrimStart('\\', '/');
        //    parent = parent.TrimEnd('\\', '/');

        //    return parent + "/" + child;
        //}
    }
}
