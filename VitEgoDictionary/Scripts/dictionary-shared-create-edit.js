//  Global Variables
var speechPart = $('#speech-part').val();
var allSynonyms = [];

//  Data retrieving and element initializing
$(function () {

    //  Synonyms to be data source for synonym typeaheads
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

    //  Initializes synonym typeaheads
    $('.synonym').each(function () {
        var $this = $(this);
        var processing = $this.parents('div.data-subitem').find('img');
        $this.setUniqueId();
        $this.typeahead({
            hint: true,
            highlight: true,
            minLength: 1,
        },
        {
            name: 'allSynonyms',
            displayKey: 'name',
            source: allSynonyms.ttAdapter(),
            templates: {
                empty: function () {
                    if ($this.attr('db-id') != '-1' || $this.attr('db-item') != '-1') {
                        $this.attr('db-id', '-1');
                        $this.attr('db-item', '-1');
                    }
                    var selectpicker = $this.parents('.form-group').siblings('.form-group').first().find('select');
                    selectpicker.empty();
                    selectpicker.selectpicker('refresh');
                    return false;
                },
                suggestion: function (datum) {
                    return '<div class="tt-suggestion tt-selectable"><span>' + datum.name +
                        '</span>&nbsp;<span class="note">' + datum.speechPart + '</span></div>';
                }
            }
        }).on('typeahead:selected', function (event, datum) {
            var $this = $(this);
            $this.attr('db-id', datum.id);
            $this.attr('db-item', datum.item);
            $this.resetElement();
            var selectpicker = $this.parents('.form-group').siblings('.form-group').first().find('select');
            selectpicker.empty();
            $.ajax({
                type: "GET",
                url: "/Data/Meanings",
                contentType: 'application/json; charset=utf-8',
                data: ({ id:datum.id, item:datum.item }),
                beforeSend: function () { processing.show(); },
                success: function (data) {
                    if (data != null) {
                        $.each(data, function () {
                            selectpicker.attr('db-item', this.item)
                            selectpicker.append('<option value="' + this.id + '">' + this.name + '</option>');
                        });
                        selectpicker.selectpicker('refresh');
                    }
                },
                error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); },
                complete: function () { processing.hide(); }
            });
        });
    });

    //  Changes the title as a base word changed 
    $('#base-word').on('change', function () {
        $('#panel-body-header').find('span').first().text($(this).val());
    });

    $('#base-word').focus();
});

//  Panel toggle switching
$(function () {
    jQuery.fn.extend({

        panelToggle: function (callback) {
            var section = $(this);
            var editPanel = section.find('.edit-panel');
            if (editPanel.is(':visible')) { section.panelClose(callback); }
            else { section.panelOpen(callback); }
        },

        panelOpen: function (callback) {
            var allClosed = true;
            var section = $(this);
            var editPanel = section.find('.edit-panel');
            var duration = Math.log(editPanel.height())*100;
            var inputs = section.find('div.form-group > input[type = text]');
            var openedEditPanel = $('[id $= "-section"] > .edit-panel').not(':hidden');
            if (openedEditPanel.length > 0) {
                openedEditPanel.parents('[id $= "-section"]').panelClose(function () {
                    section.find('.edit-panel').slideDown(duration, function () {
                        if (inputs.length > 0) { inputs[0].focus(); }
                        section.find('[class *= "fa-chevron"]').removeClass('fa-chevron-right').addClass('fa-chevron-down');
                        if (typeof callback == "function") { callback(); }
                    });
                });
            } else {
                section.find('.edit-panel').slideDown(duration, function () {
                    if (inputs.length > 0) { inputs[0].focus(); }
                    section.find('[class *= "fa-chevron"]').removeClass('fa-chevron-right').addClass('fa-chevron-down');
                    if (typeof callback == "function") { callback(); }
                });
            }
            if (typeof callback == "function") { callback(); }
        },

        panelClose: function (callback) {
            var section = $(this);
            var editPanel = section.find('.edit-panel');
            var duration = Math.log(editPanel.height()) * 100;
            var result = true;
            if (editPanel.is(':visible')) {
                if (section.panelValidate()) {
                    editPanel.slideUp(duration, function () {
                        section.find('[class *= "fa-chevron"]').removeClass('fa-chevron-down').addClass('fa-chevron-right');
                        if (typeof callback == "function") { callback(); }
                    });
                } else { result = false; }
            } else { if (typeof callback == "function") { callback(); } }
            return result;
        },

        panelValidate: function () {
            var form = $(this).find('form');
            return form.validateForm();
        },

        panelItemCount: function () {
            var section = $(this);
            var count = 0;
            //  For variation section, only filled inputs are counted
            if (section.attr('id') == 'variation-section') {
                section.find('div.edit-panel').find('div.data-item').find('input[type = text]').each(function () {
                    if ($(this).val()) count++;
                });
            } else {
                count = section.find('div.edit-panel').find('div.data-item').length;
            }
            section.find('span.item-counter').text(count + (count == 1 ? ' item' : ' items'));
        }
    });

    $('.panel-toggle').on('click', function () {
        $(this).parents('[id $= "-section"]').panelToggle();
    });
});

