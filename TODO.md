# TODO
- Add a recent files list.
- Console logging performance issues.
- Allow associating .nefs files with the app, and allow opening an archive via command line.
- Add protection against saving over an archive in a game's directory (or always force a "Save As").
- Fix sorting by id in debug view (and allow resetting sorting order?)
- Extracting non-compressed files from encrypted archives?
- Check if the part 6 field has a flag indicating if data is encrypted.
- Fix verifying hash for game.dat headers.
- Fix part 3 writing. Need to append bytes to end to get on a 4 byte boundary (?)