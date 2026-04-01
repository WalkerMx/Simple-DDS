Imports System.Security.Cryptography

Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
            If OFD.ShowDialog = DialogResult.OK Then
                Dim AlphaMode As Integer = 0
                If NoAlphaRB.Checked = True Then AlphaMode = 0
                If OneBitAlphaRB.Checked = True Then AlphaMode = 1
                If EightBitAlphaRB.Checked = True Then AlphaMode = 2
                Using DDSTestImage As New DDS_Encoder(Image.FromFile(OFD.FileName), AlphaMode, CompressionCheckBox.Checked, MipMapCheckBox.Checked, ExtendedHeaderCheckBox.Checked, QualityCheckBox.Checked)
                    Using SFD As New SaveFileDialog With {.Filter = "DDS Files|*.dds|All Files|*.*"}
                        If SFD.ShowDialog = DialogResult.OK Then
                            DDSTestImage.SaveImage(SFD.FileName)
                        End If
                    End Using
                End Using
            End If
        End Using

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Using OFD As New OpenFileDialog With {.Filter = "DDS Files|*.dds"}
            If OFD.ShowDialog = DialogResult.OK Then
                Using DecodeTest As New DDS_Decoder(OFD.FileName)
                    Dim FileExt As String = OutputFormatComboBox.SelectedItem.ToString
                    Using SFD As New SaveFileDialog With {.Filter = $"{FileExt} Files|*.{FileExt.ToLower}|All Files|*.*"}
                        If SFD.ShowDialog = DialogResult.OK Then
                            Select Case FileExt
                                Case "PNG"
                                    DecodeTest.SaveImage(SFD.FileName, Imaging.ImageFormat.Png)
                                Case "JPG"
                                    DecodeTest.SaveImage(SFD.FileName, Imaging.ImageFormat.Jpeg)
                                Case "BMP"
                                    DecodeTest.SaveImage(SFD.FileName, Imaging.ImageFormat.Bmp)
                            End Select
                        End If
                    End Using
                End Using
            End If
        End Using
    End Sub

End Class
