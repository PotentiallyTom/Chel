using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;

namespace Chel;
public struct HyperTetrahedron
{
    public Vector4 A {get;set;}
    public Vector4 B {get;set;}
    public Vector4 C {get;set;}
    public Vector4 D {get;set;}
    public HyperTetrahedron(Vector4 a, Vector4 b, Vector4 c, Vector4 d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if(obj is null) return false;
        HyperTetrahedron that = (HyperTetrahedron)obj;
        return A == that.A && B == that.B && C == that.C && D == that.D;
    }
    public static bool operator ==(HyperTetrahedron v1, HyperTetrahedron v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator !=(HyperTetrahedron v1, HyperTetrahedron v2)
    {
        return !(v1==v2);
    }
    private static Vector4 planeIntersection(Vector4 a, Vector4 b, float w)
    {
        float downRatio = w / (a.W - b.W);
        return new Vector4(
            a.X - (a.X - b.X) * downRatio,
            a.Y - (a.Y - b.Y) * downRatio,
            a.Z - (a.Z - b.Z) * downRatio,
            w
        );
    }
    public IEnumerable<Vector4> Slice(float w)
    {
        List<Vector4> above = new List<Vector4>(4);
        List<Vector4> below = new List<Vector4>(4);

        if(A.W > w) above.Add(A); else below.Add(A);
        if(B.W > w) above.Add(B); else below.Add(B);
        if(C.W > w) above.Add(C); else below.Add(C);
        if(D.W > w) above.Add(D); else below.Add(D);

        switch(above.Count)
        {
            case 0 : 
                return Array.Empty<Vector4>();
            case 1 :
                return new Vector4[] {
                    planeIntersection(above[0],below[0],w),
                    planeIntersection(above[0],below[1],w),
                    planeIntersection(above[0],below[2],w),
                };
            case 2 : 
                Vector4 a = planeIntersection(above[0],below[0],w);
                Vector4 b = planeIntersection(above[0],below[1],w);
                Vector4 c = planeIntersection(above[1],below[0],w);
                Vector4 d = planeIntersection(above[1],below[1],w);
                return new Vector4[] {
                    a,b,c,
                    a,d,c,
                };
            case 3 :
                return new Vector4[] {
                    planeIntersection(above[0],below[0],w),
                    planeIntersection(above[1],below[0],w),
                    planeIntersection(above[2],below[0],w),
                };
            case 4 : 
                return Array.Empty<Vector4>();
        }
        throw new UnreachableException();
    }
    public float[] AsArray()
    {
        return new float[] {
            A.X, A.Y, A.Z, A.W,
            B.X, B.Y, B.Z, B.W,
            C.X, C.Y, C.Z, C.W,
            D.X, D.Y, D.Z, D.W
        };
    }
}
public class HyperObject
{
    public IEnumerable<HyperTetrahedron> Facets {get;}
    public HyperObject(IEnumerable<HyperTetrahedron> facets)
    {
        Facets = facets;
    }
    public float[] AsArray()
    {
        return Facets.SelectMany(x=>x.AsArray()).ToArray();
    }
}