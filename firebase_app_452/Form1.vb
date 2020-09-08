Imports uPLibrary.Networking.M2Mqtt
Imports uPLibrary.Networking.M2Mqtt.Messages
Imports System.Text
Imports System.Threading
Imports System.ComponentModel

Public Class Form1
    Dim client As MqttClient
    Dim relay1 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}} 'mảng 2 hàng 4 cột, gtrị cao nhất ko vượt quá 2^16
    Dim relay2 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}}
    Dim relay3 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}}
    Dim relay4 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}}
    Dim relay5 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}}
    Dim relay6 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}}
    Dim relay7 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}}
    Dim relay8 = New String(3, 1) {{"0", "0"}, {"0", "0"}, {"0", "0"}, {"0", "0"}}
    Dim sensor = New String() {0, 0} 'mảng có 2 phần tử giá trị cao nhất không vượt qua 255
    Dim buffer = New Byte() {0, 0}

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False 'tránh lỗi biên dịch

        Dim caCert As New Security.Cryptography.X509Certificates.X509Certificate  'tạo khoá an toàn cho ca và client
        Dim clientCert As New Security.Cryptography.X509Certificates.X509Certificate
        Try
            client = New MqttClient("mqtt.ngoinhaiot.com", 1111, False, caCert, clientCert, 0) 'secure As Boolean = 0 // MqttSslProtocols = 0,
            Dim clientId As String = Guid.NewGuid().ToString()

            AddHandler client.MqttMsgPublishReceived, AddressOf Client_MqttMsgPublishReceived
            AddHandler client.ConnectionClosed, AddressOf Client_Disconnect

            client.Connect(clientId, "tung_thanh_nguyen", "aJVISmwF")
            'If client.IsConnected Then
            'ToolStripStatusLabel2.Text = "Trạng thái sẵn sàng"
            'End If
        Catch
            MsgBox("Không có mạng internet. Vui lòng kiểm tra internet và khởi động lại phần mềm",, "Lỗi")
            Me.Close()
        End Try
        client.Subscribe({"tung_thanh_nguyen/#"}, {0})
        ToolStripStatusLabel1.Text = Date.Now.ToLongTimeString()

    End Sub
    Private Sub Client_Disconnect(sender As Object, e As EventArgs)
        MsgBox("Xác nhận tắt phần mềm và ngắt kết nối đến máy chủ?",, "Đang tắt phần mềm")
    End Sub

    Private Sub Client_MqttMsgPublishReceived(ByVal sender As Object, ByVal e As MqttMsgPublishEventArgs)
        Dim topic() As Char = e.Topic.ToCharArray()
        Dim data As String = Encoding.Default.GetString(e.Message)
        analyse(topic, data)
        Label6.Text = sensor(0) & " độ C" 'hien thi nhiet do
        Label7.Text = sensor(1) & " %" 'hien thi do am

        '----------------------------------hien thi hinh anh den on/off
        If relay1(0, 0) = 1 Then
            Display_led_1.Image = My.Resources.light_on
        Else
            Display_led_1.Image = My.Resources.light_off
        End If

        If relay2(0, 0) = 1 Then
            Display_led_2.Image = My.Resources.light_on
        Else
            Display_led_2.Image = My.Resources.light_off
        End If

        If relay3(0, 0) = 1 Then
            Display_led_3.Image = My.Resources.light_on
        Else
            Display_led_3.Image = My.Resources.light_off
        End If

        If relay4(0, 0) = 1 Then
            Display_led_4.Image = My.Resources.light_on
        Else
            Display_led_4.Image = My.Resources.light_off
        End If

        If relay5(0, 0) = 1 Then
            Display_led_5.Image = My.Resources.light_on
        Else
            Display_led_5.Image = My.Resources.light_off
        End If

        If relay6(0, 0) = 1 Then
            Display_led_6.Image = My.Resources.light_on
        Else
            Display_led_6.Image = My.Resources.light_off
        End If

        If relay7(0, 0) = 1 Then
            Display_led_7.Image = My.Resources.light_on
        Else
            Display_led_7.Image = My.Resources.light_off
        End If

        If relay8(0, 0) = 1 Then
            Display_led_8.Image = My.Resources.light_on
        Else
            Display_led_8.Image = My.Resources.light_off
        End If

        state_1.Value = relay1(0, 0)  'hien thi trang thai tai cac switch
        state_2.Value = relay2(0, 0)
        state_3.Value = relay3(0, 0)
        state_4.Value = relay4(0, 0)
        state_5.Value = relay5(0, 0)
        state_6.Value = relay6(0, 0)
        state_7.Value = relay7(0, 0)
        state_8.Value = relay8(0, 0)

        Select Case select_led_to_show.SelectedIndex 'hiển thị các giá trị thời gian đặt
            Case 0
                show_time(relay1)
            Case 1
                show_time(relay2)
            Case 2
                show_time(relay3)
            Case 3
                show_time(relay4)
            Case 4
                show_time(relay5)
            Case 5
                show_time(relay6)
            Case 6
                show_time(relay7)
            Case 7
                show_time(relay8)
        End Select
        ToolStripStatusLabel2.ForeColor = Color.Blue
        ToolStripStatusLabel2.Text = "Đã đồng bộ dữ liệu với các thiết bị khác"
    End Sub

    Private Sub analyse(tp() As Char, dt As String) 'phân tích giá trị đường dẫn và data
        Dim flag As Byte = 0
        For i = 0 To tp.Length() - 1
            If IsNumeric(tp(i)) Then
                buffer(flag) = Char.GetNumericValue(tp(i)) 'đổi dữ liệu kiểu char sang số
                flag = flag + 1
            End If
        Next
        '-----------------------------------------ghi hết data vào các mảng-----------------------------
        Select Case buffer(0)
            Case 1
                relay1(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 2
                relay2(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 3
                relay3(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 4
                relay4(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 5
                relay5(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 6
                relay6(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 7
                relay7(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 8
                relay8(buffer(1) \ 2, buffer(1) Mod 2) = dt
            Case 9
                sensor(buffer(1)) = dt
        End Select

        '-------------------------------------------đã ghi xong----------------------------------------------------
    End Sub

    '----------------------------------------cho phép thay đổi giá trị cài thời gian--------------------------------------
    Private Sub allow_change_schedule_CheckedChanged(sender As Object, e As EventArgs) Handles allow_change_schedule.CheckedChanged
        If allow_change_schedule.Checked = False Then
            time_start_1.Enabled = False
            time_start_2.Enabled = False
            time_start_3.Enabled = False
            time_stop_1.Enabled = False
            time_stop_2.Enabled = False
            time_stop_3.Enabled = False
        Else
            time_start_1.Enabled = True
            time_start_2.Enabled = True
            time_start_3.Enabled = True
            time_stop_1.Enabled = True
            time_stop_2.Enabled = True
            time_stop_3.Enabled = True
        End If
    End Sub
    '-------------------------------------------------------end-----------------------------------------------------

    '-----------------------------------------cho phép bật tắt  manual----------------------------------------
    Private Sub disable_all_schedule_CheckedChanged(sender As Object, e As EventArgs) Handles disable_all_schedule.CheckedChanged
        If disable_all_schedule.Checked = True Then
            state_1.Enabled = True
            state_2.Enabled = True
            state_3.Enabled = True
            state_4.Enabled = True
            state_5.Enabled = True
            state_6.Enabled = True
            state_7.Enabled = True
            state_8.Enabled = True
        Else
            state_1.Enabled = False
            state_2.Enabled = False
            state_3.Enabled = False
            state_4.Enabled = False
            state_5.Enabled = False
            state_6.Enabled = False
            state_7.Enabled = False
            state_8.Enabled = False
        End If
    End Sub
    '-------------------------------------------------------end------------------------------------------------

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click 'tạm thời để thế này. sau lưu vào gg sheet
        Process.Start("https://thingspeak.com/channels/1116371")
    End Sub
    '--------------------------------------------------------------end----------------------------------------

    '------------------------------------kiểm tra giá trị thời gian nhập tránh lỗi--------------------
    Private Sub time_stop_1_DropDownClosed(sender As Object, e As EventArgs) Handles time_stop_1.DropDownClosed
        If (time_stop_1.SelectedIndex <= time_start_1.SelectedIndex) Then
            time_stop_1.SelectedIndex = -1
            MsgBox("Thời gian tắt đèn không được nhỏ hơn thời gian bật đèn. Mời lựa chọn lại.",, "Lỗi xảy ra")
        End If
    End Sub

    Private Sub time_stop_2_DropDownClosed(sender As Object, e As EventArgs) Handles time_stop_2.DropDownClosed
        If (time_stop_2.SelectedIndex <= time_start_2.SelectedIndex) Then
            time_stop_2.SelectedIndex = -1
            MsgBox("Thời gian tắt đèn không được nhỏ hơn thời gian bật đèn. Mời lựa chọn lại.",, "Lỗi xảy ra")
        End If
    End Sub

    Private Sub time_stop_3_DropDownClosed(sender As Object, e As EventArgs) Handles time_stop_3.DropDownClosed
        If (time_stop_3.SelectedIndex <= time_start_3.SelectedIndex) Then
            time_stop_3.SelectedIndex = -1
            MsgBox("Thời gian tắt đèn không được nhỏ hơn thời gian bật đèn. Mời lựa chọn lại.",, "Lỗi xảy ra")
        End If
    End Sub

    Private Sub time_start_2_DropDownClosed(sender As Object, e As EventArgs) Handles time_start_2.DropDownClosed
        If (time_start_2.SelectedIndex <= time_stop_1.SelectedIndex) Then
            time_start_2.SelectedIndex = -1
            MsgBox("Thời gian tắt/bật đèn bị trùng. Mời lựa chọn lại.",, "Lỗi xảy ra")
        End If
    End Sub

    Private Sub time_start_3_DropDownClosed(sender As Object, e As EventArgs) Handles time_start_3.DropDownClosed
        If (time_start_3.SelectedIndex <= time_stop_2.SelectedIndex) Then
            time_start_3.SelectedIndex = -1
            MsgBox("Thời gian tắt/bật đèn bị trùng. Mời lựa chọn lại.",, "Lỗi xảy ra")
        End If
    End Sub

    '--------------------------------------kết thúc kiểm tra-------------------------------------------------


    '-------------------------save giá trị vào mây-----------------------------------------------------------
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ToolStripStatusLabel2.ForeColor = Color.Red
        ToolStripStatusLabel2.Text = "Đang lưu giá trị! Nếu quá lâu phần mềm không phản hồi vui lòng kiểm tra lại kết nối internet."

        Select Case select_led_to_show.SelectedIndex
            Case 0
                get_time(relay1, 1)
            Case 1
                get_time(relay2, 2)
            Case 2
                get_time(relay3, 3)
            Case 3
                get_time(relay4, 4)
            Case 4
                get_time(relay5, 5)
            Case 5
                get_time(relay6, 6)
            Case 6
                get_time(relay7, 7)
            Case 7
                get_time(relay8, 8)
            Case -1
                ToolStripStatusLabel2.Text = "Chưa chọn cổng led!"
        End Select
    End Sub
    '--------------------------------end save-----------------------------------------------------------
    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        client.Disconnect()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        ToolStripStatusLabel1.Text = Date.Now.ToLongTimeString() & "                       "

        Chart1.Series("Series1").Points.AddXY(Date.Now.ToLongTimeString(), sensor(0))
        Chart1.Update()

        Chart2.Series("Series1").Points.AddXY(Date.Now.ToLongTimeString(), sensor(1))
        Chart2.Update()

        Chart3.Series("Series1").Points.AddXY(Date.Now.ToLongTimeString(), relay1(0, 1))
        Chart3.Update()

        Chart4.Series("Series1").Points.AddXY(Date.Now.ToLongTimeString(), relay2(0, 1))
        Chart4.Update()
    End Sub

    Private Sub select_led_to_show_SelectedIndexChanged(sender As Object, e As EventArgs) Handles select_led_to_show.SelectedIndexChanged
        Select Case select_led_to_show.SelectedIndex 'hiển thị các giá trị thời gian đặt
            Case 0
                show_time(relay1)
            Case 1
                show_time(relay2)
            Case 2
                show_time(relay3)
            Case 3
                show_time(relay4)
            Case 4
                show_time(relay5)
            Case 5
                show_time(relay6)
            Case 6
                show_time(relay7)
            Case 7
                show_time(relay8)
        End Select
    End Sub
    Private Sub show_time(arr(,) As String) 'arr(,) truyền mảng 2 chiều vào hàm
        time_start_1.SelectedIndex = arr(1, 0) - 1
        time_start_2.SelectedIndex = arr(2, 0) - 1
        time_start_3.SelectedIndex = arr(3, 0) - 1
        time_stop_1.SelectedIndex = arr(1, 1) - 1
        time_stop_2.SelectedIndex = arr(2, 1) - 1
        time_stop_3.SelectedIndex = arr(3, 1) - 1
    End Sub 'hiện thời gian đặt
    Private Sub get_time(ByRef arr(,) As String, num As Byte)
        arr(1, 0) = time_start_1.SelectedIndex + 1
        arr(2, 0) = time_start_2.SelectedIndex + 1
        arr(3, 0) = time_start_3.SelectedIndex + 1
        arr(1, 1) = time_stop_1.SelectedIndex + 1
        arr(2, 1) = time_stop_2.SelectedIndex + 1
        arr(3, 1) = time_stop_3.SelectedIndex + 1
        For i = 1 To 3
            For j = 0 To 1
                client.Publish("tung_thanh_nguyen/" & num & "/" & (i * 2 + j), Encoding.Default.GetBytes(arr(i, j)), 0, True)
            Next
        Next
    End Sub 'lưu và đẩy thời gian đặt

    '-----------------------------------bật tắt manual--------------------------
    Private Sub state_1_ValueChanged(sender As Object, e As EventArgs) Handles state_1.ValueChanged
        client.Publish("tung_thanh_nguyen/1/0", Encoding.Default.GetBytes(state_1.Value), 0, True)
    End Sub

    Private Sub state_2_ValueChanged(sender As Object, e As EventArgs) Handles state_2.ValueChanged
        client.Publish("tung_thanh_nguyen/2/0", Encoding.Default.GetBytes(state_2.Value), 0, True)
    End Sub

    Private Sub state_3_ValueChanged(sender As Object, e As EventArgs) Handles state_3.ValueChanged
        client.Publish("tung_thanh_nguyen/3/0", Encoding.Default.GetBytes(state_3.Value), 0, True)
    End Sub

    Private Sub state_4_ValueChanged(sender As Object, e As EventArgs) Handles state_4.ValueChanged
        client.Publish("tung_thanh_nguyen/4/0", Encoding.Default.GetBytes(state_4.Value), 0, True)
    End Sub

    Private Sub state_5_ValueChanged(sender As Object, e As EventArgs) Handles state_5.ValueChanged
        client.Publish("tung_thanh_nguyen/5/0", Encoding.Default.GetBytes(state_5.Value), 0, True)
    End Sub

    Private Sub state_6_ValueChanged(sender As Object, e As EventArgs) Handles state_6.ValueChanged
        client.Publish("tung_thanh_nguyen/6/0", Encoding.Default.GetBytes(state_6.Value), 0, True)
    End Sub

    Private Sub state_7_ValueChanged(sender As Object, e As EventArgs) Handles state_7.ValueChanged
        client.Publish("tung_thanh_nguyen/7/0", Encoding.Default.GetBytes(state_7.Value), 0, True)
    End Sub

    Private Sub state_8_ValueChanged(sender As Object, e As EventArgs) Handles state_8.ValueChanged
        client.Publish("tung_thanh_nguyen/8/0", Encoding.Default.GetBytes(state_8.Value), 0, True)
    End Sub


    '----------------------------------------------------------------------------------------
End Class
