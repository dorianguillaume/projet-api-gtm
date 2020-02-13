using System;
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
    public class ClientsController : ControllerBase
    {
        private readonly GrandHotelContext _context;

        public ClientsController(GrandHotelContext context)
        {
            _context = context;
        }

        // GET: grandhotel/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClient()
        {
            return await _context.Client.ToListAsync();
        }

        /// <summary>
        /// GET: grandhotel/Clients/5
        /// </summary>
        /// <param name="id">Id du client</param>
        /// <returns>Retourn les informations d'un client avec le détail de son Adresse et de son/ses numéros de téléphones</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Client.Include(c => c.Adresse).Include(c => c.Telephone).FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        // PUT: grandhotel/Clients/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, Client client)
        {
            if (id != client.Id)
            {
                return BadRequest();
            }


            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: grandhotel/Clients
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            _context.Client.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = client.Id }, client);
        }

        [HttpPost("{id}/telephone")]
        public async Task<ActionResult> PostTelephone(int id, Telephone telephone)
        {
            //On vérifie que l'idée passé en paramètre est similaire à celui de la requête
            if (id != telephone.IdClient)
            {
                return BadRequest("Erreur ID client entre la requête et l'url");
            }

            _context.Telephone.Add(telephone);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                var sqle = e.InnerException as SqlException;

                if (sqle.Number == 2627)
                {
                    return BadRequest("Le numéro de téléphone existe déjà dans la base");
                }
                return BadRequest("Erreur lors de l'ajout du numéro de téléphone dans la base");
            }

            return Ok("Le numéro de téléphone créé est le suivant : " + telephone.Numero);
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Client>> DeleteClient(int id)
        {
            var client = await _context.Client.Include(c => c.Reservation).FirstOrDefaultAsync(c => c.Id == id);


            if (client == null)
            {
                return NotFound("Aucun client identifié par ce numéro n'a pu être trouvé");
            }
            else if (client.Reservation.Any())
            {
                return BadRequest("Impossible de supprimer ce client (Des commandes lui sont rattachées).");
            }
            else
            {
                //In récupère adresse et téléphones
                var adresse = await _context.Adresse.Where(a => a.IdClient == id).FirstOrDefaultAsync();
                var telephone = await _context.Telephone.Where(t => t.IdClient == id).ToListAsync();

                //SI telephone et numéro non vide on supprime l'.les entitée.s
                if (telephone.Any())
                {
                    _context.Telephone.RemoveRange(telephone);
                }
                if (adresse != null)
                {
                    _context.Adresse.Remove(adresse);
                }

                _context.Client.Remove(client);

                await _context.SaveChangesAsync();

                return client;
            }

        }

        private bool ClientExists(int id)
        {
            return _context.Client.Any(e => e.Id == id);
        }
    }
}
