using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    // Define a class to receive parsed values
    class Options
    {
        [Option('o', "output", DefaultValue = null,
          HelpText = "Destinaton folder for all generated files.")]
        public string OutputDir { get; set; }

        [ValueList(typeof(List<string>), MaximumElements = 1)]
        public IList<string> Input { get; set; }

        [Option('s', "source", DefaultValue = false,
            HelpText = "All input files will be treated as C# source files.")]
        public bool Source { get; set; }

        [Option("toolchain",
            HelpText = "Target Architecture: VisualCpp, Gcc, ArduinoIde or platformio")]
        public String Toolchain { get; set; }

        [Option('b', "board",
        HelpText = "The Board to use")]
        public String Board { get; set; }

        [Option('e', "entry", DefaultValue = null,
        HelpText = "The class containing the main method to start.")]
        public String EntryClass { get; set; }

        [Option("test", DefaultValue = null,
        HelpText = "The name of the test class or method to run. Leave empty to runn all tests in assembly.")]
        public String Test { get; set; }

        [Option('c', "compile-only", DefaultValue = false,
        HelpText = "Do not start the executable after compilation")]
        public bool CompileOnly { get; set; }

        [Option('v', "verbose", DefaultValue = false,
            HelpText = "Print verbose debug messages")]
        public bool Verbose { get; set; }

        [Option('g', "generate-only", DefaultValue = false,
        HelpText = "Only generate the c source and project files")]
        public bool GenerateOnly { get; set; }

        [Option("override", DefaultValue = false,
        HelpText = "Override existing files. WARNING, this maybe causes loss of your data")]
        public bool Override { get; set; }

        [Option('w', "wait", DefaultValue = false,
        HelpText = "Wait for key after execution finished")]
        public bool Wait { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
