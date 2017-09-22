/** Alert Position Top  **/ 
! function ($) {
    'use strict';
    $("#showtop").on('click', function () {
        $("#alerttop").fadeToggle(350);
    });

    /** Alert Position Bottom  **/

    $("#showbottom").on('click', function () {
        $("#alertbottom").fadeToggle(350);
    });

    /** Alert Position Top Left  **/

    $("#showtopleft").on('click', function () {
        $("#alerttopleft").fadeToggle(350);
    });

    /** Alert Position Top Right  **/

    $("#showtopright").on('click', function () {
        $("#alerttopright").fadeToggle(350);
    });

    /** Alert Position Bottom Left  **/

    $("#showbottomleft").on('click', function () {
        $("#alertbottomleft").fadeToggle(350);
    });

    /** Alert Position Bottom Right  **/

    $("#showbottomright").on('click', function () {
        $("#alertbottomright").fadeToggle(350);
    });
}(window.jQuery);