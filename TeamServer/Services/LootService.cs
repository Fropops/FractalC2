using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using Common.APIModels;
using Common.Config;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;
using TeamServer.Services;
using TeamServer.Helper;

[InjectableService]
public interface ILootService
{
    Task AddFileAsync(string agentId, string fileName, byte[] fileData);
    Task<bool> DeleteFileAsync(string agentId, string fileName);
    Task<Loot> GetFileAsync(string agentId, string fileName, bool includeData = false, bool includeThumbnail = true);
    Task<List<Loot>> GetAgentLootsAsync(string agentId, bool includeData = false, bool includeThumbnail = true);
    string GetAgentFilePath(string agentId, string fileName);
    string GetAgentPath(string agentId);
}

[InjectableServiceImplementation(typeof(ILootService))]
public class LootService : ILootService
{
    private readonly FoldersConfig _foldersConfig;
    private readonly ConcurrentDictionary<string, AgentLootCache> _cache;
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico" };
    private const int CacheExpirationSeconds = 30;
    private const int ThumbnailMaxSize = 100;

    public LootService(IConfiguration configuration)
    {
        _foldersConfig = configuration.FoldersConfigs();
        _cache = new ConcurrentDictionary<string, AgentLootCache>();
    }

    public async Task AddFileAsync(string agentId, string fileName, byte[] fileData)
    {
        var agentPath = GetAgentPath(agentId);
        Directory.CreateDirectory(agentPath);

        var filePath = GetAgentFilePath(agentId, fileName);
        await File.WriteAllBytesAsync(filePath, fileData);

        // Invalider le cache pour cet agent
        InvalidateCache(agentId);
    }

    public async Task<bool> DeleteFileAsync(string agentId, string fileName)
    {
        var filePath = GetAgentFilePath(agentId, fileName);

        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);

        // Supprimer du cache
        if (_cache.TryGetValue(agentId, out var cache))
        {
            cache.Loots.RemoveAll(l => l.FileName == fileName);
        }

