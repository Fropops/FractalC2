using Common.APIModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;

namespace TeamServer.Controllers
{
    public class LootController : Controller
    {
        private readonly ILootService _lootService;

        public LootController(ILootService lootService)
        {
            _lootService = lootService;
        }

        /// <summary>
        /// Récupère tous les loots d'un agent
        /// </summary>
        /// <param name="agentId">ID de l'agent</param>
        /// <param name="includeData">Inclure les données complètes des fichiers (base64)</param>
        /// <param name="includeThumbnail">Inclure les thumbnails des images</param>
        /// <returns>Liste des loots</returns>
        [HttpGet("loot/{agentId}")]
        [ProducesResponseType(typeof(List<Loot>), 200)]
        public async Task<IActionResult> GetAgentLoots(
            string agentId,
            [FromQuery] bool includeData = false,
            [FromQuery] bool includeThumbnail = true)
        {
            try
            {
                var loots = await _lootService.GetAgentLootsAsync(agentId, includeData, includeThumbnail);
                return Ok(loots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des loots", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère un loot spécifique d'un agent
        /// </summary>
        /// <param name="agentId">ID de l'agent</param>
        /// <param name="fileName">Nom du fichier</param>
        /// <param name="includeData">Inclure les données complètes du fichier (base64)</param>
        /// <param name="includeThumbnail">Inclure le thumbnail si c'est une image</param>
        /// <returns>Le loot demandé</returns>
        [HttpGet("loot/{agentId}/{fileName}")]
        [ProducesResponseType(typeof(Loot), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFile(
            string agentId,
            string fileName,
            [FromQuery] bool includeData = true,
            [FromQuery] bool includeThumbnail = true)
        {
            try
            {
                var loot = await _lootService.GetFileAsync(agentId, fileName, includeData, includeThumbnail);

                if (loot == null)
                    return NotFound(new { message = $"Fichier '{fileName}' non trouvé pour l'agent '{agentId}'" });

                return Ok(loot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération du fichier", error = ex.Message });
            }
        }

      
        /// <summary>
        /// Ajoute un fichier à partir de données base64
        /// </summary>
        /// <param name="agentId">ID de l'agent</param>
        /// <param name="request">Requête contenant le nom du fichier et les données en base64</param>
        /// <returns>Confirmation de l'ajout</returns>
        [HttpPost("loot/{agentId}/add")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddFile(string agentId, [FromBody] Loot loot)
        {
            try
            {
                if (string.IsNullOrEmpty(loot.FileName))
                    return BadRequest(new { message = "Nom de fichier requis" });

                if (string.IsNullOrEmpty(loot.Data))
                    return BadRequest(new { message = "Données du fichier requises" });

                await _lootService.AddFileAsync(agentId, loot.FileName, Convert.FromBase64String(loot.Data));

                return CreatedAtAction(
                    nameof(GetFile),
                    new { agentId, fileName = loot.FileName },
                    new { message = "Fichier ajouté avec succès", agentId, fileName = loot.FileName }
                );
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Format base64 invalide" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de l'ajout du fichier", error = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un fichier d'un agent
        /// </summary>
        /// <param name="agentId">ID de l'agent</param>
        /// <param name="fileName">Nom du fichier à supprimer</param>
        /// <returns>Confirmation de la suppression</returns>
        [HttpDelete("loot/{agentId}/{fileName}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFile(string agentId, string fileName)
        {
            try
            {
                var deleted = await _lootService.DeleteFileAsync(agentId, fileName);

                if (!deleted)
                    return NotFound(new { message = $"Fichier '{fileName}' non trouvé pour l'agent '{agentId}'" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la suppression du fichier", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère le thumbnail d'une image
        /// </summary>
        /// <param name="agentId">ID de l'agent</param>
        /// <param name="fileName">Nom du fichier image</param>
        /// <returns>Le thumbnail en base64 ou le fichier image directement</returns>
        [HttpGet("loot/{agentId}/thumbnail/{fileName}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetThumbnail(string agentId, string fileName)
        {
            try
            {
                var loot = await _lootService.GetFileAsync(agentId, fileName, includeData: false, includeThumbnail: true);

                if (loot == null)
                    return NotFound(new { message = $"Fichier '{fileName}' non trouvé pour l'agent '{agentId}'" });

                if (!loot.IsImage)
                    return BadRequest(new { message = "Le fichier n'est pas une image" });

                if (string.IsNullOrEmpty(loot.ThumbnailData))
                    return NotFound(new { message = "Thumbnail non disponible" });

                var thumbnailBytes = Convert.FromBase64String(loot.ThumbnailData);
                return File(thumbnailBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération du thumbnail", error = ex.Message });
            }
        }
    }
}
