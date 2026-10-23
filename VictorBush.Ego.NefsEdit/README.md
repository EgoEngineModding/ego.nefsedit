# NeFS Edit

NeFS Edit is an application that supports browsing, extracting, and modifying contents of NeFS archives. These archive files are used by Ego Engine games such as DiRT 4 and DiRT Rally 2.

Requires .NET 8 Desktop Runtime.

## Features
- Open and browse NeFS archives (including encrypted archives).
- Open and browse game.dat archives (DiRT Rally 2 and DiRT 4).
- Extract files and directories from archives.
- Replace files in an archive.
- Save archives, including multi-volume NeFS 1.5.1 archives.
- Recalculate the 1.5.1 header checksum and per-chunk checksums when saving.

### Multi-volume archives
Saving an archive with more than one volume rebuilds the primary volume and copies the secondary volume files next to the destination. Unchanged items in secondary volumes keep their original data and offsets. New or replaced items are written to the primary volume.

The 1.5.1 header checksum is recalculated for XOR-encoded headers, such as the PS3 variant.

### Supported Versions
A table showing which app version introduced support for each NeFS archive version.

| NeFS Version | Read | Write |
|--------------|------|-------|
| 0.1.0        | 0.7  | X     |
| 0.2.0        | 0.7  | X     |
| 1.3.0        | 0.7  | X     |
| 1.4.0        | 0.7  | X     |
| 1.5.0        | 0.7  | X     |
| 1.5.1        | 0.7  | 0.7   |
| 1.6.0        | 0.6  | 0.7   |
| 2.0.0        | 0.1  | 0.1   |

Multi-volume writing and XOR-encoded 1.5.1 checksum recalculation were added on the `vers9` branch.

## Limitations
- Saving encrypted archives is not supported. This would require access to the private key originally used by the game developers.
- Saving game.dat files is not supported. The header information for game.dat files is stored in the game executable. Modifying the executable will not be supported.
- Creating new archives from scratch is not supported.
- Adding new items to an archive is not supported.
- Multi-volume saving rebuilds only the primary volume. Secondary volumes are copied as-is; unchanged items in them are not recompressed.
- The writer does not pad the primary volume to the NeFS block size. Games that require a block-aligned archive may need the file padded externally.
- Opening game.nefs files is supported, but some items in those archives do not get recognized properly. Saving game.nefs files will most likely result in a corrupted archive.

## Support

This application is provided as-is, without any support; use at your own risk. However, feel free to report issues to the GitHub project: https://github.com/victorbush/ego.nefsedit.
