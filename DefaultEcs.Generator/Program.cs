using DefaultEcs.Generator.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DefaultEcs.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var assemblyFileName = args[0];
            var outputDirectory = args[1];

            Console.WriteLine("Assembly path: " + assemblyFileName);
            Console.WriteLine("Output directory: " + outputDirectory);

            const bool allowRegenerate = true; // TODO make optional if needed

            var targetAssembly = Assembly.LoadFile(assemblyFileName);
            var types = GetLoadableTypes(targetAssembly);
            var generators = typeof(ICodeGenerator).Assembly.GetTypes().Where(t => !t.IsInterface && typeof(ICodeGenerator).IsAssignableFrom(t)).Select(p => (ICodeGenerator)Activator.CreateInstance(p)).ToList();

            var namespaces = new HashSet<string>();
            foreach (var type in types)
            {
                var destinationFilePath = outputDirectory + type.Name + ".cs";
                if (File.Exists(destinationFilePath) && !allowRegenerate)
                    continue;

                //Console.WriteLine("Can process type: " + type + " " + generators.Any(p => p.CanProcess(type)));

                if (!generators.Any(p => p.CanProcess(type)))
                    continue;

                var sb = new StringBuilder();
                foreach (var generator in generators)
                    generator.Process(sb, type, namespaces);

                foreach (var ns in namespaces)
                    sb.Insert(0, "using " + ns + ";" + Environment.NewLine);

                File.WriteAllText(destinationFilePath, sb.ToString());
                Console.WriteLine("Generated code for type: " + type);
            }

            Console.ReadLine();
        }

        public static Type[] GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }

    }
}
