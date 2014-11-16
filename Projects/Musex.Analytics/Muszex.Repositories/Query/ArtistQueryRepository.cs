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
        ArtistBundle GetArtist(int id);

    }
    public class ArtistQueryRepository : IArtistQueryRepository
    {

        public List<ArtistBundle> Artists
        {
            get
            {
                return new List<ArtistBundle>
                {
                    new ArtistBundle(
                        new Artist(1,"Iman Omari", new List<SocialLink>(),  new Locale("USA", "California"),new List<Genre>{Genre.RnB, Genre.HipHop}),
                        new ArtistMedia(1,"imom2.mp4","")
                        )
                };
            }
        }

        public ArtistBundle GetArtist(int id)
        {
            return Artists.FirstOrDefault(x => x.Artist.Id == id);
        }
    }
}
