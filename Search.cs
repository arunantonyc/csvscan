using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace csvscan
{
    public class Search
    {
        //Default search results file name template
        static string OUTPUTFILENAME = "searchResults";
        //Source folder path
        string _srcPath;
        //Verbose mode or not
        bool _verbose;
        long _scanCtr;
        /// <summary>
        /// Total records scanned from all files in source folder
        /// </summary>        
        public long ScanCount
        {
            get { return _scanCtr; }
        }
        long _resultsCtr;
        /// <summary>
        /// Total results found from all files in source folder
        /// </summary>
        public long ResultsCount
        {
            get { return _resultsCtr; }
        }

        private Search(bool verbose)
        {
            _verbose = verbose;
        }
        /// <summary>
        /// Search from source folder
        /// </summary>
        /// <param name="verbose">Verbose mode</param>
        /// <param name="srcFilePath">Source files folder</param>
        public Search(bool verbose, string srcFilePath) : this(verbose)
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
        /// <summary>
        /// Scan for records in source files that match the fitlers
        /// </summary>
        /// <param name="filterFile">Filter file path</param>
        /// <param name="outputPath">Output folder path</param>
        public void ScanFolder(string filterFile, string outputPath)
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
                bool writeCsvHeader = !File.Exists(outputPath);
                // create output file                
                if (!string.IsNullOrEmpty(outputPath))
                {
                    wtr = new StreamWriter(outputPath, true);
                }
                StringComparer strComparer = new StringComparer();
                // scan source files
                string[] sourceFiles = Directory.GetFiles(_srcPath);
                _scanCtr = 0;
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    var foundCtr = ScanFile(sourceFiles[i], filters, wtr, out int readCtr, strComparer);
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
                // display folder scan & output progress
                TimeSpan ts = DateTime.Now - stTime;
                Console.WriteLine("");
                Console.WriteLine("Scan Time: {0}, Found: {1}/{2}, Output: {3} ", ts.TotalSeconds, _resultsCtr, _scanCtr, outputPath.PadLeft(20, '.'));
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Scan source file to filter records to an output file
        /// </summary>
        /// <param name="srcFile">CSV Source file</param>
        /// <param name="filters">List of column filters</param>
        /// <param name="outputFile">Filtered output file</param>
        /// <param name="readCtr">Total records read from source file</param>
        /// <returns>Number of records found matching the filters</returns>
        public int ScanFile(string srcFile, Dictionary<int, string> filters, StreamWriter csvWtr, out int readCtr, StringComparer strComparer)
        {
            readCtr = 0;
            int resultsCtr = 0;
            try
            {
                // search source file
                using (StreamReader rdr = new StreamReader(srcFile))
                {
                    // read the header line
                    var csvHeader = rdr.ReadLine();
                    // read data lines
                    string line;
                    while ((line = rdr.ReadLine()) != null)
                    {
                        readCtr++;
                        var fields = line.Split(',');
                        try
                        {
                            if (ScanRecord(filters, fields, strComparer))
                            {
                                resultsCtr++;
                                if (csvWtr != null)
                                {
                                    csvWtr.WriteLine(line);
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
                // flush output records found till now
                csvWtr.Flush();
                if (_verbose)
                {
                    // display file scan and output progress
                    var fileName = "/" + Path.GetFileName(srcFile);
                    Console.WriteLine("Source File: {0}, Found:{1}/{2}", fileName.PadLeft(20, '.'), resultsCtr.ToString().PadLeft(7), readCtr.ToString().PadLeft(7));
                }
            }
        }
        /// <summary>
        /// Compare csv record with filters
        /// </summary>
        /// <param name="filters">List of filters</param>
        /// <param name="fields">Array of data</param>
        /// <param name="strComparer">Compare logic</param>
        /// <returns>True if it passes else false</returns>
        public bool ScanRecord(Dictionary<int, string> filters, string[] fields, StringComparer strComparer)
        {
            bool checksPassed = true;
            try
            {
                foreach (int fieldIdx in filters.Keys)
                {
                    var field = fields[fieldIdx].Trim();
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
