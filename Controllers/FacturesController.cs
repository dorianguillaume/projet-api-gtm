﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using projetAPI_GTM.Models;

namespace projetAPI_GTM.Controllers
{
    [Route("grandhotel/[controller]")]
    [ApiController]
    public class FacturesController : ControllerBase
    {
        private readonly GrandHotelContext _context;

        public FacturesController(GrandHotelContext context)
        {
            _context = context;
        }

        // GET ALL : GENERATION ENTITY
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facture>>> GetFacture()
        {
            return await _context.Facture.ToListAsync();
        }

        /// <summary>
        /// 2. Obtenir une facture identifiée par son Id, avec son détail
        /// </summary>
        /// <param name="id">Id du Client</param>
        /// <returns>Facture avec LigneFacture</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Facture>> GetFacture(int id)
        {
            var facture = await _context.Facture.Include(f => f.LigneFacture).FirstOrDefaultAsync(f => f.Id == id);

            if (facture == null)
            {
                return NotFound("Aucune facture n'est enregistré avec cet identifiant");
            }
            return facture;
        }

        /// <summary>
        /// 1. Obtenir la liste des factures d’un client
        /// à partir d’une date donnée ou plusieurs (par défaut sur un an glissant) 
        /// sans leurs détails 
        /// </summary>
        /// <param name="id">Id du Client</param>
        /// <param name="date1">DateTime</param>
        /// <param name="date2">DateTime</param>
        /// <returns>List Facture</returns>
        [HttpGet("client/{id}")]
        public async Task<ActionResult<IEnumerable<Facture>>> GetFacture(int id, [FromQuery] DateTime date1, [FromQuery] DateTime date2)
        {
            var factures = new List<Facture>();

            //SI Aucune date n'est renseigné on renvoie la liste de l'ensemble des factures du client
            if (date1 == DateTime.MinValue && date2 == DateTime.MinValue)
            {
                factures = await _context.Facture.Where(f => f.IdClient == id).ToListAsync();
            }
            //SI uniquement date 1 --> facture client - 1 an glissant de date1
            else if (date1 == DateTime.MinValue)
            {
                factures = await _context.Facture.Where(f => f.IdClient == id).Where(f => f.DateFacture <= date2 && f.DateFacture >= f.DateFacture.AddYears(-1)).ToListAsync();
            }
            //SI uniquement date 2 --> facture client - 1 an glissant de date2
            else if (date2 == DateTime.MinValue)
            {
                factures = await _context.Facture.Where(f => f.IdClient == id).Where(f => f.DateFacture <= date1 && f.DateFacture >= f.DateFacture.AddYears(-1)).ToListAsync();
            }
            //SINON factures d'un client comprisent entre date1 et 2
            else
            {
                factures = await _context.Facture.Where(f => f.IdClient == id).Where(f => f.DateFacture >= date1 && f.DateFacture <= date2).ToListAsync();
            }

            return factures;
        }

        // PUT: GENERATION ENTITY
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFacture(int id, Facture facture)
        {
            if (id != facture.Id)
            {
                return BadRequest();
            }

            _context.Entry(facture).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FactureExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// 5. Mettre à jour la date et le mode de paiement d'une facture
        /// Modification Date et Mode de Paiement UNIQUEMENT d'une facture
        /// (On peut ajouter d'autres éléments dans la requête mais ils ne seront pas modifiés)
        /// </summary>
        /// <param name="id">Id Facture</param>
        /// <param name="facture">Facture</param>
        /// <returns>Requête OK quand valide</returns>
        [HttpPut("{id}/paiement")]
        public async Task<IActionResult> PutFacturePaiement(int id, Facture facture)
        {
            if (id != facture.Id)
            {
                return BadRequest("L'ID de l'URL ne correspond pas à celui de du corps de la requête.");
            }     
            
            _context.Attach(facture);
            _context.Entry(facture).Property("DatePaiement").IsModified = true;
            _context.Entry(facture).Property("CodeModePaiement").IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                var sqle = e.InnerException as SqlException;

                if (sqle.Number == 8152)
                {
                    return BadRequest("Le mode de paiement n'est pas valide (CB - CHQ - ESP)");
                }
                else return BadRequest("Erreur lors de la mise à jour de la facture");
            }
            return Ok("Le mode de paiement et la date de votre facture ont bien été modifiés");
        }

        /// <summary>
        /// 3. Créer une facture
        /// </summary>
        /// <param name="facture">Facture</param>
        /// <returns>Facture créée</returns>
        [HttpPost]
        public async Task<ActionResult<Facture>> PostFacture(Facture facture)
        {
            _context.Facture.Add(facture);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFacture", new { id = facture.Id }, facture);
        }

        /// <summary>
        /// 4. Ajouter une ligne à une facture donnée
        /// (incrémentation manuelle car pas auto-incrémenté)
        /// </summary>
        /// <param name="id">Id Facture</param>
        /// <param name="lignefacture">LigneFacture</param>
        /// <returns>Requête OK si la ligne est créé</returns>
        [HttpPost("{id}/lignefacture")]
        public async Task<ActionResult> PostLigneFacture(int id, LigneFacture lignefacture)
        {
            //On vérifie que l'id passé en paramètre est similaire à celui de la requête
            if (id != lignefacture.IdFacture)
            {
                return BadRequest("Erreur ID facture entre la requête et l'url");
            }

            _context.LigneFacture.Add(lignefacture);

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateException e)
            {
                var sqle = e.InnerException as SqlException;

                if (sqle.Number == 2627)
                {
                    return BadRequest("Le numéro de ligne de facture existe déjà dans la base");
                }
                return BadRequest("Erreur lors de l'ajout de la ligne de facture dans la base");
            }

            return Ok("Le numéro de la ligne de facture créée est le suivant : " + lignefacture.NumLigne);
        }

        // DELETE: GENERATION ENTITY
        [HttpDelete("{id}")]
        public async Task<ActionResult<Facture>> DeleteFacture(int id)
        {
            var facture = await _context.Facture.FindAsync(id);
            if (facture == null)
            {
                return NotFound();
            }

            _context.Facture.Remove(facture);
            await _context.SaveChangesAsync();

            return facture;
        }

        private bool FactureExists(int id)
        {
            return _context.Facture.Any(e => e.Id == id);
        }
    }
}

