using System.Reflection;
using Mapster;

namespace MapsterChecker;

public class MapsterValidator
{
    public static async Task<bool> ValidateAllMappingsAsync(string solutionPath)
    {
        Console.WriteLine("🔍 Mapster Validation Report");
        Console.WriteLine("===========================\n");

        // Find all Adapt invocations using Roslyn
        var invocations = await AdaptInvocationFinder.FindAdaptInvocationsAsync(solutionPath);
        
        // Load assemblies
        var assemblyLoader = new AssemblyLoader();
        await assemblyLoader.LoadAssembliesFromSolutionAsync(solutionPath);

        // Show all found invocations
        ShowAllInvocations(invocations);

        // Validate mappings
        var allValid = ValidateMappings(invocations);

        Console.WriteLine($"\n📊 Summary: {(allValid ? "✅ All mappings valid" : "❌ Some mappings failed")}");
        return allValid;
    }

    private static void ShowAllInvocations(List<AdaptInvocationFinder.AdaptInvocation> invocations)
    {
        Console.WriteLine($"📋 Found {invocations.Count} Adapt Invocation(s)");
        Console.WriteLine("=====================================");
        
        if (invocations.Count == 0)
        {
            Console.WriteLine("No .Adapt() calls found in codebase.\n");
            return;
        }

        for (int i = 0; i < invocations.Count; i++)
        {
            var inv = invocations[i];
            var fileName = Path.GetFileName(inv.FilePath);
            Console.WriteLine($"{i + 1}. {fileName}:{inv.LineNumber}");
            Console.WriteLine($"   {inv.FullInvocation}");
            
            if (inv.SourceType != null && inv.DestinationType != null)
            {
                var sourceTypeName = GetSimpleTypeName(inv.SourceType);
                var destTypeName = GetSimpleTypeName(inv.DestinationType);
                Console.WriteLine($"   {sourceTypeName} → {destTypeName}");
            }
            else
            {
                Console.WriteLine($"   ⚠️ Could not resolve types");
            }
            Console.WriteLine();
        }
    }

    private static bool ValidateMappings(List<AdaptInvocationFinder.AdaptInvocation> invocations)
    {
        Console.WriteLine("🔍 Validation Results");
        Console.WriteLine("====================");

        try
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Compile();
            Console.WriteLine("✅ TypeAdapterConfig compilation successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TypeAdapterConfig compilation failed: {ex.Message}");
            return false;
        }

        var allValid = true;
        var uniqueMappings = GetUniqueMappings(invocations);
        
        if (uniqueMappings.Count == 0)
        {
            Console.WriteLine("No mappings to validate.\n");
            return true;
        }

        Console.WriteLine($"Validating {uniqueMappings.Count} unique mapping(s):\n");

        foreach (var (sourceType, destType) in uniqueMappings)
        {
            var success = ValidateMapping(sourceType, destType);
            var result = success ? "✅" : "❌";
            Console.WriteLine($"{result} {sourceType.Name} → {destType.Name}");
            
            if (!success)
                allValid = false;
        }

