namespace Tkmm.Core.Helpers;

public static class DirectoryHelper
{
    public static void DeleteTargetsFromDirectory(string targetDirectory, string[] targets, bool recursive = false)
    {
        foreach (string target in targets) {
            string absolutePath = Path.Combine(targetDirectory, target);
            DeleteTarget(absolutePath, recursive);
        }
    }
    
    private static void DeleteTarget(string target, bool recursive = false)
    {
        if (File.Exists(target)) {
            File.Delete(target);
            return;
        }
        
        if (Directory.Exists(target)) {
            Directory.Delete(target, recursive);
        }
    }

    public static void Copy(string source, string output, bool overwrite = false)
    {
        Directory.CreateDirectory(output);
        
        foreach (string sourceFile in Directory.EnumerateFiles(source)) {
            string outputFile = Path.Combine(output, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, outputFile, overwrite);
        }

        foreach (string directory in Directory.EnumerateDirectories(source)) {
            string outputDirectory = Path.Combine(output, Path.GetFileName(directory));
            Copy(directory, outputDirectory, overwrite);
        }
    }
}