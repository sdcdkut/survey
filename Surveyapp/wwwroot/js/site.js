
if (!Modernizr.inputtypes.date){
    $("input[type='date']").datepicker({
        showButtonPanel: true,
        changeMonth: true,
        changeYear: true,
        showAnim: "slideDown",
        dateFormat:"yy-mm-dd"
    });
}