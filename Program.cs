using System;
using System.Reflection;

namespace csvscan
{
    class Program
    {
        static void Main(string[] args)
        {
            bool verbose = false;
            try
            {
                var assm = Assembly.GetExecutingAssembly();
                var version = assm.GetName().Version;
                Console.WriteLine("CSV Scan v{0}.{1}", version.Major, version.Minor);
                   
                if (args.Length > 0)
                {
                    switch (args[0].ToLower().Trim())
                    {
                        case "-h":
                            Help();
                            return;
                        case "-v":
                            verbose = true;
                            Console.WriteLine("Mode: Verbose");
                            break;
                        case "-s":
                        default:
                            Console.WriteLine("Mode: Silent");
                            break;
                    }
                }
                else
                {
                    Help();
                    return;
                }


                string oper;
                if (args.Length > 1)
                { oper = args[1]; }
                else
                {
                    if (!verbose)
                    {
                        Console.WriteLine("Operation not found. In silent mode, operation & options are to be provided as arguments.");
                        return;
                    }
                    Console.Write("(R)ead /(W)rite? ");
                    oper = Console.ReadLine().ToLower();
                }
                switch (oper)
                {
                    case "r":
                    case "-r":
                        if (!verbose)
                        {
                            if (args.Length < 5)
                            {
                                Console.WriteLine("In silent mode, options are to be provided as arguments.");
                                return;
                            }
                        }
                        #region Read
                        Console.WriteLine("Operation: Read");
                        Console.Write("Source File/Path: ");
                        string sourcePath;
                        if (args.Length > 2)
                        {
                            sourcePath = args[2].Replace("\"", "");
                            Console.WriteLine(sourcePath);
                        }
                        else
                        { sourcePath = Console.ReadLine(); }

                        Console.Write("Filter File: ");
                        string filterPath;
                        if (args.Length > 3)
                        {
                            filterPath = args[3].Replace("\"", "");
                            Console.WriteLine(filterPath);
                        }
                        else
                        { filterPath = Console.ReadLine(); }

                        Console.Write("Output Path: ");
                        string outputPath;
                        if (args.Length > 4)
                        {
                            outputPath = args[4].Replace("\"", "");
                            Console.WriteLine(outputPath);
                        }
                        else
                        { outputPath = Console.ReadLine(); }
                        #endregion
                        Console.WriteLine("");
                        Search scn = new Search(verbose, sourcePath);
                        scn.ScanFolder(filterPath, outputPath);
                        Console.WriteLine("");
                        break;
                    default:
                        Console.WriteLine("Invalid operation, exiting app.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured - {0}", ex.Message);
            }
            if (verbose)
            {
                Console.WriteLine("Press any key to close.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Program Command Arguments readme
        /// </summary>
        static void Help()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage: csvscan [mode] [operation] [options]");
            Console.WriteLine("mode: ");
            Console.WriteLine("\t -h \t Help (default)");
            Console.WriteLine("\t -v \t Verbose");
            Console.WriteLine("\t -s \t Silent");
            Console.WriteLine("");
            Console.WriteLine("operation: ");
            Console.WriteLine("\t -r \t Read");
            Console.WriteLine("");

            Console.WriteLine("options: ");
            Console.WriteLine("\t -r \"Source Folder\" \"Filter File\" \"Output Folder\"");
            Console.WriteLine("");
            Console.WriteLine("\t Source Folder: One or more source files in csv format.");
            Console.WriteLine("");
            Console.WriteLine("\t Filters File: Text file with one column filter per line.");
            Console.WriteLine("");
            Console.WriteLine("\t\tFilter format: {Column Index}={Value-1},{Value-2},..");
            Console.WriteLine("\t\te.g. Text Filter: 11=12345,hello,world,qwerty");
            Console.WriteLine("\t\te.g. Starts-With: 11=1234*,hell*");
            Console.WriteLine("\t\te.g. Ends-With: 11=*2345,*orld");
            Console.WriteLine("\t\te.g. Contains: 11=*234*,*wert*");
            Console.WriteLine("");
            Console.WriteLine("\t Output Folder: Filtered results folder");
            Console.WriteLine("");

        }
    }
}
