
'use strict';

app.factory('artistData', function ($http, $log, $q) {
    return {
        getArtistData: function (id) {
            var defrd = $q.defer();
            $http({ method: 'GET', url: '/artist/GetArtistResult/' + id })
                .success(function(data, status, headers, config) {
                defrd.resolve(data);
            }).error(function(data, status, headers, config) {
                $log.warn(data, status, headers(), config);
                defrd.reject(data);
            });

            return defrd.promise;
        }
    };
});