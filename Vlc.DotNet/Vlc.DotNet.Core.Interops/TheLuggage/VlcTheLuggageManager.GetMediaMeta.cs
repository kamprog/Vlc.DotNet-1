﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using Vlc.DotNet.Core.Interops.Signatures.TheLuggage;

namespace Vlc.DotNet.Core.Interops
{
    public sealed partial class VlcTheLuggageManager
    {
        public string GetMediaMeta(IntPtr mediaInstance, MediaMetadatas metadata)
        {
            if (mediaInstance == IntPtr.Zero)
                throw new ArgumentException("Media instance is not initialized.");
            var ptr = GetInteropDelegate<GetMediaMetadata>().Invoke(mediaInstance, metadata);
            if (ptr == IntPtr.Zero)
                return null;
            return Marshal.PtrToStringUni(ptr);
        }
    }
}