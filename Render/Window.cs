﻿using Chel.Parse;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Chel.Render;
public class RenderWindow : GameWindow
{
    public RenderWindow(int width, int height, string title) 
        : base(
            GameWindowSettings.Default, 
            new NativeWindowSettings() { Size = (width, height), Title = title }
        ) 
        {
            VSync = VSyncMode.On;
            this.width = width;
            this.height = height;
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(0.7f, (float)width / (float)height, 0.1f, 100.0f);
            modelMatrix = Matrix4.Identity;
        }
        Renderpack renderpack;
        int width;
        int height;
        Vector3 cameraPosition = new Vector3(0f,0f,3f);
        Vector3 cameraTarget = new Vector3(0f,0f,0);
        Vector3 cameraDirection {get => Vector3.Normalize(cameraPosition - cameraTarget);} 
        Vector3 cameraRight {get =>Vector3.Normalize(Vector3.Cross(Vector3.UnitY, cameraDirection));}
        Vector3 cameraUp {get => Vector3.Cross(cameraDirection, cameraRight);}
        Matrix4 viewMatrix {get => Matrix4.LookAt(cameraPosition, cameraTarget, cameraUp);}
        Matrix4 projectionMatrix;
        Matrix4 modelMatrix;
        float[] vertices;
        int VertexArrayObject;
        int FacetBufferObject;
        int TriBufferObject;
        int ConstBufferObject;
        int VertexBufferLength;
        VertexFragmentShader vertexFragmentShader;
        ComputeShader computeShader;
        HyperObject @object;
        int transformMatrixLocation;

        Matrix4 viewModelProjectMatrix;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f,0.2f,0.2f,1.0f);

            renderpack = Renderpack.Load(@"RenderPacks\Slicer.yml");
            @object = new StylParser().ParseFile(@"Models\normalhypercube.styl");

            vertices = @object.AsArray();     

            VertexBufferLength = (int)(vertices.Length * renderpack.OutputRatio);

            vertexFragmentShader = renderpack.VertexFragmentShader;
            computeShader = renderpack.ComputeShader;

            // transformMatrixLocation = GL.GetUniformLocation(shader.Handle, "transformMatrix");
            // GL.UniformMatrix4(transformMatrixLocation, false, ref viewModelProjectMatrix);

            FacetBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, FacetBufferObject);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, FacetBufferObject);

            TriBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, TriBufferObject);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, VertexBufferLength * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, TriBufferObject);

            // ConstBufferObject = GL.GenBuffer();
            // GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ConstBufferObject);
            // GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float), new float[] {0}, BufferUsageHint.StreamDraw);
            // GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, ConstBufferObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, TriBufferObject);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            
            computeShader.Use();

            int wSliceLoc = GL.GetUniformLocation(computeShader.Handle,"sliceDepth");
            // GL.Uniform1(wSliceLoc,1,new float[] {0.5f});
            GL.Uniform1(wSliceLoc,1,new float[] {new Random().NextSingle()});

            Matrix4 transfom4 = new Matrix4(
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                0,0,0,1
            );

            int transformLoc = GL.GetUniformLocation(computeShader.Handle, "transform");
            GL.UniformMatrix4(transformLoc, false, ref transfom4);

            GL.DispatchCompute(vertices.Length / 16,1,1);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            
            vertexFragmentShader.Use();
            // float[] outputDebug = new float[VertexBufferLength];
            // GL.BindBuffer(BufferTarget.ShaderStorageBuffer, TriBufferObject);
            // GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer,0,VertexBufferLength * sizeof(float), outputDebug);

            GL.BindVertexArray(VertexArrayObject);

            GL.DrawArrays(renderpack.PrimitiveType,0,(int)(vertices.Length * renderpack.OutputRatio));
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0,0,e.Width,e.Height);
            width = e.Width;
            height = e.Height;
        }
    // Create a new 4D object
    // start filling in gaps by writing
    // Missing critical analysis
    // Why is it techinically complex?
    // Push it and make it significant
}