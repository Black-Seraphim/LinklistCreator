using LinkListCreator.Model;

namespace LinkListCreator.Tools
{
    internal static class Seed
    {
        /// <summary>
        /// Seed the tiles with some default values.
        /// </summary>
        public static List<Tile> SeedTiles()
        {
            return new List<Tile>()
            {
                 new() {
                    Title = "Google",
                    Type = TileType.Multi,
                    Url = "https://www.google.com",
                    ImageUrl = "google_search.png",
                    Links = new List<Link>()
                    {
                        new() {
                            Title = "Search",
                            Url = "https://www.google.com"
                        },
                        new() {
                            Title = "Contacts",
                            Url = "https://contacts.google.com"
                        },
                        new() {
                            Title = "Drive",
                            Url = "https://drive.google.com"
                        },
                        new() {
                            Title = "Maps",
                            Url = "https://maps.google.com"
                        },
                        new() {
                            Title = "Calendar",
                            Url = "https://calendar.google.com"
                        }
                    }
                },
                new() {
                    Title = "Streaming",
                    Type = TileType.Multi,
                    Url = "https://www.netflix.com",
                    ImageUrl = "streaming.png",
                    Links = new List<Link>()
                    {
                        new() {
                            Title = "Netflix",
                            Url = "https://www.netflix.com"
                        },
                        new() {
                            Title = "YouTube",
                            Url = "https://www.youtube.com"
                        },
                        new() {
                            Title = "Disney+",
                            Url = "https://www.disneyplus.com"
                        }
                    }
                },
                new() {
                    Title = "Wikipedia",
                    Type = TileType.Single,
                    Url = "https://de.wikipedia.org",
                    ImageUrl = "wikipedia.png",
                    Links = new List<Link>()
                },
                new() {
                    Title = "ChatGPT",
                    Type = TileType.Single,
                    Url = "https://chat.com",
                    ImageUrl = "openai.png",
                    BorderRadiusActive = true,
                    BorderRadiusPercentage = "10",
                    Links = new List<Link>()
                },
                new() {
                    Title = "Translate",
                    Type = TileType.Multi,
                    Url = "https://www.deepl.com/de/translator",
                    ImageUrl = "translate.png",
                    Links = new List<Link>()
                    {
                        new() {
                            Title = "DeepL",
                            Url = "https://www.deepl.com/de/translator"
                        },
                        new() {
                            Title = "Google Translate",
                            Url = "https://translate.google.com/"
                        },
                        new() {
                            Title = "leo.org",
                            Url = "https://www.leo.org/englisch-deutsch"
                        }
                    }
                }
            };
        }
    }
}
