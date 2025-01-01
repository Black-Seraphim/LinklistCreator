using LinkListCreator.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using LinkListCreator.Services;
using System.IO;
using Microsoft.JSInterop;
using System.Text.Json;
using System.IO.Compression;
using System.Diagnostics;
using System.Globalization;
using LinkListCreator.Tools;

namespace LinkListCreator
{
    public partial class Creator
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private IFileDialogService FileDialogService { get; set; } = default!;

        [Inject]
        private FileUploadService FileUploadService { get; set; } = default!;

        public Settings Settings { get; set; } = new();
        public List<Tile> Tiles { get; set; } = new();
        public List<Link> Links { get; set; } = new();
        public Tile? SelectedTile { get; set; }
        public Link? SelectedLink { get; set; }
        private IBrowserFile? SelectedFile { get; set; }

        public string TileOptionsVisibility { get; set; } = "";
        public string LinkOptionsVisibility { get; set; } = "hidden";
        public string RemoveTileVisibility { get; set; } = "hidden";
        public string RemoveLinkVisibility { get; set; } = "hidden";
        public string AddLinkVisibility { get; set; } = "hidden";
        public string MenuVisibility { get; set; } = "hidden";

        /// <summary>
        /// On component initialization, the tiles are seeded.
        /// </summary>
        protected override void OnInitialized()
        {
            Tiles = Seed.SeedTiles();
        }

