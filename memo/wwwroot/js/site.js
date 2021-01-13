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


// ==========================================================================
// PARAMETERS
// ==========================================================================

// Parameters for DatePicker calendar
var datepickerParameters = new Array({
    format: "dd.mm.yyyy",
    todayBtn: "linked",
    startDate: "1988-09-02",
    language: "cs",
    daysOfWeekHighlighted: "0,6",
    calendarWeeks: true,
    autoclose: true,
    todayHighlight: true
});


// ==========================================================================
// FIXES
// ==========================================================================

// Fix Select2 not able to write text when in popup Modal
// Source: https://stackoverflow.com/questions/18487056/select2-doesnt-work-when-embedded-in-a-bootstrap-modal/19574076#19574076
$.fn.modal.Constructor.prototype._enforceFocus = function () { };

// Add 'replaceAll()' function to JavaScript Strings
String.prototype.replaceAllTxt = function replaceAll(search, replace) { return this.split(search).join(replace); };


// ==========================================================================
// INITIALIZE UPON PAGE LOAD
// ==========================================================================
$(document).ready(function () {
    initializeTooltips();
    initializeSelectpicker();
    initializeOrderAjaxModalClickEvent();
});

function getBaseUrl() {
    if (window.location.host == "ar-sc") {
        return window.location.origin + "/" + window.location.pathname.split('/')[1];
    }
    return window.location.origin;
}

/**
 * After each keypress, POST to method that checks for availability of entered item
 * If False is returned, show warning text and color border to red, hiden and green otherwise.
 * @param {element} parent $(this) element every time
 * @param {string} controllerName Controller Name
 * @param {string} [ignoreName] String which has to be ignored, if it's the same as entered name, return False
 */
