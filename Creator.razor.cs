using LinkListCreator.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkListCreator.Services;
using System.IO;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO.Compression;
using System.Diagnostics;
using System.Globalization;
using AngleSharp.Html.Parser;
using AngleSharp.Html;
using AngleSharp.Css.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using AngleSharp;
using AngleSharp.Css;

namespace LinkListCreator
{
    public partial class Creator
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private IFileDialogService FileDialogService { get; set; } = default!;

        public List<Tile> Tiles { get; set; } = new();
        public List<Link> Links { get; set; } = new();
        public Tile? SelectedTile { get; set; }
        public Link? SelectedLink { get; set; }

        public string TileOptionsVisibility { get; set; } = "";
        public string LinkOptionsVisibility { get; set; } = "hidden";
        public string RemoveTileVisibility { get; set; } = "hidden";
        public string RemoveLinkVisibility { get; set; } = "hidden";
        public string AddLinkVisibility { get; set; } = "hidden";
        public string MenuVisibility { get; set; } = "hidden";

        public Settings Settings { get; set; } = new();


        private ElementReference? InputFileRef { get; set; }
        private IBrowserFile? SelectedFile { get; set; }
        [Inject]
        private FileUploadService FileUploadService { get; set; } = default!;

        private async Task OnFileSelected(InputFileChangeEventArgs e)
        {
            SelectedFile = e.File;

            if (SelectedFile == null) return;

            if (SelectedTile == null) return;

            try
            {
                string fileName = $"{SelectedFile.Name}";
                await FileUploadService.SaveFileAsync(SelectedFile, fileName);
                SelectedTile.ImageUrl = fileName;
            }
            catch (Exception)
            {

            }
        }

        public void ButtonMenu_OnClick()
        {
            MenuVisibility = (MenuVisibility == "hidden") ? "visible" : "hidden";
        }

        public async Task ButtonLoad_OnClickAsync()
        {
            MenuVisibility = "hidden";

            LinkList? linkList = LoadLinkList();

            if (linkList == null) { return; }

            Tiles = linkList.Tiles;
            Settings = linkList.Settings;

            await UpdateAllCssVariables();
        }

        public void ButtonSave_OnClick()
        {
            MenuVisibility = "hidden";

            LinkList linkList = new()
            {
                Tiles = Tiles,
                Settings = Settings
            };

            StoreLinkList(linkList);
        }

        /// <summary>
        /// Store the link list as a JSON file.
        /// Open a file dialog to save the file.
        /// </summary>
        /// <param name="linkList">Linklist to save</param>
        private void StoreLinkList(LinkList linkList)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(linkList, options);

            string? filePath = FileDialogService.GetSaveFilePath($"{Settings.LinklistName.ToLower()}.linklist");

