using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultEcs.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ComponentAttribute : Attribute
    {
        public bool IsUnique;

        public ComponentAttribute() { }
        public ComponentAttribute(bool isUnique) { IsUnique = isUnique; }
    }
}
