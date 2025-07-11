# Changelog

## Unreleased
- Added support for reading NeFS versions 0.1.0, 0.2.0, 1.3.0, 1.4.0, 1.5.0, 1.5.1
- Added support for reading split data and multiple volume archives
- Added support for reading big-endian NeFS headers
- Improved accuracy of header auto-search in game executables, added support for elf and mach-o (Linux, Mac, PS3 exe)

## Version 0.6.0
- Added support for NeFS version 1.6.
- Added recent files list and improved open file dialog.
- Added support for handling duplicate items in archives.
- Improved finding and loading NeFS headers from executables.
- Fixed various loading bugs for NeFS version 2.0 headers.

## Version 0.5.0
- Added ability to open game.dat files (archives with separate header/data files).
- Added ability to close an archive.
- Added archive and item debug output views.
- Fixed "Extract To" to extract directly to the location specified.
- Fixed/improved interpretation of various NeFS header fields.
- Replaced Archive Details pane with an Archive Debug view.
- Refactored NefsEdit and NefsLib substantially.
- Improved application/library separation.
- Added various unit tests.
- Switched to Serilog for logging.

## Version 0.4.0
- Added ability to open encrypted NeFS archives.
- Fixed issue with extracting non-compressed files.

## Version 0.3.0
- Added ability to extract multiple files at once.
- Added ability to extract directories.
- Added Quick Extract feature.
- Fixed some issues with loading and replacing items from game.nefs.

## Version 0.2.0
- Added context menu when right-clicking items.
- Added keyboard shortcut keys.
- Added ability to save archive in place (in addition to Save As).
- Using .NET compression library for extraction now.
- Removed dependency on offzip and packzip.

## Version 0.1.2
- Using .NET compression library instead of spawning packzip processes.

## Version 0.1.1
- Fixed some issues related to replacing files in archive with larger files.

## Version 0.1.0
- Initial release. Limited functionality and little polish.
