namespace Microsoft.PowerShell.Commands.Internal
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    internal static class Buffer
    {
        internal static unsafe int IndexOfByte(byte* src, byte value, int index, int count)
        {
            byte* numPtr = src + index;
            while ((((int) numPtr) & 3) != 0)
            {
                if (count == 0)
                {
                    return -1;
                }
                if (numPtr[0] == value)
                {
                    return (int) ((long) ((numPtr - src) / 1));
                }
                count--;
                numPtr++;
            }
            int num = (int) ((value << 8) + value);
            num = (num << 0x10) + num;
            while (count > 3)
            {
                int num2 = *((int*) numPtr);
                num2 ^= num;
                int num3 = 0x7efefeff + num2;
                num2 ^= int.MaxValue;
                num2 ^= num3;
                if ((num2 & 0x81010100) != 0)
                {
                    int num4 = (int) ((long) ((numPtr - src) / 1));
                    if (numPtr[0] == value)
                    {
                        return num4;
                    }
                    if (numPtr[1] == value)
                    {
                        return (num4 + 1);
                    }
                    if (numPtr[2] == value)
                    {
                        return (num4 + 2);
                    }
                    if (numPtr[3] == value)
                    {
                        return (num4 + 3);
                    }
                }
                count -= 4;
                numPtr += 4;
            }
            while (count > 0)
            {
                if (numPtr[0] == value)
                {
                    return (int) ((long) ((numPtr - src) / 1));
                }
                count--;
                numPtr++;
            }
            return -1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void memcpy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                fixed (byte* numRef = src)
                {
                    memcpyimpl(numRef + srcIndex, pDest + destIndex, len);
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void memcpy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            if (len != 0)
            {
                fixed (byte* numRef = dest)
                {
                    memcpyimpl(src + srcIndex, numRef + destIndex, len);
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void memcpy(char* pSrc, int srcIndex, char* pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                memcpyimpl((byte*) (pSrc + srcIndex), (byte*) (pDest + destIndex), len * 2);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void memcpyimpl(byte* src, byte* dest, int len)
        {
            if (len >= 0x10)
            {
                do
                {
                    *((int*) dest) = *((int*) src);
                    *((int*) (dest + 4)) = *((int*) (src + 4));
                    *((int*) (dest + 8)) = *((int*) (src + 8));
                    *((int*) (dest + 12)) = *((int*) (src + 12));
                    dest += 0x10;
                    src += 0x10;
                }
                while ((len -= 0x10) >= 0x10);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *((int*) dest) = *((int*) src);
                    *((int*) (dest + 4)) = *((int*) (src + 4));
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *((int*) dest) = *((int*) src);
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *((short*) dest) = *((short*) src);
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                {
                    dest++;
                    src++;
                    dest[0] = src[0];
                }
            }
        }

        internal static unsafe void ZeroMemory(byte* src, long len)
        {
            while (true)
            {
                len -= 1L;
                if (len <= 0L)
                {
                    return;
                }
                src[(int) len] = 0;
            }
        }
    }
}

