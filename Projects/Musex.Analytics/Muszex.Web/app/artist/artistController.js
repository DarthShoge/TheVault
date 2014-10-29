
(function () {
    var app = angular.module("artistApp", [])


    var artistController = function ($scope) {
        var artist = {
            name: "Bamboo Razack",
            locale: "London, UK",
            genres: [{ name: "Hip-hop" },
                { name: "Soul" },
                { name: "Electronica" }
            ]

        };

        $scope.artist = artist;
    };

    app.controller("artistController",["$scope",artistController])
}())