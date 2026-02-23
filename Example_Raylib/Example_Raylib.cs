using System;
using System.Diagnostics;
using System.Numerics;
using NuklearDotNet;
using Raylib_cs;

namespace Example_Raylib
{
	unsafe sealed class RaylibTexture
	{
		public Texture2D Texture;
		public Image Image;

		public RaylibTexture(int W, int H, IntPtr Data)
		{
			Image = new Image
			{
				Width = W,
				Height = H,
				Mipmaps = 1,
				Format = PixelFormat.UncompressedR8G8B8A8,
				Data = (void*)Data
			};

			Texture = Raylib.LoadTextureFromImage(Image);
			Raylib.SetTextureFilter(Texture, TextureFilter.Bilinear);
			Raylib.SetTextureWrap(Texture, TextureWrap.Clamp);
		}
	}

	unsafe sealed class RaylibDevice : NuklearDeviceTex<RaylibTexture>
	{
		float DpiScale = 1.0f;

		public RaylibDevice()
		{
			Vector2 dpi = Raylib.GetWindowScaleDPI();
			DpiScale = dpi.X;
		}

		public float GetDpiScale() => DpiScale;

		public override RaylibTexture CreateTexture(int W, int H, IntPtr Data)
		{
			return new RaylibTexture(W, H, Data);
		}

		NkVertex* VertsPtr;
		int VertsCount;
		ushort* IndsPtr;
		int IndsCount;

		public override void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer)
		{
			fixed (NkVertex* v = VertexBuffer) VertsPtr = v;
			fixed (ushort* i = IndexBuffer) IndsPtr = i;
			VertsCount = VertexBuffer.Length;
			IndsCount = IndexBuffer.Length;
		}

		static void DrawVert(NkVertex V)
		{
			Rlgl.Color4ub(V.Color.R, V.Color.G, V.Color.B, V.Color.A);
			Rlgl.TexCoord2f(V.UV.X, V.UV.Y);
			Rlgl.Vertex2f(V.Position.X, V.Position.Y);
		}

