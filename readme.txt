NeFS Edit

*** Use at your own risk. Backup your files. ***

Version 0.1.1
- Fixed some issues related to replacing files in archive with larger files.

Version 0.1.0

Initial release. Limited functionality and little polish.

Features:
- Open a NeFS archive and browse contents.
- Extract files (individually) from an archive.
- Replace files (individually) in an archive.
- Save archive to a chosen location.

What's NOT supported:
- A lot of things.
- Multiple extraction of files, extraction of directories, etc.
- Saving an archive in place (can do a Save As and overwrite the existing file though).
- Sorting items in editor.
- Adding items to an archive.
- Creating new archives from scratch.
- A lot more.

Other notes:
- Extracting files and replacing files are not as efficient as they could be. Currently
    the application uses offzip/packzip and simply spawns new processes. When replacing
	large files, this is very slow since we have to compress each chunk. Lots of room
	for improvement here since this work can be parallelized.
- game.nefs not guaranteed to work.
- Items aren't sorted by type or by filename.

Credits:
- This application utilizes offzip.exe and packzip.exe.
	- http://aluigi.altervista.org/mytoolz.htm
	- NeFS Edit is not afiliated with the author of these utilities.
