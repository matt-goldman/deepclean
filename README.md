# DeepClean

[![CI](https://github.com/matt-goldman/deepclean/actions/workflows/ci.yml/badge.svg)](https://github.com/matt-goldman/deepclean/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/DeepClean.svg)](https://www.nuget.org/packages/DeepClean/)

_"Somebody had to, right?"_

A cross-platform .NET global tool that recursively deletes `bin` and `obj` folders from your current directory. Perfect for cleaning up .NET projects and freeing disk space.

## Features

- 🚀 **Fast & Efficient**: Quickly finds and removes all bin/obj folders recursively
- 🌍 **Cross-Platform**: Works on Windows, Linux, and macOS
- 🛡️ **Safe**: Asks for confirmation before deletion (can be skipped with `-y`)
- 🔍 **Dry-Run Mode**: Preview what would be deleted without actually deleting
- 💪 **Robust Error Handling**: Gracefully handles file locks, permission issues, and other errors
- 📊 **Detailed Summary**: Shows how many folders were deleted and how much space was freed

## Installation

Install as a global .NET tool:

```bash
dotnet tool install -g DeepClean
```

Update to the latest version:

```bash
dotnet tool update -g DeepClean
```

Uninstall:

```bash
dotnet tool uninstall -g DeepClean
```

## Usage

Navigate to the root directory of your .NET solution or project(s) and run:

```bash
deepclean
```

### Options

| Option | Description |
|--------|-------------|
| `-d`, `--dry-run` | Preview what would be deleted without actually deleting |
| `-y`, `--yes` | Skip confirmation prompt and delete automatically |
| `-h`, `--help` | Show help message |

### Examples

**Preview what would be deleted:**
```bash
deepclean --dry-run
```

**Delete without confirmation:**
```bash
deepclean --yes
```

**Interactive mode (default):**
```bash
deepclean
```

This will show you all the folders to be deleted and ask for confirmation.

## Sample Output

```
DeepClean - Recursively delete bin and obj folders
==================================================

Starting directory: /home/user/projects/MyApp

Found 4 folder(s) to delete:
  - /home/user/projects/MyApp/src/MyApp.Api/bin
  - /home/user/projects/MyApp/src/MyApp.Api/obj
  - /home/user/projects/MyApp/src/MyApp.Core/bin
  - /home/user/projects/MyApp/src/MyApp.Core/obj

Do you want to delete these folders? (y/n): y
Deleting folders...
  ✓ Deleted: /home/user/projects/MyApp/src/MyApp.Api/bin
  ✓ Deleted: /home/user/projects/MyApp/src/MyApp.Api/obj
  ✓ Deleted: /home/user/projects/MyApp/src/MyApp.Core/bin
  ✓ Deleted: /home/user/projects/MyApp/src/MyApp.Core/obj

Summary:
  Deleted: 4 folder(s)
  Freed space: 1.23 GB
```

## Error Handling

DeepClean handles various error scenarios gracefully:

- **File Locks**: If files are locked by another process, the error is reported but the tool continues
- **Permission Issues**: If you don't have permission to delete a folder, it's reported as an error
- **Path Length Issues**: Handles long paths that exceed OS limitations
- **Access Denied**: Reports directories that require elevation or special permissions

All errors are collected and displayed at the end of execution.

## Building from Source

```bash
git clone https://github.com/matt-goldman/deepclean.git
cd deepclean/DeepClean
dotnet build
dotnet run -- --help
```

## Publishing

To create a NuGet package:

```bash
dotnet pack -c Release
```

The GitHub Actions workflow automatically publishes to NuGet when a release is created.

## License

MIT License - see [LICENSE](LICENSE) for details
