using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Provider;
using System.Net;
using System.Threading;
using Mono.Data.PowerShell.Provider;
using Mono.Data.PowerShell.Provider.PathNodeProcessors;

namespace Mono.Data.OData.Provider
{
    public class ODataEntityContentReader : IContentReader
    {
        private readonly Uri _entityContentUri;
        private readonly IContext _context;
        private WebClient _client;
        private bool _read;

        public ODataEntityContentReader(Uri entityContentUri, IContext context)
        {
            _entityContentUri = entityContentUri;
            _context = context;
            _client = new WebClient();            
        }

        public void Dispose()
        {
            var client = Interlocked.Exchange( ref _client, null );
            if( null == client )
            {
                return;
            }

            client.Dispose();
        }

        public IList Read(long readCount)
        {
            if( _read )
            {
                return null;
            }
            _read = true;
            var data = _client.DownloadData(_entityContentUri);
            return new List<byte[]> {data};
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            
        }

        public void Close()
        {
            
        }
    }
}