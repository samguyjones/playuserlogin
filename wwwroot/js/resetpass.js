$(function() {
    $('#new-password').submit(function(event) {
        $.ajax({
          type:        'POST',
          url:         '/User/ChangePassword',
          data:        JSON.stringify({
            password:  $('#new-password input[name="password"').val(),
            resetCode: $('#new-password input[name="resetCode"').val(),
            tokenId:   $('#new-password input[name="tokenId"').val()
          }),
          datatype:    'json',
          contentType: 'application/json; charset=utf-8'
        })
        .done(function() {
          window.location.href="/User";
        })
        .fail(function(error) {
          var error = (error.status==400) ? error.responseText : "Server Error";
          $('#new-password-status').text('Error:  ' + error);
        });
        event.preventDefault();
      });
});