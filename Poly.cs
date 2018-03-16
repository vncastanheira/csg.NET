using OpenTK.Graphics;
using System;
using System.IO;

namespace csg_NET
{
    public class Poly
    {
        public int NumberOfVertices { get; private set; }
        public Poly Next { get; private set; }
        public enum eCP { FRONT = 0, SPLIT, BACK, ONPLANE }

        public Vertex[] verts;
        public Plane plane;
        public int TextureID;
        public Color4 Color;

        public bool IsLast { get { return Next == null; } }

        public Poly()
        {
            Next = null;
            verts = new Vertex[0];

            var rand = new Random();
            float r = rand.Next(0, 100) / 100f;
            float g = rand.Next(0, 100) / 100f;
            float b = rand.Next(0, 100) / 100f;
            Color = new Color4(r, g, b, 1.0f);
        }

        public Poly CopyList()
        {
            Poly p = CopyPoly();

            if (!IsLast)
                p.AddPoly(Next.CopyList());

            return p;
        }

        public Poly CopyPoly()
        {
            Poly p = new Poly();
            p.TextureID = TextureID;
            p.NumberOfVertices = NumberOfVertices;
            p.plane = plane;
            p.verts = new Vertex[NumberOfVertices];
            Array.Copy(verts, p.verts, NumberOfVertices);
            return p;
        }

        public Poly ClipToList(Poly poly, bool clipOnPlane)
        {
            switch (ClassifyPoly(poly))
            {
                case eCP.FRONT:
                    return poly.CopyPoly();

                case eCP.BACK:
                    if (IsLast) return null;

                    return Next.ClipToList(poly, clipOnPlane);

                case eCP.ONPLANE:
                    float angle = Vector3.Dot(plane.n, poly.plane.n) - 1;

                    if (angle < Mathf.EPSILON && angle > -Mathf.EPSILON)
                    {
                        if (!clipOnPlane)
                            return poly.CopyPoly();
                    }

                    if (IsLast)
                        return null;

                    return Next.ClipToList(poly, clipOnPlane);

                case eCP.SPLIT:
                    Poly front = null;
                    Poly back = null;

                    SplitPoly(poly, ref front, ref back);

                    if (IsLast)
                        return front;

                    Poly backFrags = Next.ClipToList(back, clipOnPlane);

                    if (backFrags == null)
                        return front;

                    if (backFrags == back)
                        return poly.CopyPoly();

                    front.AddPoly(backFrags);

                    return front;
            }

            return null;
        }

        public void AddVertex(Vertex vertex)
        {
            Vertex[] vertices = new Vertex[NumberOfVertices + 1];
            Array.Copy(verts, vertices, NumberOfVertices);

            verts = vertices;
            verts[NumberOfVertices] = vertex;
            NumberOfVertices++;
        }

        public void AddPoly(Poly pPoly_)
        {
            if (pPoly_ != null)
            {
                if (IsLast)
                {
                    Next = pPoly_;
                    return;
                }

                Poly p = Next;
                while (!p.IsLast)
                {
                    p = p.Next;
                }

                p.Next = pPoly_;
            }
        }

        public void SetNext(Poly poly)
        {
            if (IsLast)
            {
                Next = poly;
                return;
            }

            Poly p = poly;
            while (!p.IsLast)
            {
                p = p.Next;
            }

            p.SetNext(Next);
            Next = poly;
        }

        public bool CalculatePlane()
        {
            Vector3 centerOfMass;
            float magnitude;
            int i, j;

            if (NumberOfVertices < 3)
            {
                Console.WriteLine("Polygon has less than 3 vertices!");
                return false;
            }

            plane.n = Vector3.zero;
            centerOfMass = Vector3.zero;

            for (i = 0; i < NumberOfVertices; i++)
            {
                j = i + 1;

                if (j >= NumberOfVertices) j = 0;

                plane.n.x += (verts[i].p.y - verts[j].p.y) * (verts[i].p.z + verts[j].p.z);
                plane.n.y += (verts[i].p.z - verts[j].p.z) * (verts[i].p.x + verts[j].p.x);
                plane.n.z += (verts[i].p.x - verts[j].p.x) * (verts[i].p.y + verts[j].p.y);

                centerOfMass.x += verts[i].p.x;
                centerOfMass.y += verts[i].p.y;
                centerOfMass.z += verts[i].p.z;
            }

            if (Math.Abs(plane.n.x) < Mathf.EPSILON
            && Math.Abs(plane.n.y) < Mathf.EPSILON
            && Math.Abs(plane.n.z) < Mathf.EPSILON)
            {
                return false;
            }

            magnitude = (float)Math.Sqrt(plane.n.x * plane.n.x + plane.n.y * plane.n.y + plane.n.z * plane.n.z);
            if (magnitude < Mathf.EPSILON)
                return false;

            plane.n /= magnitude;
            centerOfMass /= NumberOfVertices;
            plane.d = -(Vector3.Dot(centerOfMass, plane.n));

            return true;
        }

