﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Vlc.DotNet.Core.Interops
{
    public abstract class VlcInteropsManager : IDisposable
    {
        private readonly Dictionary<string, Delegate> myInteropDelegates = new Dictionary<string, Delegate>();
        private IntPtr myLibVlcDllHandle;
        private IntPtr myLibVlcCoreDllHandle;

        public VlcVersions VlcVersion
        {
            get
            {
#if !NET20
                return VlcVersions.GetVersion(GetInteropDelegate<Signatures.Common.GetVersion>().Invoke().ToStringAnsi());
#else
                return VlcVersions.GetVersion(IntPtrExtensions.ToStringAnsi(GetInteropDelegate<Signatures.Common.GetVersion>().Invoke()));
#endif
            }
        }

        public VlcInteropsManager(DirectoryInfo dynamicLinkLibrariesPath)
        {
            if (!dynamicLinkLibrariesPath.Exists)
                throw new DirectoryNotFoundException();

            var libVlcCoreDllPath = Path.Combine(dynamicLinkLibrariesPath.FullName, "libvlccore.dll");
            if (!File.Exists(libVlcCoreDllPath))
                throw new FileNotFoundException();

            var libVlcDllPath = Path.Combine(dynamicLinkLibrariesPath.FullName, "libvlc.dll");
            if (!File.Exists(libVlcDllPath))
                throw new FileNotFoundException();

            myLibVlcCoreDllHandle = Win32Interops.LoadLibrary(libVlcCoreDllPath);
            if (myLibVlcCoreDllHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            myLibVlcDllHandle = Win32Interops.LoadLibrary(libVlcDllPath);
            if (myLibVlcDllHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        internal T GetInteropDelegate<T>()
        {
            string vlcFunctionName = null;
            try
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(LibVlcFunctionAttribute), false);
                if (attrs.Length == 0)
                    throw new Exception("Could not find the LibVlcFunctionAttribute.");
                var attr = (LibVlcFunctionAttribute)attrs[0];
                vlcFunctionName = attr.FunctionName;
                if (myInteropDelegates.ContainsKey(vlcFunctionName))
                    return (T)Convert.ChangeType(myInteropDelegates[attr.FunctionName], typeof(T), null);
                var procAddress = Win32Interops.GetProcAddress(myLibVlcDllHandle, attr.FunctionName);
                if (procAddress == IntPtr.Zero)
                    throw new Win32Exception();
                var delegateForFunctionPointer = Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T));
                myInteropDelegates[attr.FunctionName] = delegateForFunctionPointer;
                return (T)Convert.ChangeType(delegateForFunctionPointer, typeof(T), null);
            }
            catch (Win32Exception e)
            {
                throw new MissingMethodException(String.Format("The address of the function '{0}' does not exist in libvlc library.", vlcFunctionName), e);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (myLibVlcDllHandle != IntPtr.Zero)
            {
                Win32Interops.FreeLibrary(myLibVlcDllHandle);
                myLibVlcDllHandle = IntPtr.Zero;
            }
            if (myLibVlcCoreDllHandle != IntPtr.Zero)
            {
                Win32Interops.FreeLibrary(myLibVlcCoreDllHandle);
                myLibVlcCoreDllHandle = IntPtr.Zero;
            }
        }

        ~VlcInteropsManager()
        {
            Dispose(false);
        }

    }
}
