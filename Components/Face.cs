using System;
using Mathlib;

namespace csg_NET
{
    public class Face
    {
        public Plane plane;
        public Plane[] texAxis;
        public float[] texScale;
        public float texRotation;
        public Texture texture;

        public Face Next { get; private set; }
        public bool IsLast { get { return Next == null; } }

        public Face()
        {
            Next = null;
            texture = new Texture();
        }

        public void AddFace(Face face)
        {
            if (IsLast)
            {
                Next = face;
                return;
            }

            Face f = Next;
            while (!f.IsLast)
            {
                f = f.Next;
            }

            f.Next = face;
        }

        public void SetNext(Face face)
        {
            if (IsLast)
            {
                Next = face;
                return;
            }

            // Insert the give list
            Face f = face;
            while (!f.IsLast)
            {
                f = f.Next;
            }

            f.SetNext(Next);
            Next = face;
        }

        /// <summary>
        /// Create the polygons from the faces
        /// </summary>
        public Poly GetPolys()
        {
            int nFaces = 0;
            Face face = this;

            while (face != null)
            {
                face = face.Next;
                nFaces++;
            }

            Poly polyList = null;
            Face lfi = null;
            Face lfj = null;
            Face lfk = null;

            // Create polygons
            face = this;

            for (int c = 0; c < nFaces; c++)
            {
                if (polyList == null)
                {
                    polyList = new Poly();
                }
                else
                {
                    polyList.AddPoly(new Poly());
                }

                if (c == nFaces - 3)
                {
                    lfi = face.Next;
                }
                else if (c == nFaces - 2)
                {
                    lfj = face.Next;
                }
                else if (c == nFaces - 1)
                {
                    lfk = face.Next;
                }

                face = face.Next;
            }

            // Loop through faces and create polygons
            Poly pi = polyList;

            for (Face fi = this; fi != lfi; fi = fi.Next)
            {
                Poly pj = pi.Next;

                for (Face fj = fi.Next; fj != lfj; fj = fj.Next)
                {
                    Poly pk = pj.Next;

                    for (Face fk = fj.Next; fk != lfk; fk = fk.Next)
                    {
                        Vector3 p = Vector3.zero;

                        if (fi.plane.GetIntersection(fj.plane, fk.plane, ref p))
                        {
                            Face f = this;

                            while (true)
                            {
                                if (f.plane.ClassifyPoints(p) == Plane.eCP.FRONT)
                                {
                                    break;
                                }

                                if (!f.IsLast)
                                {
                                    Vertex v = new Vertex { p = p };

                                    pi.AddVertex(v);
                                    pj.AddVertex(v);
                                    pk.AddVertex(v);

                                    break;
                                }

                                f = f.Next;
                            }
                        }

                        pk = pk.Next;
                    }

                    pj = pj.Next;
                }

                pi = pi.Next;
            }

            return polyList;
        }
    }
}
