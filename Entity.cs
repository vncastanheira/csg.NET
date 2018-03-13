using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace csg_NET
{
    public class Entity
    {
        private Entity pNext;
        private Property pProperties;
        private Poly pPolys;

        public Entity GetNext { get { return pNext; } }
        public Property GetProperties { get { return pProperties; } }
        public Polys GetPolys { get { return pPolys; } }

        public Entity()
        {
            pNext = null;
            pProperties = null;
            pPolys = null;
        }

        public int GetNumberOfProperties()
        {
            Property p = pProperties;
            int uiCount = 0;

            while (pProperties != null)
            {
                p = p.GetNext;
                uiCount++;
            }

            return uiCount;
        }

        public int GetNumberOfPolys()
        {
            Poly p = pPolys;
            int uiCount = 0;

            while (pPolys != null)
            {
                p = p.GetNext;
                uiCount++;
            }

            return uiCount;
        }

        public void AddEntity(Entity entity)
        {
            if(IsLast)
            {
                pNext = entity;
                return;
            }

            Entity ent = pNext;

            while (!ent.IsLast)
            {
                ent = ent.GetNext;
            }

            ent.pNext = entity;
        }

        public void AddProperty(Property property)
        {
            if (pProperties == null)
            {
                pProperties = property;
                return;
            }

            Property prop = pProperties;
            while (!prop.IsLast)
            {
                prop = prop.GetNext;
            }

            prop.SetNext(property);
        }

        public void AddPoly(Poly poly)
        {
            if (pPolys == null)
            {
                pPolys = poly;
                return;
            }

            Poly p = pPolys;
            while (!p.IsLast)
            {
                p = p.GetNext;
            }

            p.SetNext(poly);
        }

        // TODO: file access
        public void WriteEntity(StreamWriter filestream) { }

        public bool IsLast { get { return pNext == null; } }
    }
}
