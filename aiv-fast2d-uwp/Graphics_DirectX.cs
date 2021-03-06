﻿using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using Matrix4 = SharpDX.Matrix;
using SharpDX.D3DCompiler;

namespace Aiv.Fast2D
{
    public static class Graphics
    {
        private static int internalCounter;

        private static int GetNextId()
        {
            return internalCounter++;
        }

        private static DeviceContext2 currentContext;
        private static RenderTargetView currentTargetView;
        private static Color currentClearColor;

        private static int currentBuffer;
        private static int currentShader;

        private static Dictionary<int, SharpDX.Direct3D11.Buffer> buffers;
        private static Dictionary<int, DirectXBufferData> buffersData;

        private class DirectXBufferData
        {
            public int index;
            public int elementSize;
            public DirectXArray array;
        }

        private class DirectXConstantBuffer
        {
            public int index;
            public SharpDX.Direct3D11.Buffer buffer;
            public int mode;
        }

        private class DirectXShader
        {
            private VertexShader vs;
            private PixelShader ps;
            private InputLayout inputLayout;
            private Dictionary<string, DirectXConstantBuffer> constantBuffers;
            private Dictionary<int, DirectXConstantBuffer> constantBuffersByUid;

            public DirectXShader(VertexShader vs, PixelShader ps, InputLayout layout, string[] vertexUniforms, string[] fragmentUniforms)
            {
                this.vs = vs;
                this.ps = ps;
                this.inputLayout = layout;
                this.constantBuffers = new Dictionary<string, DirectXConstantBuffer>();
                this.constantBuffersByUid = new Dictionary<int, DirectXConstantBuffer>();
                int i = 0;
                if (vertexUniforms != null)
                {
                    for (int j = 0; j < vertexUniforms.Length; j++)
                    {
                        // alocate space for a float4x4
                        float[] data = new float[16];
                        SharpDX.Direct3D11.Buffer constantBuffer = SharpDX.Direct3D11.Buffer.Create(currentContext.Device, BindFlags.ConstantBuffer, data, data.Length * sizeof(float));
                        constantBuffers[vertexUniforms[j]] = new DirectXConstantBuffer() { index = i, buffer = constantBuffer, mode = 0 };
                        constantBuffersByUid[i] = constantBuffers[vertexUniforms[j]];
                        i++;
                    }
                }
                if (fragmentUniforms != null)
                {
                    for (int j = 0; j < fragmentUniforms.Length; j++)
                    {
                        // alocate space for a float4x4
                        float[] data = new float[16];
                        SharpDX.Direct3D11.Buffer constantBuffer = SharpDX.Direct3D11.Buffer.Create(currentContext.Device, BindFlags.ConstantBuffer, data, data.Length * sizeof(float));
                        constantBuffers[fragmentUniforms[j]] = new DirectXConstantBuffer() { index = i, buffer = constantBuffer, mode = 1 };
                        constantBuffersByUid[i] = constantBuffers[fragmentUniforms[j]];
                        i++;
                    }
                }
            }

            public int GetUniform(string name)
            {
                return this.constantBuffers[name].index;
            }

            public void SetUniform<T>(int uid, T value) where T : struct
            {
                var buffer = this.constantBuffersByUid[uid].buffer;
                int slot = this.constantBuffersByUid[uid].index;
                int mode = this.constantBuffersByUid[uid].mode;
                currentContext.UpdateSubresource<T>(ref value, buffer);
                if (mode == 0)
                {
                    currentContext.VertexShader.SetConstantBuffer(slot, buffer);
                }
                else if (mode == 1)
                {
                    currentContext.PixelShader.SetConstantBuffer(slot, buffer);
                }
            }

            public void Use()
            {
                currentContext.VertexShader.Set(vs);
                currentContext.PixelShader.Set(ps);
                currentContext.InputAssembler.InputLayout = inputLayout;

                for (int i = 0; i < currentArray.ActiveBuffers; i++)
                {
                    currentContext.InputAssembler.SetVertexBuffers(i, currentArray.Buffers[i]);
                }

                currentContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            }
        }

        private class DirectXArray
        {
            private VertexBufferBinding[] buffers;
            private int activeBuffers;

            public int ActiveBuffers
            {
                get
                {
                    return activeBuffers;
                }
            }

            public DirectXArray()
            {

                buffers = new VertexBufferBinding[8];
                activeBuffers = 0;
            }

            public void SetBuffer(int index, VertexBufferBinding buffer)
            {
                buffers[index] = buffer;
                if (index + 1 > activeBuffers)
                    activeBuffers = index + 1;
            }

