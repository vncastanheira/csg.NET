using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace csg_NET
{
    public class Brush
    {
        private Vector3 min, max;
        private Brush next;
        private Poly polys;

        public Brush GetNext { get { return next; } }
        public Poly GetPolys { get { return polys; } }
        public int GetNumberOfPolys { get; }
        public int GetNumberOfBrushes { get; }
        public bool IsLast { get; }

        public Brush()
        {

        }

        public Brush CopyList() { }

        public void SetNext(Brush brush) { }
        public void AddPoly(Poly poly) { }

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

        public void CalculateAABB() { }
        public bool AABBIntersect(Brush brush) { }

    }
}
