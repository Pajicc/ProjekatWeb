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

namespace BookingApp.Controllers
{

    [RoutePrefix("api")]
    public class AccomodationTypesController : ApiController
    {
        private BAContext db = new BAContext();

        [HttpGet]
        [Route("AccomodationTypes", Name ="AccTypes")]
        public IQueryable<AccomodationType> GetAccomodationTypes()
        {
            return db.AccomodationTypes;
        }

        [HttpGet]
        [Route("AccomodationTypes/{id}")]
        [ResponseType(typeof(AccomodationType))]
        public IHttpActionResult GetAccomodationType(int id)
        {
            AccomodationType accomodationType = db.AccomodationTypes.Find(id);
            if (accomodationType == null)
            {
                return NotFound();
            }

            return Ok(accomodationType);
        }

        [HttpPut]
        [Route("AccomodationTypes/{id}")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutAccomodationType(int id, AccomodationType accomodationType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != accomodationType.Id)
            {
                return BadRequest();
            }
            if (db.AccomodationTypes.Any(x => (x.Name == accomodationType.Name)&& (x.Id != accomodationType.Id)))
            {
                return BadRequest("Name must be unique.");
            }
            db.Entry(accomodationType).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccomodationTypeExists(id))
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

        [HttpPost]
        [Route("AccomodationTypes")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(AccomodationType))]
        public IHttpActionResult PostAccomodationType(AccomodationType accomodationType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(db.AccomodationTypes.Any(x=> (x.Name == accomodationType.Name)))
            {
                return BadRequest("Name must be unique.");
            }

            db.AccomodationTypes.Add(accomodationType);
            db.SaveChanges();

            return CreatedAtRoute("AccTypes", new { id = accomodationType.Id }, accomodationType);
        }

        [HttpDelete]
        [Route("AccomodationTypes/{id}")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(AccomodationType))]
        public IHttpActionResult DeleteAccomodationType(int id)
        {
            AccomodationType accomodationType = db.AccomodationTypes.Find(id);
            if (accomodationType == null)
            {
                return NotFound();
            }

            db.AccomodationTypes.Remove(accomodationType);
            db.SaveChanges();

            return Ok(accomodationType);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AccomodationTypeExists(int id)
        {
            return db.AccomodationTypes.Count(e => e.Id == id) > 0;
        }
    }
}