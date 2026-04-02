# Managed DDS Encoder/Decoder (VB.NET)

A lightweight, standalone, and high-performance DDS (DirectDraw Surface) encoder and decoder for .NET. It is portable and has has zero dependancies, so you can create textures without installing huge C++ SDKs or DirectX libraries.

## Features

* **Zero Dependencies:** Pure VB.NET implementation using `System.Drawing`.
* **Format Support:**
    * **Uncompressed:** B8G8R8X8 (Opaque) and B8G8R8A8 (Alpha).
    * **BC1 (DXT1):** Optimized for opaque textures and 1-bit "cutout" alpha.
    * **BC3 (DXT5):** High-quality 8-bit interpolated alpha for smooth transparency.
* **Performance:**
    * **Multi-threaded:** Uses `Parallel.For` logic optimized for physical CPU cores.
    * **Direct Memory Access:** Uses `System.Runtime.InteropServices` for fast pixel buffer manipulation.
* **Auto-MipMapping:** Generates full mip-chains using box-filter downsampling.

---

## Compression Quality Levels

| Feature | **Fast Mode** | **High Quality Mode** |
| :--- | :--- | :--- |
| **Algorithm** | Bounding Box | Weighted Least Squares |
| **Metric** | Simple Midpoint | Euclidean Distance Squared |
| **Perceptual** | Numerical Min/Max | Luminance Weighted (G > R > B) |
| **Speed** | Instant (<40ms for 1MP) | ~9x slower (~280ms for 1MP) |

---

## Usage Examples

Turn any standard image into a compressed DDS file:
```vbnet
Using SourceImage As Image = Image.FromFile("texture.png")

    ' Parameters: Image, AlphaMode (0 - Opaque DXT1, 1 - 1-bit DXT1, 2 - 8-bit DXT5), Compress (T/F), Mips (T/F), DX10 (T/F), HighQuality (T/F)
    Using MyDDS As New DDS(SourceImage, 2, True, True, False, True)
        MyDDS.SaveImage("output.dds")
    End Using

End Using
```

Read a DDS file and convert it back to a standard .NET Bitmap for display or conversion:
```vbnet
Using Decoder As New DDS_Decoder("input.dds")
    Decoder.SaveImage("output.png", Imaging.ImageFormat.Png)
End Using
```
