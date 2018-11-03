using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;

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

            var bytes = File.ReadAllBytes(opts.InputFile);
            var fstBytes = bytes.Skip(7).ToArray();
            var fst = default(FST<int>);
            if (bytes[6] == 'D')
            {
                fst = FST<int>.FromBytes(fstBytes, outputType);
            } 
            else if (bytes[6] == 'C')
            {
                fst = FST<int>.FromBytesCompressed(fstBytes, outputType);
            }
            else
            {
                throw new NotSupportedException("FST format is not supported or input is not correct");
            }
            PrintConsole(ConsoleColor.White, $"FST read from: {opts.InputFile}, time: {timer.Elapsed}");

            timer.Restart();
            var terms = 0;
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

            return 0;
        }

        private static int DoBuild(BuildOptions opts)
        {
            var timer = Stopwatch.StartNew();
            var input = File.ReadAllLines(opts.InputFile).OrderBy(x=>x.Split("->")[0], StringComparer.Ordinal).ToArray();
            var terms = new string[input.Length];
            var outputs = new int[input.Length];
            for (int i=0; i<input.Length; ++i)
            {
                var s = input[i].Split("->");
                terms[i] = s[0];
                outputs[i] = int.Parse(s[1]);
                // Console.WriteLine($"{terms[i]}->{outputs[i]}");
            }
            PrintConsole(ConsoleColor.White, $"Input read term: {terms.Length}, time: {timer.Elapsed}");

            timer.Restart();
            var fst = new FSTBuilder<int>(outputType).FromList(terms, outputs);
            PrintConsole(ConsoleColor.White, $"FST constructed time: {timer.Elapsed}");

            timer.Restart();
            for (int i=0; i<terms.Length; ++i)
            {
                if (!fst.TryMatch(terms[i], out var value) || value != outputs[i])
                {
                    throw new Exception($"Bug at term {terms[i]}: {value} != {outputs[i]}");
                }
            }
            PrintConsole(ConsoleColor.White, $"FST verification time: {timer.Elapsed}");

            var size = 0;
            timer.Restart();
            if (opts.Format == "Default")
            {
                var fstBytes = fst.GetBytes();
                var data = new byte[6 + 1 + fstBytes.Length];
                data[0] = (byte)'F';
                data[1] = (byte)'S';
                data[2] = (byte)'T';
                data[3] = (byte)'-';
                data[4] = (byte)'0';
                data[5] = (byte)'1';
                data[6] = (byte)'D';
                Array.Copy(fstBytes, 0, data, 7, fstBytes.Length);
                File.WriteAllBytes(opts.OutputFile, data);
                size = data.Length;
            }
            else if (opts.Format == "Compressed")
            {
                var fstBytes = fst.GetBytesCompressed();
                var data = new byte[6 + 1 + fstBytes.Length];
                data[0] = (byte)'F';
                data[1] = (byte)'S';
                data[2] = (byte)'T';
                data[3] = (byte)'-';
                data[4] = (byte)'0';
                data[5] = (byte)'1';
                data[6] = (byte)'C';
                Array.Copy(fstBytes, 0, data, 7, fstBytes.Length);
                File.WriteAllBytes(opts.OutputFile, data);
                size = data.Length;
            }
            else if (opts.Format == "Dot")
            {
                throw new NotImplementedException();
            }
            PrintConsole(ConsoleColor.White, $"FST written to the output file: {opts.OutputFile}, size: {size}, time: {timer.Elapsed}");

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
