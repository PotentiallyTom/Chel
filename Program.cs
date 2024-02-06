using Chel.Render;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Desktop;

namespace Chel;

public class Program
{
    public static void Main(string[] args)
    {
        using(RenderWindow rw = new(800,600,"Chel"))
        {
            rw.Run();
        }
    }
}