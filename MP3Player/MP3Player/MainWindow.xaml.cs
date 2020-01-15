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
using System.Threading;

namespace MP3Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string IP_ADDRESS = "127.0.0.1";
        private const int PORT = 3222;
        private List<MusicFileDto> musicFiles;
        private MediaPlayer player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoadBtnClick(object sender, RoutedEventArgs e)
        {
            var selectedMusic = playList.SelectedItem as MusicFileDto;

            if (selectedMusic is null)
            {
                MessageBox.Show("Выберите музыку");
                return;
            }

            var fileName = @$"{selectedMusic.Author}-{selectedMusic.SongName}(mp3p).mp3";
            var musicPosition = new TimeSpan(0);
            if (!File.Exists(fileName))
            {
                using (var client = new TcpClient())
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT));
                    using (var stream = client.GetStream())
                    {
                        var json = JsonConvert.SerializeObject(selectedMusic);
                        var musicInfo = Encoding.UTF8.GetBytes(json);
                        await stream.WriteAsync(musicInfo, 0, musicInfo.Length);
                        var resultStringBuilder = new StringBuilder();

                        var buffer = new byte[1024];
                        File.Create(fileName).Close();
                        do
                        {
                            await stream.ReadAsync(buffer, 0, buffer.Length);
                            using (FileStream fstream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                            {
                                await fstream.WriteAsync(buffer);
                                if (!File.Exists(fileName + "=tmp.mp3"))
                                {
                                    File.Copy(fileName, fileName + "=tmp.mp3");
                                }
                            }
                            if (File.Exists(fileName + "=tmp.mp3"))
                            {
                                player.Open(new Uri(fileName + "=tmp.mp3", UriKind.RelativeOrAbsolute));
                                player.Play();
                            }
                            musicPosition = player.Position;
                        }
                        while (stream.DataAvailable);
                    }
                    File.Delete(fileName + "=tmp.mp3");
                }
            }
            player.Open(new Uri(fileName, UriKind.RelativeOrAbsolute));
            player.Position = musicPosition;
            player.Play();
        }

        private async void UpdateBtnClick(object sender, RoutedEventArgs e)
        {
            using (var client = new TcpClient())
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT));
                using (var stream = client.GetStream())
                {
                    var jsonResultStringBuilder = new StringBuilder();
                    var data = Encoding.UTF8.GetBytes("Update");
                    await stream.WriteAsync(data, 0, data.Length);
                    Thread.Sleep(500);
                    while (stream.DataAvailable)
                    {
                        var buffer = new byte[1024];
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                        jsonResultStringBuilder.Append(Encoding.UTF8.GetString(buffer));
                    }
                    musicFiles = JsonConvert.DeserializeObject<List<MusicFileDto>>(jsonResultStringBuilder.ToString());
                }
            }
            playList.ItemsSource = null;
            playList.ItemsSource = musicFiles;
        }
    }
}