        public void SortVerticesCW()
        {
            // Calculate center of polygon
            Vector3 center = Vector3.zero;
            int i;

            for (i = 0; i < NumberOfVertices; i++)
            {
                center += verts[i].p;
            }

            center /= NumberOfVertices;

            // Sort vertices
            for (i = 0; i < NumberOfVertices - 2; i++)
            {
                Vector3 a = Vector3.zero;
                Plane p = new Plane();
                float smallestAngle = -1;
                int smallest = -1;

                a = verts[i].p - center;
                a.Normalize();

                p.PointsToPlane(verts[i].p, center, center + plane.n);

                for (int j = i + 1; j < NumberOfVertices; j++)
                {
                    if (p.ClassifyPoints(verts[j].p) != Plane.eCP.BACK)
                    {
                        Vector3 b = verts[j].p - center;
                        b.Normalize();

                        float angle = Vector3.Dot(a, b);
                        if (angle > smallestAngle)
                        {
                            smallestAngle = angle;
                            smallest = j;
                        }
                    }
                }

                if (smallest == -1)
                {
                    Console.WriteLine("Error: degenerate polygon!");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                Vertex t = verts[smallest];
                verts[smallest] = verts[i + 1];
                verts[i + 1] = t;
            }

            // Check if vertex order needs to be reversed for back-facing polygon
            Plane oldPlane = plane;

            CalculatePlane();

            if (Vector3.Dot(plane.n, oldPlane.n) < 0)
            {
                int j = NumberOfVertices;

                for (int index = 0; index < j; index++)
                {
                    Vertex v = verts[index];
                    verts[index] = verts[j - index - 1];
                    verts[j - index - 1] = v;
                }

            }
        }

        // declared function without a body, go figure
        //public void ToLeftHanded() { }

        public void CalculateTextureCoordinates(int texWidth, int texHeight,
            Plane texAxisU, Plane texAxisV,
            float texScaleU, float texScaleV)
        {

            // Calculate texture coordinates
            for (int i = 0; i < NumberOfVertices; i++)
            {
                float U, V;

                U = Vector3.Dot(texAxisU.n, verts[i].p);
                U = U / texWidth / texScaleU;
                U = U + (texAxisU.d / texWidth);

                V = Vector3.Dot(texAxisV.n, verts[i].p);
                V = V / texHeight / texScaleV;
                V = V + (texAxisV.d / texHeight);

                verts[i].tex = new float[] { U, V };
            }

            // Check which axis should be normalized
            bool doU = true, doV = true;

            for (int i = 0; i < NumberOfVertices; i++)
            {
                if (verts[i].tex[0] < 1 && verts[i].tex[0] > -1)
                {
                    doU = false;
                }

                if (verts[i].tex[1] < 1 && verts[i].tex[1] > -1)
                {
                    doV = false;
                }
            }

            // Calculate coordinate nearest to 0
            if (doU || doV)
            {
                float nearestU = 0;
                float U = verts[0].tex[0];

                float nearestV = 0;
                float V = verts[0].tex[1];

                if (doU)
                {
                    if (U > 1)
                    {
                        nearestU = (float)Math.Floor(U);
                    }
                    else
                    {
                        nearestU = (float)Math.Ceiling(U);
                    }
                }

                if (doV)
                {
                    if (V > 1)
                    {
                        nearestV = (float)Math.Floor(V);
                    }
                    else
                    {
                        nearestV = (float)Math.Ceiling(V);
                    }
                }

                for (int i = 0; i < NumberOfVertices; i++)
                {
                    if (doU)
                    {
                        U = verts[i].tex[0];

                        if (Math.Abs(U) < Math.Abs(nearestU))
                        {
                            if (U > 1)
                            {
                                nearestU = (float)Math.Floor(U);
                            }
                            else
                            {
                                nearestU = (float)Math.Ceiling(U);
                            }
                        }
                    }

                    if (doV)
                    {
                        V = verts[i].tex[1];

                        if (Math.Abs(V) < Math.Abs(nearestV))
                        {
                            if (V > 1)
                            {
                                nearestV = (float)Math.Floor(V);
                            }
                            else
                            {
                                nearestV = (float)Math.Ceiling(V);
                            }
                        }
                    }
                }

                // Normalize texture coordinates
                for (int i = 0; i < NumberOfVertices; i++)
                {
                    verts[i].tex[0] = verts[i].tex[0] - nearestU;
                    verts[i].tex[1] = verts[i].tex[1] - nearestV;
                }
            }
        }

        public void SplitPoly(Poly poly, ref Poly front, ref Poly back)
        {
            Plane.eCP[] cp = new Plane.eCP[poly.NumberOfVertices];

            // classify all points
            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                cp[i] = plane.ClassifyPoints(poly.verts[i].p);
            }

            // builds fragments
            Poly newFront = new Poly();
            Poly newBack = new Poly();

            newFront.TextureID = poly.TextureID;
            newBack.TextureID = poly.TextureID;
            newFront.plane = poly.plane;
            newBack.plane = poly.plane;

            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                // Add point to appropriate list
                switch (cp[i])
                {
                    case Plane.eCP.FRONT:
                        newFront.AddVertex(poly.verts[i]);
                        break;
                    case Plane.eCP.BACK:
                        newBack.AddVertex(poly.verts[i]);
                        break;
                    case Plane.eCP.ONPLANE:
                        newFront.AddVertex(poly.verts[i]);
                        newBack.AddVertex(poly.verts[i]);
                        break;
                }

                // Check if edges should be split
                int iNext = i + 1;
                bool ignore = false;

                if (i == (poly.NumberOfVertices - 1))
                    iNext = 0;

                if (cp[i] == Plane.eCP.ONPLANE && cp[iNext] != Plane.eCP.ONPLANE)
                {
                    ignore = true;
                }
                else if (cp[iNext] == Plane.eCP.ONPLANE && cp[i] != Plane.eCP.ONPLANE)
                {
                    ignore = true;
                }

                if (!ignore && (cp[i] != cp[iNext]))
                {
                    Vertex v = new Vertex();    // New vertex created by splitting
                    float p = 0f;               // Percentage between the two points

                    plane.GetIntersection(poly.verts[i].p, poly.verts[iNext].p, v.p, p);

                    v.tex[0] = poly.verts[iNext].tex[0] - poly.verts[i].tex[0];
                    v.tex[1] = poly.verts[iNext].tex[1] - poly.verts[i].tex[1];

                    v.tex[0] = poly.verts[i].tex[0] + (p * v.tex[0]);
                    v.tex[1] = poly.verts[i].tex[1] + (p * v.tex[1]);

                    newFront.AddVertex(v);
                    newBack.AddVertex(v);
                }
            }

