using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csg_NET
{
    public class Face
    {
        private Face next;

        public Plane plane;
        public Plane[] texAxis;
        public float[] texScale;
        public Texture texture;

        public Face GetNext { get { return next; } }
        public bool IsLast { get { return; } }

        public Face()
        {

        }

        public void AddFace(Face face) { }
        public void SetNext(Face face) { }

        public Poly GetPolys() { }
    }
}
