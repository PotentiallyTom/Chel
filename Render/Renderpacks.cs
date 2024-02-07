using OpenTK.Graphics.OpenGL4;
using YamlDotNet.Serialization;
namespace Chel.Render;
public readonly struct Renderpack
{
    private struct inputModel
    {
        public float OutputRatio {get;set;}
        public string? VertexShaderPath {get;set;}
        public string? FragmentShaderPath {get;set;}
        public string? ComputeShaderPath {get;set;}
        public string? PrimitiveType {get;set;} 
    }
    public float OutputRatio {get;}
    public VertexFragmentShader VertexFragmentShader {get;}
    public ComputeShader ComputeShader {get;}
    public PrimitiveType PrimitiveType {get;}

    public static Renderpack Load(string path)
    {
        string data = File.ReadAllText(path);
        inputModel parsed = new DeserializerBuilder().Build().Deserialize<inputModel>(data);

        return new Renderpack(parsed.OutputRatio, parsed.VertexShaderPath ?? "", parsed.FragmentShaderPath ?? "", parsed.ComputeShaderPath ?? "", Enum.Parse<PrimitiveType>(parsed.PrimitiveType ?? ""));
    }

    public Renderpack(
        float outputRatio, 
        string vertexShaderPath, 
        string fragmentShaderPath, 
        string computeShaderPath, 
        PrimitiveType primitiveType)
    {
        OutputRatio = outputRatio;
        VertexFragmentShader = new VertexFragmentShader(vertexShaderPath, fragmentShaderPath);
        ComputeShader = new ComputeShader(computeShaderPath);
        PrimitiveType = primitiveType;
    }
}