        Console.WriteLine();
        return allValid;
    }

    private static List<(Type source, Type dest)> GetUniqueMappings(List<AdaptInvocationFinder.AdaptInvocation> invocations)
    {
        var uniqueMappings = new HashSet<(Type, Type)>();
        
        foreach (var invocation in invocations)
        {
            if (invocation.SourceType == null || invocation.DestinationType == null)
                continue;
                
            var sourceType = ResolveType(invocation.SourceType);
            var destType = ResolveType(invocation.DestinationType);
            
            if (sourceType != null && destType != null)
                uniqueMappings.Add((sourceType, destType));
        }
        
        return uniqueMappings.ToList();
    }

    private static bool ValidateMapping(Type sourceType, Type destType)
    {
        try
        {
            var sourceInstance = CreateDefaultInstance(sourceType);
            if (sourceInstance == null)
                return false;
                
            sourceInstance.Adapt(destType);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetSimpleTypeName(string fullTypeName)
    {
        var parts = fullTypeName.Split('.');
        return parts.Last();
    }

    private static bool ValidateUsingCompilation(List<AdaptInvocationFinder.AdaptInvocation> invocations,
        HashSet<(Type, Type)> testedMappings)
    {
        Console.WriteLine("📋 Method 1: Compilation-based validation");
        Console.WriteLine("==========================================");

        try
        {
            // Get the global config and try to compile it
            var config = TypeAdapterConfig.GlobalSettings;
            config.Compile();

            Console.WriteLine("✅ TypeAdapterConfig compilation successful");

            var allValid = true;

            foreach (var invocation in invocations)
            {
                if (invocation.SourceType == null || invocation.DestinationType == null)
                    continue;

                var sourceType = ResolveType(invocation.SourceType);
                var destType = ResolveType(invocation.DestinationType);

                if (sourceType == null || destType == null)
                    continue;

                var mappingKey = (sourceType, destType);
                if (testedMappings.Contains(mappingKey))
                    continue;

                testedMappings.Add(mappingKey);

                // Check if there's a specific rule for this mapping
                var hasRule =
                    config.RuleMap.Any(kvp => kvp.Key.Source == sourceType && kvp.Key.Destination == destType);

                Console.WriteLine($"📍 {sourceType.Name} -> {destType.Name}");
                Console.WriteLine($"   Has explicit rule: {(hasRule ? "✅" : "❌")}");

                if (!hasRule)
                {
                    Console.WriteLine("   ⚠️  No explicit mapping rule found");
                    allValid = false;
                }
                
                
            }

            Console.WriteLine();
            return allValid;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Compilation failed: {ex.Message}");
            return false;
        }
    }

    private static bool ValidateUsingDirectMapping(List<AdaptInvocationFinder.AdaptInvocation> invocations,
        HashSet<(Type, Type)> testedMappings)
    {
        Console.WriteLine("🎯 Method 2: Direct mapping validation");
        Console.WriteLine("======================================");

        var allValid = true;

        foreach (var invocation in invocations)
        {
            if (invocation.SourceType == null || invocation.DestinationType == null)
                continue;

            var sourceType = ResolveType(invocation.SourceType);
            var destType = ResolveType(invocation.DestinationType);

            if (sourceType == null || destType == null)
                continue;

            var mappingKey = (sourceType, destType);
            if (testedMappings.Contains(mappingKey))
                continue;

            testedMappings.Add(mappingKey);

            Console.WriteLine($"📍 Testing: {sourceType.Name} -> {destType.Name}");

            try
            {
                // Create a default instance and try to map it
                var sourceInstance = CreateDefaultInstance(sourceType);
                if (sourceInstance != null)
                {
                    sourceInstance.Adapt(destType);
                    Console.WriteLine("   ✅ Mapping successful");
                }
                else
                {
                    Console.WriteLine("   ⚠️  Could not create source instance");
                    allValid = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Mapping failed: {ex.Message}");
                allValid = false;
            }
        }

        Console.WriteLine();
        return allValid;
    }

    private static void ShowConfigurationIssues()
    {
        Console.WriteLine("⚙️  Configuration Analysis");
        Console.WriteLine("==========================");

        var config = TypeAdapterConfig.GlobalSettings;

        Console.WriteLine($"Total rules configured: {config.RuleMap.Count}");
        Console.WriteLine("Default settings:");
        Console.WriteLine($"  - RequireExplicitMapping: {config.RequireExplicitMapping}");
        Console.WriteLine($"  - RequireDestinationMemberSource: {config.RequireDestinationMemberSource}");
        Console.WriteLine($"  - IgnoreNullValues: {new Func<bool, object>(config.Default.IgnoreNullValues)}");

        if (config.RuleMap.Count > 0)
        {
            Console.WriteLine("\nConfigured mappings:");
            foreach (var rule in config.RuleMap.Take(10).ToList())
                Console.WriteLine($"  - {rule.Key.Source.Name} -> {rule.Key.Destination.Name}");

            if (config.RuleMap.Count > 10)
                Console.WriteLine($"  ... and {config.RuleMap.Count - 10} more");
        }

        Console.WriteLine();
    }

    private static Type? ResolveType(string typeName)
    {
        // Try to resolve the type from loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }

    private static object? CreateDefaultInstance(Type type)
    {
        try
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            if (type == typeof(string))
                return string.Empty;

            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
                return Activator.CreateInstance(type);

            return null;
        }
        catch
        {
            return null;
        }
    }
}

public class AssemblyLoader
{
    public Task<List<Assembly>> LoadAssembliesFromSolutionAsync(string solutionPath)
    {
        var assemblies = new List<Assembly>();

        // For now, just return currently loaded assemblies
        // In a more complete implementation, you could build the solution
        // and load the resulting assemblies

        assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)).ToList());

        return Task.FromResult(assemblies);
    }
}