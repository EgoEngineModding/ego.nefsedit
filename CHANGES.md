
# Changelog

## Version 0.5.0
- Added ability to open game.dat files (archives with separate header/data files).
- Added ability to close an archive.
- Added archive and item debug output views.
- Fixed "Extract To" to extract directly to the location specified.
- Replaced Archive Details pane with an Archive Debug view.
- Refactored NefsEdit and NefsLib code for better application/library separation.
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
