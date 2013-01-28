using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADWSResultEntry
	{
		private ADObject _dirObject;

		private string _objectReferenceProperty;

		private string _distinguishedName;

		private string _relativeDistinguishedName;

		private string _parentContainer;

		public ADObject DirObject
		{
			get
			{
				return this._dirObject;
			}
			set
			{
				this._dirObject = value;
			}
		}

		public string DistinguishedName
		{
			get
			{
				return this._distinguishedName;
			}
			set
			{
				this._distinguishedName = value;
			}
		}

		public string ObjectReferenceProperty
		{
			get
			{
				return this._objectReferenceProperty;
			}
			set
			{
				this._objectReferenceProperty = value;
			}
		}

		public string ParentContainer
		{
			get
			{
				return this._parentContainer;
			}
			set
			{
				this._parentContainer = value;
			}
		}

		public string RelativeDistinguishedName
		{
			get
			{
				return this._relativeDistinguishedName;
			}
			set
			{
				this._relativeDistinguishedName = value;
			}
		}

		public ADWSResultEntry()
		{
			this.DirObject = new ADObject();
		}
	}
}