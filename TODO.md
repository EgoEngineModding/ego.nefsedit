# TODO
- Logging verbosity options (i.e., settings save/lod, etc can be debug). Cleanup info/dbg/wrn/error usage.
- Save window position, size, pane locations, etc for next startup.
- Console logging performance issues (https://github.com/victorbush/ego.nefsedit/issues/8).
- Fix verifying hash for game.dat headers (https://github.com/victorbush/ego.nefsedit/issues/9).
- Fix sorting by id in debug view and allow resetting sorting order (https://github.com/victorbush/ego.nefsedit/issues/11).
- Allow associating .nefs files with the app, and allow opening an archive via command line.
- Add protection against saving over an archive in a game's directory (or always force a "Save As").
- Extracting non-compressed files from encrypted archives?
- Fix part 3 writing. Need to append bytes to end to get on a 4 byte boundary (?)