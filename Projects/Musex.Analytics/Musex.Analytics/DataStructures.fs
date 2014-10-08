namespace Musex.Analytics
    module DataStructures =
        type ApiConfig = {Key: string; Secret: string}

        type Artist = {Name:string}

        type SoundCloudArtist = {Id: int; Name:string;FollowerCount: int; FollowingCount: int}
