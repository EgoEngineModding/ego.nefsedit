# NeFS Edit

NeFS Edit allows opening and modifying NeFS archive files. These archive files are used by Ego Engine games such as DiRT 4 and DiRT Rally 2.

There are two components to this project.
- NefsLib - a C# library for working with NeFS archives.
- NefsEdit - a Windows Forms application that uses NefsLib.

## TODO
- Add undo/redo commands.
- Check for modified archive state. If not modified, disable save option.
- Show item state in app - replaced items become a certain color, deleted items become red, etc.
- Show/hide "Item" menu if an item is/isn't selected.
- Add a recent files list.
- Allow associating .nefs files with the app, and allow opening an archive via command line.
- Add protection against saving over an archive in a game's directory (or always force a "Save As").
- Evaluate NefsProgress progress reporting.

## Acknowledgements
Special thanks to [@Gigi1237]( https://github.com/Gigi1237 ) for encryption work.