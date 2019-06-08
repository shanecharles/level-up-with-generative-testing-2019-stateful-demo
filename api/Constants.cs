using System.Collections.Generic;
using System;

namespace Constants
{
    public static class ShiftAction
    {
        public const string StartFloor = "StartFloor";
        public const string EndFloor = "EndFloor";
        public const string StartBreak = "StartBreak";
        public const string EndBreak = "EndBreak"; 
        public static readonly HashSet<string> ActionLookup = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase){
            StartFloor,
            StartBreak,
            EndFloor,
            EndBreak
        };
    }
}