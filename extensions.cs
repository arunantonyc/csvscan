using System.IO;

namespace csvscan
{
    public class Helpers
    {
        public static string GetNewFileName(string targetPath, string filePrefix, string fileExtn)
        {
            int tempFileCtr = 1;
            string outputFile = "";
            do
            {
                outputFile = string.Format("{0}/{1}-{2}.{3}", targetPath, filePrefix, tempFileCtr, fileExtn);
                tempFileCtr++;
            } while (File.Exists(outputFile));
            return outputFile;
        }
    }
}
