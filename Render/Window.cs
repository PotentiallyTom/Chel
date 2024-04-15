using Chel.Parse;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using YamlDotNet.Core.Tokens;

namespace Chel.Render;
public class RenderWindow : GameWindow
{
    public RenderWindow(int width, int height, string title, string renderpackPath, string modelPath) 
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

            renderpack = Renderpack.Load(renderpackPath);
            @object = new StylParser().ParseFile(modelPath);
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

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Gequal);
            GL.ClearDepth(-20);
            GL.ClearColor(0.2f,0.2f,0.2f,1.0f);
            // GL.ClearColor(1f,1f,1f,1f);

            vertices = @object.AsArray();     

            VertexBufferLength = (int)(vertices.Length * renderpack.OutputRatio);

            vertexFragmentShader = renderpack.VertexFragmentShader;
            computeShader = renderpack.ComputeShader;

            FacetBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, FacetBufferObject);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, FacetBufferObject);

            TriBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, TriBufferObject);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, VertexBufferLength * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, TriBufferObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, TriBufferObject);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1,1,VertexAttribPointerType.Float, false, 4 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            compTransformLoc = GL.GetUniformLocation(computeShader.Handle, "transform");
            if(compTransformLoc == -1) throw new InvalidDataException($"Attempting to access the transform matrix in the compute shader, but it doesn't exist");

            vertTransformLoc = GL.GetUniformLocation(vertexFragmentShader.Handle, "transform");
            if(vertTransformLoc == -1) throw new InvalidDataException($"Attempting to access the transform matrix in the vertex shader, but it doesn't exist");

            foreach(string s in renderpack.ComputeShaderUniforms)
            {
                int loc = GL.GetUniformLocation(computeShader.Handle, s);
                if(loc == -1) throw new InvalidDataException($"Attempting to access uniform {s} in the compute shader, but it doesn't exist");
                computeShaderUniforms.Add((s,loc,0));
            }
            foreach(string s in renderpack.VertexFragmentShaderUniforms)
            {
                int loc = GL.GetUniformLocation(vertexFragmentShader.Handle, s);
                if(loc == -1) throw new InvalidDataException($"Attempting to access uniform {s} in the vertex or fragment shader, but it doesn't exist");
                vertexFragmentShaderUniforms.Add((s,loc,0));
            }

            if(computeShaderUniforms.Count() + vertexFragmentShaderUniforms.Count() > 8)
            {
                throw new InvalidDataException($"Attempting to define too many arbitrary uniforms ({computeShaderUniforms.Count() + vertexFragmentShaderUniforms.Count()} > 8)");
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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

            // Console.WriteLine("====");
            // for(int i = 0 ; i < outputDebug.Length ; i += 4)
            // {
            //     Console.WriteLine($"{outputDebug[i]} {outputDebug[i+1]} {outputDebug[i+2]} {outputDebug[i+3]}");
            // }

            foreach((string _, int location, float value) in vertexFragmentShaderUniforms)
            {
                GL.Uniform1(location, value);
            }

            GL.UniformMatrix4(vertTransformLoc, false, ref viewModelProjectMatrix);
            GL.BindVertexArray(VertexArrayObject);

            GL.DrawArrays(renderpack.PrimitiveType,0,(int)(vertices.Length * renderpack.OutputRatio));
            // GL.DrawArrays(renderpack.PrimitiveType,0,3);
            SwapBuffers();
        }
        bool wasKeyDown = false;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            Vector2 mouseDelta = MouseState.Delta;
            float rotationSensitivity3D = 0.01f;
            float rotationSensitivity4D = 0.015f;
            float scaleSensitivity = 0.05f;

            // 3D view transformations
            float scaleFactor = (float)Math.Exp(MouseState.ScrollDelta.Y * scaleSensitivity);
            Matrix4.CreateScale(scaleFactor, out Matrix4 scale);

            modelMatrix = scale * modelMatrix;
            if(MouseState.IsButtonDown(MouseButton.Left))
            {
                Matrix4.CreateRotationX(mouseDelta.Y * rotationSensitivity3D, out Matrix4 rotYZ);
                Matrix4.CreateRotationY(mouseDelta.X * rotationSensitivity3D, out Matrix4 rotXZ);

                modelMatrix = rotXZ * rotYZ * modelMatrix;
            }
            else if(MouseState.IsButtonDown(MouseButton.Right))
            {
                Matrix4.CreateRotationZ(mouseDelta.X * rotationSensitivity3D, out Matrix4 rotXY);
                modelMatrix = rotXY * modelMatrix;
            }
            viewModelProjectMatrix = viewMatrix  * projectionMatrix * modelMatrix ;

            // 4D view transformations
            if(KeyboardState.IsKeyDown(Keys.LeftShift)) rotationSensitivity4D *= -1;
            float cosSens4 = (float)Math.Cos(rotationSensitivity4D);
            float sinSens4 = (float)Math.Sin(rotationSensitivity4D);
            if(KeyboardState.IsKeyDown(Keys.A))
            {
                //XY    
                Matrix4.CreateRotationZ(rotationSensitivity4D, out Matrix4 res);
                computeShaderTransformMatrix = res * computeShaderTransformMatrix;
            }
            if(KeyboardState.IsKeyDown(Keys.S))
            {
                //XZ
                Matrix4.CreateRotationY(rotationSensitivity4D, out Matrix4 res);
                computeShaderTransformMatrix = res * computeShaderTransformMatrix;
            }
            if(KeyboardState.IsKeyDown(Keys.D))
            {
                //YZ
                Matrix4.CreateRotationX(rotationSensitivity4D, out Matrix4 res);
                computeShaderTransformMatrix = res * computeShaderTransformMatrix;
            }
            if(KeyboardState.IsKeyDown(Keys.Q))
            {
                //XW
                Matrix4 res = new Matrix4(
                    cosSens4, 0, 0, -sinSens4,
                    0,        1, 0, 0,
                    0,        0, 1, 0,
                    sinSens4, 0, 0, cosSens4
                );
                computeShaderTransformMatrix = res * computeShaderTransformMatrix;
            }
            if(KeyboardState.IsKeyDown(Keys.W))
            {
                //YW
                Matrix4 res = new Matrix4(
                    1, 0,        0, 0,
                    0, cosSens4, 0, -sinSens4,
                    0, 0,        1, 0,
                    0, sinSens4, 0, cosSens4
                );
                computeShaderTransformMatrix = res * computeShaderTransformMatrix;
            }
            if(KeyboardState.IsKeyDown(Keys.E))
            {
                //ZW
                Matrix4 res = new Matrix4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, cosSens4, -sinSens4,
                    0, 0, sinSens4, cosSens4
                );
                computeShaderTransformMatrix = res * computeShaderTransformMatrix;
            }
            if(KeyboardState.IsKeyPressed(Keys.R))
            {
                computeShaderTransformMatrix = Matrix4.Identity;
            }

            // Arbitrary inputs
            float arbSens = 0.005f;
            int keyIndex = 0;
            if(KeyboardState.IsKeyDown(Keys.LeftShift)) arbSens *= -1;
            foreach(Keys k in new Keys[] {Keys.Z, Keys.X, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M, Keys.Comma}
                .Take(computeShaderUniforms.Count() + vertexFragmentShaderUniforms.Count()))
            {
                // Work out which set of uniforms we need to update
                if(keyIndex < computeShaderUniforms.Count())
                {
                    int index = keyIndex;
                    if(KeyboardState.IsKeyDown(k))
                    {
                        computeShaderUniforms[index] = (computeShaderUniforms[index].name, computeShaderUniforms[index].location, computeShaderUniforms[index].value + arbSens);
                    }
                }
                else
                {
                    int index = keyIndex - computeShaderUniforms.Count();
                    if(KeyboardState.IsKeyDown(k))
                    {
                        computeShaderUniforms[index] = (computeShaderUniforms[index].name, computeShaderUniforms[index].location, computeShaderUniforms[index].value + arbSens);
                    }
                }
                keyIndex++;
            }

            // Dump the state after the keys are released
            if(KeyboardState.IsAnyKeyDown || MouseState.IsAnyButtonDown) wasKeyDown = true;
            if(!KeyboardState.IsAnyKeyDown && !MouseState.IsAnyButtonDown && wasKeyDown)
            {
                wasKeyDown = false;
                Console.WriteLine();
                Console.WriteLine("Current State:");
                Console.WriteLine("4D Transform matrix:");
                Console.WriteLine(computeShaderTransformMatrix.Row0);
                Console.WriteLine(computeShaderTransformMatrix.Row1);
                Console.WriteLine(computeShaderTransformMatrix.Row2);
                Console.WriteLine(computeShaderTransformMatrix.Row3);
                Console.WriteLine();
                Console.WriteLine("3D Transform matrix:");
                Console.WriteLine(modelMatrix.Row0);
                Console.WriteLine(modelMatrix.Row1);
                Console.WriteLine(modelMatrix.Row2);
                Console.WriteLine(modelMatrix.Row3);
                Console.WriteLine();
                Console.WriteLine("Compute Uniforms:");
                foreach(var u in computeShaderUniforms)
                {
                    Console.WriteLine($"{u.name}: {u.value}");
                }
                Console.WriteLine();
                Console.WriteLine("Vertex Fragment Uniforms:");
                foreach(var u in vertexFragmentShaderUniforms)
                {
                    Console.WriteLine($"{u.name}: {u.value}");
                }
                Console.WriteLine();
                Console.WriteLine("=================");
            }
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