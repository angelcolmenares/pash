namespace System.Management.Automation.Remoting
{
    using System;

    internal class FragmentedRemoteObject
    {
        private byte[] _blob;
        private int _blobLength;
        private const int _blobLengthOffset = 0x11;
        private const int _blobOffset = 0x15;
        private const int _flagsOffset = 0x10;
        private long _fragmentId;
        private const int _fragmentIdOffset = 8;
        private bool _isEndFragment;
        private bool _isStartFragment;
        private long _objectId;
        private const int _objectIdOffset = 0;
        internal const byte EFlag = 2;
        internal const int HeaderLength = 0x15;
        internal const byte SFlag = 1;

        internal FragmentedRemoteObject()
        {

        }

        internal FragmentedRemoteObject (byte[] blob, long objectId, long fragmentId, bool isEndFragment)
		{
			this._objectId = objectId;
			this._fragmentId = fragmentId;
			this._isStartFragment = fragmentId == 0L;
			this._isEndFragment = isEndFragment;
			this._blob = blob;
			this._blobLength = this._blob.Length;
			if (this._objectId == 0) {
				this._objectId = 1;
			}
        }

        internal static int GetBlobLength(byte[] fragmentBytes, int startIndex)
        {
            int num = 0;
            int num2 = startIndex + 0x11;
            num += (fragmentBytes[num2++] << 0x18) & 0x7f000000;
            num += (fragmentBytes[num2++] << 0x10) & 0xff0000;
            num += (fragmentBytes[num2++] << 8) & 0xff00;
            return (num + (fragmentBytes[num2++] & 0xff));
        }

        internal byte[] GetBytes()
        {
            int num = 8;
            int num2 = 8;
            int num3 = 1;
            int num4 = 4;
            int num5 = (((num + num2) + num3) + num4) + this.BlobLength;
            byte[] destinationArray = new byte[num5];
            int num6 = 0;
            num6 = 0;
            destinationArray[num6++] = (byte) ((this.ObjectId >> 0x38) & 0x7fL);
            destinationArray[num6++] = (byte) ((this.ObjectId >> 0x30) & 0xffL);
            destinationArray[num6++] = (byte) ((this.ObjectId >> 40) & 0xffL);
            destinationArray[num6++] = (byte) ((this.ObjectId >> 0x20) & 0xffL);
            destinationArray[num6++] = (byte) ((this.ObjectId >> 0x18) & 0xffL);
            destinationArray[num6++] = (byte) ((this.ObjectId >> 0x10) & 0xffL);
            destinationArray[num6++] = (byte) ((this.ObjectId >> 8) & 0xffL);
            destinationArray[num6++] = (byte) (this.ObjectId & 0xffL);
            num6 = 8;
            destinationArray[num6++] = (byte) ((this.FragmentId >> 0x38) & 0x7fL);
            destinationArray[num6++] = (byte) ((this.FragmentId >> 0x30) & 0xffL);
            destinationArray[num6++] = (byte) ((this.FragmentId >> 40) & 0xffL);
            destinationArray[num6++] = (byte) ((this.FragmentId >> 0x20) & 0xffL);
            destinationArray[num6++] = (byte) ((this.FragmentId >> 0x18) & 0xffL);
            destinationArray[num6++] = (byte) ((this.FragmentId >> 0x10) & 0xffL);
            destinationArray[num6++] = (byte) ((this.FragmentId >> 8) & 0xffL);
            destinationArray[num6++] = (byte) (this.FragmentId & 0xffL);
            num6 = 0x10;
            byte num7 = this.IsStartFragment ? ((byte) 1) : ((byte) 0);
            byte num8 = this.IsEndFragment ? ((byte) 2) : ((byte) 0);
            destinationArray[num6++] = (byte) (num7 | num8);
            num6 = 0x11;
            destinationArray[num6++] = (byte) ((this.BlobLength >> 0x18) & 0xff);
            destinationArray[num6++] = (byte) ((this.BlobLength >> 0x10) & 0xff);
            destinationArray[num6++] = (byte) ((this.BlobLength >> 8) & 0xff);
            destinationArray[num6++] = (byte) (this.BlobLength & 0xff);
            Array.Copy(this._blob, 0, destinationArray, 0x15, this.BlobLength);
            return destinationArray;
        }

        internal static long GetFragmentId(byte[] fragmentBytes, int startIndex)
        {
            long num = 0L;
            int num2 = startIndex + 8;
            num = (fragmentBytes[num2++] << 0x38) & 0x7f00000000000000L;
            num += (fragmentBytes[num2++] << 0x30) & 0xff000000000000L;
            num += (fragmentBytes[num2++] << 40) & 0xff0000000000L;
            num += (fragmentBytes[num2++] << 0x20) & 0xff00000000L;
            num += (long) ((fragmentBytes[num2++] << 0x18) & 0xff000000L);
            num += (fragmentBytes[num2++] << 0x10) & 0xff0000L;
            num += (fragmentBytes[num2++] << 8) & 0xff00L;
            return (num + (fragmentBytes[num2++] & 0xffL));
        }

        internal static bool GetIsEndFragment(byte[] fragmentBytes, int startIndex)
        {
            return ((fragmentBytes[startIndex + 0x10] & 2) != 0);
        }

        internal static bool GetIsStartFragment(byte[] fragmentBytes, int startIndex)
        {
            return ((fragmentBytes[startIndex + 0x10] & 1) != 0);
        }

        internal static long GetObjectId(byte[] fragmentBytes, int startIndex)
        {
            long num = 0L;
            int num2 = startIndex;
            num = (fragmentBytes[num2++] << 0x38) & 0x7f00000000000000L;
            num += (fragmentBytes[num2++] << 0x30) & 0xff000000000000L;
            num += (fragmentBytes[num2++] << 40) & 0xff0000000000L;
            num += (fragmentBytes[num2++] << 0x20) & 0xff00000000L;
            num += (long) ((fragmentBytes[num2++] << 0x18) & 0xff000000L);
            num += (fragmentBytes[num2++] << 0x10) & 0xff0000L;
            num += (fragmentBytes[num2++] << 8) & 0xff00L;
            return (num + (fragmentBytes[num2++] & 0xffL));
        }

        internal byte[] Blob
        {
            get
            {
                return this._blob;
            }
            set
            {
                this._blob = value;
            }
        }

        internal int BlobLength
        {
            get
            {
                return this._blobLength;
            }
            set
            {
                this._blobLength = value;
            }
        }

        internal long FragmentId
        {
            get
            {
                return this._fragmentId;
            }
            set
            {
                this._fragmentId = value;
            }
        }

        internal bool IsEndFragment
        {
            get
            {
                return this._isEndFragment;
            }
            set
            {
                this._isEndFragment = value;
            }
        }

        internal bool IsStartFragment
        {
            get
            {
                return this._isStartFragment;
            }
            set
            {
                this._isStartFragment = value;
            }
        }

        internal long ObjectId
        {
            get
            {
                return this._objectId;
            }
            set
            {
                this._objectId = value;
            }
        }
    }
}

