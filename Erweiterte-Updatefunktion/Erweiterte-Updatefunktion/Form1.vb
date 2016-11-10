#Region "Imports"

Imports System.IO
Imports System.Net
Imports INIDatei.INIDatei 'DLL INIDatei

#End Region


Public Class Form1

#Region "Definitionen"

    Dim INI As New INIDatei.INIDatei 'Dekalriert INI
    Private WithEvents HTTPClient As WebClient 'HTTPClient

#End Region

#Region "Form-Events"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label3.Text = Application.ProductVersion 'Gibt aktuelle Version in Label aus
    End Sub

    Private Sub MetroTile1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        UpdatePrüfung()
    End Sub

#End Region

#Region "Subs"

    Sub UpdatePrüfung()
        Try
            If My.Computer.Network.IsAvailable = True Then 'Prüft ob Netzwerk-Verbindung besteht
                'Definitionen
                Dim FTPLink As String
                Dim DownloadVerzeichnisVersion As String
                Dim AktuelleVersion As String
                Dim NeueVersion As String

                'Zuweisungen
                FTPLink = "http://pckiste.bplaced.net/AutoUpdate/Version.txt" 'FTP-DownloadLink zur Text-Datei mit evtl. neuer Version
                DownloadVerzeichnisVersion = My.Application.Info.DirectoryPath & "\Temp\Version.txt" 'System.Text.Encoding.GetEncoding(1252) 'Wohin die Text-Datei heruntergeladen werden soll
                AktuelleVersion = Application.ProductVersion 'Die derzeit verwendete Version

                'Download der Version.txt
                My.Computer.Network.DownloadFile(FTPLink, DownloadVerzeichnisVersion, "", "", False, 3000, True) 'Download der Text-Datei (Adresse, Ziel, Benutzername, Passwort, UI, Timeout, Overwrite) lädt alle Dateien nicht nur binäre

                'Einlesen der Version.txt in RTB1
                Dim LeseVerzeichnis As String = My.Application.Info.DirectoryPath & "\Temp\Version.txt"
                Dim SRText As String
                Using SR As New System.IO.StreamReader(LeseVerzeichnis)
                    SRText = SR.ReadToEnd()
                End Using
                RichTextBox1.Text = SRText

                'Schreiben in PrüfenVersion.txt aus RTB1
                Dim Schreiben As String = My.Application.Info.DirectoryPath & "\Temp\PrüfenVersion.txt"
                Using SW As New System.IO.StreamWriter(Schreiben, False, System.Text.Encoding.GetEncoding(1252)) '(True = Anfügen der Daten / False = Überschreiben der Daten)
                    SW.Write(SRText)
                End Using

                'Auf Version prüfen
                INI.Pfad = My.Application.Info.DirectoryPath & "\Temp\PrüfenVersion.txt" 'Pfad zum Auslesen der Text-Datei (hier wird die Klasse "INIDatei" verwendet - siehe ältere Videos)
                NeueVersion = INI.WertLesen("Info", "Version", "") 'Liest Version aus

                Select Case True 'Vergleicht Version
                    Case AktuelleVersion = NeueVersion 'Gleiche Version
                        MsgBox("Sie verwenden bereits die neuste Version.", MsgBoxStyle.Information, "ABC")
                    Case AktuelleVersion < NeueVersion 'Alte Version auf PC
                        Dim Result As MsgBoxResult
                        Result = MsgBox("Es ist eine neue Version (" & NeueVersion & ") verfügbar. Soll diese heruntergeladen werden?", MsgBoxStyle.Information Or MsgBoxStyle.YesNo, "ABC")

                        If Result = MsgBoxResult.Yes = True Then
                            Dim DownloadLink As String 'Downloadlink zu Update.exe
                            Dim DownloadVerzeichnisUpdate As String 'Zielpfad, wohin die Update.exe heruntergeladen werden soll

                            DownloadLink = INI.WertLesen("Info", "DownloadLink", "") 'Liest DownloadLinks aus
                            DownloadVerzeichnisUpdate = My.Application.Info.DirectoryPath & "\Temp\Update.exe" 'Gibt Zielpfad an

                            'Schaltet Progressbar visible
                            ProgressBar1.Value = 0
                            ProgressBar1.Maximum = 100
                            ProgressBar1.Visible = True
                            Label4.Visible = True

                            HTTPClient = New WebClient 'HttpClient (lädt nur binäre Dateien)
                            Try
                                HTTPClient.DownloadFileAsync(New Uri(DownloadLink), DownloadVerzeichnisUpdate) 'Lädt Update herunter (asynchron, damit Anzeige in ProgressBar möglich ist - MultiThreading)
                            Catch ex As Exception
                                MsgBox(ex.Message)
                            End Try
                        Else
                            Exit Sub
                        End If
                    Case Else 'Nicht definierter Zustand
                        Exit Sub
                End Select
            Else
                MsgBox("Ihr PC hat derzeit keinen Internetzugriff.", MsgBoxStyle.Exclamation, "ABC")
                Exit Sub
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub HTTPClient_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles HTTPClient.DownloadFileCompleted
        'Wenn Download fertig
        Try
            MsgBox("Das Update wurde erfolgreich heruntergeladen. Der Installer wird nun gestartet. Folgen Sie den Anweisungen auf dem Bildschirm.", MsgBoxStyle.Information, "ABC")

            Dim UpdateStarten As String
            UpdateStarten = My.Application.Info.DirectoryPath & "\Temp\Update.exe"

            Process.Start(UpdateStarten) 'Ausführen des Updates
            Me.Dispose() 'Beendet das Programm für das Update
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "TextCrypter")
        End Try
    End Sub

    Private Sub HTTPClient_DownloadProgressChanged(ByVal sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs) Handles HTTPClient.DownloadProgressChanged
        'Progressbar
        With ProgressBar1 'Berechnet Anzeige für Fortschritt
            .Value = e.ProgressPercentage

            Dim TotalBytes As Long = e.TotalBytesToReceive / 1024 'Fragt Gesamtgröße (Bytes) ab, die es zu empfangen gilt (/1024 um KiloBytes zu erhalten)
            Dim Bytes As Long = e.BytesReceived / 1024 'Fragt Bytes ab, die schon angekommen sind
            If TotalBytes < 1 Then TotalBytes = 1 'Wenn kleiner 1 Byte, dann 1 Byte
            If Bytes < 1 Then Bytes = 1 'Wenn kleiner 1 Byte, dann 1 Byte

            Label4.Text = Bytes.ToString & " KB von " & TotalBytes.ToString & " KB" 'Anzeige im Label

            ProgressBar1.Refresh() 'Refresh für ProgressBar
        End With
    End Sub

#End Region

End Class