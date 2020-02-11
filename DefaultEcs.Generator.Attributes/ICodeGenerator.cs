using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultEcs.Generator.Attributes
{
    public interface ICodeGenerator
    {

        void Initialize();

        bool CanProcess(Type t);
        void Process(StringBuilder sb, Type t, HashSet<string> requiredNamespaces);
        
        void Finish(string outputPath, HashSet<string> oldFiles);
    }
}
