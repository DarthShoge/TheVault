namespace Muszex.Analytics.DataStructures

    type ApiConfig = {Key: string; Secret: string}
    type SocialTag = |Twitter|Soundcloud|Bandcamp|Facebook|LastFm|Tumblr|Website|Youtube|Songkick
    type SocialLink = {Tag: SocialTag; Uri: string}
    type Locale = {Country: string; Address:string}

    type SubGenre = |Rap|Trap|Soul|Folk
    type Genre = |HipHop = 0|Contry = 1|RnB = 2|Rock = 3|Pop = 4|Rap=5|Trap=6|Soul=7|Folk=8 |Electronic= 9
    type Artist = { Id : int; Name:string; SocialLinks: SocialLink seq; Locale:Locale;Genres:Genre seq; }
    type ArtistMedia = {Id : int;VideoLink : string; PictureLink:string}
    type SoundCloudArtist = {Id: int; Name:string;FollowerCount: int; FollowingCount: int}
    type Attribute = {Key:string; Value:double}
    type ProviderResponse = {Link:string; Provider:string; Attributes:Attribute seq }

    type ArtistBundle = {Artist: Artist;Media:ArtistMedia}
