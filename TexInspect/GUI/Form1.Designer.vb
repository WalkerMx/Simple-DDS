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
        Me.PreMultAlphaRB = New System.Windows.Forms.RadioButton()
        Me.OverrideComboBox = New System.Windows.Forms.ComboBox()
        Me.NormalCheckBox = New System.Windows.Forms.CheckBox()
        Me.ImageExportGroup = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.OutputFormatComboBox = New System.Windows.Forms.ComboBox()
        Me.ExportImageButton = New System.Windows.Forms.Button()
        Me.InfoTextBox = New System.Windows.Forms.TextBox()
        Me.LoadImageButton = New System.Windows.Forms.Button()
        Me.RightPanel = New System.Windows.Forms.Panel()
        Me.BenchGroupBox = New System.Windows.Forms.GroupBox()
        Me.CalcMetricsButton = New System.Windows.Forms.Button()
        Me.DecBenchButton = New System.Windows.Forms.Button()
        Me.EncBenchButton = New System.Windows.Forms.Button()
        Me.PreviewPictureBox = New System.Windows.Forms.PictureBox()
        Me.LeftPanel = New System.Windows.Forms.Panel()
        Me.DDSExportGroup.SuspendLayout()
        Me.ImageExportGroup.SuspendLayout()
        Me.RightPanel.SuspendLayout()
        Me.BenchGroupBox.SuspendLayout()
        CType(Me.PreviewPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.LeftPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'ExportDDSButton
        '
        Me.ExportDDSButton.Location = New System.Drawing.Point(12, 260)
        Me.ExportDDSButton.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.ExportDDSButton.Name = "ExportDDSButton"
        Me.ExportDDSButton.Size = New System.Drawing.Size(366, 65)
        Me.ExportDDSButton.TabIndex = 12
        Me.ExportDDSButton.Text = "Convert"
        Me.ExportDDSButton.UseVisualStyleBackColor = True
        '
        'CompressionCheckBox
        '
        Me.CompressionCheckBox.AutoSize = True
        Me.CompressionCheckBox.Checked = True
        Me.CompressionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CompressionCheckBox.Location = New System.Drawing.Point(12, 79)
        Me.CompressionCheckBox.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.CompressionCheckBox.Name = "CompressionCheckBox"
        Me.CompressionCheckBox.Size = New System.Drawing.Size(170, 29)
        Me.CompressionCheckBox.TabIndex = 4
        Me.CompressionCheckBox.Text = "Compression"
        Me.CompressionCheckBox.UseVisualStyleBackColor = True
        '
        'MipMapCheckBox
        '
        Me.MipMapCheckBox.AutoSize = True
        Me.MipMapCheckBox.Location = New System.Drawing.Point(12, 37)
        Me.MipMapCheckBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.MipMapCheckBox.Name = "MipMapCheckBox"
        Me.MipMapCheckBox.Size = New System.Drawing.Size(132, 29)
        Me.MipMapCheckBox.TabIndex = 3
        Me.MipMapCheckBox.Text = "MipMaps"
        Me.MipMapCheckBox.UseVisualStyleBackColor = True
        '
        'NoAlphaRB
        '
        Me.NoAlphaRB.AutoSize = True
        Me.NoAlphaRB.Checked = True
        Me.NoAlphaRB.Location = New System.Drawing.Point(194, 33)
        Me.NoAlphaRB.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.NoAlphaRB.Name = "NoAlphaRB"
        Me.NoAlphaRB.Size = New System.Drawing.Size(131, 29)
        Me.NoAlphaRB.TabIndex = 7
        Me.NoAlphaRB.TabStop = True
        Me.NoAlphaRB.Text = "No Alpha"
        Me.NoAlphaRB.UseVisualStyleBackColor = True
        '
        'SharpAlphaRB
        '
        Me.SharpAlphaRB.AutoSize = True
        Me.SharpAlphaRB.Location = New System.Drawing.Point(194, 77)
        Me.SharpAlphaRB.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.SharpAlphaRB.Name = "SharpAlphaRB"
        Me.SharpAlphaRB.Size = New System.Drawing.Size(161, 29)
        Me.SharpAlphaRB.TabIndex = 8
        Me.SharpAlphaRB.Text = "Sharp Alpha"
        Me.SharpAlphaRB.UseVisualStyleBackColor = True
        '
        'SmoothAlphaRB
        '
        Me.SmoothAlphaRB.AutoSize = True
        Me.SmoothAlphaRB.Location = New System.Drawing.Point(194, 121)
        Me.SmoothAlphaRB.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.SmoothAlphaRB.Name = "SmoothAlphaRB"
        Me.SmoothAlphaRB.Size = New System.Drawing.Size(177, 29)
        Me.SmoothAlphaRB.TabIndex = 9
        Me.SmoothAlphaRB.Text = "Smooth Alpha"
        Me.SmoothAlphaRB.UseVisualStyleBackColor = True
        '
        'ExtendedHeaderCheckBox
        '
        Me.ExtendedHeaderCheckBox.AutoSize = True
        Me.ExtendedHeaderCheckBox.Checked = True
        Me.ExtendedHeaderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ExtendedHeaderCheckBox.Location = New System.Drawing.Point(12, 121)
        Me.ExtendedHeaderCheckBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.ExtendedHeaderCheckBox.Name = "ExtendedHeaderCheckBox"
        Me.ExtendedHeaderCheckBox.Size = New System.Drawing.Size(97, 29)
        Me.ExtendedHeaderCheckBox.TabIndex = 5
        Me.ExtendedHeaderCheckBox.Text = "DX10"
        Me.ExtendedHeaderCheckBox.UseVisualStyleBackColor = True
        '
        'DDSExportGroup
        '
        Me.DDSExportGroup.Controls.Add(Me.PreMultAlphaRB)
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
        Me.DDSExportGroup.Location = New System.Drawing.Point(6, 6)
        Me.DDSExportGroup.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.DDSExportGroup.Name = "DDSExportGroup"
        Me.DDSExportGroup.Padding = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.DDSExportGroup.Size = New System.Drawing.Size(388, 337)
        Me.DDSExportGroup.TabIndex = 0
        Me.DDSExportGroup.TabStop = False
        Me.DDSExportGroup.Text = "Image to DDS"
        '
        'PreMultAlphaRB
        '
        Me.PreMultAlphaRB.AutoSize = True
        Me.PreMultAlphaRB.Location = New System.Drawing.Point(194, 165)
        Me.PreMultAlphaRB.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.PreMultAlphaRB.Name = "PreMultAlphaRB"
        Me.PreMultAlphaRB.Size = New System.Drawing.Size(178, 29)
        Me.PreMultAlphaRB.TabIndex = 10
        Me.PreMultAlphaRB.TabStop = True
        Me.PreMultAlphaRB.Text = "PreMult Alpha"
        Me.PreMultAlphaRB.UseVisualStyleBackColor = True
        '
        'OverrideComboBox
        '
        Me.OverrideComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.OverrideComboBox.FormattingEnabled = True
        Me.OverrideComboBox.Location = New System.Drawing.Point(12, 210)
        Me.OverrideComboBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.OverrideComboBox.Name = "OverrideComboBox"
        Me.OverrideComboBox.Size = New System.Drawing.Size(362, 33)
        Me.OverrideComboBox.TabIndex = 11
        '
        'NormalCheckBox
        '
        Me.NormalCheckBox.AutoSize = True
        Me.NormalCheckBox.Location = New System.Drawing.Point(12, 165)
        Me.NormalCheckBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.NormalCheckBox.Name = "NormalCheckBox"
        Me.NormalCheckBox.Size = New System.Drawing.Size(160, 29)
        Me.NormalCheckBox.TabIndex = 6
        Me.NormalCheckBox.Text = "Normal Map"
        Me.NormalCheckBox.UseVisualStyleBackColor = True
        '
        'ImageExportGroup
        '
        Me.ImageExportGroup.Controls.Add(Me.Label1)
        Me.ImageExportGroup.Controls.Add(Me.OutputFormatComboBox)
        Me.ImageExportGroup.Controls.Add(Me.ExportImageButton)
        Me.ImageExportGroup.Enabled = False
        Me.ImageExportGroup.Location = New System.Drawing.Point(6, 354)
        Me.ImageExportGroup.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.ImageExportGroup.Name = "ImageExportGroup"
        Me.ImageExportGroup.Padding = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.ImageExportGroup.Size = New System.Drawing.Size(388, 163)
        Me.ImageExportGroup.TabIndex = 0
        Me.ImageExportGroup.TabStop = False
        Me.ImageExportGroup.Text = "DDS to Image"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 40)
        Me.Label1.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(155, 25)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Output Format:"
        '
        'OutputFormatComboBox
        '
        Me.OutputFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.OutputFormatComboBox.FormattingEnabled = True
        Me.OutputFormatComboBox.Items.AddRange(New Object() {"PNG", "BMP", "JPG"})
        Me.OutputFormatComboBox.Location = New System.Drawing.Point(178, 35)
        Me.OutputFormatComboBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.OutputFormatComboBox.Name = "OutputFormatComboBox"
        Me.OutputFormatComboBox.Size = New System.Drawing.Size(194, 33)
        Me.OutputFormatComboBox.TabIndex = 13
        '
        'ExportImageButton
        '
        Me.ExportImageButton.Location = New System.Drawing.Point(12, 87)
        Me.ExportImageButton.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.ExportImageButton.Name = "ExportImageButton"
        Me.ExportImageButton.Size = New System.Drawing.Size(364, 65)
        Me.ExportImageButton.TabIndex = 14
        Me.ExportImageButton.Text = "Convert"
        Me.ExportImageButton.UseVisualStyleBackColor = True
        '
        'InfoTextBox
        '
        Me.InfoTextBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.InfoTextBox.Location = New System.Drawing.Point(0, 0)
        Me.InfoTextBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.InfoTextBox.Multiline = True
        Me.InfoTextBox.Name = "InfoTextBox"
        Me.InfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.InfoTextBox.Size = New System.Drawing.Size(400, 704)
        Me.InfoTextBox.TabIndex = 2
        '
        'LoadImageButton
        '
        Me.LoadImageButton.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.LoadImageButton.Location = New System.Drawing.Point(0, 704)
        Me.LoadImageButton.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.LoadImageButton.Name = "LoadImageButton"
        Me.LoadImageButton.Size = New System.Drawing.Size(400, 65)
        Me.LoadImageButton.TabIndex = 1
        Me.LoadImageButton.Text = "Load Image"
        Me.LoadImageButton.UseVisualStyleBackColor = True
        '
        'RightPanel
        '
        Me.RightPanel.Controls.Add(Me.BenchGroupBox)
        Me.RightPanel.Controls.Add(Me.ImageExportGroup)
        Me.RightPanel.Controls.Add(Me.DDSExportGroup)
        Me.RightPanel.Dock = System.Windows.Forms.DockStyle.Right
        Me.RightPanel.Location = New System.Drawing.Point(1200, 0)
        Me.RightPanel.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.RightPanel.Name = "RightPanel"
        Me.RightPanel.Size = New System.Drawing.Size(400, 769)
        Me.RightPanel.TabIndex = 0
        '
        'BenchGroupBox
        '
        Me.BenchGroupBox.Controls.Add(Me.CalcMetricsButton)
        Me.BenchGroupBox.Controls.Add(Me.DecBenchButton)
        Me.BenchGroupBox.Controls.Add(Me.EncBenchButton)
        Me.BenchGroupBox.Enabled = False
        Me.BenchGroupBox.Location = New System.Drawing.Point(6, 529)
        Me.BenchGroupBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.BenchGroupBox.Name = "BenchGroupBox"
        Me.BenchGroupBox.Padding = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.BenchGroupBox.Size = New System.Drawing.Size(388, 217)
        Me.BenchGroupBox.TabIndex = 0
        Me.BenchGroupBox.TabStop = False
        Me.BenchGroupBox.Text = "Image Tests"
        '
        'CalcMetricsButton
        '
        Me.CalcMetricsButton.Location = New System.Drawing.Point(12, 148)
        Me.CalcMetricsButton.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.CalcMetricsButton.Name = "CalcMetricsButton"
        Me.CalcMetricsButton.Size = New System.Drawing.Size(364, 44)
        Me.CalcMetricsButton.TabIndex = 17
        Me.CalcMetricsButton.Text = "Quality Benchmark"
        Me.CalcMetricsButton.UseVisualStyleBackColor = True
        '
        'DecBenchButton
        '
        Me.DecBenchButton.Enabled = False
        Me.DecBenchButton.Location = New System.Drawing.Point(12, 92)
        Me.DecBenchButton.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.DecBenchButton.Name = "DecBenchButton"
        Me.DecBenchButton.Size = New System.Drawing.Size(364, 44)
        Me.DecBenchButton.TabIndex = 16
        Me.DecBenchButton.Text = "Decoding Benchmark"
        Me.DecBenchButton.UseVisualStyleBackColor = True
        '
        'EncBenchButton
        '
        Me.EncBenchButton.Enabled = False
        Me.EncBenchButton.Location = New System.Drawing.Point(12, 37)
        Me.EncBenchButton.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.EncBenchButton.Name = "EncBenchButton"
        Me.EncBenchButton.Size = New System.Drawing.Size(364, 44)
        Me.EncBenchButton.TabIndex = 15
        Me.EncBenchButton.Text = "Encoding Benchmark"
        Me.EncBenchButton.UseVisualStyleBackColor = True
        '
        'PreviewPictureBox
        '
        Me.PreviewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PreviewPictureBox.Location = New System.Drawing.Point(400, 0)
        Me.PreviewPictureBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.PreviewPictureBox.Name = "PreviewPictureBox"
        Me.PreviewPictureBox.Size = New System.Drawing.Size(800, 769)
        Me.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PreviewPictureBox.TabIndex = 1
        Me.PreviewPictureBox.TabStop = False
        '
        'LeftPanel
        '
        Me.LeftPanel.Controls.Add(Me.InfoTextBox)
        Me.LeftPanel.Controls.Add(Me.LoadImageButton)
        Me.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left
        Me.LeftPanel.Location = New System.Drawing.Point(0, 0)
        Me.LeftPanel.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        Me.LeftPanel.Name = "LeftPanel"
        Me.LeftPanel.Size = New System.Drawing.Size(400, 769)
        Me.LeftPanel.TabIndex = 0
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 25.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1600, 769)
        Me.Controls.Add(Me.PreviewPictureBox)
        Me.Controls.Add(Me.LeftPanel)
        Me.Controls.Add(Me.RightPanel)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.MinimumSize = New System.Drawing.Size(1590, 733)
        Me.Name = "Form1"
        Me.Text = "TexInspect"
        Me.DDSExportGroup.ResumeLayout(False)
        Me.DDSExportGroup.PerformLayout()
        Me.ImageExportGroup.ResumeLayout(False)
        Me.ImageExportGroup.PerformLayout()
        Me.RightPanel.ResumeLayout(False)
        Me.BenchGroupBox.ResumeLayout(False)
        CType(Me.PreviewPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.LeftPanel.ResumeLayout(False)
        Me.LeftPanel.PerformLayout()
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
    Friend WithEvents OutputFormatComboBox As ComboBox
    Friend WithEvents NormalCheckBox As CheckBox
    Friend WithEvents OverrideComboBox As ComboBox
    Friend WithEvents InfoTextBox As TextBox
    Friend WithEvents LoadImageButton As Button
    Friend WithEvents RightPanel As Panel
    Friend WithEvents PreviewPictureBox As PictureBox
    Friend WithEvents LeftPanel As Panel
    Friend WithEvents Label1 As Label
    Friend WithEvents EncBenchButton As Button
    Friend WithEvents BenchGroupBox As GroupBox
    Friend WithEvents CalcMetricsButton As Button
    Friend WithEvents DecBenchButton As Button
    Friend WithEvents PreMultAlphaRB As RadioButton
End Class
