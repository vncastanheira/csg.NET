using System;
using System.Collections.Generic;
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

        public Poly GetNext { get; }
        public Poly CopyList() { }
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

                    SplitPoly(poly, front, back);

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

        public int GetNumberOfVertices { get { return numberOfVertices; } }

        public void AddVertex(Vertex vertex);
        public void AddPoly(Poly poly) { }
        public void SetNext(Poly poly) { }

        // TODO: file write
        public void WritePoly(StreamWriter fileStream) { }

        public bool CalculatePlane() { }
        public void SortVerticesCW() { }
        public void ToLeftHanded() { }
        // TODO: make better parameters (C# can't take arrays with fixed sizes)
        public void CalculateTextureCoordinates(int texWidth, int texHeight, Plane[] textAxis, double[] texScale) { }
        public Poly SplitPoly(Poly poly, Poly front, Poly back) { }
        public eCP ClassifyPoly(Poly poly) { }

        public bool IsLast { get; }

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
