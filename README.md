# TexInspect DDS Encoder/Decoder (VB.NET)
A lightweight, fully managed DDS encoder and decoder. TexInspect is a zero-dependency application for handling 2D DirectDraw Surface textures.

## Features
* **Tiny Footprint:** Under 100Kb, portable even if you use floppy disks.
* **Compatibility:** Only requires .NET 4.7.2, no extra runtimes or redistributables.  Compatible with Windows 7 SP1 - 11.
* **Supported Modes:**
    * **Encoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Fast).
    * **Decoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Full).
* **MipMaps:** Automated chain generation using box-filter downsampling.
* **CubeMaps:** Automated decoding and saving of CubeMap arrays.
* **Headers:** Supports Legacy FourCC and modern DX10 (DXGI_Format) extended headers.
* **Reporting:** Capable of reading and validating DDS headers, and generating full reports.
* **3D CubeMap Previews:** Full 3D previewing of CubeMaps with rotation (GUI Only).

## System Requirements
### Minimum
* **Operating System:** Windows 7 SP1 (x86) with .NET Framework 4.7.2
* **Processor:** Dual-Core CPU (e.g., Intel Pentium G-series / Core2 Duo)
* **Memory:** 4 GB RAM
* **Storage:** >128 KB available space
* **Graphics:** Integrated Graphics or any DirectX 9.0c compatible GPU with 128MB of VRAM
### Recommended
* **Operating System:** Windows 10 / 11 (x64)
* **Processor:** Quad-Core CPU or better
* **Memory:** 8 GB RAM
* **Storage:** Any SSD (SATA or NVMe)
* **Graphics:** Any Dedicated GPU (e.g. GTX 600 Series with 1GB VRAM)
> GPU requirements are recommended minimums for the OS.  TexInspect is GPU-agnostic.

## Performance
Comparison testing was done on a Xeon E3-1260L (2011), strictly on the CPU.  Results are taken from a 50-run average.  Texconv was run with the "-bc q" flag; Simple-DDS uses BC7 Mode 6 only, for fast, high-quality block compression.  Note that actual encoding and decoding times will be much faster on newer CPUs.

### Encoding
| | TexConv 4K | TexInspect 4K | Delta |
| :--- | :--- | :--- | :--- |
| BC1 No Mips | 663.34ms | 452.58ms | 1.5x Faster |
| BC1 Full Mips | 1,229.55ms | 699.64ms | 1.8x Faster |
| BC3 No Mips | 1,086.78ms | 526.18ms | 2.1x Faster |
| BC3 Full Mips | 2,063.11ms | 814.26ms | 2.5x Faster |
| BC7 (Mode 6) No Mips | 74,101ms | 525.54ms | 141.1x Faster |
| BC7 (Mode 6) Full Mips | 102,513ms | 819.28ms | 125.1x Faster |

### Decoding
| | TexConv 4K | TexInspect 4K | Delta |
| :--- | :--- | :--- | :--- |
| BC1 to PNG | 2,105.18ms | 305.28ms | 6.9x Faster |
| BC3 to PNG | 2,033.15ms | 380.66ms | 5.3x Faster |
| BC7 to PNG | 2,993.03ms | 620.08ms | 4.8x Faster |

## Quality
Kodak Lossless TrueColor Image Suite SSIM Benchmark (24 Images)
| | TexConv BC7 | TexInspect BC7 | TexConv BC3 | TexInspect BC3 |
| :--- | :--- | :--- | :--- | :--- |
| **Average** | 0.9909 | 0.9858 | 0.9561 | 0.9454 |
| **Worst-Case** | 0.9862 | 0.9684 | 0.9427 | 0.9248 |
> SSIM Reference: 1.0 = Lossless | >0.98 = Indistinguishable | >0.95 = Excellent | >0.90 = Acceptable

## CLI Usage
```vbnet
Usage: TexInspectCLI.exe <input_path> [options]

Options:
  -fmt <format>     Target Format (e.g., BC7_UNORM, DXT1, ATI2). Default: BC7_UNORM_SRGB
  -m                Generate Mipmaps
  -nx, --nodx10     Force legacy DDS header (Implicitly enabled for DXT/ATI formats, usually unneeded)
  -o <path>         Output file or directory
  -ext <extension>  Output extension for batch decoding (e.g., .jpg, .bmp). Default: .png
  -r, --recursive   Search subdirectories when processing a folder
  -f, --force       Suppress warnings and overwrite files
  --info            Show header info for the target file(s) without processing

Examples:
  Encode PNG to BC3 DDS:
    TexInspectCLI.exe texture.png -fmt BC3_UNORM_SRGB -m

  Encode PNG to legacy DXT5 DDS:
    TexInspectCLI.exe texture.png -fmt DXT5

  Encode a folder of images to DDS into a specific output folder:
    TexInspectCLI.exe "C:\InputImages" -o "C:\OutputDDS" -fmt BC7_UNORM_SRGB -m -f

  Decode a folder of DDS files to JPEGs:
    TexInspectCLI.exe "C:\InputDDS" -o "C:\OutputJPEGs" -ext .jpg -f

  View header data of a specific file:
    TexInspectCLI.exe texture.dds --info
```

## Class Usage
If you would like to use TexInspect's engine, usage is simple.  Drop DDS_Common.vb, DDS_Encoder.vb, and/or DDS_Decoder.vb (or compile and add TexInspect.dll) into your project, and call it like this:
### Encoding
```vbnet
'SourcePath, DXGI_Format, MipMaps, LegacySupport
Using Encoder As New DDS_Encoder("input.png", DXGI_Format.DXGI_FORMAT_BC7_UNORM, False, True)
    Encoder.Save("output.dds")
End Using
```
### Decoding
```vbnet
Using Decoder As New DDS_Decoder("input.dds")
    Decoder.Save("output.png", Imaging.ImageFormat.Png)
End Using
```
