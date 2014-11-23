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
                        new ArtistMedia(1,"/Content/videos/imom2.mp4","/Content/images/artists/imom.jpeg",new List<Song>
                        {
                            new Song("Energy","/content/audio/imomeneg.mp3")
                        })),
                        new ArtistBundle(
                        new Artist(2,"Doja Cat", new List<SocialLink>(),  new Locale("USA", "Los Angeles"),new List<Genre>{Genre.RnB, Genre.Trap, Genre.Electronic,Genre.Rap}),
                        new ArtistMedia(1,"/Content/videos/dojacat.mp4","",new List<Song>{new Song("Beautiful","/content/audio/dojabeau.mp3")}))
                        
                };
            }
        }

        public ArtistBundle GetArtist(int id)
        {
            return Artists.FirstOrDefault(x => x.Artist.Id == id);
        }
    }
}
