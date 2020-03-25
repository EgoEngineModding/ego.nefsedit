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
- Be aware of an item id vs a generic list index
	- When dealing with item metadata, the library keeps them sorted by id.
	- The NefsItemList is sorted and indexed by item id, as are the different header part classes (NefsHeaderPart*X*). Data is stored sorted dictionaries (sorted by id). That way entries are enumerated by id.
	- There are no guarantees on item order in the archive header (for example, parts 1 and 2 are not necessarily sorted by id). However, when NefsLib writes this metadata, it will have the entries sorted by id.
	- Header part 4 is an example of the difference between an item id and an index. Not every item has an entry in part 4. The entries in part 4 are accessed by indices into the list, not by item id.
- Use the Microsoft logging abstractions.
	- Logging is configured by the consumer of the library by providing a static ILoggerFactory to NefsLog.LoggerFactory. If none is provided, a NullLoggerFactory is used.
- Have some unit tests.
	- And at least keep them passing.
	- Write tests where they make sense.

### Questions
- Can items in an archive be out of order in terms of parent/child relationship? i.e., can a file have a lower id number than its parent directory? Currently the assumption is "no".