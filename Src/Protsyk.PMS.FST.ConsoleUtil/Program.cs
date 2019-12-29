using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;
using Protsyk.PMS.FST.Persistance;

namespace Protsyk.PMS.FST.ConsoleUtil
{
    [Verb("build", HelpText = "Build Finite State Transducer from input file")]
    class BuildOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input file (text, UTF-8 encoded)")]
        public string InputFile { get; set; }

        [Option('n', "inputformat", Default = "map", HelpText = "Input format (sorted, map, plain)")]
        public string InputFormat { get; set; }

        [Option('o', "output", Required = false, Default = "output.fst", HelpText = "Output file name")]
        public string OutputFile { get; set; }

        [Option('c', "cachesize", Required = false, Default = 65000, HelpText = "Size of cache for minimized nodes")]
        public int CacheSize { get; set; }
    }

    [Verb("print", HelpText = "Print all terms and values encoded in FST")]
    class PrintOptions
    {
        [Option('i', "input", Required = false, Default = "output.fst", HelpText = "Input FST file")]
        public string InputFile { get; set; }

        [Option('p', "pattern", Required = false, Default = "*", HelpText = "Pattern for terms")]
        public string Pattern {get; set;}
    }

    [Verb("render", HelpText = "Render FST into dot notation")]
    class RenderOptions
    {
        [Option('i', "input", Required = false, Default = "output.fst", HelpText = "Input FST file")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, Default = "output.dot", HelpText = "Output dot file")]
        public string OutputFile { get; set; }
    }

    class Program
    {
        private static IFSTOutput<int> outputType = FSTVarIntOutput.Instance;

        static int Main(string[] args)
        {
            PrintConsole(ConsoleColor.Green, "PMS Finite State Transducer (FST) Library (c) Petro Protsyk 2018-2020");

            return Parser.Default.ParseArguments<BuildOptions, PrintOptions, RenderOptions>(args)
              .MapResult(
                (BuildOptions opts) => DoBuild(opts),
                (PrintOptions opts) => DoPrint(opts),
                (RenderOptions opts) => DoRender(opts),
                errors => 255);
        }

        private static int DoPrint(PrintOptions opts)
        {
            var timer = Stopwatch.StartNew();

            timer.Restart();
            var terms = 0;
            using (var inputFile = new FileStorage(opts.InputFile))
            {
                using (var fst = new PersistentFST<int>(outputType, inputFile))
                {
                    var maxLength = fst.Header == null ? 1024 : fst.Header.MaxLength;
                    if (fst.Header != null)
                    {
                        PrintConsole(ConsoleColor.White, $"FST header terms: {fst.Header.TermCount}, max length: {fst.Header.MaxLength}, states: {fst.Header.States}");
                    }
                    foreach (var term in fst.Match(new WildcardMatcher(opts.Pattern, maxLength)))
                    {
                        if (!fst.TryMatch(term, out int value))
                        {
                            throw new Exception("This is a bug");
                        }

                        ++terms;
                        Console.WriteLine($"{term}->{value}");
                    }
                    PrintConsole(ConsoleColor.White, $"FST print terms: {terms}, time: {timer.Elapsed}");
                }
            }
            return 0;
        }

        private static IEnumerable<ValueTuple<string, int>> ParseConvertAndSort(string fileName)
        {
            var index = 0;
            foreach (var item in File.ReadAllLines(fileName)
                             .Select(x => x.Replace("->", "").ToLowerInvariant())
                             .Distinct()
                             .OrderBy(x => x, StringComparer.Ordinal)
                             .Select(x => $"{x}->{index++}"))
            {
                var terms = item.Split("->");
                yield return (terms[0], int.Parse(terms[1]));
            }
        }

        private static IEnumerable<ValueTuple<string, int>> ParseAndSort(string fileName)
        {
            foreach (var item in File.ReadAllLines(fileName)
                                     .OrderBy(x => x.Split("->")[0], StringComparer.Ordinal))
            {
                var terms = item.Split("->");
                yield return (terms[0], int.Parse(terms[1]));
            }
        }

        private static IEnumerable<ValueTuple<string, int>> ParseSorted(string fileName)
        {
            foreach (var item in File.ReadLines(fileName))
            {
                var terms = item.Split("->");
                yield return (terms[0], int.Parse(terms[1]));
            }
        }

        private static IEnumerable<ValueTuple<string, int>> ParseFromOptions(BuildOptions opts)
        {
            if (opts.InputFormat == "sorted")
            {
                return ParseSorted(opts.InputFile);
            }

            if (opts.InputFormat == "map")
            {
                return ParseAndSort(opts.InputFile);
            }

            if (opts.InputFormat == "plain")
            {
                return ParseConvertAndSort(opts.InputFile);
            }

            throw new ArgumentException($"Input format is not correct {opts.InputFormat}");
        }

        private static int DoBuild(BuildOptions opts)
        {
            var timer = Stopwatch.StartNew();

            if (File.Exists(opts.OutputFile))
            {
                File.Delete(opts.OutputFile);
            }

            timer.Restart();
            var terms = 0;
            using (var outputFile = new FileStorage(opts.OutputFile))
            {
                using (var fstBuilder = new FSTBuilder<int>(outputType, opts.CacheSize, outputFile))
                {
                    fstBuilder.Begin();
                    foreach (var (term, score) in ParseFromOptions(opts))
                    {
                        fstBuilder.Add(term, score);
                        ++terms;
                    }
                    fstBuilder.End();
                    PrintConsole(ConsoleColor.White, $"FST constructed time: {timer.Elapsed}, terms: {terms}, cache size: {opts.CacheSize}, Memory: {Process.GetCurrentProcess().WorkingSet64}, output size: {outputFile.Length}");
                }
            }

            using (var outputFile = new FileStorage(opts.OutputFile))
            {
                if (outputFile.Length < 64 * 1024 * 1024)
                {
                    timer.Restart();
                    var data = new byte[outputFile.Length];
                    outputFile.ReadAll(0, data, 0, data.Length);
                    var fst = FST<int>.FromBytesCompressed(data, outputType);
                    foreach (var (term, score) in ParseFromOptions(opts))
                    {
                        if (!fst.TryMatch(term, out var value) || value != score)
                        {
                            throw new Exception($"Bug at term {term}: {value} != {score}");
                        }
                    }
                    PrintConsole(ConsoleColor.White, $"FST (memory) verification time: {timer.Elapsed}");
                }
            }


            timer.Restart();
            using (var outputFile = new FileStorage(opts.OutputFile))
            {
                using (var fst = new PersistentFST<int>(outputType, outputFile))
                {
                    foreach (var (term, score) in ParseFromOptions(opts))
                    {
                        if (!fst.TryMatch(term, out var value) || value != score)
                        {
                            throw new Exception($"Bug at term {term}: {value} != {score}");
                        }
                    }
                }
            }
            PrintConsole(ConsoleColor.White, $"FST (file)   verification time: {timer.Elapsed}");

            return 0;
        }

        private static int DoRender(RenderOptions opts)
        {
            var timer = Stopwatch.StartNew();

            if (File.Exists(opts.OutputFile))
            {
                File.Delete(opts.OutputFile);
            }

            timer.Restart();
            using (var inputFile = new FileStorage(opts.InputFile))
            {
                using (var outputFile = new FileStorage(opts.OutputFile))
                {
                    using (var fst = new PersistentFST<int>(outputType, inputFile))
                    {
                        fst.ToDotNotation(outputFile);
                    }
                }
            }

            PrintConsole(ConsoleColor.White, $"FST rendered to dot file {opts.OutputFile}, time: {timer.Elapsed}");
            return 0;
        }

        private static void PrintConsole(ConsoleColor color, string text)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = old;
        }
    }
}
