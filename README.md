# TexInspect DDS Encoder/Decoder (VB.NET)
A lightweight, fully managed DDS encoder and decoder. TexInspect is a zero-dependency application for handling 2D DirectDraw Surface textures.

![](https://raw.githubusercontent.com/WalkerMx/DemoImages/refs/heads/master/TexInspect/TexInspectSS.png)

## Features
* **Tiny Footprint:** Under 100KB, portable even if you use floppy disks.
* **Compatibility:** Only requires .NET 4.7.2, no extra runtimes or redistributables.  Compatible with Windows 7 SP1 - 11.
* **Supported Modes:**
    * **Encoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Fast).
    * **Decoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Full).
* **MipMaps:** Automated chain generation using Catmull-Rom downsampling.
* **CubeMaps:** Automated decoding and saving of CubeMap arrays.
* **Quality Analysis:** Supports assessing MSE, PSNR, and SSIM.
* **Headers:** Supports Legacy FourCC and modern DX10 (DXGI_Format) extended headers.
* **Reporting:** Capable of reading and validating DDS headers, and generating full reports.
* **3D CubeMap Previews:** Full 3D previewing of CubeMaps with rotation (GUI Only).

## System Requirements
### Minimum
* **Operating System:** Windows 7 SP1 (x86) with .NET Framework 4.7.2
* **Processor:** Dual-Core CPU (e.g., AMD Athlon 64 X2 / Intel Core2 Duo)
* **Memory:** 2 GB RAM
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
Comparison testing was done on a i9-9980HK (2019), strictly on the CPU.  Results are taken from a 50-run average.  Texconv was run with the "-bc q" flag; TexInspect uses an optimized single-pass BC7 algorithm, for fast, high-quality block compression.

### Encoding
| | TexInspect 4K | TexConv 4K | Delta |
| :--- | :--- | :--- | :--- |
| BC7 No Mips | 272.86ms | 23226.3ms | 85.1x Faster |
| BC7 Full Mips | 402.3ms | 32747.8ms | 81.4x Faster |
| BC5 No Mips | 233.56ms | 215.8ms | 1.08x Slower |
| BC5 Full Mips | 345.28ms | 513.9ms | 1.5x Faster |
| BC3 No Mips | 238.5ms | 428.0ms | 1.8x Faster |
| BC3 Full Mips | 349.2ms | 790.5ms | 2.3x Faster |
| BC1 No Mips | 226.26ms | 409.6ms | 1.8x Faster |
| BC1 Full Mips | 305.74ms | 766.7ms | 2.5x Faster |

### Decoding
| | TexInspect 4K | TexConv 4K | Delta |
| :--- | :--- | :--- | :--- |
| BC7 | 66.28ms | 1600.1ms | 24.1x Faster |
| BC5 | 51.88ms | 423.3ms | 8.2x Faster |
| BC3 | 56.76ms | 1380.4ms | 24.3x Faster |
| BC1 | 40.02ms | 1354.0ms | 33.8x Faster |

## Quality
Kodak Lossless TrueColor Image Suite Benchmarks (24 Images)
| MSE | TexConv BC7 | TexInspect BC7 | TexConv BC3 | TexInspect BC3 |
| :--- | :--- | :--- | :--- | :--- |
| **Average** | 4.6653 | 4.4707 | 21.1078 | 31.0008 |
| **Worst-Case** | 10.1183 | 9.0946 | 47.2959 | 71.734 |
> MSE Reference: 0 = Lossless | <2 = Indistinguishable | <20 = Excellent | <65 = Acceptable

| PSNR | TexConv BC7 | TexInspect BC7 | TexConv BC3 | TexInspect BC3 |
| :--- | :--- | :--- | :--- | :--- |
| **Average** | 41.9745 dB | 42.0644 dB | 35.3033 dB | 33.7313 dB |
| **Worst-Case** | 38.0797 dB | 38.543 dB | 31.3826 dB | 29.5736 dB |
> PSNR Reference: 128 = Lossless | >45 dB = Indistinguishable | >35 dB = Excellent | >30 dB = Acceptable

| SSIM | TexConv BC7 | TexInspect BC7 | TexConv BC3 | TexInspect BC3 |
| :--- | :--- | :--- | :--- | :--- |
| **Average** | 0.9909 | 0.9903 | 0.9608 | 0.9580 |
| **Worst-Case** | 0.9862 | 0.9826 | 0.9476 | 0.9398 |
> SSIM Reference: 1.0 = Lossless | >0.98 = Indistinguishable | >0.95 = Excellent | >0.90 = Acceptable

## CLI Usage
```
Usage: TexInspectCLI.exe <input_path> [options]

Options:
  -fmt <format>                  Target Format (e.g., BC7_UNORM, DXT1, ATI2). Default: BC7_UNORM_SRGB
  -m                             Generate Mipmaps
  -ndx, --nodx10                 Force legacy DDS header (Implicitly enabled for DXT/ATI formats)
  -o <path>                      Output file or directory
  -ext <extension>               Output extension for batch decoding (e.g., .jpg, .bmp). Default: .png
  -r, --recursive                Search subdirectories when processing a folder
  -f, --force                    Suppress warnings and overwrite files
  --info                         Show header info for the target file(s) without processing
  -q, --quality <path>           Compare input to reference path and print average MSE, PSNR, & SSIM
  -qv, --qualityverbose <path>   Compare input to reference path and print average & per-channel metrics

Examples:
  Encode PNG to BC7 DDS:
    TexInspectCLI.exe texture.png -fmt BC7_UNORM_SRGB -m

  Encode PNG to legacy DXT5 DDS:
    TexInspectCLI.exe texture.png -fmt DXT5

  Encode a folder of images to DDS into a specific output folder:
    TexInspectCLI.exe "C:\InputImages" -o "C:\OutputDDS" -fmt BC7_UNORM_SRGB -m -f

  Decode a folder of DDS files to JPEGs:
    TexInspectCLI.exe "C:\InputDDS" -o "C:\OutputJPEGs" -ext .jpg -f

  View header data of a specific file:
    TexInspectCLI.exe texture.dds --info

  Generate quality metrics between two files:
    TexInspectCLI.exe texture.dds -q texture.png
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
