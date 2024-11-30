using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkListCreator.Model
{
    public class Tile
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public TileType Type { get; set; } = TileType.Single;
        public List<Link> Links { get; set; } = new();
        public string ImageUrl { get; set; } = string.Empty;
        public bool BorderRadiusActive { get; set; } = false;
        public int BorderRadiusPercentage { get; set; } = 10;
    }
}
