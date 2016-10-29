//  Data retrieving and element initializing
$(function () {
    var allWords = new Bloodhound({
        datumTokenizer: function (datum) {
            return Bloodhound.tokenizers.whitespace(datum.name);
        },
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        limit: 10,
        prefetch: {
            url: '/Data/Words/?speechPart=' + speechPart,
            cache: false,
            filter: function (list) {
                return $.map(list, function (item) {
                    return {
                        id: item.id,
                        name: item.name,
                        speechPart: item.speechPart
                    };
                });
            }
        }
    });
});

//  Form submit & validation functions
$('#submit').on('click', function () {
    var $form = $('#item-form-base');
    if ($form.validateForm()) {
        var parameters = {
            ID: $('#base-word').attr('db-id'),
            Name: $.trim($('#base-word').val().toLowerCase()),
            Topic: $('#topic').val(),
            Formality: $('#formality').val()
        };
        parameters.Meanings = [];
        $('#meaning-section').find('div.edit-panel').find('div.data-item-container').find('div.data-item').each(function (i) {
            var dataItem = $(this);
            var meaning = dataItem.find('input[type = text][class ~= "meaning"]');
            var meaningId = meaning.attr('db-id');
            var examples = [];
            var synonyms = [];
            dataItem.find('div.data-example-subitem-container').find('div.data-subitem').each(function (j) {
                var dataExampleItem = $(this);
                var exampleInput = dataExampleItem.find('input[type = text][class ~= example]');
                var exampleId = exampleInput.attr('db-id');
                examples[j] = { ID: exampleId, Example: $.trim(exampleInput.val()) };
            });
            dataItem.find('div.data-synonym-subitem-container').find('div.data-subitem').each(function (j) {
                var dataSynonymItem = $(this);
                var synonymInput = dataSynonymItem.find('select');
                var synonymItem = synonymInput.attr('db-item');
                synonyms[j] = { ItemType: synonymItem, Meaning: synonymInput.val() };
            });
            parameters.Meanings[i] = {
                ID: meaningId,
                Meaning: $.trim(meaning.val().toLowerCase()),
                Examples: examples,
                Synonyms: synonyms
            };
        });
        $.ajax({
            type: "POST",
            url: $('#url').val(),
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ parameters: parameters }),
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
});