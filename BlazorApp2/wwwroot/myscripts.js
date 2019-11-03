function onafterrender() {
    console.log("onAfterRenderStart");
    var movie = document.getElementsByClassName("stopprop");
    for (var i = 0; i < movie.length; i++) {
        movie.item(i).addEventListener('contextmenu', function (e) {
            e.stopPropagation();
            //alert('div');
            console.log("child1 clicked");
        }, false);
    }

    /*var movie = document.getElementsByClassName("movieImg");
    for (var i = 0, j = movie.length; i < j; i++) {
        movie[i].addEventListener("mouseover", function () {
            var preview = this.getElementsByClassName("previewBulk")[0];
            preview.style.left = ((this.offsetWidth - preview.offsetWidth) / 2) + 20;
            preview.style.top = -(this.offsetHeight + preview.offsetHeight);
            preview.style.visibility = "visible";
        });

        movie[i].addEventListener("mouseout", function () {
            var preview = this.getElementsByClassName("previewBulk")[0];
            preview.style.visibility = "hidden";
        });
    }*/

};

function mysp(event) {
    event.stopPropagation();
    console.log("child1 clicked");
}

var $li = $('#menu li').click(function () {
    $li.removeClass('selected');
    $(this).addClass('selected');
});