        /// <summary>
        /// Update the CSS variable in settings and browser.
        /// </summary>
        /// <param name="value">css value name to update</param>
        /// <param name="e">event that contains the updated value</param>
        /// <returns></returns>
        private async Task UpdateCssVariable(string value, ChangeEventArgs e)
        {
            if (e.Value is not string stringValue) { return; }

            switch (value)
            {
                case "--main-bg-color":
                    Settings.SelectedMainBgColor = stringValue;
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--main-bg-color", Settings.SelectedMainBgColor);
                    break;
                case "--tile-bg-color":
                    Settings.SelectedTileBgColor = stringValue;
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-color", Settings.SelectedTileBgColor);
                    break;
                case "--tile-bg-hover-color":
                    Settings.SelectedTileBgHoverColor = stringValue;
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-hover-color", Settings.SelectedTileBgHoverColor);
                    break;
                case "--link-bg-color":
                    Settings.SelectedLinkBgColor = stringValue;
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-color", Settings.SelectedLinkBgColor);
                    break;
                case "--link-bg-hover-color":
                    Settings.SelectedLinkBgHoverColor = stringValue;
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-hover-color", Settings.SelectedLinkBgHoverColor);
                    break;
                case "--button-bg-color":
                    Settings.SelectedButtonBgColor = stringValue;
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--button-bg-color", Settings.SelectedButtonBgColor);
                    break;
                case "--link-text-color":
                    Settings.SelectedLinkTextColor = stringValue;
                    await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-text-color", Settings.SelectedLinkTextColor);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Refresh all CSS variables in the browser.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshCssVariables()
        {
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--main-bg-color", Settings.SelectedMainBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-color", Settings.SelectedTileBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--tile-bg-hover-color", Settings.SelectedTileBgHoverColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-color", Settings.SelectedLinkBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-bg-hover-color", Settings.SelectedLinkBgHoverColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--button-bg-color", Settings.SelectedButtonBgColor);
            await JSRuntime.InvokeVoidAsync("updateCssVariable", "--link-text-color", Settings.SelectedLinkTextColor);
        }

        /// <summary>
        /// On button click, visibility of the menu is toggled.
        /// </summary>
        public void ButtonMenu_OnClick()
        {
            MenuVisibility = (MenuVisibility == "hidden") ? "visible" : "hidden";
        }

        /// <summary>
        /// On button click, a file dialog is opened to load a linklist.
        /// After loading the linklist, all tiles and settings are updated.
        /// Also, the CSS variables are updated in the browser.
        /// Menu is closed after loading the linklist.
        /// </summary>
        /// <returns></returns>
        public async Task ButtonLoad_OnClickAsync()
        {
            LinkList? linkList = LoadLinkList();

            if (linkList == null) { return; }

            Tiles = linkList.Tiles;
            Settings = linkList.Settings;

            await RefreshCssVariables();

            MenuVisibility = "hidden";
        }

        /// <summary>
        /// Open a file dialog to load and deserialize a linklist-json.
        /// </summary>
        /// <returns></returns>
        private LinkList? LoadLinkList()
        {
            string? filePath = FileDialogService.GetOpenFilePath();

            if (filePath == null) { return null; }

            string jsonContent = File.ReadAllText(filePath);

            LinkList? linkList = JsonSerializer.Deserialize<LinkList>(jsonContent);

            return linkList;
        }

        /// <summary>
        /// On button click, the linklist is saved in JSON format.
        /// Menu is closed after saving the linklist.
        /// </summary>
        public void ButtonSave_OnClick()
        {
            LinkList linkList = new()
            {
                Tiles = Tiles,
                Settings = Settings
            };

            StoreLinkList(linkList);

            MenuVisibility = "hidden";
        }

        /// <summary>
        /// Store the linklist as a JSON file.
        /// Therefore, a file dialog is opened to choose the file location.
        /// </summary>
        /// <param name="linkList">linklist to be safed as JSON</param>
        private void StoreLinkList(LinkList linkList)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(linkList, options);

            string? filePath = FileDialogService.GetSaveFilePath($"{Settings.LinklistName.ToLower()}.linklist");

            if (filePath == null) { return; }

            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// On button click, the linklist is created and stored in a zip package.
        /// After storing the linklist, the linklist folder is opened in the explorer.
        /// Menu is closed after creating the linklist.
        /// </summary>
        public void ButtonCreate_OnClick()
        {
            StoreFiles();

            CreateLinklistZipPackage();

            Process.Start("explorer.exe", Path.Combine(AppContext.BaseDirectory, "Linklists", Settings.LinklistName));

            MenuVisibility = "hidden";
        }

        /// <summary>
        /// Store all necessary files for the linklist.
        /// </summary>
        private void StoreFiles()
        {
            StoreLinklistHtml();

            StoreLinklistCss();

            StoreDefaultCss();

            StoreDefaultFonts();

            StoreImages();
        }

        /// <summary>
        /// Create and prettify the HTML string for the linklist.
        /// Then, store the HTML file in the linklist folder.
        /// </summary>
        private void StoreLinklistHtml()
        {
            string linklistHtml = CreateHtmlLinklist();
            linklistHtml = Prettify.PrettifyHtml(linklistHtml);

            string htmlPath = Path.Combine("Linklists", Settings.LinklistName);
            Directory.CreateDirectory(htmlPath);
            string htmlFile = Path.Combine(htmlPath, "index.html");
            File.WriteAllText(htmlFile, linklistHtml);
        }

        /// <summary>
        /// Create and prettify the CSS string for the linklist.
        /// Then, store the CSS file in the linklist folder.
        /// </summary>
        private void StoreLinklistCss()
        {
            string linklistCss = CreateCssLinklist();
            linklistCss = Prettify.PrettifyCss(linklistCss);

            string cssPath = Path.Combine("Linklists", Settings.LinklistName, "css");
            Directory.CreateDirectory(cssPath);
            string cssFile = Path.Combine(cssPath, $"{Settings.LinklistName.ToLower()}.css");
            File.WriteAllText(cssFile, linklistCss);
        }

        /// <summary>
        /// Store the default CSS file in the linklist folder.
        /// </summary>
        private void StoreDefaultCss()
        {
            File.Copy("files/styles.css", Path.Combine("Linklists", Settings.LinklistName, "css", "styles.css"), true);
        }

        /// <summary>
        /// Store the default fonts in the linklist folder.
        /// </summary>
        private void StoreDefaultFonts()
        {
            string fontPath = Path.Combine("Linklists", Settings.LinklistName, "font");
            Directory.CreateDirectory(fontPath);
            File.Copy("files/Roboto-Medium.ttf", Path.Combine(fontPath, "Roboto-Medium.ttf"), true);
        }

        /// <summary>
        /// Store the images for each tile in the linklist folder.
        /// If no image is set, a default image is used.
        /// If the image does not exist, it is skipped.
        /// </summary>
        private void StoreImages()
        {
            string imagePath = Path.Combine("Linklists", Settings.LinklistName, "img");
            string sourcePath = Path.Combine("wwwroot", "images");

            if (!Directory.Exists(imagePath)) 
            { 
                Directory.CreateDirectory(imagePath); 
            }

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

        /// <summary>
        /// Create a zip package for the linklist folder.
        /// </summary>
        private void CreateLinklistZipPackage()
        {
            string zipPath = Path.Combine("Linklists", $"{Settings.LinklistName}.zip");
            string sourcePath = Path.Combine("Linklists", Settings.LinklistName);

            if (File.Exists(zipPath)) { File.Delete(zipPath); }

            using ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);

            archive.CreateEntry(sourcePath);
        }

        /// <summary>
        /// Create the CSS string for the linklist.
        /// </summary>
        /// <returns></returns>
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
            cssBuilder.AppendLine($"--tile-width: {(Settings.TileWidthPx / 10.0).ToString(cultureInfo)}rem;");
            cssBuilder.AppendLine($"");

            cssBuilder.AppendLine($"--tile-shadow-color-hover: #000000;");
            cssBuilder.AppendLine($"--tile-shadow-color-1: rgba(0, 0, 0, 0.3);");
            cssBuilder.AppendLine($"--tile-shadow-color-2: rgba(0, 0, 0, 0.36);");
            cssBuilder.AppendLine($"--tile-shadow-color-3: rgba(0, 0, 0, 0.38);");
            cssBuilder.AppendLine($"}}");
            cssBuilder.AppendLine($"");

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

        /// <summary>
        /// Create the HTML string for the linklist.
        /// </summary>
        /// <returns></returns>
        private string CreateHtmlLinklist()
        {
            string defaultCssPath = "css/styles.css";
            string linklistCssPath = $"css/{Settings.LinklistName.ToLower()}.css";

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

        /// <summary>
        /// Create the HTML string for a single tile.
        /// </summary>
        /// <param name="tile">tile to create html from</param>
        /// <returns>html tile string</returns>
        private string CreateHtmlSingleTile(Tile tile)
        {
            string imgPath = "img";
            string imgUrl = (string.IsNullOrEmpty(tile.ImageUrl)) ? "phoenix.png" : tile.ImageUrl;
            string fullImgUrl = Path.Combine(imgPath, imgUrl);

            string target = Settings.OpenInNewWindow ? " target=\"_blank\"" : "";

            StringBuilder htmlBuilder = new();

            htmlBuilder.AppendLine($"<div class=\"tile single tile-{tile.Title.ToLower()}\">");
            htmlBuilder.AppendLine($"<a href=\"{tile.Url}\"{target}>");
            htmlBuilder.AppendLine($"<div><img src=\"{fullImgUrl}\" alt=\"{tile.ImageUrl}\"></div>");
            htmlBuilder.AppendLine($"<div><span>{tile.Title}</span></div>");
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"</div>");

            return htmlBuilder.ToString();
        }

        /// <summary>
        /// Create the HTML string for a multi tile.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        private string CreateHtmlMultiTile(Tile tile)
        {
            string imgPath = "img";
            string imgUrl = (string.IsNullOrEmpty(tile.ImageUrl)) ? "phoenix.png" : tile.ImageUrl;
            string fullImgUrl = Path.Combine(imgPath, imgUrl);

            string target = Settings.OpenInNewWindow ? " target=\"_blank\"" : "";

            StringBuilder htmlBuilder = new();

            htmlBuilder.AppendLine($"<div class=\"tile multi tile-{tile.Title.ToLower()}\">");
            htmlBuilder.AppendLine($"<a href=\"{tile.Url}\"{target}>");
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
                htmlBuilder.AppendLine($"<li><a href=\"{link.Url}\"{target}><span>{link.Title}</span></a></li>");
            }
            htmlBuilder.AppendLine($"</ul>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");

            return htmlBuilder.ToString();
        }

