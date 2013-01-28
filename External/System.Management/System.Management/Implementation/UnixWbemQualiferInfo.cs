using System;

namespace System.Management
{
	public class UnixWbemQualiferInfo : ICloneable
	{
		public UnixWbemQualiferInfo ()
		{

		}

		public string Name
		{
			get;set;
		}

		public CimType Type
		{
			get;set;
		}

		public bool PropagateToInstance
		{
			get;set;
		}

		public bool PropagateToDerivedClasses
		{
			get;set;
		}

		public bool Overridable
		{
			get;set;
		}

		public bool Ammendable
		{
			get;set;
		}

		public QualifierOrigin Origin
		{
			get;set;
		}

		public Type OriginType
		{
			get;set;
		}

		public object Value
		{
			get;set;
		}

		#region ICloneable implementation

		public object Clone ()
		{
			return new UnixWbemQualiferInfo { Name = Name, OriginType = OriginType, Type = Type, PropagateToInstance = PropagateToInstance, PropagateToDerivedClasses = PropagateToDerivedClasses, Ammendable = Ammendable, Overridable = Overridable, Origin = Origin, Value = Value }; 
		}

		#endregion
	}
}

