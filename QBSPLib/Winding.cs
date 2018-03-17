using Mathlib;

namespace QBSPLib
{
    public class Winding
    {
        public int numpoints;
        public Vector3[] points;

        public Winding()
        {
            numpoints = 0;
            points = new Vector3[8];
        }
    }
}
