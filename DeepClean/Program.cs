using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeepClean;

internal class Program
{
    private static int deletedFolders = 0;
    private static long freedSpace = 0;
    private static List<string> errors = new();
    private static bool dryRun = false;
    private static bool autoConfirm = false;

    static int Main(string[] args)
    {
        Console.WriteLine("DeepClean - Recursively delete bin and obj folders");
        Console.WriteLine("==================================================");
        Console.WriteLine();

        // Parse arguments
        ParseArguments(args);

        // Get the starting directory
        string startPath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Starting directory: {startPath}");
        Console.WriteLine();

        try
        {
            // Find all bin and obj folders
            var foldersToDelete = FindFoldersToDelete(startPath);

            if (foldersToDelete.Count == 0)
            {
                Console.WriteLine("No bin or obj folders found.");
                return 0;
            }

            Console.WriteLine($"Found {foldersToDelete.Count} folder(s) to delete:");
            foreach (var folder in foldersToDelete)
            {
                Console.WriteLine($"  - {folder}");
            }
            Console.WriteLine();

            if (dryRun)
            {
                Console.WriteLine("DRY RUN MODE - No folders will be deleted.");
                return 0;
            }

            // Ask for confirmation
            if (!autoConfirm && !ConfirmDeletion())
            {
                Console.WriteLine("Operation cancelled.");
                return 0;
            }

            // Delete the folders
            Console.WriteLine("Deleting folders...");
            foreach (var folder in foldersToDelete)
            {
                DeleteFolder(folder);
            }

            // Print summary
            Console.WriteLine();
            Console.WriteLine("Summary:");
            Console.WriteLine($"  Deleted: {deletedFolders} folder(s)");
            Console.WriteLine($"  Freed space: {FormatBytes(freedSpace)}");

            if (errors.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Errors encountered:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                return 1; // Return error code if there were errors
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    static void ParseArguments(string[] args)
    {
        foreach (var arg in args)
        {
            switch (arg.ToLower())
            {
                case "--dry-run":
                case "-d":
                    dryRun = true;
                    break;
                case "--yes":
                case "-y":
                    autoConfirm = true;
                    break;
                case "--help":
                case "-h":
                    PrintHelp();
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine($"Warning: Unknown argument '{arg}' ignored.");
                    break;
            }
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("Usage: deepclean [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -d, --dry-run    Show what would be deleted without actually deleting");
        Console.WriteLine("  -y, --yes        Skip confirmation prompt and delete automatically");
        Console.WriteLine("  -h, --help       Show this help message");
        Console.WriteLine();
        Console.WriteLine("Description:");
        Console.WriteLine("  Recursively finds and deletes all 'bin' and 'obj' folders from the");
        Console.WriteLine("  current directory downwards. Useful for cleaning up .NET projects.");
    }

    static List<string> FindFoldersToDelete(string rootPath)
    {
        var foldersToDelete = new List<string>();

        try
        {
            SearchDirectory(rootPath, foldersToDelete);
        }
        catch (UnauthorizedAccessException ex)
        {
            errors.Add($"Access denied to directory: {rootPath} - {ex.Message}");
        }
        catch (Exception ex)
        {
            errors.Add($"Error searching directory {rootPath}: {ex.Message}");
        }

        return foldersToDelete;
    }

    static void SearchDirectory(string path, List<string> foldersToDelete)
    {
        try
        {
            // Check if current directory is bin or obj
            string dirName = Path.GetFileName(path);
            if (dirName.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("obj", StringComparison.OrdinalIgnoreCase))
            {
                foldersToDelete.Add(path);
                return; // Don't search inside bin/obj folders
            }

            // Search subdirectories
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                SearchDirectory(directory, foldersToDelete);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we don't have access to
            errors.Add($"Access denied: {path}");
        }
        catch (PathTooLongException)
        {
            errors.Add($"Path too long: {path}");
        }
        catch (Exception ex)
        {
            errors.Add($"Error accessing {path}: {ex.Message}");
        }
    }

    static bool ConfirmDeletion()
    {
        Console.Write("Do you want to delete these folders? (y/n): ");
        var response = Console.ReadLine()?.Trim().ToLower();
        return response == "y" || response == "yes";
    }

    static void DeleteFolder(string path)
    {
        try
        {
            // Calculate size before deletion
            long size = CalculateDirectorySize(path);

            // Try to delete the folder
            Directory.Delete(path, recursive: true);

            deletedFolders++;
            freedSpace += size;
            Console.WriteLine($"  ✓ Deleted: {path}");
        }
        catch (UnauthorizedAccessException)
        {
            errors.Add($"Access denied (may require elevation): {path}");
            Console.WriteLine($"  ✗ Access denied: {path}");
        }
        catch (IOException ex)
        {
            // This often happens with file locks
            errors.Add($"I/O error (possibly locked files): {path} - {ex.Message}");
            Console.WriteLine($"  ✗ I/O error: {path}");
        }
        catch (Exception ex)
        {
            errors.Add($"Error deleting {path}: {ex.Message}");
            Console.WriteLine($"  ✗ Error: {path}");
        }
    }

    static long CalculateDirectorySize(string path)
    {
        try
        {
            return CalculateDirectorySizeRecursive(path);
        }
        catch
        {
            return 0; // If we can't calculate, return 0
        }
    }

    static long CalculateDirectorySizeRecursive(string path)
    {
        long size = 0;
        try
        {
            // Calculate size of files in current directory
            var dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.GetFiles())
            {
                try
                {
                    size += file.Length;
                }
                catch
                {
                    // Skip files we can't access
                }
            }

            // Recursively calculate size of subdirectories
            foreach (var subDir in dirInfo.GetDirectories())
            {
                size += CalculateDirectorySizeRecursive(subDir.FullName);
            }
        }
        catch
        {
            // Skip directories we can't access
        }

        return size;
    }

    static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