function checkItemNameExists(parent, controllerName, ignoreName="") {
    var baseUrl = getBaseUrl();
    $warningElement = $('#NumberAlreadyExistsWarning');
    if (!$($warningElement).length) {
        $html = '<p id="NumberAlreadyExistsWarning" class="hide text-danger text-right pt-1 pb-0 mb-0" >Číslo je již použito!</p>';
        $spanId = parent.attr('id').replaceAllTxt("_", ".");
        $span = $(`span[data-valmsg-for="${$spanId}"]`);
        $($html).insertBefore($span);
    }
    var enteredText = parent.val();
    var $this = parent;
    $.ajax({
        //url: '@Url.Action("itemNameExists", "Offers")',
        url: `${baseUrl}/${controllerName}/itemNameExists`,
        type: 'POST',
        dataType: "json",
        data: { itemName: enteredText, ignoreName: ignoreName },
        success: function (response) {
            if(response.exists == true) {
                $this.css("border-bottom", "4px solid red");
                $('#NumberAlreadyExistsWarning').show();
            } else {
                $this.css("border-bottom", "4px solid limegreen");
                $('#NumberAlreadyExistsWarning').hide();
            }
        },
        error: function(response) {
            console.log("Error...", response);
        }
    });
}

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
    $('.selectpicker').select2({
        // dropdownCssClass: 'form-control',
        minimumInputLength: 0,  // minimum number of characters required to start a search
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
 * @summary Return date (can do days additon) in format dd.MM.yyyy
 * @description
 * - Example:
 *  - var futureDate = getPrettyDate(new Date("24.12.2020"), 1)
 *  - "25.12.2020"
 * @param  {[Date]} date [date e.g. new Date('xxxx')]
 * @param  {[integer]} daysToAdd [default: 0; days to add to the <date>]
 * @return {[string]}     [String in format dd.MM.yyyy]
 */
function getPrettyDate(date, daysToAdd = 0) {
    date.setDate(date.getUTCDate() + daysToAdd);
    var futureDate =
    ('0' + (date.getDate())).slice(-2)
    + '.'
    + ('0' + (date.getMonth() + 1)).slice(-2)
    + '.'
    + date.getFullYear();
    return futureDate;  // dd.MM.yyyy
}

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
function getPrettyDateUS(date, daysToAdd = 0) {
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

// ==========================================================================
// NOTIFICATIONS - ALERTS
// ==========================================================================
$("#success-alert").fadeTo(4000, 500).slideUp(500, function(){
    $("#success-alert").slideUp(500);
});
$("#info-alert").fadeTo(4000, 500).slideUp(500, function(){
    $("#info-alert").slideUp(500);
});
$("#error-alert").fadeTo(60000, 500).slideUp(500, function(){
    $("#error-alert").slideUp(500);
});


// Ajax modal popup the partial view
function initializeOrderAjaxModalClickEvent() {
    // all buttons with data-toggle equal to ajax-modal
    $('button[data-toggle="ajax-modal"]').unbind('click');  // first unbind click or it will duplicate the event to existing DOM objects
    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data("url") + "/" + $(this).val();
        console.log("initializeOrderAjaxModalClickEvent: " + url);
        $.get(url).done(function (data) {
            $('#modal-placeholder').html(data);
            // $('#modal-placeholder-' + id).html(data);
            $('#modal-placeholder > .modal').modal('show');
            // $('#modal-placeholder-' + id + ' > .modal').modal('show');
            // initializeSelectpicker();
        });
    });
}


$('body').on('load', 'div[data-toggle=checkboxes]', function () {
    $(this).checkboxes(); // checkboxes on matched element
});


// Modal, user select OrderCode, press Vlozit, it will insert value into a distinc val
function OrderCodeToInput(id) {
    // console.log("OrderCodeToInput id: " + id);
    let SelectValue = $(`#SelectedOrderCode_${id}`).val();
    $(`#Order_OrderCodes_${id}__OrderCode`).val(SelectValue).blur();
}





// ==========================================================================
// OTHER
// ==========================================================================
function isNumeric(str) {
    if (typeof str != "string") return false;  // we only process strings!
    return !isNaN(str) && // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
        !isNaN(parseFloat(str));  // ...and ensure strings of whitespace fail
}


// Number.prototype.formatMoney = function(c, d, t){
//     var n = this,
//     c = isNaN(c = Math.abs(c)) ? 2 : c,
//     d = d == undefined ? "." : d,
//     t = t == undefined ? "," : t,
//     s = n < 0 ? "-" : "",
//     i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "",
//     j = (j = i.length) > 3 ? j % 3 : 0;
//     return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
// };

/*
decimal_sep: character used as deciaml separtor, it defaults to '.' when omitted
thousands_sep: char used as thousands separator, it defaults to ',' when omitted
*/
Number.prototype.formatMoney = function(decimals, decimal_sep, thousands_sep)
{
   var n = this,
   c = isNaN(decimals) ? 2 : Math.abs(decimals), //if decimal is zero we must take it, it means user does not want to show any decimal
   d = decimal_sep || '.', //if no decimal separator is passed we use the dot as default decimal separator (we MUST use a decimal separator)

   /*
   according to [https://stackoverflow.com/questions/411352/how-best-to-determine-if-an-argument-is-not-sent-to-the-javascript-function]
   the fastest way to check for not defined parameter is to use typeof value === 'undefined'
   rather than doing value === undefined.
   */
   t = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep, //if you don't want to use a thousands separator you can pass empty string as thousands_sep value

   sign = (n < 0) ? '-' : '',

   //extracting the absolute value of the integer part of the number and converting to string
   i = parseInt(n = Math.abs(n).toFixed(c)) + '',

   j = ((j = i.length) > 3) ? j % 3 : 0;
   return sign + (j ? i.substr(0, j) + t : '') + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : '');
};



