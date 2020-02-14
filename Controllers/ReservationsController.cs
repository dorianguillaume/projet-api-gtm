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
    public class ReservationsController : ControllerBase
    {
        private readonly GrandHotelContext _context;

        public ReservationsController(GrandHotelContext context)
        {
            _context = context;
        }

        // GET ALL : GENERATION ENTITY
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservation()
        {
            return await _context.Reservation.ToListAsync();
        }

        /// <summary>
        /// 1. Obtenir les réservations pour une date donnée
        /// </summary>
        /// <param name="date">DateTime</param>
        /// <returns>List Reservation</returns>
        [HttpGet("dateresa/")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservation([FromQuery] DateTime date)
        {

            if (date == DateTime.MinValue)
            {
                return BadRequest("Aucun paramètre date n'a été renseigné dans la requête");
            }

            var reservations = new List<Reservation>();
            reservations = await _context.Reservation.Where(r => r.Jour == date).ToListAsync();

            if (!reservations.Any())
            {
                return NotFound("Aucune réservation n'a été enregistré à cette date");
            } else return reservations;
        }

        // GET: ENTITY GENERATION
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(short id)
        {
            var reservation = await _context.Reservation.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        /// <summary>
        /// 2. Obtenir l’historique des réservations d’un client
        /// </summary>
        /// <param name="id">Id du Client</param>
        /// <returns>List de Reservation</returns>
        [HttpGet("client/{id}")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservation(int id)
        {
            var reservations = new List<Reservation>();

            reservations = await _context.Reservation.Where(r => r.IdClient == id).ToListAsync();

            if (!reservations.Any())
            {
                return NotFound("Ce client n'a effectué aucune réservation");
            }else return reservations;
        }

        /// <summary>
        /// 3.B - Mofidication d'une réservation en fonction de sa date et de sa chambre
        /// (On ne peut pas modifier date de réservation et chambre)
        /// </summary>
        /// <param name="id">NumChambre</param>
        /// <param name="date">DateTime --> Date de réservation</param>
        /// <param name="reservation">Reservation (modifications)</param>
        /// <returns>Requête ok si modif accépter</returns>
        [HttpPut("client/{id}")]
        public async Task<IActionResult> PutReservation(short id, Reservation reservation)
        {
            if (id != reservation.IdClient)
            {
                return BadRequest("Il y a une erreur entre les paramètre passer dans l'URL et dans le corps de la requête");
            }

            //On récupère la potentiel réservation pour s'assurer qu'elle existe
            var reservationAModifer = await _context.Reservation.Where(r => r.IdClient == id && r.NumChambre == reservation.NumChambre && r.Jour == reservation.Jour).FirstOrDefaultAsync();
            if (reservationAModifer == null)
            {
                return NotFound("Aucune réservation n'est enregistrée pour cette chambre à cette date");
            }
            else
            {

                reservationAModifer.NbPersonnes = reservation.NbPersonnes;
                reservationAModifer.HeureArrivee = reservation.HeureArrivee;
                reservationAModifer.Travail = reservation.Travail;

                _context.Entry(reservationAModifer).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();   
                }
                catch (DbUpdateException e)
                {
                    var sqle = e.InnerException as SqlException;

                    return BadRequest("Erreur lors de la modification de la reservation : "+sqle.Number);
                }
            
               return Ok("Votre reservation a été modifiée avec succès");
            }


        }

        /// <summary>
        /// 3.A - Ajout d'une réservation
        /// </summary>
        /// <param name="reservation">Reservation</param>
        /// <returns>Reservation créée</returns>
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            _context.Reservation.Add(reservation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                var sqle = e.InnerException as SqlException;

                if (sqle.Number == 2627)
                {
                    return BadRequest("Cette chambre est déjà attribuée à cette date");
                }
                return BadRequest("Erreur lors de l'ajout de la reservation "+sqle.Number);
            }

            return CreatedAtAction("GetReservation", new { id = reservation.NumChambre }, reservation);
        }

        /// <summary>
        /// 3.C - Suppression d'une réservation
        /// </summary>
        /// <param name="id">NumChambre</param>
        /// <param name="date">DateTime de la Reservation</param>
        /// <returns>Reservation supprimée</returns>
        // DELETE: api/Reservations/5
        [HttpDelete("chambre/{id}/date/{date}")]
        public async Task<ActionResult<Reservation>> DeleteReservation(short id, DateTime date)
        {
            var reservation = await _context.Reservation.Where(r => r.Jour == date && r.NumChambre == id).FirstOrDefaultAsync();

            if (reservation == null)
            {
                return NotFound("Aucune réservation n'est enregistré à cette date pour cette chambre");
            }


            _context.Reservation.Remove(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        private bool ReservationExists(short id)
        {
            return _context.Reservation.Any(e => e.NumChambre == id);
        }
    }
}