        return true;
    }

    public async Task<Loot> GetFileAsync(string agentId, string fileName, bool includeData = false, bool includeThumbnail = true)
    {
        // Vérifier et rafraîchir le cache si nécessaire
        await RefreshCacheIfNeededAsync(agentId);

        if (!_cache.TryGetValue(agentId, out var cache))
            return null;

        var loot = cache.Loots.FirstOrDefault(l => l.FileName == fileName);
        if (loot == null)
            return null;

        // Générer le thumbnail si nécessaire
        if (includeThumbnail && loot.IsImage == "true" && string.IsNullOrEmpty(loot.ThumbnailData))
        {
            await GenerateThumbnailAsync(agentId, loot);
        }

        // Charger les données si nécessaire
        if (includeData && string.IsNullOrEmpty(loot.Data))
        {
            await LoadFileDataAsync(agentId, loot);
        }

        // Retourner une copie pour éviter les modifications du cache
        return new Loot
        {
            FileName = loot.FileName,
            AgentId = loot.AgentId,
            IsImage = loot.IsImage,
            ThumbnailData = includeThumbnail ? loot.ThumbnailData : null,
            Data = includeData ? loot.Data : null
        };
    }

    public async Task<List<Loot>> GetAgentLootsAsync(string agentId, bool includeData = false, bool includeThumbnail = true)
    {
        await RefreshCacheIfNeededAsync(agentId);

        if (!_cache.TryGetValue(agentId, out var cache))
            return new List<Loot>();

        var result = new List<Loot>();

        foreach (var loot in cache.Loots)
        {
            // Générer le thumbnail si nécessaire
            if (includeThumbnail && loot.IsImage == "true" && string.IsNullOrEmpty(loot.ThumbnailData))
            {
                await GenerateThumbnailAsync(agentId, loot);
            }

            // Charger les données si nécessaire
            if (includeData && string.IsNullOrEmpty(loot.Data))
            {
                await LoadFileDataAsync(agentId, loot);
            }

            result.Add(new Loot
            {
                FileName = loot.FileName,
                AgentId = loot.AgentId,
                IsImage = loot.IsImage,
                ThumbnailData = includeThumbnail ? loot.ThumbnailData : null,
                Data = includeData ? loot.Data : null
            });
        }

        return result;
    }

    public string GetAgentFilePath(string agentId, string fileName)
    {
        return Path.Combine(_foldersConfig.WorkingFolder, "Loot", agentId, fileName);
    }

    public string GetAgentPath(string agentId)
    {
        return Path.Combine(_foldersConfig.WorkingFolder, "Loot", agentId);
    }

    private async Task RefreshCacheIfNeededAsync(string agentId)
    {
        var now = DateTime.UtcNow;

        if (_cache.TryGetValue(agentId, out var cache))
        {
            // Vérifier si le cache est encore valide
            if ((now - cache.LastRefresh).TotalSeconds < CacheExpirationSeconds)
                return;
        }

        await RefreshCacheAsync(agentId);
    }

    private async Task RefreshCacheAsync(string agentId)
    {
        var agentPath = GetAgentPath(agentId);

        if (!Directory.Exists(agentPath))
        {
            _cache[agentId] = new AgentLootCache
            {
                LastRefresh = DateTime.UtcNow,
                Loots = new List<Loot>()
            };
            return;
        }

        var files = Directory.GetFiles(agentPath);
        var fileNames = files.Select(Path.GetFileName).ToHashSet();

        // Récupérer ou créer le cache
        if (!_cache.TryGetValue(agentId, out var cache))
        {
            cache = new AgentLootCache
            {
                LastRefresh = DateTime.UtcNow,
                Loots = new List<Loot>()
            };
            _cache[agentId] = cache;
        }

        // Supprimer les fichiers qui n'existent plus
        cache.Loots.RemoveAll(l => !fileNames.Contains(l.FileName));

        // Ajouter les nouveaux fichiers
        var existingFileNames = cache.Loots.Select(l => l.FileName).ToHashSet();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            if (!existingFileNames.Contains(fileName))
            {
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var isImage = ImageExtensions.Contains(extension);

                cache.Loots.Add(new Loot
                {
                    FileName = fileName,
                    AgentId = agentId,
                    IsImage = isImage ? "true" : "false",
                    ThumbnailData = null,
                    Data = null
                });
            }
        }

        cache.LastRefresh = DateTime.UtcNow;
    }

    private async Task GenerateThumbnailAsync(string agentId, Loot loot)
    {
        try
        {
            var filePath = GetAgentFilePath(agentId, loot.FileName);

            if (!File.Exists(filePath))
                return;

            // Utilisation de ImageSharp (plus moderne et cross-platform)
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(filePath);

            // Calculer les nouvelles dimensions
            int width, height;
            if (image.Width > image.Height)
            {
                width = ThumbnailMaxSize;
                height = (int)((double)image.Height / image.Width * ThumbnailMaxSize);
            }
            else
            {
                height = ThumbnailMaxSize;
                width = (int)((double)image.Width / image.Height * ThumbnailMaxSize);
            }

            // Redimensionner l'image
            image.Mutate(x => x.Resize(width, height));

            // Convertir en base64
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            loot.ThumbnailData = Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception)
        {
            // En cas d'erreur (fichier corrompu, etc.), on ignore
            loot.ThumbnailData = string.Empty;
        }
    }

    private async Task LoadFileDataAsync(string agentId, Loot loot)
    {
        try
        {
            var filePath = GetAgentFilePath(agentId, loot.FileName);

            if (!File.Exists(filePath))
                return;

            var fileData = await File.ReadAllBytesAsync(filePath);
            loot.Data = Convert.ToBase64String(fileData);
        }
        catch (Exception)
        {
            loot.Data = string.Empty;
        }
    }

    private void InvalidateCache(string agentId)
    {
        if (_cache.TryGetValue(agentId, out var cache))
        {
            // Forcer un rafraîchissement au prochain accès
            cache.LastRefresh = DateTime.MinValue;
        }
    }

    private class AgentLootCache
    {
        public DateTime LastRefresh { get; set; }
        public List<Loot> Loots { get; set; }
    }
}
