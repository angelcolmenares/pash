using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    public static class NativeUnsignedExtensions
    {
        public static int ToInt32(this uint obj)
        {
            return Convert.ToInt32(obj);
        }
    }
}

/*
[CompilerGenerated]
internal sealed class <>f__AnonymousType0<<entry>j__TPar, <completionText>j__TPar>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly <completionText>j__TPar <completionText>i__Field;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly <entry>j__TPar <entry>i__Field;

    [DebuggerHidden]
    public <>f__AnonymousType0(<entry>j__TPar entry, <completionText>j__TPar completionText)
    {
        this.<entry>i__Field = entry;
        this.<completionText>i__Field = completionText;
    }

    [DebuggerHidden]
    public override bool Equals(object value)
    {
        var type = value as <>f__AnonymousType0<<entry>j__TPar, <completionText>j__TPar>;
        return (((type != null) && EqualityComparer<<entry>j__TPar>.Default.Equals(this.<entry>i__Field, type.<entry>i__Field)) && EqualityComparer<<completionText>j__TPar>.Default.Equals(this.<completionText>i__Field, type.<completionText>i__Field));
    }

    [DebuggerHidden]
    public override int GetHashCode()
    {
        int num = -1547396885;
        num = (-1521134295 * num) + EqualityComparer<<entry>j__TPar>.Default.GetHashCode(this.<entry>i__Field);
        return ((-1521134295 * num) + EqualityComparer<<completionText>j__TPar>.Default.GetHashCode(this.<completionText>i__Field));
    }

    [DebuggerHidden]
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("{ entry = ");
        builder.Append(this.<entry>i__Field);
        builder.Append(", completionText = ");
        builder.Append(this.<completionText>i__Field);
        builder.Append(" }");
        return builder.ToString();
    }

    public <completionText>j__TPar completionText
    {
        get
        {
            return this.<completionText>i__Field;
        }
    }

    public <entry>j__TPar entry
    {
        get
        {
            return this.<entry>i__Field;
        }
    }
}

*/