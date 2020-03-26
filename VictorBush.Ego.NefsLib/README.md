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
	- Some header parts contain a list of item metadata. This list can be accessed by an index into this list.
		- The list is not guaranteed to be sorted by item id.
		- The part 1 entry for an item contains the index to use to access the item metadata in other parts.
	- The header part classes (NefsHeaderPart*X*) retain the order of data when a header is read.
		- They provide two ways to access this data:
			- A list of entries that retains the order as read in from the header. This allows enumerating the entries by index.
			- A dictionary keyed by item id and sorted by item id. This allows enumerating the entries by item id.
	- The NefsItemList is sorted and indexed by item id.
	- When NefsLib writes a header, it will write the entries sorted by id.
- Use the Microsoft logging abstractions.
	- Logging is configured by the consumer of the library by providing a static ILoggerFactory to NefsLog.LoggerFactory. If none is provided, a NullLoggerFactory is used.
- Have some unit tests.
	- And at least keep them passing.
	- Write tests where they make sense.

### Questions
- Can items in an archive be out of order in terms of parent/child relationship? i.e., can a file have a lower id number than its parent directory? Currently the assumption is "no".