﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muszex.Analytics;
using Muszex.Analytics.DataStructures;
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

        [Test]
        public void GivenArtistIsFoundThenUserIsDirectedToArtisProfile()
        {
            //arrange
            var queryRepository = new MockArtistQueryRepository();
            var genres = new List<Genre> {Genre.RnB};
            queryRepository.NextArtist = new ArtistBundle(new Artist(1,"Micheal Jackson", new List<SocialLink>(), new Locale("USA", "Motown"), genres), null);
            var controller = new ArtistController(queryRepository);
            //act
            var result = controller.Id(100) as ViewResult;
                //assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Id", result.ViewName);
        }

        private class MockArtistQueryRepository : IArtistQueryRepository
        {
            public ArtistBundle GetArtist(int id)
            {
                return NextArtist;
            }

            public ArtistBundle NextArtist { get; set; }

        }
    }
}
