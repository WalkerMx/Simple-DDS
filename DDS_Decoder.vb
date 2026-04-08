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
    Public Caps2 As DDS_Caps2

    Public DXGIFormat As DXGI_Format
    Public ResourceDimension As DX10_ResourceDimension
    Public MiscFlag As DX10_MiscFlags
    Public ArraySize As Integer
    Public MiscFlags2 As DX10_MiscFlags2

    Public ExtendedHeader As Boolean

    Private SourceBytes As Byte()
    Private DecodedBytes As Byte()
    Private FilePath As String

    Public Sub New(Source As String)
        FilePath = Source
        ReadHeader(Source)
        ' BeginDecode()
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
                    AlphaMode = If(MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE, 0, 1)
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
                Case Else
                    Throw New Exception("Unsupported DXGI Format!")
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
                    Case Else
                        Throw New Exception("Unsupported DXT Format!")
                End Select
            ElseIf (PixelFlags And DDS_PixelFlags.DDPF_RGB) = DDS_PixelFlags.DDPF_RGB Then
                CompressionMode = 0
                BytesToRead = (Width * Height * (RGBBitCount \ 8))
                If (PixelFlags And DDS_PixelFlags.DDPF_ALPHAPIXELS) = DDS_PixelFlags.DDPF_ALPHAPIXELS Then
                    AlphaMode = 2
                Else
                    AlphaMode = 0
                End If
            Else
                Throw New Exception("Unknown Format Error!")
            End If
        End If

        If BytesToRead > 0 Then
            SourceBytes = GetFileBytes(FilePath, DataOffset, BytesToRead)
        End If

        If CompressionMode = 0 Then
            DecodeUncompressed(AlphaMode)
        Else
            DecodeCompressed(AlphaMode)
        End If

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

    Private Sub DecodeCompressed(AlphaMode As Integer)
        Dim BlockWidth As Integer = (Width + 3) \ 4
        Dim BlockHeight As Integer = (Height + 3) \ 4
        Dim BytesPerBlock As Integer = If(AlphaMode = 2, 16, 8)

        DecodedBytes = New Byte(Width * Height * 4 - 1) {}

        Parallel.For(0, BlockHeight, Options, Sub(yBlock)
                                                  Dim ColorPalette(3, 3) As Byte
                                                  Dim AlphaPalette(7) As Byte

                                                  For xBlock As Integer = 0 To BlockWidth - 1
                                                      Dim SourceIndex As Integer = (yBlock * BlockWidth + xBlock) * BytesPerBlock
                                                      Dim ColorOffset As Integer = If(AlphaMode = 2, SourceIndex + 8, SourceIndex)

                                                      Dim ColorTable As UInteger = DecodeColorBlock(ColorOffset, ColorPalette, AlphaMode)
                                                      Dim AlphaTable As ULong = 0

                                                      If AlphaMode = 2 Then
                                                          AlphaTable = DecodeAlphaBlock(SourceIndex, AlphaPalette)
                                                      End If

                                                      For i As Integer = 0 To 15
                                                          Dim py As Integer = (yBlock * 4) + (i >> 2)
                                                          Dim px As Integer = (xBlock * 4) + (i And 3)

                                                          If px < Width AndAlso py < Height Then
                                                              Dim destIdx As Integer = (py * Width + px) * 4
                                                              Dim cIdx As Integer = CInt((ColorTable >> (i * 2)) And 3UI)

                                                              DecodedBytes(destIdx) = ColorPalette(cIdx, 0)
                                                              DecodedBytes(destIdx + 1) = ColorPalette(cIdx, 1)
                                                              DecodedBytes(destIdx + 2) = ColorPalette(cIdx, 2)

                                                              If AlphaMode = 2 Then
                                                                  Dim AlphaIndex As Integer = CInt((AlphaTable >> (i * 3)) And 7UL)
                                                                  DecodedBytes(destIdx + 3) = AlphaPalette(AlphaIndex)
                                                              Else
                                                                  DecodedBytes(destIdx + 3) = ColorPalette(cIdx, 3)
                                                              End If
                                                          End If
                                                      Next
                                                  Next
                                              End Sub)
    End Sub

    Private Function DecodeColorBlock(Offset As Integer, cPal(,) As Byte, AlphaMode As Integer) As UInteger
        Dim c0_raw As UShort = BitConverter.ToUInt16(SourceBytes, Offset)
        Dim c1_raw As UShort = BitConverter.ToUInt16(SourceBytes, Offset + 2)
        Dim ColorTable As UInteger = BitConverter.ToUInt32(SourceBytes, Offset + 4)

        Dim p0() As Byte = Unpack565(c0_raw)
        Dim p1() As Byte = Unpack565(c1_raw)

        cPal(0, 0) = p0(0) : cPal(0, 1) = p0(1) : cPal(0, 2) = p0(2) : cPal(0, 3) = 255
        cPal(1, 0) = p1(0) : cPal(1, 1) = p1(1) : cPal(1, 2) = p1(2) : cPal(1, 3) = 255

        If AlphaMode = 2 OrElse c0_raw > c1_raw Then
            cPal(2, 0) = CByte((CInt(cPal(0, 0)) * 2 + cPal(1, 0)) \ 3)
            cPal(2, 1) = CByte((CInt(cPal(0, 1)) * 2 + cPal(1, 1)) \ 3)
            cPal(2, 2) = CByte((CInt(cPal(0, 2)) * 2 + cPal(1, 2)) \ 3)
            cPal(2, 3) = 255

            cPal(3, 0) = CByte((CInt(cPal(0, 0)) + cPal(1, 0) * 2) \ 3)
            cPal(3, 1) = CByte((CInt(cPal(0, 1)) + cPal(1, 1) * 2) \ 3)
            cPal(3, 2) = CByte((CInt(cPal(0, 2)) + cPal(1, 2) * 2) \ 3)
            cPal(3, 3) = 255
        Else
            cPal(2, 0) = CByte((CInt(cPal(0, 0)) + cPal(1, 0)) \ 2)
            cPal(2, 1) = CByte((CInt(cPal(0, 1)) + cPal(1, 1)) \ 2)
            cPal(2, 2) = CByte((CInt(cPal(0, 2)) + cPal(1, 2)) \ 2)
            cPal(2, 3) = 255

            cPal(3, 0) = 0 : cPal(3, 1) = 0 : cPal(3, 2) = 0
            cPal(3, 3) = CByte(If(AlphaMode = 1, 0, 255))
        End If

        Return ColorTable
    End Function

    Private Function DecodeAlphaBlock(Offset As Integer, aPal() As Byte) As ULong
        Dim a0 As Byte = SourceBytes(Offset)
        Dim a1 As Byte = SourceBytes(Offset + 1)
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
        Dim AlphaTable As ULong = 0
        For i As Integer = 0 To 5
            AlphaTable = AlphaTable Or (CType(SourceBytes(Offset + 2 + i), ULong) << (i * 8))
        Next
        Return AlphaTable
    End Function

    Public Sub SaveImage(Path As String, Format As ImageFormat)
        Try
            BeginDecode()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
            Exit Sub
        End Try
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
