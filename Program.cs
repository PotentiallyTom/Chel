using Chel.Render;

namespace Chel;

public class Program
{
    const string DEFAULT_PACK = @"RenderPacks\OrthographicWireframe.yml";
    const string DEFAULT_MODEL = @"Models\normalhypercube.styl";
    public static void Main(string[] args)
    {
        string pack = DEFAULT_PACK;
        string model = DEFAULT_MODEL;
        switch(args.Length)
        {
            case 0:
                break;
            case 1 : 
                pack = args[0];
                break;
            case 2 :
                pack = args[0];
                model = args[1];
                break;
            default :
                Console.WriteLine("ERROR: Invalid argument construction");
                return;
        }
        try
        {
        using(RenderWindow rw = new(800,600,"Chel",pack,model))
            {
                rw.Run();
            }
        }
        catch(FileNotFoundException e)
        {
            Console.WriteLine($"ERROR: File {e.FileName} not found");
            return;
        }
        catch(Exception e)
        {
            Console.WriteLine($"ERROR: An exception has occured and the program has been killed");
            Console.WriteLine(e.Message);
        }

    }
}