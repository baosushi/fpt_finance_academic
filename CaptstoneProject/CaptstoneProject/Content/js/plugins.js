$(document).ready(function () {
    'use strict';

    /* MetisMenu */
    $('#side-menu').metisMenu();

    /* Footable */
    if ($(".footable").length) {
        $('.footable').footable();
    }

    if ($(".product-images").length) {
        $('.product-images').slick({
            dots: true
        });
    }

    /* Top Stats Show Hide */
    $("#topstats").on('click', function () {
        $(".topstats").slideToggle(100);
    });

    /* Sidepanel Show-Hide */

    $(".sidepanel-open-button").on('click', function () {
        $(".sidepanel").toggle(100);
    });

    /* Sidebar Show-Hide On Mobile */

    $(".sidebar-open-button-mobile").on('click', function () {
        $(".sidebar").toggle(150);
    });

    /* Sidebar Show-Hide */

    $('.sidebar-open-button').on('click', function () {
        if ($('.sidebar').hasClass('hidden')) {
            $('.sidebar').removeClass('hidden');
            $('.content').css({
                'marginLeft': 250
            });
        } else {
            $('.sidebar').addClass('hidden');
            $('.content').css({
                'marginLeft': 0
            });
        }
    });

    /* CounterUp */
    $().counterUp && $("[data-counter='counterup']").counterUp({
        delay: 10,
        time: 1e3
    });

    /* Agile Sortable */
    if ($("#todo, #inprogress, #completed").length) {
        $("#todo, #inprogress, #completed").sortable({
            connectWith: ".connectList",
            update: function( event, ui ) {

                var todo = $( "#todo" ).sortable( "toArray" );
                var inprogress = $( "#inprogress" ).sortable( "toArray" );
                var completed = $( "#completed" ).sortable( "toArray" );
                $('.output').html("ToDo: " + window.JSON.stringify(todo) + "<br/>" + "In Progress: " + window.JSON.stringify(inprogress) + "<br/>" + "Completed: " + window.JSON.stringify(completed));
            }
        }).disableSelection();
    }

    /* ===========================================================
     PANEL TOOLS
     ===========================================================*/
     /* Minimize */
     $(".panel-tools .minimise-tool").on('click', function (event) {
        $(this).parents(".panel").find(".panel-body").slideToggle(100);

        return false;
    });

     /* Close */

     $(".panel-tools .closed-tool").on('click', function (event) {
        $(this).parents(".panel").fadeToggle(400);

        return false;
    });

     /* Search */

     $(".panel-tools .search-tool").on('click', function (event) {
        $(this).parents(".panel").find(".panel-search").toggle(100);

        return false;
    });

     /* expand */

     $('.panel-tools .expand-tool').on('click', function () {
        if ($(this).parents(".panel").hasClass('panel-fullsize')) {
            $(this).parents(".panel").removeClass('panel-fullsize');
        } else {
            $(this).parents(".panel").addClass('panel-fullsize');
        }
    });

    /* ===========================================================
     Widget Tools
     ===========================================================*/
     /* Close */
     $(".widget-tools .closed-tool").on('click', function (event) {
        $(this).parents(".widget").fadeToggle(400);

        return false;
    });

     /* expand */
     $('.widget-tools .expand-tool').on('click', function () {
        if ($(this).parents(".widget").hasClass('widget-fullsize')) {
            $(this).parents(".widget").removeClass('widget-fullsize');
        } else {
            $(this).parents(".widget").addClass('widget-fullsize');

        }
    });

     /* Foxlabel Alerts */
     /* Default */
     $(".foxlabel-alert .closed").on('click', function (event) {
        $(this).parents(".foxlabel-alert").fadeToggle(350);

        return false;
    });

     /* Click to close */
     $(".foxlabel-alert-click").on('click', function (event) {
        $(this).fadeToggle(350);

        return false;
    });


     $(function () {
        /* Tooltips */
        $('[data-toggle="tooltip"]').tooltip();

        /* Popover */
        $('[data-toggle="popover"]').popover();
        

    });

     /* Page Loading */
     $(window).on('load', function () {
        $(".loading").fadeOut(750);
    });

     /* Ladda Button */
     if ($(".ladda-button").length) {
         Ladda.bind( '.ladda-button', { timeout: 2000 } );

         Ladda.bind(".ladda-button:not(.ladda-progress)", {
            timeout: 2e3
        }), Ladda.bind(".ladda-progress", {
            callback: function(a) {
                var t = 0,
                d = setInterval(function() {
                    t = Math.min(t + .1 * Math.random(), 1), a.setProgress(t), 1 === t && (a.stop(), clearInterval(d))
                }, 200)
            }
        })
    }

    /* initialize the external events
    -----------------------------------------------------------------*/
    $('#external-events .fc-event').each(function () {

        // store data so the calendar knows to render an event upon drop
        $(this).data('event', {
            title: $.trim($(this).text()), // use the element's text as the event title
            stick: true // maintain when user navigates (see docs on the renderEvent method)
        });

        // make the event draggable using jQuery UI
        $(this).draggable({
            zIndex: 999,
            revert: true, // will cause the event to go back to its
            revertDuration: 0 //  original position after the drag
        });

    });

    /*    Calendar widget    */
    if ($("#calendar").length) {
        /* initialize the calendar
        -----------------------------------------------------------------*/
        $('#calendar').fullCalendar({
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,basicWeek,basicDay'
            },
            defaultDate: '2017-02-12',
            editable: true,
            droppable: true, // this allows things to be dropped onto the calendar
            eventLimit: true, // allow "more" link when too many events
            events: [{
                title: 'All Day Event',
                start: '2017-02-01',
                color: '#9A80B9'
            }, {
                title: 'Long Event',
                start: '2017-02-07',
                end: '2017-02-10',
                color: '#EF4836'
            }, {
                id: 999,
                title: 'Repeating Event',
                start: '2017-02-09 16:00:00'
            }, {
                id: 999,
                title: 'Repeating Event',
                start: '2017-02-16 16:00:00'
            }, {
                title: 'Conference',
                start: '2017-02-03',
                end: '2017-02-05',
                color: '#E99844'
            }, {
                title: 'Meeting',
                start: '2017-02-12 10:30:00',
                end: '2017-02-12 12:30:00'
            }, {
                title: 'Lunch',
                start: '2017-02-12 12:00:00'
            }, {
                title: 'Meeting',
                start: '2017-02-12 14:30:00'
            }, {
                title: 'Happy Hour',
                start: '2017-02-12 17:30:00'
            }, {
                title: 'Dinner',
                start: '2017-02-12 20:00:00'
            }, {
                title: 'Birthday Party',
                start: '2017-02-13 07:00:00',
                color: '#26A65B'
            }]
        });
    }

    /* Datatable */
    if ($("#example0").length) {

        $('#example0').DataTable();
        var table = $('#example').DataTable({
            "columnDefs": [{
                "visible": false,
                "targets": 2
            }],
            "order": [
            [2, 'asc']
            ],
            "displayLength": 25,
            "drawCallback": function (settings) {
                var api = this.api();
                var rows = api.rows({
                    page: 'current'
                }).nodes();
                var last = null;

                api.column(2, {
                    page: 'current'
                }).data().each(function (group, i) {
                    if (last !== group) {
                        $(rows).eq(i).before(
                            '<tr class="group"><td colspan="5">' + group + '</td></tr>'
                            );

                        last = group;
                    }
                });
            }
        });

        // Datatable Order by the grouping
        $('#example tbody').on('click', 'tr.group', function () {
            var currentOrder = table.order()[0];
            if (currentOrder[0] === 2 && currentOrder[1] === 'asc') {
                table.order([2, 'desc']).draw();
            } else {
                table.order([2, 'asc']).draw();
            }
        });
    }

    /* Form Element */
    if ($("#date-range-picker").length) {

        // Basic Date Range Picker
        $('#date-range-picker').daterangepicker(null, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);
        });
        // Basic Single Date Picker 
        $('#date-picker').daterangepicker({
            singleDatePicker: true
        }, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);
        });
        // Date Range and Time Picker 
        $('#date-range-and-time-picker').daterangepicker({
            timePicker: true,
            timePickerIncrement: 30,
            format: 'MM/DD/YYYY h:mm A'
        }, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);
        });
    }
        // Basic Single Date Picker 
        if ($("#date-picker-product").length) {
            $('#date-picker-product').daterangepicker({
                singleDatePicker: true
            }, function (start, end, label) {
                console.log(start.toISOString(), end.toISOString(), label);
            });
        }


    // Chart Today Sales
    if ($("#todaysales").length) {
        var seriesData = [
        [],
        [],
        []
        ];
        var random = new Rickshaw.Fixtures.RandomData(20);

        for (var i = 0; i < 110; i++) {
            random.addData(seriesData);
        }

        // instantiate our graph!

        var graph = new Rickshaw.Graph({
            element: document.getElementById("todaysales"),
            renderer: 'bar',
            series: [{
                color: "#fe7070",
                data: seriesData[0],
                name: 'Bing'
            }, {
                color: "#fdacac",
                data: seriesData[1],
                name: 'Google'
            }, {
                color: "#fedada",
                data: seriesData[2],
                name: 'Yahoo'
            }]
        });

        graph.render();

        var hoverDetail = new Rickshaw.Graph.HoverDetail({
            graph: graph,
            formatter: function (series, x, y) {
                var date = '<span class="date">' + new Date(x * 1000).toUTCString() + '</span>';
                var swatch = '<span class="detail_swatch" style="background-color: ' + series.color + '"></span>';
                var content = swatch + series.name + ": " + parseInt(y, 10) + '<br>' + date;
                return content;
            }
        });

        // Today Activity
        // set up our data series with 50 random data points

        var seriesData = [
        [],
        [],
        []
        ];
        var random = new Rickshaw.Fixtures.RandomData(20);

        for (var i = 0; i < 50; i++) {
            random.addData(seriesData);
        }

    }

    /* Chart Today Activity  */
    if ($("#todayactivity").length) {
        // instantiate our graph!

        var graph = new Rickshaw.Graph({
            element: document.getElementById("todayactivity"),
            renderer: 'area',
            series: [{
                color: "#9A80B9",
                data: seriesData[0],
                name: 'London'
            }, {
                color: "#CDC0DC",
                data: seriesData[1],
                name: 'Tokyo'
            }]
        });

        graph.render();

        var hoverDetail = new Rickshaw.Graph.HoverDetail({
            graph: graph,
            formatter: function (series, x, y) {
                var date = '<span class="date">' + new Date(x * 1000).toUTCString() + '</span>';
                var swatch = '<span class="detail_swatch" style="background-color: ' + series.color + '"></span>';
                var content = swatch + series.name + ": " + parseInt(y, 10) + '<br>' + date;
                return content;
            }
        });
    }

    /* wysihtml5 editor */
    if ($(".textarea").length) {
        $('.textarea').wysihtml5();
    }

    /* Css Animation */
    $('.animation_select').click(function () {
        $('#animation_box').removeAttr('class').attr('class', '');
        var animation = $(this).attr("data-animation");
        $('#animation_box').addClass('animated');
        $('#animation_box').addClass(animation);
        return false;
    });
    
});