using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using Muszex.Repositories;

namespace Muszex.Web.Controllers
{
    public class ArtistController : Controller
    {
        private readonly IArtistQueryRepository _artistQueryRepository;

        public ArtistController(IArtistQueryRepository artistQueryRepository)
        {
            _artistQueryRepository = artistQueryRepository;
        }

        // GET: /Artist/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Id(int artistId = 0)
        {
            var result = _artistQueryRepository.GetArtist(artistId);

            if (result == null)
                return RedirectToAction("NotFound");


            return View("Id",result);
        }

        public ActionResult NotFound()
        {
            return View();
        }
    }
}
