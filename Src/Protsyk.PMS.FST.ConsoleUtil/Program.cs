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

        [Option('o', "output", Required = false, Default = "output.fst", HelpText = "Output file name")]
        public string OutputFile { get; set; }

        [Option('f', "format", Required = false, Default = "Default", HelpText = "Output format (Default, Compressed, DOT)")]
        public string Format { get; set; }

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

    class Program
    {
        private static IFSTOutput<int> outputType = FSTVarIntOutput.Instance;

        static int Main(string[] args)
        {
            PrintConsole(ConsoleColor.Green, "PMS Finite State Transducer (FST) Library (c) Petro Protsyk 2018");

            return Parser.Default.ParseArguments<BuildOptions, PrintOptions>(args)
              .MapResult(
                (BuildOptions opts) => DoBuild(opts),
                (PrintOptions opts) => DoPrint(opts),
                errors => 255);
        }

        private static int DoPrint(PrintOptions opts)
        {
            var timer = Stopwatch.StartNew();

            timer.Restart();
            var terms = 0;
            using (var outputFile = new FileStorage(opts.InputFile))
            {
                using (var fst = new PersistentFST<int>(outputType, outputFile))
                {
                    foreach (var term in fst.Match(new WildcardMatcher(opts.Pattern, 255)))
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

        private static int DoBuild(BuildOptions opts)
        {
            var timer = Stopwatch.StartNew();
            var input = File.ReadAllLines(opts.InputFile).OrderBy(x => x.Split("->")[0], StringComparer.Ordinal).ToArray();
            var terms = new string[input.Length];
            var outputs = new int[input.Length];
            for (int i = 0; i < input.Length; ++i)
            {
                var s = input[i].Split("->");
                terms[i] = s[0];
                outputs[i] = int.Parse(s[1]);
                // Console.WriteLine($"{terms[i]}->{outputs[i]}");
            }
            PrintConsole(ConsoleColor.White, $"Input read term: {terms.Length}, time: {timer.Elapsed}");

            if (File.Exists(opts.OutputFile))
            {
                File.Delete(opts.OutputFile);
            }

            timer.Restart();
            using (var outputFile = new FileStorage(opts.OutputFile))
            {
                using (var fstBuilder = new FSTBuilder<int>(outputType, opts.CacheSize, outputFile))
                {
                    fstBuilder.Begin();
                    for (int j = 0; j < terms.Length; ++j)
                    {
                        fstBuilder.Add(terms[j], outputs[j]);
                    }
                    fstBuilder.End();
                    PrintConsole(ConsoleColor.White, $"FST constructed time: {timer.Elapsed}, cache size: {opts.CacheSize}, Memory: {Process.GetCurrentProcess().WorkingSet64}, output size: {outputFile.Length}");
                }
            }

            using (var outputFile = new FileStorage(opts.OutputFile))
            {
                if (outputFile.Length < 64 * 1024 * 1024)
                {
                    timer.Restart();
                    var data = new Byte[outputFile.Length];
                    outputFile.ReadAll(0, data, 0, data.Length);
                    var fst = FST<int>.FromBytesCompressed(data, outputType);
                    for (int i = 0; i < terms.Length; ++i)
                    {
                        if (!fst.TryMatch(terms[i], out var value) || value != outputs[i])
                        {
                            throw new Exception($"Bug at term {terms[i]}: {value} != {outputs[i]}");
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
                    for (int i = 0; i < terms.Length; ++i)
                    {
                        if (!fst.TryMatch(terms[i], out var value) || value != outputs[i])
                        {
                            throw new Exception($"Bug at term {terms[i]}: {value} != {outputs[i]}");
                        }
                    }
                }
            }
            PrintConsole(ConsoleColor.White, $"FST (file)   verification time: {timer.Elapsed}");

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
