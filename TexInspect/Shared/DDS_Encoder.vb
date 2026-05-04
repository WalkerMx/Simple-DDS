' DDS Encoder Class by WalkerMx
' Based on the documentation found here:
' http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddsfileformat.htm
' https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds

Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class DDS_Encoder
    Implements IDisposable

    Public Disposed As Boolean

    Public Signature As String
    Public HeaderSize As Integer
    Public SurfaceFlags As DDS_SurfaceFlags
    Public Height As Integer
    Public Width As Integer
    Public PitchLinearSize As Integer
    Public Depth As Integer
    Public MipMapCount As Integer

    Public SubHeaderSize As Integer
    Public PixelFlags As DDS_PixelFlags
    Public FourCC As String
    Public RGBBitCount As Integer

    Public RedBitMask As Byte()
    Public GreenBitMask As Byte()
    Public BlueBitMask As Byte()
    Public AlphaBitMask As Byte()

    Public Caps1 As DDS_Caps1
    Public Caps2 As DDS_Caps2

    Public DXGIFormat As DXGI_Format
    Public ResourceDimension As DX10_ResourceDimension
    Public MiscFlag As DX10_MiscFlags
    Public ArraySize As Integer
    Public MiscFlags2 As DX10_MiscFlags2

    Private HasAlpha As Boolean
    Private HasMipMaps As Boolean
    Private HasCompression As Boolean
    Private HasExtendedHeader As Boolean

    Private MipCount As Integer
    Private BytesPerBlock As Integer
    Private CompressionMode As Integer

    Private CubeFaces As String()

    Private HeaderBytes As Byte()
    Private WorkingBytes As Byte()
    Private PayloadBytes As Byte()

    <ThreadStatic> Private Shared BufferA As Integer()
    <ThreadStatic> Private Shared BufferB As Integer()
    <ThreadStatic> Private Shared BufferC As Integer()
    <ThreadStatic> Private Shared BufferD As Integer()
    <ThreadStatic> Private Shared BufferE As Integer()
    <ThreadStatic> Private Shared BufferF As Integer()
    <ThreadStatic> Private Shared BufferG As Integer()
    <ThreadStatic> Private Shared BufferH As ULong()

    ''' <summary>
    ''' Creates a DDS Image by explicitly defining the target DXGI Format. Header specifications are automatically inferred.
    ''' </summary>
    ''' <param name="Source">Image to create DDS from.</param>
    ''' <param name="Format">The explicit DXGI format to encode to.</param>
    ''' <param name="MipMaps">Create mipmaps for distant objects.</param>
    ''' <param name="LegacySupport">If true, strips the DX10 header and uses standard FourCC/Bitmasks. Throws an exception if the format requires DX10.</param>
    Public Sub New(Source As String, Format As DXGI_Format, MipMaps As Boolean, Optional LegacySupport As Boolean = False)
        ResourceDimension = DX10_ResourceDimension.D3D10_RESOURCE_DIMENSION_TEXTURE2D
        MiscFlag = DX10_MiscFlags.D3D10_RESOURCE_MISC_NONE
        HasMipMaps = MipMaps
        DXGIFormat = Format
        RedBitMask = {0, 0, 0, 0}
        GreenBitMask = {0, 0, 0, 0}
        BlueBitMask = {0, 0, 0, 0}
        AlphaBitMask = {0, 0, 0, 0}
        Using TempBitmap As Bitmap = Image.FromFile(Source)
            Width = TempBitmap.Width
            Height = TempBitmap.Height
            HasAlpha = Image.IsAlphaPixelFormat(TempBitmap.PixelFormat)
            Dim SourceRect As New Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height)
            Dim SourceData As BitmapData = TempBitmap.LockBits(SourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
            WorkingBytes = New Byte(SourceData.Stride * TempBitmap.Height - 1) {}
            Marshal.Copy(SourceData.Scan0, WorkingBytes, 0, WorkingBytes.Length)
            TempBitmap.UnlockBits(SourceData)
        End Using
        Dim DynamicAlpha As DX10_MiscFlags2 = If(HasAlpha, DX10_MiscFlags2.DDS_ALPHA_MODE_STRAIGHT, DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE)
        Select Case Format
            Case &H46, &H47, &H48 ' BC1 Typeless, UNORM, SRGB
                HasCompression = True
                CompressionMode = 1
                BytesPerBlock = 8
                FourCC = "DXT1"
                MiscFlags2 = DynamicAlpha
            Case &H49, &H4A, &H4B ' BC2 Typeless, UNORM, SRGB
                HasCompression = True
                CompressionMode = 2
                BytesPerBlock = 16
                FourCC = "DXT3"
                MiscFlags2 = DynamicAlpha
            Case &H4C, &H4D, &H4E ' BC3 Typeless, UNORM, SRGB
                HasCompression = True
                CompressionMode = 3
                BytesPerBlock = 16
                FourCC = "DXT5"
                MiscFlags2 = DynamicAlpha
            Case &H4F, &H50 ' BC4 Typeless, UNORM
                HasCompression = True
                CompressionMode = 4
                BytesPerBlock = 8
                FourCC = "ATI1"
                MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE
            Case &H52, &H53 ' BC5 Typeless, UNORM
                HasCompression = True
                CompressionMode = 5
                BytesPerBlock = 16
                FourCC = "ATI2"
                MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE
            Case &H61, &H62, &H63 ' BC7 Typeless, UNORM, SRGB
                If LegacySupport Then
                    Throw New ArgumentException($"Invalid format: {Format.ToString()}.")
                End If
                HasCompression = True
                CompressionMode = 7
                BytesPerBlock = 16
                MiscFlags2 = DynamicAlpha
            Case &H57, &H5A, &H5B ' B8G8R8A8 Typeless, UNORM, SRGB
                HasCompression = False
                CompressionMode = -1
                BytesPerBlock = 4
                RGBBitCount = 32
                FourCC = ""
                MiscFlags2 = DynamicAlpha
                If LegacySupport Then
                    RedBitMask = {0, 0, &HFF, 0}
                    GreenBitMask = {0, &HFF, 0, 0}
                    BlueBitMask = {&HFF, 0, 0, 0}
                    AlphaBitMask = {0, 0, 0, &HFF}
                End If
            Case &H58, &H5C, &H5D ' B8G8R8X8 Typeless, UNORM, SRGB
                HasCompression = False
                CompressionMode = -1
                BytesPerBlock = 4
                RGBBitCount = 32
                FourCC = ""
                MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE
                If LegacySupport Then
                    RedBitMask = {0, 0, &HFF, 0}
                    GreenBitMask = {0, &HFF, 0, 0}
                    BlueBitMask = {&HFF, 0, 0, 0}
                End If
        End Select
        If LegacySupport Then
            HasExtendedHeader = False
            If HasCompression Then
                PixelFlags = DDS_PixelFlags.DDPF_FOURCC
                RGBBitCount = 0
            Else
                PixelFlags = DDS_PixelFlags.DDPF_RGB
            End If
            If HasAlpha AndAlso Format <> DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM Then
                PixelFlags = PixelFlags Or DDS_PixelFlags.DDPF_ALPHAPIXELS
            End If
        Else
            HasExtendedHeader = True
            PixelFlags = DDS_PixelFlags.DDPF_FOURCC
            FourCC = "DX10"
            RGBBitCount = 0
        End If
        SurfaceFlags = DDS_SurfaceFlags.DDSD_CAPS Or DDS_SurfaceFlags.DDSD_PIXELFORMAT Or DDS_SurfaceFlags.DDSD_WIDTH Or DDS_SurfaceFlags.DDSD_HEIGHT
        Caps1 = DDS_Caps1.DDSCAPS_TEXTURE
        If MipMaps Then
            MipCount = CalcMips(Width, Height)
            SurfaceFlags = SurfaceFlags Or DDS_SurfaceFlags.DDSD_MIPMAPCOUNT
            Caps1 = Caps1 Or DDS_Caps1.DDSCAPS_COMPLEX Or DDS_Caps1.DDSCAPS_MIPMAP
        End If
        If HasCompression Then
            SurfaceFlags = SurfaceFlags Or DDS_SurfaceFlags.DDSD_LINEARSIZE
            PitchLinearSize = Math.Max(1, ((Width + 3) \ 4)) * BytesPerBlock * Math.Max(1, ((Height + 3) \ 4))
        Else
            SurfaceFlags = SurfaceFlags Or DDS_SurfaceFlags.DDSD_PITCH
            PitchLinearSize = Width * BytesPerBlock
        End If
        WriteHeader()
    End Sub

    ''' <summary>
    ''' Creates a DDS CubeMap by explicitly defining the target DXGI Format.
    ''' Expected face order in the Sources array: +X (Right), -X (Left), +Y (Top), -Y (Bottom), +Z (Front), -Z (Back).
    ''' </summary>
    ''' <param name="Sources">Array of 6 image paths corresponding to the cubemap faces.</param>
    ''' <param name="Format">The explicit DXGI format to encode to.</param>
    ''' <param name="MipMaps">Create mipmaps for distant objects.</param>
    ''' <param name="LegacySupport">If true, strips the DX10 header and uses standard FourCC/Bitmasks. Throws an exception if the format requires DX10.</param>
    Public Sub New(Sources As String(), Format As DXGI_Format, MipMaps As Boolean, Optional LegacySupport As Boolean = False)
        Me.New(Sources(0), Format, MipMaps, LegacySupport)
        If Sources.Length <> 6 Then
            Throw New ArgumentException("A cubemap requires exactly 6 image faces.")
        End If
        CubeFaces = Sources
        Caps1 = Caps1 Or DDS_Caps1.DDSCAPS_COMPLEX
        Caps2 = Caps2 Or DDS_Caps2.DDSCAPS2_CUBEMAP
        Caps2 = Caps2 Or DDS_Caps2.DDSCAPS2_CUBEMAP_POSITIVEX
        Caps2 = Caps2 Or DDS_Caps2.DDSCAPS2_CUBEMAP_NEGATIVEX
        Caps2 = Caps2 Or DDS_Caps2.DDSCAPS2_CUBEMAP_POSITIVEY
        Caps2 = Caps2 Or DDS_Caps2.DDSCAPS2_CUBEMAP_NEGATIVEY
        Caps2 = Caps2 Or DDS_Caps2.DDSCAPS2_CUBEMAP_POSITIVEZ
        Caps2 = Caps2 Or DDS_Caps2.DDSCAPS2_CUBEMAP_NEGATIVEZ
        If HasExtendedHeader Then
            MiscFlag = DX10_MiscFlags.D3D10_RESOURCE_MISC_TEXTURECUBE
        End If
        WriteHeader()
    End Sub

    Private Sub WriteHeader()

        Using HeaderStream As New MemoryStream()

            HeaderStream.Write(OrderBytes("DDS "), 0, 4)                    ' dwMagic
            HeaderStream.Write(OrderBytes(124), 0, 4)                       ' dwSize
            HeaderStream.Write(OrderBytes(SurfaceFlags), 0, 4)              ' dwFlags
            HeaderStream.Write(OrderBytes(Height), 0, 4)                    ' dwHeight
            HeaderStream.Write(OrderBytes(Width), 0, 4)                     ' dwWidth
            HeaderStream.Write(OrderBytes(PitchLinearSize), 0, 4)           ' dwPitchOrLinearSize
            HeaderStream.Write(OrderBytes(0), 0, 4)                         ' dwDepth
            HeaderStream.Write(OrderBytes(MipCount), 0, 4)                  ' dwMipMapCount

            HeaderStream.Write(New Byte(43) {}, 0, 44)                      ' dwReserved1 x11

            HeaderStream.Write(OrderBytes(32), 0, 4)                        ' DDPIXELFORMAT dwSize
            HeaderStream.Write(OrderBytes(PixelFlags), 0, 4)                ' DDPIXELFORMAT dwFlags
            HeaderStream.Write(OrderBytes(FourCC), 0, 4)                    ' DDPIXELFORMAT dwFourCC
            HeaderStream.Write(OrderBytes(RGBBitCount), 0, 4)               ' DDPIXELFORMAT dwRGBBitCount
            HeaderStream.Write(RedBitMask, 0, 4)                            ' DDPIXELFORMAT dwRBitMask
            HeaderStream.Write(GreenBitMask, 0, 4)                          ' DDPIXELFORMAT dwGBitMask
            HeaderStream.Write(BlueBitMask, 0, 4)                           ' DDPIXELFORMAT dwBBitMask
            HeaderStream.Write(AlphaBitMask, 0, 4)                          ' DDPIXELFORMAT dwABitMask

            HeaderStream.Write(OrderBytes(Caps1), 0, 4)                     ' dwCaps1
            HeaderStream.Write(OrderBytes(Caps2), 0, 4)                     ' dwCaps2

            HeaderStream.Write(New Byte(11) {}, 0, 12)                      ' dwCaps3, dwCaps4, dwReserved2

            If HasExtendedHeader Then
                HeaderStream.Write(OrderBytes(DXGIFormat), 0, 4)            ' dwDxgiFormat
                HeaderStream.Write(OrderBytes(ResourceDimension), 0, 4)     ' dwResourceDimension
                HeaderStream.Write(OrderBytes(MiscFlag), 0, 4)              ' dwMiscFlag
                HeaderStream.Write(OrderBytes(1), 0, 4)                     ' dwArraySize
                HeaderStream.Write(OrderBytes(MiscFlags2), 0, 4)            ' dwMiscFlags2
            End If

            HeaderBytes = HeaderStream.ToArray

        End Using

    End Sub

    Public Sub BeginEncode()
        Dim TempWidth As Integer = Width
        Dim TempHeight As Integer = Height
        Using PayloadStream As New MemoryStream()
            PayloadStream.Write(HeaderBytes, 0, HeaderBytes.Count)
            Dim NextBytes As Byte() = GetImageData(WorkingBytes, TempWidth, TempHeight)
            PayloadStream.Write(NextBytes, 0, NextBytes.Count)
            If HasMipMaps Then
                For i = 0 To MipCount - 2
                    WorkingBytes = HalveArray(WorkingBytes, TempWidth, TempHeight)
                    TempWidth = Math.Max(1, TempWidth >> 1)
                    TempHeight = Math.Max(1, TempHeight >> 1)
                    NextBytes = GetImageData(WorkingBytes, TempWidth, TempHeight)
                    PayloadStream.Write(NextBytes, 0, NextBytes.Count)
                Next
            End If
            WorkingBytes = Nothing
            PayloadBytes = PayloadStream.ToArray
        End Using
    End Sub

    Public Sub BeginEncodeCube()
        Using PayloadStream As New MemoryStream()
            PayloadStream.Write(HeaderBytes, 0, HeaderBytes.Length)
            For faceIndex As Integer = 0 To 5
                Dim TempWidth As Integer = Me.Width
                Dim TempHeight As Integer = Me.Height
                Using TempBitmap As Bitmap = Image.FromFile(CubeFaces(faceIndex))
                    If TempBitmap.Width <> Me.Width OrElse TempBitmap.Height <> Me.Height Then
                        Throw New InvalidDataException($"Dimensions of face {faceIndex} do not match the base (+X) image.")
                    End If
                    Dim SourceRect As New Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height)
                    Dim SourceData As BitmapData = TempBitmap.LockBits(SourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
                    WorkingBytes = New Byte(SourceData.Stride * TempBitmap.Height - 1) {}
                    Marshal.Copy(SourceData.Scan0, WorkingBytes, 0, WorkingBytes.Length)
                    TempBitmap.UnlockBits(SourceData)
                End Using
                Dim NextBytes As Byte() = GetImageData(WorkingBytes, TempWidth, TempHeight)
                PayloadStream.Write(NextBytes, 0, NextBytes.Length)
                If HasMipMaps Then
                    For i As Integer = 0 To MipCount - 2
                        WorkingBytes = HalveArray(WorkingBytes, TempWidth, TempHeight)
                        TempWidth = Math.Max(1, TempWidth >> 1)
                        TempHeight = Math.Max(1, TempHeight >> 1)
                        NextBytes = GetImageData(WorkingBytes, TempWidth, TempHeight)
                        PayloadStream.Write(NextBytes, 0, NextBytes.Length)
                    Next
                End If
            Next
            WorkingBytes = Nothing
            PayloadBytes = PayloadStream.ToArray
        End Using
    End Sub

    Private Function GetImageData(BitmapBytes As Byte(), Width As Integer, Height As Integer) As Byte()
        If HasCompression Then
            Return BlockCompress(BitmapBytes, Width, Height)
        Else
            Return WriteUncompressed(BitmapBytes, HasAlpha)
        End If
    End Function

    Private Function WriteUncompressed(SourceData As Byte(), Alpha As Boolean) As Byte()
        If Alpha Then
            If MipCount > 1 Then
                Return DirectCast(SourceData.Clone(), Byte())
            Else
                Return SourceData
            End If
        End If
        Dim Result(SourceData.Length - 1) As Byte
        Buffer.BlockCopy(SourceData, 0, Result, 0, SourceData.Length)
        For i As Integer = 3 To Result.Length - 1 Step 4
            Result(i) = &HFF
        Next
        Return Result
    End Function

    Private Function BlockCompress(SourceData As Byte(), Width As Integer, Height As Integer) As Byte()
        Dim BlockWidth As Integer = Math.Max(1, (Width + 3) \ 4)
        Dim BlockHeight As Integer = Math.Max(1, (Height + 3) \ 4)
        Dim Result(BlockWidth * BlockHeight * BytesPerBlock - 1) As Byte
        Parallel.For(0, BlockHeight, Options, Sub(yBlock)
                                                  If BufferA Is Nothing Then
                                                      BufferA = New Integer(15) {} : BufferB = New Integer(15) {}
                                                      BufferC = New Integer(15) {} : BufferD = New Integer(15) {}
                                                      BufferE = New Integer(7) {} : BufferF = New Integer(7) {}
                                                      BufferG = New Integer(15) {} : BufferH = New ULong(15) {}
                                                  End If
                                                  Dim yPixelBase As Integer = yBlock * 4
                                                  Dim rowOutputOffset As Integer = yBlock * BlockWidth * BytesPerBlock
                                                  For xBlock As Integer = 0 To BlockWidth - 1
                                                      Dim xPixelBase As Integer = xBlock * 4
                                                      Dim currentBlockOffset As Integer = rowOutputOffset + (xBlock * BytesPerBlock)
                                                      Select Case CompressionMode
                                                          Case 0 ' BC1
                                                              EncodeBlockBC1(SourceData, xPixelBase, yPixelBase, Width, Height, Result, currentBlockOffset, BufferA)
                                                          Case 1 ' BC1a
                                                              Dim AlphaMask As UShort = 0
                                                              EncodeBlockBC1(SourceData, xPixelBase, yPixelBase, Width, Height, Result, currentBlockOffset, BufferA, AlphaMask)
                                                              If HasAlpha AndAlso AlphaMask > 0 Then
                                                                  EncodeBlockBC1a(Result, currentBlockOffset, BufferA, AlphaMask)
                                                              End If
                                                          Case 2 ' BC2
                                                              EncodeBlockBC2(SourceData, xPixelBase, yPixelBase, Width, Height, Result, currentBlockOffset)
                                                              EncodeBlockBC1(SourceData, xPixelBase, yPixelBase, Width, Height, Result, currentBlockOffset + 8, BufferA)
                                                          Case 3 ' BC3
                                                              EncodeBlockBC3(SourceData, xPixelBase, yPixelBase, Width, Height, 3, Result, currentBlockOffset, BufferA)
                                                              EncodeBlockBC1(SourceData, xPixelBase, yPixelBase, Width, Height, Result, currentBlockOffset + 8, BufferB)
                                                          Case 4 ' BC4
                                                              EncodeBlockBC3(SourceData, xPixelBase, yPixelBase, Width, Height, 2, Result, currentBlockOffset, BufferA)
                                                          Case 5 ' BC5
                                                              EncodeBlockBC3(SourceData, xPixelBase, yPixelBase, Width, Height, 2, Result, currentBlockOffset, BufferA)
                                                              EncodeBlockBC3(SourceData, xPixelBase, yPixelBase, Width, Height, 1, Result, currentBlockOffset + 8, BufferB)
                                                          Case 7 ' BC7 (Modes 1, 6, & 7)
                                                              EncodeBlockBC7(SourceData, xPixelBase, yPixelBase, Width, Height, Result, currentBlockOffset, BufferA, BufferB, BufferC, BufferD, BufferE, BufferF, BufferG, BufferH)
                                                      End Select
                                                  Next
                                              End Sub)
        Return Result
    End Function

    Private Sub EncodeBlockBC7(SourceData As Byte(), xPixelBase As Integer, yPixelBase As Integer, Width As Integer, Height As Integer, Result As Byte(), OutputOffset As Integer, LocalB() As Integer, LocalG() As Integer, LocalR() As Integer, LocalA() As Integer, LocalMin() As Integer, LocalMax() As Integer, IndicesBuffer() As Integer, ColorTable() As ULong)
        For i As Integer = 0 To 7
            LocalMin(i) = 255
            LocalMax(i) = 0
        Next
        Dim LocalIndex As Integer = 0
        For j As Integer = 0 To 3
            Dim yPixel As Integer = Math.Min(yPixelBase + j, Height - 1)
            Dim RowInputOffset As Integer = yPixel * Width * 4
            For i As Integer = 0 To 3
                Dim xPixel As Integer = Math.Min(xPixelBase + i, Width - 1)
                Dim PixelIndex As Integer = RowInputOffset + (xPixel * 4)
                Dim valB As Integer = SourceData(PixelIndex)
                Dim valG As Integer = SourceData(PixelIndex + 1)
                Dim valR As Integer = SourceData(PixelIndex + 2)
                Dim valA As Integer = SourceData(PixelIndex + 3)
                LocalB(LocalIndex) = valB
                LocalG(LocalIndex) = valG
                LocalR(LocalIndex) = valR
                LocalA(LocalIndex) = valA
                If valR < LocalMin(0) Then LocalMin(0) = valR
                If valR > LocalMax(0) Then LocalMax(0) = valR
                If valG < LocalMin(1) Then LocalMin(1) = valG
                If valG > LocalMax(1) Then LocalMax(1) = valG
                If valB < LocalMin(2) Then LocalMin(2) = valB
                If valB > LocalMax(2) Then LocalMax(2) = valB
                If valA < LocalMin(3) Then LocalMin(3) = valA
                If valA > LocalMax(3) Then LocalMax(3) = valA
                LocalIndex += 1
            Next
        Next
        Dim Threshold As Integer = 48
        Dim MaxDist As Integer = 0
        Dim CornerAIndex As Integer = 0, CornerBIndex As Integer = 0
        Dim R_Dist, G_Dist, B_Dist, Mask As Integer
        For i = 0 To 2
            For j = i + 1 To 3
                Dim CornerA = BlockCorners(i)
                Dim CornerB = BlockCorners(j)
                R_Dist = LocalR(CornerA) - LocalR(CornerB) : Mask = R_Dist >> 31 : R_Dist = (R_Dist + Mask) Xor Mask
                G_Dist = LocalG(CornerA) - LocalG(CornerB) : Mask = G_Dist >> 31 : G_Dist = (G_Dist + Mask) Xor Mask
                B_Dist = LocalB(CornerA) - LocalB(CornerB) : Mask = B_Dist >> 31 : B_Dist = (B_Dist + Mask) Xor Mask
                Dim RGB_Dist As Integer = R_Dist + G_Dist + B_Dist
                If RGB_Dist > MaxDist Then
                    MaxDist = RGB_Dist
                    CornerAIndex = CornerA
                    CornerBIndex = CornerB
                End If
            Next
        Next
        Dim UseMode1 As Boolean = False
        Dim UseMode7 As Boolean = False
        Dim BestIndex As Integer = -1
        If MaxDist >= Threshold Then
            Dim CornerA_R = LocalR(CornerAIndex), CornerA_G = LocalG(CornerAIndex), CornerA_B = LocalB(CornerAIndex)
            Dim CornerB_R = LocalR(CornerBIndex), CornerB_G = LocalG(CornerBIndex), CornerB_B = LocalB(CornerBIndex)
            Dim shapeBits As Integer = 0
            For c As Integer = 0 To 3
                Dim CornerIndex = BlockCorners(c)
                R_Dist = LocalR(CornerIndex) - CornerA_R : Mask = R_Dist >> 31 : R_Dist = (R_Dist + Mask) Xor Mask
                G_Dist = LocalG(CornerIndex) - CornerA_G : Mask = G_Dist >> 31 : G_Dist = (G_Dist + Mask) Xor Mask
                B_Dist = LocalB(CornerIndex) - CornerA_B : Mask = B_Dist >> 31 : B_Dist = (B_Dist + Mask) Xor Mask
                Dim dA_dist As Integer = R_Dist + G_Dist + B_Dist
                R_Dist = LocalR(CornerIndex) - CornerB_R : Mask = R_Dist >> 31 : R_Dist = (R_Dist + Mask) Xor Mask
                G_Dist = LocalG(CornerIndex) - CornerB_G : Mask = G_Dist >> 31 : G_Dist = (G_Dist + Mask) Xor Mask
                B_Dist = LocalB(CornerIndex) - CornerB_B : Mask = B_Dist >> 31 : B_Dist = (B_Dist + Mask) Xor Mask
                Dim dB_Dist As Integer = R_Dist + G_Dist + B_Dist
                Dim target As Integer = If(dA_dist < dB_Dist, 0, 1)
                shapeBits = shapeBits Or (target << (3 - c))
            Next
            If (shapeBits And 8) = 8 Then
                shapeBits = (Not shapeBits) And 15
            End If
            Dim shapeIndex As Integer = shapeBits And 7
            BestIndex = ParitionMap(shapeIndex)
            If BestIndex <> -1 Then
                If LocalMax(3) - LocalMin(3) > 0 Then
                    UseMode7 = True
                Else
                    UseMode1 = True
                End If
            End If
        End If
        If UseMode7 Then
            EncodeMode7(BestIndex, LocalMin, LocalMax, IndicesBuffer, LocalR, LocalG, LocalB, LocalA, Result, OutputOffset)
            Return
        End If
        If UseMode1 Then
            EncodeMode1(BestIndex, LocalMin, LocalMax, IndicesBuffer, LocalR, LocalG, LocalB, Result, OutputOffset)
            Return
        End If
        EncodeMode6(LocalMin, LocalMax, LocalR, LocalG, LocalB, LocalA, ColorTable, Result, OutputOffset)
    End Sub

    Private Sub EncodeBlockBC3(SourceData As Byte(), xPixelBase As Integer, yPixelBase As Integer, Width As Integer, Height As Integer, ChannelOffset As Integer, Result As Byte(), OutputOffset As Integer, ChannelArray() As Integer)
        Dim idx As Integer = 0
        For j As Integer = 0 To 3
            Dim py As Integer = Math.Min(yPixelBase + j, Height - 1)
            Dim rowInputOffset As Integer = py * Width * 4
            For i As Integer = 0 To 3
                Dim px As Integer = Math.Min(xPixelBase + i, Width - 1)
                Dim pixelIdx As Integer = rowInputOffset + (px * 4)
                ChannelArray(idx) = SourceData(pixelIdx + ChannelOffset)
                idx += 1
            Next
        Next
        Dim Val0 As Byte = 0
        Dim Val1 As Byte = 255
        For i As Integer = 0 To 15
            Dim LocalTemp As Byte = ChannelArray(i)
            If LocalTemp > Val0 Then Val0 = LocalTemp
            If LocalTemp < Val1 Then Val1 = LocalTemp
        Next
        If Val0 = Val1 Then
            If Val0 > 0 Then
                Val1 -= 1
            Else
                Val0 += 1
            End If
        End If
        Result(OutputOffset) = Val0
        Result(OutputOffset + 1) = Val1
        Dim BitBuffer As Long = 0
        Dim BitsLoaded As Integer = 0
        Dim ByteOffset As Integer = OutputOffset + 2
        Dim Range As Integer = CInt(Val0) - Val1
        For i As Integer = 0 To 15
            Dim v As Integer = ChannelArray(i)
            Dim Index As Byte
            If v = Val0 Then
                Index = 0
            ElseIf v = Val1 Then
                Index = 1
            Else
                Index = CByte(Math.Max(2, Math.Min(7 - ((CInt(v) - Val1) * 7 \ Range), 7)))
            End If
            BitBuffer = BitBuffer Or (CLng(Index) << BitsLoaded)
            BitsLoaded += 3
            While BitsLoaded >= 8
                Result(ByteOffset) = CByte(BitBuffer And &HFF)
                BitBuffer >>= 8
                BitsLoaded -= 8
                ByteOffset += 1
            End While
        Next
    End Sub

    Private Sub EncodeBlockBC2(SourceData As Byte(), xPixelBase As Integer, yPixelBase As Integer, Width As Integer, Height As Integer, Result As Byte(), OutputOffset As Integer)
        Dim byteIdx As Integer = 0
        For j As Integer = 0 To 3
            Dim py As Integer = Math.Min(yPixelBase + j, Height - 1)
            Dim rowInputOffset As Integer = py * Width * 4
            For i As Integer = 0 To 3 Step 2
                Dim px0 As Integer = Math.Min(xPixelBase + i, Width - 1)
                Dim alpha0 As Byte = SourceData(rowInputOffset + (px0 * 4) + 3)
                Dim nibble0 As Byte = CByte(alpha0 >> 4)
                Dim px1 As Integer = Math.Min(xPixelBase + i + 1, Width - 1)
                Dim alpha1 As Byte = SourceData(rowInputOffset + (px1 * 4) + 3)
                Dim nibble1 As Byte = CByte(alpha1 >> 4)
                Result(OutputOffset + byteIdx) = CByte(nibble0 Or (nibble1 << 4))
                byteIdx += 1
            Next
        Next
    End Sub

    Private Sub EncodeBlockBC1a(Result As Byte(), OutputOffset As Integer, PixelArray() As Integer, AlphaMask As UShort)
        If AlphaMask = 0 Then Return
        Dim Col0 As UShort = Result(OutputOffset) Or (CUShort(Result(OutputOffset + 1)) << 8)
        Dim Col1 As UShort = Result(OutputOffset + 2) Or (CUShort(Result(OutputOffset + 3)) << 8)
        Dim temp As UShort = Col0 : Col0 = Col1 : Col1 = temp
        Result(OutputOffset) = CByte(Col0 And &HFF)
        Result(OutputOffset + 1) = CByte(Col0 >> 8)
        Result(OutputOffset + 2) = CByte(Col1 And &HFF)
        Result(OutputOffset + 3) = CByte(Col1 >> 8)
        Dim R0 As Integer = (Col0 >> 11) << 3
        Dim G0 As Integer = ((Col0 >> 5) And &H3F) << 2
        Dim B0 As Integer = (Col0 And &H1F) << 3
        Dim R1 As Integer = (Col1 >> 11) << 3
        Dim G1 As Integer = ((Col1 >> 5) And &H3F) << 2
        Dim B1 As Integer = (Col1 And &H1F) << 3
        Dim R2 As Integer = (R0 + R1) \ 2
        Dim G2 As Integer = (G0 + G1) \ 2
        Dim B2 As Integer = (B0 + B1) \ 2
        Dim ColorTable As UInteger = 0
        Dim shift As Integer = 0
        For i As Integer = 0 To 15
            Dim Index As UInteger = 0
            If (AlphaMask And (1US << i)) <> 0 Then
                Index = 3
            Else
                Dim Pixel As UShort = PixelArray(i)
                Dim PixR As Integer = (Pixel >> 11) << 3
                Dim PixG As Integer = ((Pixel >> 5) And &H3F) << 2
                Dim PixB As Integer = (Pixel And &H1F) << 3
                Dim dR As Integer = PixR - R0 : Dim dG As Integer = PixG - G0 : Dim dB As Integer = PixB - B0
                Dim minErr As Integer = (dR * dR) + (dG * dG) + (dB * dB)
                Index = 0
                dR = PixR - R1 : dG = PixG - G1 : dB = PixB - B1
                Dim err1 As Integer = (dR * dR) + (dG * dG) + (dB * dB)
                If err1 < minErr Then minErr = err1 : Index = 1
                dR = PixR - R2 : dG = PixG - G2 : dB = PixB - B2
                Dim err2 As Integer = (dR * dR) + (dG * dG) + (dB * dB)
                If err2 < minErr Then Index = 2
            End If
            ColorTable = ColorTable Or (Index << shift)
            shift += 2
        Next
        Result(OutputOffset + 4) = CByte(ColorTable And &HFF)
        Result(OutputOffset + 5) = CByte((ColorTable >> 8) And &HFF)
        Result(OutputOffset + 6) = CByte((ColorTable >> 16) And &HFF)
        Result(OutputOffset + 7) = CByte((ColorTable >> 24) And &HFF)
    End Sub

    Private Sub EncodeBlockBC1(SourceData As Byte(), xPixelBase As Integer, yPixelBase As Integer, Width As Integer, Height As Integer, Result As Byte(), OutputOffset As Integer, PixelArray() As Integer, Optional ByRef AlphaMask As UShort = 0)
        Dim MaxY As Integer = Height - 1
        Dim MaxX As Integer = Width - 1
        Dim MaxLum As Integer = -1
        Dim MinLum As Integer = 1000000
        Dim Col0 As UShort = 0
        Dim Col1 As UShort = 0
        AlphaMask = 0
        Dim idx As Integer = 0
        For j As Integer = 0 To 3
            Dim py As Integer = yPixelBase + j
            If py > MaxY Then py = MaxY
            Dim rowInputOffset As Integer = py * Width * 4
            For i As Integer = 0 To 3
                Dim px As Integer = xPixelBase + i
                If px > MaxX Then px = MaxX
                Dim pixelIdx As Integer = rowInputOffset + (px * 4)
                Dim b As Integer = SourceData(pixelIdx)
                Dim g As Integer = SourceData(pixelIdx + 1)
                Dim r As Integer = SourceData(pixelIdx + 2)
                Dim a As Integer = SourceData(pixelIdx + 3)
                Dim pixel565 As UShort = 0
                Dim Lum As Integer = 0
                If a < 128 Then
                    AlphaMask = AlphaMask Or CUShort(1 << idx)
                Else
                    pixel565 = CUShort(((r And &HF8) << 8) Or ((g And &HFC) << 3) Or (b >> 3))
                    Lum = (r * 77) + (g * 151) + (b * 28)
                End If
                PixelArray(idx) = pixel565
                idx += 1
                If Lum > MaxLum Then
                    MaxLum = Lum
                    Col0 = pixel565
                End If
                If Lum < MinLum Then
                    MinLum = Lum
                    Col1 = pixel565
                End If
            Next
        Next
        If Col0 < Col1 Then
            Dim temp As UShort = Col0 : Col0 = Col1 : Col1 = temp
        ElseIf Col0 = Col1 Then
            If Col0 > 0 Then Col1 -= 1US Else Col0 += 1US
        End If
        Dim R0 As Integer = (Col0 >> 11) << 3
        Dim G0 As Integer = ((Col0 >> 5) And &H3F) << 2
        Dim B0 As Integer = (Col0 And &H1F) << 3
        Dim R1 As Integer = (Col1 >> 11) << 3
        Dim G1 As Integer = ((Col1 >> 5) And &H3F) << 2
        Dim B1 As Integer = (Col1 And &H1F) << 3
        Dim R2 As Integer = (2 * R0 + R1) \ 3 : Dim G2 As Integer = (2 * G0 + G1) \ 3 : Dim B2 As Integer = (2 * B0 + B1) \ 3
        Dim R3 As Integer = (R0 + 2 * R1) \ 3 : Dim G3 As Integer = (G0 + 2 * G1) \ 3 : Dim B3 As Integer = (B0 + 2 * B1) \ 3
        Dim ColorTable As UInteger = 0
        Dim shift As Integer = 0
        For i As Integer = 0 To 15
            Dim Pixel As UShort = PixelArray(i)
            Dim PixR As Integer = (Pixel >> 11) << 3
            Dim PixG As Integer = ((Pixel >> 5) And &H3F) << 2
            Dim PixB As Integer = (Pixel And &H1F) << 3
            Dim dR As Integer = PixR - R0 : Dim dG As Integer = PixG - G0 : Dim dB As Integer = PixB - B0
            Dim minErr As Integer = (dR * dR) + (dG * dG) + (dB * dB)
            Dim Index As UInteger = 0
            dR = PixR - R1 : dG = PixG - G1 : dB = PixB - B1
            Dim err1 As Integer = (dR * dR) + (dG * dG) + (dB * dB)
            If err1 < minErr Then minErr = err1 : Index = 1
            dR = PixR - R2 : dG = PixG - G2 : dB = PixB - B2
            Dim err2 As Integer = (dR * dR) + (dG * dG) + (dB * dB)
            If err2 < minErr Then minErr = err2 : Index = 2
            dR = PixR - R3 : dG = PixG - G3 : dB = PixB - B3
            Dim err3 As Integer = (dR * dR) + (dG * dG) + (dB * dB)
            If err3 < minErr Then Index = 3
            ColorTable = ColorTable Or (Index << shift)
            shift += 2
        Next
        Result(OutputOffset) = CByte(Col0 And &HFF)
        Result(OutputOffset + 1) = CByte(Col0 >> 8)
        Result(OutputOffset + 2) = CByte(Col1 And &HFF)
        Result(OutputOffset + 3) = CByte(Col1 >> 8)
        Result(OutputOffset + 4) = CByte(ColorTable And &HFF)
        Result(OutputOffset + 5) = CByte((ColorTable >> 8) And &HFF)
        Result(OutputOffset + 6) = CByte((ColorTable >> 16) And &HFF)
        Result(OutputOffset + 7) = CByte((ColorTable >> 24) And &HFF)
    End Sub

#Region "BC7 Modes"

    Private Sub EncodeMode1(bestPartition As Integer, minVal() As Integer, maxVal() As Integer, indices() As Integer, LocalR() As Integer, LocalG() As Integer, LocalB() As Integer, Result As Byte(), OutputOffset As Integer)
        Dim subMask = PartitionTable2(bestPartition)
        For i = 0 To 5
            minVal(i) = 255
            maxVal(i) = 0
        Next
        For i = 0 To 15
            Dim s = (subMask >> i) And 1
            Dim sOff = If(s = 1, 3, 0)
            If LocalR(i) < minVal(sOff) Then minVal(sOff) = LocalR(i)
            If LocalR(i) > maxVal(sOff) Then maxVal(sOff) = LocalR(i)
            If LocalG(i) < minVal(sOff + 1) Then minVal(sOff + 1) = LocalG(i)
            If LocalG(i) > maxVal(sOff + 1) Then maxVal(sOff + 1) = LocalG(i)
            If LocalB(i) < minVal(sOff + 2) Then minVal(sOff + 2) = LocalB(i)
            If LocalB(i) > maxVal(sOff + 2) Then maxVal(sOff + 2) = LocalB(i)
        Next
        Dim eR0 As Integer = minVal(0) >> 2, eR1 As Integer = maxVal(0) >> 2
        Dim eG0 As Integer = minVal(1) >> 2, eG1 As Integer = maxVal(1) >> 2
        Dim eB0 As Integer = minVal(2) >> 2, eB1 As Integer = maxVal(2) >> 2
        Dim minLum0 As Integer = minVal(2) + (minVal(1) << 1) + minVal(0)
        Dim range0 As Integer = (maxVal(2) + (maxVal(1) << 1) + maxVal(0)) - minLum0
        If range0 < 1 Then range0 = 1
        Dim eR2 As Integer = minVal(3) >> 2, eR3 As Integer = maxVal(3) >> 2
        Dim eG2 As Integer = minVal(4) >> 2, eG3 As Integer = maxVal(4) >> 2
        Dim eB2 As Integer = minVal(5) >> 2, eB3 As Integer = maxVal(5) >> 2
        Dim minLum1 As Integer = minVal(5) + (minVal(4) << 1) + minVal(3)
        Dim range1 As Integer = (maxVal(5) + (maxVal(4) << 1) + maxVal(3)) - minLum1
        If range1 < 1 Then range1 = 1
        For i = 0 To 15
            Dim s = (subMask >> i) And 1
            Dim pixLum = LocalB(i) + (LocalG(i) << 1) + LocalR(i)
            Dim index As Integer
            If s = 0 Then
                index = ((pixLum - minLum0) * 7 + (range0 >> 1)) \ range0
            Else
                index = ((pixLum - minLum1) * 7 + (range1 >> 1)) \ range1
            End If
            If index > 7 Then index = 7
            If index < 0 Then index = 0
            indices(i) = index
        Next
        Dim anchor0 As Integer = 0
        Dim anchor1 As Integer = AnchorIndexTable2(bestPartition)
        If indices(anchor0) >= 4 Then
            Dim temp As Integer
            temp = eR0 : eR0 = eR1 : eR1 = temp
            temp = eG0 : eG0 = eG1 : eG1 = temp
            temp = eB0 : eB0 = eB1 : eB1 = temp
            For i = 0 To 15
                If ((subMask >> i) And 1) = 0 Then indices(i) = 7 - indices(i)
            Next
        End If
        If indices(anchor1) >= 4 Then
            Dim temp As Integer
            temp = eR2 : eR2 = eR3 : eR3 = temp
            temp = eG2 : eG2 = eG3 : eG3 = temp
            temp = eB2 : eB2 = eB3 : eB3 = temp
            For i = 0 To 15
                If ((subMask >> i) And 1) = 1 Then indices(i) = 7 - indices(i)
            Next
        End If
        Dim Low1 As ULong = 2UL
        Low1 = Low1 Or (CULng(bestPartition) << 2)
        Low1 = Low1 Or (CULng(eR0) << 8)
        Low1 = Low1 Or (CULng(eR1) << 14)
        Low1 = Low1 Or (CULng(eR2) << 20)
        Low1 = Low1 Or (CULng(eR3) << 26)
        Low1 = Low1 Or (CULng(eG0) << 32)
        Low1 = Low1 Or (CULng(eG1) << 38)
        Low1 = Low1 Or (CULng(eG2) << 44)
        Low1 = Low1 Or (CULng(eG3) << 50)
        Low1 = Low1 Or (CULng(eB0) << 56)
        Low1 = Low1 Or ((CULng(eB1) And 3UL) << 62)
        Dim High1 As ULong = ((CULng(eB1) >> 2) And 15UL)
        High1 = High1 Or (CULng(eB2) << 4)
        High1 = High1 Or (CULng(eB3) << 10)
        Dim bitOffset As Integer = 18
        For i = 0 To 15
            Dim bits As Integer = If(i = anchor0 OrElse i = anchor1, 2, 3)
            High1 = High1 Or (CULng(indices(i)) << bitOffset)
            bitOffset += bits
        Next
        For i As Integer = 0 To 7
            Result(OutputOffset + i) = CByte((Low1 >> (i << 3)) And &HFFUL)
            Result(OutputOffset + 8 + i) = CByte((High1 >> (i << 3)) And &HFFUL)
        Next
    End Sub

    Private Sub EncodeMode6(minVal() As Integer, maxVal() As Integer, LocalR() As Integer, LocalG() As Integer, LocalB() As Integer, LocalA() As Integer, ColorTable() As ULong, Result As Byte(), OutputOffset As Integer)
        Dim R0 As Integer = minVal(0) >> 1, R1 As Integer = maxVal(0) >> 1
        Dim G0 As Integer = minVal(1) >> 1, G1 As Integer = maxVal(1) >> 1
        Dim B0 As Integer = minVal(2) >> 1, B1 As Integer = maxVal(2) >> 1
        Dim A0 As Integer = minVal(3) >> 1, A1 As Integer = maxVal(3) >> 1
        Dim MinLum6 As Integer = minVal(2) + (minVal(1) << 1) + minVal(0) + minVal(3)
        Dim MaxLum6 As Integer = maxVal(2) + (maxVal(1) << 1) + maxVal(0) + maxVal(3)
        Dim Range6 As Integer = MaxLum6 - MinLum6
        If Range6 < 1 Then Range6 = 1
        For i As Integer = 0 To 15
            Dim PixLum As Integer = LocalB(i) + (LocalG(i) << 1) + LocalR(i) + LocalA(i)
            Dim Index As Integer = ((PixLum - MinLum6) * 15 + (Range6 >> 1)) \ Range6
            If Index > 15 Then Index = 15
            If Index < 0 Then Index = 0
            ColorTable(i) = CULng(Index)
        Next
        If ColorTable(0) >= 8UL Then
            Dim Temp As Integer
            Temp = R0 : R0 = R1 : R1 = Temp
            Temp = G0 : G0 = G1 : G1 = Temp
            Temp = B0 : B0 = B1 : B1 = Temp
            Temp = A0 : A0 = A1 : A1 = Temp
            For i As Integer = 0 To 15
                ColorTable(i) = 15UL - ColorTable(i)
            Next
        End If
        Dim LowBytes As ULong = &H40UL
        LowBytes = LowBytes Or (CULng(R0) << 7)
        LowBytes = LowBytes Or (CULng(R1) << 14)
        LowBytes = LowBytes Or (CULng(G0) << 21)
        LowBytes = LowBytes Or (CULng(G1) << 28)
        LowBytes = LowBytes Or (CULng(B0) << 35)
        LowBytes = LowBytes Or (CULng(B1) << 42)
        LowBytes = LowBytes Or (CULng(A0) << 49)
        LowBytes = LowBytes Or (CULng(A1) << 56)
        LowBytes = LowBytes Or (1UL << 63)
        Dim HighBytes As ULong = 1UL
        HighBytes = HighBytes Or ((ColorTable(0) And 7UL) << 1)
        For i As Integer = 1 To 15
            HighBytes = HighBytes Or ((ColorTable(i) And 15UL) << (i * 4))
        Next
        For i As Integer = 0 To 7
            Result(OutputOffset + i) = CByte((LowBytes >> (i << 3)) And &HFFUL)
            Result(OutputOffset + 8 + i) = CByte((HighBytes >> (i << 3)) And &HFFUL)
        Next
    End Sub

    Private Sub EncodeMode7(bestPartition As Integer, minVal() As Integer, maxVal() As Integer, indices() As Integer, LocalR() As Integer, LocalG() As Integer, LocalB() As Integer, LocalA() As Integer, Result As Byte(), OutputOffset As Integer)
        Dim subMask = PartitionTable2(bestPartition)
        For i = 0 To 7
            minVal(i) = 255
            maxVal(i) = 0
        Next
        For i = 0 To 15
            Dim sOff = ((subMask >> i) And 1) << 2
            If LocalR(i) < minVal(sOff) Then minVal(sOff) = LocalR(i)
            If LocalR(i) > maxVal(sOff) Then maxVal(sOff) = LocalR(i)
            If LocalG(i) < minVal(sOff + 1) Then minVal(sOff + 1) = LocalG(i)
            If LocalG(i) > maxVal(sOff + 1) Then maxVal(sOff + 1) = LocalG(i)
            If LocalB(i) < minVal(sOff + 2) Then minVal(sOff + 2) = LocalB(i)
            If LocalB(i) > maxVal(sOff + 2) Then maxVal(sOff + 2) = LocalB(i)
            If LocalA(i) < minVal(sOff + 3) Then minVal(sOff + 3) = LocalA(i)
            If LocalA(i) > maxVal(sOff + 3) Then maxVal(sOff + 3) = LocalA(i)
        Next
        Dim eR0 As Integer = minVal(0) >> 3, eR1 As Integer = maxVal(0) >> 3
        Dim eG0 As Integer = minVal(1) >> 3, eG1 As Integer = maxVal(1) >> 3
        Dim eB0 As Integer = minVal(2) >> 3, eB1 As Integer = maxVal(2) >> 3
        Dim eA0 As Integer = minVal(3) >> 3, eA1 As Integer = maxVal(3) >> 3
        Dim minLum0 As Integer = minVal(2) + (minVal(1) << 1) + minVal(0) + minVal(3)
        Dim maxLum0 As Integer = maxVal(2) + (maxVal(1) << 1) + maxVal(0) + maxVal(3)
        Dim range0 As Integer = maxLum0 - minLum0
        If range0 < 1 Then range0 = 1
        Dim eR2 As Integer = minVal(4) >> 3, eR3 As Integer = maxVal(4) >> 3
        Dim eG2 As Integer = minVal(5) >> 3, eG3 As Integer = maxVal(5) >> 3
        Dim eB2 As Integer = minVal(6) >> 3, eB3 As Integer = maxVal(6) >> 3
        Dim eA2 As Integer = minVal(7) >> 3, eA3 As Integer = maxVal(7) >> 3
        Dim minLum1 As Integer = minVal(6) + (minVal(5) << 1) + minVal(4) + minVal(7)
        Dim maxLum1 As Integer = maxVal(6) + (maxVal(5) << 1) + maxVal(4) + maxVal(7)
        Dim range1 As Integer = maxLum1 - minLum1
        If range1 < 1 Then range1 = 1
        For i = 0 To 15
            Dim s = (subMask >> i) And 1
            Dim pixLum = LocalB(i) + (LocalG(i) << 1) + LocalR(i) + LocalA(i)
            Dim index As Integer
            If s = 0 Then
                index = ((pixLum - minLum0) * 3 + (range0 >> 1)) \ range0
            Else
                index = ((pixLum - minLum1) * 3 + (range1 >> 1)) \ range1
            End If
            If index > 3 Then index = 3
            If index < 0 Then index = 0
            indices(i) = index
        Next
        Dim anchor0 As Integer = 0
        Dim anchor1 As Integer = AnchorIndexTable2(bestPartition)
        If indices(anchor0) >= 2 Then
            Dim temp As Integer
            temp = eR0 : eR0 = eR1 : eR1 = temp
            temp = eG0 : eG0 = eG1 : eG1 = temp
            temp = eB0 : eB0 = eB1 : eB1 = temp
            temp = eA0 : eA0 = eA1 : eA1 = temp
            For i = 0 To 15
                If ((subMask >> i) And 1) = 0 Then indices(i) = 3 - indices(i)
            Next
        End If
        If indices(anchor1) >= 2 Then
            Dim temp As Integer
            temp = eR2 : eR2 = eR3 : eR3 = temp
            temp = eG2 : eG2 = eG3 : eG3 = temp
            temp = eB2 : eB2 = eB3 : eB3 = temp
            temp = eA2 : eA2 = eA3 : eA3 = temp
            For i = 0 To 15
                If ((subMask >> i) And 1) = 1 Then indices(i) = 3 - indices(i)
            Next
        End If
        Dim Low1 As ULong = 128UL
        Low1 = Low1 Or (CULng(bestPartition) << 8)
        Low1 = Low1 Or (CULng(eR0) << 14)
        Low1 = Low1 Or (CULng(eR1) << 19)
        Low1 = Low1 Or (CULng(eR2) << 24)
        Low1 = Low1 Or (CULng(eR3) << 29)
        Low1 = Low1 Or (CULng(eG0) << 34)
        Low1 = Low1 Or (CULng(eG1) << 39)
        Low1 = Low1 Or (CULng(eG2) << 44)
        Low1 = Low1 Or (CULng(eG3) << 49)
        Low1 = Low1 Or (CULng(eB0) << 54)
        Low1 = Low1 Or (CULng(eB1) << 59)
        Dim High1 As ULong = CULng(eB2)
        High1 = High1 Or (CULng(eB3) << 5)
        High1 = High1 Or (CULng(eA0) << 10)
        High1 = High1 Or (CULng(eA1) << 15)
        High1 = High1 Or (CULng(eA2) << 20)
        High1 = High1 Or (CULng(eA3) << 25)
        Dim bitOffset As Integer = 34
        For i = 0 To 15
            Dim bits As Integer = If(i = anchor0 OrElse i = anchor1, 1, 2)
            High1 = High1 Or (CULng(indices(i)) << bitOffset)
            bitOffset += bits
        Next
        For i As Integer = 0 To 7
            Result(OutputOffset + i) = CByte((Low1 >> (i << 3)) And &HFFUL)
            Result(OutputOffset + 8 + i) = CByte((High1 >> (i << 3)) And &HFFUL)
        Next
    End Sub

#End Region

    Public Sub Save(FilePath As String)
        If CubeFaces IsNot Nothing Then
            BeginEncodeCube()
        Else
            BeginEncode()
        End If
        File.WriteAllBytes(FilePath, PayloadBytes)
    End Sub

    Private Function CalcMips(Width As Integer, Height As Integer) As Integer
        Dim xMips As Integer = GetDivTwo(Width)
        Dim yMips As Integer = GetDivTwo(Height)
        Return Math.Min(xMips, yMips) + 1
    End Function

    Private Function GetDivTwo(Source As Integer) As Integer
        Dim Count As Integer = 0
        Dim TempSize As Integer = Source
        While TempSize > 1
            TempSize >>= 1
            Count += 1
        End While
        Return Count
    End Function

    Private Function HalveArray(SourceData() As Byte, Width As Integer, Height As Integer) As Byte()
        Dim TempWidth As Integer = Math.Max(1, Width >> 1)
        Dim TempHeight As Integer = Math.Max(1, Height >> 1)
        Dim DestData(TempWidth * TempHeight * 4 - 1) As Byte
        Parallel.For(0, TempHeight, Options, Sub(y)
                                                 Dim destRowOffset As Integer = y * TempWidth * 4
                                                 For x As Integer = 0 To TempWidth - 1
                                                     Dim destPixelOffset As Integer = destRowOffset + (x * 4)
                                                     Dim sumB As Integer = 0, sumG As Integer = 0, sumR As Integer = 0, sumA As Integer = 0
                                                     Dim weightIdx As Integer = 0
                                                     For sy As Integer = -1 To 2
                                                         Dim srcY As Integer = Math.Max(0, Math.Min((y << 1) + sy, Height - 1))
                                                         Dim srcRowOffset As Integer = srcY * Width * 4
                                                         For sx As Integer = -1 To 2
                                                             Dim srcX As Integer = Math.Max(0, Math.Min((x << 1) + sx, Width - 1))
                                                             Dim srcPixelOffset As Integer = srcRowOffset + (srcX * 4)
                                                             Dim w As Integer = Weight4x4(weightIdx)
                                                             weightIdx += 1
                                                             sumB += SourceData(srcPixelOffset) * w
                                                             sumG += SourceData(srcPixelOffset + 1) * w
                                                             sumR += SourceData(srcPixelOffset + 2) * w
                                                             sumA += SourceData(srcPixelOffset + 3) * w
                                                         Next
                                                     Next
                                                     DestData(destPixelOffset) = CByte(Math.Max(0, Math.Min(255, sumB >> 8)))
                                                     DestData(destPixelOffset + 1) = CByte(Math.Max(0, Math.Min(255, sumG >> 8)))
                                                     DestData(destPixelOffset + 2) = CByte(Math.Max(0, Math.Min(255, sumR >> 8)))
                                                     DestData(destPixelOffset + 3) = CByte(Math.Max(0, Math.Min(255, sumA >> 8)))
                                                 Next
                                             End Sub)
        Return DestData
    End Function

    Private Function OrderBytes(Source As Integer) As Byte()
        Return BitConverter.GetBytes(Source)
    End Function

    Private Function OrderBytes(Source As String) As Byte()
        Dim Bytes(3) As Byte
        If Not String.IsNullOrEmpty(Source) Then
            Dim Temp As Byte() = System.Text.Encoding.ASCII.GetBytes(Source)
            Array.Copy(Temp, Bytes, Math.Min(Temp.Length, 4))
        End If
        Return Bytes
    End Function

    Protected Overridable Sub Dispose(Disposing As Boolean)
        If Not Disposed Then
            CubeFaces = Nothing
            HeaderBytes = Nothing
            PayloadBytes = Nothing
        End If
        Disposed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
