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
	public struct InputAnalogActionHandle_t : System.IEquatable<InputAnalogActionHandle_t>, System.IComparable<InputAnalogActionHandle_t> {
		public ulong m_InputAnalogActionHandle;

		public InputAnalogActionHandle_t(ulong value) {
			m_InputAnalogActionHandle = value;
		}

		public override string ToString() {
			return m_InputAnalogActionHandle.ToString();
		}

		public override bool Equals(object other) {
			return other is InputAnalogActionHandle_t && this == (InputAnalogActionHandle_t)other;
		}

		public override int GetHashCode() {
			return m_InputAnalogActionHandle.GetHashCode();
		}

		public static bool operator ==(InputAnalogActionHandle_t x, InputAnalogActionHandle_t y) {
			return x.m_InputAnalogActionHandle == y.m_InputAnalogActionHandle;
		}

		public static bool operator !=(InputAnalogActionHandle_t x, InputAnalogActionHandle_t y) {
			return !(x == y);
		}

		public static explicit operator InputAnalogActionHandle_t(ulong value) {
			return new InputAnalogActionHandle_t(value);
		}

		public static explicit operator ulong(InputAnalogActionHandle_t that) {
			return that.m_InputAnalogActionHandle;
		}

		public bool Equals(InputAnalogActionHandle_t other) {
			return m_InputAnalogActionHandle == other.m_InputAnalogActionHandle;
		}

		public int CompareTo(InputAnalogActionHandle_t other) {
			return m_InputAnalogActionHandle.CompareTo(other.m_InputAnalogActionHandle);
		}
	}
}

#endif // STEAM