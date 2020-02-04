using DefaultEcs.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultEcs.Generator.Generators
{
    public class UniqueComponentGenerator : ICodeGenerator
    {

        private const string UNIQUE_TEMPLATE = @"namespace DefaultEcs
{
    partial class World
    {
        private EntitySet *classEscapedFullName*;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref *ClassFullName* Get*ClassName*()
        {
            if (*classEscapedFullName* == null)
                *classEscapedFullName* = GetEntities().With<*ClassFullName*>().AsSet();
            
            if (*classEscapedFullName*.Count == 0)
                throw new InvalidOperationException(""Entity with *ClassName* does not exists!"");
            if (*classEscapedFullName*.Count > 1)
                throw new InvalidOperationException(""More than one entity with *ClassName*!"");

            var array = *classEscapedFullName*.GetEntities();
            return ref array[0].Get<*ClassFullName*>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set*ClassName*(in *ClassFullName* comp)
        {
             if (*classEscapedFullName* == null)
                *classEscapedFullName* = GetEntities().With<*ClassFullName*>().AsSet();
            
            if (*classEscapedFullName*.Count == 0)
                throw new InvalidOperationException(""Entity with *ClassName* does not exists!"");
            if (*classEscapedFullName*.Count > 1)
                throw new InvalidOperationException(""More than one entity with *ClassName*!"");

            var array = *classEscapedFullName*.GetEntities();
            array[0].Set(in comp);
        }
    }
}";

        private static Type requiredAttribute = typeof(ComponentAttribute);

        public bool CanProcess(Type t)
        {
            return t.HasComponent(requiredAttribute) && ((ComponentAttribute)t.GetCustomAttributes(requiredAttribute, false)[0]).IsUnique;
        }

        public void Process(StringBuilder sb, Type t, HashSet<string> requiredNamespaces)
        {
            requiredNamespaces.Add("System.Runtime.CompilerServices");
            requiredNamespaces.Add("System");

            sb.AppendLine(UNIQUE_TEMPLATE.ReplaceClassInformation(t));
        }

    }
}
