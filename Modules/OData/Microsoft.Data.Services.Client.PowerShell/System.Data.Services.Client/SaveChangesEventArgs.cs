namespace System.Data.Services.Client
{
    using System;

    internal class SaveChangesEventArgs : EventArgs
    {
        private DataServiceResponse response;

        public SaveChangesEventArgs(DataServiceResponse response)
        {
            this.response = response;
        }
    }
}

