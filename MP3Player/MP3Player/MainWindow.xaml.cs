using MP3Player.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;

namespace MP3Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string IP_ADDRESS = "10.1.4.41";
        private const int PORT = 3231;
        private List<MusicFileDto> musicFiles;
        private MediaPlayer player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            UpdateList();
            playList.ItemsSource = musicFiles;
        }

        private async void LoadBtnClick(object sender, RoutedEventArgs e)
        {
            var path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Musics";
            using (var client = new TcpClient())
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT));
                using (var stream = client.GetStream())
                {
                    var data = Encoding.UTF8.GetBytes(musicFiles[playList.SelectedIndex].Id.ToString());
                    stream.Write(data, 0, data.Length);
                    var resultStringBuilder = new StringBuilder();

                    var buffer = new byte[1024];
                    stream.Read(buffer, 0, buffer.Length);
                    resultStringBuilder.Append(Encoding.UTF8.GetString(buffer));

                    Directory.CreateDirectory(path);
                    var selectedMusic = JsonConvert.DeserializeObject<MusicFile>(resultStringBuilder.ToString());
                    
                    path += $@"\{selectedMusic.Author}-{selectedMusic.SongName}.mp3";
                    using (var sw = new StreamWriter(path, true, Encoding.Default))
                    {
                        await sw.WriteLineAsync(resultStringBuilder.ToString());
                    }
                }
            }
            player.Open(new Uri(path, UriKind.RelativeOrAbsolute));
            player.Play();
        }

        private void UpdateList()
        {
            using (var client = new TcpClient())
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT));
                using (var stream = client.GetStream())
                {
                    var jsonResultStringBuilder = new StringBuilder();
                    while (stream.DataAvailable)
                    {
                        var buffer = new byte[1024];
                        stream.Read(buffer, 0, buffer.Length);
                        jsonResultStringBuilder.Append(Encoding.UTF8.GetString(buffer));
                    }
                    musicFiles = JsonConvert.DeserializeObject<List<MusicFileDto>>(jsonResultStringBuilder.ToString());
                }
            }
        }

        private void UpdateBtnClick(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }
    }
}
