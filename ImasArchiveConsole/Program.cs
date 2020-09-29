using System;
using Imas;
using CommandLine;
using Imas.Archive;
using System.Threading.Tasks;

namespace ImasArchiveConsole
{
    class Program
    {
        [Verb("patch", true, HelpText = "Patch an arc file.")]
        class PatchArcOptions
        {
            [Option('i', "input", Required = true, HelpText = "Input arc file to be patched.")]
            public string Input { get; set; }
            [Option('p', "patch", Required =true, HelpText = "Patch zip file to apply to arc.")]
            public string Patch { get; set; }
            [Option('o', "output", Required = true, HelpText = "Name of output patched arc.")]
            public string Output { get; set; }
            [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
            public bool Verbose { get; set; }
        }
        static async Task<int> Main(string[] args)
        {
            try
            {
                await Parser.Default.ParseArguments<PatchArcOptions, object>(args)
                    .WithParsedAsync<PatchArcOptions>((PatchArcOptions o) => PatchArc(o));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
            return 0;
        }

        static async Task PatchArc(PatchArcOptions o)
        {
                await ArcFile.PatchArcFromZip(
                    o.Input, 
                    o.Patch, 
                    o.Output,
                    o.Verbose ? consoleProgress : null);
        }

        static readonly Progress<ProgressData> consoleProgress = new Progress<ProgressData>(Console.WriteLine);
    }
}
