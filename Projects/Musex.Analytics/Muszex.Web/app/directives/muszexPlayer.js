app.directive("muszexPlayer", function () {
    var onReadCallBack = function() {
        
    }
    soundManager.setup({
        url: '/content/misc/',
        onready: function () {
            onReadCallBack();
        },
        ontimeout: function () {
        }
    });

    threeSixtyPlayer.config.scaleFont = (navigator.userAgent.match(/msie/i) ? false : true);
    threeSixtyPlayer.config.showHMSTime = true;
    threeSixtyPlayer.config.useWaveformData = true;
    threeSixtyPlayer.config.useEQData = true;
    if (threeSixtyPlayer.config.useWaveformData) {
        soundManager.flash9Options.useWaveformData = true;
    }
    if (threeSixtyPlayer.config.useEQData) {
        soundManager.flash9Options.useEQData = true;
    }
    if (threeSixtyPlayer.config.usePeakData) {
        soundManager.flash9Options.usePeakData = true;
    }

    if (threeSixtyPlayer.config.useWaveformData || threeSixtyPlayer.flash9Options.useEQData || threeSixtyPlayer.flash9Options.usePeakData) {
        soundManager.preferFlash = true;
    }
    if (window.location.href.match(/hifi/i)) {
        threeSixtyPlayer.config.useFavIcon = true;
    }
    if (window.location.href.match(/html5/i)) {
        soundManager.useHTML5Audio = true;
    }

    return {
        restrict: 'E',
        templateUrl: '/app/templates/soundPlayer.html',
        scope : {
            src: '=',
            songName: '=',
            onready: "&"
        }
    };


})