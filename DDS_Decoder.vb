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

    Private FilePath As String
    Private SourceBytes As Byte()
    Private DecodedBytes As Byte()

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
                Case &H46, &H47, &H48 ' BC1 Typeless, UNORM, SRGB
                    CompressionMode = If(MiscFlags2 = DX10_MiscFlags2.DDS_ALPHA_MODE_OPAQUE, 0, 1)
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 8
                Case &H49, &H4A, &H4B ' BC2 Typeless, UNORM, SRGB
                    CompressionMode = 2
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                Case &H4C, &H4D, &H4E ' BC3 Typeless, UNORM, SRGB
                    CompressionMode = 3
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                Case &H4F, &H50 ' BC4 Typeless, UNORM
                    CompressionMode = 4
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 8
                Case &H52, &H53 ' BC5 Typeless, UNORM
                    CompressionMode = 5
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                Case &H61, &H62, &H63 ' BC7 Typeless, UNORM, SRGB
                    CompressionMode = 7
                    BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                Case &H57, &H5A, &H5B ' B8G8R8A8 Typeless, UNORM, SRGB
                    CompressionMode = -1
                    AlphaMode = 2
                    BytesToRead = Width * Height * 4
                Case &H58, &H5C, &H5D ' B8G8R8X8 Typeless, UNORM, SRGB
                    CompressionMode = -1
                    AlphaMode = 0
                    BytesToRead = Width * Height * 4
                Case Else
                    Throw New Exception("Unsupported DXGI Format: " & DXGIFormat.ToString.Replace("DXGI_FORMAT_", ""))
            End Select
        Else
            If (PixelFlags And DDS_PixelFlags.DDPF_FOURCC) = DDS_PixelFlags.DDPF_FOURCC Then
                Select Case FourCC
                    Case "DXT1"
                        If (PixelFlags And DDS_PixelFlags.DDPF_ALPHAPIXELS) = DDS_PixelFlags.DDPF_ALPHAPIXELS Then
                            CompressionMode = 1
                        Else
                            CompressionMode = 0
                        End If
                        BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 8
                    Case "DXT3"
                        CompressionMode = 2
                        BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                    Case "DXT5"
                        CompressionMode = 3
                        BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                    Case "ATI1", "BC4U"
                        CompressionMode = 4
                        BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 8
                    Case "ATI2", "BC5U"
                        CompressionMode = 5
                        BytesToRead = Math.Max(1, (Width + 3) \ 4) * Math.Max(1, (Height + 3) \ 4) * 16
                    Case Else
                        Throw New Exception("Unsupported DXT Format: " & FourCC)
                End Select
            ElseIf (PixelFlags And DDS_PixelFlags.DDPF_RGB) = DDS_PixelFlags.DDPF_RGB Then
                CompressionMode = -1
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
        If CompressionMode = -1 Then
            DecodeUncompressed(AlphaMode)
        Else
            DecodeCompressed(CompressionMode)
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

    Private Sub DecodeCompressed(CompressionMode As Integer)
        Dim BlockWidth As Integer = (Width + 3) \ 4
        Dim BlockHeight As Integer = (Height + 3) \ 4
        Dim BytesPerBlock As Integer = If(CompressionMode = 0 OrElse CompressionMode = 1 OrElse CompressionMode = 4, 8, 16)
        DecodedBytes = New Byte(Width * Height * 4 - 1) {}

        Parallel.For(0, BlockHeight, Options, Sub(yBlock)
                                                  Dim yPixelBase As Integer = yBlock * 4

                                                  For xBlock As Integer = 0 To BlockWidth - 1
                                                      Dim xPixelBase As Integer = xBlock * 4
                                                      Dim SourceIndex As Integer = (yBlock * BlockWidth + xBlock) * BytesPerBlock

                                                      Select Case CompressionMode
                                                          Case 0, 1 ' BC1 / BC1a
                                                              DecodeBlockBC1(SourceIndex, xPixelBase, yPixelBase, CompressionMode)
                                                          Case 2 ' BC2
                                                              DecodeBlockBC1(SourceIndex + 8, xPixelBase, yPixelBase, CompressionMode)
                                                              DecodeBlockBC2(SourceIndex, xPixelBase, yPixelBase)
                                                          Case 3 ' BC3
                                                              DecodeBlockBC1(SourceIndex + 8, xPixelBase, yPixelBase, CompressionMode)
                                                              DecodeBlockBC3(SourceIndex, xPixelBase, yPixelBase, 3)
                                                          Case 4 ' BC4
                                                              DecodeBlockBC3(SourceIndex, xPixelBase, yPixelBase, 2)
                                                              DecodeBlockBC4(xPixelBase, yPixelBase)
                                                          Case 5 ' BC5
                                                              DecodeBlockBC3(SourceIndex, xPixelBase, yPixelBase, 2)
                                                              DecodeBlockBC3(SourceIndex + 8, xPixelBase, yPixelBase, 1)
                                                              DecodeBlockBC5(xPixelBase, yPixelBase)
                                                          Case 7 ' BC7
                                                              DecodeBlockBC7(SourceIndex, xPixelBase, yPixelBase)
                                                      End Select
                                                  Next
                                              End Sub)
    End Sub


    Private Sub DecodeBlockBC7(Offset As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Mode As Integer = ModeLUT(SourceBytes(Offset))
        Dim BitPos As Integer = Mode + 1
        Select Case Mode
            Case 0 : DecodeMode0(Offset, BitPos, xPixelBase, yPixelBase)
            Case 1 : DecodeMode1(Offset, BitPos, xPixelBase, yPixelBase)
            Case 2 : DecodeMode2(Offset, BitPos, xPixelBase, yPixelBase)
            Case 3 : DecodeMode3(Offset, BitPos, xPixelBase, yPixelBase)
            Case 4 : DecodeMode4(Offset, BitPos, xPixelBase, yPixelBase)
            Case 5 : DecodeMode5(Offset, BitPos, xPixelBase, yPixelBase)
            Case 6 : DecodeMode6(Offset, BitPos, xPixelBase, yPixelBase)
            Case 7 : DecodeMode7(Offset, BitPos, xPixelBase, yPixelBase)
            Case Else
                DecodeModeDiagnostic(Offset, BitPos, xPixelBase, yPixelBase)
        End Select
    End Sub

    Private Sub DecodeBlockBC5(xPixelBase As Integer, yPixelBase As Integer)
        For i As Integer = 0 To 15
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                Dim RVal As Integer = DecodedBytes(destIdx + 2)
                Dim GVal As Integer = DecodedBytes(destIdx + 1)
                Dim dX As Integer = RVal - 128
                Dim dY As Integer = GVal - 128
                Dim dSq As Integer = (dX * dX) + (dY * dY)
                Dim BVal As Integer = 255 - (dSq >> 7)
                If BVal < 128 Then BVal = 128
                If BVal > 255 Then BVal = 255
                DecodedBytes(destIdx + 0) = CByte(BVal)
                DecodedBytes(destIdx + 3) = 255
            End If
        Next
    End Sub

    Private Sub DecodeBlockBC4(xPixelBase As Integer, yPixelBase As Integer)
        For i As Integer = 0 To 15
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                Dim RVal As Byte = DecodedBytes(destIdx + 2)
                DecodedBytes(destIdx) = RVal
                DecodedBytes(destIdx + 1) = RVal
                DecodedBytes(destIdx + 3) = 255
            End If
        Next
    End Sub

    Private Sub DecodeBlockBC3(Offset As Integer, xPixelBase As Integer, yPixelBase As Integer, TargetChannel As Integer)
        Dim aPal(7) As Byte
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
        For i As Integer = 0 To 15
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = ((pY * Width + pX) * 4)
                Dim aIdx As Integer = CInt((AlphaTable >> (i * 3)) And 7UL)
                DecodedBytes(destIdx + TargetChannel) = aPal(aIdx)
                If TargetChannel <> 3 Then
                    DecodedBytes(destIdx + 3) = 255
                End If
            End If
        Next
    End Sub

    Private Sub DecodeBlockBC2(Offset As Integer, xPixelBase As Integer, yPixelBase As Integer)
        For byteIdx As Integer = 0 To 7
            Dim packedByte As Byte = SourceBytes(Offset + byteIdx)
            For nibble As Integer = 0 To 1
                Dim nibIdx As Integer = (byteIdx * 2) + nibble
                Dim pX As Integer = xPixelBase + (nibIdx And 3)
                Dim pY As Integer = yPixelBase + (nibIdx >> 2)
                If pX < Width AndAlso pY < Height Then
                    Dim destIdx As Integer = (pY * Width + pX) * 4
                    Dim alpha4Bit As Byte
                    If nibble = 0 Then
                        alpha4Bit = CByte(packedByte And &HF)
                    Else
                        alpha4Bit = CByte((packedByte >> 4) And &HF)
                    End If
                    DecodedBytes(destIdx + 3) = CByte((alpha4Bit << 4) Or alpha4Bit)
                End If
            Next
        Next
    End Sub

    Private Sub DecodeBlockBC1(Offset As Integer, xPixelBase As Integer, yPixelBase As Integer, ActiveMode As Integer)
        Dim c0_raw As UShort = BitConverter.ToUInt16(SourceBytes, Offset)
        Dim c1_raw As UShort = BitConverter.ToUInt16(SourceBytes, Offset + 2)
        Dim ColorTable As UInteger = BitConverter.ToUInt32(SourceBytes, Offset + 4)
        Dim cPal(3, 3) As Byte
        Dim p0() As Byte = Unpack565(c0_raw)
        Dim p1() As Byte = Unpack565(c1_raw)
        cPal(0, 0) = p0(0) : cPal(0, 1) = p0(1) : cPal(0, 2) = p0(2) : cPal(0, 3) = 255
        cPal(1, 0) = p1(0) : cPal(1, 1) = p1(1) : cPal(1, 2) = p1(2) : cPal(1, 3) = 255
        If ActiveMode >= 2 OrElse c0_raw > c1_raw Then
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
            cPal(3, 3) = CByte(If(ActiveMode = 1, 0, 255))
        End If
        For i As Integer = 0 To 15
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim cIdx As Integer = CInt((ColorTable >> (i * 2)) And 3UI)
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = cPal(cIdx, 0)
                DecodedBytes(destIdx + 1) = cPal(cIdx, 1)
                DecodedBytes(destIdx + 2) = cPal(cIdx, 2)
                DecodedBytes(destIdx + 3) = cPal(cIdx, 3)
            End If
        Next
    End Sub

#Region "BC7 Modes"

    Private Sub DecodeMode0(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Partition As Integer = ReadBits(Offset, BitPos, 4)
        Dim R(5), G(5), B(5), P(5) As Integer
        For i As Integer = 0 To 5
            R(i) = ReadBits(Offset, BitPos, 4)
        Next
        For i As Integer = 0 To 5
            G(i) = ReadBits(Offset, BitPos, 4)
        Next
        For i As Integer = 0 To 5
            B(i) = ReadBits(Offset, BitPos, 4)
        Next
        For i As Integer = 0 To 5
            P(i) = ReadBits(Offset, BitPos, 1)
        Next
        For i As Integer = 0 To 5
            R(i) = (R(i) << 1) Or P(i) : R(i) = (R(i) << 3) Or (R(i) >> 2)
            G(i) = (G(i) << 1) Or P(i) : G(i) = (G(i) << 3) Or (G(i) >> 2)
            B(i) = (B(i) << 1) Or P(i) : B(i) = (B(i) << 3) Or (B(i) >> 2)
        Next
        Dim pTableVal As UInteger = PartitionTable3(Partition)
        Dim subsetMap(15) As Integer
        Dim Anchors() As Integer = {0, AnchorIndexTable3_1(Partition), AnchorIndexTable3_2(Partition)}
        For i As Integer = 0 To 15
            Dim subset As Integer = CInt((pTableVal >> (i * 2)) And 3UI)
            subsetMap(i) = subset
        Next
        For i As Integer = 0 To 15
            Dim s As Integer = subsetMap(i)
            Dim numBits As Integer = 3
            If i = Anchors(0) OrElse i = Anchors(1) OrElse i = Anchors(2) Then
                numBits -= 1
            End If
            Dim index As Integer = ReadBits(Offset, BitPos, numBits)
            Dim w As Integer = Weight3(index)
            Dim iw As Integer = 64 - w
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(((B(s * 2) * iw + B(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 1) = CByte(((G(s * 2) * iw + G(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 2) = CByte(((R(s * 2) * iw + R(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 3) = 255
            End If
        Next
    End Sub

    Private Sub DecodeMode1(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Partition As Integer = ReadBits(Offset, BitPos, 6)
        Dim R(3), G(3), B(3), P(1) As Integer
        For i As Integer = 0 To 3
            R(i) = ReadBits(Offset, BitPos, 6)
        Next
        For i As Integer = 0 To 3
            G(i) = ReadBits(Offset, BitPos, 6)
        Next
        For i As Integer = 0 To 3
            B(i) = ReadBits(Offset, BitPos, 6)
        Next
        P(0) = ReadBits(Offset, BitPos, 1) : P(1) = ReadBits(Offset, BitPos, 1)
        For i As Integer = 0 To 3
            Dim pBit As Integer = P(i \ 2)
            R(i) = (R(i) << 1) Or pBit : R(i) = (R(i) << 1) Or (R(i) >> 6)
            G(i) = (G(i) << 1) Or pBit : G(i) = (G(i) << 1) Or (G(i) >> 6)
            B(i) = (B(i) << 1) Or pBit : B(i) = (B(i) << 1) Or (B(i) >> 6)
        Next
        Dim pTableVal As Integer = PartitionTable2(Partition)
        Dim subsetMap(15) As Integer
        Dim Anchors() As Integer = {0, AnchorIndexTable2(Partition)}
        For i As Integer = 0 To 15
            subsetMap(i) = (pTableVal >> i) And 1
        Next
        For i As Integer = 0 To 15
            Dim s As Integer = subsetMap(i)
            Dim numBits As Integer = 3
            If i = Anchors(0) OrElse i = Anchors(1) Then
                numBits -= 1
            End If
            Dim index As Integer = ReadBits(Offset, BitPos, numBits)
            Dim w As Integer = Weight3(index)
            Dim iw As Integer = 64 - w
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(((B(s * 2) * iw + B(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 1) = CByte(((G(s * 2) * iw + G(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 2) = CByte(((R(s * 2) * iw + R(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 3) = 255
            End If
        Next
    End Sub

    Private Sub DecodeMode2(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Partition As Integer = ReadBits(Offset, BitPos, 6)
        Dim R(5), G(5), B(5) As Integer
        For i As Integer = 0 To 5
            R(i) = ReadBits(Offset, BitPos, 5)
        Next
        For i As Integer = 0 To 5
            G(i) = ReadBits(Offset, BitPos, 5)
        Next
        For i As Integer = 0 To 5
            B(i) = ReadBits(Offset, BitPos, 5)
        Next
        For i As Integer = 0 To 5
            R(i) = (R(i) << 3) Or (R(i) >> 2)
            G(i) = (G(i) << 3) Or (G(i) >> 2)
            B(i) = (B(i) << 3) Or (B(i) >> 2)
        Next
        Dim pTableVal As UInteger = PartitionTable3(Partition)
        Dim subsetMap(15) As Integer
        Dim Anchors() As Integer = {0, AnchorIndexTable3_1(Partition), AnchorIndexTable3_2(Partition)}
        For i As Integer = 0 To 15
            Dim subset As Integer = CInt((pTableVal >> (i * 2)) And 3UI)
            subsetMap(i) = subset
        Next
        For i As Integer = 0 To 15
            Dim s As Integer = subsetMap(i)
            Dim numBits As Integer = 2
            If i = Anchors(0) OrElse i = Anchors(1) OrElse i = Anchors(2) Then
                numBits -= 1
            End If
            Dim index As Integer = ReadBits(Offset, BitPos, numBits)
            Dim w As Integer = Weight2(index)
            Dim iw As Integer = 64 - w
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(((B(s * 2) * iw + B(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 1) = CByte(((G(s * 2) * iw + G(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 2) = CByte(((R(s * 2) * iw + R(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 3) = 255
            End If
        Next
    End Sub

    Private Sub DecodeMode3(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Partition As Integer = ReadBits(Offset, BitPos, 6)
        Dim R(3), G(3), B(3), P(3) As Integer
        For i As Integer = 0 To 3
            R(i) = ReadBits(Offset, BitPos, 7)
        Next
        For i As Integer = 0 To 3
            G(i) = ReadBits(Offset, BitPos, 7)
        Next
        For i As Integer = 0 To 3
            B(i) = ReadBits(Offset, BitPos, 7)
        Next
        For i As Integer = 0 To 3
            P(i) = ReadBits(Offset, BitPos, 1)
        Next
        For i As Integer = 0 To 3
            R(i) = (R(i) << 1) Or P(i)
            G(i) = (G(i) << 1) Or P(i)
            B(i) = (B(i) << 1) Or P(i)
        Next
        Dim pTableVal As Integer = PartitionTable2(Partition)
        Dim subsetMap(15) As Integer
        Dim Anchors() As Integer = {0, AnchorIndexTable2(Partition)}
        For i As Integer = 0 To 15
            subsetMap(i) = (pTableVal >> i) And 1
        Next
        For i As Integer = 0 To 15
            Dim s As Integer = subsetMap(i)
            Dim numBits As Integer = 2
            If i = Anchors(0) OrElse i = Anchors(1) Then
                numBits -= 1
            End If
            Dim index As Integer = ReadBits(Offset, BitPos, numBits)
            Dim w As Integer = Weight2(index)
            Dim iw As Integer = 64 - w
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(((B(s * 2) * iw + B(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 1) = CByte(((G(s * 2) * iw + G(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 2) = CByte(((R(s * 2) * iw + R(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 3) = 255
            End If
        Next
    End Sub

    Private Sub DecodeMode4(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Rotation As Integer = ReadBits(Offset, BitPos, 2)
        Dim IndexMode As Integer = ReadBits(Offset, BitPos, 1)
        Dim r0 As Integer = ReadBits(Offset, BitPos, 5) : Dim r1 As Integer = ReadBits(Offset, BitPos, 5)
        Dim g0 As Integer = ReadBits(Offset, BitPos, 5) : Dim g1 As Integer = ReadBits(Offset, BitPos, 5)
        Dim b0 As Integer = ReadBits(Offset, BitPos, 5) : Dim b1 As Integer = ReadBits(Offset, BitPos, 5)
        Dim a0 As Integer = ReadBits(Offset, BitPos, 6) : Dim a1 As Integer = ReadBits(Offset, BitPos, 6)
        r0 = (r0 << 3) Or (r0 >> 2) : r1 = (r1 << 3) Or (r1 >> 2)
        g0 = (g0 << 3) Or (g0 >> 2) : g1 = (g1 << 3) Or (g1 >> 2)
        b0 = (b0 << 3) Or (b0 >> 2) : b1 = (b1 << 3) Or (b1 >> 2)
        a0 = (a0 << 2) Or (a0 >> 4) : a1 = (a1 << 2) Or (a1 >> 4)
        Dim ColorBits As Integer = If(IndexMode = 0, 2, 3)
        Dim AlphaBits As Integer = If(IndexMode = 0, 3, 2)
        Dim Indices2Bit(15) As Integer
        For i As Integer = 0 To 15
            Indices2Bit(i) = ReadBits(Offset, BitPos, If(i = 0, 1, 2))
        Next
        Dim Indices3Bit(15) As Integer
        For i As Integer = 0 To 15
            Indices3Bit(i) = ReadBits(Offset, BitPos, If(i = 0, 2, 3))
        Next
        Dim ColorIndices(15) As Integer
        Dim AlphaIndices(15) As Integer
        If IndexMode = 0 Then
            ColorIndices = Indices2Bit
            AlphaIndices = Indices3Bit
        Else
            ColorIndices = Indices3Bit
            AlphaIndices = Indices2Bit
        End If
        For i As Integer = 0 To 15
            Dim cw As Integer = If(IndexMode = 0, Weight2(ColorIndices(i)), Weight3(ColorIndices(i)))
            Dim icw As Integer = 64 - cw
            Dim aw As Integer = If(IndexMode = 0, Weight3(AlphaIndices(i)), Weight2(AlphaIndices(i)))
            Dim iaw As Integer = 64 - aw
            Dim r As Integer = ((r0 * icw + r1 * cw + 32) >> 6) And 255
            Dim g As Integer = ((g0 * icw + g1 * cw + 32) >> 6) And 255
            Dim b As Integer = ((b0 * icw + b1 * cw + 32) >> 6) And 255
            Dim a As Integer = ((a0 * iaw + a1 * aw + 32) >> 6) And 255
            Select Case Rotation
                Case 1 : Dim t As Integer = r : r = a : a = t
                Case 2 : Dim t As Integer = g : g = a : a = t
                Case 3 : Dim t As Integer = b : b = a : a = t
            End Select
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(b)
                DecodedBytes(destIdx + 1) = CByte(g)
                DecodedBytes(destIdx + 2) = CByte(r)
                DecodedBytes(destIdx + 3) = CByte(a)
            End If
        Next
    End Sub


    Private Sub DecodeMode5(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Rotation As Integer = ReadBits(Offset, BitPos, 2)
        Dim r0 As Integer = ReadBits(Offset, BitPos, 7) : Dim r1 As Integer = ReadBits(Offset, BitPos, 7)
        Dim g0 As Integer = ReadBits(Offset, BitPos, 7) : Dim g1 As Integer = ReadBits(Offset, BitPos, 7)
        Dim b0 As Integer = ReadBits(Offset, BitPos, 7) : Dim b1 As Integer = ReadBits(Offset, BitPos, 7)
        Dim a0 As Integer = ReadBits(Offset, BitPos, 8) : Dim a1 As Integer = ReadBits(Offset, BitPos, 8)
        r0 = (r0 << 1) Or (r0 >> 6) : r1 = (r1 << 1) Or (r1 >> 6)
        g0 = (g0 << 1) Or (g0 >> 6) : g1 = (g1 << 1) Or (g1 >> 6)
        b0 = (b0 << 1) Or (b0 >> 6) : b1 = (b1 << 1) Or (b1 >> 6)
        Dim ColorIndices(15) As Integer
        For i As Integer = 0 To 15
            ColorIndices(i) = ReadBits(Offset, BitPos, If(i = 0, 1, 2))
        Next
        Dim AlphaIndices(15) As Integer
        For i As Integer = 0 To 15
            AlphaIndices(i) = ReadBits(Offset, BitPos, If(i = 0, 1, 2))
        Next
        For i As Integer = 0 To 15
            Dim cw As Integer = Weight2(ColorIndices(i))
            Dim icw As Integer = 64 - cw
            Dim aw As Integer = Weight2(AlphaIndices(i))
            Dim iaw As Integer = 64 - aw
            Dim r As Integer = ((r0 * icw + r1 * cw + 32) >> 6) And 255
            Dim g As Integer = ((g0 * icw + g1 * cw + 32) >> 6) And 255
            Dim b As Integer = ((b0 * icw + b1 * cw + 32) >> 6) And 255
            Dim a As Integer = ((a0 * iaw + a1 * aw + 32) >> 6) And 255
            Select Case Rotation
                Case 1 : Dim t As Integer = r : r = a : a = t
                Case 2 : Dim t As Integer = g : g = a : a = t
                Case 3 : Dim t As Integer = b : b = a : a = t
            End Select
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(b)
                DecodedBytes(destIdx + 1) = CByte(g)
                DecodedBytes(destIdx + 2) = CByte(r)
                DecodedBytes(destIdx + 3) = CByte(a)
            End If
        Next
    End Sub

    Private Sub DecodeMode6(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim r0 As Integer = ReadBits(Offset, BitPos, 7) : Dim r1 As Integer = ReadBits(Offset, BitPos, 7)
        Dim g0 As Integer = ReadBits(Offset, BitPos, 7) : Dim g1 As Integer = ReadBits(Offset, BitPos, 7)
        Dim b0 As Integer = ReadBits(Offset, BitPos, 7) : Dim b1 As Integer = ReadBits(Offset, BitPos, 7)
        Dim a0 As Integer = ReadBits(Offset, BitPos, 7) : Dim a1 As Integer = ReadBits(Offset, BitPos, 7)
        Dim p0 As Integer = ReadBits(Offset, BitPos, 1) : Dim p1 As Integer = ReadBits(Offset, BitPos, 1)
        r0 = (r0 << 1) Or p0 : r1 = (r1 << 1) Or p1
        g0 = (g0 << 1) Or p0 : g1 = (g1 << 1) Or p1
        b0 = (b0 << 1) Or p0 : b1 = (b1 << 1) Or p1
        a0 = (a0 << 1) Or p0 : a1 = (a1 << 1) Or p1
        For i As Integer = 0 To 15
            Dim Index As Integer = ReadBits(Offset, BitPos, If(i = 0, 3, 4))
            Dim w As Integer = Weight4(Index)
            Dim iw As Integer = 64 - w
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(((b0 * iw + b1 * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 1) = CByte(((g0 * iw + g1 * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 2) = CByte(((r0 * iw + r1 * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 3) = CByte(((a0 * iw + a1 * w + 32) >> 6) And 255)
            End If
        Next
    End Sub

    Private Sub DecodeMode7(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer)
        Dim Partition As Integer = ReadBits(Offset, BitPos, 6)
        Dim R(3), G(3), B(3), A(3), P(3) As Integer
        For i As Integer = 0 To 3
            R(i) = ReadBits(Offset, BitPos, 5)
        Next
        For i As Integer = 0 To 3
            G(i) = ReadBits(Offset, BitPos, 5)
        Next
        For i As Integer = 0 To 3
            B(i) = ReadBits(Offset, BitPos, 5)
        Next
        For i As Integer = 0 To 3
            A(i) = ReadBits(Offset, BitPos, 5)
        Next
        For i As Integer = 0 To 3
            P(i) = ReadBits(Offset, BitPos, 1)
        Next
        For i As Integer = 0 To 3
            R(i) = (R(i) << 1) Or P(i) : R(i) = (R(i) << 2) Or (R(i) >> 4)
            G(i) = (G(i) << 1) Or P(i) : G(i) = (G(i) << 2) Or (G(i) >> 4)
            B(i) = (B(i) << 1) Or P(i) : B(i) = (B(i) << 2) Or (B(i) >> 4)
            A(i) = (A(i) << 1) Or P(i) : A(i) = (A(i) << 2) Or (A(i) >> 4)
        Next
        Dim pTableVal As Integer = PartitionTable2(Partition)
        Dim subsetMap(15) As Integer
        Dim Anchors() As Integer = {0, AnchorIndexTable2(Partition)}
        For i As Integer = 0 To 15
            subsetMap(i) = (pTableVal >> i) And 1
        Next
        For i As Integer = 0 To 15
            Dim s As Integer = subsetMap(i)
            Dim numBits As Integer = 2
            If i = Anchors(0) OrElse i = Anchors(1) Then
                numBits -= 1
            End If
            Dim index As Integer = ReadBits(Offset, BitPos, numBits)
            Dim w As Integer = Weight2(index)
            Dim iw As Integer = 64 - w
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = CByte(((B(s * 2) * iw + B(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 1) = CByte(((G(s * 2) * iw + G(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 2) = CByte(((R(s * 2) * iw + R(s * 2 + 1) * w + 32) >> 6) And 255)
                DecodedBytes(destIdx + 3) = CByte(((A(s * 2) * iw + A(s * 2 + 1) * w + 32) >> 6) And 255)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Replaces the specific block with a solid color, useful for debugging.
    ''' </summary>
    ''' <param name="ColorIndex">0: Red, 1: Green, 2: Blue, 3: Yellow, 4: Magenta, 5: Cyan, 6: White, 7: Black</param>
    Private Sub DecodeModeDiagnostic(Offset As Integer, ByRef BitPos As Integer, xPixelBase As Integer, yPixelBase As Integer, Optional ColorIndex As Integer = 4)
        Dim B() As Byte = {0, 0, 255, 0, 255, 255, 255, 0}
        Dim G() As Byte = {0, 255, 0, 255, 0, 255, 255, 0}
        Dim R() As Byte = {255, 0, 0, 255, 255, 0, 255, 0}
        Dim safeIdx As Integer = Math.Abs(ColorIndex) Mod 8
        For i As Integer = 0 To 15
            Dim pX As Integer = xPixelBase + (i And 3)
            Dim pY As Integer = yPixelBase + (i >> 2)
            If pX < Width AndAlso pY < Height Then
                Dim destIdx As Integer = (pY * Width + pX) * 4
                DecodedBytes(destIdx) = B(safeIdx)
                DecodedBytes(destIdx + 1) = G(safeIdx)
                DecodedBytes(destIdx + 2) = R(safeIdx)
                DecodedBytes(destIdx + 3) = 255
            End If
        Next
        BitPos = 128
    End Sub

    Private Function ReadBits(Offset As Integer, ByRef BitPosition As Integer, BitCount As Integer) As Integer
        Dim Value As Integer = 0
        Dim BitsRead As Integer = 0
        While BitsRead < BitCount
            Dim ByteIdx As Integer = BitPosition >> 3
            Dim BitInByte As Integer = BitPosition And 7
            Dim BitsToReadFromByte As Integer = Math.Min(BitCount - BitsRead, 8 - BitInByte)
            Dim Mask As Integer = (1 << BitsToReadFromByte) - 1
            Dim Bits As Integer = (SourceBytes(Offset + ByteIdx) >> BitInByte) And Mask
            Value = Value Or (Bits << BitsRead)
            BitPosition += BitsToReadFromByte
            BitsRead += BitsToReadFromByte
        End While
        Return Value
    End Function

#End Region

    Public Sub Save(Path As String, Format As ImageFormat)
        BeginDecode()
        Using TempImage As New Bitmap(Width, Height, PixelFormat.Format32bppArgb)
            Dim Rect As New Rectangle(0, 0, Width, Height)
            Dim TempData As BitmapData = TempImage.LockBits(Rect, ImageLockMode.WriteOnly, TempImage.PixelFormat)
            Marshal.Copy(DecodedBytes, 0, TempData.Scan0, DecodedBytes.Length)
            TempImage.UnlockBits(TempData)
            TempImage.Save(Path, Format)
        End Using
    End Sub

    Public Function ToBitmap() As Bitmap
        BeginDecode()
        Dim TempImage As New Bitmap(Width, Height, PixelFormat.Format32bppArgb)
        Dim Rect As New Rectangle(0, 0, Width, Height)
        Dim TempData As BitmapData = TempImage.LockBits(Rect, ImageLockMode.WriteOnly, TempImage.PixelFormat)
        Marshal.Copy(DecodedBytes, 0, TempData.Scan0, DecodedBytes.Length)
        TempImage.UnlockBits(TempData)
        Return TempImage
    End Function

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
