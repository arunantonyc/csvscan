using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace csvscan
{
    public class Scan
    {
        static string OUTPUTFILENAME = "searchResults";
        string _srcPath;
        bool _verbose;

        long _scanCtr;
        public long ScanCount
        {
            get { return _scanCtr; }
        }
        long _resultsCtr;
        public long ResultsCount
        {
            get { return _resultsCtr; }
        }

        public Scan(bool verbose)
        {
            _verbose = verbose;
        }
        public Scan(bool verbose, string srcFilePath) : this(verbose)
        {
            if (!Directory.Exists(srcFilePath)) throw new DirectoryNotFoundException("Source folder/file not found.");
            string srcFolder = Path.GetFullPath(srcFilePath).Replace('\\', '/');

            var extn = Path.GetExtension(srcFilePath);
            if (string.IsNullOrEmpty(extn))
            {
                srcFolder += '/';
            }
            _srcPath = srcFolder.Substring(0, srcFolder.LastIndexOf('/'));
        }

        public void FilterFolder(string filterFile, string outputPath)
        {
            if (!File.Exists(filterFile)) throw new FileNotFoundException("Filters file not found.");
            Dictionary<int, string> filters = new Dictionary<int, string>();

            // load filters
            using (StreamReader rdrFilters = new StreamReader(filterFile))
            {
                string line;
                do
                {
                    line = rdrFilters.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var filterParts = line.Split('=');
                        if (filterParts.Length < 2)
                        { continue; }

                        int indx = 0;
                        try
                        {
                            indx = int.Parse(filterParts[0]);
                        }
                        catch { continue; }

                        filters.Add(indx, filterParts[1]);
                    }
                } while (!string.IsNullOrEmpty(line));
            }
            if (filters.Count < 1)
                throw new Exception("Invalid filters or none loaded.");
            else
                if (_verbose) { Console.WriteLine("Loaded {0} filters", filters.Count); }

            // get a unique output filename
            if (!Directory.Exists(outputPath)) throw new DirectoryNotFoundException("Output folder not found.");
            outputPath = Helpers.GetNewFileName(outputPath, OUTPUTFILENAME, "csv");

            // source files
            DateTime stTime = DateTime.Now;
            try
            {
                // scan src files
                string[] sourceFiles = Directory.GetFiles(_srcPath);
                _scanCtr = 0;
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    var foundCtr = Filter(sourceFiles[i], filters, outputPath, out int readCtr);
                    _resultsCtr += foundCtr;
                    _scanCtr += readCtr;
                }
            }
            catch { throw new Exception("Scan failed"); }
            finally
            {
                TimeSpan ts = DateTime.Now - stTime;
                Console.WriteLine("");
                Console.WriteLine("Scan Time: {0}, Found: {1}/{2}, Output: {3} ", ts.TotalSeconds, _resultsCtr, _scanCtr, outputPath.PadLeft(20, '.'));
                Console.WriteLine("");
            }
        }

        public int Filter(string srcFile, Dictionary<int, string> filters, string outputFile, out int readCtr)
        {
            readCtr = 0;
            int resultsCtr = 0;
            StreamWriter wtr = null;
            try
            {
                bool writeCsvHeader = !File.Exists(outputFile);
                // open output file
                CsvWriter csvWtr = null;
                wtr = new StreamWriter(outputFile, true);
                csvWtr = new CsvWriter(wtr);
                csvWtr.Configuration.HasHeaderRecord = false;

                // search source file
                using (StreamReader rdr = new StreamReader(srcFile))
                {
                    var csvRdr = new CsvReader(rdr);
                    csvRdr.Configuration.TrimOptions = TrimOptions.Trim | TrimOptions.InsideQuotes;
                    csvRdr.Read();
                    csvRdr.ReadHeader();

                    while (csvRdr.Read())
                    {
                        readCtr++;
                        try
                        {
                            bool checksPassed = true;
                            foreach (int fieldIdx in filters.Keys)
                            {
                                var field = csvRdr.GetField<string>(fieldIdx).Trim();
                                if (!string.IsNullOrEmpty(field))
                                {
                                    List<string> vals = filters[fieldIdx].Trim().Split(',').ToList();
                                    if (!vals.Contains(field))
                                    {
                                        checksPassed = false;
                                        continue;
                                    }
                                }
                                else { checksPassed = false; continue; }
                            }
                            if (checksPassed)
                            {
                                var record = csvRdr.GetRecord<dynamic>();
                                if (csvWtr != null)
                                {
                                    csvWtr.WriteRecord(record);
                                    csvWtr.NextRecord();
                                }
                                resultsCtr++;
                            }

                        }
                        catch { }
                    }
                }
                return resultsCtr;
            }
            catch
            {
                return resultsCtr;
            }
            finally
            {
                // close output file
                if (wtr != null)
                {
                    wtr.Close();
                    //wtr.Dispose();
                }
                if (_verbose)
                {
                    var fileName = "/" + Path.GetFileName(srcFile);
                    Console.WriteLine("Source File: {0}, Found:{1}/{2}", fileName.PadLeft(20, '.'), resultsCtr.ToString().PadLeft(7), readCtr.ToString().PadLeft(7));
                }
            }
        }
    }
}
