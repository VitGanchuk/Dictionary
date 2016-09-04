$(function () {
    jQuery.fn.extend({
        setUniqueId: function () {
            var $this = $(this);
            var id = $this.attr('id');
            if (id == undefined) {
                $this.attr('id', Math.floor(Math.random() * 26) + Date.now());
            }
            return $this;
        },

        validateForm: function () {
            var $form = $(this);
            var invalid = [];
            var result = true;
            $form.find('input[type=text],input[type=password]').
                not('[class ~= "tt-hint"]').each(function () {
                var $element = $(this);
                if (!$element.validateElement()) {
                    invalid.push($element);
                }
            });
            if (invalid.length > 0) {
                invalid[0].focus();
                result = false;
            }
            return result;
        },

        validateElement: function () {
            var $element = $(this);
            var result = true;
            $element.resetElement();
            if ($element.hasClass('typeahead')) {
                if (!$element.val()) $element.attr('db-id', -1);
                if ($element.val() && $element.attr('db-id') < 1) {
                    $element.setError('Please select an item from the list');
                    result = false;
                }
            }
            if ($element.hasClass('required') && result) {
                if (!$element.val()) {
                    $element.setError('Please fill this field out');
                    result = false;
                }
            }
            if ($element.hasClass('match') && result) {
                var $elementToMatch = $('#' + $element.attr("data-match"));
                if ($element.val() != $elementToMatch.val()) {
                    $element.setError('Please specify passwords that match');
                    result = false;
                }
            }
            if ($element.hasClass('minlength') && result) {
                var minlength = parseInt($element.attr("data-minlength"));
                if ($element.val().length < minlength) {
                    $element.setError('Please specify a password at least ' + minlength + ' characters long');
                    result = false;
                }
            }
            return result;
        },

        resetForm: function () {
            $form = $(this);
            $form.find('input[type=text]').each(function () {
                $(this).resetElement();
            });
        },

        resetElement: function () {
            var $element = $(this);
            if ($element.hasClass('error')) {
                $element.removeClass('error');
                $element.next('label').remove(); 
            }
        },

        setError: function (errorMessage) {
            var $element = $(this);
            $element.resetElement();
            var id = $element.attr('id');
            $element.addClass('error');
            var errorLabel = '<label id="' + id + '-error" class="error" for="' + id + '">' + errorMessage + '</label>';
            $element.after(errorLabel);
        }
    });

    //  Set an unique ID to elements that don't have them before
    $('input[type=text]').each(function () {
        $(this).setUniqueId();
    });

    //  Resets errors in inputs and validates them if Enter pressed
    $('form').on('keypress', 'input[type=text]', function (event) {
        $this = $(this);
        $this.resetElement();
        if (event.which == 13) {
            $this.validateElement();
        }
    });
});