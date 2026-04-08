Public Class Form1

    'Dim BenchmarkTime As Integer = 0
    'Dim BenchmarkTimer As Stopwatch

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        OutputFormatComboBox.SelectedIndex = 0



    End Sub

    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
            If OFD.ShowDialog = DialogResult.OK Then
                Dim AlphaMode As Integer = 0
                If NoAlphaRB.Checked = True Then AlphaMode = 0
                If OneBitAlphaRB.Checked = True Then AlphaMode = 1
                If EightBitAlphaRB.Checked = True Then AlphaMode = 2
                Using DDSTestImage As New DDS_Encoder(OFD.FileName, AlphaMode, CompressionCheckBox.Checked, MipMapCheckBox.Checked, ExtendedHeaderCheckBox.Checked)
                    Using SFD As New SaveFileDialog With {.Filter = "DDS Files|*.dds|All Files|*.*"}
                        If SFD.ShowDialog = DialogResult.OK Then
                            Button1.Enabled = False
                            Await Task.Run(Sub() DDSTestImage.SaveImage(SFD.FileName))
                            Button1.Enabled = True
                        End If
                    End Using
                End Using
            End If
        End Using
    End Sub

    Private Async Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Using OFD As New OpenFileDialog With {.Filter = "DDS Files|*.dds"}
            If OFD.ShowDialog = DialogResult.OK Then
                Using DecodeTest As New DDS_Decoder(OFD.FileName)
                    Dim FileExt As String = OutputFormatComboBox.SelectedItem.ToString
                    Using SFD As New SaveFileDialog With {.Filter = $"{FileExt} Files|*.{FileExt.ToLower}|All Files|*.*"}
                        If SFD.ShowDialog = DialogResult.OK Then
                            Button2.Enabled = False
                            Select Case FileExt
                                Case "PNG"
                                    Await Task.Run(Sub() DecodeTest.SaveImage(SFD.FileName, Imaging.ImageFormat.Png))
                                Case "JPG"
                                    Await Task.Run(Sub() DecodeTest.SaveImage(SFD.FileName, Imaging.ImageFormat.Jpeg))
                                Case "BMP"
                                    Await Task.Run(Sub() DecodeTest.SaveImage(SFD.FileName, Imaging.ImageFormat.Bmp))
                            End Select
                            Button2.Enabled = True
                        End If
                    End Using
                End Using
            End If
        End Using
    End Sub

    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
    '    Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
    '        If OFD.ShowDialog = DialogResult.OK Then
    '            Dim AlphaMode As Integer = 0
    '            If NoAlphaRB.Checked = True Then AlphaMode = 0
    '            If OneBitAlphaRB.Checked = True Then AlphaMode = 1
    '            If EightBitAlphaRB.Checked = True Then AlphaMode = 2
    '            BenchmarkTime = 0
    '            BenchmarkTimer = Stopwatch.StartNew
    '            For i = 0 To 49
    '                Using DDSTestImage As New DDS_Encoder(OFD.FileName, AlphaMode, CompressionCheckBox.Checked, MipMapCheckBox.Checked, ExtendedHeaderCheckBox.Checked)
    '                End Using
    '            Next
    '            BenchmarkTimer.Stop()
    '            BenchmarkTime = BenchmarkTimer.ElapsedMilliseconds
    '            MsgBox($"Average: {BenchmarkTime / 50}ms")
    '        End If
    '    End Using

    'End Sub

    'Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
    '    Using OFD As New OpenFileDialog With {.Filter = "DDS Files|*.dds"}
    '        If OFD.ShowDialog = DialogResult.OK Then
    '            BenchmarkTime = 0
    '            BenchmarkTimer = Stopwatch.StartNew
    '            For i = 0 To 49
    '                Using DecodeTest As New DDS_Decoder(OFD.FileName)
    '                End Using
    '            Next
    '            BenchmarkTimer.Stop()
    '            BenchmarkTime = BenchmarkTimer.ElapsedMilliseconds
    '            MsgBox($"Average: {BenchmarkTime / 50}ms")
    '        End If
    '    End Using
    'End Sub

End Class
