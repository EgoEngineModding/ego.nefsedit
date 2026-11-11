using System.IO.Abstractions;
using System.Text;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace NefsScan;

internal static class Program
{
	private static long baseOffset;

	private static async Task<int> Main(string[] args)
	{
		// Optional global flag: --at=<offset> (decimal or 0x-prefixed hex)
		var remaining = new List<string>();
		foreach (var arg in args)
		{
			if (arg.StartsWith("--at=", StringComparison.OrdinalIgnoreCase))
			{
				var value = arg[5..];
				baseOffset = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
					? Convert.ToInt64(value[2..], 16)
					: Convert.ToInt64(value);
				continue;
			}

			remaining.Add(arg);
		}

		args = remaining.ToArray();

		if (args.Length == 0)
		{
			Console.WriteLine("usage:");
			Console.WriteLine("  NefsScan scan <file.nfs>                         lista todos os cabecalhos NeFS do arquivo");
			Console.WriteLine("  NefsScan info <archive.nfs> [--at=<offset>]");
			Console.WriteLine("  NefsScan list <archive.nfs> [filter] [--at=<offset>]");
			Console.WriteLine("  NefsScan extract <archive.nfs> <name> <out-file> [--at=<offset>]");
			Console.WriteLine("  NefsScan grep <archive.nfs> <text> [--at=<offset>]");
			Console.WriteLine("  NefsScan grepci <archive.nfs> <text> [--at=<offset>]");
			Console.WriteLine("  NefsScan cmp <a.nfs> <b.nfs>");
			Console.WriteLine("  NefsScan replace <archive.nfs> <out.nfs> <name> <file> [<name> <file> ...]");
			Console.WriteLine("  NefsScan add <archive.nfs> <out.nfs> <name> <file> [<name> <file> ...]");
			Console.WriteLine();
			Console.WriteLine("  Sem --at, o primeiro cabecalho valido do arquivo e usado automaticamente.");
			return 2;
		}

		return args[0].ToLowerInvariant() switch
		{
			"scan" when args.Length >= 2 => await Scan(args[1]),
			"probe" when args.Length >= 2 => await Probe(args[1]),
			"info" when args.Length >= 2 => await Info(args[1]),
			"save" when args.Length >= 3 => await Save(args[1], args[2]),
			"cmp" when args.Length >= 3 => await Compare(args[1], args[2]),
			"list" when args.Length >= 2 => await List(args[1], args.Length >= 3 ? args[2] : null),
			"grep" when args.Length >= 3 => await Grep(args[1], args[2]),
			"grepci" when args.Length >= 3 => await GrepCi(args[1], args[2]),
			"extract" when args.Length >= 4 => await Extract(args[1], args[2], args[3]),
			"replace" when args.Length >= 5 && args.Length % 2 == 1 => await Replace(args[1], args[2], args[3..]),
			"add" when args.Length >= 5 && (args.Length - 3) % 2 == 0 => await Add(args[1], args[2], args[3..]),
			_ => PrintUsageAndFail(),
		};
	}

