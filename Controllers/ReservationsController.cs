using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        // Obtenir la liste des réservations pour une date donnée (IL Y A UN SOUCI QUE JE N'AI PAS IDENTIFIE)
        [HttpGet("dateresa/")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservation([FromQuery] DateTime date)
        {
            var reservations = new List<Reservation>();
            reservations = await _context.Reservation.Where(r => r.Jour == date).ToListAsync();

            //SI Aucune date n'est renseignée on renvoie une BadRequest
            if (date == null)
            {
                return BadRequest();
            }

            //Sinon retourner la liste des réservation pour cette date donnée
            else
            {
                return reservations;
            }
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

            return reservations;
        }

        // PUT: api/Reservations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(short id, Reservation reservation)
        {
            if (id != reservation.NumChambre)
            {
                return BadRequest();
            }

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
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
            catch (DbUpdateException)
            {
                if (ReservationExists(reservation.NumChambre))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetReservation", new { id = reservation.NumChambre }, reservation);
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Reservation>> DeleteReservation(short id)
        {
            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
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
