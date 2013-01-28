namespace System.Spatial
{
    using System;

    internal class ActionOnDispose : IDisposable
    {
        private Action action;

        public ActionOnDispose(Action action)
        {
            Util.CheckArgumentNull(action, "action");
            this.action = action;
        }

        public void Dispose()
        {
            if (this.action != null)
            {
                this.action();
                this.action = null;
            }
        }
    }
}

