using System.Globalization;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;

namespace Chel.Parse;
public class StylParser
{
    private readonly string REGEX_GET_FACETS = @"\[[^\]]*\]";
    private readonly string REGEX_GET_VERTICIES = @"\(([^),]*,){3}[^),]*\)";
    private readonly string REGEX_GET_FLOATS = @"[^,()]+";
    public float ParseFloat(string floatString)
    {
        float? _toReturn = float.TryParse(
            floatString, 
            NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, 
            NumberFormatInfo.CurrentInfo, 
            out float @out
            ) ? @out : null;

        if(_toReturn is null) throw new InvalidDataException($"Failed to parse {floatString}");
        else return (float)_toReturn;
    }
    private HyperTetrahedron parseFacet(string facet) 
    {
        IEnumerable<string> _vertexStrings = Regex.Matches(facet,REGEX_GET_VERTICIES).Select(x=>x.Value);

        if(_vertexStrings.Count() != 4) throw new InvalidDataException($"Failed to parse {facet} due to invalid facet format");

        IEnumerable<IEnumerable<float>> _floats = _vertexStrings.Select(x=>Regex.Matches(x,REGEX_GET_FLOATS).Select(y=>ParseFloat(y.Value)));
        
        foreach(IEnumerable<float> v in _floats)
        {
            if(v.Count() != 4) throw new InvalidDataException(
                $"Failed to parse {facet} due to invalid vertex format" + Environment.NewLine +
                $"(Expected 4,4,4,4), got ({String.Join(',',_floats.Select(x=>x.Count()))})"
            );
        }

        return new HyperTetrahedron( 
            new Vector4(
                _floats.ElementAt(0).ElementAt(0),
                _floats.ElementAt(0).ElementAt(1),
                _floats.ElementAt(0).ElementAt(2),
                _floats.ElementAt(0).ElementAt(3)
            ),
            new Vector4(
                _floats.ElementAt(1).ElementAt(0),
                _floats.ElementAt(1).ElementAt(1),
                _floats.ElementAt(1).ElementAt(2),
                _floats.ElementAt(1).ElementAt(3)
            ),
            new Vector4(
                _floats.ElementAt(2).ElementAt(0),
                _floats.ElementAt(2).ElementAt(1),
                _floats.ElementAt(2).ElementAt(2),
                _floats.ElementAt(2).ElementAt(3)
            ),
            new Vector4(
                _floats.ElementAt(3).ElementAt(0),
                _floats.ElementAt(3).ElementAt(1),
                _floats.ElementAt(3).ElementAt(2),
                _floats.ElementAt(3).ElementAt(3)
            )
        );

    }
    private  bool isWhiteSpace(char c)
    {
        return new char[] {' ', '\n', '\r', '\t'}.Contains(c);
    }
    public HyperObject ParseText(string data)
    {
        IEnumerable<string> facetStrings = Regex.Matches(data, REGEX_GET_FACETS).Select(x=>x.Value);
        IEnumerable<HyperTetrahedron> tetrahedrons = facetStrings.Select(x=>parseFacet(x));
        return new HyperObject(tetrahedrons);
    }
    public  HyperObject ParseLines(IEnumerable<string> data)
    {
        string _decomposed = string.Join(null,data);
        return ParseText(_decomposed);
    }
    public  HyperObject ParseFile(string filePath)
    {
        string _data = File.ReadAllText(filePath);
        return ParseText(_data);
    }
}
