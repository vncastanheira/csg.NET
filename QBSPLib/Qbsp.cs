using System;
using Mathlib;

namespace QBSPLib
{
    public static class Qbsp
    {
        #region Constants

        public const float ON_EPSILON = 0.05f;
        public const int BOGUS_RANGE = 18000;

        #endregion

        public static Winding BaseWindingForPlane(Plane p)
        {
            int i, x;
            float max, v;

            Vector3 org = Vector3.zero;
            Vector3 vright = Vector3.zero;
            Vector3 vup = Vector3.zero;

            Winding w = new Winding();

            // find the major axis

            max = -BOGUS_RANGE;
            x = -1;
            for (i = 0; i < 3; i++)
            {
                v = Math.Abs(p.n[i]);
                if (v > max)
                {
                    x = i;
                    max = v;
                }
            }

            if (x == 1)
                throw new Exception("No axis found.");

            switch (x)
            {
                case 0:
                case 1:
                    vup[2] = 1;
                    break;
                case 2:
                    vup[0] = 1;
                    break;
            }

            v = Vector3.Dot(vup, p.n);
            vup += -v * p.n;
            vup.Normalize();

            org.Scale(p.n * p.d);           // VectorScale (p->normal, p->dist, org)
            Vector3.Cross(vright, p.n);     // CrossProduct (vup, p->normal, vright)

            vup.Scale(vup * BOGUS_RANGE);
            vright.Scale(vright * BOGUS_RANGE);

            // project a really big	axis aligned box onto the plane
            w = NewWinding(4);
            w.points[0] = org - vright;
            w.points[0] = w.points[0] + vup;

            w.points[1] = org + vright;
            w.points[1] = w.points[1] + vup;

            w.points[2] = org + vright;
            w.points[2] = w.points[2] - vup;

            w.points[3] = org - vright;
            w.points[3] = w.points[3] - vup;

            w.numpoints = 4;

            return w;
        }
    }
}
