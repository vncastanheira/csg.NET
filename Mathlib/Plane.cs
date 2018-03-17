using System;

namespace Mathlib
{
    public class Plane
    {
        public Vector3 n;   // normal
        public float d;    // distance

        public enum eCP { FRONT = 0, BACK, ONPLANE }

        public Plane()
        {
            n = Vector3.zero;
            d = 0;
        }

        public Plane(Vector3 n, float d)
        {
            this.n = n;
            this.d = d;
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            PointsToPlane(a, b, c);
        }

        public void PointsToPlane(Vector3 a, Vector3 b, Vector3 c)
        {
            n = Vector3.Cross((c - b), (a - b));
            n.Normalize();

            d = Vector3.Dot(-n, a);
        }

        public float DistanceToPlane(Vector3 v)
        {
            return Vector3.Dot(n, v) + d;
        }

        public eCP ClassifyPoints(Vector3 v)
        {
            float Distance = DistanceToPlane(v);

            if (Distance > Mathf.EPSILON)
            {
                return eCP.FRONT;
            }
            else if (Distance < -Mathf.EPSILON)
            {
                return eCP.BACK;
            }

            return eCP.ONPLANE;
        }

        public bool GetIntersection(Plane a, Plane b, ref Vector3 v)
        {
            float denom = Vector3.Dot(n, Vector3.Cross(a.n, b.n));

            if (Math.Abs(denom) < Mathf.EPSILON)
            {
                return false;
            }

            v = (-d * Vector3.Cross(a.n, b.n)) - (a.d * Vector3.Cross(b.n, n)) - (b.d * Vector3.Cross(n, a.n));
            v /= denom;

            return true;
        }

        public bool GetIntersection(Vector3 start, Vector3 end, Vector3 intersection, float percentage)
        {
            Vector3 direction = end - start;
            float num, denom;

            direction.Normalize();

            denom = Vector3.Dot(n, direction);

            if (Math.Abs(denom) < Mathf.EPSILON)
            {
                return false;
            }

            num = -DistanceToPlane(start);
            percentage = num / denom;
            intersection = start + (direction * percentage);
            percentage = percentage / (end - start).magnitude;

            return true;
        }
    }
}
