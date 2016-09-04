$(function () {
    $('#modal-dialog').on('show.bs.modal', function (event) {
        var target = $(event.relatedTarget);
        if (target[0] != undefined) {
            $(document).data('confirmation-target', target);
            var modal = $(this);
            modal.find('.modal-body').html(target.data('content'))
            if (target.data('buttons') == 'CloseConfirm') {
                modal.find('.modal-title').text('Confirmation');
                var action = target.data('action');
                modal.find('.modal-footer').html(
                    '<button type="button" class="button button-default" data-dismiss="modal">Close</button>' +
                    '<button type="button" class="button button-primary button-delete" data-dismiss="modal">Delete</button>');
            } else {
                //Another set of buttons
            }
        } else {
            //  Something else
        }
    });

    if (navigator.appName == "Opera") {
        $('#modal-dialog').removeClass('fade');
    }

    $('.modal-footer').on('click', '.button-delete', function () {
        var action = $(document).data('confirmation-target').data('action');
        var target = $(document).data('confirmation-target');
        switch(action)
        {
            case 'delete-data-item':
                {
                    var section = target.parents('div.section');
                    var itemToDelete = target.parents('div[class ~= data-item]');
                    itemToDelete.slideUp(Math.log(itemToDelete.height()) * 100, function () {
                        $(this).remove();
                        section.panelItemCount();
                    });
                    break;
                }
            case 'delete-data-subitem':
                {
                    var itemToDelete = target.parents('div[class ~= data-subitem]');
                    itemToDelete.slideUp(Math.log(itemToDelete.height()) * 100, function () {
                        $(this).remove(); 
                    });
                    break;
                }
            default:
                {
                    $.ajax({
                        type: "POST",
                        url: action,
                        contentType: 'application/json; charset=utf-8',
                        beforeSend: function () { $('#process').modal('show'); },
                        success: function (data) {
                            if (data.result == "redirect") {
                                window.location = data.url;
                            } else if (data.result == "error") { showServerSideError(data); }
                        },
                        error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); },
                        complete: function () { $('#process').modal('hide'); }
                    });
                }
        }
    });
});

function showHttpRequestError(jqXhr, status, error) {
    var modal = $('#modal-dialog');
    var content = '<p><span class="warning">Status: </span>' + status + '</p><p><span class="warning">Error: </span>' + error + '</p>';
    modal.find('.modal-title').text('AJAX Error: ' + jqXhr.status);
    modal.find('.modal-body').html(content)
    modal.find('.modal-footer').html('<button type="button" class="button button-primary" data-dismiss="modal">Close</button>');
    modal.modal('show');
}

function showServerSideError(data) {
    var modal = $('#modal-dialog');
    var content = '<p><span class="warning">Error: </span>' + data.message + '</p>';
    if (data.innerMessage != null) {
        content = content + '<p><span class="warning">Inner Error: </span>' + data.innerMessage + '</p>';
    }
    modal.find('.modal-title').text('AJAX Error');
    modal.find('.modal-body').html(content)
    modal.find('.modal-footer').html('<button type="button" class="button button-primary" data-dismiss="modal">Close</button>');
    modal.modal('show');
}

function showClientSideError(message) {
    var modal = $('#modal-dialog');
    var content = '<p><span class="warning">Error: </span>' + message + '</p>';
    modal.find('.modal-title').text('Error');
    modal.find('.modal-body').html(content)
    modal.find('.modal-footer').html('<button type="button" class="button button-primary" data-dismiss="modal">Close</button>');
    modal.modal('show');
}