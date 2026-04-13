Imports System.IO
Imports System.Text
Imports System.Drawing.Imaging

Public Class Form1

    Dim Bench As Boolean = False
    Dim BenchTime As Integer
    Dim BenchTimer As Stopwatch

    Dim FilePath As String
    Dim PreviewImage As Image

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        OutputFormatComboBox.SelectedIndex = 0
    End Sub

    Private Async Sub LoadImageButton_Click(sender As Object, e As EventArgs) Handles LoadImageButton.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp;*.dds"}
            If OFD.ShowDialog = DialogResult.OK Then
                Dim Extension As String = Path.GetExtension(OFD.FileName).ToLower
                InfoTextBox.Clear()
                If PreviewImage IsNot Nothing Then PreviewImage.Dispose()
                FilePath = OFD.FileName
                Dim ResultText As String = ""
                If Extension = ".dds" Then
                    Try
                        Using DDSDecoder As New DDS_Decoder(OFD.FileName)
                            If Bench Then
                                BenchTime = 0
                                BenchTimer = Stopwatch.StartNew
                                For i = 0 To 49
                                    DDSDecoder.BeginDecode()
                                Next
                                BenchTimer.Stop()
                                BenchTime = BenchTimer.ElapsedMilliseconds
                                MsgBox($"Average: {BenchTime / 50}ms")
                            Else
                                InfoTextBox.Text = GetDDSReport(DDSDecoder)
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
                PreviewPictureBox.Image = PreviewImage
                Me.Refresh()
            End If
        End Using
    End Sub

    Private Sub ExportImageButton_Click(sender As Object, e As EventArgs) Handles ExportImageButton.Click
        Dim FileExt As String = OutputFormatComboBox.SelectedItem.ToString
        Using SFD As New SaveFileDialog With {.Filter = $"{FileExt} Files|*.{FileExt.ToLower}|All Files|*.*"}
            If SFD.ShowDialog = DialogResult.OK Then
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
            End If
        End Using
    End Sub

    Private Async Sub ExportDDSButton_Click(sender As Object, e As EventArgs) Handles ExportDDSButton.Click
        If OverrideComboBox.SelectedItem Is Nothing Then Return
        Dim targetFormat As DXGI_Format = GetFormatFromString(OverrideComboBox.SelectedItem.ToString())
        Dim isLegacy As Boolean = Not ExtendedHeaderCheckBox.Checked
        Dim doMipMaps As Boolean = MipMapCheckBox.Checked
        Using SFD As New SaveFileDialog With {.Filter = "DDS Files|*.dds|All Files|*.*"}
            If SFD.ShowDialog = DialogResult.OK Then
                ExportDDSButton.Enabled = False
                Using DDSEncoder As New DDS_Encoder(FilePath, targetFormat, doMipMaps, isLegacy)
                    If Bench Then
                        Dim BenchTime As Long = 0
                        Dim BenchTimer = Stopwatch.StartNew()
                        For i = 0 To 49
                            DDSEncoder.BeginEncode()
                        Next
                        BenchTimer.Stop()
                        BenchTime = BenchTimer.ElapsedMilliseconds
                        MsgBox($"Average: {BenchTime / 50}ms")
                    Else
                        Await Task.Run(Sub() DDSEncoder.Save(SFD.FileName))
                    End If
                End Using
                ExportDDSButton.Enabled = True
            End If
        End Using
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

    Private Sub SelectFirstItem(SourceControl As ComboBox)
        If SourceControl.Items.Count > 0 Then
            SourceControl.SelectedIndex = 0
        End If
    End Sub

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

End Class
