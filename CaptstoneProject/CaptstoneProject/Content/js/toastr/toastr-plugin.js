$(document).ready(function () {
    'use strict';

    /* Toastr */
    setTimeout(function () {
        toastr.options = {
            closeButton: true,
            progressBar: true,
            showMethod: 'slideDown',
            timeOut: 4000
        };
        toastr.success('Responsive Admin Theme', 'Welcome to Foxlabel');
    }, 1300);
});

