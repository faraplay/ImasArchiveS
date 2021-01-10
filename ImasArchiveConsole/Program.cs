using System;
using Imas;
using CommandLine;
using Imas.Archive;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Imas.Spreadsheet;

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

        [Verb("make-patch", HelpText = "Create or add to a patch zip.")]
        class MakePatchOptions
        {
            [Option('O', "overwrite", Default = false, HelpText = "Forces creation of new patch zip instead of adding to existing patch.")]
            public bool Overwrite { get; set; }
            [Option('p', "patch", Required = true, HelpText = "Name of patch to create or edit.")]
            public string Patch { get; set; }
            [Option('f', "file", HelpText = "A pair of arguments with the name of file to add to patch, and file name that the game uses. " +
                "Can have multiple pairs.")]
            public IEnumerable<string> Files { get; set; }
            [Option('c', "commu", Required = false, HelpText = "Name of the spreadsheet containing the commus.")]
            public string Commu { get; set; }
            [Option('P', "parameter", Required = false, HelpText = "Name of spreadsheet containing parameter data.")]
            public string Parameter { get; set; }
            [Option('I', "image", Required = false, HelpText = "Directory containing images.")]
            public string Image { get; set; }
            [Option('l', "lyrics", Required = false, HelpText = "Directory containing lyrics files.")]
            public string Lyrics { get; set; }
            [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
            public bool Verbose { get; set; }
        }

        [Verb("extract", HelpText = "Extract data from an arc file.")]
        class ExtractOptions
        {
            [Option('i', "input", Required = true, HelpText = "Name of arc to read from.")]
            public string Input { get; set; }
            [Option('o', "output", Required = true, HelpText = "Name of output directory or spreadsheet.")]
            public string Output { get; set; }
            [Option('O', "overwrite", Default = false, HelpText = "Overwrites the contents of any existing spreadsheet or directory.")]
            public bool Overwrite { get; set; }
            [Option('c', "commu", HelpText = "Extract commu data to a spreadsheet.", SetName = "commu")]
            public bool Commu { get; set; }
            [Option('P', "parameter", HelpText = "Extract parameter data to a spreadsheet.", SetName = "param")]
            public bool Parameter { get; set; }
            [Option('I', "image", HelpText = "Extract images to a new directory.", SetName = "image")]
            public bool Image { get; set; }
            [Option('l', "lyrics", HelpText = "Extract lyrics to a new directory.", SetName = "lyric")]
            public bool Lyrics { get; set; }
            [Option('A', "all", HelpText = "Extract all files to a new directory.", SetName = "all")]
            public bool All { get; set; }
            [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
            public bool Verbose { get; set; }
        }

        [Verb("copy", HelpText = "Copy columns of data from one spreadsheet to another.")]
        class CopyOptions
        {
            [Option('s', "source", Required = true, HelpText = "The spreadsheet to copy from.")]
            public string Source { get; set; }
            [Option('d', "destination", HelpText = "The spreadsheet to copy data into.")]
            public string Destination { get; set; }
            [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
            public bool Verbose { get; set; }
        }

        static async Task<int> Main(string[] args)
        {
            try
            {
                (await
                (await
                (await
                Parser.Default.ParseArguments<PatchArcOptions, MakePatchOptions, ExtractOptions, CopyOptions>(args)
                    .WithParsedAsync<PatchArcOptions>((PatchArcOptions o) => PatchArc(o)))
                    .WithParsedAsync<MakePatchOptions>((MakePatchOptions o) => MakePatch(o)))
                    .WithParsedAsync<ExtractOptions>((ExtractOptions o) => Extract(o)))
                    .WithParsed<CopyOptions>((CopyOptions o) => Copy(o));
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

        static async Task MakePatch(MakePatchOptions o)
        {
            var progress = o.Verbose ? consoleProgress : null;
            using PatchZipFile patchZipFile = new PatchZipFile(o.Patch, o.Overwrite ? PatchZipMode.Create : PatchZipMode.Update);
            if (o.Files.Any())
            {
                List<string> files = o.Files.ToList();
                if (files.Count % 2 != 0)
                {
                    Console.Error.WriteLine("Odd number of --file command-line parameters.");
                    return;
                }
                else
                {
                    for (int i = 0; i < files.Count; i += 2)
                    {
                        progress?.Report(string.Format("Adding file {0} as {1}.", files[i], files[i + 1]));
                        patchZipFile.AddFile(files[i], files[i + 1]);
                    }
                }
            }
            if (o.Commu != null)
            {
                progress?.Report("Adding commus.");
                await patchZipFile.AddCommus(o.Commu, progress, progress);
            }
            if (o.Parameter != null)
            {
                progress?.Report("Adding parameter files.");
                patchZipFile.AddParameterFiles(o.Parameter);
            }
            if (o.Image != null)
            {
                progress?.Report("Adding images.");
                await patchZipFile.AddGtfs(o.Image, progress);
            }
            if (o.Lyrics != null)
            {
                progress?.Report("Adding lyrics.");
                await patchZipFile.AddLyrics(o.Lyrics, progress);
            }
            progress?.Report("Done.");
        }

        static async Task Extract(ExtractOptions o)
        {
            var progress = o.Verbose ? consoleProgress : null;
            if (!o.Commu &&
                !o.Parameter &&
                !o.Image &&
                !o.Lyrics &&
                !o.All)
            {
                Console.Error.WriteLine("No data specified to extract. Use --help for guidelines on usage.");
                return;
            }

            using ArcFile arcFile = new ArcFile(o.Input);
            if (o.Commu)
            {
                await arcFile.ExtractCommusToXlsx(o.Output, o.Overwrite, progress);
            }
            if (o.Parameter)
            {
                await arcFile.ExtractParameterToXlsx(o.Output, o.Overwrite, progress);
            }
            if (o.Image)
            {
                if (o.Overwrite && Directory.Exists(o.Output))
                {
                    Directory.Delete(o.Output, true);
                }
                await arcFile.ExtractAllImages(o.Output, progress);
            }
            if (o.Lyrics)
            {
                if (o.Overwrite && Directory.Exists(o.Output))
                {
                    Directory.Delete(o.Output, true);
                }
                await arcFile.ExtractLyrics(o.Output, progress);
            }
            if (o.All)
            {
                if (o.Overwrite && Directory.Exists(o.Output))
                {
                    Directory.Delete(o.Output, true);
                }
                await arcFile.ExtractAllAsync(o.Output, false, progress);
            }
        }

        static void Copy(CopyOptions o)
        {
            var progress = o.Verbose ? consoleProgress : null;
            using XlsxColumnCopy xlsxColumnCopy = new XlsxColumnCopy(o.Destination, o.Source);
            xlsxColumnCopy.CopyColumns(progress);
        }

        static readonly IProgress<object> consoleProgress = new Progress<object>(Console.WriteLine);
    }
}
