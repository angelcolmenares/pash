namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    public sealed class PathInfoStack : Stack<PathInfo>
    {
        private string stackName;

        internal PathInfoStack(string stackName, Stack<PathInfo> locationStack)
        {
            if (locationStack == null)
            {
                throw PSTraceSource.NewArgumentNullException("locationStack");
            }
            if (string.IsNullOrEmpty(stackName))
            {
                throw PSTraceSource.NewArgumentException("stackName");
            }
            this.stackName = stackName;
            PathInfo[] array = new PathInfo[locationStack.Count];
            locationStack.CopyTo(array, 0);
            for (int i = array.Length - 1; i >= 0; i--)
            {
                base.Push(array[i]);
            }
        }

        public string Name
        {
            get
            {
                return this.stackName;
            }
        }
    }
}

