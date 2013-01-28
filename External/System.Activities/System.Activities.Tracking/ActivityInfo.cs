using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Activities;
using System.Windows.Markup;

namespace System.Activities.Tracking
{
	[DataContract]
	public sealed class ActivityInfo
	{
		public ActivityInfo (string name, string id, string instanceId, string typeName)
		{
			Id = id;
			Name = name;
			InstanceId = instanceId;
			TypeName = typeName;
		}

		[DataMember]
		public string Id { get; private set; }

		[DataMember]
		public string InstanceId { get; private set; }

		[DataMember]
		public string Name { get; private set; }

		[DataMember]
		public string TypeName { get; private set; }

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
