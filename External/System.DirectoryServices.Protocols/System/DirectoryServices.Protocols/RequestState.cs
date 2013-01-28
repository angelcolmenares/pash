using System;
using System.IO;
using System.Net;
using System.Text;

namespace System.DirectoryServices.Protocols
{
	internal class RequestState
	{
		public const int bufferSize = 0x400;

		public StringBuilder responseString;

		public string requestString;

		public HttpWebRequest request;

		public Stream requestStream;

		public Stream responseStream;

		public byte[] bufferRead;

		public UTF8Encoding encoder;

		public DsmlAsyncResult dsmlAsync;

		internal bool abortCalled;

		internal Exception exception;

		public RequestState()
		{
			this.responseString = new StringBuilder(0x400);
			this.encoder = new UTF8Encoding();
			this.bufferRead = new byte[0x400];
		}
	}
}