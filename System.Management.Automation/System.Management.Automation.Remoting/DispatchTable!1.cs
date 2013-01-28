namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class DispatchTable<T> where T: class
    {
        private long _nextCallId;
        private Dictionary<long, AsyncObject<T>> _responseAsyncObjects;
        internal const long VoidCallId = -100L;

        public DispatchTable()
        {
            this._responseAsyncObjects = new Dictionary<long, AsyncObject<T>>();
        }

        internal void AbortAllCalls()
        {
            lock (this._responseAsyncObjects)
            {
                List<long> allCalls = this.GetAllCalls();
                this.AbortCalls(allCalls);
            }
        }

        private void AbortCall(long callId)
        {
            if (this._responseAsyncObjects.ContainsKey(callId))
            {
                this.GetResponseAsyncObject(callId).Value = default(T);
            }
        }

        private void AbortCalls(List<long> callIds)
        {
            foreach (long num in callIds)
            {
                this.AbortCall(num);
            }
        }

        internal long CreateNewCallId()
        {
            long num = Interlocked.Increment(ref this._nextCallId);
            AsyncObject<T> obj2 = new AsyncObject<T>();
            lock (this._responseAsyncObjects)
            {
                this._responseAsyncObjects[num] = obj2;
            }
            return num;
        }

        private List<long> GetAllCalls()
        {
            List<long> list = new List<long>();
            foreach (KeyValuePair<long, AsyncObject<T>> pair in this._responseAsyncObjects)
            {
                list.Add(pair.Key);
            }
            return list;
        }

        internal T GetResponse(long callId, T defaultValue)
        {
            AsyncObject<T> responseAsyncObject = null;
            lock (this._responseAsyncObjects)
            {
                responseAsyncObject = this.GetResponseAsyncObject(callId);
            }
            T local = responseAsyncObject.Value;
            lock (this._responseAsyncObjects)
            {
                this._responseAsyncObjects.Remove(callId);
            }
            if (local == null)
            {
                return defaultValue;
            }
            return local;
        }

        private AsyncObject<T> GetResponseAsyncObject(long callId)
        {
            return this._responseAsyncObjects[callId];
        }

        internal void SetResponse(long callId, T remoteHostResponse)
        {
            lock (this._responseAsyncObjects)
            {
                if (this._responseAsyncObjects.ContainsKey(callId))
                {
                    this.GetResponseAsyncObject(callId).Value = remoteHostResponse;
                }
            }
        }
    }
}

