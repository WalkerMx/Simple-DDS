# Managed DDS Encoder/Decoder (VB.NET)

A lightweight, fully managed DDS encoder and decoder. This library provides a zero-dependency solution for handling DirectDraw Surface textures in .NET, utilizing parallel processing for high performance.

## Features

* **Compression:** Supports **BC1 (DXT1)** and **BC3 (DXT5)**.
* **Encoding Modes:**
    * **Fast:** Rapid block processing.
    * **High Quality:** Luminance-based endpoint selection for better visual fidelity.
* **Alpha Support:** Options for Opaque, 1-bit (threshold), or 8-bit (interpolated) alpha.
* **Mipmaps:** Automated chain generation using box-filter downsampling.
* **Headers:** Supports Legacy FourCC and modern **DX10 (DXGI_Format)** extended headers.

---

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
