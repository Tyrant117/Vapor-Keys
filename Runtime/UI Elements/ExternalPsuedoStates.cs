using System;

namespace VaporKeys.UIElements
{
    [Flags]
    public enum ExternalPseudoStates
    {
        Active = 0x1,
        Hover = 0x2,
        Checked = 0x8,
        Disabled = 0x20,
        Focus = 0x40,
        Root = 0x80
    }
}
