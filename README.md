# Simple DDS Encoder/Decoder (VB.NET)

A lightweight, fully managed DDS encoder and decoder. Simple-DDS provides a zero-dependency solution for handling 2D DirectDraw Surface Textures in .NET, utilizing parallel processing for high performance.

## Class Features
* **Supported Modes:**
    * **Encoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Lite).
    * **Decoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Full).
* **Mipmaps:** Automated chain generation using box-filter downsampling.
* **Headers:** Supports Legacy FourCC and modern **DX10 (DXGI_Format)** extended headers.

## Demo App (TexInspect)
* **Tiny Footprint:** Under 100Kb, portable even if you use Floppy Disks.
* **Native Windows Compatibility:** Only requires .NET 4.7.2, no extra runtimes or redistributables.  Compatible with Windows 7 through 11.
* **Robust Header Support:** Capable of reading and validating any DDS header, and reporting complete specifications.

## Performance

Comparison testing was done on a Xeon E3-1260L, strictly on the CPU.  Results are taken from a 50-run average.  Texconv was run with the "-bc q" flag; Simple-DDS uses BC7 Mode 6 only, for fast, high-quality block compression.

| | texconv | Simple-DDS | Delta |
| :--- | :--- | :--- | :--- |
| BC1 No Mips | 663.34ms | 821.58ms | 1.2x Slower |
| BC1 Full Mips | 1,229.55ms | 1,413.32ms | 1.1x Slower |
| BC3 No Mips | 1,086.78ms | 1,177.76ms | 1.1x Slower |
| BC3 Full Mips | 2,063.11ms | 1,763.86ms | 1.2x Faster |
| BC7 (Mode 6) No Mips | 74,101ms | 784.14ms | 94.5x Faster |
| BC7 (Mode 6) Full Mips | 102,513ms | 1,373.6ms | 74.6x Faster |
| | | | |
| BC1 to PNG | 2,105.18ms | 305.28ms | 6.9x Faster |
| BC3 to PNG | 2,033.15ms | 380.66ms | 5.3x Faster |
| BC7 to PNG | 2,993.03ms | 620.08ms | 4.8x Faster |

## Quality

| | texconv BC7 | Simple-DDS BC7 | texconv BC1/3 | Simple-DDS BC1/3 |
| :--- | :--- | :--- | :--- | :--- |
| 4K Opaque MSE | 2.7 | 3.6 | 15.1 | 23.783 |
| 4K Opaque PSNR | 43.9 | 42.5 | 36.3 | 34.4 |
| 4K Opaque SSIM | 0.9915 | 0.9878 | 0.9536 | 0.9377 |
| | | | | |
| 4K Alpha MSE | 209.2 | 162.2 | 62.9 | 120.8 |
| 4K Alpha PNSR | 24.9 | 26 | 30.1 | 27.3 |
| 4K Alpha SSIM | 0.9153 | 0.9271 | 0.9597 | 0.9301 |

## Usage Examples

### Encoding
```vbnet
'SourcePath, DXGI_Format, MipMaps, LegacySupport
Using Encoder As New DDS_Encoder("input.png", DXGI_Format.DXGI_FORMAT_BC7_UNORM, False, True)
    Encoder.SaveImage("output.dds")
End Using
```

### Decoding
```vbnet
Using Decoder As New DDS_Decoder("input.dds")
    Decoder.SaveImage("output.png", Imaging.ImageFormat.Png)
End Using
```