//  Data items adding & removing
$(function () {
    //  Adds the entire meaning data item
    $('a.more-meanings').on('click', function () {
        var section = $('#meaning-section');
        if (section.panelValidate()) {
            var dataItem =
                '<div class="subsection data-item">' +
                    '<div class="dashboard-subsection">' +
                        '<span class="dashboard-title">Meaning</span>' +
                        '<a href="#/" class="pull-right small" data-toggle="modal" data-target="#modal-dialog"' +
                            'data-content="Are you sure you want to delete this meaning?" data-action="delete-data-item" data-buttons="CloseConfirm">' +
                            '<span class="fa fa-trash-o fa-lg fa-fw"></span><span>Delete</span>' +
                        '</a>' +
                    '</div>' +
                    '<div class="row">' +
                        '<div class="col-xs-12 col-sm-6 form-group">' +
                            '<label class="required">Meaning</label>' +
                            '<input type="text" autocomplete="off" class="meaning form-control required" db-id="-1">' +
                        '</div>' +
                        '<div class="col-xs-12 col-sm-6">' +
                            '<div class="data-example-subitem-container">' +
                                '<div class="form-group data-subitem">' +
                                    '<label class="required">Example</label>' +
                                    '<a href="#/" class="inner pull-right small" data-toggle="modal" data-target="#modal-dialog"' +
                                        'data-content="Are you sure you want to delete this example?" data-action="delete-data-subitem" data-buttons="CloseConfirm">' +
                                        '<span class="fa fa-trash-o fa-lg fa-fw"></span>' +
                                    '</a>' +
                                    '<input type="text" class="example inner form-control required" autocomplete="off" , db-id="-1" />' +
                                '</div>' +
                            '</div>' +
                            '<div class="text-right form-group">' +
                                '<a href="#/" class="more-meaning-examples small">' +
                                    '<span class="fa fa-plus-square-o fa-lg fa-fw"></span><span>More examples</span>' +
                                '</a>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                    '<hr style="border-color:#DDD" />' +
                    '<div class="synonyms">' +
                        '<div class="data-synonym-subitem-container">' +
                        '</div>' +
                        '<div>' +
                            '<a href="#/" class="more-synonyms small">' +
                                '<span class="fa fa-plus-square-o fa-lg fa-fw"></span><span>More synonyms</span>' +
                            '</a>' +
                        '</div>' +
                    '</div>' +
                '</div>';
            var itemContainer = section.find('div.data-item-container');
            $(dataItem).hide().appendTo(itemContainer).slideDown(function () {
                section.panelItemCount();
                itemContainer.find('input[type=text]').each(function () { $(this).setUniqueId(); });
                itemContainer.find('input[type = text][class ~= meaning]').last().focus();
            });
        }
    });

    //  Adds one more meaning example data subitem
    $('div.edit-panel').on('click', 'a.more-meaning-examples', (function () {
        var $this = $(this);
        var section = $this.parents('div.section');
        if (section.panelValidate()) {
            var dataExampleItem =
                '<div class="form-group data-subitem">' +
                    '<label class="required">Example</label>' +
                    '<a href="#/" class="inner pull-right small" data-toggle="modal" data-target="#modal-dialog"' +
                        'data-content="Are you sure you want to delete this example?" data-action="delete-data-subitem" data-buttons="CloseConfirm">' +
                        '<span class="fa fa-trash-o fa-lg fa-fw"></span>' +
                    '</a>' +
                    '<input type="text" class="example inner form-control required" autocomplete="off" , db-id="-1" />' +
                '</div>';
            var itemContainer = $this.parents('div.data-item').find('div.data-example-subitem-container');
            $(dataExampleItem).hide().appendTo(itemContainer).slideDown(function () {
                itemContainer.find('input[type = text]').last().setUniqueId().focus();
            });
        }
    }));

    //  Adds more synonym data subitem
    $('div.edit-panel').on('click', 'a.more-synonyms', function () {
        var $link = $(this);
        var section = $link.parents('div.section');
        if (section.panelValidate()) {
            var dataItem =
                '<div class="subsection data-subitem">' +
                    '<div class="dashboard-subsection">' +
                        '<span class="dashboard-title">Synonym</span>' +
                        '<a href="#/" class="pull-right small" data-toggle="modal" data-target="#modal-dialog"' +
                            'data-content="Are you sure you want to delete this synonym from the synonym set?"' +
                            'data-action="delete-data-subitem" data-buttons="CloseConfirm">' +
                            '<span class="fa fa-trash-o fa-lg fa-fw"></span><span>Delete</span>' +
                        '</a>' +
                    '</div>' +
                    '<div class="row">' +
                        '<div class="col-xs-12 col-sm-6 form-group">' +
                            '<label class="required">Synonym</label>' +
                            '<input type="text" class="synonym typeahead form-control required"' +
                                'autocomplete="off" db-id="-1" db-item="-1" />' +
                        '</div>' +
                        '<div class="col-xs-12 col-sm-6 form-group">' +
                            '<label style="margin-right:5px">Synonym Meaning</label>' +
                            '<img style="display:none" src="/Content/Style/Pics/processing.gif" alt="processing..." /> ' +
                            '<select class="selectpicker form-control synonym-meaning" db-item="-1" >' +
                            '</select>' +
                        '</div>' +
                    '</div>' +
                '</div>';
            var itemContainer = $link.parents('div.synonyms').find('div.data-synonym-subitem-container');
            $(dataItem).hide().appendTo(itemContainer).slideDown(function () {
                itemContainer.find('input[type = text]').last().each(function () {
                    var $this = $(this);
                    $this.setUniqueId();
                    $this.typeahead({
                        hint: true,
                        highlight: true,
                        minLength: 1,
                    },
                    {
                        name: 'allSynonyms',
                        displayKey: 'name',
                        source: allSynonyms.ttAdapter(),
                        templates: {
                            empty: function () {
                                if ($this.attr('db-id') != '-1' || $this.attr('db-item') != '-1') {
                                    $this.attr('db-id', '-1');
                                    $this.attr('db-item', '-1');
                                }
                                var selectpicker = $this.parents('.form-group').siblings('.form-group').first().find('select');
                                selectpicker.empty();
                                selectpicker.selectpicker('refresh');
                                return false;
                            },
                            suggestion: function (datum) {
                                return '<div class="tt-suggestion tt-selectable"><span>' + datum.name +
                                    '</span>&nbsp;<span class="note">' + datum.speechPart + '</span></div>';
                            }
                        }
                    }).on('typeahead:selected', function (event, datum) {
                        var $this = $(this);
                        var processing = $this.parents('div.data-subitem').find('img');
                        $this.attr('db-id', datum.id);
                        $this.attr('db-item', datum.item);
                        $this.resetElement();
                        var selectpicker = $this.parents('.form-group').siblings('.form-group').first().find('select');
                        selectpicker.empty();
                        $.ajax({
                            type: "GET",
                            url: "/Data/Meanings",
                            contentType: 'application/json; charset=utf-8',
                            data: ({ id: datum.id, item: datum.item }),
                            beforeSend: function () { processing.show(); },
                            success: function (data) {
                                if (data != null) {
                                    $.each(data, function () {
                                        selectpicker.attr('db-item', this.item)
                                        selectpicker.append('<option value="' + this.id + '">' + this.name + '</option>');
                                    });
                                    selectpicker.selectpicker('refresh');
                                }
                            },
                            error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); },
                            complete: function () { processing.hide(); }
                        });
                    });
                }).focus();
            });
        }
    });
});

