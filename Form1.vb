Imports System.IO
Imports System.Text
Imports System.Numerics
Imports System.Drawing.Imaging

Public Class Form1

    Dim FilePath As String
    Dim PreviewImage As Image

    Private CubeMode As Boolean = False
    Private CubeVerts(7) As Vector3
    Private CubeFaces(5) As CubeFace
    Private PreviewCubeFaces(5) As CubeFace
    Private PreviewScale As Single = 120.0F

    Private Class CubeFace
        Public Image As Bitmap
        Public V0 As Integer
        Public V1 As Integer
        Public V2 As Integer
        Public V3 As Integer
        Public Normal As Vector3
    End Class

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        DoubleBuffered = True
        OutputFormatComboBox.SelectedIndex = 0
        CubeVerts(0) = New Vector3(-1, 1, 1)
        CubeVerts(1) = New Vector3(1, 1, 1)
        CubeVerts(2) = New Vector3(1, -1, 1)
        CubeVerts(3) = New Vector3(-1, -1, 1)
        CubeVerts(4) = New Vector3(-1, 1, -1)
        CubeVerts(5) = New Vector3(1, 1, -1)
        CubeVerts(6) = New Vector3(1, -1, -1)
        CubeVerts(7) = New Vector3(-1, -1, -1)
    End Sub

    Private Async Sub LoadImageButton_Click(sender As Object, e As EventArgs) Handles LoadImageButton.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp;*.dds"}
            If OFD.ShowDialog = DialogResult.OK Then
                Dim Extension As String = Path.GetExtension(OFD.FileName).ToLower
                InfoTextBox.Clear()
                If PreviewImage IsNot Nothing Then PreviewImage.Dispose()
                DisposeCubeFaces()
                FilePath = OFD.FileName
                Dim ResultText As String = ""
                If Extension = ".dds" Then
                    Try
                        Using DDSDecoder As New DDS_Decoder(OFD.FileName)
                            InfoTextBox.Text = GetDDSReport(DDSDecoder)
                            If DDSDecoder.IsCubeMap Then
                                CubeMode = True
                                Dim TempCubeMaps As Bitmap() = Await Task.Run(Function() DDSDecoder.ToCubeBitmaps())
                                LoadCubeMaps(PreviewCubeFaces, TempCubeMaps)

                                LoadCubeMaps(CubeFaces, TempCubeMaps)
                            Else
                                CubeMode = False
                                PreviewImage = Await Task.Run(Function() DDSDecoder.ToBitmap())
                            End If
                        End Using
                        DDSExportGroup.Enabled = False
                        ImageExportGroup.Enabled = True
                    Catch ex As Exception
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        DDSExportGroup.Enabled = False
                        ImageExportGroup.Enabled = False
                        PreviewImage = New Bitmap(1, 1)
                    End Try
                Else
                    Using TempImage As Image = Image.FromFile(OFD.FileName)
                        InfoTextBox.Text = GetImageReport(TempImage)
                        PreviewImage = New Bitmap(TempImage)
                    End Using
                    DDSExportGroup.Enabled = True
                    ImageExportGroup.Enabled = False
                End If
                If CubeMode Then
                    PreviewPictureBox.Image = Nothing
                Else
                    PreviewPictureBox.Image = PreviewImage
                End If
                Me.Refresh()
            End If
        End Using
    End Sub

    Private Sub ExportImageButton_Click(sender As Object, e As EventArgs) Handles ExportImageButton.Click
        Dim FileExt As String = OutputFormatComboBox.SelectedItem.ToString
        Using SFD As New SaveFileDialog With {.Filter = $"{FileExt} Files|*.{FileExt.ToLower}|All Files|*.*", .FileName = Path.GetFileNameWithoutExtension(FilePath)}
            If SFD.ShowDialog = DialogResult.OK Then
                LoadImageButton.Enabled = False
                ExportImageButton.Enabled = False
                Select Case FileExt
                    Case "PNG"
                        PreviewImage.Save(SFD.FileName, ImageFormat.Png)
                    Case "JPG"
                        PreviewImage.Save(SFD.FileName, ImageFormat.Jpeg)
                    Case "BMP"
                        PreviewImage.Save(SFD.FileName, ImageFormat.Bmp)
                End Select
                ExportImageButton.Enabled = True
                LoadImageButton.Enabled = True
            End If
        End Using
    End Sub

    Private Async Sub ExportDDSButton_Click(sender As Object, e As EventArgs) Handles ExportDDSButton.Click
        If OverrideComboBox.SelectedItem Is Nothing Then Return
        Dim targetFormat As DXGI_Format = GetFormatFromString(OverrideComboBox.SelectedItem.ToString())
        Dim isLegacy As Boolean = Not ExtendedHeaderCheckBox.Checked
        Dim doMipMaps As Boolean = MipMapCheckBox.Checked
        Using SFD As New SaveFileDialog With {.Filter = "DDS Files|*.dds|All Files|*.*", .FileName = Path.GetFileNameWithoutExtension(FilePath)}
            If SFD.ShowDialog = DialogResult.OK Then
                LoadImageButton.Enabled = False
                ExportDDSButton.Enabled = False
                Using DDSEncoder As New DDS_Encoder(FilePath, targetFormat, doMipMaps, isLegacy)
                    Await Task.Run(Sub() DDSEncoder.Save(SFD.FileName))
                End Using
                ExportDDSButton.Enabled = True
                LoadImageButton.Enabled = True
            End If
        End Using
    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PreviewPictureBox.Paint
        If CubeMode Then
            TrackBarH.Visible = True
            TrackBarV.Visible = True
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.None
            e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            e.Graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
            Dim CubeYaw As Single = CSng(TrackBarH.Value * (Math.PI / 180.0))
            Dim CubePitch As Single = CSng(-TrackBarV.Value * (Math.PI / 180.0))
            Dim RotationMatrix As Matrix4x4 = Matrix4x4.CreateFromYawPitchRoll(CubeYaw, CubePitch, 0)
            Dim LightDir As Vector3 = Vector3.Normalize(New Vector3(-0.5F, 0.5F, 1.0F))
            Dim OffsetX As Single = PreviewPictureBox.Width / 2.0F
            Dim OffsetY As Single = PreviewPictureBox.Height / 2.0F
            Dim PreviewPoints(7) As PointF
            For i As Integer = 0 To 7
                Dim Rotation As Vector3 = Vector3.Transform(CubeVerts(i), RotationMatrix)
                PreviewPoints(i) = New PointF(OffsetX + (Rotation.X * PreviewScale), OffsetY - (Rotation.Y * PreviewScale))
            Next
            For Each Face In CubeFaces
                Dim RotationNormal As Vector3 = Vector3.Transform(Face.Normal, RotationMatrix)
                If RotationNormal.Z > 0 Then
                    Dim DestPoints() As PointF = {PreviewPoints(Face.V0), PreviewPoints(Face.V1), PreviewPoints(Face.V3)}
                    Dim BottomRight As New PointF(DestPoints(1).X + DestPoints(2).X - DestPoints(0).X, DestPoints(1).Y + DestPoints(2).Y - DestPoints(0).Y)
                    Dim CenterX As Single = (DestPoints(0).X + BottomRight.X) / 2.0F
                    Dim CenterY As Single = (DestPoints(0).Y + BottomRight.Y) / 2.0F
                    Dim overlap As Single = 0.5F
                    For p As Integer = 0 To 2
                        Dim dx As Single = DestPoints(p).X - CenterX
                        Dim dy As Single = DestPoints(p).Y - CenterY
                        Dim len As Single = CSng(Math.Sqrt(dx * dx + dy * dy))
                        If len > 0 Then
                            DestPoints(p).X += (dx / len) * overlap
                            DestPoints(p).Y += (dy / len) * overlap
                        End If
                    Next
                    Dim normNormal As Vector3 = Vector3.Normalize(RotationNormal)
                    Dim dot As Single = Vector3.Dot(normNormal, LightDir)
                    Dim brightness As Single = 0.3F + (Math.Max(0, dot) * 0.7F)
                    Dim cm As New ColorMatrix(New Single()() {
                                              New Single() {brightness, 0, 0, 0, 0},
                                              New Single() {0, brightness, 0, 0, 0},
                                              New Single() {0, 0, brightness, 0, 0},
                                              New Single() {0, 0, 0, 1, 0},
                                              New Single() {0, 0, 0, 0, 1}})
                    Using imgAttr As New ImageAttributes()
                        imgAttr.SetWrapMode(Drawing2D.WrapMode.TileFlipXY)
                        imgAttr.SetColorMatrix(cm)
                        Dim srcRect As New Rectangle(0, 0, Face.Image.Width, Face.Image.Height)
                        e.Graphics.DrawImage(Face.Image, DestPoints, srcRect, GraphicsUnit.Pixel, imgAttr)
                    End Using
                End If
            Next
        Else
            TrackBarH.Visible = False
            TrackBarV.Visible = False
        End If
    End Sub

    Private Sub TrackBarH_Scroll(sender As Object, e As EventArgs) Handles TrackBarH.Scroll, TrackBarH.ValueChanged
        PreviewPictureBox.Invalidate()
    End Sub

    Private Sub TrackBarV_Scroll(sender As Object, e As EventArgs) Handles TrackBarV.Scroll, TrackBarV.ValueChanged
        PreviewPictureBox.Invalidate()
    End Sub

    Private Sub LoadCubeMaps(CubeFaces As CubeFace(), CubeMapImages As Bitmap())
        CubeFaces(0) = New CubeFace With {.V0 = 0, .V1 = 1, .V2 = 2, .V3 = 3, .Normal = New Vector3(0, 0, 1), .Image = CubeMapImages(4)}   ' Z+
        CubeFaces(1) = New CubeFace With {.V0 = 5, .V1 = 4, .V2 = 7, .V3 = 6, .Normal = New Vector3(0, 0, -1), .Image = CubeMapImages(5)}  ' Z-
        CubeFaces(2) = New CubeFace With {.V0 = 4, .V1 = 5, .V2 = 1, .V3 = 0, .Normal = New Vector3(0, 1, 0), .Image = CubeMapImages(2)}   ' Y+
        CubeFaces(3) = New CubeFace With {.V0 = 3, .V1 = 2, .V2 = 6, .V3 = 7, .Normal = New Vector3(0, -1, 0), .Image = CubeMapImages(3)}  ' Y-
        CubeFaces(4) = New CubeFace With {.V0 = 4, .V1 = 0, .V2 = 3, .V3 = 7, .Normal = New Vector3(-1, 0, 0), .Image = CubeMapImages(1)}  ' X-
        CubeFaces(5) = New CubeFace With {.V0 = 1, .V1 = 5, .V2 = 6, .V3 = 2, .Normal = New Vector3(1, 0, 0), .Image = CubeMapImages(0)}   ' X+
    End Sub

    Public Sub UpdateOverrideFormats(sender As Object, e As EventArgs) Handles CompressionCheckBox.CheckedChanged, SmoothAlphaRB.CheckedChanged, SharpAlphaRB.CheckedChanged, NoAlphaRB.CheckedChanged, ExtendedHeaderCheckBox.CheckedChanged, NormalCheckBox.CheckedChanged
        Dim IsDX10 As Boolean = ExtendedHeaderCheckBox.Checked
        Dim AlphaMode As Integer = 0
        If NoAlphaRB.Checked Then AlphaMode = 0
        If SharpAlphaRB.Checked Then AlphaMode = 1
        If SmoothAlphaRB.Checked Then AlphaMode = 2
        OverrideComboBox.Items.Clear()
        If NormalCheckBox.Checked Then
            If CompressionCheckBox.Checked Then
                OverrideComboBox.Items.Add(If(IsDX10, "BC5 UNORM", "ATI2 (BC5)"))
            Else
                OverrideComboBox.Items.Add("BGRX (B8G8R8X8)")
            End If
        ElseIf CompressionCheckBox.Checked Then
            Select Case AlphaMode
                Case 0
                    If IsDX10 Then OverrideComboBox.Items.Add("BC7 UNORM")
                    OverrideComboBox.Items.Add(If(IsDX10, "BC1 UNORM", "DXT1"))
                    OverrideComboBox.Items.Add(If(IsDX10, "BC4 UNORM", "ATI1 (BC4)"))
                Case 1
                    OverrideComboBox.Items.Add(If(IsDX10, "BC1 UNORM", "DXT1"))
                    OverrideComboBox.Items.Add(If(IsDX10, "BC2 UNORM", "DXT3"))
                Case 2
                    If IsDX10 Then OverrideComboBox.Items.Add("BC7 UNORM")
                    OverrideComboBox.Items.Add(If(IsDX10, "BC3 UNORM", "DXT5"))
            End Select
        Else
            Select Case AlphaMode
                Case 0
                    OverrideComboBox.Items.Add("BGRX (B8G8R8X8)")
                Case 1, 2
                    OverrideComboBox.Items.Add("BGRA (B8G8R8A8)")
            End Select
        End If
        SelectFirstItem(OverrideComboBox)
        OverrideComboBox.Enabled = (OverrideComboBox.Items.Count > 1)
    End Sub

    Private Function GetFormatFromString(FormatName As String) As DXGI_Format
        Select Case FormatName
            Case "BC1 UNORM", "DXT1"
                Return DXGI_Format.DXGI_FORMAT_BC1_UNORM
            Case "BC2 UNORM", "DXT3"
                Return DXGI_Format.DXGI_FORMAT_BC2_UNORM
            Case "BC3 UNORM", "DXT5"
                Return DXGI_Format.DXGI_FORMAT_BC3_UNORM
            Case "BC4 UNORM", "ATI1 (BC4)"
                Return DXGI_Format.DXGI_FORMAT_BC4_UNORM
            Case "BC5 UNORM", "ATI2 (BC5)"
                Return DXGI_Format.DXGI_FORMAT_BC5_UNORM
            Case "BC7 UNORM"
                Return DXGI_Format.DXGI_FORMAT_BC7_UNORM
            Case "BGRX (B8G8R8X8)"
                Return DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM
            Case "BGRA (B8G8R8A8)"
                Return DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM
            Case Else
                Throw New Exception($"Unsupported format: {FormatName}")
        End Select
    End Function

    Public Function GetDDSReport(Source As DDS_Decoder) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("===== Info =====")
        sb.AppendLine()
        sb.AppendLine("[Core Properties]")
        sb.AppendLine($"Signature:        {Source.Signature}")
        sb.AppendLine($"Resolution:       {Source.Width} x {Source.Height}")
        If Source.Depth > 0 Then
            sb.AppendLine($"Depth:            {Source.Depth}")
        End If
        sb.AppendLine($"MipMap Count:     {Source.MipMapCount}")
        sb.AppendLine($"Pitch/Linear Size:{Source.PitchLinearSize} bytes")
        sb.AppendLine($"Extended Header:  {Source.ExtendedHeader}")
        sb.AppendLine()
        sb.AppendLine("[Pixel Format]")
        sb.AppendLine($"Header Size:      {Source.HeaderSize} bytes")
        sb.AppendLine($"Sub-Header Size:  {Source.SubHeaderSize} bytes")
        If Not String.IsNullOrWhiteSpace(Source.FourCC) Then
            sb.AppendLine($"FourCC:           {Source.FourCC}")
        End If
        sb.AppendLine($"RGB Bit Count:    {Source.RGBBitCount}")
        sb.AppendLine($"Red Bit Mask:     0x{Source.RedBitMask.ToString("X8")}")
        sb.AppendLine($"Green Bit Mask:   0x{Source.GreenBitMask.ToString("X8")}")
        sb.AppendLine($"Blue Bit Mask:    0x{Source.BlueBitMask.ToString("X8")}")
        sb.AppendLine($"Alpha Bit Mask:   0x{Source.AlphaBitMask.ToString("X8")}")
        sb.AppendLine()
        sb.AppendLine("[Surface & Capabilities]")
        sb.AppendLine($"Surface Flags:    {Source.SurfaceFlags.ToString()}")
        sb.AppendLine($"Pixel Flags:      {Source.PixelFlags.ToString()}")
        sb.AppendLine($"Caps 1:           {Source.Caps1.ToString()}")
        sb.AppendLine($"Caps 2:           {Source.Caps2.ToString()}")
        sb.AppendLine()
        If Source.ExtendedHeader Then
            sb.AppendLine("[DX10 Extended Header]")
            sb.AppendLine($"DXGI Format:      {Source.DXGIFormat.ToString()}")
            sb.AppendLine($"Dimension:        {Source.ResourceDimension.ToString()}")
            sb.AppendLine($"Array Size:       {Source.ArraySize}")
            sb.AppendLine($"Misc Flag 1:      {Source.MiscFlag.ToString()}")
            sb.AppendLine($"Misc Flag 2:      {Source.MiscFlags2.ToString()}")
            sb.AppendLine()
        End If
        Return sb.ToString()
    End Function

    Public Function GetImageReport(img As Image) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("===== Info =====")
        sb.AppendLine()
        sb.AppendLine("[Core Properties]")
        sb.AppendLine($"Resolution:       {img.Width} x {img.Height}")
        sb.AppendLine($"Source Format:    {GetImageFormatName(img.RawFormat)}")
        sb.AppendLine($"DPI (Print Res):  {Math.Round(img.HorizontalResolution)} x {Math.Round(img.VerticalResolution)} DPI")
        sb.AppendLine()
        sb.AppendLine("[Pixel Format]")
        sb.AppendLine($"Format Layout:    {img.PixelFormat.ToString()}")
        Dim hasAlpha As Boolean = Image.IsAlphaPixelFormat(img.PixelFormat)
        sb.AppendLine($"Has Alpha Channel:{If(hasAlpha, "Yes", "No")}")
        Dim bitsPerPixel As Integer = Image.GetPixelFormatSize(img.PixelFormat)
        Dim rawMemoryBytes As Long = CLng(img.Width) * img.Height * (bitsPerPixel \ 8)
        sb.AppendLine($"Bit Depth:        {bitsPerPixel} bits per pixel")
        sb.AppendLine($"Uncompressed Size:{rawMemoryBytes:N0} bytes")
        Return sb.ToString()
    End Function

    Private Function GetImageFormatName(format As ImageFormat) As String
        If format.Equals(ImageFormat.Png) Then Return "PNG"
        If format.Equals(ImageFormat.Jpeg) Then Return "JPEG"
        If format.Equals(ImageFormat.Bmp) Then Return "BMP"
        If format.Equals(ImageFormat.Tiff) Then Return "TIFF"
        If format.Equals(ImageFormat.Gif) Then Return "GIF"
        If format.Equals(ImageFormat.Icon) Then Return "ICO"
        If format.Equals(ImageFormat.MemoryBmp) Then Return "Memory Bitmap"
        Return "Unknown/Custom Format"
    End Function

    Private Sub DisposeCubeFaces()
        For Each Face In CubeFaces
            If Face IsNot Nothing AndAlso Face.Image IsNot Nothing Then
                Face.Image.Dispose()
                Face.Image = Nothing
            End If
        Next
    End Sub

    Private Sub SelectFirstItem(SourceControl As ComboBox)
        If SourceControl.Items.Count > 0 Then
            SourceControl.SelectedIndex = 0
        End If
    End Sub

End Class