            newFront.CalculatePlane();
            newBack.CalculatePlane();

            front = newFront;
            back = newBack;
        }

        public eCP ClassifyPoly(Poly poly)
        {
            bool front = false, back = false;
            float dist;

            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                dist = Vector3.Dot(plane.n, poly.verts[i].p) + plane.d;

                if (dist > 0.001f)
                {
                    if (back)
                        return eCP.SPLIT;

                    front = true;
                }
                else if (dist < -0.001f)
                {
                    if (front)
                        return eCP.SPLIT;

                    back = true;
                }
            }

            if (front) return eCP.FRONT;
            else if (back) return eCP.BACK;

            return eCP.ONPLANE;
        }

        // TODO: file write
        public void WritePoly(StreamWriter fileStream) { }

        public static bool operator ==(Poly p1, Poly p2)
        {
            if (ReferenceEquals(p1, null))
                return ReferenceEquals(p2, null);

            if (ReferenceEquals(p2, null))
                return ReferenceEquals(p1, null);

            if (p1.NumberOfVertices == p2.NumberOfVertices)
            {
                if (p1.plane.d == p2.plane.d)
                {
                    if (p1.plane.n == p2.plane.n)
                    {
                        for (int i = 0; i < p1.NumberOfVertices; i++)
                        {
                            if (p1.verts[i].p == p2.verts[i].p)
                            {
                                if (p1.verts[i].tex[0] != p2.verts[i].tex[0])
                                {
                                    return false;
                                }

                                if (p1.verts[i].tex[1] != p2.verts[i].tex[1])
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }

                        if (p1.TextureID == p2.TextureID)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool operator !=(Poly p1, Poly p2)
        {
            return !(p1 == p2);
        }
    }
}
