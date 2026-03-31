# Managed DDS Encoder (VB.NET)

A lightweight, standalone, and high-performance DDS (DirectDraw Surface) encoder for .NET. It is portable and has has zero dependancies, so you can create textures without installing huge C++ SDKs or DirectX libraries.

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

This library implements two distinct encoding strategies to balance iteration speed with final visual fidelity.

| Feature | **Fast Mode** | **High Quality Mode** |
| :--- | :--- | :--- |
| **Algorithm** | Bounding Box (Extrema) | Weighted Least Squares |
| **Metric** | Simple Midpoint | Euclidean Distance Squared |
| **Perceptual** | Numerical Min/Max | Luminance Weighted (G > R > B) |
| **Speed** | Instant (<40ms for 1MP) | ~9x slower (~280ms for 1MP) |

### High Quality Mode Details
The "High Quality" preset uses a **Weighted Least Squares** approach. It selects block endpoints based on perceived luminance and then performs a mathematical nearest-neighbor search for every pixel. This significantly reduces color-shifting and banding artifacts common in simpler encoders.

---

## Usage

Integrating the encoder is dead-simple. Simply add the `DDS.vb` class to your project and call it as follows:

```vbnet
Using SourceImage As Image = Image.FromFile("texture.png")

    ' Parameters: Image, AlphaMode (0-2), Compress (True/False), MipMaps, ExtendedHeader, HighQuality
    Using MyDDS As New DDS(SourceImage, 2, True, True, False, True)
        MyDDS.SaveImage("output.dds")
    End Using

End Using
