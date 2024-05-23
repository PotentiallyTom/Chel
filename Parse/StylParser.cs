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
            NumberStyles.AllowExponent 
            | NumberStyles.AllowLeadingSign 
            | NumberStyles.AllowDecimalPoint, 
            NumberFormatInfo.CurrentInfo, 
            out float @out
            ) ? @out : null;

        if(_toReturn is null) 
            throw new InvalidDataException($"Failed to parse {floatString}");
        else return (float)_toReturn;
    }
    private HyperTetrahedron parseFacet(string facet) 
    {
        IEnumerable<string> _vertexStrings = Regex.Matches(facet, REGEX_GET_VERTICIES).Select(x=>x.Value);

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
    public HyperObject ParseText(string data, bool doClosureCheck = false)
    {
        IEnumerable<string> facetStrings = Regex.Matches(data, REGEX_GET_FACETS).Select(x=>x.Value);
        IEnumerable<HyperTetrahedron> tetrahedrons = facetStrings.Select(parseFacet);
        if(doClosureCheck && !Is4Regular(tetrahedrons)) 
        {
            throw new InvalidDataException("Parsed Object is not closed");
        }
        return new HyperObject(tetrahedrons);
    }
    public HyperObject ParseFile(string filePath, bool doClosureCheck = false)
    {
        string _data = File.ReadAllText(filePath);
        return ParseText(_data, doClosureCheck);
    }

    private static bool DoesShareFaceAndNotSame(HyperTetrahedron t1, HyperTetrahedron t2)
    {
        IEnumerable<Vector4> t1Vecs = new Vector4[] {t1.A, t1.B, t1.C, t1.D};
        IEnumerable<Vector4> t2Vecs = new Vector4[] {t2.A, t2.B, t2.C, t2.D};
        IEnumerable<(Vector4 a,Vector4 b)> CartesianProduct = 
            from a in t1Vecs
            from b in t2Vecs
            select (a, b);

        return CartesianProduct.Count(x=>x.a == x.b) == 3;        
    }
    public bool Is4Regular(IEnumerable<HyperTetrahedron> facets)
    {
        List<int> CountOf = new();
        foreach(HyperTetrahedron f in facets)
        {
            CountOf.Add(facets.Count(x=>DoesShareFaceAndNotSame(x,f)));
        }
        return CountOf.Count(x=>x!=4) == 0;
    }
}
