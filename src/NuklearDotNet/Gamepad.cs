using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuklearDotNet {

	/// <summary>
	/// Gamepad button identifiers matching <c>enum nk_gamepad_button</c> in nuklear_gamepad.h.
	/// <para>Values 0-12 map to d-pad directions, face buttons, shoulder buttons, and system buttons.</para>
	/// </summary>
	/// <example>
	/// <code>
	/// if (console.IsButtonPressed(0, NkGamepadButton.A))
	///     ActivateSelectedWidget();
	/// </code>
	/// </example>
	public enum NkGamepadButton {
		Invalid = -1,
		Up = 0,
		Down,
		Left,
		Right,
		A,
		B,
		X,
		Y,
		LB,
		RB,
		Back,
		Start,
		Guide,
		L3,
		R3,
		Misc1,
		RightPaddle1,
		LeftPaddle1,
		RightPaddle2,
		LeftPaddle2,
		Touchpad,
		Misc2,
		Misc3,
		Misc4,
		Misc5,
		Misc6,
		Last
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct NkGamepadInputSource {
		public nint UserData;
		public nint Init;
		public nint Update;
		public nint Free;
		public nint Name;
		public nint InputSourceName;
		public int Id;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct NkGamepad {
		public int Available;
		public uint Buttons;
		public uint ButtonsPrev;
		public fixed byte Name[16];
		public nint Data;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct NkGamepads {
		public NkGamepad Pad0;
		public NkGamepad Pad1;
		public NkGamepad Pad2;
		public NkGamepad Pad3;
		public nk_context* Ctx;
		public NkGamepadInputSource InputSource;
	}

	public static unsafe partial class Nuklear {

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_debug_sizeof_gamepads();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_debug_sizeof_gamepad();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_debug_sizeof_gamepad_input_source();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_sdl3_gamepad_init();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_sdl3_gamepad_shutdown();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_sdl3_gamepad_pump_events(NkGamepads* gamepads);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_init(NkGamepads* gamepads, nk_context* ctx, void* user_data);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_init_with_source(NkGamepads* gamepads, nk_context* ctx, NkGamepadInputSource source);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_gamepad_free(NkGamepads* gamepads);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_gamepad_update(NkGamepads* gamepads);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_is_available(NkGamepads* gamepads, int num);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_gamepad_set_available(NkGamepads* gamepads, int num, int available);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_is_button_down(NkGamepads* gamepads, int num, NkGamepadButton button);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_is_button_pressed(NkGamepads* gamepads, int num, NkGamepadButton button);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_is_button_released(NkGamepads* gamepads, int num, NkGamepadButton button);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_any_button_pressed(NkGamepads* gamepads, int num);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_gamepad_button(NkGamepads* gamepads, int num, NkGamepadButton button, int down);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_gamepad_count(NkGamepads* gamepads);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial byte* nk_gamepad_name(NkGamepads* gamepads, int num);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial NkGamepadInputSource nk_gamepad_input_source(NkGamepads* gamepads);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_gamepad_set_input_source(NkGamepads* gamepads, NkGamepadInputSource source);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial NkGamepadInputSource nk_gamepad_sdl3_input_source(void* user_data);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial NkGamepadInputSource nk_gamepad_keyboard_input_source(void* user_data);
	}
}
