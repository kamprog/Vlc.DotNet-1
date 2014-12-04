﻿using System;
using System.Runtime.InteropServices;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Core.Interops.Signatures;

namespace Vlc.DotNet.Core
{
    public sealed partial class VlcMediaPlayer
    {
        private EventCallback myOnMediaPlayerMediaChangedInternalEventCallback;
        public event EventHandler<VlcMediaPlayerMediaChangedEventArgs> MediaChanged;

        private void OnMediaPlayerMediaChangedInternal(IntPtr ptr)
        {
            var args = (VlcEventArg) Marshal.PtrToStructure(ptr, typeof (VlcEventArg));

            foreach (var vlcMedia in Medias)
            {
                if (vlcMedia.MediaInstance == args.MediaPlayerMediaChanged.MediaInstance)
                {
                    OnMediaPlayerMediaChanged(vlcMedia);
                    return;
                }
            }
            OnMediaPlayerMediaChanged(new VlcMedia(this, new VlcMediaInstance(Manager, args.MediaPlayerMediaChanged.MediaInstance)));
        }

        public void OnMediaPlayerMediaChanged(VlcMedia media)
        {
            var del = MediaChanged;
            if (del != null)
                del(this, new VlcMediaPlayerMediaChangedEventArgs(media));
        }
    }
}