# NeFS Edit

NeFS Edit allows opening and modifying NeFS archive files. These archive files are used by Ego Engine games such as DiRT 4 and DiRT Rally 2.

There are two components to this project.
- NefsLib - a C# library for working with NeFS archives.
- NefsEdit - a Windows Forms application that uses NefsLib.

## TODO
- Add a recent files list.
- Console logging performance issues.
- Allow associating .nefs files with the app, and allow opening an archive via command line.
- Add protection against saving over an archive in a game's directory (or always force a "Save As").
- Fix sorting by id in debug view (and allow resetting sorting order?)
- Extracting non-compressed files from encrypted archives?
- Check if the part 6 field has a flag indicating if data is encrypted.
- Fix verifying hash for game.dat headers.
- Fix part 3 writing. Need to append bytes to end to get on a 4 byte boundary (?)

## Acknowledgements
Special thanks to [@Gigi1237]( https://github.com/Gigi1237 ) for encryption work.