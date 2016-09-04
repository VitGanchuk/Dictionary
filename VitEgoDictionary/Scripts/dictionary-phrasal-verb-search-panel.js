//  Data retrieving and element initializing
$(function () {
    //  Phrasal verbs to be searched
    var phrasalVerbs = new Bloodhound({
        datumTokenizer: function (datum) {
            return Bloodhound.tokenizers.whitespace(datum.name);
        },
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        limit: 10,
        prefetch: {
            url: '/Data/PhrasalVerbs',
            cache: false,
            filter: function (list) {
                return $.map(list, function (item) {
                    return {
                        id: item.id,
                        name: item.name
                    };
                });
            }
        }
    });

    //  Base verbs to initialize the appropriate typeahed 
    var baseVerbs = new Bloodhound({
        datumTokenizer: function (datum) {
            return Bloodhound.tokenizers.whitespace(datum.name);
        },
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        limit: 10,
        prefetch: {
            url: '/Data/Words/?speechPart=2',
            data: { speechPart: 2 },
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

    //  Initializes search typeahead
    $('#search').typeahead({
        hint: true,
        highlight: true,
        minLength: 1,
    },
    {
        name: 'phrasalVerbs',
        displayKey: 'name',
        source: phrasalVerbs.ttAdapter(),
        templates: {
            empty: function () {
                //var $this = $(this);
                if ($this.attr('db-id') != '-1') { $this.attr('db-id', '-1'); }
                return false;
            },
            suggestion: function (datum) {
                return '<div class="tt-suggestion tt-selectable"><span>' + datum.name + '</span></div>';
            }
        }
    }).on('typeahead:selected', function (event, datum) {
        var $this = $(this);
        $this.attr('db-id', datum.id);
        $this.resetElement();
        $('#search-form').submit();
    }).focus();

    //  Initializes base verb typeahead
    var baseVerb =  $('#base-verb');
    baseVerb.typeahead({
        hint: true,
        highlight: true,
        minLength: 1,
    },
    {
        name: 'baseVerbs',
        displayKey: 'name',
        source: baseVerbs.ttAdapter(),
        templates: {
            empty: function () {
                //var $this = $(this);
                if ($this.attr('db-id') != '-1') { $this.attr('db-id', '-1'); }
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
});

//  Form submit & validation functions
$(function () {
    //  Validates and submits the simple search form
    $('#search-form').submit(function(event) {
        event.preventDefault();
        var id = $('#search').attr('db-id');
        $.ajax({
            type: "GET",
            url: "/PhrasalVerb/Item/" + id,
            contentType: 'application/json; charset=utf-8',
            //data: JSON.stringify({ id: id }),
            beforeSend: function () { /*$('#process').modal('show');*/ },
            success: function (data) {
                if (data.result == "redirect") {
                    window.location = data.url;
                } else if (data.result == "error") { showServerSideError(data); }
            },
            error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); },
            complete: function () { /*$('#process').modal('hide');*/ }
        });
    });

    //  Validates and submits the advanced search form
    $('#submit').on('click', function () {
        $('#advanced-search-cancel').click();
        var $form = $('#search-advanced-form');
        var baseVerb = $('#base-verb');
        if (!baseVerb.val()) { baseVerb.attr('db-id', '-1'); }
        if ($form.validateForm())
        {
            $('#page').val('0');
            var parameters = {
                Destination: 0,
                BaseVerb: baseVerb.attr('db-id')
            };
            if ($('#formalities').val() != 'all') { parameters.Formalities = $('#formalities').val(); }
            $.ajax({
                type: "POST",
                url: "/PhrasalVerb/LoadItems",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ parameters: parameters }),
                beforeSend: function () { /*$('#process').modal('show');*/ },
                success: function (data) {
                    var list = $('ul.index-list');
                    var header = $('#panel-body-header');
                    list.add(header).fadeOut(function () {
                        var pager = $('ul.pagination');
                        pager.empty();
                        pager.append('<li><a href="#/" aria-label="First" data-page="0"><span aria-hidden="true">&laquo;</span></a></li>');
                        for (var i = 0; i < data.pagesToDisplay.length; i++) {
                            var item = '<li class="item"><a href="#/" data-page="' + data.pagesToDisplay[i] + '">' + (data.pagesToDisplay[i] + 1) + '</a></li';
                            pager.append(item);
                        }
                        pager.append('<li><a href="#/" aria-label="Last" data-page="' + (data.overallPages - 1) + '"><span aria-hidden="true">&raquo;</span></a></li>');
                        pager.children('li.item').has('a[data-page = "' + data.page + '"]').addClass('active');
                        list.empty('');
                        if (data.items.length == 0) {
                            item =
                                '<li>' +
                                    '<p class="warning">Sorry, no results were found! Please try to search again with another parameters!</p>' +
                                '</li>';
                            list.append(item);
                        } else {
                            for (var i = 0; i < data.items.length; i++) {
                                var item =
                                    '<li>' +
                                        '<a href="/PhrasalVerb/Item/' + data.items[i].id + '"><span class="index-list-item">' + data.items[i].name + '</span></a>' +
                                        '<ul class="meaning-list">';
                                        for (var j = 0; j < data.items[i].meanings.length; j++) {
                                            item = item + '<li><span class="meaning">' + data.items[i].meanings[j].meaning + '</li></span>';
                                        }
                                        item = item + '</ul>' +
                                    '</li>';
                                list.append(item);
                            }
                        }
                        var span = '<span class="pull-right note">';
                        if (data.isFiltered) span = span + '<span class="fa fa-filter fa-fw"></span>';
                        else span = span + '<span class="fa fa-hashtag fa-fw"></span>&nbsp;';
                        span = span + data.overallRecords + '</span>';
                        $(header).text('Search Results').append(span);
                        list.add(header).fadeIn();
                    });
                },
                error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); },
                complete: function () { /*$('#process').modal('hide');*/ }
            });
        }
    });
});