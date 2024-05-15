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
	public struct InputDigitalActionHandle_t : System.IEquatable<InputDigitalActionHandle_t>, System.IComparable<InputDigitalActionHandle_t> {
		public ulong m_InputDigitalActionHandle;

		public InputDigitalActionHandle_t(ulong value) {
			m_InputDigitalActionHandle = value;
		}

		public override string ToString() {
			return m_InputDigitalActionHandle.ToString();
		}

		public override bool Equals(object other) {
			return other is InputDigitalActionHandle_t && this == (InputDigitalActionHandle_t)other;
		}

		public override int GetHashCode() {
			return m_InputDigitalActionHandle.GetHashCode();
		}

		public static bool operator ==(InputDigitalActionHandle_t x, InputDigitalActionHandle_t y) {
			return x.m_InputDigitalActionHandle == y.m_InputDigitalActionHandle;
		}

		public static bool operator !=(InputDigitalActionHandle_t x, InputDigitalActionHandle_t y) {
			return !(x == y);
		}

		public static explicit operator InputDigitalActionHandle_t(ulong value) {
			return new InputDigitalActionHandle_t(value);
		}

		public static explicit operator ulong(InputDigitalActionHandle_t that) {
			return that.m_InputDigitalActionHandle;
		}

		public bool Equals(InputDigitalActionHandle_t other) {
			return m_InputDigitalActionHandle == other.m_InputDigitalActionHandle;
		}

		public int CompareTo(InputDigitalActionHandle_t other) {
			return m_InputDigitalActionHandle.CompareTo(other.m_InputDigitalActionHandle);
		}
	}
}

#endif // STEAM
