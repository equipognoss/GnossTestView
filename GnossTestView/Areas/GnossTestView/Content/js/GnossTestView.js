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
        body.addClass("palco");

        $('iframe', body).remove();

        var url = $('input#URL').val();
        var sessionID = $('input#SessionID').val();
        var user = $('input#User').val();
        var pass = $('input#Password').val();
        var contentLocal = $('input#contentLocal').is(':checked');
        var proyectoSeleccionado = $('#proyectoSeleccionado').text();
        var submit = $(this).val();
        
        var urlIframe = location.protocol + '//' + location.host + '/LoadURL';
        var srcIframe = urlIframe + "?URL=" + url + "&SessionID=" + sessionID + "&User=" + user + "&Password=" + pass + "&contentLocal=" + contentLocal + "&proyectoSeleccionado=" + proyectoSeleccionado + "&submit=" + submit

        body.append("<iframe id='iframeContenedor' src='" + srcIframe + "' />");
        
        setTimeout(function () { engancharEvento(); }, 1000);
    });
});

function engancharEvento() {
    var iframe = document.getElementById("iframeContenedor");

    iframe.contentDocument.onkeydown = fkey;
    iframe.contentDocument.onkeypress = fkey;
    iframe.contentDocument.onkeyup = fkey;

    recargando = false;
}

document.onkeydown = fkey;
document.onkeypress = fkey
document.onkeyup = fkey;

var recargando = false;
var body = $('body');

function fkey(e) {
    if (body.hasClass("palco")) {
        e = e || window.event;

        if (e.keyCode == 116) {
            if (!recargando) {
                recargando = true;
                $('#cargar').focus()
                $("#cargar").trigger("click");
            }
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
    }
}

window.onbeforeunload = function (e) {
    if (body.hasClass("palco") && recargando) {
        var dialogText = '¿Seguro que quieres salir?';
        e.returnValue = dialogText;
        return dialogText;
    }
};