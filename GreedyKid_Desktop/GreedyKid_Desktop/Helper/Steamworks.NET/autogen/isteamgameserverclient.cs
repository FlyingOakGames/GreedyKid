// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if STEAM

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks {
	public static class SteamGameServerClient {
		/// <summary>
		/// <para> Creates a communication pipe to the Steam client.</para>
		/// <para> NOT THREADSAFE - ensure that no other threads are accessing Steamworks API when calling</para>
		/// </summary>
		public static HSteamPipe CreateSteamPipe() {
			InteropHelp.TestIfAvailableGameServer();
			return (HSteamPipe)NativeMethods.ISteamClient_CreateSteamPipe(CSteamGameServerAPIContext.GetSteamClient());
		}

		/// <summary>
		/// <para> Releases a previously created communications pipe</para>
		/// <para> NOT THREADSAFE - ensure that no other threads are accessing Steamworks API when calling</para>
		/// </summary>
		public static bool BReleaseSteamPipe(HSteamPipe hSteamPipe) {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamClient_BReleaseSteamPipe(CSteamGameServerAPIContext.GetSteamClient(), hSteamPipe);
		}

		/// <summary>
		/// <para> connects to an existing global user, failing if none exists</para>
		/// <para> used by the game to coordinate with the steamUI</para>
		/// <para> NOT THREADSAFE - ensure that no other threads are accessing Steamworks API when calling</para>
		/// </summary>
		public static HSteamUser ConnectToGlobalUser(HSteamPipe hSteamPipe) {
			InteropHelp.TestIfAvailableGameServer();
			return (HSteamUser)NativeMethods.ISteamClient_ConnectToGlobalUser(CSteamGameServerAPIContext.GetSteamClient(), hSteamPipe);
		}

		/// <summary>
		/// <para> used by game servers, create a steam user that won't be shared with anyone else</para>
		/// <para> NOT THREADSAFE - ensure that no other threads are accessing Steamworks API when calling</para>
		/// </summary>
		public static HSteamUser CreateLocalUser(out HSteamPipe phSteamPipe, EAccountType eAccountType) {
			InteropHelp.TestIfAvailableGameServer();
			return (HSteamUser)NativeMethods.ISteamClient_CreateLocalUser(CSteamGameServerAPIContext.GetSteamClient(), out phSteamPipe, eAccountType);
		}

		/// <summary>
		/// <para> removes an allocated user</para>
		/// <para> NOT THREADSAFE - ensure that no other threads are accessing Steamworks API when calling</para>
		/// </summary>
		public static void ReleaseUser(HSteamPipe hSteamPipe, HSteamUser hUser) {
			InteropHelp.TestIfAvailableGameServer();
			NativeMethods.ISteamClient_ReleaseUser(CSteamGameServerAPIContext.GetSteamClient(), hSteamPipe, hUser);
		}

		/// <summary>
		/// <para> retrieves the ISteamUser interface associated with the handle</para>
		/// </summary>
		public static IntPtr GetISteamUser(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamUser(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> retrieves the ISteamGameServer interface associated with the handle</para>
		/// </summary>
		public static IntPtr GetISteamGameServer(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamGameServer(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> set the local IP and Port to bind to</para>
		/// <para> this must be set before CreateLocalUser()</para>
		/// </summary>
		public static void SetLocalIPBinding(ref SteamIPAddress_t unIP, ushort usPort) {
			InteropHelp.TestIfAvailableGameServer();
			NativeMethods.ISteamClient_SetLocalIPBinding(CSteamGameServerAPIContext.GetSteamClient(), ref unIP, usPort);
		}

		/// <summary>
		/// <para> returns the ISteamFriends interface</para>
		/// </summary>
		public static IntPtr GetISteamFriends(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamFriends(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns the ISteamUtils interface</para>
		/// </summary>
		public static IntPtr GetISteamUtils(HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamUtils(CSteamGameServerAPIContext.GetSteamClient(), hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns the ISteamMatchmaking interface</para>
		/// </summary>
		public static IntPtr GetISteamMatchmaking(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamMatchmaking(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns the ISteamMatchmakingServers interface</para>
		/// </summary>
		public static IntPtr GetISteamMatchmakingServers(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamMatchmakingServers(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns the a generic interface</para>
		/// </summary>
		public static IntPtr GetISteamGenericInterface(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamGenericInterface(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns the ISteamUserStats interface</para>
		/// </summary>
		public static IntPtr GetISteamUserStats(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamUserStats(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns the ISteamGameServerStats interface</para>
		/// </summary>
		public static IntPtr GetISteamGameServerStats(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamGameServerStats(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns apps interface</para>
		/// </summary>
		public static IntPtr GetISteamApps(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamApps(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> networking</para>
		/// </summary>
		public static IntPtr GetISteamNetworking(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamNetworking(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> remote storage</para>
		/// </summary>
		public static IntPtr GetISteamRemoteStorage(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamRemoteStorage(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> user screenshots</para>
		/// </summary>
		public static IntPtr GetISteamScreenshots(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamScreenshots(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> game search</para>
		/// </summary>
		public static IntPtr GetISteamGameSearch(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamGameSearch(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns the number of IPC calls made since the last time this function was called</para>
		/// <para> Used for perf debugging so you can understand how many IPC calls your game makes per frame</para>
		/// <para> Every IPC call is at minimum a thread context switch if not a process one so you want to rate</para>
		/// <para> control how often you do them.</para>
		/// </summary>
		public static uint GetIPCCallCount() {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamClient_GetIPCCallCount(CSteamGameServerAPIContext.GetSteamClient());
		}

		/// <summary>
		/// <para> API warning handling</para>
		/// <para> 'int' is the severity; 0 for msg, 1 for warning</para>
		/// <para> 'const char *' is the text of the message</para>
		/// <para> callbacks will occur directly after the API function is called that generated the warning or message.</para>
		/// </summary>
		public static void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction) {
			InteropHelp.TestIfAvailableGameServer();
			NativeMethods.ISteamClient_SetWarningMessageHook(CSteamGameServerAPIContext.GetSteamClient(), pFunction);
		}

		/// <summary>
		/// <para> Trigger global shutdown for the DLL</para>
		/// </summary>
		public static bool BShutdownIfAllPipesClosed() {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamClient_BShutdownIfAllPipesClosed(CSteamGameServerAPIContext.GetSteamClient());
		}

		/// <summary>
		/// <para> Expose HTTP interface</para>
		/// </summary>
		public static IntPtr GetISteamHTTP(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamHTTP(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Exposes the ISteamController interface - deprecated in favor of Steam Input</para>
		/// </summary>
		public static IntPtr GetISteamController(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamController(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Exposes the ISteamUGC interface</para>
		/// </summary>
		public static IntPtr GetISteamUGC(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamUGC(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> returns app list interface, only available on specially registered apps</para>
		/// </summary>
		public static IntPtr GetISteamAppList(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamAppList(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Music Player</para>
		/// </summary>
		public static IntPtr GetISteamMusic(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamMusic(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Music Player Remote</para>
		/// </summary>
		public static IntPtr GetISteamMusicRemote(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamMusicRemote(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> html page display</para>
		/// </summary>
		public static IntPtr GetISteamHTMLSurface(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamHTMLSurface(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> inventory</para>
		/// </summary>
		public static IntPtr GetISteamInventory(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamInventory(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Video</para>
		/// </summary>
		public static IntPtr GetISteamVideo(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamVideo(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Parental controls</para>
		/// </summary>
		public static IntPtr GetISteamParentalSettings(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamParentalSettings(CSteamGameServerAPIContext.GetSteamClient(), hSteamuser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Exposes the Steam Input interface for controller support</para>
		/// </summary>
		public static IntPtr GetISteamInput(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamInput(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Steam Parties interface</para>
		/// </summary>
		public static IntPtr GetISteamParties(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamParties(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}

		/// <summary>
		/// <para> Steam Remote Play interface</para>
		/// </summary>
		public static IntPtr GetISteamRemotePlay(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
			InteropHelp.TestIfAvailableGameServer();
			using (var pchVersion2 = new InteropHelp.UTF8StringHandle(pchVersion)) {
				return NativeMethods.ISteamClient_GetISteamRemotePlay(CSteamGameServerAPIContext.GetSteamClient(), hSteamUser, hSteamPipe, pchVersion2);
			}
		}
	}
}

#endif // STEAM
