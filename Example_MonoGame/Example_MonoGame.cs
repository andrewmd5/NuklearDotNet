using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuklearDotNet;
using System;
using System.Runtime.InteropServices;

namespace Example_MonoGame
{
    internal unsafe class MonoGameDevice : NuklearDeviceTex<Texture2D>
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;
        private readonly RasterizerState _rasterizerState;
        private NKVertexPositionColorTexture[] _vertexBuffer;
        private short[] _indexBuffer;

        public MonoGameDevice(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            _effect = new BasicEffect(graphicsDevice);
            _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = true;

            _rasterizerState = new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true };
        }

        public override Texture2D CreateTexture(int W, int H, IntPtr Data)
        {
            var data = new ReadOnlySpan<int>((void*)Data, W * H).ToArray();

            var texture = new Texture2D(_graphicsDevice, W, H);
            texture.SetData(data);

            return texture;
        }

        public override void Render(NkHandle Userdata, Texture2D Texture, NkRect ClipRect, uint Offset, uint Count)
        {
            var prevBlendState = _graphicsDevice.BlendState;
            var prevSamplerState = _graphicsDevice.SamplerStates[0];
            var prevDepthStencilState = _graphicsDevice.DepthStencilState;
            var prevRasterizerState = _graphicsDevice.RasterizerState;
            var prevScissor = _graphicsDevice.ScissorRectangle;

            _graphicsDevice.BlendState = BlendState.NonPremultiplied;
            _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = _rasterizerState;

            _effect.Projection = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0, 100f);
            _effect.Texture = Texture;
            _effect.CurrentTechnique.Passes[0].Apply();

            _graphicsDevice.ScissorRectangle = new Rectangle((int)ClipRect.X, (int)ClipRect.Y, (int)ClipRect.W, (int)ClipRect.H);

            _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexBuffer, 0, _vertexBuffer.Length, _indexBuffer, (int)Offset, (int)Count / 3);

            _graphicsDevice.BlendState = prevBlendState;
            _graphicsDevice.SamplerStates[0] = prevSamplerState;
            _graphicsDevice.DepthStencilState = prevDepthStencilState;
            _graphicsDevice.RasterizerState = prevRasterizerState;
            _graphicsDevice.ScissorRectangle = prevScissor;
        }

        public override void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer)
        {
            _vertexBuffer = MemoryMarshal.Cast<NkVertex, NKVertexPositionColorTexture>(VertexBuffer).ToArray();
            _indexBuffer = MemoryMarshal.Cast<ushort, short>(IndexBuffer).ToArray();
        }

        /// <summary>
        /// Struct that matches <see cref="NkVertex"/> layout so we can reinterpret-cast without per-vertex conversion.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 12)]
        struct NKVertexPositionColorTexture : IVertexType
        {
            public Vector2 Position;
            public Vector2 UV;
            public NkColor Color;

            public static readonly VertexDeclaration VertexDeclaration;

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

            static NKVertexPositionColorTexture()
            {
                VertexDeclaration = new VertexDeclaration(
                    new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                    new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(16, VertexElementFormat.Byte4, VertexElementUsage.Color, 0));
            }
        }
    }
}
