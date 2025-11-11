//using System;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using ImGuiNET;
//using UnityEngine;

//namespace CU_ItemSpawner_Mod
//{
//    extern alias vectors;
//    public class ImGuiRenderer : IDisposable
//    {
//        private IntPtr _context;
//        private bool _frameBegun;
//        private Texture2D _fontTexture;
//        private Material _material;
//        private Mesh _mesh;
//        private List<Vector3> _verts = new List<Vector3>();
//        private List<Vector2> _uvs = new List<Vector2>();
//        private List<Color> _cols = new List<Color>();
//        private List<int> _idx = new List<int>();

//        public ImGuiRenderer()
//        {
//            _context = ImGui.CreateContext();
//            ImGui.SetCurrentContext(_context);
//            var io = ImGui.GetIO();
//            io.Fonts.AddFontDefault();
//            CreateDeviceObjects();
//        }

//        public void Dispose()
//        {
//            if (_mesh != null)
//            {
//                UnityEngine.Object.Destroy(_mesh);
//                _mesh = null;
//            }
//            if (_material != null)
//            {
//                UnityEngine.Object.Destroy(_material);
//                _material = null;
//            }
//            if (_fontTexture != null)
//            {
//                UnityEngine.Object.Destroy(_fontTexture);
//                _fontTexture = null;
//            }
//            ImGui.DestroyContext(_context);
//        }

//        private void CreateDeviceObjects()
//        {
//            var io = ImGui.GetIO();
//            IntPtr pixels;
//            int width, height, bytesPerPixel;
//            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);
//            int size = width * height * bytesPerPixel;
//            byte[] pixelBytes = new byte[size];
//            Marshal.Copy(pixels, pixelBytes, 0, size);
//            _fontTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
//            _fontTexture.LoadRawTextureData(pixelBytes);
//            _fontTexture.Apply();
//            io.Fonts.SetTexID((IntPtr)1);
//            Shader shader = Shader.Find("Unlit/Transparent");
//            if (shader == null)
//                shader = Shader.Find("Sprites/Default");
//            _material = new Material(shader);
//            _material.mainTexture = _fontTexture;
//            _mesh = new Mesh();
//            _mesh.MarkDynamic();
//        }

//        public void BeginFrame()
//        {
//            if (_frameBegun) return;
//            ImGui.SetCurrentContext(_context);
//            var io = ImGui.GetIO();
//            io.DisplaySize = new vectors::System.Numerics.Vector2(Screen.width, Screen.height);
//            io.DeltaTime = Time.deltaTime > 0 ? Time.deltaTime : 1f / 60f;
//            UpdateInput(io);
//            ImGui.NewFrame();
//            _frameBegun = true;
//        }

//        public void EndFrame()
//        {
//            if (!_frameBegun) return;
//            ImGui.Render();
//            RenderDrawData(ImGui.GetDrawData());
//            _frameBegun = false;
//        }

//        private void UpdateInput(ImGuiIOPtr io)
//        {
//            io.MousePos = new vectors::System.Numerics.Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
//            io.MouseDown[0] = Input.GetMouseButton(0);
//            io.MouseDown[1] = Input.GetMouseButton(1);
//            io.MouseDown[2] = Input.GetMouseButton(2);
//            foreach (char c in Input.inputString)
//                io.AddInputCharacter(c);
//        }

//        private Color ColorFromUInt(uint col)
//        {
//            byte r = (byte)((col >> 0) & 0xFF);
//            byte g = (byte)((col >> 8) & 0xFF);
//            byte b = (byte)((col >> 16) & 0xFF);
//            byte a = (byte)((col >> 24) & 0xFF);
//            return new Color32(r, g, b, a);
//        }

//        private void RenderDrawData(ImDrawDataPtr drawData)
//        {
//            if (drawData.CmdListsCount == 0) return;
//            _verts.Clear();
//            _uvs.Clear();
//            _cols.Clear();
//            _idx.Clear();
//            int vtxOffset = 0;
//            for (int n = 0; n < drawData.CmdListsCount; n++)
//            {
//                var cmdList = drawData.CmdLists[n];
//                var vtxBuffer = cmdList.VtxBuffer;
//                var idxBuffer = cmdList.IdxBuffer;
//                for (int i = 0; i < vtxBuffer.Size; i++)
//                {
//                    var v = vtxBuffer[i];
//                    _verts.Add(new Vector3(v.pos.X, Screen.height - v.pos.Y, 0));
//                    _uvs.Add(new Vector2(v.uv.X, v.uv.Y));
//                    _cols.Add(ColorFromUInt(v.col));
//                }
//                for (int i = 0; i < idxBuffer.Size; i++)
//                    _idx.Add(idxBuffer[i]);
//                vtxOffset += vtxBuffer.Size;
//            }
//            _mesh.Clear();
//            _mesh.SetVertices(_verts);
//            _mesh.SetUVs(0, _uvs);
//            _mesh.SetColors(_cols);
//            _mesh.SetTriangles(_idx, 0);
//            Matrix4x4 matrix = Matrix4x4.identity;
//            _material.mainTexture = _fontTexture;
//            _material.SetPass(0);
//            Graphics.DrawMeshNow(_mesh, matrix);
//        }
//    }
//}
