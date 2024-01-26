' DirectBitmap Class by WalkerMx
' Based on the documentation found here:
' http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddsfileformat.htm

Public Class DDS
    Implements IDisposable

    Public Disposed As Boolean

    Private HeaderBytes As New List(Of Byte)
    Private PayloadBytes As New List(Of Byte)
    Private dwReserved As Byte() = {0, 0, 0, 0}

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

    ''' <summary>
    ''' Creates a DDS Image from a standard Image file.
    ''' </summary>
    ''' <param name="SourceImage">Image to create DDS from.</param>
    ''' <param name="Alpha">Alpha support.  If using compression, will use 1-bit alpha.</param>
    ''' <param name="Compress">Applies DXT1 compression.  Width and height need to be divisible by four.</param>
    ''' <param name="MipMaps">Number of mipmaps to create.  Use -1 for auto.</param>
    Public Sub New(SourceImage As Image, Alpha As Boolean, Compress As Boolean, MipMaps As Integer)

        Dim DDS_SurfaceFlags As New List(Of SurfaceFlags)
        Dim DDS_PixelFlags As New List(Of PixelFlags)
        Dim DDS_Caps1 As New List(Of Caps1)
        Dim PLS As Integer
        Dim RMask As Byte() = {0, 0, &HFF, 0}   ' BGRA/BGRX
        Dim GMask As Byte() = {0, &HFF, 0, 0}
        Dim BMask As Byte() = {&HFF, 0, 0, 0}
        Dim AMask As Byte() = {0, 0, 0, &HFF}

        DDS_SurfaceFlags.AddRange({SurfaceFlags.DDSD_CAPS, SurfaceFlags.DDSD_PIXELFORMAT, SurfaceFlags.DDSD_WIDTH, SurfaceFlags.DDSD_HEIGHT})
        DDS_Caps1.Add(Caps1.DDSCAPS_TEXTURE)

        If MipMaps = -1 Then MipMaps = CalcMips(SourceImage.Width, SourceImage.Height)

        If Alpha = True Then
            DDS_PixelFlags.Add(PixelFlags.DDPF_ALPHAPIXELS)
        End If

        If Compress = True Then
            DDS_SurfaceFlags.Add(SurfaceFlags.DDSD_PITCH)
            DDS_PixelFlags.Add(PixelFlags.DDPF_FOURCC)
            PLS = SourceImage.Width * 2
            RMask = {0, 0, 0, 0}
            GMask = {0, 0, 0, 0}
            BMask = {0, 0, 0, 0}
            AMask = {0, 0, 0, 0}
        Else
            DDS_SurfaceFlags.Add(SurfaceFlags.DDSD_LINEARSIZE)
            DDS_PixelFlags.Add(PixelFlags.DDPF_RGB)
            PLS = SourceImage.Width * SourceImage.Height * 0.5
        End If

        If MipMaps > 0 Then
            DDS_SurfaceFlags.Add(SurfaceFlags.DDSD_MIPMAPCOUNT)
            DDS_Caps1.Add(Caps1.DDSCAPS_COMPLEX)
            DDS_Caps1.Add(Caps1.DDSCAPS_MIPMAP)
        End If

        HeaderBytes.AddRange(OrderBytes("DDS "))                                ' dwMagic
        HeaderBytes.AddRange(OrderBytes(124))                                   ' dwSize
        HeaderBytes.AddRange(OrderBytes(DDS_SurfaceFlags.ToArray))              ' dwFlags
        HeaderBytes.AddRange(OrderBytes(SourceImage.Height))                    ' dwHeight
        HeaderBytes.AddRange(OrderBytes(SourceImage.Width))                     ' dwWidth
        HeaderBytes.AddRange(OrderBytes(PLS))                                   ' dwPitchOrLinearSize
        HeaderBytes.AddRange(OrderBytes(0))                                     ' dwDepth
        HeaderBytes.AddRange(OrderBytes(MipMaps))                               ' dwMipMapCount

        For i = 0 To 10
            HeaderBytes.AddRange(dwReserved)                                    ' dwReserved1 x 11
        Next

        HeaderBytes.AddRange(OrderBytes(32))                                    ' DDPIXELFORMAT dwSize
        HeaderBytes.AddRange(OrderBytes(DDS_PixelFlags.ToArray))                ' DDPIXELFORMAT dwFlags
        HeaderBytes.AddRange(IIf(Compress, OrderBytes("DXT1"), OrderBytes(0)))  ' DDPIXELFORMAT dwFourCC
        HeaderBytes.AddRange(IIf(Compress, OrderBytes(0), OrderBytes(32)))      ' DDPIXELFORMAT dwRGBBitCount
        HeaderBytes.AddRange(RMask)                                             ' DDPIXELFORMAT dwRBitMask
        HeaderBytes.AddRange(GMask)                                             ' DDPIXELFORMAT dwGBitMask
        HeaderBytes.AddRange(BMask)                                             ' DDPIXELFORMAT dwBBitMask
        HeaderBytes.AddRange(AMask)                                             ' DDPIXELFORMAT dwABitMask

        HeaderBytes.AddRange(OrderBytes(DDS_Caps1.ToArray))                     ' DDCAPS2 dwCaps1
        HeaderBytes.AddRange(OrderBytes(0))                                     ' DDCAPS2 dwCaps2 (Unused)

        For i = 0 To 1
            HeaderBytes.AddRange(dwReserved)                                    ' DDCAPS2 Reserved x 2
        Next

        HeaderBytes.AddRange(dwReserved)                                        ' dwReserved2

        If Alpha And Compress Then
            PayloadBytes = BlockCompressAlpha(SourceImage).ToList
        ElseIf Alpha Then
            PayloadBytes = WriteUncompressedAlpha(SourceImage).ToList
        ElseIf Compress Then
            PayloadBytes = BlockCompress(SourceImage).ToList
        Else
            PayloadBytes = WriteUncompressed(SourceImage).ToList
        End If

        If MipMaps > 0 Then
            Dim MipMap As Bitmap = SourceImage
            For i = 0 To MipMaps - 1
                MipMap = HalveImage(MipMap)
                If Alpha And Compress Then
                    PayloadBytes.AddRange(BlockCompressAlpha(MipMap))
                ElseIf Alpha Then
                    PayloadBytes.AddRange(WriteUncompressedAlpha(MipMap))
                ElseIf Compress Then
                    PayloadBytes.AddRange(BlockCompress(MipMap))
                Else
                    PayloadBytes.AddRange(WriteUncompressed(MipMap))
                End If
            Next
        End If

    End Sub

    Private Function WriteUncompressed(Source As Image) As Byte()           ' B8G8R8X8_UNORM
        Dim Result As New List(Of Byte)
        Using SourceDirect As New DirectBitmap(Source)
            For y = 0 To SourceDirect.Height - 1
                For x = 0 To SourceDirect.Width - 1
                    Dim TempPixel As Color = SourceDirect.GetPixel(x, y)
                    Result.Add(TempPixel.B)
                    Result.Add(TempPixel.G)
                    Result.Add(TempPixel.R)
                    Result.Add(&HFF)
                Next
            Next
        End Using
        Return Result.ToArray
    End Function

    Private Function WriteUncompressedAlpha(Source As Image) As Byte()      ' B8G8R8A8_UNORM
        Dim Result As New List(Of Byte)
        Using SourceDirect As New DirectBitmap(Source)
            For y = 0 To SourceDirect.Height - 1
                For x = 0 To SourceDirect.Width - 1
                    Dim TempPixel As Color = SourceDirect.GetPixel(x, y)
                    Result.Add(TempPixel.B)
                    Result.Add(TempPixel.G)
                    Result.Add(TempPixel.R)
                    Result.Add(TempPixel.A)
                Next
            Next
        End Using
        Return Result.ToArray
    End Function

    Private Function BlockCompress(Source As Image) As Byte()               ' BC1_UNORM
        Dim Result As New List(Of Byte)

        Using SourceDirect As New DirectBitmap(Source)
            For y = 0 To SourceDirect.Height - 1 Step 4
                For x = 0 To SourceDirect.Width - 1 Step 4

                    Dim ValueMatrix()() As Integer = {({0, 0, 0, 0}), ({0, 0, 0, 0}), ({0, 0, 0, 0}), ({0, 0, 0, 0})}

                    Dim MaxPixelValue As Integer = -1
                    Dim MinPixelValue As Integer = 66000
                    Dim MaxPixel As New Color
                    Dim MinPixel As New Color

                    For j = 0 To 3
                        For i = 0 To 3
                            Dim CurColor As Color = SourceDirect.GetPixel(x + i, y + j)
                            Dim CurValue As Integer = Convert.ToInt32(RGB_888_To_565(CurColor), 2)
                            ValueMatrix(j)(i) = CurValue
                            If CurValue > MaxPixelValue Then
                                MaxPixelValue = CurValue
                                MaxPixel = CurColor
                            End If
                            If CurValue < MinPixelValue Then
                                MinPixelValue = CurValue
                                MinPixel = CurColor
                            End If
                        Next
                    Next

                    Dim Color0 As String = RGB_888_To_565(MaxPixel)
                    Dim Color1 As String = RGB_888_To_565(MinPixel)

                    Result.Add(Convert.ToByte(Color0.Substring(8, 8), 2))
                    Result.Add(Convert.ToByte(Color0.Substring(0, 8), 2))
                    Result.Add(Convert.ToByte(Color1.Substring(8, 8), 2))
                    Result.Add(Convert.ToByte(Color1.Substring(0, 8), 2))

                    Dim StepVal As Integer = Math.Floor((MaxPixelValue - MinPixelValue) / 3)

                    For j = 0 To 3
                        Dim BitString As New Text.StringBuilder
                        For i = 3 To 0 Step -1
                            Dim CurValue As Integer = ValueMatrix(j)(i)
                            Select Case CurValue
                                Case >= (StepVal * 2) + MinPixelValue
                                    BitString.Append("00")
                                Case >= StepVal + MinPixelValue
                                    BitString.Append("10")
                                Case >= MinPixelValue
                                    BitString.Append("11")
                                Case Else
                                    BitString.Append("01")
                            End Select
                        Next
                        Result.Add(Convert.ToByte(BitString.ToString, 2))
                        BitString.Clear()
                    Next

                Next
            Next
        End Using

        Return Result.ToArray
    End Function

    Private Function BlockCompressAlpha(Source As Image) As Byte()          ' BC1_UNORM with 1-bit Alpha
        Dim Result As New List(Of Byte)

        Using SourceDirect As New DirectBitmap(Source)
            For y = 0 To SourceDirect.Height - 1 Step 4
                For x = 0 To SourceDirect.Width - 1 Step 4

                    Dim ValueMatrix()() As Integer = {({0, 0, 0, 0}), ({0, 0, 0, 0}), ({0, 0, 0, 0}), ({0, 0, 0, 0})}

                    Dim MaxPixelValue As Integer = -1
                    Dim MinPixelValue As Integer = 66000
                    Dim MaxPixel As New Color
                    Dim MinPixel As New Color

                    For j = 0 To 3
                        For i = 0 To 3
                            Dim CurColor As Color = SourceDirect.GetPixel(x + i, y + j)
                            Dim CurValue As Integer = Convert.ToInt32(RGB_888_To_565(CurColor), 2)
                            ValueMatrix(j)(i) = IIf(CurColor.A < &HFF, 0, CurValue)
                            If CurValue > MaxPixelValue Then
                                MaxPixelValue = CurValue
                                MaxPixel = CurColor
                            End If
                            If CurValue < MinPixelValue Then
                                MinPixelValue = CurValue
                                MinPixel = CurColor
                            End If
                        Next
                    Next

                    Dim Color0 As String = RGB_888_To_565(MinPixel)
                    Dim Color1 As String = RGB_888_To_565(MaxPixel)

                    Result.Add(Convert.ToByte(Color0.Substring(8, 8), 2))
                    Result.Add(Convert.ToByte(Color0.Substring(0, 8), 2))
                    Result.Add(Convert.ToByte(Color1.Substring(8, 8), 2))
                    Result.Add(Convert.ToByte(Color1.Substring(0, 8), 2))

                    Dim StepVal As Integer = Math.Floor((MaxPixelValue - MinPixelValue) / 2)

                    For j = 0 To 3
                        Dim BitString As New Text.StringBuilder
                        For i = 3 To 0 Step -1
                            Dim CurValue As Integer = ValueMatrix(j)(i)
                            Select Case CurValue
                                Case = 0
                                    BitString.Append("11")
                                Case >= StepVal + MinPixelValue
                                    BitString.Append("00")
                                Case >= MinPixelValue
                                    BitString.Append("10")
                                Case Else
                                    BitString.Append("01")
                            End Select
                        Next
                        Result.Add(Convert.ToByte(BitString.ToString, 2))
                        BitString.Clear()
                    Next

                Next
            Next
        End Using

        Return Result.ToArray
    End Function

    Public Sub SaveImage(FilePath As String)
        Dim FileBytes As New List(Of Byte)
        FileBytes.AddRange(HeaderBytes)
        FileBytes.AddRange(PayloadBytes)
        IO.File.WriteAllBytes(FilePath, FileBytes.ToArray)
    End Sub

    Private Function RGB_888_To_565(Source As Color) As String
        Dim Result As New List(Of Byte)
        Dim R5Val As Integer = Math.Floor(Source.R * 0.122)
        Dim G6Val As Integer = Math.Floor(Source.G * 0.248)
        Dim B5Val As Integer = Math.Floor(Source.B * 0.122)
        Return PrepBits(R5Val, 5) & PrepBits(G6Val, 6) & PrepBits(B5Val, 5)
    End Function

    Private Function CalcMips(Width As Integer, Height As Integer) As Integer
        Dim xMips As Integer = GetDivTwo(Width)
        Dim yMips As Integer = GetDivTwo(Height)
        Return Math.Min(xMips, yMips)
    End Function

    Private Function GetDivTwo(Source As Integer) As Integer
        For i = 12 To 0 Step -1
            Dim DivVal As Double = Source / (2 ^ i)
            If DivVal = CInt(DivVal) AndAlso DivVal > 8 Then Return i
        Next
        Return 0
    End Function

    Private Function HalveImage(Source As Image) As Image
        Dim Result As New Bitmap(CInt(Source.Width * 0.5), CInt(Source.Height * 0.5))
        Using Gr As Graphics = Graphics.FromImage(Result)
            Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear
            Gr.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
            Gr.DrawImage(Source, 0, 0, Result.Width, Result.Height)
        End Using
        Return Result
    End Function

    Private Function PrepBits(Source As Integer, Length As Integer) As String
        Return Convert.ToString(Source, 2).PadLeft(Length, "0"c)
    End Function

    Private Function OrderBytes(Source As Integer()) As Byte()
        Return BitConverter.GetBytes(Source.Sum)
    End Function

    Private Function OrderBytes(Source As Integer) As Byte()
        Return BitConverter.GetBytes(Source)
    End Function

    Private Function OrderBytes(Source As String) As Byte()
        Return Text.Encoding.ASCII.GetBytes(Source)
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
