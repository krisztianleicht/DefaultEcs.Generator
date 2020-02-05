using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultEcs.Generator
{
    public class GeneratorOption
    {

        [Option('d', "dll", Required = true, HelpText = "Assembly files to be processed.")]
        public IEnumerable<string> Assemblies { get; set; }

        [Option('o', "outputpath", Required = true, HelpText = "Directory where the files will be generated.")]
        public string OutputPath { get; set; }

        [Option('r', "regenerateAll", Default = true, HelpText = "Regenerate every file.")]
        public bool RegenerateAll { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

    }
}
