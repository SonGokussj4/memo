// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
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
    startDate: "1980-01-01",
    // endDate: "2021-01-01",
    todayBtn: "linked",
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

// Initialize bootstrap-tooltips
$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})


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
        // if (value) {
        //     $(this).closest(".field-wrapper").addClass("hasValue");
        // } else {
        //     $(this).closest(".field-wrapper").removeClass("hasValue");
        // }
    });

    $('.field-wrapper input').focusout(function () {
        var text_val = $(this).val();
        // if (text_val === "") {
        //     $(this).closest(".field-wrapper").removeClass("hasValue");
        // } else {
        //     $(this).closest(".field-wrapper").addClass('hasValue');
        // }
        $(this).closest(".field-wrapper").toggleClass('hasValue', text_val !== "");
    }).focusout(); //trigger the focusout event manually
};

$(function () {
    floatingLabelsInit();
})