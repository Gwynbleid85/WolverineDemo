namespace MapsterChecker;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Mapster Mapping Validation");
        Console.WriteLine("==============================\n");

        var solutionPath = args.Length > 0 ? args[0] : FindSolutionFile();
        
        if (string.IsNullOrEmpty(solutionPath))
        {
            Console.WriteLine("❌ No solution file found. Please provide a solution path as an argument.");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"Using solution: {solutionPath}\n");

        var allValid = await MapsterValidator.ValidateAllMappingsAsync(solutionPath);

        Console.WriteLine($"\nOverall Result: {(allValid ? "✅ All mappings valid" : "❌ Some mappings invalid")}");
        
        if (!allValid)
        {
            Environment.Exit(1);
        }
    }

    private static string? FindSolutionFile()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var solutionFiles = Directory.GetFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly);
        
        if (solutionFiles.Length == 1)
        {
            return solutionFiles[0];
        }
        
        if (solutionFiles.Length > 1)
        {
            Console.WriteLine($"Multiple solution files found. Please specify which one to use:");
            foreach (var file in solutionFiles)
            {
                Console.WriteLine($"  {Path.GetFileName(file)}");
            }
            return null;
        }
        
        var parentDir = Directory.GetParent(currentDir);
        while (parentDir != null)
        {
            solutionFiles = Directory.GetFiles(parentDir.FullName, "*.sln", SearchOption.TopDirectoryOnly);
            if (solutionFiles.Length > 0)
            {
                return solutionFiles[0];
            }
            parentDir = parentDir.Parent;
        }
        
        return null;
    }
}