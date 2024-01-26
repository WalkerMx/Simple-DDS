# About
This is intended to be a dead-simple tool for creating DDS files. The _DDS.vb_ class file is intended as a resource to help you create DDS files or implement your own DDS writer.

# Features Supported
- BC1 Compression (DXT1)
- Auto-generate MipMaps
- Alpha Channel

# Notes
Both B8G8R8X8 and B8G8R8A8 uncompressed formats are supported.  Uncompressed 24BBP B8G8R8 is technically possible, but has very poor compatibility.

BC1 compression usually looks pretty rough.  BC1 with 1-bit alpha is just awful.
