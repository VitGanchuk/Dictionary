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

    //  Initializes associated words typeahead
    $('.associated-word').typeahead({
        hint: true,
        highlight: true,
        minLength: 1,
    },
    {
        name: 'associatedWords',
        displayKey: 'name',
        source: allWords.ttAdapter(),
        templates: {
            empty: function () {
                //var $this = $(this);
                if ($this.attr('db-id') != '-1' || $this.attr('db-item') != '-1') {
                    $this.attr('db-id', '-1');
                }
                return false;
            },
            suggestion: function (datum) {
                return '<div class="tt-suggestion tt-selectable"><span>' + datum.name + '</span>&nbsp;<span class="note">' + datum.speechPart + '</span></div>';
            }
        }
    }).on('typeahead:selected', function (event, datum) {
        var $this = $(this);
        $this.attr('db-id', datum.id);
        $this.resetElement();
    });

    $('a.more-associated-words').on('click', function () {
        var section = $('#mix-section');
        if (section.panelValidate()) {
            var dataItem =
                '<div class="col-xs-12 col-sm-6 data-item">' +
                    '<div class="subsection">' +
                        '<div class="dashboard-subsection">' +
                            '<a href="#/" class="pull-right small" data-toggle="modal" data-target="#modal-dialog"' +
                                    'data-content="Are you sure you want to delete this associated word?" data-action="delete-data-item" data-buttons="CloseConfirm">' +
                                '<span class="fa fa-trash-o fa-lg fa-fw"></span><span>Delete</span>' +
                            '</a>' +
                        '</div>' +
                        '<div class="form-group">' +
                            '<label class="required">Associated Word</label>' +
                            '<input type="text" class="associated-word typeahead form-control required" autocomplete="off" , db-id="-1" , db-mix-id="-1" />' +
                        '</div>' +
                    '</div>' +
                '</div>';
            var itemContainer = section.find('div.data-item-container');
            $(dataItem).hide().appendTo(itemContainer).slideDown(function () {
                section.panelItemCount();
                itemContainer.find('input[type = text]').last().each(function () {
                    var $this = $(this);
                    $this.setUniqueId();
                    $this.typeahead({
                        hint: true,
                        highlight: true,
                        minLength: 1,
                    },
                    {
                        name: 'associatedWords',
                        displayKey: 'name',
                        source: allWords.ttAdapter(),
                        templates: {
                            empty: function () {
                                //var $this = $(this);
                                if ($this.attr('db-id') != '-1' || $this.attr('db-item') != '-1') {
                                    $this.attr('db-id', '-1');
                                }
                                return false;
                            },
                            suggestion: function (datum) {
                                return '<div class="tt-suggestion tt-selectable"><span>' + datum.name + '</span>&nbsp;<span class="note">' + datum.speechPart + '</span></div>';
                            }
                        }
                    }).on('typeahead:selected', function (event, datum) {
                        var $this = $(this);
                        $this.attr('db-id', datum.id);
                        $this.resetElement();
                    });
                }).focus();
            });
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
        parameters.Mixes = [];
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
        $('#mix-section').find('div.edit-panel').find('div.data-item').each(function (i) {
            var dataItem = $(this);
            var collocationMix = dataItem.find('input[type = text]').not('[class ~= "tt-hint"]');
            var mixId = collocationMix.attr('db-mix-id');
            var wordId = collocationMix.attr('db-id');
            parameters.Mixes[i] = { ID: mixId, Word: wordId };
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