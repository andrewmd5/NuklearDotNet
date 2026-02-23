using System;
using System.Drawing;
using System.Windows.Forms;
using NuklearDotNet;
using ExampleShared;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Example_WindowsForms {
    unsafe class FormDevice : NuklearDeviceTex<Bitmap> {
        Graphics Gfx;
        NkVertex[] Verts;
        ushort[] Inds;

        public FormDevice(Graphics Gfx) {
            this.Gfx = Gfx;
        }

        public override Bitmap CreateTexture(int W, int H, IntPtr Data) {
            Bitmap Bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            BitmapData Dta = Bmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            // Nuklear outputs RGBA, GDI+ Format32bppArgb is BGRA in memory — swap R and B
            byte* src = (byte*)Data;
            byte* dst = (byte*)Dta.Scan0;
            for (int i = 0; i < W * H; i++) {
                dst[0] = src[2]; // B
                dst[1] = src[1]; // G
                dst[2] = src[0]; // R
                dst[3] = src[3]; // A
                src += 4;
                dst += 4;
            }

            Bmp.UnlockBits(Dta);
            return Bmp;
        }

        void FillColoredTriangle(NkVertex v0, NkVertex v1, NkVertex v2) {
            PointF[] pts = [
                new(v0.Position.X, v0.Position.Y),
                new(v1.Position.X, v1.Position.Y),
                new(v2.Position.X, v2.Position.Y),
            ];

            bool sameColor = v0.Color.R == v1.Color.R && v0.Color.G == v1.Color.G
                && v0.Color.B == v1.Color.B && v0.Color.A == v1.Color.A
                && v0.Color.R == v2.Color.R && v0.Color.G == v2.Color.G
                && v0.Color.B == v2.Color.B && v0.Color.A == v2.Color.A;

            if (sameColor) {
                using var brush = new SolidBrush(Color.FromArgb(v0.Color.A, v0.Color.R, v0.Color.G, v0.Color.B));
                Gfx.FillPolygon(brush, pts);
            } else {
                using var path = new GraphicsPath();
                path.AddPolygon(pts);
                using var brush = new PathGradientBrush(path);
                brush.CenterColor = Color.FromArgb(
                    (v0.Color.A + v1.Color.A + v2.Color.A) / 3,
                    (v0.Color.R + v1.Color.R + v2.Color.R) / 3,
                    (v0.Color.G + v1.Color.G + v2.Color.G) / 3,
                    (v0.Color.B + v1.Color.B + v2.Color.B) / 3);
                brush.SurroundColors = [
                    Color.FromArgb(v0.Color.A, v0.Color.R, v0.Color.G, v0.Color.B),
                    Color.FromArgb(v1.Color.A, v1.Color.R, v1.Color.G, v1.Color.B),
                    Color.FromArgb(v2.Color.A, v2.Color.R, v2.Color.G, v2.Color.B),
                ];
                Gfx.FillPath(brush, path);
            }
        }

        public override void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer) {
            Verts = VertexBuffer.ToArray();
            Inds = IndexBuffer.ToArray();
        }

        public override void Render(NkHandle Userdata, Bitmap Texture, NkRect ClipRect, uint Offset, uint Count) {
            Gfx.SetClip(new RectangleF(ClipRect.X, ClipRect.Y, ClipRect.W, ClipRect.H));
            Gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            Gfx.PixelOffsetMode = PixelOffsetMode.Half;

            for (uint i = 0; i < Count; ) {
                uint b = Offset + i;

                // Detect quads: Nuklear emits [i0, i1, i2, i0, i2, i3]
                bool isQuad = i + 6 <= Count
                    && Inds[b + 3] == Inds[b + 0]
                    && Inds[b + 4] == Inds[b + 2];

                if (isQuad) {
                    NkVertex v0 = Verts[Inds[b + 0]]; // top-left
                    NkVertex v1 = Verts[Inds[b + 1]]; // top-right
                    NkVertex v2 = Verts[Inds[b + 2]]; // bottom-right
                    NkVertex v3 = Verts[Inds[b + 5]]; // bottom-left

                    float srcW = (v2.UV.X - v0.UV.X) * Texture.Width;
                    float srcH = (v2.UV.Y - v0.UV.Y) * Texture.Height;

                    if (srcW >= 4f && srcH >= 4f) {
                        // Textured quad (text glyphs etc.) — DrawImage with color tint
                        float r = v0.Color.R / 255f, g = v0.Color.G / 255f;
                        float bl = v0.Color.B / 255f, a = v0.Color.A / 255f;
                        using var ia = new ImageAttributes();
                        ia.SetColorMatrix(new ColorMatrix([
                            [r,  0,  0,  0, 0],
                            [0,  g,  0,  0, 0],
                            [0,  0, bl,  0, 0],
                            [0,  0,  0,  a, 0],
                            [0,  0,  0,  0, 1],
                        ]));
                        Gfx.DrawImage(Texture,
                            new Rectangle((int)v0.Position.X, (int)v0.Position.Y,
                                (int)(v1.Position.X - v0.Position.X), (int)(v3.Position.Y - v0.Position.Y)),
                            v0.UV.X * Texture.Width, v0.UV.Y * Texture.Height, srcW, srcH,
                            GraphicsUnit.Pixel, ia);
                    } else {
                        // Solid or gradient quad — vertex color fill
                        FillColoredTriangle(v0, v1, v2);
                        FillColoredTriangle(v0, v2, v3);
                    }
                    i += 6;
                } else {
                    // Individual triangle (circles, arcs, knobs) — vertex color fill
                    FillColoredTriangle(Verts[Inds[b + 0]], Verts[Inds[b + 1]], Verts[Inds[b + 2]]);
                    i += 3;
                }
            }

            Gfx.ResetClip();
        }
    }

    public partial class NuklearForm : Form {
        public NuklearForm() {
            InitializeComponent();
        }

        Bitmap BBufferBitmap;
        Graphics BBuffer;
        Graphics FBuffer;
        FormDevice Dev;

        private void NuklearForm_Load(object sender, EventArgs e) {
            FBuffer = CreateGraphics();

            BBufferBitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            BBuffer = Graphics.FromImage(BBufferBitmap);
            Dev = new FormDevice(BBuffer);
            Shared.Init(Dev);

            MouseMove += (S, E) => Dev.OnMouseMove(E.X, E.Y);
            MouseDown += (S, E) => Dev.OnMouseButton(NuklearEvent.MouseButton.Left, E.X, E.Y, true);
            MouseUp += (S, E) => Dev.OnMouseButton(NuklearEvent.MouseButton.Left, E.X, E.Y, false);
            MouseWheel += (S, E) => Dev.OnScroll(0, E.Delta / 120f);
            KeyDown += (S, E) => OnKey(Dev, E, true);
            KeyUp += (S, E) => OnKey(Dev, E, false);
            KeyPress += (S, E) => Dev.OnText(E.KeyChar.ToString());

            Thread RenderThread = new Thread(Render);
            RenderThread.IsBackground = true;
            RenderThread.Start();
        }

        static void OnKey(FormDevice Dev, KeyEventArgs E, bool Down) {
            switch (E.KeyCode) {
                case Keys.ShiftKey: Dev.OnKey(NkKeys.Shift, Down); break;
                case Keys.ControlKey: Dev.OnKey(NkKeys.Ctrl, Down); break;
                case Keys.Delete: Dev.OnKey(NkKeys.Del, Down); break;
                case Keys.Enter: Dev.OnKey(NkKeys.Enter, Down); break;
                case Keys.Tab: Dev.OnKey(NkKeys.Tab, Down); break;
                case Keys.Back: Dev.OnKey(NkKeys.Backspace, Down); break;
                case Keys.Up: Dev.OnKey(NkKeys.Up, Down); break;
                case Keys.Down: Dev.OnKey(NkKeys.Down, Down); break;
                case Keys.Left: Dev.OnKey(NkKeys.Left, Down); break;
                case Keys.Right: Dev.OnKey(NkKeys.Right, Down); break;
                case Keys.Home: Dev.OnKey(NkKeys.LineStart, Down); break;
                case Keys.End: Dev.OnKey(NkKeys.LineEnd, Down); break;
            }
            if (E.Control && Down) {
                switch (E.KeyCode) {
                    case Keys.C: Dev.OnKey(NkKeys.Copy, true); break;
                    case Keys.V: Dev.OnKey(NkKeys.Paste, true); break;
                    case Keys.X: Dev.OnKey(NkKeys.Cut, true); break;
                    case Keys.A: Dev.OnKey(NkKeys.TextSelectAll, true); break;
                }
            }
        }

        void Render() {
            Thread.Sleep(1000);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (true) {
                BBuffer.Clear(Color.CornflowerBlue);

                Shared.DrawLoop((float)sw.Elapsed.TotalSeconds);
                sw.Restart();

                try {
                    FBuffer.DrawImage(BBufferBitmap, Point.Empty);
                } catch (Exception) {
                    return;
                }

                Thread.Sleep(16);
            }
        }
    }
}
