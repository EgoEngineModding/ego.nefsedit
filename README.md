# NeFS Edit

NeFS Edit allows opening and modifying NeFS archive files. These archive files are used by Ego Engine games such as DiRT 4 and DiRT Rally 2.

There are two components to this project.
- NefsLib - a C# library for working with NeFS archives.
- NefsEdit - a Windows Forms application that uses NefsLib.

## TODO
- Fix archive saving.
- Fix list view items not being updated after save.
- Verify hash for encrypted nefs.
- Add a recent files list.
- Allow associating .nefs files with the app, and allow opening an archive via command line.
- Add protection against saving over an archive in a game's directory (or always force a "Save As").
- Evaluate NefsProgress progress reporting (and the performance hit for using Dispatcher to sync with UI thread).
- Fix sorting by id (and allow resetting sorting order?)

## Acknowledgements
Special thanks to [@Gigi1237]( https://github.com/Gigi1237 ) for encryption work.