﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Bootstrap-Select options
// $(function () {
//     $('.selectpicker').selectpicker({
//         liveSearch: true,
//         liveSearchNormalize: true,
//         reload: true,
//     });
// });


// PARAMETERS
//---------------------------------------------------------------------------

// Parameters for DatePicker calendar
var datepickerParameters = new Array({
    format: "yyyy-mm-dd",
    // format: "dd.mm.yyyy",
    todayBtn: "linked",
    startDate: "1980-01-01",
    language: "cs",
    daysOfWeekHighlighted: "0,6",
    calendarWeeks: true,
    autoclose: true,
    todayHighlight: true
});

// OTHER
//---------------------------------------------------------------------------
$(document).ready(function () {
    $('.selectpicker').select2({
        // dropdownCssClass: 'form-control',
    });
});

// ==========================================================================
// INITIALIZE UPON PAGE LOAD
// ==========================================================================
// Initialize bootstrap-tooltips
$(function () {
    initializeTooltips();
})

function initializeTooltips() {
    $('[data-toggle="tooltip"]').tooltip()
}

// Filter button on top of the bootstrap datatable to switch filter textboxes
//---------------------------------------------------------------------------
$(function() {
    $('#table').bootstrapTable();
    const $table = $('#table');

    FilterButton = $('#BtnMujFilter');
    FilterButton.click(function() {
        $table.bootstrapTable('destroy');
        $table.data("filter-control", !$table.data("filter-control"));
        $table.bootstrapTable();
    });
})

/**
 * @summary Return date (can do days additon) in format RRRR-MM-DD
 * @description
 * - Example:
 *  - var futureDate = getPrettyDate(new Date("2020-08-01"), 1)
 *  - "2020-08-02"
 * @param  {[Date]} date [date e.g. new Date('xxxx')]
 * @param  {[integer]} daysToAdd [default: 0; days to add to the <date>]
 * @return {[string]}     [String in format RRRR-MM-DD]
 */
function getPrettyDate(date, daysToAdd = 0) {
    date.setDate(date.getUTCDate() + daysToAdd);
    var futureDate = date.getFullYear()
        + '-'
        + ('0' + (date.getMonth() + 1)).slice(-2)
        + '-'
        + ('0' + (date.getDate())).slice(-2);
    return futureDate  // RRRR-MM-DD
}

//--------------------
// FLOATING TOOLTIPS
//--------------------
function floatingLabelsInit () {

    $(".field-wrapper .field-placeholder").on("click", function () {
        $(this).closest(".field-wrapper").find("input").focus();
    });

    $(".field-wrapper input").on("keyup", function () {
        var value = $.trim($(this).val());
        $(this).closest(".field-wrapper").toggleClass('hasValue', value);
        // if (value !== "") {
        //     $(this).closest(".field-wrapper").addClass("hasValue");
        // }
        // else {
        //     $(this).closest(".field-wrapper").removeClass("hasValue");
        // }
    });

    $('.field-wrapper input').focusout(function () {
        var text_val = $(this).val();
        $(this).closest(".field-wrapper").toggleClass('hasValue', text_val !== "");
        // if (text_val === "") {
        //     $(this).closest(".field-wrapper").removeClass("hasValue");
        // }
        // else {
        //     $(this).closest(".field-wrapper").addClass('hasValue');
        // }
        console.log(`focusout: '${$(this).val()}'`)
        if (text_val !== "") {
            $(this).closest(".field-wrapper").addClass("hasValue");
        }
    }).focusout(); //trigger the focusout event manually
};

//$(function () {
//    floatingLabelsInit();
//})

$("#success-alert").fadeTo(4000, 500).slideUp(500, function(){
    $("#success-alert").slideUp(500);
});
$("#info-alert").fadeTo(4000, 500).slideUp(500, function(){
    $("#info-alert").slideUp(500);
});
$("#error-alert").fadeTo(60000, 500).slideUp(500, function(){
    $("#error-alert").slideUp(500);
});

// Add 'replaceAll()' function to JavaScript Strings
String.prototype.replaceAllTxt = function replaceAll(search, replace) { return this.split(search).join(replace); }

// Ajax modal popup the partial view
function initializeOrderAjaxModalClickEvent() {
    // all buttons with data-toggle equal to ajax-modal
    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        console.log("initializeOrderAjaxModalClickEvent: " + url);
        $.get(url).done(function (data) {
            $('#modal-placeholder').html(data);
            $('#modal-placeholder > .modal').modal('show');
        });
    });
}

// Only after the document loads
$(function () {
    initializeOrderAjaxModalClickEvent();
});

$('body').on('load', 'div[data-toggle=checkboxes]', function () {
    $(this).checkboxes(); // checkboxes on matched element
});


// Modal, user select OrderCode, press Vlozit, it will insert value into a distinc val
function OrderCodeToInput(id) {
    console.log("OrderCodeToInput id: " + id);
    let SelectValue = $(`#SelectedOrderCode_${id}`).val();
    $(`#Order_OrderCodes_${id}__OrderCode`).val(SelectValue).blur();
}


// $('button[data-toggle="ajax-modal"]').click(function (event) {
//     var url = $(this).data('url');
//     console.log(url);
//     $.get(url).done(function (data) {
//         $('#modal-placeholder').html(data);
//         $('#modal-placeholder > .modal').modal('show');
//     });
// });
