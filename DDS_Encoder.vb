' DDS Encoder Class by WalkerMx
' Based on the documentation found here:
' http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddsfileformat.htm
' https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds

Imports System.IO
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
    Public Caps2 As Integer

    Public DXGIFormat As DXGI_Format
    Public ResourceDimension As DX10_ResourceDimension
    Public MiscFlag As DX10_MiscFlags
    Public ArraySize As Integer
    Public MiscFlags2 As DX10_MiscFlags2

    Private SourcePath As String
    Private AlphaMode As Integer
    Private MipMapEnabled As Boolean
    Private CompressionEnabled As Boolean
    Private ExtendedHeaderEnabled As Boolean

    Private MipCount As Integer
    Private BytesPerBlock As Integer

    Private HeaderBytes As Byte()
    Private PayloadBytes As Byte()

    ''' <summary>
    ''' Creates a DDS Image from a standard Image file.
    ''' </summary>
    ''' <param name="Source">Image to create DDS from.</param>
    ''' <param name="Alpha">Alpha support.  0 for Opaque, 1 for 1-bit Alpha, 2 for full 8-bit alpha.</param>
    ''' <param name="Compress">Applies DXT1 or DXT5 compression depending on Alpha Mode.</param>
    ''' <param name="MipMaps">Create mipmaps for distant objects.  Increases file size by ~33%.</param>
    ''' <param name="ExtendedHeader">Add extended DX10 header.  Disable for legacy texture support.</param>
    Public Sub New(Source As String, Alpha As Integer, Compress As Boolean, MipMaps As Boolean, ExtendedHeader As Boolean)

        ResourceDimension = DX10_ResourceDimension.D3D10_RESOURCE_DIMENSION_TEXTURE2D
        MiscFlag = DX10_MiscFlags.D3D10_RESOURCE_MISC_NONE
        RGBBitCount = 32
        FourCC = "DXT1"
        RedBitMask = {0, 0, &HFF, 0}
        GreenBitMask = {0, &HFF, 0, 0}
        BlueBitMask = {&HFF, 0, 0, 0}
        AlphaBitMask = {0, 0, 0, &HFF}

        SourcePath = Source
        AlphaMode = Alpha
        MipMapEnabled = MipMaps
        CompressionEnabled = Compress
        ExtendedHeaderEnabled = ExtendedHeader

        BytesPerBlock = 8

        Using TempImage As Image = Image.FromFile(Source)
            Width = TempImage.Width
            Height = TempImage.Height
        End Using

        SurfaceFlags = DDS_SurfaceFlags.DDSD_CAPS Or DDS_SurfaceFlags.DDSD_PIXELFORMAT Or DDS_SurfaceFlags.DDSD_WIDTH Or DDS_SurfaceFlags.DDSD_HEIGHT
        Caps1 = DDS_Caps1.DDSCAPS_TEXTURE

        If MipMaps Then MipCount = CalcMips(Width, Height)

        If Alpha > 0 Then
            PixelFlags = PixelFlags Or DDS_PixelFlags.DDPF_ALPHAPIXELS
        Else
            AlphaBitMask = {0, 0, 0, 0}
        End If

        If Compress = True Then
            SurfaceFlags = SurfaceFlags Or DDS_SurfaceFlags.DDSD_LINEARSIZE
            PixelFlags = PixelFlags Or DDS_PixelFlags.DDPF_FOURCC
            If Alpha = 2 Then
                FourCC = "DXT5"
                BytesPerBlock = 16
            End If
            PitchLinearSize = Math.Max(1, ((Width + 3) \ 4)) * BytesPerBlock * Math.Max(1, ((Height + 3) \ 4))
            RedBitMask = {0, 0, 0, 0}
            GreenBitMask = {0, 0, 0, 0}
            BlueBitMask = {0, 0, 0, 0}
            AlphaBitMask = {0, 0, 0, 0}
        Else
            FourCC = ""
            SurfaceFlags = SurfaceFlags Or DDS_SurfaceFlags.DDSD_PITCH
            PixelFlags = PixelFlags Or DDS_PixelFlags.DDPF_RGB
            PitchLinearSize = Width * 4
        End If

        If MipMaps Then
            SurfaceFlags = SurfaceFlags Or DDS_SurfaceFlags.DDSD_MIPMAPCOUNT
            Caps1 = Caps1 Or DDS_Caps1.DDSCAPS_COMPLEX
            Caps1 = Caps1 Or DDS_Caps1.DDSCAPS_MIPMAP
        End If

        If ExtendedHeader Then
            FourCC = "DX10"
            RedBitMask = {0, 0, 0, 0}
            GreenBitMask = {0, 0, 0, 0}
            BlueBitMask = {0, 0, 0, 0}
            AlphaBitMask = {0, 0, 0, 0}
            PixelFlags = DDS_PixelFlags.DDPF_FOURCC
            RGBBitCount = 0

            If Compress Then
                Select Case Alpha
                    Case 0
                        DXGIFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM
                        MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE
                    Case 1
                        DXGIFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM
                        MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_STRAIGHT
                    Case 2
                        DXGIFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM
                        MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_STRAIGHT
                End Select
            Else
                Select Case Alpha
                    Case 0
                        DXGIFormat = DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM
                        MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE
                    Case 2
                        DXGIFormat = DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM
                        MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_STRAIGHT
                End Select
            End If

        End If

        WriteHeader()

        ' BeginEncode()

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
            HeaderStream.Write(OrderBytes(0), 0, 4)                         ' dwCaps2

            HeaderStream.Write(New Byte(11) {}, 0, 12)                      ' dwCaps3, dwCaps4, dwReserved2

            If ExtendedHeaderEnabled Then
                HeaderStream.Write(OrderBytes(DXGIFormat), 0, 4)            ' dwDxgiFormat
                HeaderStream.Write(OrderBytes(ResourceDimension), 0, 4)     ' dwResourceDimension
                HeaderStream.Write(OrderBytes(MiscFlag), 0, 4)              ' dwMiscFlag
                HeaderStream.Write(OrderBytes(1), 0, 4)                     ' dwArraySize
                HeaderStream.Write(OrderBytes(MiscFlags2), 0, 4)            ' dwMiscFlags2
            End If

            HeaderBytes = HeaderStream.ToArray

        End Using

    End Sub

    Private Sub BeginEncode()
        Dim TempBytes As Byte()
        Dim TempWidth As Integer = Width
        Dim TempHeight As Integer = Height
        Using PayloadStream As New MemoryStream()
            PayloadStream.Write(HeaderBytes, 0, HeaderBytes.Count)
            Using TempImage As Image = Image.FromFile(SourcePath)
                TempBytes = ExtractBitmapBytes(TempImage)
            End Using
            Dim NextBytes As Byte() = GetImageData(TempBytes, TempWidth, TempHeight)
            PayloadStream.Write(NextBytes, 0, NextBytes.Count)
            If MipMapEnabled Then
                For i = 0 To MipCount - 2
                    TempBytes = HalveArray(TempBytes, TempWidth, TempHeight)
                    TempWidth = Math.Max(1, TempWidth >> 1)
                    TempHeight = Math.Max(1, TempHeight >> 1)
                    NextBytes = GetImageData(TempBytes, TempWidth, TempHeight)
                    PayloadStream.Write(NextBytes, 0, NextBytes.Count)
                Next
            End If
            PayloadBytes = PayloadStream.ToArray
        End Using
    End Sub

    Private Function GetImageData(BitmapBytes As Byte(), Width As Integer, Height As Integer) As Byte()
        If CompressionEnabled Then
            Return BlockCompress(BitmapBytes, Width, Height)
        Else
            Return WriteUncompressed(BitmapBytes, AlphaMode <> 0)
        End If
    End Function

    Private Function ExtractBitmapBytes(Source As Bitmap) As Byte()
        Dim SourceRect As New Rectangle(0, 0, Source.Width, Source.Height)
        Dim SourceData As BitmapData = Source.LockBits(SourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Dim SourceBytes(SourceData.Stride * Source.Height - 1) As Byte
        Marshal.Copy(SourceData.Scan0, SourceBytes, 0, SourceBytes.Length)
        Source.UnlockBits(SourceData)
        Return SourceBytes
    End Function

    Private Function WriteUncompressed(SourceData As Byte(), Alpha As Boolean) As Byte()
        If Alpha Then
            Return DirectCast(SourceData.Clone(), Byte())
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
        Dim IsBC7 As Boolean = (DXGIFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM)
        Dim CutoutAlpha As Boolean = (AlphaMode = 1)
        Dim FullAlpha As Boolean = (AlphaMode = 2)
        Parallel.For(0, BlockHeight, Options, Sub(yBlock)
                                                  Dim yPixelBase As Integer = yBlock * 4
                                                  Dim rowOutputOffset As Integer = yBlock * BlockWidth * BytesPerBlock
                                                  Dim BlockData(63) As Byte
                                                  For xBlock As Integer = 0 To BlockWidth - 1
                                                      Dim xPixelBase As Integer = xBlock * 4
                                                      Dim byteIdx As Integer = 0
                                                      For j As Integer = 0 To 3
                                                          Dim py As Integer = Math.Min(yPixelBase + j, Height - 1)
                                                          Dim rowInputOffset As Integer = py * Width * 4
                                                          For i As Integer = 0 To 3
                                                              Dim px As Integer = Math.Min(xPixelBase + i, Width - 1)
                                                              Dim pixelIdx As Integer = rowInputOffset + (px * 4)
                                                              BlockData(byteIdx) = SourceData(pixelIdx)
                                                              BlockData(byteIdx + 1) = SourceData(pixelIdx + 1)
                                                              BlockData(byteIdx + 2) = SourceData(pixelIdx + 2)
                                                              BlockData(byteIdx + 3) = SourceData(pixelIdx + 3)
                                                              byteIdx += 4
                                                          Next
                                                      Next
                                                      Dim currentBlockOffset As Integer = rowOutputOffset + (xBlock * BytesPerBlock)
                                                      If IsBC7 Then
                                                          Array.Copy(EncodeBlockBC7(BlockData), 0, Result, currentBlockOffset, 16)
                                                      Else
                                                          If FullAlpha Then
                                                              Array.Copy(EncodeAlphaBlockBC3(BlockData), 0, Result, currentBlockOffset, 8)
                                                              currentBlockOffset += 8
                                                          End If
                                                          Array.Copy(EncodeColorBlockBC1(BlockData, Not CutoutAlpha), 0, Result, currentBlockOffset, 8)
                                                      End If
                                                  Next
                                              End Sub)
        Return Result
    End Function

    Private Function EncodeBlockBC7(BlockData As Byte()) As Byte()
        Dim Result(15) As Byte
        Dim MinB As Integer = 255, MaxB As Integer = 0
        Dim MinG As Integer = 255, MaxG As Integer = 0
        Dim MinR As Integer = 255, MaxR As Integer = 0
        Dim MinA As Integer = 255, MaxA As Integer = 0
        For i As Integer = 0 To 15
            Dim Offset As Integer = i * 4
            Dim valB As Integer = BlockData(Offset)
            Dim valG As Integer = BlockData(Offset + 1)
            Dim valR As Integer = BlockData(Offset + 2)
            Dim valA As Integer = BlockData(Offset + 3)
            If valB < MinB Then MinB = valB
            If valB > MaxB Then MaxB = valB
            If valG < MinG Then MinG = valG
            If valG > MaxG Then MaxG = valG
            If valR < MinR Then MinR = valR
            If valR > MaxR Then MaxR = valR
            If valA < MinA Then MinA = valA
            If valA > MaxA Then MaxA = valA
        Next
        Dim R0 As Integer = MinR >> 1, R1 As Integer = MaxR >> 1
        Dim G0 As Integer = MinG >> 1, G1 As Integer = MaxG >> 1
        Dim B0 As Integer = MinB >> 1, B1 As Integer = MaxB >> 1
        Dim A0 As Integer = MinA >> 1, A1 As Integer = MaxA >> 1
        Dim ColorTable(15) As ULong
        Dim MinLum As Integer = MinB + (MinG << 1) + MinR + MinA
        Dim MaxLum As Integer = MaxB + (MaxG << 1) + MaxR + MaxA
        Dim Range As Single = If(MaxLum - MinLum < 1, 1.0F, CSng(MaxLum - MinLum))
        For i As Integer = 0 To 15
            Dim Offset As Integer = i * 4
            Dim PixB As Integer = BlockData(Offset)
            Dim PixG As Integer = BlockData(Offset + 1)
            Dim PixR As Integer = BlockData(Offset + 2)
            Dim PixA As Integer = BlockData(Offset + 3)
            Dim PixLum As Integer = PixB + (PixG << 1) + PixR + PixA
            Dim Index As Integer = CInt(Math.Round(((PixLum - MinLum) / Range) * 15.0F))
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
        Dim LowBytes As ULong = 0
        LowBytes = LowBytes Or &H40UL
        LowBytes = LowBytes Or (CULng(R0) << 7)
        LowBytes = LowBytes Or (CULng(R1) << 14)
        LowBytes = LowBytes Or (CULng(G0) << 21)
        LowBytes = LowBytes Or (CULng(G1) << 28)
        LowBytes = LowBytes Or (CULng(B0) << 35)
        LowBytes = LowBytes Or (CULng(B1) << 42)
        LowBytes = LowBytes Or (CULng(A0) << 49)
        LowBytes = LowBytes Or (CULng(A1) << 56)
        LowBytes = LowBytes Or (1UL << 63)
        Dim HighBytes As ULong = 0
        HighBytes = HighBytes Or 1UL
        HighBytes = HighBytes Or ((ColorTable(0) And 7UL) << 1)
        For i As Integer = 1 To 15
            HighBytes = HighBytes Or ((ColorTable(i) And 15UL) << (i * 4))
        Next
        Array.Copy(BitConverter.GetBytes(LowBytes), 0, Result, 0, 8)
        Array.Copy(BitConverter.GetBytes(HighBytes), 0, Result, 8, 8)
        Return Result
    End Function

    Private Function EncodeAlphaBlockBC3(BlockData As Byte()) As Byte()
        Dim AlphaArray(15) As Byte
        For i As Integer = 0 To 15
            AlphaArray(i) = BlockData((i * 4) + 3)
        Next
        Dim Result(7) As Byte
        Dim Alpha0 As Byte = AlphaArray.Max()
        Dim Alpha1 As Byte = AlphaArray.Min()
        If Alpha0 = Alpha1 Then
            If Alpha0 > 0 Then
                Alpha1 -= 1
            Else
                Alpha0 += 1
            End If
        End If
        Result(0) = Alpha0
        Result(1) = Alpha1
        Dim BitBuffer As Long = 0
        Dim BitsLoaded As Integer = 0
        Dim ByteOffset As Integer = 2
        For Each a In AlphaArray
            Dim Index As Byte
            Dim Range As Integer = CInt(Alpha0) - Alpha1
            If a = Alpha0 Then
                Index = 0
            ElseIf a = Alpha1 Then
                Index = 1
            Else
                Index = CByte(Clamp(7 - ((CInt(a) - Alpha1) * 7 \ Range), 2, 7))
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
        Return Result
    End Function

    Private Function EncodeColorBlockBC1(BlockData As Byte(), ForceOpaque As Boolean) As Byte()
        Dim PixelArray(15) As UShort
        For i As Integer = 0 To 15
            Dim offset As Integer = i * 4
            Dim b As Integer = BlockData(offset)
            Dim g As Integer = BlockData(offset + 1)
            Dim r As Integer = BlockData(offset + 2)
            Dim a As Integer = BlockData(offset + 3)
            If Not ForceOpaque AndAlso a < 128 Then
                PixelArray(i) = 0
            Else
                PixelArray(i) = CUShort(((r And &HF8) << 8) Or ((g And &HFC) << 3) Or (b >> 3))
            End If
        Next
        Dim Col0 As UShort, Col1 As UShort
        Dim MaxLum As Double = -1.0, MinLum As Double = 1000.0
        For Each Pixel In PixelArray
            Dim Lum As Double = (0.299 * ((Pixel >> 11) << 3)) + (0.587 * (((Pixel >> 5) And &H3F) << 2)) + (0.114 * ((Pixel And &H1F) << 3))
            If Lum > MaxLum Then MaxLum = Lum : Col0 = Pixel
            If Lum < MinLum Then MinLum = Lum : Col1 = Pixel
        Next
        If ForceOpaque Then
            If Col0 < Col1 Then
                Swap(Col0, Col1)
            Else
                If Col0 = Col1 Then
                    If Col0 > 0 Then
                        Col1 -= 1US
                    Else
                        Col0 += 1US
                    End If
                End If
            End If
        ElseIf Col0 > Col1 Then
            Swap(Col0, Col1)
        End If
        Dim RVals(3) As Integer, GVals(3) As Integer, BVals(3) As Integer
        RVals(0) = (Col0 >> 11) << 3 : GVals(0) = ((Col0 >> 5) And &H3F) << 2 : BVals(0) = (Col0 And &H1F) << 3
        RVals(1) = (Col1 >> 11) << 3 : GVals(1) = ((Col1 >> 5) And &H3F) << 2 : BVals(1) = (Col1 And &H1F) << 3
        If ForceOpaque Then
            RVals(2) = (2 * RVals(0) + RVals(1)) \ 3 : GVals(2) = (2 * GVals(0) + GVals(1)) \ 3 : BVals(2) = (2 * BVals(0) + BVals(1)) \ 3
            RVals(3) = (RVals(0) + 2 * RVals(1)) \ 3 : GVals(3) = (GVals(0) + 2 * GVals(1)) \ 3 : BVals(3) = (BVals(0) + 2 * BVals(1)) \ 3
        Else
            RVals(2) = (RVals(0) + RVals(1)) \ 2 : GVals(2) = (GVals(0) + GVals(1)) \ 2 : BVals(2) = (BVals(0) + BVals(1)) \ 2
        End If
        Dim ColorTable As UInteger = 0
        For i As Integer = 0 To 15
            Dim Pixel As UShort = PixelArray(i)
            Dim Index As UInteger = 0
            If Not ForceOpaque AndAlso Pixel = 0 Then
                Index = 3
            Else
                Dim PixR As UShort = (Pixel >> 11) << 3
                Dim PixG As UShort = ((Pixel >> 5) And &H3F) << 2
                Dim PixB As UShort = (Pixel And &H1F) << 3
                Dim MinError As Long = Long.MaxValue
                Dim Count = If(ForceOpaque, 3, 2)
                For k = 0 To Count
                    Dim DistR As Integer = PixR - RVals(k)
                    Dim DistG As Integer = PixG - GVals(k)
                    Dim DistB As Integer = PixB - BVals(k)
                    Dim CurrentError As Long = (DistR * DistR) + (DistG * DistG) + (DistB * DistB)
                    If CurrentError < MinError Then MinError = CurrentError : Index = CUInt(k)
                Next
            End If
            ColorTable = ColorTable Or (Index << (i * 2))
        Next
        Dim Result(7) As Byte
        BitConverter.GetBytes(Col0).CopyTo(Result, 0)
        BitConverter.GetBytes(Col1).CopyTo(Result, 2)
        BitConverter.GetBytes(ColorTable).CopyTo(Result, 4)
        Return Result
    End Function

    Public Sub SaveImage(FilePath As String)
        BeginEncode()
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
        For y As Integer = 0 To TempHeight - 1
            Dim srcY0 As Integer = (y << 1) * Width * 4
            Dim srcY1 As Integer = Math.Min((y << 1) + 1, Height - 1) * Width * 4
            Dim destRowOffset As Integer = y * TempWidth * 4
            For x As Integer = 0 To TempWidth - 1
                Dim x0 As Integer = (x << 1) * 4
                Dim x1 As Integer = Math.Min((x << 1) + 1, Width - 1) * 4
                Dim destPixelOffset As Integer = destRowOffset + (x * 4)
                For c As Integer = 0 To 3
                    Dim sum As Integer = CInt(SourceData(srcY0 + x0 + c)) + SourceData(srcY0 + x1 + c) + SourceData(srcY1 + x0 + c) + SourceData(srcY1 + x1 + c)
                    DestData(destPixelOffset + c) = CByte(sum >> 2)
                Next
            Next
        Next
        Return DestData
    End Function

    Private Function OrderBytes(Source As Integer) As Byte()
        Return BitConverter.GetBytes(Source)
    End Function

    Private Function OrderBytes(Source As String) As Byte()
        Dim Bytes(3) As Byte
        If Not String.IsNullOrEmpty(Source) Then
            Dim Temp As Byte() = Text.Encoding.ASCII.GetBytes(Source)
            Array.Copy(Temp, Bytes, Math.Min(Temp.Length, 4))
        End If
        Return Bytes
    End Function

    Protected Overridable Sub Dispose(Disposing As Boolean)
        If Not Disposed Then
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
