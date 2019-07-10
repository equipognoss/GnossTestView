$(document).ready(function () {

    $(':checkbox').checkboxpicker();

    $('#proySelected').change(function () {
        if ($(this).val() == 'newProy') {
            $('#panNombreProy').show();
        }
        else {
            $('#panNombreProy').hide();
        }
    });


    $('.form-group.botones input').click(function () {
        var body = $('body');
        body.addClass("palco");

        $('iframe', body).remove();

        var url = $('input#URL').val();
        var sessionID = $('input#SessionID').val();
        var user = $('input#User').val();
        var pass = $('input#Password').val();
        var contentLocal = $('input#contentLocal').is(':checked');
        var submit = $(this).val();
        
        var urlIframe = location.protocol + '//' + location.host + '/LoadURL';
        var srcIframe = urlIframe + "?URL=" + url + "&SessionID=" + sessionID + "&User=" + user + "&Password=" + pass + "&contentLocal=" + contentLocal + "&submit=" + submit

        body.append("<iframe id='iframeContenedor' src='" + srcIframe + "' />");

        setTimeout(function () { engancharEvento(); }, 1000);
    });
});

function engancharEvento() {

    var iframe = document.getElementById("iframeContenedor");

    iframe.contentDocument.onkeydown = function () { if (event.keyCode == 116) { $("#cargar").trigger("click"); event.preventDefault(); event.stopPropagation(); } };
    iframe.contentDocument.onkeypress = function () { if (event.keyCode == 116) { $("#cargar").trigger("click"); event.preventDefault(); event.stopPropagation(); } };
    iframe.contentDocument.onkeyup = function () { if (event.keyCode == 116) { $("#cargar").trigger("click"); event.preventDefault(); event.stopPropagation(); } };


}

document.onkeydown = fkey;
document.onkeypress = fkey
document.onkeyup = fkey;

function fkey(e) {
    var body = $('body');
    if (body.hasClass("palco")) {
        e = e || window.event;

        if (e.keyCode == 116) {
            $("#cargar").trigger("click");
            event.preventDefault();
            event.stopPropagation();
            return false;
        }
    }
}
