using DefaultEcs.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set*ClassName*(*FieldParamDefinition*)
        {
*FieldParamInitialization*
            Set*ClassName*(in instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove*ClassName*() => Remove<*ClassFullName*>();
    }
}";

        private const string GET_SET_FLAG_TEMPLATE = @"namespace DefaultEcs
{
    partial struct Entity
    {
        public bool Is*ClassName*
        {
            get
            {
                return Has<*ClassFullName*>();
            }
            set
            {
                if (value != Has<*ClassFullName*>())
                {
                    if (value)
                        Set(default(*ClassFullName*));
                    else
                        Remove<*ClassFullName*>();
                }
            }
        }
    }
}";


        private static Type requiredAttribute = typeof(ComponentAttribute);

        public bool CanProcess(Type t)
        {
            return t.HasComponent(requiredAttribute);
        }

        public void Process(StringBuilder sb, Type t, HashSet<string> requiredNamespaces)
        {
            if (t.GetFields().Length == 0)
            {
                sb.AppendLine(GET_SET_FLAG_TEMPLATE.ReplaceClassInformation(t));
            }
            else
            {
                requiredNamespaces.Add("System.Runtime.CompilerServices");

                sb.AppendLine(
                    GET_SET_TEMPLATE
                    .ReplaceParamInitialization(t)
                    .ReplaceClassInformation(t)
                    );
            }
        }
    }
}
