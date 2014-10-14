﻿using System;
using Vlc.DotNet.Core.Interops.Signatures.Rincewind;

namespace Vlc.DotNet.Core.Interops
{
    public sealed partial class VlcRincewindManager
    {
        public IntPtr GetMediaFromMediaPlayer(IntPtr mediaPlayerInstance)
        {
            if (mediaPlayerInstance == IntPtr.Zero)
                return IntPtr.Zero;
            return GetInteropDelegate<GetMediaFromMediaPlayer>().Invoke(mediaPlayerInstance);
        }
    }
}
