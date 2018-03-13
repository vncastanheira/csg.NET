using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace csg_NET
{
    public class Entity
    {
        private Entity next;
        private Property properties;
        private Poly polys;

        public Entity GetNext { get { return next; } }
        public Property GetProperties { get { return properties; } }
        public Poly GetPolys { get { return polys; } }

        public Entity()
        {
            next = null;
            properties = null;
            polys = null;
        }

        public int GetNumberOfProperties()
        {
            Property p = properties;
            int uiCount = 0;

            while (properties != null)
            {
                p = p.GetNext;
                uiCount++;
            }

            return uiCount;
        }

        public int GetNumberOfPolys()
        {
            Poly p = polys;
            int uiCount = 0;

            while (polys != null)
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
                next = entity;
                return;
            }

            Entity ent = next;

            while (!ent.IsLast)
            {
                ent = ent.GetNext;
            }

            ent.next = entity;
        }

        public void AddProperty(Property property)
        {
            if (properties == null)
            {
                properties = property;
                return;
            }

            Property prop = properties;
            while (!prop.IsLast)
            {
                prop = prop.GetNext;
            }

            prop.SetNext(property);
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

        // TODO: file access
        public void WriteEntity(StreamWriter filestream) { }

        public bool IsLast { get { return next == null; } }
    }
}
