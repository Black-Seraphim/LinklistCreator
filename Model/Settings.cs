using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkListCreator.Model
{
    public class Settings
    {
        public string LinklistName { get; set; } = "Linklist";
        public string SelectedMainBgColor { get; set; } = "#303030";
        public string SelectedTileBgColor { get; set; } = "#505050";
        public string SelectedTileBgHoverColor { get; set; } = "#1a3b4d";
        public string SelectedLinkBgColor { get; set; } = "#101010";
        public string SelectedLinkBgHoverColor { get; set; } = "#303030";
        public string SelectedButtonBgColor { get; set; } = "#303030";
        public string SelectedLinkTextColor { get; set; } = "#a0a0a0";
        public int TileWidthPx { get; set; } = 200;
    }
}
