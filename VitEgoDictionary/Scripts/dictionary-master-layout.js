$(function () {
    //  Open navigation menu in the mobile mode
    $('ul.navigation-mobile > li > a').on('click', function () {
        var $this = $(this);
        var menu = $this.next('ul');
        if (menu.is(':visible')) {
            menu.slideUp(300);
        } else {
            menu.slideDown(300);
        }
    });

    //  Initializes some elements 
    $('.selectpicker').selectpicker({ size: 5 });

    $("#owl-carousel").owlCarousel({
        singleItem: true,
        autoPlay: true,
        rewindNav: true,
        stopOnHover: false,
        autoHeight: true,
        pagination: false,
        transitionStyle: "fade"
    });
});