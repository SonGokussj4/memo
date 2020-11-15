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


// ==========================================================================
// PARAMETERS
// ==========================================================================

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


// ==========================================================================
// INITIALIZE UPON PAGE LOAD
// ==========================================================================
$(document).ready(function () {
    initializeTooltips();
    initializeSelectpicker();
    initializeOrderAjaxModalClickEvent();
});


// ==========================================================================
// INITIALIZING FUNCTIONS
// ==========================================================================
/**
 * Initialize bootstrap-tooltips
 */
function initializeTooltips() {
    $('[data-toggle="tooltip"]').tooltip({
        trigger: 'hover'
    });
}

/**
 * Selectpicker: select2 [https://select2.org/]
 */
function initializeSelectpicker() {
    console.log("Initializing Selectpicker");
    $('.selectpicker').select2({
        // dropdownCssClass: 'form-control',
        minimumInputLength: 1,  // minimum number of characters required to start a search
        dropdownAutoWidth: true,  // make width of the dropdown to MAX of the longest item
    });
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
});

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
    return futureDate;  // RRRR-MM-DD
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
        console.log(`focusout: '${$(this).val()}'`);
        if (text_val !== "") {
            $(this).closest(".field-wrapper").addClass("hasValue");
        }
    }).focusout(); //trigger the focusout event manually
}

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
String.prototype.replaceAllTxt = function replaceAll(search, replace) { return this.split(search).join(replace); };

// Ajax modal popup the partial view
function initializeOrderAjaxModalClickEvent() {
    // all buttons with data-toggle equal to ajax-modal
    $('button[data-toggle="ajax-modal"]').unbind('click');  // first unbind click or it will duplicate the event to existing DOM objects
    $('button[data-toggle="ajax-modal"]').click(function (event) {
        let url = $(this).data('url');
        // let id = $(this).data('id');
        console.log("initializeOrderAjaxModalClickEvent: " + url);
        $.get(url).done(function (data) {
            $('#modal-placeholder').html(data);
            // $('#modal-placeholder-' + id).html(data);
            $('#modal-placeholder > .modal').modal('show');
            // $('#modal-placeholder-' + id + ' > .modal').modal('show');
            initializeSelectpicker();
        });
    });
}


$('body').on('load', 'div[data-toggle=checkboxes]', function () {
    $(this).checkboxes(); // checkboxes on matched element
});


// Modal, user select OrderCode, press Vlozit, it will insert value into a distinc val
function OrderCodeToInput(id) {
    console.log("OrderCodeToInput id: " + id);
    let SelectValue = $(`#SelectedOrderCode_${id}`).val();
    $(`#Order_OrderCodes_${id}__OrderCode`).val(SelectValue).blur();
}


// ==========================================================================
// FIXES
// ==========================================================================

// Fix Select2 not able to write text when in popup Modal
// Source: https://stackoverflow.com/questions/18487056/select2-doesnt-work-when-embedded-in-a-bootstrap-modal/19574076#19574076
$.fn.modal.Constructor.prototype._enforceFocus = function () { };
