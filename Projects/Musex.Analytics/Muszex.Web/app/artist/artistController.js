
'use strict';

(function () {
    var app = angular.module("artistApp", ['ui.bootstrap']);


    var artistController = function ($scope) {
        var artist = {
            name: "Bamboo Razack",
            locale: "London, UK",
            genres: [{ name: "Hip-hop" },
                { name: "Soul" },
                { name: "Electronica" }
            ],

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


       
        $scope.artist = artist;
    };

    app.controller("artistController", ["$scope", artistController]);
}())