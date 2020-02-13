$(document).ready(function () {

    $(':checkbox').checkboxpicker();

    $('.form-group.botones input').click(function () {
        body.addClass("palco");
        body.removeClass("showBasicAuth");
        $('#authBasic').hide();

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


    $('body.configuracion .form-group label span.fa.fa-trash:not(".disabled")').click(function () {
        $(this).closest('.form-group').remove();
    });

    $('body a.dropdown-toggle.authBasic').click(function () {
        event.preventDefault();
        $('body').toggleClass('showBasicAuth');
        $('#authBasic').toggle();
    });

    $('#confirmationModal .modal-footer').on('click', '.btn-primary.download', function () {
        var boton = $(this);
        var modal = boton.closest('.modal');
        var modalBody =  modal.find('.modal-body');
        var proyecto = boton.attr("proyecto");
        
        boton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>  Loading...');
        modal.find('.modal-footer .btn-secondary').hide();

        $.post("/configuration/downloadViews", { proyectName: proyecto })
          .done(function (status) {
              if (status) {
                  modalBody.append('<div class="alert alert-success" role="alert">Descarga correcta</div>');
              }
              else {
                  modalBody.append('<div class="alert alert-danger" role="alert">Descarga fallida</div>');
              }
              modal.find('.modal-footer').hide();
          });
    });


    $('#confirmationModal .modal-footer').on('click', '.btn-primary.upload', function () {
        var boton = $(this);
        var modal = boton.closest('.modal');
        var modalBody = modal.find('.modal-body');
        var proyecto = boton.attr("proyecto");
        var userUpload = modalBody.find("#userUpload").val();

        boton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>  Loading...');
        modal.find('.modal-footer .btn-secondary').hide();

        $.post("/configuration/uploadViews", { proyectName: proyecto, user: userUpload })
          .done(function (status) {
              if (status) {
                  modalBody.append('<div class="alert alert-success" role="alert">  Simulacion OK</div>');
              }
              else {
                  modalBody.append('<div class="alert alert-danger" role="alert"> Simulacion KO</div>');
              }
              modalBody.append('<div class="alert alert-warning" role="alert">TO-DO<br />NO SE HAN SUBIDO LOS CAMBIOS. FALTA EL DESARROLLO<br />' +
                  'Cuidado con no sobreescribir los ficheros de \"TextosPersonalizadosPersonalizacion y CMS\"</div>');
              modal.find('.modal-footer').hide();
          });

    });

    $('#confirmationModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);

        var title = button.data('title');
        var proyecto = button.data('proyecto');
        var action = button.attr("title");
        var btnClass = button.attr("class");
        var typeAction = btnClass.replace('fa fa-cloud-', '');

        var modal = $(this);
        modal.find('.modal-body .alert').remove();
        modal.find('.modal-body').children().hide();
        modal.find('.modal-body .' + typeAction).show();
        modal.find('.modal-title').text(title + ' \'' + proyecto + '\'');

        modal.find('.modal-footer').show();
        modal.find('.modal-footer .btn-secondary').show();
        modal.find('.modal-footer .btn-primary').attr('class', 'btn btn-primary ' + typeAction).attr('proyecto', proyecto).html("<span class='" + btnClass + "'></span>" + action);
    })
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