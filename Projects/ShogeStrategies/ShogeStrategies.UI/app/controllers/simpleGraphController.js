
tradingApp.controller('SimpleGraphController', function SimpleGraphController($scope) {

    $scope.graphData = {
        symbol: 'YHOO',
        dailyHi: 19.4,
        dailyLo: 19.3,
        dailyValues: [
            { start: new Date('12/01/2012'), value: 15 },
            { start: new Date('12/02/2012'), value: 13 },
            { start: new Date('12/03/2012'), value: 5 },
            { start: new Date('12/04/2012'), value: 23 },
            { start: new Date('12/05/2012'), value: 15 },
            { start: new Date('12/06/2012'), value: 14 },
            { start: new Date('12/07/2012'), value: 12 },
        ]
    };
});