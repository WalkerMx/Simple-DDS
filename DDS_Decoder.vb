' DDS Decoder Class by WalkerMx
' Based on the documentation found here:
' http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddsfileformat.htm
' https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds

Imports System.IO
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class DDS_Decoder

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

    Public RedBitMask As Integer
    Public GreenBitMask As Integer
    Public BlueBitMask As Integer
    Public AlphaBitMask As Integer

    Public Caps1 As DDS_Caps1
    Public Caps2 As Integer

    Public ExtendedHeader As Boolean

    Public DXGIFormat As DXGI_Format
    Public ResourceDimension As DX10_ResourceDimension
    Public MiscFlag As DX10_MiscFlags
    Public ArraySize As Integer
    Public MiscFlags2 As DX10_AlphaMode

    Private SourceBytes As Byte()
    Private DecodedBytes As Byte()
    Private FilePath As String

    Public Sub New(Source As String)
        FilePath = Source
        ReadHeader(Source)
    End Sub

    Private Sub ReadHeader(Source As String)
        Using FS As New FileStream(Source, FileMode.Open, FileAccess.Read)
            Using Reader As New BinaryReader(FS)

                Signature = New String(Reader.ReadChars(4))             ' dwMagic
                HeaderSize = Reader.ReadInt32()                         ' dwSize
                SurfaceFlags = Reader.ReadInt32()                       ' dwFlags
                Height = Reader.ReadInt32()                             ' dwHeight
                Width = Reader.ReadInt32()                              ' dwWidth
                PitchLinearSize = Reader.ReadInt32()                    ' dwPitchOrLinearSize
                Depth = Reader.ReadInt32()                              ' dwDepth
                MipMapCount = Reader.ReadInt32()                        ' dwMipMapCount

                Reader.BaseStream.Seek(44, SeekOrigin.Current)          ' dwReserved1 x11

                SubHeaderSize = Reader.ReadInt32()                      ' DDPIXELFORMAT dwSize
                PixelFlags = Reader.ReadInt32()                         ' DDPIXELFORMAT dwFlags
                FourCC = New String(Reader.ReadChars(4))                ' DDPIXELFORMAT dwFourCC
                RGBBitCount = Reader.ReadInt32()                        ' DDPIXELFORMAT dwRGBBitCount
                RedBitMask = Reader.ReadInt32()                         ' DDPIXELFORMAT dwRBitMask
                GreenBitMask = Reader.ReadInt32()                       ' DDPIXELFORMAT dwGBitMask
                BlueBitMask = Reader.ReadInt32()                        ' DDPIXELFORMAT dwBBitMask
                AlphaBitMask = Reader.ReadInt32()                       ' DDPIXELFORMAT dwABitMask

                Caps1 = Reader.ReadInt32()                              ' dwCaps
                Caps2 = Reader.ReadInt32()                              ' dwCaps2

                Reader.BaseStream.Seek(12, SeekOrigin.Current)          ' dwCaps3, dwCaps4, dwReserved2

                If FourCC = "DX10" Then
                    ExtendedHeader = True
                    DXGIFormat = Reader.ReadInt32()                     ' dxgiFormat
                    ResourceDimension = Reader.ReadInt32()              ' resourceDimension
                    MiscFlag = Reader.ReadInt32()                       ' miscFlag
                    ArraySize = Reader.ReadInt32()                      ' arraySize
                    MiscFlags2 = Reader.ReadInt32()                     ' miscFlags2
                    Select Case DXGIFormat
                        Case DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM : SetMask(2, 1, 0, 3) : RGBBitCount = 32
                        Case DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM : SetMask(2, 1, 0) : RGBBitCount = 32
                    End Select
                End If

            End Using
        End Using

    End Sub

    Private Sub BeginDecode()

        Dim AlphaMode As Integer = 0
        Dim CompressionMode As Integer = 0
        Dim BytesToRead As Integer = 0
        Dim DataOffset As Integer = 128

        If ExtendedHeader Then
            DataOffset = 148
            Select Case DXGIFormat
                Case DXGI_Format.DXGI_FORMAT_BC1_UNORM
                    CompressionMode = 1
                    AlphaMode = If(MiscFlags2 = DX10_AlphaMode.DDS_ALPHA_MODE_OPAQUE, 0, 1)
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 8
                Case DXGI_Format.DXGI_FORMAT_BC3_UNORM
                    CompressionMode = 2
                    AlphaMode = 2
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                Case DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM
                    CompressionMode = 0
                    AlphaMode = 2
                    BytesToRead = Width * Height * 4
                Case DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM
                    CompressionMode = 0
                    AlphaMode = 0
                    BytesToRead = Width * Height * 4
            End Select
        Else
            If (PixelFlags And DDS_PixelFlags.DDPF_FOURCC) = DDS_PixelFlags.DDPF_FOURCC Then
                Select Case FourCC
                    Case "DXT1"
                        CompressionMode = 1
                        BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 8
                        If (PixelFlags And DDS_PixelFlags.DDPF_ALPHAPIXELS) = DDS_PixelFlags.DDPF_ALPHAPIXELS Then
                            AlphaMode = 1
                        Else
                            AlphaMode = 0
                        End If
                    Case "DXT5"
                        CompressionMode = 2
                        AlphaMode = 2
                        BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                End Select
            ElseIf (PixelFlags And DDS_PixelFlags.DDPF_RGB) = DDS_PixelFlags.DDPF_RGB Then
                CompressionMode = 0
                BytesToRead = (Width * Height * (RGBBitCount \ 8))
                If (PixelFlags And DDS_PixelFlags.DDPF_ALPHAPIXELS) = DDS_PixelFlags.DDPF_ALPHAPIXELS Then
                    AlphaMode = 2
                Else
                    AlphaMode = 0
                End If
            End If
        End If

        If BytesToRead > 0 Then
            SourceBytes = GetFileBytes(FilePath, DataOffset, BytesToRead)
        End If

        Select Case CompressionMode
            Case 0
                DecodeUncompressed(AlphaMode)
            Case 1
                DecodeBC1(AlphaMode)
            Case 2
                DecodeBC3()
        End Select

    End Sub

    Private Sub DecodeUncompressed(AlphaMode As Integer)
        Dim bytesPerPixel As Integer = RGBBitCount \ 8
        Dim RowPitch As Integer = 0

        If (SurfaceFlags And DDS_SurfaceFlags.DDSD_PITCH) = DDS_SurfaceFlags.DDSD_PITCH Then
            RowPitch = PitchLinearSize
        Else
            RowPitch = (Width * bytesPerPixel + 3) And Not 3
        End If

        Dim rShift As Integer = GetShiftCount(RedBitMask)
        Dim gShift As Integer = GetShiftCount(GreenBitMask)
        Dim bShift As Integer = GetShiftCount(BlueBitMask)
        Dim aShift As Integer = GetShiftCount(AlphaBitMask)

        Dim DecodeAlpha As Boolean = (AlphaMode > 0) AndAlso (AlphaBitMask <> 0)

        DecodedBytes = New Byte(Width * Height * 4 - 1) {}

        Dim Options As New ParallelOptions With {.MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount \ 2)}

        Parallel.For(0, Height, Options, Sub(y)
                                             Dim SourceRowStart As Integer = y * RowPitch
                                             Dim DestRowStart As Integer = y * Width * 4

                                             For x As Integer = 0 To Width - 1
                                                 Dim SourceIndex As Integer = SourceRowStart + (x * bytesPerPixel)
                                                 Dim DestIndex As Integer = DestRowStart + (x * 4)

                                                 If SourceIndex + bytesPerPixel > SourceBytes.Length Then Continue For

                                                 Dim pixelValue As UInteger = 0
                                                 If bytesPerPixel = 4 Then
                                                     pixelValue = BitConverter.ToUInt32(SourceBytes, SourceIndex)
                                                 ElseIf bytesPerPixel = 3 Then
                                                     pixelValue = CUInt(SourceBytes(SourceIndex)) Or (CUInt(SourceBytes(SourceIndex + 1)) << 8) Or (CUInt(SourceBytes(SourceIndex + 2)) << 16)
                                                 ElseIf bytesPerPixel = 2 Then
                                                     pixelValue = BitConverter.ToUInt16(SourceBytes, SourceIndex)
                                                 End If

                                                 DecodedBytes(DestIndex) = CByte((pixelValue And CUInt(BlueBitMask)) >> bShift)
                                                 DecodedBytes(DestIndex + 1) = CByte((pixelValue And CUInt(GreenBitMask)) >> gShift)
                                                 DecodedBytes(DestIndex + 2) = CByte((pixelValue And CUInt(RedBitMask)) >> rShift)

                                                 If DecodeAlpha Then
                                                     DecodedBytes(DestIndex + 3) = CByte((pixelValue And CUInt(AlphaBitMask)) >> aShift)
                                                 Else
                                                     DecodedBytes(DestIndex + 3) = 255
                                                 End If
                                             Next
                                         End Sub)
    End Sub

    Private Sub DecodeBC1(AlphaMode As Integer)
        Dim blocksWide As Integer = (Width + 3) \ 4
        Dim blocksHigh As Integer = (Height + 3) \ 4
        DecodedBytes = New Byte(Width * Height * 4 - 1) {}

        Dim Options As New ParallelOptions With {.MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount \ 2)}

        Parallel.For(0, blocksHigh, Options, Sub(yBlock)
                                                 For xBlock As Integer = 0 To blocksWide - 1
                                                     Dim sourceIndex As Integer = (yBlock * blocksWide + xBlock) * 8

                                                     Dim c0_raw As UShort = BitConverter.ToUInt16(SourceBytes, sourceIndex)
                                                     Dim c1_raw As UShort = BitConverter.ToUInt16(SourceBytes, sourceIndex + 2)

                                                     Dim lookupTable As UInteger = BitConverter.ToUInt32(SourceBytes, sourceIndex + 4)

                                                     Dim pal(3, 3) As Byte
                                                     Dim p0() As Byte = Unpack565(c0_raw)
                                                     Dim p1() As Byte = Unpack565(c1_raw)

                                                     pal(0, 0) = p0(0) : pal(0, 1) = p0(1) : pal(0, 2) = p0(2) : pal(0, 3) = 255
                                                     pal(1, 0) = p1(0) : pal(1, 1) = p1(1) : pal(1, 2) = p1(2) : pal(1, 3) = 255

                                                     If c0_raw > c1_raw Then
                                                         pal(2, 0) = CByte((CInt(pal(0, 0)) * 2 + pal(1, 0)) \ 3)
                                                         pal(2, 1) = CByte((CInt(pal(0, 1)) * 2 + pal(1, 1)) \ 3)
                                                         pal(2, 2) = CByte((CInt(pal(0, 2)) * 2 + pal(1, 2)) \ 3)
                                                         pal(2, 3) = 255

                                                         pal(3, 0) = CByte((CInt(pal(0, 0)) + pal(1, 0) * 2) \ 3)
                                                         pal(3, 1) = CByte((CInt(pal(0, 1)) + pal(1, 1) * 2) \ 3)
                                                         pal(3, 2) = CByte((CInt(pal(0, 2)) + pal(1, 2) * 2) \ 3)
                                                         pal(3, 3) = 255
                                                     Else
                                                         pal(2, 0) = CByte((CInt(pal(0, 0)) + pal(1, 0)) \ 2)
                                                         pal(2, 1) = CByte((CInt(pal(0, 1)) + pal(1, 1)) \ 2)
                                                         pal(2, 2) = CByte((CInt(pal(0, 2)) + pal(1, 2)) \ 2)
                                                         pal(2, 3) = 255

                                                         pal(3, 0) = 0 : pal(3, 1) = 0 : pal(3, 2) = 0
                                                         pal(3, 3) = If(AlphaMode = 1, 0, 255)
                                                     End If

                                                     For i As Integer = 0 To 15
                                                         Dim py As Integer = (yBlock * 4) + (i \ 4)
                                                         Dim px As Integer = (xBlock * 4) + (i Mod 4)

                                                         If px < Width AndAlso py < Height Then
                                                             Dim colorIdx As Integer = CInt((lookupTable >> (i * 2)) And 3UI)
                                                             Dim destIdx As Integer = (py * Width + px) * 4

                                                             DecodedBytes(destIdx) = pal(colorIdx, 0)
                                                             DecodedBytes(destIdx + 1) = pal(colorIdx, 1)
                                                             DecodedBytes(destIdx + 2) = pal(colorIdx, 2)
                                                             DecodedBytes(destIdx + 3) = pal(colorIdx, 3)
                                                         End If
                                                     Next
                                                 Next
                                             End Sub)
    End Sub

    Private Sub DecodeBC3()
        Dim blocksWide As Integer = (Width + 3) \ 4
        Dim blocksHigh As Integer = (Height + 3) \ 4
        DecodedBytes = New Byte(Width * Height * 4 - 1) {}

        Dim Options As New ParallelOptions With {.MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount \ 2)}

        Parallel.For(0, blocksHigh, Options, Sub(yBlock)
                                                 For xBlock As Integer = 0 To blocksWide - 1
                                                     Dim sourceIndex As Integer = (yBlock * blocksWide + xBlock) * 16

                                                     Dim a0 As Byte = SourceBytes(sourceIndex)
                                                     Dim a1 As Byte = SourceBytes(sourceIndex + 1)
                                                     Dim aPal(7) As Byte
                                                     aPal(0) = a0
                                                     aPal(1) = a1

                                                     If a0 > a1 Then
                                                         For i As Integer = 1 To 6
                                                             aPal(i + 1) = CByte(((7 - i) * CInt(a0) + i * CInt(a1)) \ 7)
                                                         Next
                                                     Else
                                                         For i As Integer = 1 To 4
                                                             aPal(i + 1) = CByte(((5 - i) * CInt(a0) + i * CInt(a1)) \ 5)
                                                         Next
                                                         aPal(6) = 0
                                                         aPal(7) = 255
                                                     End If

                                                     Dim aData As ULong = 0
                                                     For i As Integer = 0 To 5
                                                         aData = aData Or (CType(SourceBytes(sourceIndex + 2 + i), ULong) << (i * 8))
                                                     Next

                                                     Dim colorOffset As Integer = sourceIndex + 8
                                                     Dim c0_raw As UShort = BitConverter.ToUInt16(SourceBytes, colorOffset)
                                                     Dim c1_raw As UShort = BitConverter.ToUInt16(SourceBytes, colorOffset + 2)
                                                     Dim lookupTable As UInteger = BitConverter.ToUInt32(SourceBytes, colorOffset + 4)

                                                     Dim p0() As Byte = Unpack565(c0_raw)
                                                     Dim p1() As Byte = Unpack565(c1_raw)
                                                     Dim cPal(3, 2) As Byte

                                                     For j = 0 To 2 : cPal(0, j) = p0(j) : cPal(1, j) = p1(j) : Next

                                                     cPal(2, 0) = CByte((CInt(cPal(0, 0)) * 2 + cPal(1, 0)) \ 3)
                                                     cPal(2, 1) = CByte((CInt(cPal(0, 1)) * 2 + cPal(1, 1)) \ 3)
                                                     cPal(2, 2) = CByte((CInt(cPal(0, 2)) * 2 + cPal(1, 2)) \ 3)
                                                     cPal(3, 0) = CByte((CInt(cPal(0, 0)) + cPal(1, 0) * 2) \ 3)
                                                     cPal(3, 1) = CByte((CInt(cPal(0, 1)) + cPal(1, 1) * 2) \ 3)
                                                     cPal(3, 2) = CByte((CInt(cPal(0, 2)) + cPal(1, 2) * 2) \ 3)

                                                     For i As Integer = 0 To 15
                                                         Dim py As Integer = (yBlock * 4) + (i \ 4)
                                                         Dim px As Integer = (xBlock * 4) + (i Mod 4)

                                                         If px < Width AndAlso py < Height Then
                                                             Dim cIdx As Integer = CInt((lookupTable >> (i * 2)) And 3UI)
                                                             Dim aIdx As Integer = CInt((aData >> (i * 3)) And 7UL)

                                                             Dim destIdx As Integer = (py * Width + px) * 4
                                                             DecodedBytes(destIdx) = cPal(cIdx, 0)
                                                             DecodedBytes(destIdx + 1) = cPal(cIdx, 1)
                                                             DecodedBytes(destIdx + 2) = cPal(cIdx, 2)
                                                             DecodedBytes(destIdx + 3) = aPal(aIdx)
                                                         End If
                                                     Next
                                                 Next
                                             End Sub)
    End Sub

    Public Sub SaveImage(Path As String, Format As ImageFormat)
        BeginDecode()
        Using TempImage As New Bitmap(Width, Height, PixelFormat.Format32bppArgb)
            Dim Rect As New Rectangle(0, 0, Width, Height)
            Dim TempData As BitmapData = TempImage.LockBits(Rect, ImageLockMode.WriteOnly, TempImage.PixelFormat)
            Marshal.Copy(DecodedBytes, 0, TempData.Scan0, DecodedBytes.Length)
            TempImage.UnlockBits(TempData)
            TempImage.Save(Path, Format)
        End Using
    End Sub

    Private Sub SetMask(RIdx As Integer, GIdx As Integer, BIdx As Integer, Optional AIdx As Integer = -1)
        RedBitMask = If(RIdx >= 0, &HFF << (RIdx * 8), 0)
        GreenBitMask = If(GIdx >= 0, &HFF << (GIdx * 8), 0)
        BlueBitMask = If(BIdx >= 0, &HFF << (BIdx * 8), 0)
        AlphaBitMask = If(AIdx >= 0, &HFF << (AIdx * 8), 0)
    End Sub

    Private Function Unpack565(Value As UShort) As Byte()
        Dim r As Integer = (Value And &HF800) >> 11
        Dim g As Integer = (Value And &H7E0) >> 5
        Dim b As Integer = (Value And &H1F)
        Dim resR As Byte = CByte((r << 3) Or (r >> 2))
        Dim resG As Byte = CByte((g << 2) Or (g >> 4))
        Dim resB As Byte = CByte((b << 3) Or (b >> 2))
        Return {resB, resG, resR}
    End Function

    Private Function GetShiftCount(Mask As Integer) As Integer
        If Mask = 0 Then Return 0
        Dim Shift As Integer = 0
        Dim TempMask As UInteger = CUInt(Mask)
        While (TempMask And 1UI) = 0
            TempMask >>= 1
            Shift += 1
            If Shift > 32 Then Return 0
        End While
        Return Shift
    End Function

    Private Function GetFileBytes(Source As String, Offset As Long, Count As Integer) As Byte()
        Dim Buffer() As Byte = New Byte(Count - 1) {}
        Using FileReader As New FileStream(Source, FileMode.Open, FileAccess.Read, FileShare.Read)
            FileReader.Seek(Offset, SeekOrigin.Begin)
            Dim ByteCount As Integer = FileReader.Read(Buffer, 0, Buffer.Length)
            If ByteCount < Count Then
                ReDim Preserve Buffer(ByteCount - 1)
            End If
        End Using
        Return Buffer
    End Function

    Protected Overridable Sub Dispose(Disposing As Boolean)
        If Not Disposed Then
            SourceBytes = Nothing
            DecodedBytes = Nothing
        End If
        Disposed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
