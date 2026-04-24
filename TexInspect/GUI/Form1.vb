Imports System.IO
Imports System.Text
Imports System.Numerics
Imports System.Drawing.Imaging

Public Class Form1

    Private FilePath As String
    Private TempPath As String
    Private FilePaths() As String
    Private PreviewImage As Image

    Private CubeMode As Boolean = False
    Private IsDragging As Boolean = False
    Private IsZooming As Boolean = False
    Private LastMousePos As Point
    Private CubeVerts(7) As Vector3
    Private CubeFaces(5) As CubeFace
    Private CubeScale As Single = 100.0F
    Private CubeOrientation As Quaternion = Quaternion.Identity

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
        InitCubeVertices()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        If TempPath IsNot Nothing AndAlso Directory.Exists(TempPath) Then
            Directory.Delete(TempPath, True)
        End If
    End Sub

    Public Sub UpdateOverrideFormats(sender As Object, e As EventArgs) Handles CompressionCheckBox.CheckedChanged, SmoothAlphaRB.CheckedChanged, SharpAlphaRB.CheckedChanged, NoAlphaRB.CheckedChanged, ExtendedHeaderCheckBox.CheckedChanged, NormalCheckBox.CheckedChanged
        Dim IsDX10 As Boolean = ExtendedHeaderCheckBox.Checked
        Dim AlphaMode As Integer = GetAlphaMode()
        OverrideComboBox.Items.Clear()
        PopulateOverrideFormats(IsDX10, AlphaMode)
        SelectFirstItem(OverrideComboBox)
        OverrideComboBox.Enabled = (OverrideComboBox.Items.Count > 1)
    End Sub

    Private Async Sub LoadImageButton_Click(sender As Object, e As EventArgs) Handles LoadImageButton.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp;*.dds"}
            If OFD.ShowDialog() = DialogResult.OK Then
                Await ProcessLoadedFileAsync(OFD.FileName)
            End If
        End Using
    End Sub

    Private Async Function ProcessLoadedFileAsync(TargetFilePath As String) As Task
        ResetUIAndState()
        FilePath = TargetFilePath
        Dim Extension As String = Path.GetExtension(FilePath).ToLower()
        If Extension = ".dds" Then
            Await LoadDDSFileAsync(FilePath)
        Else
            LoadStandardImage(FilePath)
        End If
        UpdatePreviewState()
    End Function

    Private Sub ResetUIAndState()
        DisposeCubeFaces()
        InfoTextBox.Clear()
        If PreviewImage IsNot Nothing Then PreviewImage.Dispose()
        FilePaths = Nothing
        CubeMode = False
        If TempPath IsNot Nothing AndAlso Directory.Exists(TempPath) Then Directory.Delete(TempPath, True)
        GC.Collect()
    End Sub

    Private Async Function LoadDDSFileAsync(Path As String) As Task
        Try
            Using DDSDecoder As New DDS_Decoder(Path)
                InfoTextBox.Text = GetDDSReport(DDSDecoder)
                If DDSDecoder.IsCubeMap Then
                    CubeMode = True
                    Dim TempCubeMaps As Bitmap() = Await Task.Run(Function() DDSDecoder.ToCubeBitmaps())
                    LoadCubeMaps(CubeFaces, TempCubeMaps)
                Else
                    PreviewImage = Await Task.Run(Function() DDSDecoder.ToBitmap())
                End If
            End Using
            ToggleExportButtons(isDDS:=True)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ToggleExportButtons(isDDS:=False, isError:=True)
            PreviewImage = New Bitmap(1, 1)
        End Try
    End Function

    Private Sub LoadStandardImage(Path As String)
        Dim DetectedFaces As String() = DetectCubeFiles(Path)
        If DetectCompositeCube(Path) = True Then
            If MessageBox.Show("Potential CubeMap detected.  Slice and load?", "CubeMap Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Information) = DialogResult.Yes Then
                Using Slicer As New CubeSlicer(Path)
                    TempPath = $"{IO.Path.GetTempPath}TexTemp\"
                    Directory.CreateDirectory(TempPath)
                    Slicer.SaveBitmaps(TempPath)
                    DetectedFaces = New String(5) {}
                    For i As Integer = 0 To 5
                        Dim FacePath As String = IO.Path.Combine($"{TempPath}Face{CubeSuffixes(i)}.png")
                        If File.Exists(FacePath) Then
                            DetectedFaces(i) = FacePath
                        End If
                    Next
                End Using
            End If
        End If
        If DetectedFaces IsNot Nothing Then
            CubeMode = True
            FilePaths = DetectedFaces
            Dim TempCubeMaps(5) As Bitmap
            For i As Integer = 0 To 5
                Using stream As New FileStream(DetectedFaces(i), IO.FileMode.Open, IO.FileAccess.Read)
                    Using tempImg = Image.FromStream(stream)
                        TempCubeMaps(i) = New Bitmap(tempImg)
                    End Using
                End Using
            Next
            LoadCubeMaps(CubeFaces, TempCubeMaps)
            InfoTextBox.Text = GetImageReport(TempCubeMaps(0))
        Else
            Using TempImage As Image = Image.FromFile(Path)
                InfoTextBox.Text = GetImageReport(TempImage)
                PreviewImage = New Bitmap(TempImage)
            End Using
        End If
        ToggleExportButtons(isDDS:=False)
    End Sub

    Private Sub ToggleExportButtons(isDDS As Boolean, Optional isError As Boolean = False)
        If isError Then
            DDSExportGroup.Enabled = False
            ImageExportGroup.Enabled = False
            Return
        End If
        DDSExportGroup.Enabled = Not isDDS
        ImageExportGroup.Enabled = isDDS
    End Sub

    Private Sub UpdatePreviewState()
        PreviewPictureBox.Image = If(CubeMode, Nothing, PreviewImage)
        Me.Refresh()
    End Sub

    Private Sub ExportImageButton_Click(sender As Object, e As EventArgs) Handles ExportImageButton.Click
        Dim FileExt As String = OutputFormatComboBox.SelectedItem.ToString()
        Dim Filter As String = $"{FileExt} Files|*.{FileExt.ToLower()}|All Files|*.*"
        Using SFD As New SaveFileDialog With {.Filter = Filter, .FileName = Path.GetFileNameWithoutExtension(FilePath)}
            If SFD.ShowDialog() = DialogResult.OK Then
                ToggleBusyState(True)
                Dim ImgFormat As ImageFormat = GetImageFormatFromExtension(FileExt)
                If CubeMode Then
                    For i = 0 To 5
                        CubeFaces(i).Image.Save(SFD.FileName.Substring(0, SFD.FileName.Length - 4) & CubeSuffixes(i) & "." & FileExt.ToLower(), ImgFormat)
                    Next
                Else
                    PreviewImage.Save(SFD.FileName, ImgFormat)
                End If
                ToggleBusyState(False)
            End If
        End Using
    End Sub

    Private Async Sub ExportDDSButton_Click(sender As Object, e As EventArgs) Handles ExportDDSButton.Click
        If OverrideComboBox.SelectedItem Is Nothing Then Return
        Dim targetFormat As DXGI_Format = GetFormatFromString(OverrideComboBox.SelectedItem.ToString())
        Dim isLegacy As Boolean = Not ExtendedHeaderCheckBox.Checked
        Dim doMipMaps As Boolean = MipMapCheckBox.Checked
        Using SFD As New SaveFileDialog With {.Filter = "DDS Files|*.dds|All Files|*.*", .FileName = Path.GetFileNameWithoutExtension(FilePath)}
            If SFD.ShowDialog() = DialogResult.OK Then
                ToggleBusyState(True)
                If CubeMode AndAlso FilePaths IsNot Nothing Then
                    Using DDSEncoder As New DDS_Encoder(FilePaths, targetFormat, doMipMaps, isLegacy)
                        Await Task.Run(Sub() DDSEncoder.Save(SFD.FileName))
                    End Using
                Else
                    Using DDSEncoder As New DDS_Encoder(FilePath, targetFormat, doMipMaps, isLegacy)
                        Await Task.Run(Sub() DDSEncoder.Save(SFD.FileName))
                    End Using
                End If
                ToggleBusyState(False)
                If TempPath IsNot Nothing AndAlso Directory.Exists(TempPath) Then
                    Directory.Delete(TempPath, True)
                End If
            End If
        End Using
    End Sub

    Private Sub EncBenchButton_Click(sender As Object, e As EventArgs) Handles EncBenchButton.Click
        RunBenchmark("Image Files|*.png;*.jpg;*.bmp", Sub(FileName)
                                                          Dim targetFormat As DXGI_Format = GetFormatFromString(OverrideComboBox.SelectedItem.ToString())
                                                          Using Encoder As New DDS_Encoder(FileName, targetFormat, MipMapCheckBox.Checked, Not ExtendedHeaderCheckBox.Checked)
                                                              Encoder.BeginEncode()
                                                          End Using
                                                      End Sub)
    End Sub

    Private Sub DecBenchButton_Click(sender As Object, e As EventArgs) Handles DecBenchButton.Click
        RunBenchmark("Image Files|*.dds", Sub(FileName)
                                              Using Decoder As New DDS_Decoder(FileName)
                                                  Decoder.BeginDecode()
                                              End Using
                                          End Sub)
    End Sub

    Private Sub RunBenchmark(Filter As String, BenchAction As Action(Of String))
        Using OFD As New OpenFileDialog With {.Filter = Filter}
            If OFD.ShowDialog() = DialogResult.OK Then
                Dim BenchTimer As Stopwatch = Stopwatch.StartNew()
                For i = 0 To 49
                    BenchAction(OFD.FileName)
                Next
                BenchTimer.Stop()
                MsgBox($"Average: {BenchTimer.ElapsedMilliseconds / 50} ms")
            End If
        End Using
    End Sub

    Private Sub CalcMetricsButton_Click(sender As Object, e As EventArgs) Handles CalcMetricsButton.Click
        If PreviewImage Is Nothing Then Return
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp;*.dds"}
            If OFD.ShowDialog() = DialogResult.OK Then
                Dim QualityReport As String = ""
                Using TempImage As Bitmap = LoadBitmapForMetrics(OFD.FileName)
                    Using QualityTest As New ImageMetrics(PreviewImage, TempImage)
                        QualityTest.CalcAll()
                        QualityReport = $"MSE: {Math.Round(QualityTest.MSE.Average, 4)} | PSNR: {Math.Round(QualityTest.PSNR.Average, 4)} | SSIM: {Math.Round(QualityTest.SSIM.Average, 4)}"
                    End Using
                End Using
                MessageBox.Show(QualityReport, "Report", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Using
    End Sub

    Private Function LoadBitmapForMetrics(TargetFilePath As String) As Bitmap
        If Path.GetExtension(TargetFilePath).ToLower() = ".dds" Then
            Using DDSDecoder As New DDS_Decoder(TargetFilePath)
                Return DDSDecoder.ToBitmap()
            End Using
        End If
        Return CType(Image.FromFile(TargetFilePath), Bitmap)
    End Function

    Private Sub PreviewPictureBox_MouseDown(sender As Object, e As MouseEventArgs) Handles PreviewPictureBox.MouseDown
        If e.Button = MouseButtons.Left Then
            IsDragging = True
            LastMousePos = e.Location
        ElseIf e.Button = MouseButtons.Middle Then
            CubeScale = 100.0F
            CubeOrientation = Quaternion.Identity
            PreviewPictureBox.Invalidate()
        ElseIf e.Button = MouseButtons.Right Then
            IsZooming = True
            LastMousePos = e.Location
        End If
    End Sub

    Private Sub PreviewPictureBox_MouseUp(sender As Object, e As MouseEventArgs) Handles PreviewPictureBox.MouseUp
        IsDragging = False
        IsZooming = False
    End Sub

    Private Sub PreviewPictureBox_MouseMove(sender As Object, e As MouseEventArgs) Handles PreviewPictureBox.MouseMove
        If e.Button = MouseButtons.Left Then
            Dim DeltaX As Single = (e.X - LastMousePos.X) * 0.01F
            Dim DeltaY As Single = (e.Y - LastMousePos.Y) * 0.01F
            Dim RotX As Quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, DeltaY)
            Dim RotY As Quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, DeltaX)
            CubeOrientation = Quaternion.Concatenate(CubeOrientation, RotX)
            CubeOrientation = Quaternion.Concatenate(CubeOrientation, RotY)
            LastMousePos = e.Location
            PreviewPictureBox.Invalidate()
        ElseIf e.Button = MouseButtons.Right Then
            CubeScale += (((e.X - e.Y) - (LastMousePos.X - LastMousePos.Y)) * 0.25F)
            LastMousePos = e.Location
            PreviewPictureBox.Invalidate()
        Else
            LastMousePos = e.Location
        End If
    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PreviewPictureBox.Paint
        If Not CubeMode Then Return
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        e.Graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
        RenderCube(e.Graphics)
    End Sub

    Private Sub RenderCube(g As Graphics)
        Dim RotationMatrix As Matrix4x4 = Matrix4x4.CreateFromQuaternion(CubeOrientation)
        Dim LightDir As Vector3 = Vector3.Normalize(New Vector3(-0.5F, 0.5F, 1.0F))
        Dim OffsetX As Single = PreviewPictureBox.Width / 2.0F
        Dim OffsetY As Single = PreviewPictureBox.Height / 2.0F
        Dim PreviewPoints(7) As PointF
        For i As Integer = 0 To 7
            Dim Rotation As Vector3 = Vector3.Transform(CubeVerts(i), RotationMatrix)
            PreviewPoints(i) = New PointF(OffsetX + (Rotation.X * CubeScale), OffsetY - (Rotation.Y * CubeScale))
        Next
        For Each Face In CubeFaces
            Dim RotationNormal As Vector3 = Vector3.Transform(Face.Normal, RotationMatrix)
            If RotationNormal.Z > 0 Then
                DrawFace(g, Face, PreviewPoints, RotationNormal, LightDir)
            End If
        Next
    End Sub

    Private Sub DrawFace(g As Graphics, Face As CubeFace, PreviewPoints() As PointF, RotationNormal As Vector3, LightDir As Vector3)
        Dim DestPoints() As PointF = {PreviewPoints(Face.V0), PreviewPoints(Face.V1), PreviewPoints(Face.V3)}
        Dim BottomRight As New PointF(DestPoints(1).X + DestPoints(2).X - DestPoints(0).X, DestPoints(1).Y + DestPoints(2).Y - DestPoints(0).Y)
        Dim CenterX As Single = (DestPoints(0).X + BottomRight.X) / 2.0F
        Dim CenterY As Single = (DestPoints(0).Y + BottomRight.Y) / 2.0F
        Dim Overlap As Single = 0.5F
        For DestPoint As Integer = 0 To 2
            Dim dX As Single = DestPoints(DestPoint).X - CenterX
            Dim dY As Single = DestPoints(DestPoint).Y - CenterY
            Dim Len As Single = CSng(Math.Sqrt(dX * dX + dY * dY))
            If Len > 0 Then
                DestPoints(DestPoint).X += (dX / Len) * Overlap
                DestPoints(DestPoint).Y += (dY / Len) * Overlap
            End If
        Next
        Dim FaceNormal As Vector3 = Vector3.Normalize(RotationNormal)
        Dim DotPrd As Single = Vector3.Dot(FaceNormal, LightDir)
        Dim Brightness As Single = 0.3F + (Math.Max(0, DotPrd) * 0.7F)
        Dim ShadingMatrix As New ColorMatrix(New Single()() {
            New Single() {Brightness, 0, 0, 0, 0},
            New Single() {0, Brightness, 0, 0, 0},
            New Single() {0, 0, Brightness, 0, 0},
            New Single() {0, 0, 0, 1, 0},
            New Single() {0, 0, 0, 0, 1}})
        Using ImgAttr As New ImageAttributes()
            ImgAttr.SetWrapMode(Drawing2D.WrapMode.TileFlipXY)
            ImgAttr.SetColorMatrix(ShadingMatrix)
            Dim Rect As New Rectangle(0, 0, Face.Image.Width, Face.Image.Height)
            g.DrawImage(Face.Image, DestPoints, Rect, GraphicsUnit.Pixel, ImgAttr)
        End Using
    End Sub

    Private Sub InitCubeVertices()
        CubeVerts(0) = New Vector3(-1, 1, 1)
        CubeVerts(1) = New Vector3(1, 1, 1)
        CubeVerts(2) = New Vector3(1, -1, 1)
        CubeVerts(3) = New Vector3(-1, -1, 1)
        CubeVerts(4) = New Vector3(-1, 1, -1)
        CubeVerts(5) = New Vector3(1, 1, -1)
        CubeVerts(6) = New Vector3(1, -1, -1)
        CubeVerts(7) = New Vector3(-1, -1, -1)
    End Sub

    Private Sub LoadCubeMaps(CubeFaces As CubeFace(), CubeMapImages As Bitmap())
        CubeFaces(0) = New CubeFace With {.V0 = 1, .V1 = 5, .V2 = 6, .V3 = 2, .Normal = New Vector3(1, 0, 0), .Image = CubeMapImages(0)}
        CubeFaces(1) = New CubeFace With {.V0 = 4, .V1 = 0, .V2 = 3, .V3 = 7, .Normal = New Vector3(-1, 0, 0), .Image = CubeMapImages(1)}
        CubeFaces(2) = New CubeFace With {.V0 = 4, .V1 = 5, .V2 = 1, .V3 = 0, .Normal = New Vector3(0, 1, 0), .Image = CubeMapImages(2)}
        CubeFaces(3) = New CubeFace With {.V0 = 3, .V1 = 2, .V2 = 6, .V3 = 7, .Normal = New Vector3(0, -1, 0), .Image = CubeMapImages(3)}
        CubeFaces(4) = New CubeFace With {.V0 = 0, .V1 = 1, .V2 = 2, .V3 = 3, .Normal = New Vector3(0, 0, 1), .Image = CubeMapImages(4)}
        CubeFaces(5) = New CubeFace With {.V0 = 5, .V1 = 4, .V2 = 7, .V3 = 6, .Normal = New Vector3(0, 0, -1), .Image = CubeMapImages(5)}
    End Sub

    Private Function GetImageFormatFromExtension(Ext As String) As ImageFormat
        Select Case Ext.ToUpper()
            Case "PNG" : Return ImageFormat.Png
            Case "JPG", "JPEG" : Return ImageFormat.Jpeg
            Case "BMP" : Return ImageFormat.Bmp
            Case Else : Return ImageFormat.Png
        End Select
    End Function

    Private Function GetAlphaMode() As Integer
        If NoAlphaRB.Checked Then Return 0
        If SharpAlphaRB.Checked Then Return 1
        If SmoothAlphaRB.Checked Then Return 2
        Return 0
    End Function

    Private Sub PopulateOverrideFormats(IsDX10 As Boolean, AlphaMode As Integer)
        If NormalCheckBox.Checked Then
            If CompressionCheckBox.Checked Then
                OverrideComboBox.Items.Add(If(IsDX10, "BC5 UNORM", "ATI2 (BC5)"))
            Else
                OverrideComboBox.Items.Add("BGRX (B8G8R8X8)")
            End If
        ElseIf CompressionCheckBox.Checked Then
            Select Case AlphaMode
                Case 0
                    If IsDX10 Then OverrideComboBox.Items.Add("BC7 sRGB")
                    OverrideComboBox.Items.Add(If(IsDX10, "BC1 sRGB", "DXT1"))
                    OverrideComboBox.Items.Add(If(IsDX10, "BC4 UNORM", "ATI1 (BC4)"))
                Case 1
                    OverrideComboBox.Items.Add(If(IsDX10, "BC1 sRGB", "DXT1"))
                    OverrideComboBox.Items.Add(If(IsDX10, "BC2 sRGB", "DXT3"))
                Case 2
                    If IsDX10 Then OverrideComboBox.Items.Add("BC7 sRGB")
                    OverrideComboBox.Items.Add(If(IsDX10, "BC3 sRGB", "DXT5"))
            End Select
        Else
            Select Case AlphaMode
                Case 0 : OverrideComboBox.Items.Add("BGRX (B8G8R8X8)")
                Case 1, 2 : OverrideComboBox.Items.Add("BGRA (B8G8R8A8)")
            End Select
        End If
    End Sub

    Private Function GetFormatFromString(FormatName As String) As DXGI_Format
        Select Case FormatName
            Case "BC1 sRGB", "DXT1" : Return DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB
            Case "BC2 sRGB", "DXT3" : Return DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB
            Case "BC3 sRGB", "DXT5" : Return DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB
            Case "BC4 UNORM", "ATI1 (BC4)" : Return DXGI_Format.DXGI_FORMAT_BC4_UNORM
            Case "BC5 UNORM", "ATI2 (BC5)" : Return DXGI_Format.DXGI_FORMAT_BC5_UNORM
            Case "BC7 sRGB" : Return DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB
            Case "BGRX (B8G8R8X8)" : Return DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB
            Case "BGRA (B8G8R8A8)" : Return DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB
            Case Else : Throw New Exception($"Unsupported format: {FormatName}")
        End Select
    End Function

    Public Function DetectCubeFiles(SourceFilePath As String) As String()
        Dim Directory As String = Path.GetDirectoryName(SourceFilePath)
        Dim FileNameWithoutExt As String = Path.GetFileNameWithoutExtension(SourceFilePath)
        Dim Extension As String = Path.GetExtension(SourceFilePath)
        If FileNameWithoutExt.Length < 3 Then Return Nothing
        Dim BaseName As String = ""
        Dim IsCubeFace As Boolean = False
        For Each suffix As String In CubeSuffixes
            If FileNameWithoutExt.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then
                BaseName = FileNameWithoutExt.Substring(0, FileNameWithoutExt.Length - 3)
                IsCubeFace = True
                Exit For
            End If
        Next
        If Not IsCubeFace Then Return Nothing
        Dim DetectedFaces(5) As String
        For i As Integer = 0 To 5
            Dim FacePath As String = Path.Combine(Directory, BaseName & CubeSuffixes(i) & Extension)
            If File.Exists(FacePath) Then
                DetectedFaces(i) = FacePath
            Else
                Return Nothing
            End If
        Next
        If MessageBox.Show("CubeMap Detected.  Load all faces?", "CubeMap Detected", MessageBoxButtons.YesNo) = DialogResult.Yes Then
            Return DetectedFaces
        Else
            Return Nothing
        End If
    End Function

    Private Function DetectCompositeCube(Source As String) As Boolean
        Dim Result As Boolean = False
        Dim TempWidth As Long, TempHeight As Long
        Using fs As New FileStream(Source, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using TempImage = Image.FromStream(fs, False, False)
                TempWidth = TempImage.Width
                TempHeight = TempImage.Height
            End Using
        End Using
        If (TempWidth * 6 = TempHeight) OrElse (TempHeight * 6 = TempWidth) Then Return True
        If (TempWidth * 4 = TempHeight * 3) OrElse (TempWidth * 3 = TempHeight * 4) Then Return True
        Return False
    End Function

    Private Function GetBounds(Source As String) As Integer()
        Using fs As New FileStream(Source, FileMode.Open, FileAccess.Read)
            Using TempImage = Image.FromStream(fs, False, False)
                Return {TempImage.Width, TempImage.Height}
            End Using
        End Using
    End Function

    Private Sub ToggleBusyState(IsBusy As Boolean)
        LoadImageButton.Enabled = Not IsBusy
        ExportImageButton.Enabled = Not IsBusy
        ExportDDSButton.Enabled = Not IsBusy
    End Sub

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

    Private Function GetImageFormatName(SourceFormat As ImageFormat) As String
        If SourceFormat.Guid = ImageFormat.Png.Guid Then Return "PNG"
        If SourceFormat.Guid = ImageFormat.Jpeg.Guid Then Return "JPEG"
        If SourceFormat.Guid = ImageFormat.Bmp.Guid Then Return "BMP"
        If SourceFormat.Guid = ImageFormat.Tiff.Guid Then Return "TIFF"
        If SourceFormat.Guid = ImageFormat.Gif.Guid Then Return "GIF"
        If SourceFormat.Guid = ImageFormat.Icon.Guid Then Return "ICO"
        If SourceFormat.Guid = ImageFormat.MemoryBmp.Guid Then Return "Memory Bitmap"
        Return "Unknown/Custom Format"
    End Function

    Private Function GetDDSReport(Source As DDS_Decoder) As String
        Dim ReportBuilder As New StringBuilder()
        ReportBuilder.AppendLine("===== Info =====")
        ReportBuilder.AppendLine()
        ReportBuilder.AppendLine("[Core Properties]")
        ReportBuilder.AppendLine($"Signature: {Source.Signature}")
        ReportBuilder.AppendLine($"Resolution: {Source.Width} x {Source.Height}")
        If Source.Depth > 0 Then ReportBuilder.AppendLine($"Depth: {Source.Depth}")
        ReportBuilder.AppendLine($"MipMap Count: {Source.MipMapCount}")
        ReportBuilder.AppendLine($"Pitch/Linear Size: {Source.PitchLinearSize} bytes")
        ReportBuilder.AppendLine($"Extended Header: {Source.ExtendedHeader}")
        ReportBuilder.AppendLine()
        ReportBuilder.AppendLine("[Pixel Format]")
        ReportBuilder.AppendLine($"Header Size: {Source.HeaderSize} bytes")
        ReportBuilder.AppendLine($"Sub-Header Size: {Source.SubHeaderSize} bytes")
        If Not Source.FourCC.Contains(vbNullChar) Then ReportBuilder.AppendLine($"FourCC: {Source.FourCC}")
        ReportBuilder.AppendLine($"RGB Bit Count: {Source.RGBBitCount}")
        ReportBuilder.AppendLine($"Red Bit Mask: 0x{Source.RedBitMask:X8}")
        ReportBuilder.AppendLine($"Green Bit Mask: 0x{Source.GreenBitMask:X8}")
        ReportBuilder.AppendLine($"Blue Bit Mask: 0x{Source.BlueBitMask:X8}")
        ReportBuilder.AppendLine($"Alpha Bit Mask: 0x{Source.AlphaBitMask:X8}")
        ReportBuilder.AppendLine()
        ReportBuilder.AppendLine("[Surface & Capabilities]")
        ReportBuilder.AppendLine($"Surface Flags: {Source.SurfaceFlags}")
        ReportBuilder.AppendLine($"Pixel Flags: {Source.PixelFlags}")
        ReportBuilder.AppendLine($"Caps 1: {Source.Caps1}")
        ReportBuilder.AppendLine($"Caps 2: {Source.Caps2}")
        If Source.ExtendedHeader Then
            ReportBuilder.AppendLine()
            ReportBuilder.AppendLine("[DX10 Extended Header]")
            ReportBuilder.AppendLine($"DXGI Format: {Source.DXGIFormat}")
            ReportBuilder.AppendLine($"Dimension: {Source.ResourceDimension}")
            ReportBuilder.AppendLine($"Array Size: {Source.ArraySize}")
            ReportBuilder.AppendLine($"Misc Flag 1: {Source.MiscFlag}")
            ReportBuilder.AppendLine($"Misc Flag 2: {Source.MiscFlags2}")
        End If
        Return ReportBuilder.ToString()
    End Function

    Private Function GetImageReport(Source As Image) As String
        Dim ReportBuilder As New StringBuilder()
        Dim BitsPerPixel As Integer = Image.GetPixelFormatSize(Source.PixelFormat)
        Dim UncompressedBytes As Long = CLng(Source.Width) * Source.Height * (BitsPerPixel \ 8)
        ReportBuilder.AppendLine("===== Info =====")
        ReportBuilder.AppendLine()
        ReportBuilder.AppendLine("[Core Properties]")
        ReportBuilder.AppendLine($"Resolution: {Source.Width} x {Source.Height}")
        ReportBuilder.AppendLine($"Source Format: {GetImageFormatName(Source.RawFormat)}")
        ReportBuilder.AppendLine($"DPI (Print Res): {Math.Round(Source.HorizontalResolution)} x {Math.Round(Source.VerticalResolution)} DPI")
        ReportBuilder.AppendLine()
        ReportBuilder.AppendLine("[Pixel Format]")
        ReportBuilder.AppendLine($"Format Layout: {Source.PixelFormat}")
        ReportBuilder.AppendLine($"Has Alpha Channel: {If(Image.IsAlphaPixelFormat(Source.PixelFormat), "Yes", "No")}")
        ReportBuilder.AppendLine($"Bit Depth: {BitsPerPixel} bits per pixel")
        ReportBuilder.AppendLine($"Uncompressed Size: {UncompressedBytes:N0} bytes")
        ReportBuilder.AppendLine()
        If FilePaths IsNot Nothing Then
            ReportBuilder.AppendLine($"[Extra Info]")
            ReportBuilder.AppendLine($"CubeMap Detected")
        End If
        Return ReportBuilder.ToString()
    End Function

End Class