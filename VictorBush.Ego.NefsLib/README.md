# NeFS Library

Library for reading and writing NeFS archive files.

## Usage

```csharp
using System.IO.Abstractions;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Progress;

// Dependencies
var fileSystem = new FileSystem();
var compressor = new NefsCompressor(fileSystem);
var progress = new NefsProgress();

// Read
var reader = new NefsReader(fileSystem);
var archive = await reader.ReadArchiveAsync(@"C:\archive.nefs", progress);

// Write
var writer = new NefsWriter(@"C:\temp", fileSystem, compressor);
archive = await writer.WriteArchiveAsync(@"C:\dest.nefs", archive, progress);
```