	/// <summary>
	/// Lists every NeFS archive contained in the file (concatenated archives).
	/// </summary>
	private static async Task<int> Scan(string path)
	{
		var fs = new FileSystem();
		var reader = new NefsReader(fs);
		var progress = new NefsProgress();
		var size = new FileInfo(path).Length;

		Console.WriteLine($"file: {path}");
		Console.WriteLine($"size: {size:N0} bytes");
		Console.WriteLine();

		var offsets = new List<long>();
		using (var stream = File.OpenRead(path))
		{
			foreach (var offset in NefsHeaderScanner.FindHeaderOffsets(stream))
			{
				offsets.Add(offset);
			}
		}

		if (offsets.Count == 0)
		{
			Console.WriteLine("nenhum cabecalho NeFS encontrado.");
			return 1;
		}

		Console.WriteLine($"cabecalhos encontrados: {offsets.Count}");
		Console.WriteLine();
		Console.WriteLine($"{"#",4} {"offset",15} {"0x",14}  {"versao",-10} {"itens",6}  nome");

		for (var i = 0; i < offsets.Count; ++i)
		{
			var offset = offsets[i];
			string version;
			string name;
			uint count;

			try
			{
				var archive = await reader.ReadArchiveAsync(path, offset, progress);
				version = archive.Header.Version.ToString();
				count = archive.Header.NumEntries;
				var first = archive.Header.Volumes.Count > 0 ? archive.Header.Volumes[0].Name : string.Empty;
				name = string.IsNullOrWhiteSpace(first) ? "(sem nome)" : first;
			}
			catch (Exception ex)
			{
				version = "ERRO";
				count = 0;
				name = ex.GetType().Name + ": " + ex.Message;
			}

			Console.WriteLine($"{i,4} {offset,15:N0} {offset,14:X}  {version,-10} {count,6}  {name}");
		}

		return 0;
	}

	private static async Task<int> List(string archivePath, string? filter)
	{
		var archive = await LoadArchive(archivePath);
		foreach (var item in archive.Items.EnumerateById())
		{
			if (item.Type != NefsItemType.File)
			{
				continue;
			}

			if (!string.IsNullOrEmpty(filter) &&
			    !item.FileName.Contains(filter, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			var t = item.Transform;
			var transform = t is null
				? "none"
				: $"{(t.IsZlibCompressed ? "zlib " : "")}{(t.IsAesEncrypted ? "aes " : "")}{(t.IsLzssCompressed ? "lzss" : "")}".Trim();
			Console.WriteLine(
				$"id={item.Id.Value,5} vol={item.Attributes.Volume} ext={item.ExtractedSize,10} comp={item.CompressedSize,10} " +
				$"chunks={item.DataSource.Size.Chunks.Count,4} xform={transform,-9} '{item.FileName}'");
		}

		return 0;
	}

	private static async Task<int> Replace(string archivePath, string outPath, string[] nameFilePairs)
	{
		var archive = await LoadArchive(archivePath);
		var fs = new FileSystem();

		for (var i = 0; i < nameFilePairs.Length; i += 2)
		{
			var name = nameFilePairs[i];
			var replacementPath = Path.GetFullPath(nameFilePairs[i + 1]);
			if (!File.Exists(replacementPath))
			{
				Console.WriteLine($"replacement file not found: {replacementPath}");
				return 1;
			}

			var matches = archive.Items.EnumerateById()
				.Where(x => x.Type == NefsItemType.File &&
				            string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase))
				.ToList();
			if (matches.Count != 1)
			{
				Console.WriteLine($"expected exactly one item named '{name}', found {matches.Count}");
				return 1;
			}

			var item = matches[0];
			var oldSize = item.ExtractedSize;
			var newSize = (uint)new FileInfo(replacementPath).Length;
			var dataSource = new NefsFileDataSource(replacementPath, 0, new NefsItemSize(newSize), false);
			foreach (var duplicate in archive.Items.GetItemDuplicates(item.Id))
			{
				duplicate.UpdateDataSource(dataSource, NefsItemState.Replaced);
			}

			Console.WriteLine(
				$"REPLACE id={item.Id.Value} '{item.FileName}' vol={item.Attributes.Volume} " +
				$"size={oldSize} -> {newSize} duplicates={archive.Items.GetItemDuplicates(item.Id).Count}");
		}

		var outDir = Path.GetDirectoryName(Path.GetFullPath(outPath))!;
		Directory.CreateDirectory(outDir);
		var tempDir = Path.Combine(outDir, ".nefstool-tmp");
		var writer = new NefsWriter(tempDir, fs, new NefsTransformer(fs));

		Console.WriteLine($"SAVING -> {outPath}");
		var before = DateTime.Now;
		var (newArchive, _) = await writer.WriteArchiveAsync(outPath, archive, new NefsProgress());
		Console.WriteLine($"SAVE OK in {(DateTime.Now - before).TotalSeconds:F1}s");

		var transformer = new NefsTransformer(fs);
		foreach (var i in Enumerable.Range(0, nameFilePairs.Length / 2))
		{
			var name = nameFilePairs[i * 2];
			var replacementPath = Path.GetFullPath(nameFilePairs[i * 2 + 1]);
			var item = newArchive.Items.EnumerateById()
				.FirstOrDefault(x => x.Type == NefsItemType.File &&
				                     string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase));
			if (item is null)
			{
				Console.WriteLine($"VERIFY '{name}': MISSING");
				continue;
			}

			var expected = System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(replacementPath));
			var actual = await HashItemAsync(fs, transformer, item);
			var state = expected.AsSpan().SequenceEqual(actual) ? "OK" : "MISMATCH";
			Console.WriteLine(
				$"VERIFY '{name}': {state} id={item.Id.Value} vol={item.Attributes.Volume} ext={item.ExtractedSize} " +
				$"chunks={item.DataSource.Size.Chunks.Count}");
		}

		return 0;
	}

