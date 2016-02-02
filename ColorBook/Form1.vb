Public Class Form1
    Dim Bitmap As Bitmap
    Dim FillColor As Color = Color.Red
    Dim BorderColor As Color = Color.Black
    Dim Stack As Stack = New Stack()
    Dim PalletteWidth As Integer = 64
    Dim Pallette(9) As Color
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Read image data.
        Bitmap = Bitmap.FromFile("C:\Users\Brent\Documents\Colouring Pages\Alien.bmp")
        ' Resize window.
        SetClientSizeCore(Bitmap.Width + PalletteWidth, Bitmap.Height + MenuStrip1.Height)
    End Sub
    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint
        ' Create Point for upper-left corner of image.
        Dim ulCorner As New Point(0, MenuStrip1.Height)
        ' Draw image to screen.
        e.Graphics.DrawImage(Bitmap, ulCorner)
        ' Draw palette.
        Pallette(0) = Color.White
        Pallette(1) = Color.Red
        Pallette(2) = Color.Blue
        Pallette(3) = Color.Green
        Pallette(4) = Color.Yellow
        Pallette(5) = Color.FromArgb(136, 0, 21)
        Pallette(6) = Color.FromArgb(255, 174, 201)
        Pallette(7) = Color.FromArgb(153, 217, 234)
        Pallette(8) = Color.FromArgb(163, 73, 164)
        Pallette(9) = Color.FromArgb(185, 122, 87)
        e.Graphics.TranslateTransform(0, MenuStrip1.Height)
        For I = 0 To Pallette.Count - 1
            Dim BrushColor = New SolidBrush(Pallette(I))
            Dim PenColor = New Pen(Brushes.Black)
            e.Graphics.FillRectangle(BrushColor, Bitmap.Width, I * PalletteWidth, PalletteWidth, PalletteWidth)
            If (Pallette(I).ToArgb() = FillColor.ToArgb()) Then
                e.Graphics.DrawRectangle(PenColor, Bitmap.Width + 1, I * PalletteWidth + 1, PalletteWidth - 2, PalletteWidth - 2)
            End If
        Next
    End Sub
    Function peek(X As Integer, Y As Integer, Values() As Byte, Stride As Integer)
        Return Color.FromArgb(Values(Y * Stride + X * 3 + 2), Values(Y * Stride + X * 3 + 1), Values(Y * Stride + X * 3))
    End Function
    Private Sub FloodFill(X As Integer, Y As Integer, TargetColor As Color)
        If (TargetColor.ToArgb() = FillColor.ToArgb()) Then
            Return
        End If
        ' Lock the bitmap's bits.  
        Dim rect As New Rectangle(0, 0, Bitmap.Width, Bitmap.Height)
        Dim bmpData As System.Drawing.Imaging.BitmapData = Bitmap.LockBits(rect, _
            Drawing.Imaging.ImageLockMode.ReadWrite, Bitmap.PixelFormat)
        ' Get the address of the first line.
        Dim ptr As IntPtr = bmpData.Scan0
        ' Declare an array to hold the bytes of the bitmap.
        ' This code is specific to a bitmap with 24 bits per pixels.
        Dim bytes As Integer = Math.Abs(bmpData.Stride) * Bitmap.Height
        Dim rgbValues(bytes - 1) As Byte
        ' Copy the RGB values into the array.
        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes)
        Dim Stride = bmpData.Stride
        Stack.Push(New Point(X, Y))
        While (Stack.Count > 0)
            Dim Point = Stack.Pop()
            X = Point.X
            Y = Point.Y
            Dim Pixel As Color = peek(X, Y, rgbValues, Stride)
            If (Pixel.ToArgb() = TargetColor.ToArgb()) Then
                Dim x1 = X
                Dim x2 = X
                Pixel = peek(x1, Y, rgbValues, Stride)
                While (x1 > 0 And Pixel.ToArgb() = TargetColor.ToArgb())
                    x1 = x1 - 1
                    Pixel = peek(x1, Y, rgbValues, Stride)
                End While
                Pixel = peek(x2, Y, rgbValues, Stride)
                While (x2 < Bitmap.Width - 1 And Pixel.ToArgb() = TargetColor.ToArgb())
                    x2 = x2 + 1
                    Pixel = peek(x2, Y, rgbValues, Stride)
                End While
                X = x1 + 1
                While (X < x2)
                    rgbValues(Y * bmpData.Stride + X * 3 + 2) = FillColor.R
                    rgbValues(Y * bmpData.Stride + X * 3 + 1) = FillColor.G
                    rgbValues(Y * bmpData.Stride + X * 3) = FillColor.B
                    If (Y - 1 > 0) Then
                        Pixel = peek(X, Y - 1, rgbValues, Stride)
                        If (Pixel.ToArgb() = TargetColor.ToArgb()) Then
                            Stack.Push(New Point(X, Y - 1))
                        End If
                    End If
                    If (Y + 2 < Bitmap.Height) Then
                        Pixel = peek(X, Y + 1, rgbValues, Stride)
                        If (Pixel.ToArgb() = TargetColor.ToArgb()) Then
                            Stack.Push(New Point(X, Y + 1))
                        End If
                    End If
                    X = X + 1
                End While
            End If
        End While
        ' Copy the RGB values back to the bitmap
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes)
        ' Unlock the bits.
        Bitmap.UnlockBits(bmpData)
    End Sub
    Private Sub Form1_MouseUp(sender As Object, e As MouseEventArgs) Handles MyBase.MouseUp
        Dim x = e.X
        Dim y = e.Y - MenuStrip1.Height
        Dim Graphics = Me.CreateGraphics()
        If (x > Bitmap.Width And y < PalletteWidth * (Pallette.Count)) Then
            FillColor = Pallette(Math.Floor(y / PalletteWidth))
            Invalidate(New Rectangle(Bitmap.Width, MenuStrip1.Height, PalletteWidth, PalletteWidth * Pallette.Count))
        ElseIf (x < Bitmap.Width And y < Bitmap.Height) Then
            Dim TargetColor As Color = Bitmap.GetPixel(x, y)
            If (TargetColor.ToArgb() <> Color.Black.ToArgb()) Then
                FloodFill(x, y, TargetColor)
                Invalidate()
            End If
        End If
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        OpenFileDialog1.ShowDialog()
    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        If (OpenFileDialog1.FileName <> Nothing) Then
            Bitmap = New Bitmap(OpenFileDialog1.FileName)
            Invalidate()
        End If
    End Sub
End Class