            if (filePath == null) { return; }
            File.WriteAllText(filePath, json);
        }

        private LinkList? LoadLinkList()
        {
            string? filePath = FileDialogService.GetOpenFilePath();

            if (filePath == null) { return null; }

            string jsonContent = File.ReadAllText(filePath);

            LinkList? linkList = JsonSerializer.Deserialize<LinkList>(jsonContent);

            return linkList;
        }


        public void ButtonCreate_OnClick()
        {
            MenuVisibility = "hidden";

            string linklistHtml = CreateHtmlLinklist();

            linklistHtml = PrettifyHtml(linklistHtml);

            string? linklistCss = CreateCssLinklist();

            linklistCss = PrettifyCss(linklistCss);

            if (linklistCss == null) { return; }

            StoreFiles(linklistHtml, linklistCss);

            CreatePackage();

            Process.Start("explorer.exe", Path.Combine(AppContext.BaseDirectory, "Linklists", Settings.LinklistName));
        }

        private void StoreFiles(string linklistHtml, string linklistCss)
        {
            string htmlPath = Path.Combine("Linklists", Settings.LinklistName);
            Directory.CreateDirectory(htmlPath);
            string htmlFile = Path.Combine(htmlPath, "index.html");
            File.WriteAllText(htmlFile, linklistHtml);

            string cssPath = Path.Combine("Linklists", Settings.LinklistName, "css");
            Directory.CreateDirectory(cssPath);
            string cssFile = Path.Combine(cssPath, "linklist.css");
            File.WriteAllText(cssFile, linklistCss);

            File.Copy("files/styles.css", Path.Combine("Linklists", Settings.LinklistName, "css", "styles.css"), true);

            string fontPath = Path.Combine("Linklists", Settings.LinklistName, "font");
            Directory.CreateDirectory(fontPath);
            File.Copy("files/Roboto-Medium.ttf", Path.Combine(fontPath, "Roboto-Medium.ttf"), true);

            CopyImages();
        }

        private void CopyImages()
        {
            string imagePath = Path.Combine("Linklists", Settings.LinklistName, "img");
            string sourcePath = Path.Combine("wwwroot", "images");

            if (!Directory.Exists(imagePath)) { Directory.CreateDirectory(imagePath); }
            foreach (Tile tile in Tiles)
            {
                string fileName = tile.ImageUrl;

                if (string.IsNullOrEmpty(tile.ImageUrl))
                {
                    fileName = "phoenix.png";
                }

                string sourceFilePath = Path.Combine(sourcePath, fileName);
                string targetFilePath = Path.Combine(imagePath, fileName);

                if (!File.Exists(sourceFilePath)) { continue; }

                File.Copy(sourceFilePath, targetFilePath, true);

            }
        }

        private void CreatePackage()
        {
            string linklistName = "Linklist";

            string zipPath = Path.Combine("Linklists", "linklist.zip");
            string sourcePath = Path.Combine("Linklists", linklistName);

            if (File.Exists(zipPath)) { File.Delete(zipPath); }

            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);

            archive.CreateEntry(sourcePath);
        }

        public string PrettifyHtml(string html)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);
            var sw = new StringWriter();
            document.ToHtml(sw, new PrettyMarkupFormatter());
            return sw.ToString();
        }

        public string? PrettifyCss(string css)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(css);
            var sw = new StringWriter();
            stylesheet.ToCss(sw, new PrettyStyleFormatter());
            return sw.ToString();
        }

        private string CreateCssLinklist()
        {
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            StringBuilder cssBuilder = new();

            cssBuilder.AppendLine($":root {{");
            cssBuilder.AppendLine($"--link-text-color: {Settings.SelectedLinkTextColor};");
            cssBuilder.AppendLine($"--main-bg-color: {Settings.SelectedMainBgColor};");
            cssBuilder.AppendLine($"--tile-bg-color: {Settings.SelectedTileBgColor};");
            cssBuilder.AppendLine($"--tile-bg-hover-color: {Settings.SelectedTileBgHoverColor};");
            cssBuilder.AppendLine($"--link-bg-color: {Settings.SelectedLinkBgColor};");
            cssBuilder.AppendLine($"--link-bg-hover-color: {Settings.SelectedLinkBgHoverColor};");
            cssBuilder.AppendLine($"--button-bg-color: {Settings.SelectedButtonBgColor};");
            cssBuilder.AppendLine($"--tile-width: {Settings.TileWidthPx}rem;");

            cssBuilder.AppendLine($"--tile-shadow-color-hover: #000000;");
            cssBuilder.AppendLine($"--tile-shadow-color-1: rgba(0, 0, 0, 0.3);");
            cssBuilder.AppendLine($"--tile-shadow-color-2: rgba(0, 0, 0, 0.36);");
            cssBuilder.AppendLine($"--tile-shadow-color-3: rgba(0, 0, 0, 0.38);");

            cssBuilder.AppendLine($"@media (max-width: 100em){{");
            cssBuilder.AppendLine($"body{{");
            cssBuilder.AppendLine($"--tile-width: {(Settings.TileWidthPx * (3.0 / 4)).ToString(cultureInfo)}rem;");
            cssBuilder.AppendLine($"}}");
            cssBuilder.AppendLine($"}}");

            cssBuilder.AppendLine($"@media (max-width: 75em){{");
            cssBuilder.AppendLine($"body{{");
            cssBuilder.AppendLine($"--tile-width: {(Settings.TileWidthPx * 0.625).ToString(cultureInfo)}rem;");
            cssBuilder.AppendLine($"}}");
            cssBuilder.AppendLine($"}}");

            cssBuilder.AppendLine($"@media (max-width: 45em){{");
            cssBuilder.AppendLine($"body{{");
            cssBuilder.AppendLine($"--tile-width: {(Settings.TileWidthPx * 0.575).ToString(cultureInfo)}rem;");
            cssBuilder.AppendLine($"}}");
            cssBuilder.AppendLine($"}}");


            cssBuilder.AppendLine($"}}");
            cssBuilder.AppendLine($"");

            foreach (Tile tile in Tiles)
            {
                if (!tile.BorderRadiusActive) { continue; }

                cssBuilder.AppendLine($".tile-{tile.Title.ToLower()} img {{");
                cssBuilder.AppendLine($"border-radius: {tile.BorderRadiusPercentage}%;");
                cssBuilder.AppendLine($"}}");
                cssBuilder.AppendLine($"");
            }

            return cssBuilder.ToString();
        }

        private string CreateHtmlLinklist()
        {
            string nameLower = Settings.LinklistName.ToLower();
            string defaultCssPath = "css/styles.css";
            string linklistCssPath = $"css/{nameLower}.css";

            StringBuilder htmlBuilder = new();

            htmlBuilder.AppendLine($"<!DOCTYPE html>");
            htmlBuilder.AppendLine($"<html>");
            htmlBuilder.AppendLine($"");
            htmlBuilder.AppendLine($"<head>");
            htmlBuilder.AppendLine($"<meta charset=\"utf-8\">");
            htmlBuilder.AppendLine($"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            htmlBuilder.AppendLine($"<title>{Settings.LinklistName}</title>");
            htmlBuilder.AppendLine($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{linklistCssPath}\">");
            htmlBuilder.AppendLine($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{defaultCssPath}\">");
            htmlBuilder.AppendLine($"</head>");
            htmlBuilder.AppendLine($"");
            htmlBuilder.AppendLine($"<body>");
            htmlBuilder.AppendLine($"");
            foreach (Tile tile in Tiles)
            {
                if (tile.Type == TileType.Single)
                {
                    htmlBuilder.AppendLine(CreateHtmlSingleTile(tile));
                }
                else
                {
                    htmlBuilder.AppendLine(CreateHtmlMultiTile(tile));
                }
            }
            htmlBuilder.AppendLine($"");
            htmlBuilder.AppendLine($"</body>");
            htmlBuilder.AppendLine($"");
            htmlBuilder.AppendLine($"</html>");
            htmlBuilder.AppendLine($"");

            return htmlBuilder.ToString();
        }

        private string CreateHtmlSingleTile(Tile tile)
        {
            string imgPath = "img";
            string imgUrl = (string.IsNullOrEmpty(tile.ImageUrl)) ? "phoenix.png" : tile.ImageUrl;
            string fullImgUrl = Path.Combine(imgPath, imgUrl);

            StringBuilder htmlBuilder = new();

            htmlBuilder.AppendLine($"<div class=\"tile single tile-{tile.Title.ToLower()}\">");
            htmlBuilder.AppendLine($"<a href=\"{tile.Url}\">");
            htmlBuilder.AppendLine($"<div><img src=\"{fullImgUrl}\" alt=\"{tile.ImageUrl}\"></div>");
            htmlBuilder.AppendLine($"<div><span>{tile.Title}</span></div>");
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"</div>");

            return htmlBuilder.ToString();
        }

        private string CreateHtmlMultiTile(Tile tile)
        {
            string imgPath = "img";
            string imgUrl = (string.IsNullOrEmpty(tile.ImageUrl)) ? "phoenix.png" : tile.ImageUrl;
            string fullImgUrl = Path.Combine(imgPath, imgUrl);

            StringBuilder htmlBuilder = new();

            htmlBuilder.AppendLine($"<div class=\"tile multi tile-{tile.Title.ToLower()}\">");
            htmlBuilder.AppendLine($"<a href=\"{tile.Url}\">");
            htmlBuilder.AppendLine($"<div><img src=\"{fullImgUrl}\" alt=\"{tile.ImageUrl}\"></div>");
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"");
            htmlBuilder.AppendLine($"<div onclick=\"\">");
            htmlBuilder.AppendLine($"<div class=\"link\">");
            htmlBuilder.AppendLine($"<span>{tile.Title}</span>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"");
            htmlBuilder.AppendLine($"<div class=\"button\">");
            htmlBuilder.AppendLine($"<i></i>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"");
            htmlBuilder.AppendLine($"<div class=\"dropdown\">");
            htmlBuilder.AppendLine($"<ul>");
            foreach (Link link in tile.Links)
            {
                htmlBuilder.AppendLine($"<li><a href=\"{link.Url}\"><span>{link.Title}</span></a></li>");
            }
            htmlBuilder.AppendLine($"</ul>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");

            return htmlBuilder.ToString();
        }

        private async Task UpdateAllCssVariables()
        {
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--main-bg-color", Settings.SelectedMainBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-color", Settings.SelectedTileBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-hover-color", Settings.SelectedTileBgHoverColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-color", Settings.SelectedLinkBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-hover-color", Settings.SelectedLinkBgHoverColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--button-bg-color", Settings.SelectedButtonBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-text-color", Settings.SelectedLinkTextColor);
        }

        private async Task UpdateCssVariables(string value, ChangeEventArgs e)
        {
            if (e.Value == null) { return; }

            switch (value)
            {
                case "--main-bg-color":
                    Settings.SelectedMainBgColor = e.Value.ToString();
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--main-bg-color", Settings.SelectedMainBgColor);
                    break;
                case "--tile-bg-color":
                    Settings.SelectedTileBgColor = e.Value.ToString();
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-color", Settings.SelectedTileBgColor);
                    break;
                case "--tile-bg-hover-color":
                    Settings.SelectedTileBgHoverColor = e.Value.ToString();
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-hover-color", Settings.SelectedTileBgHoverColor);
                    break;
                case "--link-bg-color":
                    Settings.SelectedLinkBgColor = e.Value.ToString();
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-color", Settings.SelectedLinkBgColor);
                    break;
                case "--link-bg-hover-color":
                    Settings.SelectedLinkBgHoverColor = e.Value.ToString();
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-hover-color", Settings.SelectedLinkBgHoverColor);
                    break;
                case "--button-bg-color":
                    Settings.SelectedButtonBgColor = e.Value.ToString();
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--button-bg-color", Settings.SelectedButtonBgColor);
                    break;
                case "--link-text-color":
                    Settings.SelectedLinkTextColor = e.Value.ToString();
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-text-color", Settings.SelectedLinkTextColor);
                    break;
                default:
                    break;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Seed();
        }

        public void SelectTile(Tile tile)
        {
            SelectedLink = null;
            SelectedTile = tile;
            Links = SelectedTile.Links;

            TileOptionsVisibility = "";
            LinkOptionsVisibility = "hidden";
        }

        public void SelectLink(Link link)
        {
            SelectedLink = link;
            TileOptionsVisibility = "hidden";
            LinkOptionsVisibility = "";
        }

        public void AddTile()
        {
            Tile tile = new()
            {
                Title = "New Tile",
                Type = TileType.Single,
                Url = "#",
                Order = Tiles.Count + 1,
                Links = new()
            };

            Tiles.Add(tile);
            SelectTile(tile);
        }

        public void RemoveTile()
        {
            if (SelectedTile == null) { return; }

            Tiles.Remove(SelectedTile);

            SelectedTile = null;
            Links = new();

        }

        public void AddLink()
        {
            Link link = new()
            {
                Title = "New Link",
                Url = "#",
                Order = Links.Count + 1
            };

            Links.Add(link);
            SelectLink(link);
        }

        public void RemoveLink()
        {
            if (SelectedLink == null) { return; }

            Links.Remove(SelectedLink);

            SelectedLink = null;
        }

        public string GetTileImage()
        {
            string phoenixPath = Path.Combine("images", "phoenix.png");
            if (SelectedTile == null) { return phoenixPath; }

            string fileName = $"{SelectedTile.ImageUrl}";
            string filePath = Path.Combine("images", fileName);

            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot", filePath))) { return filePath; }

            return phoenixPath;
        }


        public void Seed()
        {
            Tiles.AddRange(new List<Tile>()
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
            });
        }

    }
}