	private static async Task<int> Add(string archivePath, string outPath, string[] nameFilePairs)
	{
		var archive = await LoadArchive(archivePath);
		var fs = new FileSystem();
		var nextId = archive.Items.EnumerateById().Select(x => x.Id.Value).Max() + 1;

		for (var i = 0; i < nameFilePairs.Length; i += 2)
		{
			var name = nameFilePairs[i];
			var filePath = Path.GetFullPath(nameFilePairs[i + 1]);
			if (!File.Exists(filePath))
			{
				Console.WriteLine($"file not found: {filePath}");
				return 1;
			}

			if (archive.Items.EnumerateById().Any(x =>
				    x.Type == NefsItemType.File &&
				    string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase)))
			{
				Console.WriteLine($"item already exists: '{name}'");
				return 1;
			}

			var size = (uint)new FileInfo(filePath).Length;
			var id = new NefsItemId(nextId++);
			var dataSource = new NefsFileDataSource(filePath, 0, new NefsItemSize(size), false);
			var transform = new NefsDataTransform(0x20000, true);
			var attributes = new NefsItemAttributes { Volume = 0, IsTransformed = true };
			var item = new NefsItem(id, id, name, id, dataSource, transform, attributes, NefsItemState.Added);
			archive.Items.Add(item);
			Console.WriteLine($"ADD id={id.Value} '{name}' size={size}");
		}

		var outDir = Path.GetDirectoryName(Path.GetFullPath(outPath))!;
		Directory.CreateDirectory(outDir);
		var tempDir = Path.Combine(outDir, ".nefstool-tmp");
		var writer = new NefsWriter(tempDir, fs, new NefsTransformer(fs));

		Console.WriteLine($"SAVING -> {outPath}");
		var before = DateTime.Now;
		var (newArchive, _) = await writer.WriteArchiveAsync(outPath, archive, new NefsProgress());
		Console.WriteLine($"SAVE OK in {(DateTime.Now - before).TotalSeconds:F1}s");

		var transformer = new NefsTransformer(fs);
		for (var i = 0; i < nameFilePairs.Length; i += 2)
		{
			var name = nameFilePairs[i];
			var filePath = Path.GetFullPath(nameFilePairs[i + 1]);
			var item = newArchive.Items.EnumerateById()
				.FirstOrDefault(x => x.Type == NefsItemType.File &&
				                     string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase));
			if (item is null)
			{
				Console.WriteLine($"VERIFY '{name}': MISSING");
				continue;
			}

