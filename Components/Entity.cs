using System.Collections.Generic;

namespace csg_NET
{
    public class Entity
    {
        public Dictionary<string, string> Properties { get; private set; }
        public Poly Polys { get; private set; }

        public int NumberOfProperties { get { return Properties.Count; } }

        public Entity()
        {
            Properties = new Dictionary<string, string>();
            Polys = null;
        }


        public int GetNumberOfPolys()
        {
            Poly p = Polys;
            int uiCount = 0;

            while (p != null)
            {
                p = p.Next;
                uiCount++;
            }

            return uiCount;
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
    }
}
