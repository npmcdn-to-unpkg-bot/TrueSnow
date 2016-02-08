﻿namespace TrueSnow.Web.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;

    using Data.Services.Contracts;
    using Models.Posts;
    using System;
    using Data.Models;
    using System.Web;
    using System.IO;
    using System.Collections.Generic;
    public class PostsController : Controller
    {
        private IPostsService posts;

        public PostsController(IPostsService posts)
        {
            this.posts = posts;
        }

        public ActionResult Index(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                var postsViewModel = this.posts
                    .GetAll()
                    .Select(p => new PostViewModel
                    {
                        Title = p.Title,
                        Content = p.Content,
                        CreatedOn = p.CreatedOn,
                        Files = p.Files,
                        Creator = p.Creator
                    })
                    .ToList();

                return PartialView("Index", postsViewModel);
            }
            else
            {
                var postsViewModel = this.posts
                    .GetByUserId(id)
                    .Select(p => new PostViewModel
                    {
                        Title = p.Title,
                        Content = p.Content,
                        CreatedOn = p.CreatedOn,
                        Files = p.Files,
                        Creator = p.Creator
                    })
                    .ToList();

                return PartialView("Index", postsViewModel);
            }
        }

        public ActionResult GetCreate()
        {
            return PartialView("Create");
        }

        public ActionResult Create(PostViewModel post, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                var postToAdd = new Post
                {
                    Title = post.Title,
                    Content = post.Content,
                    CreatedOn = DateTime.Now,
                    CreatorId = HttpContext.User.Identity.GetUserId()
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

                    postToAdd.Files = new List<Data.Models.File> { photo };
                }

                this.posts.Add(postToAdd);

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
