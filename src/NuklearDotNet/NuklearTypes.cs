using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NuklearDotNet;

public unsafe delegate void FontStashAction(nk_font_atlas* Atlas);

public unsafe interface INuklearDeviceInit {
	void Init();
}

public unsafe interface INuklearDeviceFontStash {
	void FontStash(nk_font_atlas* Atlas);
}

public interface INuklearDeviceRenderHooks {
	void BeginRender();
	void EndRender();
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct NkVector2f {
	public readonly float X, Y;

	public NkVector2f(float X, float Y) {
		this.X = X;
		this.Y = Y;
	}

	public override string ToString() {
		return $"({X}, {Y})";
	}

	public static implicit operator Vector2(NkVector2f V) {
		return new Vector2(V.X, V.Y);
	}

	public static implicit operator NkVector2f(Vector2 V) {
		return new NkVector2f(V.X, V.Y);
	}
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct NkVertex {
	public readonly NkVector2f Position;
	public readonly NkVector2f UV;
	public readonly NkColor Color;

	public override string ToString() {
		return $"Position: {Position}; UV: {UV}; Color: {Color}";
	}
}

public readonly struct NuklearEvent {
	public enum EventType {
		MouseButton,
		MouseMove,
		Scroll,
		Text,
		KeyboardKey,
		ForceUpdate
	}

	public enum MouseButton {
		Left, Middle, Right
	}

	public EventType EvtType { get; init; }
	public MouseButton MButton { get; init; }
	public NkKeys Key { get; init; }
	public int X { get; init; }
	public int Y { get; init; }
	public bool Down { get; init; }
	public float ScrollX { get; init; }
	public float ScrollY { get; init; }
	public string Text { get; init; }
}

public interface IFrameBuffered {
	void BeginBuffering();
	void EndBuffering();
	void RenderFinal();
}

public unsafe abstract class NuklearDevice {
	private readonly Queue<NuklearEvent> _events = new();

	/// <summary>Font size in pixels for custom font loading via <see cref="INuklearDeviceFontStash"/>.</summary>
	public float FontSize { get; set; }

	/// <summary>Custom font pointer set during font stash, used to override the default font after baking.</summary>
	public unsafe nk_font* CustomFont { get; set; }

	internal bool HasPendingEvents => _events.Count > 0;
	internal NuklearEvent DequeueEvent() => _events.Dequeue();

	public abstract void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer);
	public abstract void Render(NkHandle Userdata, int Texture, NkRect ClipRect, uint Offset, uint Count);
	public abstract int CreateTextureHandle(int W, int H, IntPtr Data);

	protected NuklearDevice() {
		ForceUpdate();
	}

	public void OnMouseButton(NuklearEvent.MouseButton MouseButton, int X, int Y, bool Down) {
		_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.MouseButton, MButton = MouseButton, X = X, Y = Y, Down = Down });
	}

	public void OnMouseMove(int X, int Y) {
		_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.MouseMove, X = X, Y = Y });
	}

	public void OnScroll(float ScrollX, float ScrollY) {
		_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.Scroll, ScrollX = ScrollX, ScrollY = ScrollY });
	}

	public void OnText(string Txt) {
		_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.Text, Text = Txt });
	}

	public void OnKey(NkKeys Key, bool Down) {
		_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.KeyboardKey, Key = Key, Down = Down });
	}

	public void ForceUpdate() {
		_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.ForceUpdate });
	}
}

public unsafe abstract class NuklearDeviceTex<T> : NuklearDevice where T : notnull {
	List<T?> Textures;

	protected NuklearDeviceTex() {
		Textures = new List<T?>();
		Textures.Add(default);
	}

	public int CreateTextureHandle(T Tex) {
		Textures.Add(Tex);
		return Textures.Count - 1;
	}

	public T GetTexture(int Handle) {
		return Textures[Handle] ?? throw new InvalidOperationException($"Texture handle {Handle} is null");
	}

	public sealed override int CreateTextureHandle(int W, int H, IntPtr Data) {
		T Tex = CreateTexture(W, H, Data);
		return CreateTextureHandle(Tex);
	}

	public sealed override void Render(NkHandle Userdata, int Texture, NkRect ClipRect, uint Offset, uint Count) {
		Render(Userdata, GetTexture(Texture), ClipRect, Offset, Count);
	}

	public abstract T CreateTexture(int W, int H, IntPtr Data);

	public abstract void Render(NkHandle Userdata, T Texture, NkRect ClipRect, uint Offset, uint Count);
}
