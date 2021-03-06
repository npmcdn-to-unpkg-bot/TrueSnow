﻿namespace TrueSnow.Web.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;

    using Data.Models;
    using Infrastructure.Mapping;
    using Models.Events;
    using TrueSnow.Services.Data.Contracts;

    public class EventsController : BaseController
    {
        private readonly IEventsService events;
        private readonly UserManager<User> userManager;

        public EventsController(IEventsService events, UserManager<User> userManager)
        {
            this.events = events;
            this.userManager = userManager;
        }

        public ActionResult Index()
        {
            var eventsModel = this.events
                .GetAll()
                .To<EventViewModel>()
                .ToList();

            return this.View(eventsModel);
        }

        public ActionResult ById(string id)
        {
            var eventModel = this.events.GetById(id);
            var model = this.Mapper.Map<EventViewModel>(eventModel);
            return this.View(model);
        }

        public ActionResult GetLatest()
        {
            var latestEvent = this.events.GetAll().FirstOrDefault();
            var model = this.Mapper.Map<EventViewModel>(latestEvent);
            return this.PartialView(model);
        }

        public ActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventViewModel model, HttpPostedFileBase upload)
        {
            if (this.ModelState.IsValid)
            {
                var eventToAdd = new Event
                {
                    Title = model.Title,
                    Description = HttpUtility.HtmlDecode(model.Description),
                    CreatorId = this.User.Identity.GetUserId()
                };

                if (upload != null && upload.ContentLength > 0)
                {
                    var photo = new Data.Models.File
                    {
                        FileName = Path.GetFileName(upload.FileName),
                        FileType = FileType.Photo,
                        ContentType = upload.ContentType
                    };

                    using (var reader = new BinaryReader(upload.InputStream))
                    {
                        photo.Content = reader.ReadBytes(upload.ContentLength);
                    }

                    eventToAdd.Photo = photo;
                    this.events.Add(eventToAdd);
                }

                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Attend(int eventId)
        {
            if (this.ModelState.IsValid)
            {
                var eventToAttend = this.events.GetAll().FirstOrDefault(x => x.Id == eventId);
                var currentUser = this.userManager.FindById(this.User.Identity.GetUserId());
                eventToAttend.Attendants.Add(currentUser);
                this.events.Save();
            }

            return this.Redirect(this.Request.UrlReferrer.ToString());
        }
    }
}
