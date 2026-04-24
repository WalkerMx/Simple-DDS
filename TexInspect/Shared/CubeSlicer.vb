Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class CubeSlicer
    Implements IDisposable

    Public Disposed As Boolean
    Public Width As Integer
    Public Height As Integer
    Public FaceSize As Integer
    Public Layout As ImageLayout
    Public FilePath As String

    Private CubeBitmaps As New Dictionary(Of String, Bitmap)
    Private FaceCoordinates As New Dictionary(Of String, Point)

    Public Enum ImageLayout
        StripHorizontal = 0
        StripVertical = 1
        CrossHorizontal = 2
        CrossVertical = 3
    End Enum

    Public Sub New(SourcePath As String)
        FilePath = SourcePath
        Using Source As New Bitmap(FilePath)
            Width = Source.Width
            Height = Source.Height
            GetLayout()
            GetCoordinates()
            SplitBitmap(Source)
        End Using
    End Sub

    Public Sub SaveBitmaps(TargetPath As String)
        For Each CubeFace In CubeBitmaps
            Dim FileName As String = Path.Combine(TargetPath, $"Face_{CubeFace.Key}.png")
            CubeFace.Value.Save(FileName, ImageFormat.Png)
        Next
    End Sub

    Private Sub GetLayout()
        If Width / Height = 6.0 Then
            Layout = ImageLayout.StripHorizontal
            FaceSize = Height
        ElseIf Height / Width = 6.0 Then
            Layout = ImageLayout.StripVertical
            FaceSize = Width
        ElseIf (Width / Height) = (4 / 3) Then
            Layout = ImageLayout.CrossHorizontal
            FaceSize = Width \ 4
        ElseIf (Width / Height) = (3 / 4) Then
            Layout = ImageLayout.CrossVertical
            FaceSize = Height \ 4
        Else
            Throw New Exception("Unsupported aspect ratio.")
        End If
    End Sub

    Private Sub GetCoordinates()
        Select Case Layout
            Case ImageLayout.StripHorizontal
                FaceCoordinates.Add("PX", New Point(0, 0)) : FaceCoordinates.Add("NX", New Point(1, 0))
                FaceCoordinates.Add("PY", New Point(2, 0)) : FaceCoordinates.Add("NY", New Point(3, 0))
                FaceCoordinates.Add("PZ", New Point(4, 0)) : FaceCoordinates.Add("NZ", New Point(5, 0))
            Case ImageLayout.StripVertical
                FaceCoordinates.Add("PX", New Point(0, 0)) : FaceCoordinates.Add("NX", New Point(0, 1))
                FaceCoordinates.Add("PY", New Point(0, 2)) : FaceCoordinates.Add("NY", New Point(0, 3))
                FaceCoordinates.Add("PZ", New Point(0, 4)) : FaceCoordinates.Add("NZ", New Point(0, 5))
            Case ImageLayout.CrossHorizontal
                FaceCoordinates.Add("PY", New Point(1, 0)) : FaceCoordinates.Add("NX", New Point(0, 1))
                FaceCoordinates.Add("PZ", New Point(1, 1)) : FaceCoordinates.Add("PX", New Point(2, 1))
                FaceCoordinates.Add("NZ", New Point(3, 1)) : FaceCoordinates.Add("NY", New Point(1, 2))
            Case ImageLayout.CrossVertical
                FaceCoordinates.Add("PY", New Point(1, 0)) : FaceCoordinates.Add("NX", New Point(0, 1))
                FaceCoordinates.Add("PZ", New Point(1, 1)) : FaceCoordinates.Add("PX", New Point(2, 1))
                FaceCoordinates.Add("NY", New Point(1, 2)) : FaceCoordinates.Add("NZ", New Point(1, 3))
        End Select
    End Sub

    Private Sub SplitBitmap(Source As Bitmap)
        Dim Faces = FaceCoordinates.ToArray()
        Dim TempBitmaps(5) As Bitmap
        Dim Rect As New Rectangle(0, 0, Width, Height)
        Dim SourceData As BitmapData = Source.LockBits(Rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Dim SourcePtr As IntPtr = SourceData.Scan0
        Dim SourceStride As Integer = SourceData.Stride
        Parallel.For(0, Faces.Length, Sub(i)
                                          Dim FaceWidth = Faces(i).Value.X * FaceSize
                                          Dim FaceHeight = Faces(i).Value.Y * FaceSize
                                          Dim FaceByteCount As Integer = FaceSize * FaceSize * 4
                                          Dim FaceBuffer As Byte() = New Byte(FaceByteCount - 1) {}
                                          Dim FaceStride As Integer = FaceSize * 4
                                          For y As Integer = 0 To FaceSize - 1
                                              Dim SourceRowOffset As Integer = ((FaceHeight + y) * SourceStride) + (FaceWidth * 4)
                                              Dim TargetRowOffset As Integer = y * FaceStride
                                              Dim ReadAddressPtr As IntPtr = IntPtr.Add(SourcePtr, SourceRowOffset)
                                              Marshal.Copy(ReadAddressPtr, FaceBuffer, TargetRowOffset, FaceStride)
                                          Next
                                          Dim TargetBitmap As New Bitmap(FaceSize, FaceSize, PixelFormat.Format32bppArgb)
                                          Dim TargetData As BitmapData = TargetBitmap.LockBits(New Rectangle(0, 0, FaceSize, FaceSize), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
                                          Marshal.Copy(FaceBuffer, 0, TargetData.Scan0, FaceByteCount)
                                          TargetBitmap.UnlockBits(TargetData)
                                          TempBitmaps(i) = TargetBitmap
                                      End Sub)
        Source.UnlockBits(SourceData)
        For i = 0 To 5
            CubeBitmaps.Add(Faces(i).Key, TempBitmaps(i))
        Next
    End Sub

    Protected Overridable Sub Dispose(Disposing As Boolean)
        If Not Disposed Then
            If Disposing Then
                For Each CubeBitmap In CubeBitmaps.Values
                    If CubeBitmap IsNot Nothing Then CubeBitmap.Dispose()
                Next
                CubeBitmaps.Clear()
            End If
        End If
        Disposed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

End Class