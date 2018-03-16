using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace csg_NET
{
    public class Entity
    {
        public Dictionary<string, string> Properties { get; private set; }
        public Poly Polys { get; private set; }

        public Entity()
        {
            Properties = new Dictionary<string, string>();
            Polys = null;
        }

        public int GetNumberOfProperties()
        {
            //Property p = Properties;
            //int uiCount = 0;

            //while (Properties != null)
            //{
            //    p = p.GetNext;
            //    uiCount++;
            //}

            //return uiCount;
            return Properties.Count;
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

        //public void AddProperty(Property property)
        //{
        //    if (Properties == null)
        //    {
        //        Properties = property;
        //        return;
        //    }

        //    Property prop = Properties;
        //    while (!prop.IsLast)
        //    {
        //        prop = prop.GetNext;
        //    }

        //    prop.SetNext(property);
        //}

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

        // TODO: file access
        public void WriteEntity(StreamWriter filestream) { }
    }
}
