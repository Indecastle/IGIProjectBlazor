function onafterrender() {
    console.log("onAfterRenderStart");
    var movie = document.getElementsByClassName("stopprop");
    for (var i = 0; i < movie.length; i++) {
        movie.item(i).addEventListener('contextmenu', function (e) {
            e.stopPropagation();
            //alert('div');
            console.log("child1 contextmenu-clicked");
        }, false);
        //movie.item(i).addEventListener('click', function (e) {
        //    e.stopPropagation();
        //    //alert('div');
        //    console.log("child1 clicked");
        //}, false);
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

function my_opendialog(id_name) {
    document.getElementById(id_name).click();
}


function my_getTopRight(element) {
    var rect = element.getBoundingClientRect();
    //alert("x: " + rect.right + " y: " + rect.top)
    return [ rect.right, rect.top ];
}







var droppedFiles = null;

function fileContainerChangeFile(e) {
    document.getElementById('fileSelectBox').classList.remove('fileContainerDragOver');
    try {
        droppedFiles = document.getElementById('fs').files;
        document.getElementById('fileName').textContent = droppedFiles[0].name;
    } catch (error) { ; }
    // you can also use the property from the fs field, but this won't work
    // with good old IE.
    try {
        aName = document.getElementById('fs').value;
        if (aName !== '') {
            document.getElementById('fileName').textContent = aName;
        }
    } catch (error) {
        ;
    }
}

function onDrop(e) {
    document.getElementById('fileSelectBox').classList.remove('fileContainerDragOver');
    try {
        droppedFiles = e.dataTransfer.files;
        var filesCount = droppedFiles.length;

        if (filesCount === 1) {
            // if single file is selected, show file name
            document.getElementById('fileName').textContent = droppedFiles[0].name;
        } else {
            // otherwise show number of files
            document.getElementById('fileName').textContent = filesCount + ' files selected';
        }
    } catch (error) { ; }
}

function dragOver(e) {
    document.getElementById('fileSelectBox').classList.add('fileContainerDragOver');
    e.preventDefault();
    e.stopPropagation();
}

function leaveDrop(e) {
    document.getElementById('fileSelectBox').classList.remove('fileContainerDragOver');
}

function myFunction(id) {
    /* Get the text field */
    var copyText = document.getElementById(id);

    /* Select the text field */
    copyText.select();

    /* Copy the text inside the text field */
    document.execCommand("copy");

    /* Alert the copied text */
    alert("Copied the text: " + copyText.value);
}