// ==========================================================================
// BOOTSTRAP TABLE FILTER AND SORTING FORMATTERS
// ==========================================================================
function sumHoursFormatter(data) {
    var field = parseInt(this.field);
    var result = data.map(function (row) {
        var hours = parseInt(row[field]
            .replace("hod", '')
            .replace(" ", '')
        ) || 0;
        return + hours;
    }).reduce(function (sum, i) {
        return sum + i;
    }, 0);
    return result.toLocaleString('cs-CZ') + ' hod';
}

function priceFormatterCz(data) {
    var field = parseInt(this.field);
    //var field = parseInt(this.field.replace(",", "").replace("&nbsp;", "").replace(" ", "").replace("Kč", ""));
    //var field = parseInt(this.field).val().replace(/[^0-9]/g, '')
    var vysledek = data.map(function (row) {
        var cost = parseInt(row[field]
            //.replace(/, (Kč)(&nbsp);/g, ''));
            .replaceAllTxt(",", "")
            .replaceAllTxt("&nbsp;", "")
            .replaceAllTxt(" ", "")
            .replaceAllTxt("Kč", "")
            ) || 0;
        //console.log(`CostCz: ${cost}`);
        return + cost;
    }).reduce(function (sum, i) {
        return sum + i;
    }, 0);
    return vysledek.toLocaleString('cs-CZ') + ' Kč';
}

function priceSorter(a, b) {
    let aa = a.replaceAllTxt(' Kč', '').replaceAllTxt(' €', '').replaceAllTxt('$', '')
        .replaceAllTxt('&nbsp;', '').replaceAllTxt('.', '').replaceAllTxt(',', '');
    let bb = b.replaceAllTxt(' Kč', '').replaceAllTxt(' €', '').replaceAllTxt('$', '')
        .replaceAllTxt('&nbsp;', '').replaceAllTxt('.', '').replaceAllTxt(',', '');
    return aa - bb;
}

function priceFormatter(data) {
    var field = parseInt(this.field
        .replace(",", "").replace(".", "").replace("&nbsp;", "").replace(" ", "")
        .replace("Kč", "").replace("$", "").replace("€", "")
    );
    var vysledek = data.map(function (row) {
        var cost = parseInt(row[field]
            //.replace(/, (Kč)(&nbsp);/g, ''));
            .replaceAllTxt(",", "")
            .replaceAllTxt(".", "")
            .replaceAllTxt("&nbsp;", "")
            .replaceAllTxt(" ", "")
            .replaceAllTxt("Kč", "")
            .replaceAllTxt("$", "")
            .replaceAllTxt("€", "")
            ) || 0;
        return + cost;
    }).reduce(function (sum, i) {
        return sum + i
    }, 0);
    return (vysledek).formatMoney(0, '.', ' ');
}


// NOTE: Stara vec, predtim jsem klikal na Order/Create z nabidky, ramcovky nebo bez nabidky
// a menil jsem dynamicky zbytek contentu, nyni je to delane pres individualni cshtml stranky
//$("#opt1").click(function () {
//    $(".new-order-from-offer").show();
//    $(".new-order-from-contract").hide();
//    $(".new-order-from-other").hide();
//    $(this).parent().addClass("selected");
//    $("#opt2").parent().removeClass("selected");
//    $("#opt3").parent().removeClass("selected");
//    $("#orderForm").css("visibility", "hidden");
//});
//$("#opt2").click(function () {
//    $(".new-order-from-offer").hide();
//    $(".new-order-from-contract").show();
//    $(".new-order-from-other").hide();
//    $(this).parent().addClass("selected");
//    $("#opt1").parent().removeClass("selected");
//    $("#opt3").parent().removeClass("selected");
//    $("#orderForm").css("visibility", "hidden");
//});
//$("#opt3").click(function () {
//    $(".new-order-from-offer").hide();
//    $(".new-order-from-contract").hide();
//    $(".new-order-from-other").show();
//    $(this).parent().addClass("selected");
//    $("#opt1").parent().removeClass("selected");
//    $("#opt2").parent().removeClass("selected");
//    $("#orderForm").css("visibility", "visible");
//});

