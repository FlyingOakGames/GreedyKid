// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if STEAM
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks
{
	/// Used to return English-language diagnostic error messages to caller.
	/// (For debugging or spewing to a console, etc.  Not intended for UI.)
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct SteamNetworkingErrMsg
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.k_cchMaxSteamNetworkingErrMsg)]
		public byte[] m_SteamNetworkingErrMsg;
	}
}

#endif // STEAM