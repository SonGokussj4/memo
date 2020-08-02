// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Bootstrap-Select options
$(function () {
    $('.selectpicker').selectpicker({
        liveSearch: true,
        liveSearchNormalize: true,
    });
});

// Initialize bootstrap-tooltips
$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})