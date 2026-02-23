using System;
using System.Runtime.InteropServices;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using NuklearDotNet;
using System.Diagnostics;
using ExampleShared;

namespace Example_SFML
{
	static partial class OpenGL
	{
		const string LibName = "opengl32";
		public const int GL_SCISSOR_TEST = 0xC11;

		static OpenGL()
		{
			NativeLibrary.SetDllImportResolver(typeof(OpenGL).Assembly, (name, assembly, path) =>
			{
				if (name != LibName)
					return IntPtr.Zero;

				if (OperatingSystem.IsWindows())
					return IntPtr.Zero; // default resolution finds opengl32.dll

				if (OperatingSystem.IsLinux())
					return NativeLibrary.Load("libGL.so.1");

				if (OperatingSystem.IsMacOS())
					return NativeLibrary.Load("/System/Library/Frameworks/OpenGL.framework/OpenGL");

				return IntPtr.Zero;
			});
		}

		[LibraryImport(LibName)]
		public static partial void glEnable(int Cap);

		[LibraryImport(LibName)]
		public static partial void glDisable(int Cap);

		[LibraryImport(LibName)]
		public static partial void glScissor(int X, int Y, int W, int H);

		public static void glScissor2(int WindH, int X, int Y, int W, int H)
		{
			glScissor(X, WindH - Y - H, W, H);
		}
	}

	unsafe class SFMLDevice : NuklearDeviceTex<Texture>, IFrameBuffered
	{
		RenderWindow RWind;
		RenderTexture RT;
		Sprite RenderSprite;

		public SFMLDevice(RenderWindow RWind)
		{
			this.RWind = RWind;
			RT = new RenderTexture(RWind.Size.X, RWind.Size.Y);
			RenderSprite = new Sprite(RT.Texture);
		}

		public override Texture CreateTexture(int W, int H, IntPtr Data)
		{
			byte[] pixels = new byte[W * H * 4];
			new ReadOnlySpan<byte>((void*)Data, pixels.Length).CopyTo(pixels);

			Image I = new Image((uint)W, (uint)H, pixels);
			Texture T = new Texture(I);
			T.Smooth = true;
			return T;
		}

		public void BeginBuffering()
		{
			RT.Clear(Color.Transparent);
		}

		NkVertex* VertsPtr;
		ushort* IndsPtr;

		public override void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer)
		{
			fixed (NkVertex* v = VertexBuffer) VertsPtr = v;
			fixed (ushort* i = IndexBuffer) IndsPtr = i;
		}

		public override void Render(NkHandle Userdata, Texture Texture, NkRect ClipRect, uint Offset, uint Count)
		{
			Vertex[] SfmlVerts = new Vertex[Count];

			for (int i = 0; i < Count; i++)
			{
				NkVertex V = VertsPtr[IndsPtr[Offset + i]];
				SfmlVerts[i] = new Vertex(
					new Vector2f(V.Position.X, V.Position.Y),
					new Color(V.Color.R, V.Color.G, V.Color.B, V.Color.A),
					new Vector2f(V.UV.X * Texture.Size.X, V.UV.Y * Texture.Size.Y));
			}

			OpenGL.glEnable(OpenGL.GL_SCISSOR_TEST);
			OpenGL.glScissor2((int)RT.Size.Y, (int)ClipRect.X, (int)ClipRect.Y, (int)ClipRect.W, (int)ClipRect.H);

			RT.Draw(SfmlVerts, PrimitiveType.Triangles, new RenderStates(Texture));

			OpenGL.glDisable(OpenGL.GL_SCISSOR_TEST);
		}

		public void EndBuffering()
		{
			RT.Display();
		}

		public void RenderFinal()
		{
			RWind.Draw(RenderSprite);
		}
	}

	class Program
	{
		static void OnKey(SFMLDevice Dev, KeyEventArgs E, bool Down)
		{
			if (E.Code == Keyboard.Key.LShift || E.Code == Keyboard.Key.RShift)
				Dev.OnKey(NkKeys.Shift, Down);
			else if (E.Code == Keyboard.Key.LControl || E.Code == Keyboard.Key.RControl)
				Dev.OnKey(NkKeys.Ctrl, Down);
			else if (E.Code == Keyboard.Key.Delete)
				Dev.OnKey(NkKeys.Del, Down);
			else if (E.Code == Keyboard.Key.Enter)
				Dev.OnKey(NkKeys.Enter, Down);
			else if (E.Code == Keyboard.Key.Tab)
				Dev.OnKey(NkKeys.Tab, Down);
			else if (E.Code == Keyboard.Key.Backspace)
				Dev.OnKey(NkKeys.Backspace, Down);
			else if (E.Code == Keyboard.Key.Up)
				Dev.OnKey(NkKeys.Up, Down);
			else if (E.Code == Keyboard.Key.Down)
				Dev.OnKey(NkKeys.Down, Down);
			else if (E.Code == Keyboard.Key.Left)
				Dev.OnKey(NkKeys.Left, Down);
			else if (E.Code == Keyboard.Key.Right)
				Dev.OnKey(NkKeys.Right, Down);
			else if (E.Code == Keyboard.Key.Home)
				Dev.OnKey(NkKeys.ScrollStart, Down);
			else if (E.Code == Keyboard.Key.End)
				Dev.OnKey(NkKeys.ScrollEnd, Down);
			else if (E.Code == Keyboard.Key.PageDown)
				Dev.OnKey(NkKeys.ScrollDown, Down);
			else if (E.Code == Keyboard.Key.PageUp)
				Dev.OnKey(NkKeys.ScrollUp, Down);
		}

		[DllImport("shcore.dll", SetLastError = true)]
		static extern int SetProcessDpiAwareness(int value);

		static void Main(string[] args)
		{
			// DPI-unaware so Windows handles scaling transparently
			if (OperatingSystem.IsWindows())
				try { SetProcessDpiAwareness(0); } catch { }

			Console.Title = "Nuklear SFML .NET";

			Stopwatch SWatch = Stopwatch.StartNew();
			Color ClearColor = new Color(50, 50, 50);
			VideoMode VMode = new VideoMode(1366, 768);

			RenderWindow RWind = new RenderWindow(VMode, "Nuklear SFML .NET", Styles.Close);
			RWind.SetKeyRepeatEnabled(true);

			SFMLDevice Dev = new SFMLDevice(RWind);
			RWind.Closed += (S, E) => RWind.Close();
			RWind.MouseButtonPressed += (S, E) => Dev.OnMouseButton((NuklearEvent.MouseButton)E.Button, E.X, E.Y, true);
			RWind.MouseButtonReleased += (S, E) => Dev.OnMouseButton((NuklearEvent.MouseButton)E.Button, E.X, E.Y, false);
			RWind.MouseMoved += (S, E) => Dev.OnMouseMove(E.X, E.Y);
			RWind.MouseWheelScrolled += (S, E) => Dev.OnScroll(0, E.Delta);
			RWind.KeyPressed += (S, E) => OnKey(Dev, E, true);
			RWind.KeyReleased += (S, E) => OnKey(Dev, E, false);
			RWind.TextEntered += (S, E) => Dev.OnText(E.Unicode);

			Shared.Init(Dev);

			float Dt = 0.1f;

			NuklearAPI.QueueForceUpdate();
			while (RWind.IsOpen)
			{
				RWind.DispatchEvents();
				RWind.Clear(ClearColor);

				Shared.DrawLoop(Dt);

				RWind.Display();

				Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();
			}

			Environment.Exit(0);
		}
	}
}
