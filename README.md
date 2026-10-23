# NeFS Edit

NeFS Edit allows opening and modifying NeFS archive files. These archive files are used by Ego Engine games such as DiRT 4 and DiRT Rally 2.

There are two components to this project.
- NefsLib - a C# library for working with NeFS archives.
- NefsEdit - a Windows Forms application that uses NefsLib.

## Features
- Read and browse NeFS archives, including encrypted archives (read-only) and multi-volume archives.
- Extract and replace files.
- Write NeFS 1.5.1 and 1.6.0 archives.
- Write multi-volume 1.5.1 archives. The primary volume is rebuilt; secondary volumes are copied unchanged, and new or replaced items are written to the primary volume.
- Recalculate the 1.5.1 header checksum and per-chunk checksums when saving.

## Documentation
- [NefsLib](VictorBush.Ego.NefsLib/README.md) - library usage and archive format notes.
- [NefsEdit](VictorBush.Ego.NefsEdit/README.md) - application features and limitations.
- [Changelog](CHANGES.md) - release history and recent changes.

## Acknowledgements
[@Gigi1237]( https://github.com/Gigi1237 ) - encryption/reversing contributions