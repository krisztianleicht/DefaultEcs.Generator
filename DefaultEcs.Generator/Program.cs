using CommandLine;
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
            CommandLine.Parser.Default.ParseArguments<GeneratorOption>(args)
                .WithParsed(RunCodegeneration);
        }

        private static void RunCodegeneration(GeneratorOption generatorOption)
        {
            //AppDomain.CurrentDomain.AssemblyResolve += (sender, events) => OnAssemblyResolve(sender, events, generatorOption);

            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assemblyLocation in generatorOption.Assemblies)
            {
                if (generatorOption.Verbose)
                    Log("Loading assembly from file: " + assemblyLocation);

                assemblies.Add(Assembly.LoadFrom(assemblyLocation));
            }

            var outputDirectory = generatorOption.OutputPath;

            if (generatorOption.Verbose)
                Log("Output directory: " + outputDirectory);

            bool allowRegenerate = generatorOption.RegenerateAll;

            var generators = typeof(ICodeGenerator).Assembly.GetTypes().Where(t => !t.IsInterface && typeof(ICodeGenerator).IsAssignableFrom(t)).Select(p => (ICodeGenerator)Activator.CreateInstance(p)).ToList();
            foreach (var assembly in assemblies)
            {
                var types = GetLoadableTypes(assembly);

                var namespaces = new HashSet<string>();
                foreach (var type in types)
                {
                    try
                    {
                        var destinationFilePath = outputDirectory + type.Name + ".cs";
                        if (File.Exists(destinationFilePath) && !allowRegenerate)
                            continue;

                        var sb = new StringBuilder();
                        foreach (var generator in generators)
                        {
                            if (!generator.CanProcess(type))
                                continue;
                            generator.Process(sb, type, namespaces);
                        }

                        foreach (var ns in namespaces)
                            sb.Insert(0, "using " + ns + ";" + Environment.NewLine);

                        if (sb.Length > 0)
                        {
                            File.WriteAllText(destinationFilePath, sb.ToString());
                            if (generatorOption.Verbose)
                                Log("Generated code for type: " + type);
                        }

                        namespaces.Clear();
                    }
                    catch (Exception e)
                    {
                        Log("Error while processing type: " + type.FullName);
                    }
                }
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args, GeneratorOption obj)
        {
            var name = args.Name;
            Console.WriteLine("Need to load: " + name);
            return null;
            //var path = resolver.Resolve(args.Name);

            //if (path == null)
            //{
            //    Debug.WriteLine(args.Name + " /// " + args.RequestingAssembly);
            //    Debug.WriteLine("Result: null");
            //    return null;
            //}
            //else
            //{
            //    Debug.WriteLine("Loaded: " + path);
            //    return Assembly.LoadFrom(path);
            //}

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
