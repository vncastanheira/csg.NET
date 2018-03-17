using System;

namespace csg_NET
{
    public class Brush
    {
        private Vector3 min, max;

        public Brush Next { get; private set; }
        public Poly Polys { get; private set; }

        public int GetNumberOfPolys
        {
            get
            {
                Poly poly = Polys;
                int count = 0;
                while (poly != null)
                {
                    poly = poly.Next;
                    count++;
                }
                return count;
            }
        }

        public int GetNumberOfBrushes
        {
            get
            {
                Brush brush = Next;
                int count = 1;
                while (brush != null)
                {
                    brush = brush.Next;
                    count++;
                }
                return count;
            }
        }

        public bool IsLast { get { return Next == null; } }

        public Brush()
        {
            Next = null;
            Polys = null;
        }

        public Brush CopyList()
        {
            Brush brush = new Brush();
            brush.max = max;
            brush.min = min;

            brush.Polys = Polys.CopyList();

            if (!IsLast)
            {
                brush.SetNext(Next.CopyList());
            }

            return brush;
        }

        public void SetNext(Brush brush)
        {
            if (IsLast)
            {
                Next = brush;
                return;
            }

            if (brush == null)
            {
                Next = null;
            }
            else
            {
                Brush b = brush;
                while (!b.IsLast)
                {
                    b = b.Next;
                }

                b.SetNext(Next);
                Next = brush;
            }
        }

        public void AddPoly(Poly poly)
        {
            if (Polys == null)
            {
                Polys = poly;
                return;
            }

            Poly p = Polys;
            while (!p.IsLast)
            {
                p = p.Next;
            }

            p.SetNext(poly);
        }

        public Poly MergeList()
        {
            Brush clippedList = CopyList();
            Brush clip = clippedList;
            Brush brush = null;
            Poly polyList = null;

            bool clipOnPlane = false;


            for (int i = 0; i < GetNumberOfBrushes; i++)
            {
                brush = this;
                clipOnPlane = false;

                for (int j = 0; j < GetNumberOfBrushes; j++)
                {
                    if (i == j)
                    {
                        clipOnPlane = true;
                    }
                    else
                    {
                        if (clip.AABBIntersect(brush))
                        {
                            clip.ClipToBrush(brush, clipOnPlane);
                        }
                    }

                    brush = brush.Next;
                }

                clip = clip.Next;
            }

            clip = clippedList;

            while (clip != null)
            {
                if (clip.GetNumberOfPolys != 0)
                {
                    // Extract brushes left over polygons and add them to the list
                    Poly p = clip.Polys.CopyList();

                    if (polyList == null)
                    {
                        polyList = p;
                    }
                    else
                    {
                        polyList.AddPoly(p);
                    }

                    clip = clip.Next;
                }
                else
                {
                    // Brush has no polygons and should be deleted
                    if (clip == clippedList)
                    {
                        clip = clippedList.Next;
                        clippedList.SetNext(null);
                        clippedList = clip;
                    }
                    else
                    {
                        Brush temp = clippedList;
                        while (temp != null)
                        {
                            if (temp.Next == clip)
                                break;

                            temp = temp.Next;
                        }

                        temp.Next = clip.Next;
                        clip.SetNext(null);
                        clip = temp.Next;
                    }
                }
            }

            return polyList;
        }

        public void ClipToBrush(Brush brush, bool clipOnPlane)
        {
            Poly polyList = null;
            Poly p = Polys;

            for (int i = 0; i < GetNumberOfPolys; i++)
            {
                Poly clippedPoly = brush.Polys.ClipToList(p, clipOnPlane);

                if (polyList == null)
                {
                    polyList = clippedPoly;
                }
                else
                {
                    polyList.AddPoly(clippedPoly);
                }

                p = p.Next;
            }

            Polys = polyList;
        }

        public void CalculateAABB()
        {
            min = Polys.verts[0].p;
            max = Polys.verts[0].p;

            Poly p = Polys;

            for (int i = 0; i < GetNumberOfPolys; i++)
            {
                for (int j = 0; j < p.NumberOfVertices; j++)
                {
                    // Calculate min
                    if (p.verts[j].p.x < min.x)
                    {
                        min.x = p.verts[j].p.x;
                    }

                    if (p.verts[j].p.y < min.y)
                    {
                        min.y = p.verts[j].p.y;
                    }

                    if (p.verts[j].p.z < min.z)
                    {
                        min.z = p.verts[j].p.z;
                    }

                    // Calculate max
                    if (p.verts[j].p.x > max.x)
                    {
                        max.x = p.verts[j].p.x;
                    }

                    if (p.verts[j].p.y > max.y)
                    {
                        max.y = p.verts[j].p.y;
                    }

                    if (p.verts[j].p.z > max.z)
                    {
                        max.z = p.verts[j].p.z;
                    }
                }

                p = p.Next;
            }
        }

        public bool AABBIntersect(Brush brush)
        {
            if ((min.x > brush.max.x) || (brush.min.x > max.x))
            {
                return false;
            }

            if ((min.y > brush.max.y) || (brush.min.y > max.y))
            {
                return false;
            }

            if ((min.z > brush.max.z) || (brush.min.z > max.z))
            {
                return false;
            }

            return true;
        }
    }
}
