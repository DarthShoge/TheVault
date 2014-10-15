using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }
    }
}
