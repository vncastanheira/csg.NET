using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace csg_NET
{
    public class Property
    {
        private string name;    // Property's name (zero terminated string)
        private string value;   // Property's value (zero terminated string)
        private Property next;  // Next property in linked list

        public string GetName { get { return name; } }
        public string GetValue { get { return value; } }
        public Property GetNext { get { return next; } }

        public Property()
        {
            name = string.Empty;
            value = string.Empty;
            next = null;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetValue(string value)
        {
            this.value = value;
        }

        public void SetNext(Property property)
        {
            if (IsLast)
            {
                next = property;
                return;
            }

            // Insert the given list
            Property p = property;

            while (!p.IsLast)
            {
                p = p.GetNext;
            }

            p.SetNext(next);
            next = property;
        }

        // TODO: writing property to disk (or not)
        public void WriteProperty(StreamWriter fileStream) { }

        public bool IsLast { get { return next == null; } }

    }
}
