﻿using DefaultEcs.Attributes;
using DefaultEcs.Generator.Attributes;
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

        public ref readonly Entity Get*ClassName*Entity()
        {
            if (*classEscapedFullName* == null)
                *classEscapedFullName* = GetEntities().With<*ClassFullName*>().AsSet();

            if (*classEscapedFullName*.Count == 0)
                throw new InvalidOperationException(""Entity with *ClassName* doesn't exist!"");

            if (*classEscapedFullName*.Count > 1)
                throw new InvalidOperationException(""More than one entity with *ClassName*!"");

            return ref *classEscapedFullName*.GetEntities()[0];
        }

        public bool Has*ClassName*
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (*classEscapedFullName* == null)
                    *classEscapedFullName* = GetEntities().With<*ClassFullName*>().AsSet();

                return *classEscapedFullName*.Count == 1;
            }
        }

        public ref *ClassFullName* *ClassName*
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ref Get*ClassName*Entity().Get<*ClassFullName*>(); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set*ClassName*(in *ClassFullName* comp)
        {
            if (Has*ClassName*)
            {
                Get*ClassName*Entity().Set(in comp);
            }
            else
            {
                CreateEntity().Set(in comp);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set*ClassName*(*FieldParamDefinition*)
        {
*FieldParamInitialization*
            Set*ClassName*(in instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove*ClassName*()
        {
            Get*ClassName*Entity().Dispose();
        }
    }
}";

        private const string UNIQUE_FLAG_TEMPLATE = @"namespace DefaultEcs
{
    partial class World
    {
        private EntitySet *classEscapedFullName*;

        public ref readonly Entity Get*ClassName*Entity()
        {
            if (*classEscapedFullName* == null)
                *classEscapedFullName* = GetEntities().With<*ClassFullName*>().AsSet();

            if (*classEscapedFullName*.Count == 0)
                throw new InvalidOperationException(""Entity with *ClassName* doesn't exist!"");

            if (*classEscapedFullName*.Count > 1)
                throw new InvalidOperationException(""More than one entity with *ClassName*!"");

            return ref *classEscapedFullName*.GetEntities()[0];
        }

        public bool Is*ClassName*
        {
            get
            {
                if (*classEscapedFullName* == null)
                    *classEscapedFullName* = GetEntities().With<*ClassFullName*>().AsSet();

                if (*classEscapedFullName*.Count > 1)
                    throw new InvalidOperationException(""More than one entity with *ClassName*!"");

                return *classEscapedFullName*.Count == 1;
            }
            set
            {
                if (*classEscapedFullName* == null)
                    *classEscapedFullName* = GetEntities().With<*ClassFullName*>().AsSet();

                if (*classEscapedFullName*.Count > 1)
                    throw new InvalidOperationException(""More than one entity with *ClassName*!"");

                if (value)
                {
                    if (*classEscapedFullName*.Count == 0)
                    {
                        CreateEntity().Set(default(*ClassFullName*));
                    }
                }
                else
                {
                    if (*classEscapedFullName*.Count == 1)
                    {
                        *classEscapedFullName*.GetEntities()[0].Remove<*ClassFullName*>();
                    }
                }
            }
        }
    }
}
";


        private static Type requiredAttribute = typeof(ComponentAttribute);

        public bool CanProcess(Type t)
        {
            return t.HasComponent(requiredAttribute) && ((ComponentAttribute)t.GetCustomAttributes(requiredAttribute, false)[0]).IsUnique;
        }

        public void Process(StringBuilder sb, Type t, HashSet<string> requiredNamespaces)
        {
            requiredNamespaces.Add("System");
            requiredNamespaces.Add("System.Runtime.CompilerServices");
            if (t.GetFields().Length == 0)
            {
                sb.AppendLine(UNIQUE_FLAG_TEMPLATE.ReplaceClassInformation(t));
            }
            else
            {
                sb.AppendLine(
                    UNIQUE_TEMPLATE
                    .ReplaceParamInitialization(t)
                    .ReplaceClassInformation(t)
                    );
            }
        }

        public void Initialize()
        {
        }

        public void Finish(string outputPath, HashSet<string> oldFiles)
        {
        }

    }
}