			var expected = System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(filePath));
			var actual = await HashItemAsync(fs, transformer, item);
			var state = expected.AsSpan().SequenceEqual(actual) ? "OK" : "MISMATCH";
			Console.WriteLine(
				$"VERIFY '{name}': {state} id={item.Id.Value} vol={item.Attributes.Volume} ext={item.ExtractedSize} " +
				$"chunks={item.DataSource.Size.Chunks.Count}");
		}

		return 0;
	}

	private static async Task<int> Extract(string archivePath, string name, string outPath)
	{
		var archive = await LoadArchive(archivePath);
		var fs = new FileSystem();
		var transformer = new NefsTransformer(fs);
		var item = archive.Items.EnumerateById()
			.FirstOrDefault(x => x.Type == NefsItemType.File &&
			                     string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase));
		if (item is null)
		{
			Console.WriteLine($"item not found: {name}");
			return 1;
		}

		Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);

		try
		{
			using var input = item.DataSource.OpenRead(fs);
			using var output = File.Create(outPath);
			await transformer.DetransformAsync(
				input,
				item.DataSource.Offset,
				output,
				0,
				item.ExtractedSize,
				item.DataSource.Size.Chunks,
				new NefsProgress());
		}
		catch (FileNotFoundException)
		{
			Console.WriteLine($"volume nao encontrado: {item.DataSource.FilePath}");
			Console.WriteLine("  este arquivo usa volumes separados (ex.: 'nome_0.nfs') que nao estao na pasta.");
			return 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"falha ao extrair '{item.FileName}': {ex.GetType().Name}: {ex.Message}");
			return 1;
		}

		Console.WriteLine($"EXTRACT '{item.FileName}' -> {outPath} size={item.ExtractedSize}");
		return 0;
	}

	private static async Task<int> Grep(string archivePath, string text)
	{
		var archive = await LoadArchive(archivePath);
		var fs = new FileSystem();
		var transformer = new NefsTransformer(fs);
		var needles = new[]
		{
			Encoding.UTF8.GetBytes(text),
			Encoding.Unicode.GetBytes(text),
		};
		var hits = 0;

		foreach (var item in archive.Items.EnumerateById())
		{
			if (item.Type != NefsItemType.File || item.ExtractedSize > 32 * 1024 * 1024)
			{
				continue;
			}

			try
			{
				using var input = item.DataSource.OpenRead(fs);
				using var output = new MemoryStream();
				await transformer.DetransformAsync(
					input,
					item.DataSource.Offset,
					output,
					0,
					item.ExtractedSize,
					item.DataSource.Size.Chunks,
					new NefsProgress());
				var data = output.ToArray();
				for (var n = 0; n < needles.Length; n++)
				{
					var needle = needles[n];
					var offset = 0;
					while (offset <= data.Length - needle.Length)
					{
						var i = data.AsSpan(offset).IndexOf(needle);
						if (i < 0)
						{
							break;
						}

						i += offset;
						var start = Math.Max(0, i - 80);
						var end = Math.Min(data.Length, i + needle.Length + 120);
						var context = Encoding.UTF8.GetString(data, start, end - start)
							.Replace('\r', ' ').Replace('\n', ' ').Replace('\0', ' ');
						Console.WriteLine($"HIT id={item.Id.Value} '{item.FileName}' enc={(n == 0 ? "utf8" : "utf16")} at={i}");
						Console.WriteLine($"  {context}");
						hits++;
						offset = i + needle.Length;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SKIP id={item.Id.Value} '{item.FileName}': {ex.GetType().Name}");
			}
		}

		Console.WriteLine($"hits={hits}");
		return 0;
	}

	private static async Task<int> GrepCi(string archivePath, string text)
	{
		var archive = await LoadArchive(archivePath);
		var fs = new FileSystem();
		var transformer = new NefsTransformer(fs);
		var needle = text.ToLowerInvariant();
		var hits = 0;

		foreach (var item in archive.Items.EnumerateById())
		{
			if (item.Type != NefsItemType.File || item.ExtractedSize > 64 * 1024 * 1024)
			{
				continue;
			}

			try
			{
				using var input = item.DataSource.OpenRead(fs);
				using var output = new MemoryStream();
				await transformer.DetransformAsync(
					input,
					item.DataSource.Offset,
					output,
					0,
					item.ExtractedSize,
					item.DataSource.Size.Chunks,
					new NefsProgress());
				var data = output.ToArray();

				void Scan(string decoded, int stride, string encoding)
				{
					var haystack = decoded.ToLowerInvariant();
					var offset = 0;
					while (offset <= haystack.Length - needle.Length)
					{
						var i = haystack.IndexOf(needle, offset, StringComparison.Ordinal);
						if (i < 0)
						{
							break;
						}

						var byteOffset = i * stride;
						var start = Math.Max(0, byteOffset - 80);
						var end = Math.Min(data.Length, byteOffset + needle.Length * stride + 120);
						var context = Encoding.UTF8.GetString(data, start, end - start)
							.Replace('\r', ' ').Replace('\n', ' ').Replace('\0', ' ');
						Console.WriteLine($"HIT id={item.Id.Value} '{item.FileName}' enc={encoding} at={byteOffset}");
						Console.WriteLine($"  {context}");
						hits++;
						offset = i + needle.Length;
					}
				}

				Scan(Encoding.Latin1.GetString(data), 1, "latin1");
				Scan(Encoding.Unicode.GetString(data, 0, data.Length & ~1), 2, "utf16le");
				Scan(Encoding.BigEndianUnicode.GetString(data, 0, data.Length & ~1), 2, "utf16be");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SKIP id={item.Id.Value} '{item.FileName}': {ex.GetType().Name}");
			}
		}

		Console.WriteLine($"hits={hits}");
		return 0;
	}

	private static async Task<int> Info(string archivePath)
	{
		var archive = await LoadArchive(archivePath);
		PrintInfo(archivePath, archive);
		return 0;
	}

	/// <summary>
	/// Shows, for each volume, the range of data offsets the items point at. Used to figure out where the
	/// volume data actually lives when the archive does not store it inline.
	/// </summary>
	private static async Task<int> Probe(string archivePath)
	{
		var archive = await LoadArchive(archivePath);
		PrintInfo(archivePath, archive);

		Console.WriteLine();
		Console.WriteLine("--- data offsets per volume ---");
		Console.WriteLine($"  header offset: 0x{archive.BaseOffset:X} ({archive.BaseOffset:N0})");

		var perVol = archive.Items.EnumerateById()
			.Where(x => x.Type == NefsItemType.File)
			.GroupBy(x => x.Attributes.Volume)
			.OrderBy(g => g.Key);

		foreach (var g in perVol)
		{
			var list = g.ToList();
			var min = list.Min(x => x.DataSource.Offset);
			var max = list.Max(x => x.DataSource.Offset + x.CompressedSize);
			var declared = g.Key < archive.Header.Volumes.Count ? archive.Header.Volumes[g.Key].Size : 0;

			Console.WriteLine($"  vol {g.Key}: {list.Count,5} items  " +
				$"start=0x{min:X} ({min:N0})  end=0x{max:X} ({max:N0})  span={max - min:N0}  declared=0x{declared:X}");

			foreach (var item in list.OrderBy(x => x.DataSource.Offset).Take(8))
			{
				Console.WriteLine($"      id={item.Id.Value,5} off=0x{item.DataSource.Offset:X} " +
					$"comp={item.CompressedSize,10} '{item.FileName}'");
			}
		}

		return 0;
	}

	private static async Task<int> Save(string archivePath, string outPath)
	{
		var archive = await LoadArchive(archivePath);
		Console.WriteLine($"LOAD OK: {archivePath}");
		PrintInfo(archivePath, archive);

		var fs = new FileSystem();
		var tempDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(outPath))!, ".nefstool-tmp");
		var writer = new NefsWriter(tempDir, fs, new NefsTransformer(fs));

		Console.WriteLine($"SAVING -> {outPath}");
		var before = DateTime.Now;
		var (newArchive, _) = await writer.WriteArchiveAsync(outPath, archive, new NefsProgress());
		Console.WriteLine($"SAVE OK in {(DateTime.Now - before).TotalSeconds:F1}s");

		var outInfo = await LoadArchive(outPath);
		PrintInfo(outPath, outInfo);

		// Cross-check volume membership between source and saved archives.
		var srcVol = archive.Items.EnumerateById().GroupBy(x => x.Attributes.Volume)
			.ToDictionary(g => g.Key, g => g.Count());
		var dstVol = outInfo.Items.EnumerateById().GroupBy(x => x.Attributes.Volume)
			.ToDictionary(g => g.Key, g => g.Count());
		Console.WriteLine("--- volume membership (volume -> item count) SOURCE vs SAVED ---");
		foreach (var key in srcVol.Keys.OrderBy(x => x))
		{
			Console.WriteLine($"  vol {key}: src={srcVol[key]} dst={(dstVol.GetValueOrDefault(key))}");
		}

		// Verify volume-1 items kept their original offsets.
		var srcVol1Offsets = archive.Items.EnumerateById()
			.Where(x => x.Attributes.Volume == 1)
			.ToDictionary(x => x.Id, x => x.DataSource.Offset);
		var bad = 0;
		foreach (var (id, offset) in srcVol1Offsets)
		{
			var dstItem = outInfo.Items.EnumerateById().FirstOrDefault(x => x.Id == id);
			if (dstItem is null || dstItem.DataSource.Offset != offset)
			{
				bad++;
			}
		}

		Console.WriteLine($"volume-1 items kept original offsets: {(bad == 0 ? "YES" : $"NO ({bad} mismatches)")}");
		return 0;
	}

	private static async Task<int> Compare(string aPath, string bPath)
	{
		var a = await LoadArchive(aPath);
		var b = await LoadArchive(bPath);
		var fs = new FileSystem();
		var transformer = new NefsTransformer(fs);
		var bById = b.Items.EnumerateById().ToDictionary(x => x.Id.Value);

		var contentDiff = 0;
		var sizeDiff = 0;
		var chunkDiff = 0;
		var transformDiff = 0;
		var missing = 0;
		var listed = 0;

		foreach (var ai in a.Items.EnumerateById())
		{
			if (ai.Type != NefsItemType.File)
			{
				continue;
			}

			if (!bById.TryGetValue(ai.Id.Value, out var bi))
			{
				missing++;
				continue;
			}

			var aZlib = ai.Transform?.IsZlibCompressed ?? false;
			var bZlib = bi.Transform?.IsZlibCompressed ?? false;
			var aAes = ai.Transform?.IsAesEncrypted ?? false;
			var bAes = bi.Transform?.IsAesEncrypted ?? false;
			var aLzss = ai.Transform?.IsLzssCompressed ?? false;
			var bLzss = bi.Transform?.IsLzssCompressed ?? false;

			if (aZlib != bZlib || aAes != bAes || aLzss != bLzss)
			{
				transformDiff++;
			}

			if (ai.ExtractedSize != bi.ExtractedSize)
			{
				sizeDiff++;
			}

			if (ai.DataSource.Size.Chunks.Count != bi.DataSource.Size.Chunks.Count)
			{
				chunkDiff++;
			}

			var ha = await HashItemAsync(fs, transformer, ai);
			var hb = await HashItemAsync(fs, transformer, bi);
			if (!ha.AsSpan().SequenceEqual(hb))
			{
				contentDiff++;
				if (listed < 20)
				{
					listed++;
					Console.WriteLine($"CONTENT DIFF id={ai.Id.Value} '{ai.FileName}' " +
						$"ext={ai.ExtractedSize}/{bi.ExtractedSize} chunks={ai.DataSource.Size.Chunks.Count}/{bi.DataSource.Size.Chunks.Count} " +
						$"zlib={aZlib}/{bZlib} aes={aAes}/{bAes} lzss={aLzss}/{bLzss}");
				}
			}
		}

		Console.WriteLine("=== compare summary ===");
		Console.WriteLine($"  missing in B      : {missing}");
		Console.WriteLine($"  extracted size diff: {sizeDiff}");
		Console.WriteLine($"  chunk count diff  : {chunkDiff}");
		Console.WriteLine($"  transform diff    : {transformDiff}");
		Console.WriteLine($"  CONTENT diff      : {contentDiff}");
		return 0;
	}

	private static async Task<byte[]> HashItemAsync(IFileSystem fs, NefsTransformer transformer, NefsItem item)
	{
		if (item.Type != NefsItemType.File)
		{
			return Array.Empty<byte>();
		}

		using var input = item.DataSource.OpenRead(fs);
		using var output = new MemoryStream();
		await transformer.DetransformAsync(
			input,
			item.DataSource.Offset,
			output,
			0,
			item.ExtractedSize,
			item.DataSource.Size.Chunks,
			new NefsProgress());
		return System.Security.Cryptography.SHA256.HashData(output.ToArray());
	}

	private static async Task<NefsArchive> LoadArchive(string path)
	{
		var fs = new FileSystem();
		var reader = new NefsReader(fs);
		var before = DateTime.Now;
		var archive = baseOffset > 0
			? await reader.ReadArchiveAsync(path, baseOffset, new NefsProgress())
			: await reader.ReadArchiveAsync(path, new NefsProgress());
		Console.WriteLine($"loaded in {(DateTime.Now - before).TotalSeconds:F1}s (offset 0x{archive.BaseOffset:X})");
		return archive;
	}

	private static void PrintInfo(string path, NefsArchive archive)
	{
		var fi = new FileInfo(path);
		Console.WriteLine("--- header summary ---");
		Console.WriteLine($"  file: {path}");
		Console.WriteLine($"  file size on disk: {fi.Length:N0} bytes");
		Console.WriteLine($"  version: {archive.Header.Version}");
		Console.WriteLine($"  num entries: {archive.Header.NumEntries}");
		Console.WriteLine($"  block size: 0x{archive.Header.BlockSize:X}");
		Console.WriteLine($"  split size: 0x{archive.Header.SplitSize:X}");
		Console.WriteLine($"  volumes: {archive.Header.Volumes.Count}");
		for (var i = 0; i < archive.Header.Volumes.Count; i++)
		{
			var v = archive.Header.Volumes[i];
			Console.WriteLine($"    [{i}] name='{v.Name}' size=0x{v.Size:X} ({v.Size:N0}) dataOffset=0x{v.DataOffset:X}");
		}

		var perVolItems = archive.Items.EnumerateById().GroupBy(x => x.Attributes.Volume)
			.OrderBy(g => g.Key)
			.ToList();
		Console.WriteLine("--- items per volume ---");
		foreach (var g in perVolItems)
		{
			var dataBytes = g.Sum(x => (decimal)x.CompressedSize);
			Console.WriteLine($"  vol {g.Key}: {g.Count()} items, {dataBytes:N0} compressed bytes");
		}

		var blockCount = archive.Header is VictorBush.Ego.NefsLib.Header.Version150.NefsHeader151 h151
			? h151.BlockTable.Entries.Count
			: 0;
		Console.WriteLine($"  block table entries: {blockCount}");
	}

	private static int PrintUsageAndFail()
	{
		Console.WriteLine("bad arguments");
		Console.WriteLine("usage: nefstool <info|save|cmp|list|replace> ...");
		return 2;
	}
}