            public VertexBufferBinding[] Buffers
            {
                get
                {
                    return buffers;
                }
            }
        }

        private static DirectXArray currentArray;

        private static Dictionary<int, DirectXShader> shaders;

        private static Dictionary<int, DirectXArray> arrays;

        private static Dictionary<int, SharpDX.Direct3D11.Texture2D> textures;
        private static int currentTexture;

        static Graphics()
        {
            buffers = new Dictionary<int, SharpDX.Direct3D11.Buffer>();
            shaders = new Dictionary<int, DirectXShader>();
            arrays = new Dictionary<int, DirectXArray>();
            buffersData = new Dictionary<int, DirectXBufferData>();
            textures = new Dictionary<int, Texture2D>();

            internalCounter = 0;
        }

        public static void SetContext(Window window)
        {
            currentContext = window.GetDeviceContext();
            currentTargetView = window.GetRenderTargetView();
            currentClearColor = Color.Black;
            currentContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
        }

        public static void BindFrameBuffer(int frameBufferId)
        {
        }

        public static string GetError()
        {
            return "";
        }

        public static void SetAlphaBlending()
        {
        }

        public static void SetMaskedBlending()
        {
        }

        public static void Setup()
        {
            SetAlphaBlending();
        }

		public static void EnableDepthTest()
		{
		}

		public static void DisableDepthTest()
		{
		}

        public static void ClearColor()
        {
            currentContext.ClearRenderTargetView(currentTargetView, currentClearColor);
        }

        public static void DeleteBuffer(int id)
        {
        }

        public static void DeleteTexture(int id)
        {
        }

        public static void DeleteShader(int id)
        {
        }

        public static void DeleteArray(int id)
        {
        }

        public static void Viewport(int x, int y, int width, int height)
        {
            currentContext.Rasterizer.SetViewport(x, y, width, height);
        }

        public static void EnableScissorTest()
        {
        }

        public static void DisableScissorTest()
        {
        }

        public static void Scissor(int x, int y, int width, int height)
        {
        }

        public static void SetClearColor(float r, float g, float b, float a)
        {
            currentClearColor = new Color(r, g, b, a);
        }

        public static int GetDefaultFrameBuffer()
        {
            return -1;
        }

        public static void BindTextureToUnit(int textureId, int unit)
        {
            currentTexture = textureId;
        }

