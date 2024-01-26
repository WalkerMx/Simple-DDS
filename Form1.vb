Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
            If OFD.ShowDialog = DialogResult.OK Then
                Using DDSTestImage As New DDS(Image.FromFile(OFD.FileName), CheckBox1.Checked, CheckBox2.Checked, NumericUpDown1.Value)
                    Using SFD As New SaveFileDialog With {.Filter = "DDS Files|*.dds|All Files|*.*"}
                        If SFD.ShowDialog = DialogResult.OK Then
                            DDSTestImage.SaveImage(SFD.FileName)
                        End If
                    End Using
                End Using
            End If
        End Using

    End Sub

End Class
