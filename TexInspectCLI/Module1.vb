Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.Drawing.Imaging

Public Module Module1

    Private Class CliOptions
        Public InputPath As String = ""
        Public OutputPath As String = ""
        Public Format As DXGI_Format = DXGI_Format.DXGI_FORMAT_BC7_UNORM
        Public GenerateMips As Boolean = False
        Public UseLegacyHeader As Boolean = False
        Public ShowInfo As Boolean = False
        Public ForceOverwrite As Boolean = False
        Public RecursiveSearch As Boolean = False
        Public TargetImgExt As String = ".png"
    End Class

    Private ReadOnly ImgExts As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {".png", ".jpg", ".jpeg", ".bmp", ".tga"}
    Private ReadOnly DdsExts As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {".dds"}
    Private ReadOnly LegacyFormats As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {"DXT1", "DXT3", "DXT5", "ATI1", "ATI2"}

    Private ReadOnly FormatAliases As New Dictionary(Of String, DXGI_Format)(StringComparer.OrdinalIgnoreCase) From {
        {"BC1", DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB}, {"BC1 SRGB", DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB}, {"DXT1", DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB}, {"BC1_UNORM_SRGB", DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB},
        {"BC2", DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB}, {"BC2 SRGB", DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB}, {"DXT3", DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB}, {"BC2_UNORM_SRGB", DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB},
        {"BC3", DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB}, {"BC3 SRGB", DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB}, {"DXT5", DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB}, {"BC3_UNORM_SRGB", DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB},
        {"BC4", DXGI_Format.DXGI_FORMAT_BC4_UNORM}, {"BC4 UNORM", DXGI_Format.DXGI_FORMAT_BC4_UNORM}, {"ATI1", DXGI_Format.DXGI_FORMAT_BC4_UNORM}, {"ATI1 (BC4)", DXGI_Format.DXGI_FORMAT_BC4_UNORM}, {"BC4_UNORM", DXGI_Format.DXGI_FORMAT_BC4_UNORM},
        {"BC5", DXGI_Format.DXGI_FORMAT_BC5_UNORM}, {"BC5 UNORM", DXGI_Format.DXGI_FORMAT_BC5_UNORM}, {"ATI2", DXGI_Format.DXGI_FORMAT_BC5_UNORM}, {"ATI2 (BC5)", DXGI_Format.DXGI_FORMAT_BC5_UNORM}, {"BC5_UNORM", DXGI_Format.DXGI_FORMAT_BC5_UNORM},
        {"BC7", DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB}, {"BC7 SRGB", DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB}, {"BC7_UNORM_SRGB", DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB},
        {"BGRX", DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB}, {"BGRX (B8G8R8X8)", DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB}, {"B8G8R8X8_UNORM_SRGB", DXGI_Format.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB},
        {"BGRA", DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB}, {"BGRA (B8G8R8A8)", DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB}, {"B8G8R8A8_UNORM_SRGB", DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB}
    }

    Sub Main(args As String())
        If args.Length = 0 OrElse args.Contains("-h", StringComparer.OrdinalIgnoreCase) OrElse args.Contains("--help", StringComparer.OrdinalIgnoreCase) Then
            DisplayHelp()
            Return
        End If
        Dim CliOpts As New CliOptions()
        Try
            Dim i As Integer = 0
            While i < args.Length
                Select Case args(i).ToLower()
                    Case "-f", "--force"
                        CliOpts.ForceOverwrite = True
                    Case "-fmt"
                        CliOpts.Format = GetFormatFromString(args(i + 1))
                        i += 1
                    Case "-m"
                        CliOpts.GenerateMips = True
                    Case "-nx", "--nodx10"
                        CliOpts.UseLegacyHeader = True
                    Case "-o"
                        CliOpts.OutputPath = args(i + 1)
                        i += 1
                    Case "-r", "--recursive"
                        CliOpts.RecursiveSearch = True
                    Case "-ext", "--ext"
                        If i + 1 < args.Length Then
                            CliOpts.TargetImgExt = args(i + 1)
                            If Not CliOpts.TargetImgExt.StartsWith(".") Then CliOpts.TargetImgExt = "." & CliOpts.TargetImgExt
                            i += 1
                        End If
                    Case "--info"
                        CliOpts.ShowInfo = True
                    Case Else
                        If Not args(i).StartsWith("-") Then CliOpts.InputPath = args(i)
                End Select
                i += 1
            End While
            If LegacyFormats.Contains(CliOpts.Format) Then CliOpts.UseLegacyHeader = True
            If Directory.Exists(CliOpts.InputPath) Then
                HandleBatch(CliOpts)
            ElseIf File.Exists(CliOpts.InputPath) Then
                RunProcessor(CliOpts.InputPath, CliOpts.OutputPath, CliOpts)
            Else
                Console.WriteLine($"Error: Path '{CliOpts.InputPath}' not found.")
            End If
        Catch ex As Exception
            Console.WriteLine($"[CRITICAL ERROR] {ex.Message}")
        End Try
    End Sub

    Private Sub HandleBatch(CliOpts As CliOptions)
        If Not CliOpts.ShowInfo AndAlso String.IsNullOrEmpty(CliOpts.OutputPath) AndAlso Not CliOpts.ForceOverwrite Then
            Console.Write("Warning: No output directory defined. Files will be saved in-place. Continue? (Y/N): ")
            If Console.ReadKey().Key <> ConsoleKey.Y Then
                Console.WriteLine(vbCrLf & "Operation cancelled.")
                Return
            End If
            Console.WriteLine()
        End If
        Dim SearchOpts = If(CliOpts.RecursiveSearch, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly)
        Dim FileList = Directory.EnumerateFiles(CliOpts.InputPath, "*.*", SearchOpts)
        For Each File As String In FileList
            Dim FileExt = Path.GetExtension(File)
            Dim FilePath As String = ""
            If ImgExts.Contains(FileExt) Then
                FilePath = BuildOutputPath(File, CliOpts.InputPath, CliOpts.OutputPath, ".dds")
            ElseIf DdsExts.Contains(FileExt) Then
                FilePath = BuildOutputPath(File, CliOpts.InputPath, CliOpts.OutputPath, CliOpts.TargetImgExt)
            Else
                Continue For
            End If
            RunProcessor(File, FilePath, CliOpts)
        Next
    End Sub

    Private Sub RunProcessor(Source As String, Target As String, CliOpts As CliOptions)
        Try
            Dim SourceExt = Path.GetExtension(Source).ToLower()
            If CliOpts.ShowInfo Then
                Console.WriteLine($"--- {Path.GetFileName(Source)} ---")
                If SourceExt = ".dds" Then
                    Using Decoder As New DDS_Decoder(Source)
                        Console.WriteLine(GetDDSReport(Decoder))
                    End Using
                Else
                    Using TempImage As Image = Image.FromFile(Source)
                        Console.WriteLine(GetImageReport(TempImage))
                    End Using
                End If
                Return
            End If
            Dim TargetExt = Path.GetExtension(Target).ToLower()
            If SourceExt = ".dds" Then
                If TargetExt = ".dds" Then Target = Path.ChangeExtension(Target, ".png")
                Using Decoder As New DDS_Decoder(Source)
                    If Decoder.IsCubeMap Then
                        Dim TargetCubePath As String = Path.Combine(Path.GetDirectoryName(Target), Path.GetFileNameWithoutExtension(Target))
                        Console.WriteLine($"[INFO] {Path.GetFileName(Source)} is a CubeMap. Extracting faces...")
                        Decoder.SaveCubeMaps(TargetCubePath)
                    Else
                        Decoder.Save(Target, GetImageFormat(TargetExt))
                    End If
                End Using
            Else
                If TargetExt <> ".dds" Then Target = Path.ChangeExtension(Target, ".dds")
                Using Encoder As New DDS_Encoder(Source, CliOpts.Format, CliOpts.GenerateMips, CliOpts.UseLegacyHeader)
                    Encoder.Save(Target)
                End Using
            End If
            Console.WriteLine($"[SUCCESS] {Path.GetFileName(Source)} -> {Path.GetFileName(Target)}")
        Catch ex As Exception
            Console.WriteLine($"[FAILED] {Path.GetFileName(Source)}: {ex.Message}")
        End Try
    End Sub

    Private Function BuildOutputPath(SourceFile As String, SourceDir As String, TargetDir As String, TargetExt As String) As String
        Dim ResultPath As String
        Dim FullSourceDir As String = Path.GetFullPath(SourceDir)
        Dim FullSourcePath As String = Path.GetFullPath(SourceFile)
        If String.IsNullOrEmpty(TargetDir) Then Return Path.ChangeExtension(SourceFile, TargetExt)
        If Not FullSourceDir.EndsWith(Path.DirectorySeparatorChar) Then FullSourceDir &= Path.DirectorySeparatorChar
        If FullSourcePath.StartsWith(FullSourceDir, StringComparison.OrdinalIgnoreCase) Then
            Dim PartialPath As String = FullSourcePath.Substring(FullSourceDir.Length)
            ResultPath = Path.Combine(TargetDir, PartialPath)
        Else
            ResultPath = Path.Combine(TargetDir, Path.GetFileName(SourceFile))
        End If
        ResultPath = Path.ChangeExtension(ResultPath, TargetExt)
        Directory.CreateDirectory(Path.GetDirectoryName(ResultPath))
        Return ResultPath
    End Function

    Private Function GetFormatFromString(Source As String) As DXGI_Format
        Dim TempFormat As DXGI_Format
        If FormatAliases.TryGetValue(Source, TempFormat) Then
            Return TempFormat
        Else
            Throw New Exception($"Unsupported format: {Source}")
        End If
    End Function

    Private Function GetImageFormat(Extension As String) As ImageFormat
        Select Case Extension.ToLower()
            Case ".jpg", ".jpeg" : Return ImageFormat.Jpeg
            Case ".bmp" : Return ImageFormat.Bmp
            Case Else : Return ImageFormat.Png
        End Select
    End Function

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
        Return ReportBuilder.ToString()
    End Function

    Private Sub DisplayHelp()
        Console.WriteLine("TexInspectCLI v1.0.0")
        Console.WriteLine("Usage: TexInspectCLI.exe <input_path> [options]")
        Console.WriteLine()
        Console.WriteLine("Options:")
        Console.WriteLine("  -fmt <format>     Target Format (e.g., BC7_UNORM, DXT1, ATI2). Default: BC7_UNORM_SRGB")
        Console.WriteLine("  -m                Generate Mipmaps")
        Console.WriteLine("  -nx, --nodx10     Force legacy DDS header (Implicitly enabled for DXT/ATI formats)")
        Console.WriteLine("  -o <path>         Output file or directory")
        Console.WriteLine("  -ext <extension>  Output extension for batch decoding (e.g., .jpg, .bmp). Default: .png")
        Console.WriteLine("  -r, --recursive   Search subdirectories when processing a folder")
        Console.WriteLine("  -f, --force       Suppress warnings and overwrite files")
        Console.WriteLine("  --info            Show header info for the target file(s) without processing")
        Console.WriteLine()
        Console.WriteLine("Examples:")
        Console.WriteLine("  Encode PNG to BC7 DDS:")
        Console.WriteLine("    TexInspectCLI.exe texture.png -fmt BC7_UNORM_SRGB -m" & vbCrLf)
        Console.WriteLine("  Encode PNG to legacy DXT5 DDS:")
        Console.WriteLine("    TexInspectCLI.exe texture.png -fmt DXT5" & vbCrLf)
        Console.WriteLine("  Encode a folder of images to DDS into a specific output folder:")
        Console.WriteLine("    TexInspectCLI.exe ""C:\InputImages"" -o ""C:\OutputDDS"" -fmt BC7_UNORM_SRGB -m -f" & vbCrLf)
        Console.WriteLine("  Decode a folder of DDS files to JPEGs:")
        Console.WriteLine("    TexInspectCLI.exe ""C:\InputDDS"" -o ""C:\OutputJPEGs"" -ext .jpg -f" & vbCrLf)
        Console.WriteLine("  View header data of a specific file:")
        Console.WriteLine("    TexInspectCLI.exe texture.dds --info")
    End Sub

End Module