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
    Private HighQualityEnabled As Boolean
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
    ''' <param name="HighQuality">Use advanced proccessing for compressing blocks at the cost speed.</param>
    Public Sub New(Source As String, Alpha As Integer, Compress As Boolean, MipMaps As Boolean, ExtendedHeader As Boolean, Optional HighQuality As Boolean = True)

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
        HighQualityEnabled = HighQuality
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

        If ExtendedHeader = True Then
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
                        DXGIFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM
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

            HeaderStream.Seek(44, SeekOrigin.Current)                       ' dwReserved1 x11

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

            HeaderStream.Seek(12, SeekOrigin.Current)                       ' dwCaps3, dwCaps4, dwReserved2

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
        Parallel.For(0, BlockHeight, Options, Sub(yBlock)
                                                  Dim yPixelBase As Integer = yBlock * 4
                                                  Dim rowOutputOffset As Integer = yBlock * BlockWidth * BytesPerBlock
                                                  For xBlock As Integer = 0 To BlockWidth - 1
                                                      Dim xPixelBase As Integer = xBlock * 4
                                                      Dim BlockColors As UShort() = New UShort(15) {}
                                                      Dim BlockAlphas As Byte() = New Byte(15) {}
                                                      For j As Integer = 0 To 3
                                                          Dim py As Integer = Math.Min(yPixelBase + j, Height - 1)
                                                          Dim rowInputOffset As Integer = py * Width * 4
                                                          For i As Integer = 0 To 3
                                                              Dim px As Integer = Math.Min(xPixelBase + i, Width - 1)
                                                              Dim pixelIdx As Integer = rowInputOffset + (px * 4)
                                                              Dim b As Byte = SourceData(pixelIdx)
                                                              Dim g As Byte = SourceData(pixelIdx + 1)
                                                              Dim r As Byte = SourceData(pixelIdx + 2)
                                                              Dim a As Byte = SourceData(pixelIdx + 3)
                                                              BlockAlphas(j * 4 + i) = a
                                                              If AlphaMode = 1 AndAlso a < 128 Then
                                                                  BlockColors(j * 4 + i) = 0
                                                              Else
                                                                  BlockColors(j * 4 + i) = CType(((CInt(r) And &HF8) << 8) Or ((CInt(g) And &HFC) << 3) Or (CInt(b) >> 3), UShort)
                                                              End If
                                                          Next
                                                      Next
                                                      Dim currentBlockOffset As Integer = rowOutputOffset + (xBlock * BytesPerBlock)
                                                      If AlphaMode = 2 Then
                                                          Array.Copy(EncodeAlphaBlockBC3(BlockAlphas), 0, Result, currentBlockOffset, 8)
                                                          currentBlockOffset += 8
                                                      End If
                                                      Array.Copy(EncodeColorBlockBC1(BlockColors, AlphaMode <> 1, HighQualityEnabled), 0, Result, currentBlockOffset, 8)
                                                  Next
                                              End Sub)
        Return Result
    End Function

    Private Function EncodeAlphaBlockBC3(AlphaArray As Byte()) As Byte()
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

    Private Function EncodeColorBlockBC1(PixelArray As UShort(), ForceOpaque As Boolean, HighQuality As Boolean) As Byte()
        Dim Col0 As UShort
        Dim Col1 As UShort
        If Not HighQuality Then
            Col0 = PixelArray(0) : Col1 = PixelArray(0)
            For Each Pixel In PixelArray
                If Pixel > Col0 Then Col0 = Pixel Else If Pixel < Col1 Then Col1 = Pixel
            Next
        Else
            Dim MaxLum As Double = -1.0
            Dim MinLum As Double = 1000.0
            For Each Pixel In PixelArray
                Dim Lum As Double = (0.299 * ((Pixel >> 11) << 3)) + (0.587 * (((Pixel >> 5) And &H3F) << 2)) + (0.114 * ((Pixel And &H1F) << 3))
                If Lum > MaxLum Then MaxLum = Lum : Col0 = Pixel
                If Lum < MinLum Then MinLum = Lum : Col1 = Pixel
            Next
        End If
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
        Dim RVals(3) As Integer
        Dim GVals(3) As Integer
        Dim BVals(3) As Integer
        RVals(0) = (Col0 >> 11) << 3 : GVals(0) = ((Col0 >> 5) And &H3F) << 2 : BVals(0) = (Col0 And &H1F) << 3
        RVals(1) = (Col1 >> 11) << 3 : GVals(1) = ((Col1 >> 5) And &H3F) << 2 : BVals(1) = (Col1 And &H1F) << 3
        If ForceOpaque Then
            RVals(2) = (2 * RVals(0) + RVals(1)) \ 3 : GVals(2) = (2 * GVals(0) + GVals(1)) \ 3 : BVals(2) = (2 * BVals(0) + BVals(1)) \ 3
            RVals(3) = (RVals(0) + 2 * RVals(1)) \ 3 : GVals(3) = (GVals(0) + 2 * GVals(1)) \ 3 : BVals(3) = (BVals(0) + 2 * BVals(1)) \ 3
        Else
            RVals(2) = (RVals(0) + RVals(1)) \ 2 : GVals(2) = (GVals(0) + GVals(1)) \ 2 : BVals(2) = (BVals(0) + BVals(1)) \ 2
        End If
        Dim ColorTable As UInteger = 0
        Dim Midpoint As Integer = ((Col0 And &HF7DE) >> 1) + ((Col1 And &HF7DE) >> 1)
        For i As Integer = 0 To 15
            Dim Pixel As UShort = PixelArray(i)
            Dim Index As UInteger = 0
            If Not ForceOpaque AndAlso Pixel = 0 Then
                Index = 3
            ElseIf Not HighQuality Then
                If Pixel = Col0 Then Index = 0 Else If Pixel = Col1 Then Index = 1 Else Index = If(Pixel > Midpoint, 2, 3)
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
