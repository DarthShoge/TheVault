using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Muszex.Analytics;
using Muszex.Analytics.DataStructures;
using Artist = Muszex.Analytics.DataStructures.Artist;

namespace Muszex.Repositories
{
    public interface IArtistQueryRepository
    {
        Artist GetArtist(int id);

    }
    public class ArtistQueryRepository : IArtistQueryRepository
    {
        public Artist GetArtist(int id)
        {
            return new Artist("Micheal Jackson", new List<SocialLink>(), "", new Locale("USA", "Motown"), Genre.Pop); ;
        }
    }
}
