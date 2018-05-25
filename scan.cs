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
            StreamWriter wtr = null;
            try
            {
                // output file                
                bool writeCsvHeader = !File.Exists(outputPath);
                
                CsvWriter csvWtr = null;
                if (!string.IsNullOrEmpty(outputPath))
                {
                    wtr = new StreamWriter(outputPath, true);
                    csvWtr = new CsvWriter(wtr);
                    csvWtr.Configuration.HasHeaderRecord = false;
                }

                StringComparer strComparer = new StringComparer();

                // scan src files
                string[] sourceFiles = Directory.GetFiles(_srcPath);
                _scanCtr = 0;
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    var foundCtr = Filter(sourceFiles[i], filters, csvWtr, out int readCtr, strComparer);
                    _resultsCtr += foundCtr;
                    _scanCtr += readCtr;
                }
            }
            catch { throw new Exception("Scan failed"); }
            finally
            {
                // close output file
                if (wtr != null)
                {
                    wtr.Close();
                    //wtr.Dispose();
                }
                TimeSpan ts = DateTime.Now - stTime;
                Console.WriteLine("");
                Console.WriteLine("Scan Time: {0}, Found: {1}/{2}, Output: {3} ", ts.TotalSeconds, _resultsCtr, _scanCtr, outputPath.PadLeft(20, '.'));
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Scans source file to output filtered records
        /// </summary>
        /// <param name="srcFile">CSV Source file</param>
        /// <param name="filters">List of column filters</param>
        /// <param name="outputFile">Filtered output file</param>
        /// <param name="readCtr">Total records read from source file</param>
        /// <returns>Number of records found matching the filters</returns>
        public int Filter(string srcFile, Dictionary<int, string> filters, CsvWriter csvWtr, out int readCtr, StringComparer strComparer)
        {
            readCtr = 0;
            int resultsCtr = 0;            
            try
            {
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
                            if (RecordCheck(filters, csvRdr, strComparer))
                            {
                                resultsCtr++;
                                if (csvWtr != null)
                                {
                                    var record = csvRdr.GetRecord<dynamic>();
                                    csvWtr.WriteRecord(record);
                                    csvWtr.NextRecord();
                                }
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
                if (_verbose)
                {
                    var fileName = "/" + Path.GetFileName(srcFile);
                    Console.WriteLine("Source File: {0}, Found:{1}/{2}", fileName.PadLeft(20, '.'), resultsCtr.ToString().PadLeft(7), readCtr.ToString().PadLeft(7));
                }
            }
        }

        public bool RecordCheck(Dictionary<int, string> filters, CsvReader record, StringComparer strComparer)
        {
            bool checksPassed = true;
            try
            {
                foreach (int fieldIdx in filters.Keys)
                {
                    var field = record.GetField<string>(fieldIdx).Trim();
                    if (string.IsNullOrEmpty(field)) { checksPassed = false; continue; }
                    
                    List<string> vals = filters[fieldIdx].Trim().Split(',').ToList();
                    if (!vals.Contains(field, strComparer))
                    {
                        checksPassed = false;
                        continue;
                    }
                }
                return checksPassed;
            }
            catch
            {
                return false;
            }
        }
    }

    
}
