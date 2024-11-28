using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkListCreator.Model
{
    public class LinkList
    {
        public List<Tile> Tiles { get; set; } = new();
        public Settings Settings { get; set; } = new();
    }
}
