using System;
using System.Runtime;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ForestTrustRelationshipCollision
	{
		private ForestTrustCollisionType type;

		private TopLevelNameCollisionOptions tlnFlag;

		private DomainCollisionOptions domainFlag;

		private string record;

		public string CollisionRecord
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.record;
			}
		}

		public ForestTrustCollisionType CollisionType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.type;
			}
		}

		public DomainCollisionOptions DomainCollisionOption
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.domainFlag;
			}
		}

		public TopLevelNameCollisionOptions TopLevelNameCollisionOption
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.tlnFlag;
			}
		}

		internal ForestTrustRelationshipCollision(ForestTrustCollisionType collisionType, TopLevelNameCollisionOptions TLNFlag, DomainCollisionOptions domainFlag, string record)
		{
			this.type = collisionType;
			this.tlnFlag = TLNFlag;
			this.domainFlag = domainFlag;
			this.record = record;
		}
	}
}