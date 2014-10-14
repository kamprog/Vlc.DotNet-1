﻿using System;
using Vlc.DotNet.Core.Interops.Signatures.Rincewind;

namespace Vlc.DotNet.Core.Interops
{
    public sealed partial class VlcRincewindManager
    {
        public string GetLastErrorMessage()
        {
#if !NET20
            return GetInteropDelegate<GetLastErrorMessage>().Invoke().ToStringAnsi();
#else
            return IntPtrExtensions.ToStringAnsi(GetInteropDelegate<GetLastErrorMessage>().Invoke());
#endif
        }
    }
}
