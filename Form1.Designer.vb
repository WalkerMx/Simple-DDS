<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.CompressionCheckBox = New System.Windows.Forms.CheckBox()
        Me.MipMapCheckBox = New System.Windows.Forms.CheckBox()
        Me.NoAlphaRB = New System.Windows.Forms.RadioButton()
        Me.OneBitAlphaRB = New System.Windows.Forms.RadioButton()
        Me.EightBitAlphaRB = New System.Windows.Forms.RadioButton()
        Me.ExtendedHeaderCheckBox = New System.Windows.Forms.CheckBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.OutputFormatComboBox = New System.Windows.Forms.ComboBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(5, 108)
        Me.Button1.Margin = New System.Windows.Forms.Padding(2)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(182, 34)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Convert"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'CompressionCheckBox
        '
        Me.CompressionCheckBox.AutoSize = True
        Me.CompressionCheckBox.Checked = True
        Me.CompressionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CompressionCheckBox.Location = New System.Drawing.Point(6, 41)
        Me.CompressionCheckBox.Margin = New System.Windows.Forms.Padding(2)
        Me.CompressionCheckBox.Name = "CompressionCheckBox"
        Me.CompressionCheckBox.Size = New System.Drawing.Size(86, 17)
        Me.CompressionCheckBox.TabIndex = 3
        Me.CompressionCheckBox.Text = "Compression"
        Me.CompressionCheckBox.UseVisualStyleBackColor = True
        '
        'MipMapCheckBox
        '
        Me.MipMapCheckBox.AutoSize = True
        Me.MipMapCheckBox.Location = New System.Drawing.Point(6, 19)
        Me.MipMapCheckBox.Name = "MipMapCheckBox"
        Me.MipMapCheckBox.Size = New System.Drawing.Size(69, 17)
        Me.MipMapCheckBox.TabIndex = 4
        Me.MipMapCheckBox.Text = "MipMaps"
        Me.MipMapCheckBox.UseVisualStyleBackColor = True
        '
        'NoAlphaRB
        '
        Me.NoAlphaRB.AutoSize = True
        Me.NoAlphaRB.Checked = True
        Me.NoAlphaRB.Location = New System.Drawing.Point(97, 17)
        Me.NoAlphaRB.Name = "NoAlphaRB"
        Me.NoAlphaRB.Size = New System.Drawing.Size(69, 17)
        Me.NoAlphaRB.TabIndex = 5
        Me.NoAlphaRB.TabStop = True
        Me.NoAlphaRB.Text = "No Alpha"
        Me.NoAlphaRB.UseVisualStyleBackColor = True
        '
        'OneBitAlphaRB
        '
        Me.OneBitAlphaRB.AutoSize = True
        Me.OneBitAlphaRB.Location = New System.Drawing.Point(97, 40)
        Me.OneBitAlphaRB.Name = "OneBitAlphaRB"
        Me.OneBitAlphaRB.Size = New System.Drawing.Size(75, 17)
        Me.OneBitAlphaRB.TabIndex = 6
        Me.OneBitAlphaRB.Text = "1-bit Alpha"
        Me.OneBitAlphaRB.UseVisualStyleBackColor = True
        '
        'EightBitAlphaRB
        '
        Me.EightBitAlphaRB.AutoSize = True
        Me.EightBitAlphaRB.Location = New System.Drawing.Point(97, 63)
        Me.EightBitAlphaRB.Name = "EightBitAlphaRB"
        Me.EightBitAlphaRB.Size = New System.Drawing.Size(75, 17)
        Me.EightBitAlphaRB.TabIndex = 7
        Me.EightBitAlphaRB.Text = "8-bit Alpha"
        Me.EightBitAlphaRB.UseVisualStyleBackColor = True
        '
        'ExtendedHeaderCheckBox
        '
        Me.ExtendedHeaderCheckBox.AutoSize = True
        Me.ExtendedHeaderCheckBox.Checked = True
        Me.ExtendedHeaderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ExtendedHeaderCheckBox.Location = New System.Drawing.Point(6, 63)
        Me.ExtendedHeaderCheckBox.Name = "ExtendedHeaderCheckBox"
        Me.ExtendedHeaderCheckBox.Size = New System.Drawing.Size(53, 17)
        Me.ExtendedHeaderCheckBox.TabIndex = 8
        Me.ExtendedHeaderCheckBox.Text = "DX10"
        Me.ExtendedHeaderCheckBox.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.MipMapCheckBox)
        Me.GroupBox1.Controls.Add(Me.Button1)
        Me.GroupBox1.Controls.Add(Me.CompressionCheckBox)
        Me.GroupBox1.Controls.Add(Me.ExtendedHeaderCheckBox)
        Me.GroupBox1.Controls.Add(Me.NoAlphaRB)
        Me.GroupBox1.Controls.Add(Me.EightBitAlphaRB)
        Me.GroupBox1.Controls.Add(Me.OneBitAlphaRB)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(194, 148)
        Me.GroupBox1.TabIndex = 10
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Image to DDS"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.OutputFormatComboBox)
        Me.GroupBox2.Controls.Add(Me.Button2)
        Me.GroupBox2.Location = New System.Drawing.Point(212, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(194, 148)
        Me.GroupBox2.TabIndex = 11
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "DDS to Image"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 21)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(77, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Output Format:"
        '
        'OutputFormatComboBox
        '
        Me.OutputFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.OutputFormatComboBox.FormattingEnabled = True
        Me.OutputFormatComboBox.Items.AddRange(New Object() {"PNG", "BMP", "JPG"})
        Me.OutputFormatComboBox.Location = New System.Drawing.Point(89, 18)
        Me.OutputFormatComboBox.Name = "OutputFormatComboBox"
        Me.OutputFormatComboBox.Size = New System.Drawing.Size(99, 21)
        Me.OutputFormatComboBox.TabIndex = 1
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(6, 108)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(182, 34)
        Me.Button2.TabIndex = 0
        Me.Button2.Text = "Convert"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(417, 169)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.GroupBox2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Margin = New System.Windows.Forms.Padding(2)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form1"
        Me.Text = "Simple DDS"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents CompressionCheckBox As CheckBox
    Friend WithEvents MipMapCheckBox As CheckBox
    Friend WithEvents NoAlphaRB As RadioButton
    Friend WithEvents OneBitAlphaRB As RadioButton
    Friend WithEvents EightBitAlphaRB As RadioButton
    Friend WithEvents ExtendedHeaderCheckBox As CheckBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents OutputFormatComboBox As ComboBox
End Class
