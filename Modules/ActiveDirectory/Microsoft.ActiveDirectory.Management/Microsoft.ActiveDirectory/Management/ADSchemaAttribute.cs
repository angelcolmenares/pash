using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSchemaAttribute
	{
		private ADAttributeSyntax _syntax;

		private bool _isSingleValued;

		private bool _isSystemOnly;

		private bool _isConstructed;

		private int? _linkID;

		public bool IsBackLink
		{
			get
			{
				if (!this._linkID.HasValue)
				{
					return false;
				}
				else
				{
					return this._linkID.Value % 2 != 0;
				}
			}
		}

		public bool IsConstructed
		{
			get
			{
				return this._isConstructed;
			}
			set
			{
				this._isConstructed = value;
			}
		}

		public bool IsLinkedAttribute
		{
			get
			{
				return this._linkID.HasValue;
			}
		}

		public bool IsSingleValued
		{
			get
			{
				return this._isSingleValued;
			}
			set
			{
				this._isSingleValued = value;
			}
		}

		public bool IsSystemOnly
		{
			get
			{
				return this._isSystemOnly;
			}
			set
			{
				this._isSystemOnly = value;
			}
		}

		public int? LinkID
		{
			get
			{
				return this._linkID;
			}
			set
			{
				this._linkID = value;
			}
		}

		public ADAttributeSyntax Syntax
		{
			get
			{
				return this._syntax;
			}
			set
			{
				this._syntax = value;
			}
		}

		public ADSchemaAttribute(ADAttributeSyntax syntax, bool isSingleValued, bool isSystemOnly, int? linkID, bool isConstructed)
		{
			this._syntax = syntax;
			this._isSingleValued = isSingleValued;
			this._isSystemOnly = isSystemOnly;
			this._linkID = linkID;
			this._isConstructed = isConstructed;
		}

		public ADSchemaAttribute(ADAttributeSyntax syntax, bool isSingleValued, bool isSystemOnly) : this(syntax, isSingleValued, isSystemOnly, null, false)
		{
		}

		public ADSchemaAttribute(ADAttributeSyntax syntax, bool isSingleValued, bool isSystemOnly, bool isConstructed) : this(syntax, isSingleValued, isSystemOnly, null, isConstructed)
		{
		}

		public ADSchemaAttribute(ADAttributeSyntax syntax, bool isSingleValued, bool isSystemOnly, int linkID) : this(syntax, isSingleValued, isSystemOnly, new int?(linkID), false)
		{
		}
	}
}