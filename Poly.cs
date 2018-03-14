using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace csg_NET
{
    public class Poly
    {
        private Poly next;
        private int numberOfVertices;

        public enum eCP { FRONT = 0, SPLIT, BACK, ONPLANE }

        public Vertex[] verts;
        public Plane plane;
        public int TextureID;

        public Poly GetNext { get { return next; } }
        public int GetNumberOfVertices { get { return numberOfVertices; } }
        public bool IsLast { get { return next == null; } }

        public Poly()
        {
            next = null;
            verts = new Vertex[0];

        }

        public Poly CopyList()
        {
            Poly p = CopyPoly();

            if (!IsLast)
                p.AddPoly(next.CopyList());

            return p;
        }

        public Poly CopyPoly()
        {
            Poly p = new Poly();
            p.TextureID = TextureID;
            p.numberOfVertices = numberOfVertices;
            p.plane = plane;
            p.verts = new Vertex[numberOfVertices];
            Array.Copy(verts, p.verts, numberOfVertices);
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

                    return next.ClipToList(poly, clipOnPlane);

                case eCP.ONPLANE:
                    float angle = Vector3.Dot(plane.n, poly.plane.n) - 1;

                    if(angle < Mathf.Epsilon && angle > -Mathf.Epsilon)
                    {
                        if (!clipOnPlane)
                            return poly.CopyPoly();
                    }

                    if (IsLast)
                        return null;

                    return next.ClipToList(poly, clipOnPlane);

                case eCP.SPLIT:
                    Poly front = null;
                    Poly back = null;

                    SplitPoly(poly, ref front, ref back);

                    if(IsLast)
                        return front;

                    Poly backFrags = next.ClipToList(back, clipOnPlane);

                    if (backFrags == null)
                        return front;

                    if (backFrags == back)
                        return poly.CopyPoly();

                    front.AddPoly(backFrags);

                    return front;
                default:
                    return null;
            }
        }

        public void AddVertex(Vertex vertex)
        {
            Vertex[] vertices = new Vertex[numberOfVertices + 1];
            Array.Copy(verts, vertices, numberOfVertices);

            verts = vertices;
            verts[numberOfVertices] = vertex;
            numberOfVertices++;
        }

        public void AddPoly(Poly poly)
        {
            if (poly == null)
            {
                if(IsLast)
                {
                    next = poly;
                    return;
                }

                Poly p = next;
                while (!p.IsLast)
                {
                    p = p.GetNext;
                }

                p.next = poly;
            }
        }

        public void SetNext(Poly poly)
        {
            if(IsLast)
            {
                next = poly;
                return;
            }

            Poly p = poly;
            while (!p.IsLast)
            {
                p = p.GetNext;
            }

            p.SetNext(next);
            next = poly;
        }


        public bool CalculatePlane()
        {
            Vector3 centerOfMass;
            float magnitude;
            int i, j;

            if (GetNumberOfVertices < 3)
            {
                Console.WriteLine("Polygon has less than 3 vertices!");
                return false;
            }

            plane.n = Vector3.zero;
            centerOfMass = Vector3.zero;

            for ( i = 0; i < GetNumberOfVertices; i++)
            {
                j = i + 1;

                if (j >= GetNumberOfVertices) j = 0;

                plane.n.x += (verts[i].p.y - verts[j].p.y) * (verts[i].p.z + verts[j].p.z);
                plane.n.y += (verts[i].p.z - verts[j].p.z) * (verts[i].p.x + verts[j].p.x);
                plane.n.z += (verts[i].p.x - verts[j].p.x) * (verts[i].p.y + verts[j].p.y);

                centerOfMass.x += verts[i].p.x;
                centerOfMass.y += verts[i].p.y;
                centerOfMass.z += verts[i].p.z;
            }

            if (Mathf.Abs(plane.n.x) < Mathf.Epsilon 
            &&  Mathf.Abs(plane.n.y) < Mathf.Epsilon
            &&  Mathf.Abs(plane.n.z) < Mathf.Epsilon)
            {
                return false;
            }

            magnitude = Mathf.Sqrt(plane.n.x * plane.n.x + plane.n.y * plane.n.y + plane.n.z * plane.n.z);
            if (magnitude < Mathf.Epsilon)
                return false;

            plane.n /= magnitude;
            centerOfMass /= GetNumberOfVertices;
            plane.d = -(Vector3.Dot(centerOfMass, plane.n));

            return true;
        }

        public void SortVerticesCW()
        {
            // Calculate center of polygon
            Vector3 center = Vector3.zero;
            int i;

            for (i = 0; i < GetNumberOfVertices; i++)
            {
                center += verts[i].p;
            }

            center /= GetNumberOfVertices;

            // Sort vertices
            for (i = 0; i < GetNumberOfVertices -2; i++)
            {
                Vector3 a = Vector3.zero;
                Plane p = new Plane();
                float smallestAngle = -1;
                int smallest = -1;

                a = verts[i].p - center;
                a.Normalize();

                p.PointsToPlane(verts[i].p, center, center + plane.n);

                for (int j = i + 1; j < GetNumberOfVertices; j++)
                {
                    if(p.ClassifyPoints(verts[j].p) != Plane.eCP.BACK)
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
                    Console.WriteLine("Error: degenerate polygon! lock him up");
                    Console.ReadKey();
                    Application.Quit();
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
                int j = GetNumberOfVertices;

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
            for (int i = 0; i < GetNumberOfVertices; i++)
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

            for (int i = 0; i < GetNumberOfVertices; i++)
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
                        nearestU = Mathf.Floor(U);
                    }
                    else
                    {
                        nearestU = Mathf.Ceil(U);
                    }
                }

                if (doV)
                {
                    if (V > 1)
                    {
                        nearestV = Mathf.Floor(V);
                    }
                    else
                    {
                        nearestV = Mathf.Ceil(V);
                    }
                }

                for (int i = 0; i < GetNumberOfVertices; i++)
                {
                    if (doU)
                    {
                        U = verts[i].tex[0];

                        if (Mathf.Abs(U) < Mathf.Abs(nearestU))
                        {
                            if (U > 1)
                            {
                                nearestU = Mathf.Floor(U);
                            }
                            else
                            {
                                nearestU = Mathf.Ceil(U);
                            }
                        }
                    }

                    if (doV)
                    {
                        V = verts[i].tex[1];

                        if (Mathf.Abs(V) < Mathf.Abs(nearestV))
                        {
                            if(V > 1)
                            {
                                nearestV = Mathf.Floor(V);
                            }
                            else
                            {
                                nearestV = Mathf.Ceil(V);
                            }
                        }
                    }
                }

                // Normalize texture coordinates
                for (int i = 0; i < GetNumberOfVertices; i++)
                {
                    verts[i].tex[0] = verts[i].tex[0] - nearestU;
                    verts[i].tex[1] = verts[i].tex[1] - nearestV;
                }
            }
        }

        public void SplitPoly(Poly poly, ref Poly front, ref Poly back)
        {
            Plane.eCP[] cp = new Plane.eCP[poly.GetNumberOfVertices];

            // classify all points
            for (int i = 0; i < poly.GetNumberOfVertices; i++)
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

            for (int i = 0; i < poly.GetNumberOfVertices; i++)
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

                if (i == (poly.GetNumberOfVertices - 1))
                    iNext = 0;

                if (cp[i] == Plane.eCP.ONPLANE && cp[iNext] != Plane.eCP.ONPLANE)
                {
                    ignore = true;
                }
                else if(cp[iNext] == Plane.eCP.ONPLANE && cp[i] != Plane.eCP.ONPLANE)
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

            for (int i = 0; i < poly.GetNumberOfVertices; i++)
            {
                dist = Vector3.Dot(plane.n, poly.verts[i].p) + plane.d;

                if (dist > 0.001f)
                {
                    if (back)
                        return eCP.SPLIT;

                    front = true;
                }
                else if(dist < -0.001f)
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

        public static bool operator == (Poly p1, Poly p2)
        {
            if (p1.numberOfVertices == p2.numberOfVertices)
            {
                if (p1.plane.d == p2.plane.d)
                {
                    if (p1.plane.n == p2.plane.n)
                    {
                        for (int i = 0; i < p1.GetNumberOfVertices; i++)
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
