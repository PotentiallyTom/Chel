using Chel.Parse;
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
        int VertexBufferLength;
        VertexFragmentShader vertexFragmentShader;
        ComputeShader computeShader;
        HyperObject @object;
        Matrix4 viewModelProjectMatrix;
        Matrix4 computeShaderTransformMatrix = Matrix4.Identity;
        List<(string name, int location, float value)> computeShaderUniforms = new();
        List<(string name, int location, float value)> vertexFragmentShaderUniforms = new();
        int compTransformLoc;
        int vertTransformLoc;
        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f,0.2f,0.2f,1.0f);

            renderpack = Renderpack.Load(@"RenderPacks\OrthographicWireframe.yml");
            // renderpack = Renderpack.Load(@"RenderPacks\Slicer.yml");
            @object = new StylParser().ParseFile(@"Models\normalhypercube.styl");

            vertices = @object.AsArray();     

            VertexBufferLength = (int)(vertices.Length * renderpack.OutputRatio);

            vertexFragmentShader = renderpack.VertexFragmentShader;
            computeShader = renderpack.ComputeShader;

            // transformMatrixLocation = GL.GetUniformLocation(computeShader.Handle, "transformMatrix");
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

            compTransformLoc = GL.GetUniformLocation(computeShader.Handle, "transform");
            if(compTransformLoc == -1) throw new InvalidDataException($"Attempting to access the transform matrix in the compute shader, but it doesn't exist");

            vertTransformLoc = GL.GetUniformLocation(vertexFragmentShader.Handle, "transform");
            if(vertTransformLoc == -1) throw new InvalidDataException($"Attempting to access the transform matrix in the vertex shader, but it doesn't exist");

            foreach(string s in new string[] {"sliceDepth"})
            {
                int loc = GL.GetUniformLocation(computeShader.Handle, s);
                if(loc == -1) throw new InvalidDataException($"Attempting to access uniform {s} in the compute shader, but it doesn't exist");
                computeShaderUniforms.Add((s,loc,0));
            }
            foreach(string s in new string[] {})
            {
                int loc = GL.GetUniformLocation(vertexFragmentShader.Handle, s);
                if(loc == -1) throw new InvalidDataException($"Attempting to access uniform {s} in the vertex or fragment shader, but it doesn't exist");
                computeShaderUniforms.Add((s,loc,0));
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            computeShader.Use();

            foreach((string _, int location, float value) in computeShaderUniforms)
            {
                GL.Uniform1(location, value);
            }

            Matrix4 transfom4 = computeShaderTransformMatrix;

            GL.UniformMatrix4(compTransformLoc, false, ref transfom4);

            GL.DispatchCompute(vertices.Length / 16,1,1);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            
            vertexFragmentShader.Use();
            // float[] outputDebug = new float[VertexBufferLength];
            // GL.BindBuffer(BufferTarget.ShaderStorageBuffer, TriBufferObject);
            // GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer,0,VertexBufferLength * sizeof(float), outputDebug);
            
            GL.UniformMatrix4(vertTransformLoc, false, ref viewModelProjectMatrix);
            GL.BindVertexArray(VertexArrayObject);

            GL.DrawArrays(renderpack.PrimitiveType,0,(int)(vertices.Length * renderpack.OutputRatio));
            SwapBuffers();
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            Vector2 mouseDelta = MouseState.Delta;
            float rotationSensitivity = 0.01f;
            float scaleSensitivity = 0.05f;

            if(!(KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift)))
            {
                // 3D view transformations
                float scaleFactor = (float)Math.Exp(MouseState.ScrollDelta.Y * scaleSensitivity);
                Matrix4.CreateScale(scaleFactor, out Matrix4 scale);

                modelMatrix = scale * modelMatrix;
                if(MouseState.IsButtonDown(MouseButton.Left))
                {
                    Matrix4.CreateRotationX(mouseDelta.Y * rotationSensitivity, out Matrix4 rotYZ);
                    Matrix4.CreateRotationY(mouseDelta.X * rotationSensitivity, out Matrix4 rotXZ);

                    modelMatrix = rotXZ * rotYZ * modelMatrix;
                }
                else if(MouseState.IsButtonDown(MouseButton.Right))
                {
                    Matrix4.CreateRotationZ(mouseDelta.X * rotationSensitivity, out Matrix4 rotXY);
                    modelMatrix = rotXY * modelMatrix;
                }
            }
            else
            {
                //4D view transformation
            }

            viewModelProjectMatrix = viewMatrix  * projectionMatrix * modelMatrix ;
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0,0,e.Width,e.Height);
            width = e.Width;
            height = e.Height;
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)width / (float)height, 0.1f, 100.0f);
        }
    // Create a new 4D object
    // start filling in gaps by writing
    // Missing critical analysis
    // Why is it techinically complex?
    // Push it and make it significant

    // How is it useful?
    // How can I demostrate in the report that it's useful?
    // WRapping it up
    // "I spent 2 terms making a render it has these properties"
    // Talk about open sourcing it
    // Installation and running stuff
    // More visualisation stuff - axis 
    // How can you orient yourself in the 4D space?
    // Colour - know where you are based on colour
}