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
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace BookingApp.Controllers
{
    [RoutePrefix("api")]
    public class CommentsController : ApiController
    {
        private BAContext db = new BAContext();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        //[EnableQuery]
        [Route("Comments", Name = "Comm")]
        public IQueryable<Comment> GetComments()
        {
            return db.Comments;
        }

        [HttpGet]
        [Route("Comments/{accId}/{appId}")]
        public IHttpActionResult GetComment(int accId, int appId)
        {
            Comment comment = db.Comments.Find(accId, appId);
            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        [HttpGet]
        [Route("Comments/{id}")]
        public IQueryable<Comment> GetCommentForAccomodation(int id)
        {
            return db.Comments.Where(x => x.AccomodationId == id);
        }

        [HttpPut]
        [Authorize(Roles = "AppUser")]
        [Route("Comments/{accId}/{appId}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutComment(int accId, int appId, Comment comment)
        {           

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((accId != comment.AccomodationId) || (appId != comment.AppUserId))
            {
                return BadRequest();
            }

            var user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));

            if (user == null)
            {
                return BadRequest("You are not logged in.");
            }

            if (comment.AppUserId != user.appUserId)
            {
                BadRequest("You don't have right to change the comment.");
            }

            db.Entry(comment).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(accId, appId))
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
        [Authorize(Roles = "AppUser")]
        [Route("Comments")]
        [ResponseType(typeof(Comment))]
        public IHttpActionResult PostComment(Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            /*List<RoomReservations> reservations = ReservationsExist(comment);

            if (reservations.Count == 0)
            {
                return BadRequest("You don't have reservations for this accommodation.");
            }

            RoomReservations reservation = GetReservation(reservations);

            if (reservation == null || reservation.StartDate >= DateTime.Now)
            {
                return BadRequest("You can't comment on accommodation in which you are not staying");
            }*/

            Accomodation accomodation = db.Accomodations.Where(a => a.Id == comment.AccomodationId).FirstOrDefault();

            if (accomodation == null)
            {
                return BadRequest("You can't post comment because accommodation no longer exists");
            }

            try
            {
                db.Comments.Add(comment);
                db.SaveChanges();

            }
            catch (DbUpdateException e)
            {
                return Content(HttpStatusCode.Conflict, comment);
            }

            accomodation.AverageGrade = AverageGrade(comment.AccomodationId);
            db.SaveChanges();


            return CreatedAtRoute("Comm", new { controller = "Comment", accid = comment.AccomodationId, appId = comment.AppUserId }, comment);
        }

        [HttpDelete]
        [Authorize(Roles = "AppUser")]
        [Route("Comments/{accId}/{appId}")]
        [ResponseType(typeof(Comment))]
        public IHttpActionResult DeleteComment(int accId, int appId)
        {
                 
            Comment comment = db.Comments.Find(accId, appId);
            if (comment == null)
            {
                return NotFound();
            }

            var user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));

            if (user == null)
            {
                return BadRequest("You are not logged in.");
            }

            if (comment.AppUserId != user.appUserId)
            {
                BadRequest("You don't have right to delete the comment.");
            }

            db.Comments.Remove(comment);
            db.SaveChanges();

            Accomodation accomodation = db.Accomodations.Where(a => a.Id == comment.AccomodationId).FirstOrDefault();   //ponovo izracunamo avrg grade

            if (accomodation == null)
            {
                return BadRequest("There is no accommodation for which is changing comment.");
            }

            accomodation.AverageGrade = AverageGrade(comment.AccomodationId);
            db.SaveChanges();

            return Ok(comment);
        }

        private List<RoomReservations> ReservationsExist(Comment comment)
        {
            return db.RoomReservations.Where(resevation => resevation.Room.AccomodationId.Equals(comment.AccomodationId)
                && resevation.AppUserId.Equals(comment.AppUserId) && resevation.Canceled == false).ToList();
        }

        private RoomReservations GetReservation(List<RoomReservations> reservations)
        {
            return reservations.FirstOrDefault(res => res.StartDate.Equals(reservations.Min(o => o.StartDate)));
        }

        private double AverageGrade(int accId)
        {
            List<Comment> comments = db.Comments.Where(c => c.AccomodationId == accId).ToList();

            if (comments.Count > 0)
            {
                double grade;
                try
                {
                    grade = (double)(comments.Sum(c => c.Grade)) / (double)comments.Count;
                }
                catch (DivideByZeroException)
                {
                    grade = 0;
                }

                return Math.Round(grade, 1);
            }

            return 0.0;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CommentExists(int accId, int appId)
        {
            return db.Comments.Count(e => (e.AccomodationId == accId) && (e.AppUserId == appId)) > 0;
        }

    }
}