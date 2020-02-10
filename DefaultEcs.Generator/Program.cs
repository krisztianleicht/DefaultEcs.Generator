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

        private const char DirectorySeparator = '/';

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<GeneratorOption>(args)
                .WithParsed(RunCodegeneration);
        }

        private static void RunCodegeneration(GeneratorOption generatorOption)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, events) => OnAssemblyResolve(sender, events, generatorOption);

            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assemblyLocation in generatorOption.Assemblies)
            {
                if (generatorOption.Verbose)
                    Log("Loading assembly from file: " + assemblyLocation);

                assemblies.Add(Assembly.LoadFrom(assemblyLocation));
            }

            var outputDirectory = NormalizeFilePath(generatorOption.OutputPath);

            if (generatorOption.Verbose)
                Log("Output directory: " + outputDirectory);

            var oldFiles = CollectOldFiles(outputDirectory);

            var namespaces = new HashSet<string>();
            bool allowRegenerate = generatorOption.RegenerateAll;
            var generators = typeof(ICodeGenerator).Assembly.GetTypes().Where(t => !t.IsInterface && typeof(ICodeGenerator).IsAssignableFrom(t)).Select(p => (ICodeGenerator)Activator.CreateInstance(p)).ToList();
            foreach (var assembly in assemblies)
            {
                var types = GetLoadableTypes(assembly);

                foreach (var type in types)
                {
                    try
                    {
                        var relativeDirectory = GetFolderForType(type);
                        var destinationDirectory = outputDirectory + relativeDirectory;
                        var destinationFilePath = destinationDirectory + type.Name + ".cs";
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
                            oldFiles.Remove(destinationFilePath);
                            CreateDirectoryIfNotExists(destinationDirectory);

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

            // Remove unused file
            foreach (var oldFile in oldFiles)
            {
                File.Delete(oldFile);
                Log("Removing unused file: " + oldFile);
            }
        }

        private static void CreateDirectoryIfNotExists(string destinationDirectory)
        {
            if (Directory.Exists(destinationDirectory))
                return;

            Directory.CreateDirectory(destinationDirectory);
        }

        private static HashSet<string> CollectOldFiles(string outputDirectory)
        {
            var oldFiles = new HashSet<string>();
            foreach (var file in Directory.EnumerateFiles(outputDirectory, "*.cs", SearchOption.AllDirectories))
                oldFiles.Add(NormalizeFilePath(file));
            return oldFiles;
        }

        private static string NormalizeFilePath(string file)
        {
            return file.Replace('\\', DirectorySeparator);
        }

        private static string GetFolderForType(Type type)
        {
            var fullName = type.FullName;
            if (!fullName.Contains('.'))
                return "";


            var fullNameEscaped = type.FullName.Replace('.', DirectorySeparator);
            fullNameEscaped = fullNameEscaped.Substring(0, fullNameEscaped.LastIndexOf(DirectorySeparator) + 1);
            return fullNameEscaped;
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args, GeneratorOption generatorOption)
        {
            var dllName = args.Name.Substring(0, args.Name.IndexOf(','));
            var name = args.Name;

            foreach (var resolutionDirectory in generatorOption.AssemblyResolutionFolders)
            {
                var dllPath = resolutionDirectory + dllName + ".dll";
                if (!File.Exists(dllPath))
                    continue;

                if (generatorOption.Verbose)
                    Log("Assembly successfully resolved: " + dllName + " from path: " + dllPath);
                return Assembly.LoadFrom(dllPath);
            }

            Log("Assembly not found: " + name);

            return null;
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
