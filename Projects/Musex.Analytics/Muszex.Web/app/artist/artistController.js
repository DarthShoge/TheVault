
'use strict';

(function () {


    var artistController = function ($scope, artistData, artistInit,$log) {
        var artist = {
            name: "Bamboo Razack",
            locale: "London, UK",
            genres: [{ name: "Hip-hop" },
                { name: "Soul" },
                { name: "Electronica" }
            ],

        };

        //artistData.getArtistData(1).then(
        //    function(d) { data = d; },
        //    function(sc) {}
        //);

        $scope.alertReady = function() {
            $log.warn("Up an running cap'n");
        };

        $scope.funding = {
            state: "unfunded",
            daysLeft: 134,
            currentPrc: 76,
            level: function() {
                if (this.currentPrc > 70)
                    return "success";
                else if (this.currentPrc > 50)
                    return "warning";
                else
                    return "danger";
            }
        };


        
        $scope.artist = artistInit.artist;
        $scope.currentSong = artistInit.media.songs[0];

    };

    app.controller("artistController", ["$scope", 'artistData', 'artistInit','$log', artistController]);
}())