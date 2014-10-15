using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muszex.Analytics;
using Muszex.Repositories;
using Muszex.Web.Controllers;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Muszex.Web.Tests
{
    [TestFixture]
    public class ArtistControllerTests
    {
        [Test]
        public void GivenAnUnfoundArtistThenUserIsRedirectedToNotFoundScreen()
        {
            //arrange
            var queryRepository = new MockArtistQueryRepository();
            queryRepository.NextArtist = null;
            var controller = new ArtistController(queryRepository);
            //act
            var result = controller.Id(100) as RedirectToRouteResult;
            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual("NotFound", result.RouteValues["action"]);
        }

        private class MockArtistQueryRepository : IArtistQueryRepository
        {
            public DataStructures.Artist GetArtist(int id)
            {
                return NextArtist;
            }

            public DataStructures.Artist NextArtist { get; set; }

        }
    }
}
