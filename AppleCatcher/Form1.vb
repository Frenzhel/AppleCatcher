Public Class Form1
    Private playerScore As Integer = 0
    Private playerLives As Integer = 3
    Private catchCount As Integer = 0
    Private appleX As Integer = 0
    Private appleY As Integer = 0
    Private basketX As Integer = 0
    Private fallSpeed As Single = 4.0F
    Private isGameRunning As Boolean = False
    Private isGoodApple As Boolean = True
    Private keyLeftHeld As Boolean = False
    Private keyRightHeld As Boolean = False
    Private statusMessage As String = "Press SPACE to Start"
    Private Const GOOD_APPLE_CHANCE As Integer = 70
    Private rng As New Random()
    Private ReadOnly Property AppleSize As Integer
        Get
            Return Math.Max(24, Math.Min(80, CInt(Me.ClientSize.Width * 0.08)))
        End Get
    End Property
    Private ReadOnly Property BasketWidth As Integer
        Get
            Return Math.Max(60, Math.Min(200, CInt(Me.ClientSize.Width * 0.18)))
        End Get
    End Property
    Private ReadOnly Property BasketHeight As Integer
        Get
            Return Math.Max(16, Math.Min(40, CInt(Me.ClientSize.Height * 0.04)))
        End Get
    End Property
    Private ReadOnly Property BasketY As Integer
        Get
            Return Me.ClientSize.Height - CInt(Me.ClientSize.Height * 0.09)
        End Get
    End Property
    Private ReadOnly Property BasketSpeed As Integer
        Get
            Dim baseSpeed As Integer = Math.Max(5, CInt(Me.ClientSize.Width * 0.025))
            Dim bonus As Integer = CInt((fallSpeed - 15.0F) * 0.4F)
            Return baseSpeed + Math.Max(0, bonus)
        End Get
    End Property
    Private ReadOnly Property HudFontSize As Single
        Get
            Return Math.Max(9.0F, Math.Min(22.0F, Me.ClientSize.Height / 42.0F))
        End Get
    End Property
    Private ReadOnly Property BodyFontSize As Single
        Get
            Return Math.Max(7.0F, Math.Min(16.0F, Me.ClientSize.Height / 55.0F))
        End Get
    End Property
    Private ReadOnly Property TitleFontSize As Single
        Get
            Return Math.Max(14.0F, Math.Min(40.0F, Me.ClientSize.Height / 18.0F))
        End Get
    End Property
    Private ReadOnly Property GroundHeight As Integer
        Get
            Return Math.Max(8, CInt(Me.ClientSize.Height * 0.06))
        End Get
    End Property
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Apple Catcher"
        Me.ClientSize = New Size(500, 600)
        Me.BackColor = Color.FromArgb(20, 30, 20)
        Me.DoubleBuffered = True
        Me.KeyPreview = True
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.MaximizeBox = True
        basketX = (Me.ClientSize.Width \ 2) - (BasketWidth \ 2)
        SpawnApple()
        GameTimer.Interval = 16
        GameTimer.Start()
    End Sub
    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If basketX + BasketWidth > Me.ClientSize.Width Then
            basketX = Me.ClientSize.Width - BasketWidth
        End If
        If basketX < 0 Then basketX = 0
    End Sub
    Private Sub SpawnApple()
        appleX = rng.Next(10, Me.ClientSize.Width - AppleSize - 10)
        appleY = -AppleSize
        Dim chance As Integer = rng.Next(1, 101)
        isGoodApple = (chance <= GOOD_APPLE_CHANCE)
    End Sub
    Private Function CheckCatch() As Boolean
        Dim appleBottom As Integer = appleY + AppleSize
        Dim basketRight As Integer = basketX + BasketWidth
        If appleBottom >= BasketY And
           appleBottom <= BasketY + BasketHeight + 10 And
           appleX + AppleSize >= basketX And
           appleX <= basketRight Then
            Return True
        End If
        Return False
    End Function
    Private Sub UpdateScore(pointsToAdd As Integer)
        playerScore = playerScore + pointsToAdd
        catchCount = catchCount + 1
        If catchCount Mod 5 = 0 Then
            fallSpeed = fallSpeed * 1.25F
        End If
    End Sub
    Private Sub HandleAppleMiss()
        If isGoodApple Then
            playerLives = playerLives - 1
            statusMessage = "Missed a good apple! -1 Life"
        Else
            statusMessage = "Good dodge! Worm avoided!"
        End If
    End Sub
    Private Function GetRating(finalScore As Integer) As String
        If finalScore >= 150 Then
            Return "APPLE MASTER!  Incredible!"
        ElseIf finalScore >= 100 Then
            Return "Amazing!  You are a pro!"
        ElseIf finalScore >= 60 Then
            Return "Great job!  Keep it up!"
        ElseIf finalScore >= 30 Then
            Return "Not bad!  Try again?"
        Else
            Return "Keep practicing!"
        End If
    End Function
    Private Sub ResetGame()
        playerScore = 0
        playerLives = 3
        catchCount = 0
        fallSpeed = 4.0F
        statusMessage = ""
        isGameRunning = True
        basketX = (Me.ClientSize.Width \ 2) - (BasketWidth \ 2)
        SpawnApple()
    End Sub
    Private Sub GameTimer_Tick(sender As Object, e As EventArgs) Handles GameTimer.Tick
        If Not isGameRunning Then
            Me.Invalidate()
            Return
        End If
        If keyLeftHeld And basketX > 0 Then
            basketX = basketX - BasketSpeed
        End If
        If keyRightHeld And basketX + BasketWidth < Me.ClientSize.Width Then
            basketX = basketX + BasketSpeed
        End If
        appleY = appleY + CInt(fallSpeed)
        If CheckCatch() Then
            If isGoodApple Then
                UpdateScore(10)
                statusMessage = "Nice catch!  +10 pts"
            Else
                playerLives = playerLives - 1
                statusMessage = "Worm apple!  -1 Life"
            End If
            SpawnApple()
        ElseIf appleY > Me.ClientSize.Height Then
            HandleAppleMiss()
            SpawnApple()
        End If
        If playerLives <= 0 Then
            playerLives = 0
            isGameRunning = False
            statusMessage = "GAME OVER!  Press SPACE to play again."
        End If
        Me.Invalidate()
    End Sub
    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Using bgBrush As New Drawing2D.LinearGradientBrush(
            Me.ClientRectangle,
            Color.FromArgb(10, 20, 10),
            Color.FromArgb(35, 65, 35),
            Drawing2D.LinearGradientMode.Vertical)
            g.FillRectangle(bgBrush, Me.ClientRectangle)
        End Using
        If Not isGameRunning Then
            DrawMenuScreen(g)
        Else
            DrawGround(g)
            DrawApple(g)
            DrawBasket(g)
            DrawHUD(g)
            DrawStatusMessage(g)
        End If
    End Sub
    Private Sub DrawGround(g As Graphics)
        Dim groundY As Integer = Me.ClientSize.Height - GroundHeight
        Using groundBrush As New SolidBrush(Color.FromArgb(30, 60, 20))
            g.FillRectangle(groundBrush, 0, groundY, Me.ClientSize.Width, GroundHeight)
        End Using
        Using linePen As New Pen(Color.FromArgb(50, 100, 30), 2)
            g.DrawLine(linePen, 0, groundY, Me.ClientSize.Width, groundY)
        End Using
    End Sub
    Private Sub DrawApple(g As Graphics)
        Dim bodyColor As Color = If(isGoodApple,
            Color.FromArgb(210, 35, 35),
            Color.FromArgb(90, 58, 18))
        Dim shadeColor As Color = If(isGoodApple,
            Color.FromArgb(160, 20, 20),
            Color.FromArgb(60, 35, 8))
        Using b As New SolidBrush(bodyColor)
            g.FillEllipse(b, appleX, appleY, AppleSize, AppleSize)
        End Using
        Using shade As New SolidBrush(shadeColor)
            g.FillEllipse(shade, appleX + 4, appleY + AppleSize \ 2,
                          AppleSize - 8, AppleSize \ 2 - 2)
        End Using
        Using gloss As New SolidBrush(Color.FromArgb(90, 255, 255, 255))
            Dim glossW As Integer = Math.Max(6, AppleSize \ 3)
            Dim glossH As Integer = Math.Max(4, AppleSize \ 4)
            g.FillEllipse(gloss, appleX + AppleSize \ 5, appleY + AppleSize \ 6,
                          glossW, glossH)
        End Using
        Using outlinePen As New Pen(Color.FromArgb(60, 0, 0, 0), 1.5F)
            g.DrawEllipse(outlinePen, appleX, appleY, AppleSize, AppleSize)
        End Using
        Dim stemThick As Single = Math.Max(1.5F, AppleSize / 14.0F)
        Dim stemLen As Integer = Math.Max(5, AppleSize \ 4)
        Using stemPen As New Pen(Color.SaddleBrown, stemThick)
            stemPen.StartCap = Drawing2D.LineCap.Round
            stemPen.EndCap = Drawing2D.LineCap.Round
            Dim cx As Integer = appleX + AppleSize \ 2
            g.DrawLine(stemPen, cx, appleY + 1, cx + stemLen \ 2, appleY - stemLen)
        End Using
        Dim leafSize As Integer = Math.Max(6, AppleSize \ 4)
        Using leafBrush As New SolidBrush(Color.FromArgb(60, 160, 40))
            Dim cx As Integer = appleX + AppleSize \ 2
            g.FillEllipse(leafBrush, cx + 2, appleY - leafSize, leafSize + 2, leafSize - 2)
        End Using
        If Not isGoodApple Then
            Dim wormSegSize As Integer = Math.Max(5, AppleSize \ 6)
            Using wb As New SolidBrush(Color.FromArgb(100, 200, 50))
                Dim wx As Integer = appleX + AppleSize \ 6
                Dim wy As Integer = appleY + AppleSize \ 2 + 4
                Dim segSpacing As Integer = wormSegSize + 2
                For i As Integer = 0 To 3
                    g.FillEllipse(wb, wx + (i * segSpacing), wy, wormSegSize, wormSegSize)
                Next
                Using headBrush As New SolidBrush(Color.FromArgb(140, 230, 80))
                    g.FillEllipse(headBrush, wx - 2, wy - 2, wormSegSize + 3, wormSegSize + 3)
                End Using
                Dim eyeSize As Integer = Math.Max(1, wormSegSize \ 4)
                g.FillEllipse(Brushes.Black, wx, wy + 1, eyeSize + 1, eyeSize + 1)
                g.FillEllipse(Brushes.Black, wx + 3, wy + 1, eyeSize + 1, eyeSize + 1)
            End Using
        End If
    End Sub
    Private Sub DrawBasket(g As Graphics)
        Dim bY As Integer = BasketY
        Using shadowBrush As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
            g.FillEllipse(shadowBrush, basketX + 5, bY + BasketHeight + 2,
                          BasketWidth - 10, Math.Max(4, BasketHeight \ 3))
        End Using
        Using grad As New Drawing2D.LinearGradientBrush(
            New Rectangle(basketX, bY, BasketWidth, BasketHeight),
            Color.FromArgb(200, 140, 50),
            Color.FromArgb(140, 90, 25),
            Drawing2D.LinearGradientMode.Vertical)
            g.FillRectangle(grad, basketX, bY, BasketWidth, BasketHeight)
        End Using
        Dim weaveSpacing As Integer = Math.Max(8, BasketWidth \ 8)
        Using weavePen As New Pen(Color.FromArgb(80, 100, 60, 10), 1)
            Dim wx As Integer = basketX + weaveSpacing
            Do While wx < basketX + BasketWidth - 4
                g.DrawLine(weavePen, wx, bY + 2, wx, bY + BasketHeight - 2)
                wx += weaveSpacing
            Loop
        End Using
        Dim rimHeight As Integer = Math.Max(3, BasketHeight \ 4)
        Using rimBrush As New SolidBrush(Color.FromArgb(230, 170, 70))
            g.FillRectangle(rimBrush, basketX, bY, BasketWidth, rimHeight)
        End Using
        Using outlinePen As New Pen(Color.FromArgb(160, 100, 30), 2)
            g.DrawRectangle(outlinePen, basketX, bY, BasketWidth, BasketHeight)
        End Using
    End Sub
    Private Sub DrawHUD(g As Graphics)
        Using hudFont As New Font("Consolas", HudFontSize, FontStyle.Bold)
            Using smallFont As New Font("Consolas", BodyFontSize)
                g.DrawString("Score: " & playerScore.ToString(),
                             hudFont, Brushes.LightGreen, 10, 10)
                Dim heartSize As Integer = Math.Max(12, CInt(HudFontSize * 1.1))
                Dim totalHeartsWidth As Integer = playerLives * (heartSize + 6)
                Dim heartX As Integer = (Me.ClientSize.Width \ 2) - (totalHeartsWidth \ 2)
                Dim heartY As Integer = Math.Max(8, CInt(Me.ClientSize.Height * 0.015))
                For i As Integer = 1 To playerLives
                    DrawHeart(g, heartX, heartY, heartSize, Color.OrangeRed)
                    heartX += heartSize + 6
                Next
                Dim speedLevel As Integer = CInt(Math.Floor((fallSpeed - 3.9F) / 1.0F))
                Dim speedText As String = "Speed: " & speedLevel.ToString()
                Dim speedSize As SizeF = g.MeasureString(speedText, hudFont)
                g.DrawString(speedText, hudFont, Brushes.Gold,
                             Me.ClientSize.Width - speedSize.Width - 10, 10)
                Dim legendY As Single = 10 + HudFontSize + 6
                g.DrawString("Legend:", smallFont, Brushes.DarkGray, 10, legendY)
                Dim dotSize As Integer = Math.Max(8, CInt(BodyFontSize))
                Dim legendTextX As Single = 10 + g.MeasureString("Legend:", smallFont).Width + 4
                Using redDot As New SolidBrush(Color.FromArgb(210, 35, 35))
                    g.FillEllipse(redDot, legendTextX, legendY + 2, dotSize, dotSize)
                End Using
                Dim afterRed As Single = legendTextX + dotSize + 2
                g.DrawString("=Good ", smallFont, Brushes.LightGray, afterRed, legendY)
                Dim afterGoodText As Single = afterRed + g.MeasureString("=Good ", smallFont).Width
                Using brownDot As New SolidBrush(Color.FromArgb(90, 58, 18))
                    g.FillEllipse(brownDot, afterGoodText, legendY + 2, dotSize, dotSize)
                End Using
                g.DrawString("=Bad(worm)", smallFont, Brushes.LightGray,
                             afterGoodText + dotSize + 2, legendY)
            End Using
        End Using
    End Sub
    Private Sub DrawHeart(g As Graphics, x As Integer, y As Integer,
                          size As Integer, col As Color)
        Using b As New SolidBrush(col)
            Dim half As Integer = size \ 2
            g.FillEllipse(b, x, y, half + 1, half)
            g.FillEllipse(b, x + half - 1, y, half + 1, half)
            Dim pts() As Point = {
                New Point(x, y + half \ 2),
                New Point(x + size, y + half \ 2),
                New Point(x + size \ 2, y + size)
            }
            g.FillPolygon(b, pts)
        End Using
    End Sub
    Private Sub DrawStatusMessage(g As Graphics)
        If statusMessage = "" Then Return
        Using msgFont As New Font("Consolas", HudFontSize * 0.9F, FontStyle.Bold)
            Dim msgSize As SizeF = g.MeasureString(statusMessage, msgFont)
            Dim msgX As Single = (Me.ClientSize.Width - msgSize.Width) / 2
            Dim msgY As Single = Me.ClientSize.Height - CInt(Me.ClientSize.Height * 0.2)
            Using pillBrush As New SolidBrush(Color.FromArgb(150, 0, 0, 0))
                g.FillRectangle(pillBrush, msgX - 10, msgY - 4,
                                msgSize.Width + 20, msgSize.Height + 8)
            End Using
            g.DrawString(statusMessage, msgFont, Brushes.LightYellow, msgX, msgY)
        End Using
    End Sub
    Private Sub DrawMenuScreen(g As Graphics)
        Using titleFont As New Font("Consolas", TitleFontSize, FontStyle.Bold)
            Using subFont As New Font("Consolas", HudFontSize, FontStyle.Bold)
                Using bodyFont As New Font("Consolas", BodyFontSize)
                    Dim titleText As String = "  APPLE CATCHER"
                    Dim titleSize As SizeF = g.MeasureString(titleText, titleFont)
                    Dim titleX As Single = (Me.ClientSize.Width - titleSize.Width) / 2
                    Dim titleY As Single = Me.ClientSize.Height * 0.18F
                    Dim iconSize As Integer = CInt(TitleFontSize * 1.1)
                    Using ab As New SolidBrush(Color.FromArgb(210, 35, 35))
                        g.FillEllipse(ab, CInt(titleX) + 2, CInt(titleY) + 4, iconSize, iconSize)
                    End Using
                    Using sp As New Pen(Color.SaddleBrown, 2)
                        g.DrawLine(sp, CInt(titleX) + iconSize \ 2 + 2, CInt(titleY) + 4,
                                       CInt(titleX) + iconSize \ 2 + 6, CInt(titleY) - 2)
                    End Using
                    Using gloss As New SolidBrush(Color.FromArgb(80, 255, 255, 255))
                        g.FillEllipse(gloss, CInt(titleX) + 6, CInt(titleY) + 8,
                                      Math.Max(4, iconSize \ 3), Math.Max(3, iconSize \ 4))
                    End Using
                    g.DrawString(titleText, titleFont, Brushes.LightGreen, titleX, titleY)
                    If playerScore > 0 Or playerLives = 0 Then
                        Dim rating As String = GetRating(playerScore)
                        Dim scoreStr As String = "Final Score:  " & playerScore.ToString()
                        Dim scoreSize As SizeF = g.MeasureString(scoreStr, subFont)
                        Dim scoreY As Single = titleY + titleSize.Height + Me.ClientSize.Height * 0.02F
                        g.DrawString(scoreStr, subFont, Brushes.Gold,
                                     (Me.ClientSize.Width - scoreSize.Width) / 2, scoreY)
                        Dim ratingSize As SizeF = g.MeasureString(rating, bodyFont)
                        g.DrawString(rating, bodyFont, Brushes.LightYellow,
                                     (Me.ClientSize.Width - ratingSize.Width) / 2,
                                     scoreY + scoreSize.Height + 4)
                    End If
                    Dim divY As Single = Me.ClientSize.Height * 0.42F
                    g.DrawLine(New Pen(Color.FromArgb(60, 150, 60), 1),
                               40, divY, Me.ClientSize.Width - 40, divY)
                    Dim lines() As String = {
                        "CONTROLS",
                        "  Left / Right Arrow  :  Move basket",
                        "  SPACE               :  Start / Restart",
                        "",
                        "RULES",
                        "  Red apple    =  Catch it!    (+10 pts)",
                        "  Brown apple  =  AVOID it!   (-1 life)",
                        "  Miss a red   =  -1 life",
                        "",
                        "Speed increases every 5 catches!"
                    }
                    Dim lineSpacing As Single = Math.Max(18, Me.ClientSize.Height * 0.044F)
                    Dim yPos As Single = divY + Me.ClientSize.Height * 0.025F
                    For Each line As String In lines
                        Dim isHeader As Boolean = (line = "CONTROLS" Or line = "RULES")
                        Using lineFont As New Font("Consolas",
                            If(isHeader, BodyFontSize * 1.1F, BodyFontSize),
                            If(isHeader, FontStyle.Bold, FontStyle.Regular))
                            Dim lineColor As Brush = If(isHeader, Brushes.LightGreen, Brushes.LightGray)
                            Dim lSize As SizeF = g.MeasureString(If(line = "", " ", line), lineFont)
                            g.DrawString(line, lineFont, lineColor,
                                         (Me.ClientSize.Width - lSize.Width) / 2, yPos)
                        End Using
                        yPos += If(line = "", lineSpacing * 0.4F, lineSpacing)
                    Next
                End Using
            End Using
        End Using
    End Sub
    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        Select Case e.KeyCode
            Case Keys.Left
                keyLeftHeld = True
            Case Keys.Right
                keyRightHeld = True
            Case Keys.Space
                If Not isGameRunning Then ResetGame()
        End Select
    End Sub
    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp
        Select Case e.KeyCode
            Case Keys.Left
                keyLeftHeld = False
            Case Keys.Right
                keyRightHeld = False
        End Select
    End Sub
End Class