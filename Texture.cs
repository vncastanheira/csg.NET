using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csg_NET
{
    // TODO: write this class
    public class Texture
    {
        Texture next;
        int width;
        int height;

        public enum eGT { GT_FOUND = 0, GT_LOADED, GT_ERROR };
        public int ID;
        public string name;

        public Texture GetNext { get { return next; } }
        public int GetHeight { get { return height; } }
        public int GetWidth { get { return width; } }
        public bool IsLast { get { return next == null; } }

        public Texture()
        {
            name = string.Empty;
            next = null;
            height = 0;
            width = 0;
        }

        /// <summary>
        /// I'm not gonna load WADs
        /// </summary>
        public Texture GetTexture() { return this; }

        public void SetNext(Texture texture)
        {
            if (IsLast)
            {
                next = texture;
                return;
            }

            // Insert the given list
            if (texture != null)
            {
                Texture tex = texture;
                while (!tex.IsLast)
                {
                    tex = tex.GetNext;
                }

                tex.SetNext(next);
            }

            next = texture;
        }
    }
}
