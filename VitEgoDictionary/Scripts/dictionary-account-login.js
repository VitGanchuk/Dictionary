$(function () {

    $('.checkbox').checkbox({
        checkedClass: 'fa fa-lg fa-check-square-o',
        uncheckedClass: 'fa fa-lg fa-square-o'
    });

    $('input[type = text]').on('keypress', (function (event) {
        if (event.which == 13) {
            var tabindex = parseInt($(this).attr('tabindex'));
            $('[tabindex = ' + (tabindex + 1) + ']').focus();
        }
    }));

    $('#user-name').focus();

    //  Validates and submits the login form
    $('#submit').on('click', function () {
        var $form = $('#login-form');
        if ($form.validateForm()) {
            var parameters = {
                UserName: $('#user-name').val(),
                Password: $('#password').val(),
                RememberMe: $('#remember-me').is(':checked') ? true : false,
                UrlReferrer: $('#url-referrer').attr('href')
            };
            $.ajax({
                type: "POST",
                url: "/Account/Login",
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