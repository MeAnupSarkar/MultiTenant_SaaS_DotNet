// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$(document).ready(function () {
 
    if (parseInt($("#isLoggedOut").val())==1) {

       // window.location = "/Identity/Account/Logout?returnUrl=%2F%3Fpage%3D%252F";

        $("#logout").click();
    }

})