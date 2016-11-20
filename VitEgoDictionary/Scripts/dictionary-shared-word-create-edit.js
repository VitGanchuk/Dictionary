//  Global Variables
var allPrepositions = [];
var genericWords = undefined;

//  Data retrieving and element initializing
$(function () {
    //  Gets all prepositions and keeps them
    $.ajax({
        type: 'GET',
        url: '/Data/Prepositions',
        contentType: 'application/json; charset=utf-8',
        success: function (data) { allPrepositions = data; },
        error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); }
    });

    genericWords = new Bloodhound({
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

    //  Initializes generic word typeahead
    $('#generic-word').typeahead({
        hint: true,
        highlight: true,
        minLength: 1,
    },
    {
        name: 'genericWords',
        displayKey: 'name',
        source: genericWords.ttAdapter(),
        templates: {
            empty: function () {
                //var $this = $(this);
                if ($this.attr('db-id') != '-1' || $this.attr('db-item') != '-1') {
                    $this.attr('db-id', '-1');
                    $this.attr('db-item', '-1');
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
        $this.attr('db-item', datum.item);
        $this.resetElement();
    });

    //  Makes some changes as the part of speech is changed
    $('#speech-part').on('change', (function () {
        $this = $(this);
        speechPart = $this.val();
        $('#panel-body-header').find('span').last().text($this.children('option:selected').text());
        //  Changes possible generic words
        $('#general-word-processing').show();
        $('#variation-processing').show();
        genericWords = new Bloodhound({
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
        $('#generic-word').typeahead('destroy').val('').attr('db-id','-1');
        $('#generic-word').typeahead({
            hint: true,
            highlight: true,
            minLength: 1,
        },
        {
            name: 'genericWords',
            displayKey: 'name',
            source: genericWords.ttAdapter(),
            templates: {
                empty: function () {
                    //var $this = $(this);
                    if ($this.attr('db-id') != '-1' || $this.attr('db-item') != '-1') {
                        $this.attr('db-id', '-1');
                        $this.attr('db-item', '-1');
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
            $this.attr('db-item', datum.item);
            $this.resetElement();
        });
        //  Removes all word and phrasal-verb synonyms
        $('input[type=text][class ~= synonym][db-item = 0],[db-item = 1]').parents('div[class ~= data-subitem]').slideUp(function () {
            $(this).remove();
        });
        //  Changes synonyms to be data source for synonym typeaheads
        allSynonyms = new Bloodhound({
            datumTokenizer: function (datum) {
                return Bloodhound.tokenizers.whitespace(datum.name);
            },
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            limit: 10,
            prefetch: {
                url: '/Data/Synonyms/?speechPart=' + speechPart,
                cache: false,
                filter: function (list) {
                    return $.map(list, function (item) {
                        return {
                            id: item.id,
                            name: item.name,
                            speechPart: item.speechPart,
                            item: item.item
                        };
                    });
                }
            }
        });
        //  Changes possible variations 
        //  Variation section is for nouns, verbs, adjectives, and adverbs only
        var variationSection = $('#variation-section');
        variationSection.panelClose(function () {
            variationSection.find('.data-item').remove();
            variationSection.panelItemCount();
        });
        if (speechPart == 1 || speechPart == 2 || speechPart == 3 || speechPart == 4) {
            $.ajax({
                type: 'GET',
                url: '/Data/GrammarStructures',
                data: { speechPart: parseInt(speechPart) },
                beforeSend: function () { },
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    variationSection.slideDown();
                    $.each(data, function () {
                        var dataItem =
                            '<div class="col-xs-12 col-sm-6 col-md-3 form-group data-item">' +
                                '<label>' + this.name + '</label>' +
                                '<input type="text" autocomplete="off" class="form-control" db-id="' + this.id + '">' +
                            '</div>';
                        var itemContainer = variationSection.find('div.data-item-container');
                        $(dataItem).appendTo(itemContainer)
                    });
                },
                error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); },
                complete: function () {
                    $('#general-word-processing').hide();
                    $('#variation-processing').hide();
                }
            });
        } else {
            variationSection.slideUp();
            $('#general-word-processing').hide();
            $('#variation-processing').hide();
        }

        //  Changes possibles countabilities
        //  Countability is for nouns only
        var countabilityPicker = $('#countability');
        if (speechPart == 1) {
            countabilityPicker.children('option').first().remove();
            countabilityPicker.attr('disabled', false);
            countabilityPicker.selectpicker('refresh');
        } else {
            countabilityPicker.prepend('<option value="">not applicable</option>');
            countabilityPicker.val('');
            countabilityPicker.attr('disabled', true);
            countabilityPicker.selectpicker('refresh');
        }
    }));

    //  Makes some changes as the countability is changed
    $('#countability').on('change', function () {
        var $this = $(this);
        var variationSection = $('#variation-section');
        //  Hides and cleans up the section
        if ($this.val() == 2) {
            variationSection.slideUp(function () {
                variationSection.find('input[type=text]').each(function () {
                    $(this).val('');
                });
                variationSection.panelItemCount();
            });  
        } else {
            variationSection.find('.edit-panel').hide();
            variationSection.slideDown();
        }
    });
});

//  Data items adding & removing
$(function () {
    //  Counts data item in the variation section
    $('#variation-section').find('div.edit-panel').on('blur', 'input[type = text]', (function (i) {
        $('#variation-section').panelItemCount();
    }));

    //  Adds the entire preposition data item
    $('a.more-prepositions').on('click', function () {
        var section = $('#preposition-section');
        if (section.panelValidate()) {
            var dataItem =
                '<div class="subsection data-item">' +
                    '<div class="dashboard-subsection">' +
                        '<span class="dashboard-title">Preposition</span>' +
                        '<a href="#/" class="pull-right small" data-toggle="modal" data-target="#modal-dialog"' +
                            'data-content="Are you sure you want to delete this preposition?"' +
                            'data-action="delete-data-item" data-buttons="CloseDelete">' +
                            '<span class="fa fa-trash-o fa-lg fa-fw"></span><span>Delete</span>' +
                        '</a>' +
                    '</div>' +
                    '<div class="row">' +
                        '<div class="col-xs-12 col-sm-6 form-group">' +
                            '<label>Preposition</label>' +
                            '<select class="preposition selectpicker form-control" data-live-search="true" db-id="-1" >' +
                            '</select>' +
                        '</div>' +
                        '<div class="col-xs-12 col-sm-6">' +
                            '<div class="data-example-subitem-container">' +
                                '<div class="form-group data-subitem">' +
                                    '<label class="required">Example</label>' +
                                    '<a href="#/" class="inner pull-right" data-toggle="modal" data-target="#modal-dialog"' +
                                        'data-content="Are you sure you want to delete this example?"' +
                                        'data-action="delete-data-subitem" data-buttons="CloseDelete">' +
                                        '<span class="fa fa-trash-o fa-fw"></span>' +
                                    '</a>' +
                                    '<input type="text" autocomplete="off" class="example inner form-control required" db-id="-1">' +
                                '</div>' +
                            '</div>' +
                            '<div class="text-right small form-group">' +
                                '<a href="#/" class="more-meaning-examples">' +
                                    '<span class="fa fa-plus-square-o fa-lg fa-fw"></span><span>More examples</span>' +
                                '</a>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>';
            var itemContainer = section.find('div.data-item-container');
            var availablePrepositions = allPrepositions;
            $('select.preposition').each(function () {
                var selected = $(this).val();
                availablePrepositions = $.grep(availablePrepositions, function (item) { return item.id != selected; });
            });
            $(dataItem).hide().appendTo(itemContainer).slideDown(function () {
                section.panelItemCount();
                var selectpicker = itemContainer.find('select[class ~= preposition]');
                $.each(availablePrepositions, function () {
                    selectpicker.append('<option value="' + this.id + '">' + this.name + '</option>');
                });
                selectpicker.selectpicker({ size: 5 });
            });
        }
    });
});

//  Input tabindex switching
$(function () {
    //  Tabindex switching in the variation section
    $('#variation-section').find('div.edit-panel').on('keypress', 'input[type = text]', (function (event) {    
        if (event.which == 13) {
            event.preventDefault();
            var dataItem = $(this).parents('div.data-item').next();
            if (dataItem.length == 0) {
                dataItem = $('#variation-section').find('div.data-item').first();
            } 
            dataItem.find('input[type=text]').first().focus();

        }
    }));

    //  Tabindex switching in the preposition section
    $('#preposition-section').on('change', 'select.preposition', function () {
        var exampleItemContainer = $(this).parents('div.data-item').find('div.data-example-subitem-container');
        exampleItemContainer.find('input[type = text]').last().focus();
    });

    //  Tabindex switching in the preposition section
    $('#preposition-section').find('div.edit-panel').on('keypress', 'input[type = text][class ~= example]', (function (event) {
        if (event.which == 13) {
            var $this = $(this);
            var dataExampleItem = $this.parents('div.data-subitem').next('div.data-subitem');
            // There's the next example item for this data item: focus on it
            if (dataExampleItem.length > 0) {
                dataExampleItem.find('input[type = text]').first().focus();
            //  There's no next example item for this data item: look for the next data item
            } else {
                var dataItem = $this.parents('div.data-item').next('div.data-item');
                // There's the next data item: focus on its first data example item's input 
                if (dataItem.length > 0) {
                    dataItem.find('div.data-subitem').first().find('input[type = text]').focus();
                    //  There's no next data item: look for the first data item and focus on its only input with a class of 'meaning'
                } else {
                    dataItem = $this.parents('div.data-item-container').find('div.data-item').first();
                    dataItem.find('div.data-subitem').first().find('input[type = text]').focus();
                }
            }
        }
    }));
});

//  Form submit & validation functions
$('#submit').on('click', function () {
    var $form = $('#item-form-base');
    if ($form.validateForm()) {
        var parameters = {
            ID: $('#base-word').attr('db-id'),
            Name: $.trim($('#base-word').val().toLowerCase()),
            SpeechPart: $('#speech-part').val(),
            Topic: $('#topic').val(),
            Formality: $('#formality').val(),
            Coutability: $('#countability').val(),
            GenericWord: $('#generic-word').attr('db-id')
        };
        parameters.Variations = [];
        parameters.Meanings = [];
        parameters.PrepositionMix = [];
        $('#variation-section').find('div.edit-panel').find('div.data-item').each(function () {
            var variation = $(this).find('input[type = text]');
            if (variation.val()) {
                var structureId = variation.attr('db-id');
                parameters.Variations.push({ Name: variation.val(), GrammarStructure: structureId });
            }
        });
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
        $('#preposition-section').find('div.edit-panel').find('div.data-item').each(function (i) {
            var dataItem = $(this);
            var preposition = dataItem.find('select[class ~= "preposition"]');
            var prepositionMixId = preposition.attr('db-id');
            var examples = [];
            dataItem.find('div.data-subitem').each(function (j) {
                var dataExampleItem = $(this);
                var example = dataExampleItem.find('input[type = text]');
                var exampleId = example.attr('db-id');
                examples[j] = { ID: exampleId, Example: $.trim(example.val()) };
            });
            parameters.PrepositionMix[i] = { ID: prepositionMixId, Preposition: preposition.val(), Examples: examples };
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