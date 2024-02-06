using OpenTK.Graphics.OpenGL4;

namespace Chel.Render;

public interface IShader
{
    public int Handle {get;}
    public void Use();
}
public class VertexFragmentShader : IDisposable, IShader
{
    public int Handle {get;}
    public VertexFragmentShader(string vertexPath, string fragmentPath)
    {
        int success;

        int VertexShader;
        int FragmentShader;

        string VertexShaderSource = File.ReadAllText(vertexPath);
        string FragmentShaderSource = File.ReadAllText(fragmentPath);

        VertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexShader, VertexShaderSource);

        FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentShader, FragmentShaderSource);

        GL.CompileShader(VertexShader);
        GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(VertexShader);
            Console.WriteLine(infoLog);
        }

        GL.CompileShader(FragmentShader);
        GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(FragmentShader);
            Console.WriteLine(infoLog);
        }

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, VertexShader);
        GL.AttachShader(Handle, FragmentShader);
        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }

        GL.DetachShader(Handle, VertexShader);
        GL.DetachShader(Handle, FragmentShader);
        GL.DeleteShader(FragmentShader);
        GL.DeleteShader(VertexShader);
    }
    public void Use()
    {
        GL.UseProgram(Handle);
    }

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);

            disposedValue = true;
        }
    }

    ~VertexFragmentShader()
    {
        if (disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }
}

public class ComputeShader : IDisposable, IShader
{
    public int Handle {get;}

        public ComputeShader(string computePath)
    {
        int success;

        int ComputeShader;

        string ComputeshaderSource = File.ReadAllText(computePath);

        ComputeShader = GL.CreateShader(ShaderType.ComputeShader);
        GL.ShaderSource(ComputeShader, ComputeshaderSource);

        GL.CompileShader(ComputeShader);
        GL.GetShader(ComputeShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(ComputeShader);
            Console.WriteLine(infoLog);
        }

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, ComputeShader);
        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }

        GL.DetachShader(Handle, ComputeShader);
        GL.DeleteShader(ComputeShader);
    }
    public void Use()
    {
        GL.UseProgram(Handle);
    }

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);

            disposedValue = true;
        }
    }

    ~ComputeShader()
    {
        if (disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }
}