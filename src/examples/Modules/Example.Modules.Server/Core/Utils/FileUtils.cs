namespace Example.Modules.Server.Core.Utils;

/// <summary>
///  The file utils 
/// </summary>
public class FileUtils
{
    /// <summary>
    ///  Searches the file. 
    /// </summary>
    /// <param name="directoryPath"> The directory path. </param>
    /// <param name="fileName"> The file name. </param>
    /// <returns> The file path. </returns>
    public static string SearchFile(string directoryPath, string fileName)
    {
        var files = Directory.GetFiles(directoryPath, fileName, SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
        {
            return files[0];
        }

        string filePath = null;
        var subdirectories = Directory.GetDirectories(directoryPath);
        foreach (var subdirectory in subdirectories)
        {
            filePath = SearchFile(subdirectory, fileName);
            if (!string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }
        }
        
        if (filePath is null) throw new FileNotFoundException($"File not found in {directoryPath}");
        
        return filePath;
    }
}