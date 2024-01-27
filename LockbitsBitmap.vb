Imports System.Drawing.Imaging

' LockbitsBitmap Class by WalkerMx
' Inspired by A.Konzel's DirectBitmap Class

Public Class LockbitsBitmap
    Implements IDisposable

    Private LockedImage As Bitmap
    Private BytesPerPixel As Integer
    Private LockedData As BitmapData
    Private LockedPointer As IntPtr
    Private LockedRect As Rectangle
    Private LockedCount As Integer
    Private LockedBytes As Byte()
    Private Disposed As Boolean

    Public Sub New(Source As Bitmap, ColorMode As PixelFormat)
        LockedImage = New Bitmap(Source.Width, Source.Height, ColorMode)
        Using Gr As Graphics = Graphics.FromImage(LockedImage)
            Gr.CompositingMode = Drawing2D.CompositingMode.SourceCopy
            Gr.DrawImage(Source, 0, 0, LockedImage.Width, LockedImage.Height)
        End Using
        Select Case ColorMode
            Case PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb
                BytesPerPixel = 4
            Case PixelFormat.Format24bppRgb
                BytesPerPixel = 3
            Case PixelFormat.Format16bppRgb565, PixelFormat.Format16bppRgb555
                BytesPerPixel = 2
        End Select
        LockedRect = New Rectangle(0, 0, LockedImage.Width, LockedImage.Height)
        LockedData = LockedImage.LockBits(LockedRect, ImageLockMode.ReadWrite, LockedImage.PixelFormat)
        LockedPointer = LockedData.Scan0
        LockedCount = Math.Abs(LockedData.Stride) * LockedData.Height
        LockedBytes = New Byte(LockedCount - 1) {}
        Runtime.InteropServices.Marshal.Copy(LockedPointer, LockedBytes, 0, LockedCount)
        LockedImage.UnlockBits(LockedData)
    End Sub

    Public Function GetBitmap() As Bitmap
        LockedData = LockedImage.LockBits(LockedRect, ImageLockMode.ReadWrite, LockedImage.PixelFormat)
        Runtime.InteropServices.Marshal.Copy(LockedBytes, 0, LockedPointer, LockedCount)
        LockedImage.UnlockBits(LockedData)
        Return LockedImage
    End Function

    Public Sub SetPixelBytes(x As Integer, y As Integer, PixelBytes As Byte())
        Dim Index As Integer = ((y * LockedImage.Width) + x) * BytesPerPixel
        For i = 0 To BytesPerPixel - 1
            LockedBytes(Index + i) = PixelBytes(i)
        Next
    End Sub

    Public Function GetPixelBytes(x As Integer, y As Integer) As Byte()
        Dim Result As New List(Of Byte)
        Dim Index As Integer = ((y * LockedImage.Width) + x) * BytesPerPixel
        For i = 0 To BytesPerPixel - 1
            Result.Add(LockedBytes(Index + i))
        Next
        Return Result.ToArray
    End Function

    Protected Overridable Sub Dispose(Disposing As Boolean)
        If Not Disposed Then
            If Disposing Then
                LockedImage.Dispose()
            End If
            LockedBytes = Nothing
            Disposed = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(Disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
