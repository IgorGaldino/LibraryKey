// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    $(".alertMessage").ready(function () {
        setTimeout(function () { $(".alertMessage").hide("fast") }, 4000)
    }).click(function (value) {
        value.currentTarget.hidden = true
    })
});