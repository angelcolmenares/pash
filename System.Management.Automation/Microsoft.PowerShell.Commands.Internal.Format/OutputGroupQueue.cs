namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class OutputGroupQueue
    {
        private int currentObjectCount;
        private FormatStartData formatStartData;
        private FormattedObjectsCache.ProcessCachedGroupNotification notificationCallBack;
        private int objectCount;
        private bool processingGroup;
        private Queue<PacketInfoData> queue = new Queue<PacketInfoData>();

        internal OutputGroupQueue(FormattedObjectsCache.ProcessCachedGroupNotification callBack, int objectCount)
        {
            this.notificationCallBack = callBack;
            this.objectCount = objectCount;
        }

        internal List<PacketInfoData> Add(PacketInfoData o)
        {
            FormatStartData data = o as FormatStartData;
            if (data != null)
            {
                this.formatStartData = data;
            }
            this.UpdateObjectCount(o);
            if (!this.processingGroup && (o is GroupStartData))
            {
                this.processingGroup = true;
                this.currentObjectCount = 0;
                this.queue.Enqueue(o);
                return null;
            }
            if (this.processingGroup && ((o is GroupEndData) || ((this.objectCount > 0) && (this.currentObjectCount >= this.objectCount))))
            {
                this.currentObjectCount = 0;
                this.queue.Enqueue(o);
                this.Notify();
                this.processingGroup = false;
                List<PacketInfoData> list = new List<PacketInfoData>();
                while (this.queue.Count > 0)
                {
                    list.Add(this.queue.Dequeue());
                }
                return list;
            }
            if (this.processingGroup)
            {
                this.queue.Enqueue(o);
                return null;
            }
            return new List<PacketInfoData> { o };
        }

        internal PacketInfoData Dequeue()
        {
            if (this.queue.Count == 0)
            {
                return null;
            }
            return this.queue.Dequeue();
        }

        private void Notify()
        {
            if (this.notificationCallBack != null)
            {
                List<PacketInfoData> objects = new List<PacketInfoData>();
                foreach (PacketInfoData data in this.queue)
                {
                    FormatEntryData data2 = data as FormatEntryData;
                    if ((data2 == null) || !data2.outOfBand)
                    {
                        objects.Add(data);
                    }
                }
                this.notificationCallBack(this.formatStartData, objects);
            }
        }

        private void UpdateObjectCount(PacketInfoData o)
        {
            FormatEntryData data = o as FormatEntryData;
            if ((data != null) && !data.outOfBand)
            {
                this.currentObjectCount++;
            }
        }
    }
}

