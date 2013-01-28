namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class FormattedObjectsCache
    {
        private Queue<PacketInfoData> frontEndQueue;
        private OutputGroupQueue groupQueue;

        internal FormattedObjectsCache(bool cacheFrontEnd)
        {
            if (cacheFrontEnd)
            {
                this.frontEndQueue = new Queue<PacketInfoData>();
            }
        }

        internal List<PacketInfoData> Add(PacketInfoData o)
        {
            if ((this.frontEndQueue == null) && (this.groupQueue == null))
            {
                return new List<PacketInfoData> { o };
            }
            if (this.frontEndQueue != null)
            {
                this.frontEndQueue.Enqueue(o);
                return null;
            }
            return this.groupQueue.Add(o);
        }

        internal List<PacketInfoData> Drain()
        {
            PacketInfoData data2;
            if ((this.frontEndQueue == null) && (this.groupQueue == null))
            {
                return null;
            }
            List<PacketInfoData> list = new List<PacketInfoData>();
            if (this.frontEndQueue != null)
            {
                if (this.groupQueue != null)
                {
                    while (this.frontEndQueue.Count > 0)
                    {
                        List<PacketInfoData> list2 = this.groupQueue.Add(this.frontEndQueue.Dequeue());
                        if (list2 != null)
                        {
                            foreach (PacketInfoData data in list2)
                            {
                                list.Add(data);
                            }
                        }
                    }
                }
                else
                {
                    while (this.frontEndQueue.Count > 0)
                    {
                        list.Add(this.frontEndQueue.Dequeue());
                    }
                    return list;
                }
            }
        Label_00A8:
            data2 = this.groupQueue.Dequeue();
            if (data2 != null)
            {
                list.Add(data2);
                goto Label_00A8;
            }
            return list;
        }

        internal void EnableGroupCaching(ProcessCachedGroupNotification callBack, int objectCount)
        {
            if (callBack != null)
            {
                this.groupQueue = new OutputGroupQueue(callBack, objectCount);
            }
        }

        internal delegate void ProcessCachedGroupNotification(FormatStartData formatStartData, List<PacketInfoData> objects);
    }
}

