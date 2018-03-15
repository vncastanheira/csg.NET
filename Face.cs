using System;

namespace csg_NET
{
    public class Face
    {
        private Face next;

        public Plane plane;
        public Plane[] texAxis;
        public float[] texScale;
        public float texRotation;
        public Texture texture;

        public Face GetNext { get { return next; } }
        public bool IsLast { get { return next == null; } }

        public Face()
        {
            next = null;
            texture = new Texture();
        }

        public void AddFace(Face face)
        {
            if (IsLast)
            {
                next = face;
                return;
            }

            Face f = next;
            while (!f.IsLast)
            {
                f = f.GetNext;
            }

            f.next = face;
        }

        public void SetNext(Face face)
        {
            if (IsLast)
            {
                next = face;
                return;
            }

            // Insert the give list
            Face f = face;
            while (!f.IsLast)
            {
                f = f.GetNext;
            }

            f.SetNext(next);
            next = face;
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
                face = face.GetNext;
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
                    lfi = face.GetNext;
                }
                else if (c == nFaces - 2)
                {
                    lfj = face.GetNext;
                }
                else if (c == nFaces - 1)
                {
                    lfk = face.GetNext;
                }

                face = face.GetNext;
            }

            // Loop through faces and create polygons
            Poly pi = polyList;

            for (Face fi = this; fi != lfi; fi = fi.GetNext)
            {
                Poly pj = pi.GetNext;

                for (Face fj = fi.GetNext; fj != lfj; fj = fj.GetNext)
                {
                    Poly pk = pj.GetNext;

                    for (Face fk = fj.GetNext; fk != lfk; fk = fk.GetNext)
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

                                Vertex v = new Vertex { p = p };

                                pi.AddVertex(v);
                                pj.AddVertex(v);
                                pk.AddVertex(v);

                                f = f.GetNext;
                            }
                        }

                        pk = pk.GetNext;
                    }

                    pj = pj.GetNext;
                }

                pi = pi.GetNext;
            }

            return polyList;
        }
    }
}
