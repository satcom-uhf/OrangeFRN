
/*
    When the bandcamp link is pressed, stop all propagation so AmplitudeJS doesn't
    play the song.
*/
let bandcampLinks = document.getElementsByClassName('bandcamp-link');

for (var i = 0; i < bandcampLinks.length; i++) {
    bandcampLinks[i].addEventListener('click', function (e) {
        e.stopPropagation();
    });
}


let songElements = document.getElementsByClassName('song');

for (var i = 0; i < songElements.length; i++) {
    /*
        Ensure that on mouseover, CSS styles don't get messed up for active songs.
    */
    songElements[i].addEventListener('mouseover', function () {
        this.style.backgroundColor = '#00A0FF';

        this.querySelectorAll('.song-meta-data .song-title')[0].style.color = '#FFFFFF';
        this.querySelectorAll('.song-meta-data .song-artist')[0].style.color = '#FFFFFF';

        if (!this.classList.contains('amplitude-active-song-container')) {
            this.querySelectorAll('.play-button-container')[0].style.display = 'block';
        }

        this.querySelectorAll('img.bandcamp-grey')[0].style.display = 'none';
        this.querySelectorAll('img.bandcamp-white')[0].style.display = 'block';
        this.querySelectorAll('.song-duration')[0].style.color = '#FFFFFF';
    });

    /*
        Ensure that on mouseout, CSS styles don't get messed up for active songs.
    */
    songElements[i].addEventListener('mouseout', function () {
        this.style.backgroundColor = '#FFFFFF';
        this.querySelectorAll('.song-meta-data .song-title')[0].style.color = '#272726';
        this.querySelectorAll('.song-meta-data .song-artist')[0].style.color = '#607D8B';
        this.querySelectorAll('.play-button-container')[0].style.display = 'none';
        this.querySelectorAll('img.bandcamp-grey')[0].style.display = 'block';
        this.querySelectorAll('img.bandcamp-white')[0].style.display = 'none';
        this.querySelectorAll('.song-duration')[0].style.color = '#607D8B';
    });

    /*
        Show and hide the play button container on the song when the song is clicked.
    */
    songElements[i].addEventListener('click', function () {
        this.querySelectorAll('.play-button-container')[0].style.display = 'none';
    });
}

var playlist = Vue.createApp({
    data() {
        return {
            songs: [],
            selectedDate: new Date(),
            year: new Date().getFullYear(),
            month: new Date().getMonth(),
            day: new Date().getDate()
        };
    },
    methods: {
        play(i) {
            Amplitude.playSongAtIndex(i);
        },
        updateSongs() {
            let d = this.selectedDate;
            Amplitude.stop();
            const day = ("0" + d.getDate()).slice(-2);
            const month = ("0" + (d.getMonth() + 1)).slice(-2);
            fetch(`/Files/${d.getFullYear()}${month}${day}`)
                .then((response) => response.json())
                .then((songs) => {
                    this.songs = songs;
                    Amplitude.init({
                        "songs": songs,
                        "volume": 100,
                        waveforms: {
                            sample_rate: 500
                        }
                    });
                });
        }
    },

    mounted() {
        this.updateSongs()
    },
    watch: {
        year(y) {
            this.selectedDate.setFullYear(y);
            this.updateSongs();
        },
        month(m) {
            this.selectedDate.setMonth(m);
            this.updateSongs();
        },
        day(d) {
            this.selectedDate.setDate(d);
            this.updateSongs();
        }
    }
}).mount('#amplitude-right');
//const picker = new AirDatepicker('#calendar', {
//    alwaysShow: true,
//    isMobile: true,
//    timepicker: true,
//    autoClose:true,
//    onSelect: function (d) {
//        playlist.updateSongs(d.date);
//    }
//});

