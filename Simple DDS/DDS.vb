' DDS Class by WalkerMx
' Based on the documentation found here:
' http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddsfileformat.htm

Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class DDS
    Implements IDisposable

    Public Disposed As Boolean

    Private HeaderBytes As New List(Of Byte)
    Private PayloadBytes As New List(Of Byte)

    Private Enum SurfaceFlags
        DDSD_CAPS = &H1
        DDSD_HEIGHT = &H2
        DDSD_WIDTH = &H4
        DDSD_PITCH = &H8
        DDSD_PIXELFORMAT = &H1000
        DDSD_MIPMAPCOUNT = &H20000
        DDSD_LINEARSIZE = &H80000
        DDSD_DEPTH = &H800000
    End Enum

    Private Enum PixelFlags
        DDPF_ALPHAPIXELS = &H1
        DDPF_FOURCC = &H4
        DDPF_RGB = &H40
    End Enum

    Private Enum Caps1
        DDSCAPS_COMPLEX = &H8
        DDSCAPS_TEXTURE = &H1000
        DDSCAPS_MIPMAP = &H400000
    End Enum

    Private Enum DXGI_Format
        DXGI_FORMAT_UNKNOWN = &H0
        DXGI_FORMAT_BC1_UNORM = &H47
        DXGI_FORMAT_BC3_UNORM = &H4D
        DXGI_FORMAT_B8G8R8A8_UNORM = &H57
        DXGI_FORMAT_B8G8R8X8_UNORM = &H58
    End Enum

    Private Enum DX10_ResourceDimension As Integer
        D3D10_RESOURCE_DIMENSION_UNKNOWN = &H0
        D3D10_RESOURCE_DIMENSION_BUFFER = &H1
        D3D10_RESOURCE_DIMENSION_TEXTURE1D = &H2
        D3D10_RESOURCE_DIMENSION_TEXTURE2D = &H3
        D3D10_RESOURCE_DIMENSION_TEXTURE3D = &H4
    End Enum

    Private Enum DX10_MiscFlags As Integer
        D3D10_RESOURCE_MISC_NONE = &H0
        D3D10_RESOURCE_MISC_TEXTURECUBE = &H4
    End Enum

    Private Enum DX10_AlphaMode As Integer
        DDS_ALPHA_MODE_UNKNOWN = &H0
        DDS_ALPHA_MODE_STRAIGHT = &H1
        DDS_ALPHA_MODE_PREMULTIPLIED = &H2
        DDS_ALPHA_MODE_OPAQUE = &H3
        DDS_ALPHA_MODE_CUSTOM = &H4
    End Enum

    ''' <summary>
    ''' Creates a DDS Image from a standard Image file.
    ''' </summary>
    ''' <param name="SourceImage">Image to create DDS from.</param>
    ''' <param name="Alpha">Alpha support.  0 for Opaque, 1 for 1-bit Alpha, 2 for full 8-bit alpha.</param>
    ''' <param name="Compress">Applies DXT1 or DXT5 compression depending on Alpha Mode.</param>
    ''' <param name="MipMaps">Create mipmaps for distant objects.  Increases file size by ~33%.</param>
    ''' <param name="ExtendedHeader">Add extended DX10 header.  Disable for legacy texture support.</param>
    ''' <param name="HighQuality">Use advanced proccessing for compressing blocks at the cost speed.</param>
    Public Sub New(SourceImage As Image, Alpha As Integer, Compress As Boolean, MipMaps As Boolean, ExtendedHeader As Boolean, Optional HighQuality As Boolean = True)

        Dim DDS_SurfaceFlags As New List(Of SurfaceFlags)
        Dim DDS_PixelFlags As New List(Of PixelFlags)
        Dim DDS_Caps1 As New List(Of Caps1)
        Dim DDS_DXGIFormat As New DXGI_Format
        Dim DDS_ResourceDimension As DX10_ResourceDimension = DX10_ResourceDimension.D3D10_RESOURCE_DIMENSION_TEXTURE2D
        Dim DDS_MiscFlag As DX10_MiscFlags = DX10_MiscFlags.D3D10_RESOURCE_MISC_NONE
        Dim DDS_MiscFlag2 As New DX10_AlphaMode
        Dim DDS_RGBBitCount As Integer = 32
        Dim PLS As Integer
        Dim FourCC As String = "DXT1"
        Dim BytesPerBlock As Integer = 8
        Dim MipCount As Integer = 0
        Dim RMask As Byte() = {0, 0, &HFF, 0}
        Dim GMask As Byte() = {0, &HFF, 0, 0}
        Dim BMask As Byte() = {&HFF, 0, 0, 0}
        Dim AMask As Byte() = {0, 0, 0, &HFF}

        DDS_SurfaceFlags.AddRange({SurfaceFlags.DDSD_CAPS, SurfaceFlags.DDSD_PIXELFORMAT, SurfaceFlags.DDSD_WIDTH, SurfaceFlags.DDSD_HEIGHT})
        DDS_Caps1.Add(Caps1.DDSCAPS_TEXTURE)

        If MipMaps Then MipCount = CalcMips(SourceImage.Width, SourceImage.Height)

        If Alpha > 0 Then
            DDS_PixelFlags.Add(PixelFlags.DDPF_ALPHAPIXELS)
        Else
            AMask = {0, 0, 0, 0}
        End If

        If Compress = True Then
            DDS_SurfaceFlags.Add(SurfaceFlags.DDSD_LINEARSIZE)
            DDS_PixelFlags.Add(PixelFlags.DDPF_FOURCC)
            If Alpha = 2 Then
                FourCC = "DXT5"
                BytesPerBlock = 16
            End If
            PLS = Math.Max(1, ((SourceImage.Width + 3) \ 4)) * BytesPerBlock * Math.Max(1, ((SourceImage.Height + 3) \ 4))
            RMask = {0, 0, 0, 0}
            GMask = {0, 0, 0, 0}
            BMask = {0, 0, 0, 0}
            AMask = {0, 0, 0, 0}
        Else
            FourCC = ""
            DDS_SurfaceFlags.Add(SurfaceFlags.DDSD_PITCH)
            DDS_PixelFlags.Add(PixelFlags.DDPF_RGB)
            PLS = SourceImage.Width * 4
        End If

        If MipMaps Then
            DDS_SurfaceFlags.Add(SurfaceFlags.DDSD_MIPMAPCOUNT)
            DDS_Caps1.Add(Caps1.DDSCAPS_COMPLEX)
            DDS_Caps1.Add(Caps1.DDSCAPS_MIPMAP)
        End If

        If ExtendedHeader = True Then
            FourCC = "DX10"
            RMask = {0, 0, 0, 0}
            GMask = {0, 0, 0, 0}
            BMask = {0, 0, 0, 0}
            AMask = {0, 0, 0, 0}
            DDS_PixelFlags.Clear()
            DDS_PixelFlags.Add(PixelFlags.DDPF_FOURCC)
            DDS_RGBBitCount = 0
            Select Case Alpha
                Case 0
                    If Compress Then
                        DDS_DXGIFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM
                    Else
                        DDS_DXGIFormat = DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM
                    End If
                    DDS_MiscFlag2 = DX10_AlphaMode.DDS_ALPHA_MODE_OPAQUE
                Case 1
                    DDS_DXGIFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM
                    DDS_MiscFlag2 = DX10_AlphaMode.DDS_ALPHA_MODE_STRAIGHT
                Case 2
                    If Compress Then
                        DDS_DXGIFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM
                    Else
                        DDS_DXGIFormat = DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM
                    End If
                    DDS_MiscFlag2 = DX10_AlphaMode.DDS_ALPHA_MODE_STRAIGHT
            End Select
        End If

        HeaderBytes.AddRange(OrderBytes("DDS "))                                ' dwMagic
        HeaderBytes.AddRange(OrderBytes(124))                                   ' dwSize
        HeaderBytes.AddRange(OrderBytes(DDS_SurfaceFlags.ToArray))              ' dwFlags
        HeaderBytes.AddRange(OrderBytes(SourceImage.Height))                    ' dwHeight
        HeaderBytes.AddRange(OrderBytes(SourceImage.Width))                     ' dwWidth
        HeaderBytes.AddRange(OrderBytes(PLS))                                   ' dwPitchOrLinearSize
        HeaderBytes.AddRange(OrderBytes(0))                                     ' dwDepth
        HeaderBytes.AddRange(OrderBytes(MipCount))                              ' dwMipMapCount

        For i = 0 To 10
            HeaderBytes.AddRange(OrderBytes(0))                                 ' dwReserved1 x 11
        Next

        HeaderBytes.AddRange(OrderBytes(32))                                    ' DDPIXELFORMAT dwSize
        HeaderBytes.AddRange(OrderBytes(DDS_PixelFlags.ToArray))                ' DDPIXELFORMAT dwFlags
        HeaderBytes.AddRange(OrderBytes(FourCC))                                ' DDPIXELFORMAT dwFourCC
        HeaderBytes.AddRange(OrderBytes(DDS_RGBBitCount))                       ' DDPIXELFORMAT dwRGBBitCount
        HeaderBytes.AddRange(RMask)                                             ' DDPIXELFORMAT dwRBitMask
        HeaderBytes.AddRange(GMask)                                             ' DDPIXELFORMAT dwGBitMask
        HeaderBytes.AddRange(BMask)                                             ' DDPIXELFORMAT dwBBitMask
        HeaderBytes.AddRange(AMask)                                             ' DDPIXELFORMAT dwABitMask

        HeaderBytes.AddRange(OrderBytes(DDS_Caps1.ToArray))                     ' DDCAPS2 dwCaps1
        HeaderBytes.AddRange(OrderBytes(0))                                     ' DDCAPS2 dwCaps2 (Unused)

        For i = 0 To 1
            HeaderBytes.AddRange(OrderBytes(0))                                 ' DDCAPS2 Reserved x 2
        Next

        HeaderBytes.AddRange(OrderBytes(0))                                     ' dwReserved2

        If ExtendedHeader Then
            HeaderBytes.AddRange(OrderBytes(DDS_DXGIFormat))                    ' dwDxgiFormat
            HeaderBytes.AddRange(OrderBytes(DDS_ResourceDimension))             ' dwResourceDimension
            HeaderBytes.AddRange(OrderBytes(DDS_MiscFlag))                      ' dwMiscFlag
            HeaderBytes.AddRange(OrderBytes(1))                                 ' dwArraySize
            HeaderBytes.AddRange(OrderBytes(DDS_MiscFlag2))                     ' dwMiscFlags2
        End If

        Dim CurrentW As Integer = SourceImage.Width
        Dim CurrentH As Integer = SourceImage.Height
        Dim CurrentBytes As Byte() = ExtractBitmapBytes(SourceImage)

        SourceImage.Dispose()

        PayloadBytes = GetImageData(CurrentBytes, CurrentW, CurrentH, Alpha, Compress, HighQuality).ToList()

        If MipMaps Then
            For i = 0 To MipCount - 2
                CurrentBytes = HalveArray(CurrentBytes, CurrentW, CurrentH)
                CurrentW = Math.Max(1, CurrentW >> 1)
                CurrentH = Math.Max(1, CurrentH >> 1)
                PayloadBytes.AddRange(GetImageData(CurrentBytes, CurrentW, CurrentH, Alpha, Compress, HighQuality))
            Next
        End If

    End Sub

    Private Function GetImageData(BitmapBytes As Byte(), Width As Integer, Height As Integer, AlphaMode As Integer, Compress As Boolean, HighQuality As Boolean) As Byte()
        If Compress Then
            Return BlockCompress(BitmapBytes, Width, Height, AlphaMode, HighQuality)
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

    Private Function BlockCompress(SourceData As Byte(), Width As Integer, Height As Integer, AlphaLevel As Integer, HighQuality As Boolean) As Byte()
        Dim IsBC3 As Boolean = (AlphaLevel = 2)
        Dim BytesPerBlock As Integer = If(IsBC3, 16, 8)
        Dim BlocksWide As Integer = Math.Max(1, (Width + 3) \ 4)
        Dim BlocksHigh As Integer = Math.Max(1, (Height + 3) \ 4)
        Dim Result(BlocksWide * BlocksHigh * BytesPerBlock - 1) As Byte
        Dim Options As New ParallelOptions With {.MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount \ 2)}
        Parallel.For(0, BlocksHigh, Options, Sub(yBlock)
                                                 Dim yPixelBase As Integer = yBlock * 4
                                                 Dim rowOutputOffset As Integer = yBlock * BlocksWide * BytesPerBlock
                                                 For xBlock As Integer = 0 To BlocksWide - 1
                                                     Dim xPixelBase As Integer = xBlock * 4
                                                     Dim BlockColors(15) As UShort
                                                     Dim BlockAlphas(15) As Byte
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
                                                             If AlphaLevel = 1 AndAlso a < 128 Then
                                                                 BlockColors(j * 4 + i) = 0
                                                             Else
                                                                 BlockColors(j * 4 + i) = CType(((CInt(r) And &HF8) << 8) Or ((CInt(g) And &HFC) << 3) Or (CInt(b) >> 3), UShort)
                                                             End If
                                                         Next
                                                     Next
                                                     Dim currentBlockOffset As Integer = rowOutputOffset + (xBlock * BytesPerBlock)
                                                     If IsBC3 Then
                                                         Array.Copy(EncodeAlphaBlockBC3(BlockAlphas), 0, Result, currentBlockOffset, 8)
                                                         currentBlockOffset += 8
                                                     End If
                                                     Array.Copy(EncodeColorBlockBC1(BlockColors, AlphaLevel <> 1, HighQuality), 0, Result, currentBlockOffset, 8)
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
        Dim Result(7) As Byte
        Dim Col0 As UShort
        Dim Col1 As UShort
        If HighQuality = False Then
            Col0 = PixelArray.Max()
            Col1 = PixelArray.Min()
        Else
            Dim Lum0 As Double = -1
            Dim Lum1 As Double = 1000
            For Each Pixel In PixelArray
                Dim RVal As Integer = (Pixel >> 11) << 3
                Dim GVal As Integer = ((Pixel >> 5) And &H3F) << 2
                Dim BVal As Integer = (Pixel And &H1F) << 3
                Dim Lum As Double = (0.299 * RVal) + (0.587 * GVal) + (0.114 * BVal)
                If Lum > Lum0 Then Lum0 = Lum : Col0 = Pixel
                If Lum < Lum1 Then Lum1 = Lum : Col1 = Pixel
            Next
        End If
        If ForceOpaque Then
            If Col0 < Col1 Then
                Swap(Col0, Col1)
            ElseIf Col0 = Col1 Then
                If Col0 > 0 Then Col1 -= 1US Else Col0 += 1US
            End If
        Else
            If Col0 > Col1 Then
                Swap(Col0, Col1)
            End If
        End If
        Result(0) = CByte(Col0 And &HFF)
        Result(1) = CByte(Col0 >> 8)
        Result(2) = CByte(Col1 And &HFF)
        Result(3) = CByte(Col1 >> 8)
        Dim R0 As Integer = (Col0 >> 11) << 3
        Dim G0 As Integer = ((Col0 >> 5) And &H3F) << 2
        Dim B0 As Integer = (Col0 And &H1F) << 3
        Dim R1 As Integer = (Col1 >> 11) << 3
        Dim G1 As Integer = ((Col1 >> 5) And &H3F) << 2
        Dim B1 As Integer = (Col1 And &H1F) << 3
        Dim C(3, 2) As Integer
        C(0, 0) = R0
        C(0, 1) = G0
        C(0, 2) = B0
        C(1, 0) = R1
        C(1, 1) = G1
        C(1, 2) = B1
        If ForceOpaque Then
            C(2, 0) = (2 * R0 + R1) \ 3
            C(2, 1) = (2 * G0 + G1) \ 3
            C(2, 2) = (2 * B0 + B1) \ 3
            C(3, 0) = (R0 + 2 * R1) \ 3
            C(3, 1) = (G0 + 2 * G1) \ 3
            C(3, 2) = (B0 + 2 * B1) \ 3
        Else
            C(2, 0) = (R0 + R1) \ 2
            C(2, 1) = (G0 + G1) \ 2
            C(2, 2) = (B0 + B1) \ 2
        End If
        Dim Midpoint As Integer = ((Col0 And &HF7DE) >> 1) + ((Col1 And &HF7DE) >> 1)
        Dim Offset As Integer = 4
        For j As Integer = 0 To 3
            Dim BitByte As Byte = 0
            For i As Integer = 3 To 0 Step -1
                Dim Pixel As UShort = PixelArray(j * 4 + i)
                Dim Index As Byte = 0
                If Not ForceOpaque AndAlso Pixel = 0 Then
                    Index = &B11
                ElseIf HighQuality = False Then
                    If Pixel = Col0 Then
                        Index = &B0
                    ElseIf Pixel = Col1 Then
                        Index = &B1
                    Else
                        Index = If(Pixel > Midpoint, &B10, &B11)
                    End If
                Else
                    Dim PixelR As Integer = (Pixel >> 11) << 3
                    Dim PixelG As Integer = ((Pixel >> 5) And &H3F) << 2
                    Dim PixelB As Integer = (Pixel And &H1F) << 3
                    Dim MinError As Long = Long.MaxValue
                    Dim Count As Integer = If(ForceOpaque, 3, 2)
                    For k As Integer = 0 To Count
                        Dim Distance As Long = CLng(PixelR - C(k, 0)) ^ 2 + CLng(PixelG - C(k, 1)) ^ 2 + CLng(PixelB - C(k, 2)) ^ 2
                        If Distance < MinError Then
                            MinError = Distance
                            Index = CByte(k)
                        End If
                    Next
                End If
                BitByte = CByte((BitByte << 2) Or Index)
            Next
            Result(Offset) = BitByte : Offset += 1
        Next
        Return Result
    End Function

    Public Sub SaveImage(FilePath As String)
        Dim FileBytes As New List(Of Byte)
        FileBytes.AddRange(HeaderBytes)
        FileBytes.AddRange(PayloadBytes)
        IO.File.WriteAllBytes(FilePath, FileBytes.ToArray)
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
        Dim NewW As Integer = Math.Max(1, Width >> 1)
        Dim NewH As Integer = Math.Max(1, Height >> 1)
        Dim DestData(NewW * NewH * 4 - 1) As Byte
        For y As Integer = 0 To NewH - 1
            Dim srcY0 As Integer = (y << 1) * Width * 4
            Dim srcY1 As Integer = Math.Min((y << 1) + 1, Height - 1) * Width * 4
            Dim destRowOffset As Integer = y * NewW * 4
            For x As Integer = 0 To NewW - 1
                Dim x0 As Integer = (x << 1) * 4
                Dim x1 As Integer = Math.Min((x << 1) + 1, Width - 1) * 4
                Dim destPixelOffset As Integer = destRowOffset + (x * 4)
                For c As Integer = 0 To 3
                    Dim sum As Integer = CInt(SourceData(srcY0 + x0 + c)) +
                                     SourceData(srcY0 + x1 + c) +
                                     SourceData(srcY1 + x0 + c) +
                                     SourceData(srcY1 + x1 + c)
                    DestData(destPixelOffset + c) = CByte(sum >> 2)
                Next
            Next
        Next
        Return DestData
    End Function

    Private Function OrderBytes(Source As Integer()) As Byte()
        Dim Result As Integer = 0
        For Each f In Source
            Result = Result Or f
        Next
        Return BitConverter.GetBytes(Result)
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

    Private Function Clamp(Value As Integer, MinValue As Integer, MaxValue As Integer) As Integer
        Return Math.Max(MinValue, Math.Min(Value, MaxValue))
    End Function

    Private Sub Swap(Of T)(ByRef Value1 As T, ByRef Value2 As T)
        Dim Temp As T = Value1
        Value1 = Value2
        Value2 = Temp
    End Sub

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
