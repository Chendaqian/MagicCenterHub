Add-Type -AssemblyName PresentationFramework

$sounds = @(
    "Default", "IM", "Mail", "Reminder", "SMS", "Silent",
    "Alarm", "Alarm2", "Alarm3", "Alarm4", "Alarm5",
    "Alarm6", "Alarm7", "Alarm8", "Alarm9", "Alarm10",
    "Call", "Call1", "Call2", "Call3", "Call4",
    "Call5", "Call6", "Call7", "Call8", "Call9", "Call10"
)

[xml]$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="BurntToast Sound Test" Width="500" Height="640"
        WindowStartupLocation="CenterScreen" Background="#1E1E22"
        ResizeMode="NoResize">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        <TextBlock Text="点击按钮试听通知音效" FontFamily="Segoe UI" FontSize="16"
                   FontWeight="Bold" Foreground="#7DD4D4" Margin="0,0,0,12"/>
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <WrapPanel x:Name="SoundPanel" Orientation="Horizontal"/>
        </ScrollViewer>
    </Grid>
</Window>
"@

$reader = New-Object System.Xml.XmlNodeReader $xaml
$window = [Windows.Markup.XamlReader]::Load($reader)

$panel = $window.FindName("SoundPanel")

foreach ($sound in $sounds) {
    $btn = New-Object System.Windows.Controls.Button
    $btn.Content = $sound
    $btn.Margin = "4"
    $btn.Padding = "12,6"
    $btn.FontFamily = "Segoe UI"
    $btn.FontSize = 12
    $btn.Background = "#3C3C42"
    $btn.Foreground = "#E0E0E0"
    $btn.BorderBrush = "#555"
    $btn.Cursor = [System.Windows.Input.Cursors]::Hand

    # 闭包捕获
    $s = $sound
    $btn.Add_Click({
        Import-Module BurntToast -ErrorAction SilentlyContinue
        New-BurntToastNotification -Text "Sound Test", "当前音效: $s" -Sound $s
    }.GetNewClosure())

    $panel.Children.Add($btn) | Out-Null
}

$window.ShowDialog() | Out-Null