//  Input tabindex switching
$(function () {
    //  Tabindex switching in the meaning section
    $('#meaning-section').find('div.edit-panel').on('keypress', 'input[type = text]', (function (event) {
        $this = $(this);
        if (event.which == 13 && $this.validateElement()) {
            if ($this.hasClass('meaning')) {//  From 'meaning' focus jumps to 'meaning example'
                $this.parents('.form-group').siblings().first().find('.data-example-subitem-container').find('input[type=text]').first().focus();
            } else if ($this.hasClass('example')) { //From 'meaning example' focus jumps to 'synonyms', or to next 'data-item', or to 'meaning' back
                var nextItem = undefined;
                var nextSubitem = $this.parents('.data-subitem').next('.data-subitem');
                if (nextSubitem.length > 0) {
                    nextSubitem.find('input[type=text]').first().focus();            
                } else {
                    nextSubitem = $this.parents('.data-item').find('.data-synonym-subitem-container').find('.data-subitem');
                    if (nextSubitem.length > 0) {
                        nextSubitem.find('input[type=text]').not('[class ~= "tt-hint"]').first().focus();
                    } else {
                        nextItem = $this.parents('.data-item').siblings('.data-item');
                        if (nextItem.length > 0) {
                            nextItem.find('input[type=text][class ~= meaning]').first().focus();
                        } else {
                            $this.parents('.data-item').find('input[type=text][class ~= meaning]').first().focus();
                        }
                    }
                }
            } else if ($this.hasClass('synonym')) {
                var nextSubitem = $this.parents('.data-subitem').next('.data-subitem');
                if (nextSubitem.length > 0) {
                    nextSubitem.find('input[type=text]').first().focus();
                } else {
                    nextItem = $this.parents('.data-item').siblings('.data-item');
                    if (nextItem.length > 0) {
                        nextItem.find('input[type=text][class ~= meaning]').first().focus();
                    } else {
                        $this.parents('.data-item').find('input[type=text][class ~= meaning]').first().focus();
                    }
                }
            }
        }
    }));
});