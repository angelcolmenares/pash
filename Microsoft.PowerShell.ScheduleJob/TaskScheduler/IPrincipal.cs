namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, CompilerGenerated, Guid("D98D51E5-C9B4-496A-A9C1-18980261CF0F")]
    public interface IPrincipal
    {
        void _VtblGap1_10();
        _TASK_RUNLEVEL RunLevel { [DispId(6)] get; [param: In] [DispId(6)] set; }
    }
}

