# TODO
- Fix p6 and p7 order --- 
#part 6 == #part 1
#part 7 == #part 2

- Remove EntriesById and cleanup Id vs index.
- Fix game.dat extract
- Chunk size source rather than hard code?
- Logging verbosity options (i.e., settings save/lod, etc can be debug)

- Add a recent files list (https://github.com/victorbush/ego.nefsedit/issues/7).
- Console logging performance issues (https://github.com/victorbush/ego.nefsedit/issues/8).
- Check if the part 6 field has a flag indicating if data is encrypted (https://github.com/victorbush/ego.nefsedit/issues/10).
- Fix verifying hash for game.dat headers (https://github.com/victorbush/ego.nefsedit/issues/9).
- Fix sorting by id in debug view and allow resetting sorting order (https://github.com/victorbush/ego.nefsedit/issues/11).
- Allow associating .nefs files with the app, and allow opening an archive via command line.
- Add protection against saving over an archive in a game's directory (or always force a "Save As").
- Extracting non-compressed files from encrypted archives?
- Fix part 3 writing. Need to append bytes to end to get on a 4 byte boundary (?)