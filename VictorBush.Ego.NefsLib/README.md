# NefsLib

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

## Design Notes

Below are some general notes on design philosophy for NefsLib.
- Utilize dependency injection when applicable.
    - For example, file system abstractions and temporary working directories are supplied to the library.
- Limit class mutability.
    - Things tend to be easier to reason about when there is no ambiguity about what can modify a certain property of a class.
    - For example, items are limited in the amount of data that can change after the object is constructed. To change data, a new item must be constructed.
    - The writer returns a new list of items and a new archive object (rather than modifying the archive that was input). This is beneficial in situations, for example, where there is an error is writing - the original object is not left in a partially updated state.
    - This is not an absolute, sometimes mutability makes sense.
- Wait to apply changes to an items list until the archive is written.
    - Replacement files aren't compressed until the writer begins to write.
    - This allows an application to queue up multiple changes and support undo/redo before triggering a save.
- Be aware of an item id vs index.
    - Types of identifiers:
        - Id - used by a NeFS archive to identify items and reference parent/sibling relationships (i.e., match a file to its directory).
        - SharedInfo - an index value stored in part 1 used to index into part 2 and 7.
        - FirstBlock - an index value stored in part 1 used to index into part 4.
    - Header part 1 (entry table) contains a list of all items. Each item has an id (index).
        - The number of entries is equal to the number of items specified in the header intro.
        - It is possible for an item to have duplicates sharing some of the same data.
    - Header part 2 (shared entry info table) data is accessed using the SharedInfo value.
        - The number of entries in part 1 is not guaranteed to equal the number of entries in part 2.
        - Items are sorted by dept-first directory structure (sorted by file name).
    - Header part 4 (block table) data is accessed using the FirstBlock value.
    - Header part 6 (writable entry info table) data is ordered the same as part 1 and has the same number of entries.
    - Header part 7 (writable shared entry info table) data is ordered the same as part 2 and has the same number of entries.
    - The NefsItemList can be enumerated in multiple ways:
        - by id.
        - by a depth-first traversal of the directory structure with children sorted by file name.
        - by a depth-first traversal of the directory structure with children sorted by id.
    - The "First Child Id" and "Sibling Id" fields are based on the depth-first traversal, but with children sorted by id.
- Use the Microsoft logging abstractions.
    - Logging is configured by the consumer of the library by providing a static ILoggerFactory to NefsLog.LoggerFactory. If none is provided, a NullLoggerFactory is used.
- Have some unit tests.
    - And at least keep them passing.
    - Write tests where they make sense.

### Questions
- Can items in an archive be out of order in terms of parent/child relationship? i.e., can a file have a lower id number than its parent directory? Currently the assumption is "no".
