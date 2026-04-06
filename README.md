# Simple DDS Encoder/Decoder (VB.NET)

A lightweight, fully managed DDS encoder and decoder. Simple-DDS provides a zero-dependency solution for handling DirectDraw Surface textures in .NET, utilizing parallel processing for high performance.

## Features

* **Compression:** Supports **BC1 (DXT1)** and **BC3 (DXT5)**.
* **Encoding Modes:**
    * **Fast:** Rapid block processing.
    * **High Quality:** Luminance-based endpoint selection for better visual fidelity.
* **Alpha Support:** Options for Opaque, 1-bit (threshold), or 8-bit (interpolated) alpha.
* **Mipmaps:** Automated chain generation using box-filter downsampling.
* **Headers:** Supports Legacy FourCC and modern **DX10 (DXGI_Format)** extended headers.

## Performance

Comparison testing was done on a Xeon E3-1260L, strictly on the CPU.  Results are taken from a 50-run average.

| | TexConv | Simple-DDS (HQ) | Delta |
| :--- | :--- | :--- | :--- |
| **DXT1 No Mips** | 663.34ms | 1237.18ms | 1.87x Slower |
| **DXT1 Full Mips** | 1229.55ms | 1904.5ms | 1.55x Slower |
| **DXT5 No Mips** | 1086.78ms | 2099.94ms | 1.93x Slower |
| **DXT5 Full Mips** | 2063.11ms | 2853.16ms | 1.38x Slower |
| **DXT1 to PNG** | 2105.18ms | 306.28ms | 6.87x Faster |
| **DXT5 to PNG** | 2033.15ms | 351.10ms | 5.79x Faster |

## Usage Examples

### Encoding
```vbnet
' Params: SourcePath, AlphaMode (0=None, 1=1-bit, 2=8-bit), Compress, MipMaps, DX10Header, HighQuality
Using Encoder As New DDS_Encoder("input.png", 0, True, True, True, True)
    Encoder.SaveImage("output.dds")
End Using
```

### Decoding
```vbnet
Using Decoder As New DDS_Decoder("input.dds")
    Decoder.SaveImage("output.png", Imaging.ImageFormat.Png)
End Using
```
