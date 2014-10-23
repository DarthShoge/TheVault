﻿namespace Muszex.Analytics.DataStructures

    type ApiConfig = {Key: string; Secret: string}
    type SocialTag = |Twitter|Soundcloud|Bandcamp|Facebook|LastFm|Tumblr|Website|Youtube|Songkick
    type SocialLink = {Tag: SocialTag; Uri: string}
    type Locale = {Country: string; Address:string}

    type SubGenre = |Rap|Trap|Soul|Folk
    type Genre = |HipHop = 0|Contry = 1|RnB = 2|Rock = 3|Pop = 4
    type Artist = {Name:string; SocialLinks: SocialLink seq; FundPhotoLink: string;Locale:Locale;Genre:Genre }

    type SoundCloudArtist = {Id: int; Name:string;FollowerCount: int; FollowingCount: int}
    type Attribute = {Key:string; Value:double}
    type ProviderResponse = {Link:string; Provider:string; Attributes:Attribute seq }