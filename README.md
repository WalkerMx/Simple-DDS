# Simple DDS Encoder/Decoder (VB.NET)

A lightweight, fully managed DDS encoder and decoder. Simple-DDS provides a zero-dependency solution for handling 2D DirectDraw Surface Textures in .NET, utilizing parallel processing for high performance.

## Features
* **Supported Modes:**
    * **Encoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Lite).
    * **Decoding:** BC1 (DXT1), BC2 (DXT3), BC3 (DXT5), BC4 (ATI1), BC5 (ATI2), and BC7 (Full).
* **Mipmaps:** Automated chain generation using box-filter downsampling.
* **Headers:** Supports Legacy FourCC and modern **DX10 (DXGI_Format)** extended headers.

## Performance

Comparison testing was done on a Xeon E3-1260L, strictly on the CPU.  Results are taken from a 50-run average.

| | texconv | Simple-DDS |
| :--- | :--- | :--- |
| BC1 No Mips | 663.34ms | 821.58ms |
| BC1 Full Mips | 1229.55ms | 1413.32ms |
| BC3 No Mips | 1086.78ms | 1177.76ms |
| BC3 Full Mips | 2063.11ms | 1763.86ms |
| BC7 (Mode 6) No Mips | 74101ms | 784.14ms |
| BC7 (Mode 6) Full Mips | 102513ms | 1373.6ms |
| DXT1 to PNG | 2105.18ms | 305.28ms |
| DXT5 to PNG | 2033.15ms | 380.66ms |
| BC7 to PNG | 2993.03ms | 620.08ms |

## Quality

| | texconv BC7 | Simple-DDS BC7 | texconv BC1/3 | Simple-DDS BC1/3 |
| :--- | :--- | :--- | :--- |
| 4K Opaque MSE | 2.7 | 3.6 | 15.1 | 23.783 |
| 4K Opaque PSNR | 43.9 | 42.5 | 36.3 | 34.4 |
| 4K Opaque SSIM | 0.9915 | 0.9878 | 0.9536 | 0.9377 |
| :--- | :--- | :--- | :--- |		
| 4K Alpha MSE	209.2	162.2	62.9	120.8 |
| 4K Alpha PNSR	24.9	26	30.1	27.3 |
| 4K Alpha SSIM	0.9153	0.9271	0.9597	0.9301 |

## Usage Examples

### Encoding
```vbnet
'SourcePath, AlphaMode (0=None, 1=1-bit, 2=8-bit), Compress, MipMaps, DX10Header, HighQuality
Using Encoder As New DDS_Encoder("input.png", 0, True, False, True, True)
    Encoder.SaveImage("output.dds")
End Using
```

### Decoding
```vbnet
Using Decoder As New DDS_Decoder("input.dds")
    Decoder.SaveImage("output.png", Imaging.ImageFormat.Png)
End Using
```
