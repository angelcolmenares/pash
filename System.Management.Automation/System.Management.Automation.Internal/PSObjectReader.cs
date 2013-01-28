namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Runtime.InteropServices;

    internal class PSObjectReader : ObjectReaderBase<PSObject>
    {
        public PSObjectReader([In, Out] ObjectStream stream) : base(stream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base._stream.Close();
            }
        }

        private static PSObject MakePSObject(object o)
        {
            if (o == null)
            {
                return null;
            }
            return PSObject.AsPSObject(o);
        }

        private static Collection<PSObject> MakePSObjectCollection(Collection<object> coll)
        {
            if (coll == null)
            {
                return null;
            }
            Collection<PSObject> collection = new Collection<PSObject>();
            foreach (object obj2 in coll)
            {
                collection.Add(MakePSObject(obj2));
            }
            return collection;
        }

        public override Collection<PSObject> NonBlockingRead()
        {
            return MakePSObjectCollection(base._stream.NonBlockingRead(0x7fffffff));
        }

        public override Collection<PSObject> NonBlockingRead(int maxRequested)
        {
            return MakePSObjectCollection(base._stream.NonBlockingRead(maxRequested));
        }

        public override PSObject Peek()
        {
            return MakePSObject(base._stream.Peek());
        }

        public override PSObject Read()
        {
            return MakePSObject(base._stream.Read());
        }

        public override Collection<PSObject> Read(int count)
        {
            return MakePSObjectCollection(base._stream.Read(count));
        }

        public override Collection<PSObject> ReadToEnd()
        {
            return MakePSObjectCollection(base._stream.ReadToEnd());
        }
    }
}

