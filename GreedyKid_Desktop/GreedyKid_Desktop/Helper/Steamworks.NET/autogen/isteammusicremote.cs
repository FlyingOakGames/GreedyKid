// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if STEAM

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks {
	public static class SteamMusicRemote {
		/// <summary>
		/// <para> Service Definition</para>
		/// </summary>
		public static bool RegisterSteamMusicRemote(string pchName) {
			InteropHelp.TestIfAvailableClient();
			using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName)) {
				return NativeMethods.ISteamMusicRemote_RegisterSteamMusicRemote(CSteamAPIContext.GetSteamMusicRemote(), pchName2);
			}
		}

		public static bool DeregisterSteamMusicRemote() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_DeregisterSteamMusicRemote(CSteamAPIContext.GetSteamMusicRemote());
		}

		public static bool BIsCurrentMusicRemote() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_BIsCurrentMusicRemote(CSteamAPIContext.GetSteamMusicRemote());
		}

		public static bool BActivationSuccess(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_BActivationSuccess(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		public static bool SetDisplayName(string pchDisplayName) {
			InteropHelp.TestIfAvailableClient();
			using (var pchDisplayName2 = new InteropHelp.UTF8StringHandle(pchDisplayName)) {
				return NativeMethods.ISteamMusicRemote_SetDisplayName(CSteamAPIContext.GetSteamMusicRemote(), pchDisplayName2);
			}
		}

		public static bool SetPNGIcon_64x64(byte[] pvBuffer, uint cbBufferLength) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_SetPNGIcon_64x64(CSteamAPIContext.GetSteamMusicRemote(), pvBuffer, cbBufferLength);
		}

		/// <summary>
		/// <para> Abilities for the user interface</para>
		/// </summary>
		public static bool EnablePlayPrevious(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_EnablePlayPrevious(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		public static bool EnablePlayNext(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_EnablePlayNext(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		public static bool EnableShuffled(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_EnableShuffled(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		public static bool EnableLooped(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_EnableLooped(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		public static bool EnableQueue(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_EnableQueue(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		public static bool EnablePlaylists(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_EnablePlaylists(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		/// <summary>
		/// <para> Status</para>
		/// </summary>
		public static bool UpdatePlaybackStatus(AudioPlayback_Status nStatus) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_UpdatePlaybackStatus(CSteamAPIContext.GetSteamMusicRemote(), nStatus);
		}

		public static bool UpdateShuffled(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_UpdateShuffled(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		public static bool UpdateLooped(bool bValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_UpdateLooped(CSteamAPIContext.GetSteamMusicRemote(), bValue);
		}

		/// <summary>
		/// <para> volume is between 0.0 and 1.0</para>
		/// </summary>
		public static bool UpdateVolume(float flValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_UpdateVolume(CSteamAPIContext.GetSteamMusicRemote(), flValue);
		}

		/// <summary>
		/// <para> Current Entry</para>
		/// </summary>
		public static bool CurrentEntryWillChange() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_CurrentEntryWillChange(CSteamAPIContext.GetSteamMusicRemote());
		}

		public static bool CurrentEntryIsAvailable(bool bAvailable) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_CurrentEntryIsAvailable(CSteamAPIContext.GetSteamMusicRemote(), bAvailable);
		}

		public static bool UpdateCurrentEntryText(string pchText) {
			InteropHelp.TestIfAvailableClient();
			using (var pchText2 = new InteropHelp.UTF8StringHandle(pchText)) {
				return NativeMethods.ISteamMusicRemote_UpdateCurrentEntryText(CSteamAPIContext.GetSteamMusicRemote(), pchText2);
			}
		}

		public static bool UpdateCurrentEntryElapsedSeconds(int nValue) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_UpdateCurrentEntryElapsedSeconds(CSteamAPIContext.GetSteamMusicRemote(), nValue);
		}

		public static bool UpdateCurrentEntryCoverArt(byte[] pvBuffer, uint cbBufferLength) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_UpdateCurrentEntryCoverArt(CSteamAPIContext.GetSteamMusicRemote(), pvBuffer, cbBufferLength);
		}

		public static bool CurrentEntryDidChange() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_CurrentEntryDidChange(CSteamAPIContext.GetSteamMusicRemote());
		}

		/// <summary>
		/// <para> Queue</para>
		/// </summary>
		public static bool QueueWillChange() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_QueueWillChange(CSteamAPIContext.GetSteamMusicRemote());
		}

		public static bool ResetQueueEntries() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_ResetQueueEntries(CSteamAPIContext.GetSteamMusicRemote());
		}

		public static bool SetQueueEntry(int nID, int nPosition, string pchEntryText) {
			InteropHelp.TestIfAvailableClient();
			using (var pchEntryText2 = new InteropHelp.UTF8StringHandle(pchEntryText)) {
				return NativeMethods.ISteamMusicRemote_SetQueueEntry(CSteamAPIContext.GetSteamMusicRemote(), nID, nPosition, pchEntryText2);
			}
		}

		public static bool SetCurrentQueueEntry(int nID) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_SetCurrentQueueEntry(CSteamAPIContext.GetSteamMusicRemote(), nID);
		}

		public static bool QueueDidChange() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_QueueDidChange(CSteamAPIContext.GetSteamMusicRemote());
		}

		/// <summary>
		/// <para> Playlist</para>
		/// </summary>
		public static bool PlaylistWillChange() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_PlaylistWillChange(CSteamAPIContext.GetSteamMusicRemote());
		}

		public static bool ResetPlaylistEntries() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_ResetPlaylistEntries(CSteamAPIContext.GetSteamMusicRemote());
		}

		public static bool SetPlaylistEntry(int nID, int nPosition, string pchEntryText) {
			InteropHelp.TestIfAvailableClient();
			using (var pchEntryText2 = new InteropHelp.UTF8StringHandle(pchEntryText)) {
				return NativeMethods.ISteamMusicRemote_SetPlaylistEntry(CSteamAPIContext.GetSteamMusicRemote(), nID, nPosition, pchEntryText2);
			}
		}

		public static bool SetCurrentPlaylistEntry(int nID) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_SetCurrentPlaylistEntry(CSteamAPIContext.GetSteamMusicRemote(), nID);
		}

		public static bool PlaylistDidChange() {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamMusicRemote_PlaylistDidChange(CSteamAPIContext.GetSteamMusicRemote());
		}
	}
}

#endif // STEAM
