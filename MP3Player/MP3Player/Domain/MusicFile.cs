using System;
using System.Collections.Generic;
using System.Text;

namespace MP3Player.Domain
{
    public class MusicFile
    {
        public Guid Id { get; set; }
        public string Author { get; set; }
        public string SongName { get; set; }
        public List<byte> AudioFile { get; set; } = new List<byte>();
    }
}