		public override void Render(NkHandle Userdata, RaylibTexture Texture, NkRect ClipRect, uint Offset, uint Count)
		{
			Rlgl.DisableBackfaceCulling();
			Raylib.BeginScissorMode((int)ClipRect.X, (int)ClipRect.Y, (int)ClipRect.W, (int)ClipRect.H);
			{
				Rlgl.SetTexture(Texture.Texture.Id);
				Rlgl.CheckRenderBatchLimit((int)Count / 3 * 4);

				Rlgl.Begin(DrawMode.Quads);
				for (int i = 0; i < Count; i += 3)
				{
					DrawVert(VertsPtr[IndsPtr[Offset + i]]);
					DrawVert(VertsPtr[IndsPtr[Offset + i + 1]]);
					DrawVert(VertsPtr[IndsPtr[Offset + i + 2]]);
					DrawVert(VertsPtr[IndsPtr[Offset + i + 2]]);
				}
				Rlgl.End();

				Rlgl.SetTexture(0);
			}
			Raylib.EndScissorMode();
			Rlgl.EnableBackfaceCulling();
		}
	}


	sealed class Program
	{
		static void Main(string[] args)
		{
			Stopwatch SWatch = Stopwatch.StartNew();

			Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.HighDpiWindow);
			Raylib.InitWindow(1280, 800, "NuklearDotNet - Raylib Example");
			Raylib.SetTargetFPS(60);

			RaylibDevice Dev = new RaylibDevice();
			NuklearAPI.DefaultFontHeight = 13.0f * Dev.GetDpiScale();
			ExampleShared.Shared.Init(Dev);

			float Dt = 0.1f;

			int LastMouseX = 0;
			int LastMouseY = 0;

			NuklearAPI.QueueForceUpdate();

			while (!Raylib.WindowShouldClose())
			{
				Vector2 MousePos = Raylib.GetMousePosition();
				if (LastMouseX != (int)MousePos.X || LastMouseY != (int)MousePos.Y)
				{
					LastMouseX = (int)MousePos.X;
					LastMouseY = (int)MousePos.Y;
					Dev.OnMouseMove(LastMouseX, LastMouseY);
				}

				if (Raylib.IsMouseButtonPressed(MouseButton.Left))
					Dev.OnMouseButton(NuklearEvent.MouseButton.Left, LastMouseX, LastMouseY, true);
				if (Raylib.IsMouseButtonReleased(MouseButton.Left))
					Dev.OnMouseButton(NuklearEvent.MouseButton.Left, LastMouseX, LastMouseY, false);

				if (Raylib.IsMouseButtonPressed(MouseButton.Right))
					Dev.OnMouseButton(NuklearEvent.MouseButton.Right, LastMouseX, LastMouseY, true);
				if (Raylib.IsMouseButtonReleased(MouseButton.Right))
					Dev.OnMouseButton(NuklearEvent.MouseButton.Right, LastMouseX, LastMouseY, false);

				if (Raylib.IsMouseButtonPressed(MouseButton.Middle))
					Dev.OnMouseButton(NuklearEvent.MouseButton.Middle, LastMouseX, LastMouseY, true);
				if (Raylib.IsMouseButtonReleased(MouseButton.Middle))
					Dev.OnMouseButton(NuklearEvent.MouseButton.Middle, LastMouseX, LastMouseY, false);

				Vector2 wheel = Raylib.GetMouseWheelMoveV();
				if (wheel.X != 0 || wheel.Y != 0)
					Dev.OnScroll(wheel.X, wheel.Y);

				int charPressed;
				while ((charPressed = Raylib.GetCharPressed()) != 0)
					Dev.OnText(((char)charPressed).ToString());

				int keyPressed;
				while ((keyPressed = Raylib.GetKeyPressed()) != 0) { }

				Dev.OnKey(NkKeys.Shift, Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift));
				Dev.OnKey(NkKeys.Del, Raylib.IsKeyPressed(KeyboardKey.Delete) || Raylib.IsKeyPressedRepeat(KeyboardKey.Delete));
				Dev.OnKey(NkKeys.Enter, Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressedRepeat(KeyboardKey.Enter));
				Dev.OnKey(NkKeys.Tab, Raylib.IsKeyPressed(KeyboardKey.Tab) || Raylib.IsKeyPressedRepeat(KeyboardKey.Tab));
				Dev.OnKey(NkKeys.Backspace, Raylib.IsKeyPressed(KeyboardKey.Backspace) || Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace));
				Dev.OnKey(NkKeys.Up, Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressedRepeat(KeyboardKey.Up));
				Dev.OnKey(NkKeys.Down, Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressedRepeat(KeyboardKey.Down));
				Dev.OnKey(NkKeys.Left, Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressedRepeat(KeyboardKey.Left));
				Dev.OnKey(NkKeys.Right, Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressedRepeat(KeyboardKey.Right));

				bool ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
				Dev.OnKey(NkKeys.Copy, ctrl && Raylib.IsKeyPressed(KeyboardKey.C));
				Dev.OnKey(NkKeys.Paste, ctrl && Raylib.IsKeyPressed(KeyboardKey.V));
				Dev.OnKey(NkKeys.Cut, ctrl && Raylib.IsKeyPressed(KeyboardKey.X));
				Dev.OnKey(NkKeys.TextSelectAll, ctrl && Raylib.IsKeyPressed(KeyboardKey.A));
				Dev.OnKey(NkKeys.TextWordLeft, ctrl && Raylib.IsKeyPressed(KeyboardKey.Left));
				Dev.OnKey(NkKeys.TextWordRight, ctrl && Raylib.IsKeyPressed(KeyboardKey.Right));
				Dev.OnKey(NkKeys.LineStart, Raylib.IsKeyPressed(KeyboardKey.Home));
				Dev.OnKey(NkKeys.LineEnd, Raylib.IsKeyPressed(KeyboardKey.End));

				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.Black);
				ExampleShared.Shared.DrawLoop(Dt);
				Raylib.EndDrawing();

				Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();
			}

			Environment.Exit(0);
		}
	}
}
