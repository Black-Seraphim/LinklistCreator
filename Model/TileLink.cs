using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkListCreator.Model
{
    public class TileLink
    {
        public int TileId { get; set; }
        public int LinkId { get; set; }
        public int Order { get; set; }

        public List<Tile> Tiles { get; set; } = new();
        public List<Link> Links { get; set; } = new();

    }
}
