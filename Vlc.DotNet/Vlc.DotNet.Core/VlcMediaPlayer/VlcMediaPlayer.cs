﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Core.Interops.Signatures;

namespace Vlc.DotNet.Core
{
    public sealed partial class VlcMediaPlayer : IDisposable
    {
        private IntPtr myMediaPlayer;

        public VlcMediaPlayer(DirectoryInfo vlcLibDirectory)
            : this(VlcManager.GetInstance(vlcLibDirectory))
        {
        }

        internal VlcMediaPlayer(VlcManager manager)
        {
            Medias = new Collection<VlcMedia>();
            Manager = manager;
            Manager.CreateNewInstance(null);
            myMediaPlayer = manager.CreateMediaPlayer();
            RegisterEvents();
            Chapters = new ChapterManagement(manager, myMediaPlayer);
            SubTitles = new SubTitlesManagement(manager, myMediaPlayer);
            Video = new VideoManagement(manager, myMediaPlayer);
            Audio = new AudioManagement(manager, myMediaPlayer);
        }

        internal VlcManager Manager { get; private set; }
        internal Collection<VlcMedia> Medias { get; private set; }

        public IntPtr VideoHostHandle
        {
            get { return IntPtr.Zero; }
            set { Manager.SetMediaPlayerVideoHostHandle(myMediaPlayer, value); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (myMediaPlayer == IntPtr.Zero)
                return;
            if (IsPlaying())
                Stop();
            foreach (var media in Medias)
                media.Dispose();

            UnregisterEvents();
            Manager.ReleaseMediaPlayer(myMediaPlayer);
            myMediaPlayer = IntPtr.Zero;
        }

        ~VlcMediaPlayer()
        {
            Dispose(false);
        }

        public VlcMedia SetMedia(FileInfo file, params string[] options)
        {
            return SetMedia(new VlcMedia(this, file, options));
        }

        public VlcMedia SetMedia(Uri uri, params string[] options)
        {
            return SetMedia(new VlcMedia(this, uri, options));
        }

        public VlcMedia SetMedia(VlcMedia media)
        {
            Manager.SetMediaToMediaPlayer(myMediaPlayer, media.MediaInstance);
            return media;
        }

        public VlcMedia GetMedia()
        {
            var mediaPtr = Manager.GetMediaFromMediaPlayer(myMediaPlayer);
            foreach (var vlcMedia in Medias)
            {
                if (vlcMedia.MediaInstance == mediaPtr)
                    return vlcMedia;
            }
            return new VlcMedia(this, mediaPtr);
        }

        public void Play()
        {
            Manager.Play(myMediaPlayer);
        }

        public void Pause()
        {
            Manager.Pause(myMediaPlayer);
        }

        public void Stop()
        {
            Manager.Stop(myMediaPlayer);
        }

        public bool IsPlaying()
        {
            return Manager.IsPlaying(myMediaPlayer);
        }

        public bool IsPausable()
        {
            return Manager.IsPausable(myMediaPlayer);
        }

        public void NextFrame()
        {
            Manager.NextFrame(myMediaPlayer);
        }

        public IEnumerable<FilterModuleDescription> GetAudioFilters()
        {
            var module = Manager.GetAudioFilterList();
            ModuleDescriptionStructure nextModule = (ModuleDescriptionStructure)Marshal.PtrToStructure(module, typeof(ModuleDescriptionStructure));
            var result = GetSubFilter(nextModule);
            if (module != IntPtr.Zero)
                Manager.ReleaseModuleDescriptionInstance(module);
            return result;
        }

        private List<FilterModuleDescription> GetSubFilter(ModuleDescriptionStructure module)
        {
            var result = new List<FilterModuleDescription>();
            var filterModule = FilterModuleDescription.GetFilterModuleDescription(module);
            if (filterModule == null)
            {
                return result;
            }
            result.Add(filterModule);
            if (module.NextModule != IntPtr.Zero)
            {
                ModuleDescriptionStructure nextModule = (ModuleDescriptionStructure)Marshal.PtrToStructure(module.NextModule, typeof(ModuleDescriptionStructure));
                var data = GetSubFilter(nextModule);
                if (data.Count > 0)
                    result.AddRange(data);
            }
            return result;
        }

        public IEnumerable<FilterModuleDescription> GetVideoFilters()
        {
            var module = Manager.GetVideoFilterList();
            ModuleDescriptionStructure nextModule = (ModuleDescriptionStructure)Marshal.PtrToStructure(module, typeof(ModuleDescriptionStructure));
            var result = GetSubFilter(nextModule);
            if (module != IntPtr.Zero)
                Manager.ReleaseModuleDescriptionInstance(module);
            return result;
        }

        public float Position
        {
            get { return Manager.GetMediaPosition(myMediaPlayer); }
            set { Manager.SetMediaPosition(myMediaPlayer, value); }
        }

        public bool CouldPlay
        {
            get { return Manager.CouldPlay(myMediaPlayer); }
        }

        public IChapterManagement Chapters { get; private set; }

        public float Rate
        {
            get { return Manager.GetRate(myMediaPlayer); }
            set { Manager.SetRate(myMediaPlayer, value); }
        }

        public MediaStates State
        {
            get { return Manager.GetMediaPlayerState(myMediaPlayer); }
        }

        public float FramesPerSecond
        {
            get { return Manager.GetFramesPerSecond(myMediaPlayer); }
        }

        public bool IsSeekable
        {
            get { return Manager.IsSeekable(myMediaPlayer); }
        }

        public void Navigate(NavigateModes navigateMode)
        {
            Manager.Navigate(myMediaPlayer, navigateMode);
        }

        public ISubTitlesManagement SubTitles { get; private set; }

        public IVideoManagement Video { get; private set; }

        public IAudioManagement Audio { get; private set; }

        private void RegisterEvents()
        {
            var eventManager = Manager.GetMediaPlayerEventManager(myMediaPlayer);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerBackward, myOnMediaPlayerBackwardInternalEventCallback = OnMediaPlayerBackwardInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerBuffering, myOnMediaPlayerBufferingInternalEventCallback = OnMediaPlayerBufferingInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerEncounteredError, myOnMediaPlayerEncounteredErrorInternalEventCallback = OnMediaPlayerEncounteredErrorInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerEndReached, myOnMediaPlayerEndReachedInternalEventCallback = OnMediaPlayerEndReachedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerForward, myOnMediaPlayerForwardInternalEventCallback = OnMediaPlayerForwardInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerLengthChanged, myOnMediaPlayerLengthChangedInternalEventCallback = OnMediaPlayerLengthChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerMediaChanged, myOnMediaPlayerMediaChangedInternalEventCallback = OnMediaPlayerMediaChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerOpening, myOnMediaPlayerOpeningInternalEventCallback = OnMediaPlayerOpeningInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerPausableChanged, myOnMediaPlayerPausableChangedInternalEventCallback = OnMediaPlayerPausableChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerPaused, myOnMediaPlayerPausedInternalEventCallback = OnMediaPlayerPausedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerPlaying, myOnMediaPlayerPlayingInternalEventCallback = OnMediaPlayerPlayingInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerPositionChanged, myOnMediaPlayerPositionChangedInternalEventCallback = OnMediaPlayerPositionChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerScrambledChanged, myOnMediaPlayerScrambledChangedInternalEventCallback = OnMediaPlayerScrambledChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerSeekableChanged, myOnMediaPlayerSeekableChangedInternalEventCallback = OnMediaPlayerSeekableChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerSnapshotTaken, myOnMediaPlayerSnapshotTakenInternalEventCallback = OnMediaPlayerSnapshotTakenInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerStopped, myOnMediaPlayerStoppedInternalEventCallback = OnMediaPlayerStoppedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerTimeChanged, myOnMediaPlayerTimeChangedInternalEventCallback = OnMediaPlayerTimeChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerTitleChanged, myOnMediaPlayerTitleChangedInternalEventCallback = OnMediaPlayerTitleChangedInternal);
            Manager.AttachEvent(eventManager, EventTypes.MediaPlayerVout, myOnMediaPlayerVideoOutChangedInternalEventCallback = OnMediaPlayerVideoOutChangedInternal);
        }

