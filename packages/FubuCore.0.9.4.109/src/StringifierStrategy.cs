using System;

namespace FubuCore
{
    public class StringifierStrategy
    {
        public Func<GetStringRequest, bool> Matches;
        public Func<GetStringRequest, string> StringFunction;
    }
}