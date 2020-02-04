using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultEcs.Generator.Generators
{
    public interface ICodeGenerator
    {
        bool CanProcess(Type t);
        void Process(StringBuilder sb, Type t, HashSet<string> requiredNamespaces);
    }
}