        /// <summary>
        /// On tile click, the tile is selected.
        /// </summary>
        /// <param name="tile">tile to select</param>
        public void RowTile_OnClick(Tile tile)
        {
            SelectTile(tile);
        }

        /// <summary>
        /// Select the tile, update the links, and show the tile options.
        /// Unslect the link and hide the link options.
        /// </summary>
        /// <param name="tile">tile to select</param>
        public void SelectTile(Tile tile)
        {
            SelectedTile = tile;
            Links = SelectedTile.Links;
            TileOptionsVisibility = "";

            SelectedLink = null;
            LinkOptionsVisibility = "hidden";
        }

        /// <summary>
        /// On link click, the link is selected.
        /// </summary>
        /// <param name="link">link to select</param>
        public void RowLink_OnSelect(Link link)
        {
            SelectLink(link);
        }

        /// <summary>
        /// Select the link and show the link options.
        /// Hide the tile options.
        /// </summary>
        /// <param name="link"></param>
        public void SelectLink(Link link)
        {
            SelectedLink = link;
            LinkOptionsVisibility = "";

            TileOptionsVisibility = "hidden";
        }

        /// <summary>
        /// On button click, a new tile is added to the linklist.
        /// </summary>
        public void ButtonAddTile_OnClick()
        {
            AddTile();
        }

