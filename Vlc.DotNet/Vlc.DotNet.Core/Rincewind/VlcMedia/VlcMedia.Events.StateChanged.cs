﻿using System;
using System.Runtime.InteropServices;
using Vlc.DotNet.Core.Interops.Signatures.Rincewind;

namespace Vlc.DotNet.Core.Rincewind
{
    public partial class VlcMedia
    {
        public event EventHandler<VlcMediaStateChangedEventArgs> StateChanged;

        private EventCallback myOnMediaStateChangedInternalEventCallback;

        private void OnMediaStateChangedInternal(IntPtr ptr)
        {
            var args = (VlcEventArg)Marshal.PtrToStructure(ptr, typeof(VlcEventArg));
            OnMediaStateChanged(args.MediaStateChanged.NewState);
        }

        public void OnMediaStateChanged(MediaStates state)
        {
            var del = StateChanged;
            if (del != null)
                del(this, new VlcMediaStateChangedEventArgs(state));
        }
    }
}