        public static int NewBuffer()
        {
            // allocate the space for a quad
            float[] defaultData = new float[6 * 2];
            BufferDescription bufferDescription = new BufferDescription(defaultData.Length * sizeof(float), ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            SharpDX.Direct3D11.Buffer buffer = SharpDX.Direct3D11.Buffer.Create(currentContext.Device, defaultData, bufferDescription);
            int id = GetNextId();
            buffers[id] = buffer;
            return id;
        }

        public static int NewArray()
        {
            DirectXArray array = new DirectXArray();
            int id = GetNextId();
            arrays[id] = array;
            return id;
        }


        public static void DrawArrays(int amount)
        {
            currentContext.Draw(amount, 0);
        }

        private static void RemapVertexBufferBinding(int bufferId)
        {
            DirectXBufferData data = buffersData[bufferId];
            currentArray.SetBuffer(data.index, new VertexBufferBinding(buffers[bufferId], data.elementSize * sizeof(float), 0));
        }

        public static void MapBufferToArray(int bufferId, int index, int elementSize)
        {
            BindBuffer(bufferId);
            buffersData[bufferId] = new DirectXBufferData() { index = index, elementSize = elementSize, array = currentArray };
            RemapVertexBufferBinding(bufferId);
        }

        public static void BufferData(float[] data)
        {
            DataStream stream;
            currentContext.MapSubresource(buffers[currentBuffer], MapMode.WriteDiscard, MapFlags.None, out stream);
            if (stream.Length < data.Length * sizeof(float))
            {
                stream.Dispose();
                currentContext.UnmapSubresource(buffers[currentBuffer], 0);
                buffers[currentBuffer].Dispose();
                BufferDescription bufferDescription = new BufferDescription(data.Length * sizeof(float), ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
                SharpDX.Direct3D11.Buffer buffer = SharpDX.Direct3D11.Buffer.Create(currentContext.Device, data, bufferDescription);
                buffers[currentBuffer] = buffer;
                RemapVertexBufferBinding(currentBuffer);
            }
            unsafe
            {
                fixed (float* dataFloat = data)
                {
                    IntPtr dataPtr = new IntPtr(dataFloat);
                    stream.Write(dataPtr, 0, data.Length * sizeof(float));
                    stream.Dispose();
                }
            }
            currentContext.UnmapSubresource(buffers[currentBuffer], 0);
        }

        public static void BufferData(int bufferId, float[] data)
        {
            BindBuffer(bufferId);
            BufferData(data);
        }


        public static void BufferSubData(float[] data, int offset = 0)
        {
        }

        public static void BufferSubData(int bufferId, float[] data, int offset = 0)
        {
        }

        public static string Version
        {
            get
            {
                return "";
            }
        }

        public static string Vendor
        {
            get
            {
                return "";
            }
        }

        public static string SLVersion
        {
            get
            {
                return "";
            }
        }

        public static string Renderer
        {
            get
            {
                return "";
            }
        }

        public static string Extensions
        {
            get
            {
                return "";
            }
        }

        public static void SetArrayDivisor(int id, int divisor)
        {
        }

        public static void DrawArraysInstanced(int amount, int instances)
        {
            currentContext.DrawInstanced(amount, instances, 0, 0);
        }

        public static int NewFrameBuffer()
        {
            return -1;
        }


        public static void FrameBufferTexture(int id)
        {
        }

        public static void BindArray(int id)
        {
            currentArray = arrays[id];
        }

        public static void BindBuffer(int id)
        {
            currentBuffer = id;
        }

        public static int NewTexture()
        {
            Texture2DDescription desc = new Texture2DDescription();
            Texture2D texture = new Texture2D(currentContext.Device, desc);
            int id = GetNextId();
            textures[id] = texture;
            return id;
        }

        public static void TextureBitmap(int width, int height, byte[] bitmap, int mipMap = 0)
        {
            Texture2D texture = textures[currentTexture];
        }

        public static void TextureSetRepeatX(bool repeat = true)
        {

        }

        public static void TextureSetRepeatY(bool repeat = true)
        {

        }

        public static void TextureSetLinear(bool mipMap = false)
        {

        }

        public static void TextureSetNearest(bool mipMap = false)
        {

        }

        public static int CompileShader(string vertexModern, string fragmentModern, string vertexObsolete = null, string fragmentObsolete = null, string[] attribs = null, int[] attribsSizes = null, string[] vertexUniforms = null, string[] fragmentUniforms = null)
        {

            VertexShader vertexShader;
            PixelShader fragmentShader;
            InputLayout inputLayout;

            using (var vertexShaderByteCode = ShaderBytecode.Compile(vertexModern, "main", "vs_4_0", ShaderFlags.Debug))
            {
                vertexShader = new VertexShader(currentContext.Device, vertexShaderByteCode);
                List<InputElement> elements = new List<InputElement>();
                for (int i = 0; i < attribs.Length; i++)
                {
                    SharpDX.DXGI.Format format = SharpDX.DXGI.Format.R32G32_Float;
                    if (attribsSizes[i] == 3)
                        format = SharpDX.DXGI.Format.R32G32B32_Float;
                    else if (attribsSizes[i] == 4)
                        format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                    elements.Add(new InputElement(attribs[i].ToUpper(), 0, format, i));
                }
                inputLayout = new InputLayout(currentContext.Device, vertexShaderByteCode, elements.ToArray());
            }

            using (var fragmentShaderByteCode = ShaderBytecode.Compile(fragmentModern, "main", "ps_4_0", ShaderFlags.Debug))
            {
                fragmentShader = new PixelShader(currentContext.Device, fragmentShaderByteCode);
            }

            int id = GetNextId();

            shaders[id] = new DirectXShader(vertexShader, fragmentShader, inputLayout, vertexUniforms, fragmentUniforms);

            return id;
        }

        public static void BindShader(int shaderId)
        {
            currentShader = shaderId;
            shaders[currentShader].Use();
        }

        public static int GetShaderUniformId(int shaderId, string name)
        {
            return shaders[shaderId].GetUniform(name);
        }

        public static void SetShaderUniform(int uid, int value)
        {
            shaders[currentShader].SetUniform(uid, value);
        }

        public static void SetShaderUniform(int uid, float value)
        {
            shaders[currentShader].SetUniform(uid, value);
        }

        public static void SetShaderUniform(int uid, Vector4 value)
        {
            shaders[currentShader].SetUniform(uid, value);
        }

        public static void SetShaderUniform(int uid, Matrix4 value)
        {
            shaders[currentShader].SetUniform(uid, value);
        }
    }
}
