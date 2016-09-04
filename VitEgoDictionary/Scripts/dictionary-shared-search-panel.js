//  Data retrieving and element initializing
$(function () {
    //  Castomizes selectpicker inputs to handle select ALL option
    $('.selectpicker').on('change', function (event) {
        $this = $(this);
        if (!$this.val()) {
            $this.val('all');
            $this.selectpicker('refresh');
        } else {
            if ($.inArray('all', $this.val()) >= 0) {
                var all = $this.siblings('input[type = hidden]');
                if (all.val() == 'true') {
                    $this.val($.grep($this.val(), function (i) { return i != 'all' }));
                    $this.selectpicker('refresh');
                    all.val(false);
                } else {
                    $this.val($.grep($this.val(), function (i) { return i == 'all' }));
                    $this.selectpicker('refresh');
                    all.val(true);
                }
            }
        }
    });

    //  Switches between simple and advanced search modes
    $('#advanced-search-open').on('click', function () {
        $this = $(this);
        var simplePanel = $('#simple-search-panel');
        var advancedPanel = $('#advanced-search-panel');
        if (simplePanel.is(':visible')) {
            simplePanel.slideUp(function () {
                advancedPanel.slideDown(function () {
                    var baseVerb = $('#base-verb');
                    if (baseVerb.attr('db-id') == '-1') baseVerb.focus();
                });
            }); 
        }
    });

    //  Switches between simple and advanced search modes
    $('#advanced-search-cancel').on('click', function () {
        $this = $(this);
        var simplePanel = $('#simple-search-panel');
        var advancedPanel = $('#advanced-search-panel');
        if (advancedPanel.is(':visible')) {
            advancedPanel.slideUp(function () {
                simplePanel.slideDown(function () { $('#search').focus(); });
            });
        }
    });
});