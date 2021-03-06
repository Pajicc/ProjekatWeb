﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using BookingApp.Models;
using System.Web.Http.OData;

namespace BookingApp.Controllers
{
    [RoutePrefix("api")]
    public class PlacesController : ApiController
    {
        private BAContext db = new BAContext();

        // GET: api/Places
        [HttpGet]
        [EnableQuery]
        [Route("Places", Name = "Plac")]
        public IQueryable<Place> GetPlaces()
        {
            return db.Places;
        }

        // GET: api/Places/5
        [HttpGet]
        [Route("Places/{id}")]
        [ResponseType(typeof(Place))]
        public IHttpActionResult GetPlace(int id)
        {
            Place place = db.Places.Find(id);
            if (place == null)
            {
                return NotFound();
            }

            return Ok(place);
        }

        // PUT: api/Places/5
        [HttpPut]
        [Route("Places/{id}")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPlace(int id, Place place)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != place.Id)
            {
                return BadRequest();
            }

            if(db.Places.Any(x=>(x.Name == place.Name) && (x.Id != place.Id)))
            {
                return BadRequest("Name must be unique");
            }

            db.Entry(place).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlaceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Places
        [HttpPost]
        [Route("Places")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Place))]
        public IHttpActionResult PostPlace(Place place)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(db.Places.Any(x=> (x.Name == place.Name)))
            {
                return BadRequest("Name must be unique.");
            }

            db.Places.Add(place);
            db.SaveChanges();

            return CreatedAtRoute("Plac", new { id = place.Id }, place);
        }

        // DELETE: api/Places/5
        [HttpDelete]
        [Route("Places/{id}")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Place))]
        public IHttpActionResult DeletePlace(int id)
        {
            Place place = db.Places.Find(id);
            if (place == null)
            {
                return NotFound();
            }

            db.Places.Remove(place);
            db.SaveChanges();

            return Ok(place);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PlaceExists(int id)
        {
            return db.Places.Count(e => e.Id == id) > 0;
        }
    }
}