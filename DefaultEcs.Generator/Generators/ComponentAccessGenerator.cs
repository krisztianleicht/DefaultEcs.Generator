using DefaultEcs.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultEcs.Generator.Generators
{

    public class ComponentAccessGenerator : ICodeGenerator
    {

        private const string GET_SET_TEMPLATE = @"namespace DefaultEcs
{
    partial struct Entity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has*ClassName*() => Has<*ClassFullName*>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref *ClassFullName* Get*ClassName*() => ref Get<*ClassFullName*>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set*ClassName*(in *ClassFullName* component = default) { Set(component); }
    }
}";

        private static Type requiredAttribute = typeof(ComponentAttribute);

        public bool CanProcess(Type t)
        {
            return t.HasComponent(requiredAttribute);
        }

        public void Process(StringBuilder sb, Type t, HashSet<string> requiredNamespaces)
        {
            requiredNamespaces.Add("System.Runtime.CompilerServices");
            sb.AppendLine(GET_SET_TEMPLATE.ReplaceClassInformation(t));
        }
    }
}
