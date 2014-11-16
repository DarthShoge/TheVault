using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using Muszex.Analytics.DataStructures;
using Muszex.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        public ActionResult Id(int id = 0)
        {
            var result = _artistQueryRepository.GetArtist(id);

            if (result == null)
                return View("NotFound");
            IHtmlString serializeObject = SerializeObject(result);
            return View("Id", serializeObject);
        }

        [HttpPost]
        public JsonResult GetArtistResult(int id)
        {
            var result = _artistQueryRepository.GetArtist(id);

            if (result == null)
                return null;

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public static IHtmlString SerializeObject(object value)
        {
            return new HtmlString(LowercaseJsonSerializer.SerializeObject(value));
            //serializer.Serialize(jsonWriter, value);
            //return new HtmlString(stringWriter.ToString());
        }
    }


    public class LowercaseJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new[] { new MyStringEnumConverter() }

        };

        public static string SerializeObject(object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented, Settings);
        }

        public class MyStringEnumConverter : Newtonsoft.Json.Converters.StringEnumConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is Genre)
                {
                    writer.WriteValue(Enum.GetName(typeof(Genre), (Genre)value));// or something else
                    return;
                }

                base.WriteJson(writer, value, serializer);
            }
        }
    }
}