        /// <summary>
        /// Add a new tile to the linklist.
        /// Select the new tile.
        /// </summary>
        private void AddTile()
        {
            Tile tile = new()
            {
                Title = "New Tile",
                Type = TileType.Single,
                Url = "#",
                Links = new()
            };

            Tiles.Add(tile);
            SelectTile(tile);
        }

        /// <summary>
        /// On button click, the selected tile is removed from the linklist.
        /// </summary>
        public void ButtonRemoveTile_OnClick()
        {
            RemoveTile();
        }

        /// <summary>
        /// If a tile is selected, the tile is removed from the linklist.
        /// Selected tile is set to null.
        /// Links are reset.
        /// </summary>
        public void RemoveTile()
        {
            if (SelectedTile == null) { return; }

            Tiles.Remove(SelectedTile);

            SelectedTile = null;
            Links = new();
        }

        /// <summary>
        /// On button click, a new link is added to the selected tile.
        /// </summary>
        public void ButtonAddLink_OnClick()
        {
            AddLink();
        }

        /// <summary>
        /// Add a new link to the selected tile.
        /// Select the new link.
        /// </summary>
        public void AddLink()
        {
            Link link = new()
            {
                Title = "New Link",
                Url = "#",
            };

            Links.Add(link);
            SelectLink(link);
        }

        /// <summary>
        /// On button click, the selected link is removed from the selected tile.
        /// </summary>
        public void ButtonRemoveLink_OnClick()
        {
            RemoveLink();
        }

        /// <summary>
        /// If a link is selected, the link is removed from the selected tile.
        /// Selected link is set to null.
        /// </summary>
        public void RemoveLink()
        {
            if (SelectedLink == null) { return; }

            Links.Remove(SelectedLink);

            SelectedLink = null;
        }

        /// <summary>
        /// Get the image path for the selected tile.
        /// </summary>
        /// <returns>image path string</returns>
        public string GetTileImage()
        {
            string phoenixPath = Path.Combine("images", "phoenix.png");
            if (SelectedTile == null) { return phoenixPath; }

            string fileName = $"{SelectedTile.ImageUrl}";
            string filePath = Path.Combine("images", fileName);

            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot", filePath))) { return filePath; }

            return phoenixPath;
        }

        /// <summary>
        /// On input file change, the selected file is uploaded and the image path is updated.
        /// </summary>
        /// <param name="e">event that contains the selected file</param>
        /// <returns></returns>
        private async Task InputFileImage_OnChange(InputFileChangeEventArgs e)
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
            catch
            {
                return;
            }
        }
    }
}
