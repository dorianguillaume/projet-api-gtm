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

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservation()
        {
            return await _context.Reservation.ToListAsync();
        }

        // Obtenir la liste des réservations pour une date donnée
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
            }else return reservations;
           
        }

        // GET: api/Reservations/5
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

        // Obtenir les Réservations d'un client donné avec son id
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
        /// PUT: api/Reservations/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reservation"></param>
        /// <returns></returns>
        [HttpPut("chambre/{id}/date/{date}")]
        public async Task<IActionResult> PutReservation(short id, DateTime date, Reservation reservation)
        {
            var reservationAModifer = await _context.Reservation.Where(r => r.Jour == date && r.NumChambre == id).FirstOrDefaultAsync();

            if (id != reservation.NumChambre || date != reservation.Jour)
            {
                return BadRequest("Il y a une erreur entre les paramètre passer dans l'URL et dans le corps de la requête");
            }

            if (reservationAModifer == null)
            {
                return NotFound("Aucune réservation n'est enregistrée pour cette chambre à cette date");
            }

            _context.Entry(reservation).State = EntityState.Modified;

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

        /// <summary>
        /// Ajout d'une réservation
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        // POST: api/Reservations
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
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
