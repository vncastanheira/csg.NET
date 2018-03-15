using System;

namespace csg_NET
{
    public class Brush
    {
        private Vector3 min, max;
        private Brush next;
        private Poly polys;

        public Brush GetNext { get { return next; } }
        public Poly GetPolys { get { return polys; } }

        public int GetNumberOfPolys
        {
            get
            {
                Poly poly = polys;
                int count = 0;
                while (poly != null)
                {
                    poly = poly.GetNext;
                    count++;
                }
                return count;
            }
        }

        public int GetNumberOfBrushes
        {
            get
            {
                Brush brush = next;
                int count = 1;
                while (brush != null)
                {
                    brush = brush.GetNext;
                    count++;
                }
                return count;
            }
        }

        public bool IsLast { get { return next == null; } }

        public Brush()
        {
            next = null;
            polys = null;
        }

        public Brush CopyList()
        {
            Brush brush = new Brush();
            brush.max = max;
            brush.min = min;

            brush.polys = polys.CopyList();

            if (!IsLast)
            {
                brush.SetNext(next.CopyList());
            }

            return brush;
        }

        public void SetNext(Brush brush)
        {
            if(IsLast)
            {
                next = brush;
                return;
            }

            if(brush == null)
            {
                next = null;
            }
            else
            {
                Brush b = brush;
                while (!b.IsLast)
                {
                    b = b.GetNext;
                }

                b.SetNext(next);
                next = brush;
            }
        }

        public void AddPoly(Poly poly)
        {
            if (polys == null)
            {
                polys = poly;
                return;
            }

            Poly p = polys;
            while (!p.IsLast)
            {
                p = p.GetNext;
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

                    brush = brush.GetNext;
                }

                clip = clip.GetNext;
            }

            clip = clippedList;

            while (clip != null)
            {
                if (clip.GetNumberOfPolys != 0)
                {
                    // Extract brushes left over polygons and add them to the list
                    Poly p = clip.GetPolys.CopyList();

                    if (polyList == null)
                    {
                        polyList = p;
                    }
                    else
                    {
                        polyList.AddPoly(p);
                    }

                    clip = clip.GetNext;
                }
                else
                {
                    // Brush has no polygons and should be deleted
                    if (clip == clippedList)
                    {
                        clip = clippedList.GetNext;
                        clippedList.SetNext(null);
                        clippedList = clip;
                    }
                    else
                    {
                        Brush temp = clippedList;
                        while (temp != null)
                        {
                            if (temp.GetNext == clip)
                                break;

                            temp = temp.GetNext;
                        }

                        temp.next = clip.GetNext;
                        clip.SetNext(null);
                        clip = temp.GetNext;
                    }
                }
            }

            return polyList;
        }

        public void ClipToBrush(Brush brush, bool clipOnPlane)
        {
            Poly polyList = null;
            Poly p = polys;

            for (int i = 0; i < GetNumberOfPolys; i++)
            {
                Poly clippedPoly = brush.GetPolys.ClipToList(p, clipOnPlane);

                if (polyList == null)
                {
                    polyList = clippedPoly;
                }
                else
                {
                    polyList.AddPoly(clippedPoly);
                }

                p = p.GetNext;
            }

            polys = polyList;
        }

        public void CalculateAABB()
        {
            min = polys.verts[0].p;
            max = polys.verts[0].p;

            Poly p = polys;

            for (int i = 0; i < GetNumberOfPolys; i++)
            {
                for (int j = 0; j < p.GetNumberOfVertices; j++)
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

                p = p.GetNext;
            }
        }

        public bool AABBIntersect(Brush brush)
        {
            if((min.x > brush.max.x) || (brush.min.x > max.x))
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
