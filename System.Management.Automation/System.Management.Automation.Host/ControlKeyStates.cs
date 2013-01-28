namespace System.Management.Automation.Host
{
    using System;

    [Flags]
    public enum ControlKeyStates
    {
        CapsLockOn = 0x80,
        EnhancedKey = 0x100,
        LeftAltPressed = 2,
        LeftCtrlPressed = 8,
        NumLockOn = 0x20,
        RightAltPressed = 1,
        RightCtrlPressed = 4,
        ScrollLockOn = 0x40,
        ShiftPressed = 0x10
    }
}

