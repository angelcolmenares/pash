using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("b0c22d87-38ef-486d-9b6d-8a4bb58a3d75")]
	internal class UNIX_PingStatus : CIM_ManagedSystemElement
	{
		static UNIX_PingStatus ()
		{

		}
		
		public UNIX_PingStatus ()
		{
			
		}

		
		protected override void RegisterProperies()
		{
			base.RegisterProperies ();
			RegisterProperty ("Address", CimType.String, 0);
			RegisterProperty ("BufferSize", CimType.SInt32, 0);
			RegisterProperty ("NoFragmentation", CimType.Boolean, 0);
			RegisterProperty ("ProtocolAddress", CimType.String, 0);
			RegisterProperty ("ProtocolAddressResolved", CimType.Boolean , 0);
			RegisterProperty ("RecordRoute", CimType.String, 0);
			RegisterProperty ("ReplyInconsistency", CimType.String, 0);
			RegisterProperty ("ReplySize", CimType.SInt32, 0);
			RegisterProperty ("ResolveAddressNames", CimType.Object, 0);
			RegisterProperty ("ResponseTime", CimType.Real32, 0);
			RegisterProperty ("ResponseTimeToLive", CimType.SInt32, 0);
			RegisterProperty ("RouteRecord", CimType.String, 0);
			RegisterProperty ("RouteRecordResolved", CimType.String, 0);
			RegisterProperty ("SourceRoute", CimType.String, 0);
			RegisterProperty ("SourceRouteType", CimType.String, 0);
			RegisterProperty ("TimeStampRecord", CimType.SInt64, 0);
			RegisterProperty ("TimeStampRecordAddress", CimType.String, 0);
			RegisterProperty ("TimeStampRecordAddressResolved", CimType.String, 0);
			RegisterProperty ("TimestampRoute", CimType.String, 0);
			RegisterProperty ("TimeToLive", CimType.SInt32, 0);
			RegisterProperty ("TypeofService", CimType.SInt32, 0);
			RegisterProperty ("StatusCode", CimType.SInt32, 0);
			RegisterProperty ("PrimaryAddressResolutionStatus", CimType.SInt32, 0);
			RegisterProperty ("Timeout", CimType.SInt32, 0);
		}
		
		public override string PathField {
			get { return "Address"; }
		}
		
		#region IUnixWbemClassHandler implementation

		protected override QueryParser Parser { 
			get { return new QueryParser<CIM_ManagedSystemElement_MetaImplementation> (); } 
		}

		public override IEnumerable<object> Get (string strQuery)
		{
			var ex = Parser.GetWhereClauses (strQuery, null);
			var key = ex.Keys.Where (x => x.Equals ("Address", StringComparison.OrdinalIgnoreCase)).FirstOrDefault ();
			if (!string.IsNullOrEmpty (key)) {
				Address = (string)ex[key];
				return (new UnixWbemClassObject[] { (UnixWbemClassObject)New().Get(ex) }).OfType<object> ();
			}
			return (new UnixWbemClassObject[] { }).OfType<object> ();
		}

		public string Address
		{
			get;set;
		}
		
		internal override IUnixWbemClassHandler GetInternal (object nativeObj)
		{
			var ex = (IDictionary<string, object>)nativeObj;
			var ret = base.GetInternal (nativeObj);
			object address;
			if (ex.TryGetValue ("Address", out address)) {
				Ping pingSender = new Ping ();
				PingOptions options = new PingOptions ();
				
				// Use the default Ttl value which is 128,
				// but change the fragmentation behavior.
				options.DontFragment = true;
				int bufferSize = 32;
				// Create a buffer of 32 bytes of data to be transmitted.

				int timeout = 4000;
				int ttl = 80;
				object timeoutObj;
				object ttlObj;
				object bufferSizeObj;
				if (ex.TryGetValue ("Timeout", out timeoutObj)) {
					int tempTimeout;
					if (int.TryParse (timeoutObj.ToString (), out tempTimeout)) {
						timeout = tempTimeout;
					}
				}
				if (ex.TryGetValue ("TimeToLive", out ttlObj)) {
					int tempTimeToLive;
					if (int.TryParse (ttlObj.ToString (), out tempTimeToLive)) {
						ttl = tempTimeToLive;
					}
				}
				if (ex.TryGetValue ("BufferSize", out bufferSizeObj)) {
					int tempBufferSize;
					if (int.TryParse (bufferSizeObj.ToString (), out tempBufferSize)) {
						bufferSize = tempBufferSize;
					}
				}
				string data = new string('a', bufferSize);// "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
				byte[] buffer = Encoding.ASCII.GetBytes (data);
				var start = DateTime.Now;
				options.Ttl = ttl;
				PingReply reply = pingSender.Send (address.ToString (), timeout, buffer, options);

				if (reply.Status == IPStatus.Success) {
					var hostEntry = System.Net.Dns.GetHostEntry (reply.Address.ToString ());

					ret.AddProperty ("Address", reply.Address.ToString ());
					ret.AddProperty ("BufferSize", reply.Buffer.Length);
					ret.AddProperty ("NoFragmentation", options.DontFragment);
					ret.AddProperty ("ProtocolAddress", reply.Address.ToString ());
					ret.AddProperty ("ProtocolAddressResolved", true);
					ret.AddProperty ("RecordRoute", "");
					ret.AddProperty ("ReplyInconsistency", false);
					ret.AddProperty ("ReplySize", reply.Buffer.Length);
					ret.AddProperty ("ResolveAddressNames", hostEntry.AddressList.Select (x => x.ToString ()).ToArray ());
					ret.AddProperty ("ResponseTime", (DateTime.Now - start).TotalMilliseconds);
					ret.AddProperty ("ResponseTimeToLive", reply.Options.Ttl);
					ret.AddProperty ("RouteRecord", "");
					ret.AddProperty ("RouteRecordResolved", true);
					ret.AddProperty ("SourceRoute", "");
					ret.AddProperty ("SourceRouteType", "");
					ret.AddProperty ("TimeStampRecord", reply.RoundtripTime);
					ret.AddProperty ("TimeStampRecordAddress", "");
					ret.AddProperty ("TimeStampRecordAddressResolved", "");
					ret.AddProperty ("TimestampRoute", 0);
					ret.AddProperty ("TimeToLive", reply.Options.Ttl);
					ret.AddProperty ("TypeofService", 0);
				} else {
					ret.AddProperty ("Address", address);
					ret.AddProperty ("BufferSize", buffer.Length);
					ret.AddProperty ("NoFragmentation", options.DontFragment);
					ret.AddProperty ("ProtocolAddress", address);
					ret.AddProperty ("ProtocolAddressResolved", false);
					ret.AddProperty ("RecordRoute", "");
					ret.AddProperty ("ReplyInconsistency", false);
					ret.AddProperty ("ReplySize", buffer.Length);
					ret.AddProperty ("ResolveAddressNames", new string[] {});
					ret.AddProperty ("ResponseTime", (DateTime.Now - start).TotalMilliseconds);
					ret.AddProperty ("ResponseTimeToLive", options.Ttl);
					ret.AddProperty ("RouteRecord", "");
					ret.AddProperty ("RouteRecordResolved", true);
					ret.AddProperty ("SourceRoute", "");
					ret.AddProperty ("SourceRouteType", "");

					ret.AddProperty ("TimeStampRecord", reply.RoundtripTime);
					ret.AddProperty ("TimeStampRecordAddress", "");
					ret.AddProperty ("TimeStampRecordAddressResolved", "");
					ret.AddProperty ("TimestampRoute", 0);
					ret.AddProperty ("TimeToLive", options.Ttl);
					ret.AddProperty ("TypeofService", 0);
				}
				ret.AddProperty ("StatusCode", (int)reply.Status);
				ret.AddProperty ("PrimaryAddressResolutionStatus", (int)reply.Status);
				ret.AddProperty ("Timeout", timeout);
			}
			
			return ret;
		}
		
		
#endregion
	}
}

