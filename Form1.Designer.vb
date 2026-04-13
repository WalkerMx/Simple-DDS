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
        Me.ExportDDSButton = New System.Windows.Forms.Button()
        Me.CompressionCheckBox = New System.Windows.Forms.CheckBox()
        Me.MipMapCheckBox = New System.Windows.Forms.CheckBox()
        Me.NoAlphaRB = New System.Windows.Forms.RadioButton()
        Me.SharpAlphaRB = New System.Windows.Forms.RadioButton()
        Me.SmoothAlphaRB = New System.Windows.Forms.RadioButton()
        Me.ExtendedHeaderCheckBox = New System.Windows.Forms.CheckBox()
        Me.DDSExportGroup = New System.Windows.Forms.GroupBox()
        Me.OverrideComboBox = New System.Windows.Forms.ComboBox()
        Me.NormalCheckBox = New System.Windows.Forms.CheckBox()
        Me.ImageExportGroup = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.OutputFormatComboBox = New System.Windows.Forms.ComboBox()
        Me.ExportImageButton = New System.Windows.Forms.Button()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer3 = New System.Windows.Forms.SplitContainer()
        Me.InfoTextBox = New System.Windows.Forms.TextBox()
        Me.LoadImageButton = New System.Windows.Forms.Button()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.PreviewPictureBox = New System.Windows.Forms.PictureBox()
        Me.DDSExportGroup.SuspendLayout()
        Me.ImageExportGroup.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer3.Panel1.SuspendLayout()
        Me.SplitContainer3.Panel2.SuspendLayout()
        Me.SplitContainer3.SuspendLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        CType(Me.PreviewPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ExportDDSButton
        '
        Me.ExportDDSButton.Location = New System.Drawing.Point(6, 135)
        Me.ExportDDSButton.Margin = New System.Windows.Forms.Padding(2)
        Me.ExportDDSButton.Name = "ExportDDSButton"
        Me.ExportDDSButton.Size = New System.Drawing.Size(183, 34)
        Me.ExportDDSButton.TabIndex = 0
        Me.ExportDDSButton.Text = "Convert"
        Me.ExportDDSButton.UseVisualStyleBackColor = True
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
        'SharpAlphaRB
        '
        Me.SharpAlphaRB.AutoSize = True
        Me.SharpAlphaRB.Location = New System.Drawing.Point(97, 40)
        Me.SharpAlphaRB.Name = "SharpAlphaRB"
        Me.SharpAlphaRB.Size = New System.Drawing.Size(83, 17)
        Me.SharpAlphaRB.TabIndex = 6
        Me.SharpAlphaRB.Text = "Sharp Alpha"
        Me.SharpAlphaRB.UseVisualStyleBackColor = True
        '
        'SmoothAlphaRB
        '
        Me.SmoothAlphaRB.AutoSize = True
        Me.SmoothAlphaRB.Location = New System.Drawing.Point(97, 63)
        Me.SmoothAlphaRB.Name = "SmoothAlphaRB"
        Me.SmoothAlphaRB.Size = New System.Drawing.Size(91, 17)
        Me.SmoothAlphaRB.TabIndex = 7
        Me.SmoothAlphaRB.Text = "Smooth Alpha"
        Me.SmoothAlphaRB.UseVisualStyleBackColor = True
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
        'DDSExportGroup
        '
        Me.DDSExportGroup.Controls.Add(Me.OverrideComboBox)
        Me.DDSExportGroup.Controls.Add(Me.NormalCheckBox)
        Me.DDSExportGroup.Controls.Add(Me.MipMapCheckBox)
        Me.DDSExportGroup.Controls.Add(Me.ExportDDSButton)
        Me.DDSExportGroup.Controls.Add(Me.CompressionCheckBox)
        Me.DDSExportGroup.Controls.Add(Me.ExtendedHeaderCheckBox)
        Me.DDSExportGroup.Controls.Add(Me.NoAlphaRB)
        Me.DDSExportGroup.Controls.Add(Me.SmoothAlphaRB)
        Me.DDSExportGroup.Controls.Add(Me.SharpAlphaRB)
        Me.DDSExportGroup.Enabled = False
        Me.DDSExportGroup.Location = New System.Drawing.Point(3, 3)
        Me.DDSExportGroup.Name = "DDSExportGroup"
        Me.DDSExportGroup.Size = New System.Drawing.Size(194, 175)
        Me.DDSExportGroup.TabIndex = 10
        Me.DDSExportGroup.TabStop = False
        Me.DDSExportGroup.Text = "Image to DDS"
        '
        'OverrideComboBox
        '
        Me.OverrideComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.OverrideComboBox.FormattingEnabled = True
        Me.OverrideComboBox.Location = New System.Drawing.Point(6, 109)
        Me.OverrideComboBox.Name = "OverrideComboBox"
        Me.OverrideComboBox.Size = New System.Drawing.Size(183, 21)
        Me.OverrideComboBox.TabIndex = 12
        '
        'NormalCheckBox
        '
        Me.NormalCheckBox.AutoSize = True
        Me.NormalCheckBox.Location = New System.Drawing.Point(6, 86)
        Me.NormalCheckBox.Name = "NormalCheckBox"
        Me.NormalCheckBox.Size = New System.Drawing.Size(83, 17)
        Me.NormalCheckBox.TabIndex = 9
        Me.NormalCheckBox.Text = "Normal Map"
        Me.NormalCheckBox.UseVisualStyleBackColor = True
        '
        'ImageExportGroup
        '
        Me.ImageExportGroup.Controls.Add(Me.Label1)
        Me.ImageExportGroup.Controls.Add(Me.OutputFormatComboBox)
        Me.ImageExportGroup.Controls.Add(Me.ExportImageButton)
        Me.ImageExportGroup.Enabled = False
        Me.ImageExportGroup.Location = New System.Drawing.Point(3, 184)
        Me.ImageExportGroup.Name = "ImageExportGroup"
        Me.ImageExportGroup.Size = New System.Drawing.Size(194, 85)
        Me.ImageExportGroup.TabIndex = 11
        Me.ImageExportGroup.TabStop = False
        Me.ImageExportGroup.Text = "DDS to Image"
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
        'ExportImageButton
        '
        Me.ExportImageButton.Location = New System.Drawing.Point(6, 45)
        Me.ExportImageButton.Name = "ExportImageButton"
        Me.ExportImageButton.Size = New System.Drawing.Size(182, 34)
        Me.ExportImageButton.TabIndex = 0
        Me.ExportImageButton.Text = "Convert"
        Me.ExportImageButton.UseVisualStyleBackColor = True
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.SplitContainer3)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.SplitContainer2)
        Me.SplitContainer1.Size = New System.Drawing.Size(797, 400)
        Me.SplitContainer1.SplitterDistance = 188
        Me.SplitContainer1.TabIndex = 12
        '
        'SplitContainer3
        '
        Me.SplitContainer3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer3.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer3.Name = "SplitContainer3"
        Me.SplitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer3.Panel1
        '
        Me.SplitContainer3.Panel1.Controls.Add(Me.InfoTextBox)
        '
        'SplitContainer3.Panel2
        '
        Me.SplitContainer3.Panel2.Controls.Add(Me.LoadImageButton)
        Me.SplitContainer3.Size = New System.Drawing.Size(188, 400)
        Me.SplitContainer3.SplitterDistance = 355
        Me.SplitContainer3.TabIndex = 0
        '
        'InfoTextBox
        '
        Me.InfoTextBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.InfoTextBox.Location = New System.Drawing.Point(0, 0)
        Me.InfoTextBox.Multiline = True
        Me.InfoTextBox.Name = "InfoTextBox"
        Me.InfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.InfoTextBox.Size = New System.Drawing.Size(188, 355)
        Me.InfoTextBox.TabIndex = 0
        '
        'LoadImageButton
        '
        Me.LoadImageButton.Location = New System.Drawing.Point(3, 3)
        Me.LoadImageButton.Name = "LoadImageButton"
        Me.LoadImageButton.Size = New System.Drawing.Size(182, 34)
        Me.LoadImageButton.TabIndex = 0
        Me.LoadImageButton.Text = "Load Image"
        Me.LoadImageButton.UseVisualStyleBackColor = True
        '
        'SplitContainer2
        '
        Me.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer2.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer2.Name = "SplitContainer2"
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.PreviewPictureBox)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.DDSExportGroup)
        Me.SplitContainer2.Panel2.Controls.Add(Me.ImageExportGroup)
        Me.SplitContainer2.Size = New System.Drawing.Size(605, 400)
        Me.SplitContainer2.SplitterDistance = 400
        Me.SplitContainer2.TabIndex = 0
        '
        'PreviewPictureBox
        '
        Me.PreviewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PreviewPictureBox.Location = New System.Drawing.Point(0, 0)
        Me.PreviewPictureBox.Name = "PreviewPictureBox"
        Me.PreviewPictureBox.Size = New System.Drawing.Size(400, 400)
        Me.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PreviewPictureBox.TabIndex = 0
        Me.PreviewPictureBox.TabStop = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(797, 400)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Margin = New System.Windows.Forms.Padding(2)
        Me.MinimumSize = New System.Drawing.Size(813, 439)
        Me.Name = "Form1"
        Me.Text = "Simple DDS"
        Me.DDSExportGroup.ResumeLayout(False)
        Me.DDSExportGroup.PerformLayout()
        Me.ImageExportGroup.ResumeLayout(False)
        Me.ImageExportGroup.PerformLayout()
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.SplitContainer3.Panel1.ResumeLayout(False)
        Me.SplitContainer3.Panel1.PerformLayout()
        Me.SplitContainer3.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer3.ResumeLayout(False)
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        CType(Me.PreviewPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ExportDDSButton As Button
    Friend WithEvents CompressionCheckBox As CheckBox
    Friend WithEvents MipMapCheckBox As CheckBox
    Friend WithEvents NoAlphaRB As RadioButton
    Friend WithEvents SharpAlphaRB As RadioButton
    Friend WithEvents SmoothAlphaRB As RadioButton
    Friend WithEvents ExtendedHeaderCheckBox As CheckBox
    Friend WithEvents DDSExportGroup As GroupBox
    Friend WithEvents ImageExportGroup As GroupBox
    Friend WithEvents ExportImageButton As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents OutputFormatComboBox As ComboBox
    Friend WithEvents NormalCheckBox As CheckBox
    Friend WithEvents OverrideComboBox As ComboBox
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents SplitContainer2 As SplitContainer
    Friend WithEvents SplitContainer3 As SplitContainer
    Friend WithEvents InfoTextBox As TextBox
    Friend WithEvents LoadImageButton As Button
    Friend WithEvents PreviewPictureBox As PictureBox
End Class
