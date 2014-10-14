﻿using System;
using System.Runtime.InteropServices;
using Vlc.DotNet.Core.Interops.Signatures.Rincewind;

namespace Vlc.DotNet.Core.Rincewind
{
    public partial class VlcMediaPlayer
    {
        public event EventHandler<VlcMediaPlayerSnapshotTakenEventArgs> SnapshotTaken;

        private EventCallback myOnMediaPlayerSnapshotTakenInternalEventCallback;

        private void OnMediaPlayerSnapshotTakenInternal(IntPtr ptr)
        {
            var args = (VlcEventArg)Marshal.PtrToStructure(ptr, typeof(VlcEventArg));
            var fileName = Marshal.PtrToStringAnsi(args.MediaPlayerSnapshotTaken.pszFilename);
            OnMediaPlayerSnapshotTaken(fileName);
        }

        public void OnMediaPlayerSnapshotTaken(string fileName)
        {
            var del = SnapshotTaken;
            if (del != null)
                del(this, new VlcMediaPlayerSnapshotTakenEventArgs(fileName));
        }
    }
}
