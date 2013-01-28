namespace System.Management.Automation.Tracing
{
    using System;

    public interface IEtwEventCorrelator
    {
        IEtwActivityReverter StartActivity();
        IEtwActivityReverter StartActivity(Guid relatedActivityId);

        Guid CurrentActivityId { get; set; }
    }
}

