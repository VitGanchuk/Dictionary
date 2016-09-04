//  Pagination functions
$(function () {
    $('div.pagination-container').on('click', 'ul.pagination > li > a', function () {
        var itemType = $('#item').val();
        var page = $('#page');
        var depature = page.val();
        var destination = $(this).attr('data-page');
        if (depature != destination) {
            var parameters = {
                Destination: destination
            };
            if(itemType == 'Word') if ($('#speech-parts').val() != 'all') { parameters.SpeechParts = $('#speech-parts').val(); }
            if(itemType != 'PhrasalVerb') if ($('#topics').val() != 'all') { parameters.Topics = $('#topics').val(); }
            if ($('#formalities').val() != 'all') { parameters.Formalities = $('#formalities').val(); }
            $.ajax({
                type: "POST",
                url: "/" + itemType + "/LoadItems",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ parameters: parameters }),
                beforeSend: function () { /*$('#process').modal('show');*/ },
                success: function (data) {
                    var list = $('ul.index-list');
                    list.fadeOut(function () {
                        page.val(data.page);
                        var pager = $('ul.pagination');
                        pager.empty();
                        pager.append('<li><a href="#/" aria-label="First" data-page="0"><span class="fa fa-step-backward"></span></a></li>');
                        for (var i = 0; i < data.pagesToDisplay.length; i++) {
                            var item = '<li class="item"><a href="#/" data-page="' + data.pagesToDisplay[i] + '">' + (data.pagesToDisplay[i] + 1) + '</a></li';
                            pager.append(item);
                        }
                        pager.append('<li><a href="#/" aria-label="Last" data-page="' + (data.overallPages - 1) + '"><span class="fa fa-step-forward"></span></a></li>');
                        pager.children('li.item').has('a[data-page = "' + data.page + '"]').addClass('active');
                        list.empty('');
                        for (var i = 0; i < data.items.length; i++) {
                            var item =
                                '<li>' +
                                    '<a href="/' + itemType + '/Item/' + data.items[i].id + '"><span class="index-list-item">' + data.items[i].name + '&nbsp;</span></a>';
                                    if (itemType == 'Word') { item = item + '<span class="speech-part">' + data.items[i].speechPart + '&nbsp;</span>'; }
                                    if (itemType != 'PhrasalVerb') item = item + '<span class="topic">(' + data.items[i].topic + ')</span>&nbsp;';
                                    item = item + '<ul class="meaning-list">';
                                    for (var j = 0; j < data.items[i].meanings.length; j++) {
                                        item = item + '<li><span class="meaning">' + data.items[i].meanings[j].meaning + '</li></span>';
                                    }
                                    item = item + '</ul>' +
                                '</li>';
                            list.append(item);
                        }
                        list.fadeIn();
                    });
                },
                error: function (jqXhr, status, error) { showHttpRequestError(jqXhr, status, error); },
                complete: function () { /*$('#process').modal('hide');*/ }
            });
        }
    });
});