        private void UnregisterEvents()
        {
            var eventManager = Manager.GetMediaPlayerEventManager(myMediaPlayer);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerBackward, myOnMediaPlayerBackwardInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerBuffering, myOnMediaPlayerBufferingInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerEncounteredError, myOnMediaPlayerEncounteredErrorInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerEndReached, myOnMediaPlayerEndReachedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerForward, myOnMediaPlayerForwardInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerLengthChanged, myOnMediaPlayerLengthChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerMediaChanged, myOnMediaPlayerMediaChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerOpening, myOnMediaPlayerOpeningInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerPausableChanged, myOnMediaPlayerPausableChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerPaused, myOnMediaPlayerPausedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerPlaying, myOnMediaPlayerPlayingInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerPositionChanged, myOnMediaPlayerPositionChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerScrambledChanged, myOnMediaPlayerScrambledChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerSeekableChanged, myOnMediaPlayerSeekableChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerSnapshotTaken, myOnMediaPlayerSnapshotTakenInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerStopped, myOnMediaPlayerStoppedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerTimeChanged, myOnMediaPlayerTimeChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerTitleChanged, myOnMediaPlayerTitleChangedInternalEventCallback);
            Manager.DetachEvent(eventManager, EventTypes.MediaPlayerVout, myOnMediaPlayerVideoOutChangedInternalEventCallback);
        }
    }
}