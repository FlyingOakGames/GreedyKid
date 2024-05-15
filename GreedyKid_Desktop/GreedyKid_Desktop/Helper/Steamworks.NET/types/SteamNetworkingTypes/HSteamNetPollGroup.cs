// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if STEAM

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks {
	[System.Serializable]
	public struct HSteamNetPollGroup : System.IEquatable<HSteamNetPollGroup>, System.IComparable<HSteamNetPollGroup> {
		public static readonly HSteamNetPollGroup Invalid = new HSteamNetPollGroup(0);
		public uint m_HSteamNetPollGroup;

		public HSteamNetPollGroup(uint value) {
			m_HSteamNetPollGroup = value;
		}

		public override string ToString() {
			return m_HSteamNetPollGroup.ToString();
		}

		public override bool Equals(object other) {
			return other is HSteamNetPollGroup && this == (HSteamNetPollGroup)other;
		}

		public override int GetHashCode() {
			return m_HSteamNetPollGroup.GetHashCode();
		}

		public static bool operator ==(HSteamNetPollGroup x, HSteamNetPollGroup y) {
			return x.m_HSteamNetPollGroup == y.m_HSteamNetPollGroup;
		}

		public static bool operator !=(HSteamNetPollGroup x, HSteamNetPollGroup y) {
			return !(x == y);
		}

		public static explicit operator HSteamNetPollGroup(uint value) {
			return new HSteamNetPollGroup(value);
		}

		public static explicit operator uint(HSteamNetPollGroup that) {
			return that.m_HSteamNetPollGroup;
		}

		public bool Equals(HSteamNetPollGroup other) {
			return m_HSteamNetPollGroup == other.m_HSteamNetPollGroup;
		}

		public int CompareTo(HSteamNetPollGroup other) {
			return m_HSteamNetPollGroup.CompareTo(other.m_HSteamNetPollGroup);
		}
	}
}

#endif // STEAM
