namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Threading;

    internal class DiscardingPipelineWriter : PipelineWriter
    {
        private int count;
        private bool isOpen = true;
        private ManualResetEvent waitHandle = new ManualResetEvent(true);

        public override void Close()
        {
            this.isOpen = false;
        }

        public override void Flush()
        {
        }

        public override int Write(object obj)
        {
            int num = 1;
            this.count += num;
            return num;
        }

        public override int Write(object obj, bool enumerateCollection)
        {
            if (!enumerateCollection)
            {
                return this.Write(obj);
            }
            int num = 0;
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj);
            if (enumerable != null)
            {
                IEnumerator enumerator = enumerable.GetEnumerator();
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        num++;
                    }
                    goto Label_004A;
                }
            }
            num++;
        Label_004A:
            this.count += num;
            return num;
        }

        public override int Count
        {
            get
            {
                return this.count;
            }
        }

        public override bool IsOpen
        {
            get
            {
                return this.isOpen;
            }
        }

        public override int MaxCapacity
        {
            get
            {
                return 0x7fffffff;
            }
        }

        public override System.Threading.WaitHandle WaitHandle
        {
            get
            {
                return this.waitHandle;
            }
        }
    }
}

