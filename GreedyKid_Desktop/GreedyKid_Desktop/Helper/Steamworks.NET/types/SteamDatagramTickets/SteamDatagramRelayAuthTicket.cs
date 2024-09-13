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
	/// Network-routable identifier for a service.  This is an intentionally
	/// opaque byte blob.  The relays know how to use this to forward it on
	/// to the intended destination, but otherwise clients really should not
	/// need to know what's inside.  (Indeed, we don't really want them to
	/// know, as it could reveal information useful to an attacker.)
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = Packsize.value)]
	public struct SteamDatagramRelayAuthTicket
	{
		/// Identity of the gameserver we want to talk to.  This is required.
		SteamNetworkingIdentity m_identityGameserver;

		/// Identity of the person who was authorized.  This is required.
		SteamNetworkingIdentity m_identityAuthorizedClient;

		/// SteamID is authorized to send from a particular public IP.  If this
		/// is 0, then the sender is not restricted to a particular IP.
		///
		/// Recommend to leave this set to zero.
		uint m_unPublicIP;

		/// Time when the ticket expires.  Recommended: take the current
		/// time and add 6 hours, or maybe a bit longer if your gameplay
		/// sessions are longer.
		///
		/// NOTE: relays may reject tickets with expiry times excessively
		/// far in the future, so contact us if you wish to use an expiry
		/// longer than, say, 24 hours.
		RTime32 m_rtimeTicketExpiry;

		/// Routing information where the gameserver is listening for
		/// relayed traffic.  You should fill this in when generating
		/// a ticket.
		///
		/// When generating tickets on your backend:
		/// - In production: The gameserver knows the proper routing
		///   information, so you need to call
		///   ISteamNetworkingSockets::GetHostedDedicatedServerAddress
		///   and send the info to your backend.
		/// - In development, you will need to provide public IP
		///   of the server using SteamDatagramServiceNetID::SetDevAddress.
		///   Relays need to be able to send UDP
		///   packets to this server.  Since it's very likely that
		///   your server is behind a firewall/NAT, make sure that
		///   the address is the one that the outside world can use.
		///   The traffic from the relays will be "unsolicited", so
		///   stateful firewalls won't work -- you will probably have
		///   to set up an explicit port forward.
		/// On the client:
		/// - this field will always be blank.
		SteamDatagramHostedAddress m_routing;

		/// App ID this is for.  This is required, and should be the
		/// App ID the client is running.  (Even if your gameserver
		/// uses a different App ID.)
		uint m_nAppID;

		/// Restrict this ticket to be used for a particular virtual port?
		/// Set to -1 to allow any virtual port.
		///
		/// This is useful as a security measure, and also so the client will
		/// use the right ticket (which might have extra fields that are useful
		/// for proper analytics), if the client happens to have more than one
		/// appropriate ticket.
		///
		/// Note: if a client has more that one acceptable ticket, they will
		/// always use the one expiring the latest.
		int m_nRestrictToVirtualPort;

		//
		// Extra fields.
		//
		// These are collected for backend analytics.  For example, you might
		// send a MatchID so that all of the records for a particular match can
		// be located.  Or send a game mode field so that you can compare
		// the network characteristics of different game modes.
		//
		// (At the time of this writing we don't have a way to expose the data
		// we collect to partners, but we hope to in the future so that you can
		// get visibility into network conditions.)
		//
		[StructLayout(LayoutKind.Sequential, Pack = Packsize.value)]
		struct ExtraField
		{
			enum EType
			{
				k_EType_String,
				k_EType_Int, // For most small integral values.  Uses google protobuf sint64, so it's small on the wire.  WARNING: In some places this value may be transmitted in JSON, in which case precision may be lost in backend analytics.  Don't use this for an "identifier", use it for a scalar quantity.
				k_EType_Fixed64, // 64 arbitrary bits.  This value is treated as an "identifier".  In places where JSON format is used, it will be serialized as a string.  No aggregation / analytics can be performed on this value.
			};
			EType m_eType;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
			byte[] m_szName;

			[StructLayout(LayoutKind.Explicit)]
			struct OptionValue
			{
				[FieldOffset(0)]
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
				byte[] m_szStringValue;

				[FieldOffset(0)]
				long m_nIntValue;

				[FieldOffset(0)]
				ulong m_nFixed64Value;
			}
			OptionValue m_val;
		};

		const int k_nMaxExtraFields = 16;

		int m_nExtraFields;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = k_nMaxExtraFields)]
		ExtraField[] m_vecExtraFields;

		// Reset all fields
		public void Clear()
		{
		}
	}
}

#endif // STEAM