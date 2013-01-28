//
// WebMessageEncoder.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Net.Mime;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class WebMessageEncoder : MessageEncoder
	{
		internal const string ScriptPropertyName = "618BC2B0-38AA-21A3-DB4A-404FC87B9B11"; // randomly generated

		WebMessageEncodingBindingElement source;

		public WebMessageEncoder (WebMessageEncodingBindingElement source)
		{
			this.source = source;
		}

		public override string ContentType {
#if NET_2_1
			get { return MediaType; }
#else
			get { return MediaType + "; charset=" + source.WriteEncoding.HeaderName; }
#endif
		}

		// FIXME: find out how it can be customized.
		public override string MediaType {
			get { return "application/xml"; }
		}

		public override MessageVersion MessageVersion {
			get { return MessageVersion.None; }
		}

		public override bool IsContentTypeSupported (string contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException ("contentType");
			return true; // anything is accepted.
		}

		public override Message ReadMessage (ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
		{
			throw new NotImplementedException ();
		}

		public override Message ReadMessage (Stream stream, int maxSizeOfHeaders, string contentType)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (string.IsNullOrEmpty (contentType)) contentType = "application/octet-stream";

			Encoding enc = Encoding.UTF8;
			ContentType ct = new ContentType (contentType);
			if (ct.CharSet != null)
				enc = Encoding.GetEncoding (ct.CharSet);

			WebContentFormat fmt = WebContentFormat.Xml;
			if (source.ContentTypeMapper != null)
				fmt = source.ContentTypeMapper.GetMessageFormatForContentType (contentType);
			else {
				switch (ct.MediaType) {
				case "application/json":
					fmt = WebContentFormat.Json;
					break;
				case "application/xml":
					fmt = WebContentFormat.Xml;
					break;
				default:
					fmt = WebContentFormat.Raw;
					break;
				}
			}

			Message msg = null;
			WebBodyFormatMessageProperty wp = null;
			switch (fmt) {
			case WebContentFormat.Xml:
				// FIXME: is it safe/unsafe/required to keep XmlReader open?
				msg = Message.CreateMessage (MessageVersion.None, null, XmlReader.Create (new StreamReader (stream, enc)));
				wp = new WebBodyFormatMessageProperty (WebContentFormat.Xml);
				break;
			case WebContentFormat.Json:
				// FIXME: is it safe/unsafe/required to keep XmlReader open?
#if NET_2_1
				msg = Message.CreateMessage (MessageVersion.None, null, JsonReaderWriterFactory.CreateJsonReader (stream, source.ReaderQuotas));
#else
				msg = Message.CreateMessage (MessageVersion.None, null, JsonReaderWriterFactory.CreateJsonReader (stream, enc, source.ReaderQuotas, null));
#endif
				wp = new WebBodyFormatMessageProperty (WebContentFormat.Json);
				break;
			case WebContentFormat.Raw:
				msg = new WebMessageFormatter.RawMessage (stream);
				wp = new WebBodyFormatMessageProperty (WebContentFormat.Raw);
				break;
			default:
				throw new SystemException ("INTERNAL ERROR: cannot determine content format");
			}
			if (wp != null)
				msg.Properties.Add (WebBodyFormatMessageProperty.Name, wp);
			return msg;
		}

		WebContentFormat GetContentFormat (Message message)
		{
			string name = WebBodyFormatMessageProperty.Name;
			if (message.Properties.ContainsKey (name))
				return ((WebBodyFormatMessageProperty) message.Properties [name]).Format;

			switch (MediaType) {
			case "application/xml":
			case "text/xml":
				return WebContentFormat.Xml;
			case "application/json":
			case "text/json":
				return WebContentFormat.Json;
			case "application/octet-stream":
				return WebContentFormat.Raw;
			default:
				return WebContentFormat.Default;
			}
		}

		public override void WriteMessage (Message message, Stream stream)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			

			// Handle /js and /jsdebug as the special cases.
			var script = message.Properties [ScriptPropertyName] as string;
			if (script != null) {
				var bytes = source.WriteEncoding.GetBytes (script);
				stream.Write (bytes, 0, bytes.Length);
				return;
			}


			if (!MessageVersion.Equals (message.Version))
				throw new ProtocolException (String.Format ("MessageVersion {0} is not supported", message.Version));
			if (stream == null)
				throw new ArgumentNullException ("stream");

			switch (GetContentFormat (message)) {
			case WebContentFormat.Xml:
#if NET_2_1
				using (XmlWriter w = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (new StreamWriter (stream, source.WriteEncoding))))
					message.WriteMessage (w);
#else
				using (XmlWriter w = XmlDictionaryWriter.CreateTextWriter (stream, source.WriteEncoding))
					message.WriteMessage (w);
#endif
				break;
			case WebContentFormat.Json:
				using (XmlWriter w = JsonReaderWriterFactory.CreateJsonWriter (stream, source.WriteEncoding))
					message.WriteMessage (w);
				break;
			case WebContentFormat.Raw:
				if (message is WebMessageFormatter.RawMessage)
				{
					var rmsg = (WebMessageFormatter.RawMessage) message;
					var src = rmsg.Stream;
					if (src == null) // null output
						break;

					int len = 0;
					byte [] buffer = new byte [4096];
					while ((len = src.Read (buffer, 0, buffer.Length)) > 0)
						stream.Write (buffer, 0, len);
				}
				else if (message is SimpleMessage) {

					/* TODO: REMOVE -- BIG HACK !!! */
					/* XML Binary to Stream */
					var mstream = new MemoryStream();
					var smsg = (SimpleMessage)message;
					var copy = smsg.CreateBufferedCopy (32768);

					copy.WriteMessage (mstream);
					var xmlDocument = ((copy as DefaultMessageBuffer).GetBodyWriter() as XmlReaderBodyWriter).GetXmlBak();
					var base64 = xmlDocument.Substring ("<Binary>".Length, xmlDocument.Length - "<Binary>".Length - "</Binary>".Length); 

					var resultBuffer = FromBinHexString(base64); //Convert.FromBase64String (base64);
					var result = Encoding.UTF8.GetString (resultBuffer);

					stream.Write(resultBuffer, 0, resultBuffer.Length);
					/* TODO: REVIEW WHY RAW AND not RawMessge */
				}
				break;
			case WebContentFormat.Default:
				throw new SystemException ("INTERNAL ERROR: cannot determine content format");
			}

			if (System.ServiceModel.Web.WebOperationContext.Current != null)
				OperationContext.Current.Extensions.Remove (System.ServiceModel.Web.WebOperationContext.Current);

		}

		internal static byte[] FromBinHexString (string s)
		{
			char[] array = s.ToCharArray ();
			byte[] array2 = new byte[array.Length / 2 + array.Length % 2];
			FromBinHexString (array, 0, array.Length, array2);
			return array2;
		}
		
		internal static int FromBinHexString (char[] chars, int offset, int charLength, byte[] buffer)
		{
			int num = offset;
			for (int i = 0; i < charLength - 1; i += 2)
			{
				buffer [num] = ((chars [i] <= '9') ? ((byte)(chars [i] - '0')) : ((byte)(chars [i] - 'A' + '\n')));
				int expr_33_cp_1 = num;
				buffer [expr_33_cp_1] = (byte)(buffer [expr_33_cp_1] << 4);
				int expr_40_cp_1 = num;
				buffer [expr_40_cp_1] += ((chars [i + 1] <= '9') ? ((byte)(chars [i + 1] - '0')) : ((byte)(chars [i + 1] - 'A' + '\n')));
				num++;
			}
			if (charLength % 2 != 0)
			{
				buffer [num++] = (byte)(((chars [charLength - 1] <= '9') ? ((byte)(chars [charLength - 1] - '0')) : ((byte)(chars [charLength - 1] - 'A' + '\n'))) << 4);
			}
			return num - offset;
		}


		public override ArraySegment<byte> WriteMessage (Message message, int maxMessageSize, BufferManager bufferManager,
								 int messageOffset)
		{
			throw new NotImplementedException ();
		}
	}
}
