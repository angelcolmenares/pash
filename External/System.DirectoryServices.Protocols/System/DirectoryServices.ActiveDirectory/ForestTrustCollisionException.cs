using System;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[Serializable]
	public class ForestTrustCollisionException : ActiveDirectoryOperationException, ISerializable
	{
		private ForestTrustRelationshipCollisionCollection collisions;

		public ForestTrustRelationshipCollisionCollection Collisions
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.collisions;
			}
		}

		public ForestTrustCollisionException(string message, Exception inner, ForestTrustRelationshipCollisionCollection collisions) : base(message, inner)
		{
			this.collisions = new ForestTrustRelationshipCollisionCollection();
			this.collisions = collisions;
		}

		public ForestTrustCollisionException(string message, Exception inner) : base(message, inner)
		{
			this.collisions = new ForestTrustRelationshipCollisionCollection();
		}

		public ForestTrustCollisionException(string message) : base(message)
		{
			this.collisions = new ForestTrustRelationshipCollisionCollection();
		}

		public ForestTrustCollisionException() : base(Res.GetString("ForestTrustCollision"))
		{
			this.collisions = new ForestTrustRelationshipCollisionCollection();
		}

		protected ForestTrustCollisionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.collisions = new ForestTrustRelationshipCollisionCollection();
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}