$(function () {
    $('input[type = text]').on('keypress', (function (event) {
        if (event.which == 13) {
            var tabindex = parseInt($(this).attr('tabindex'));
            $('[tabindex = ' + (tabindex + 1) + ']').focus();
        }
    }));

    $('#old-password').focus();
    
    //  Validates and submits the change password form
    $('#submit').on('click', function () {
        var $form = $('#change-password-form');
        if ($form.validateForm()) {
            var parameters = {
                OldPassword: $('#old-password').val(),
                NewPassword: $('#new-password').val(),
                ConfirmPassword: $('#confirm-password').val(),
                UrlReferrer: $('#url-referrer').attr('href')
            };
            $.ajax({
                type: "POST",
                url: "/Account/ChangePassword",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ parameters: parameters }),
                //beforeSend: function () { $('#process').modal('show'); },
                success: function (data) {
                    if (data.result == "redirect") {
                        window.location = data.url;
                    } else if (data.result == "error") { showServerSideError(data); }
                },
                error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); }
                //complete: function () { $('#process').modal('hide'); }
            });
        }
    });
});