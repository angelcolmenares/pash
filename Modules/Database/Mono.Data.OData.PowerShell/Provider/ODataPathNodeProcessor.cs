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
using Mono.Data.PowerShell.Paths.Processors;
using Mono.Data.PowerShell.Provider.PathNodes;
using Mono.Data.PowerShell.Provider.PathNodeProcessors;

namespace Mono.Data.OData.Provider
{
    public class ODataPathNodeProcessor : PathNodeProcessorBase
    {
        private readonly ODataDrive _drive;

        public ODataPathNodeProcessor(ODataDrive drive)
        {
            _drive = drive;
        }

        public override System.Collections.Generic.IEnumerable<INodeFactory> ResolvePath(IContext context, string path)
        {
            var subpath = path.Replace(_drive.Root, string.Empty);
            return base.ResolvePath(context, subpath);
        }
        protected override INodeFactory Root
        {
            get { return new ODataRootNodeFactory(_drive.ODataSourceUri, _drive.ODataSourceDocument, _drive.Metadata); }
        }
    }
}
