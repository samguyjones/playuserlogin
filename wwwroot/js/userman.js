$(function() {
  $('#create').submit(function(event) {
    $.ajax({
      type:        'POST',
      url:         '/User/Create',
      data:        JSON.stringify({
        firstName: $('#create input[name="firstName"').val(),
        lastName: $('#create input[name="lastName').val(),
        email: $('#create input[name="email"').val(),
        password: $('#create input[name="password').val()
      }),
      datatype:    'json',
      contentType: 'application/json; charset=utf-8'
    })
    .done(function() {
      $('#create-status').text('User Created');
    })
    .fail(function(error) {
      var error = (error.status==400) ? error.responseText : "Server Error";
      $('#create-status').text('Error:  ' + error);
    });
    event.preventDefault();
  });

  $('#forgot').submit(function(event) {
    $.ajax({
      type: 'POST',
      url: '/User/ForgotPassword',
      data: JSON.stringify({
        email: $('#forgot input[name="email"').val()
      }),
      dataType: 'json',
      contentType: 'application/json; charset=utf-8'
    })
    .done(function() {
      $('#forgot-status').text('Email Sent');
    })
    .fail(function(error) {
      var error = (error.status==400) ? error.responseText : "Server Error";
      $('#forgot-status').text('Error:  ' + error);
    });
    event.preventDefault();
  });

  $('#login').submit(function(event) {
    $.ajax({
      type: 'POST',
      url: '/User/Login',
      data: JSON.stringify({
        email: $('#login input[name="email"').val(),
        password: $('#login input[name="password"').val()
      }),
      dataType: 'json',
      contentType: 'application/json; charset=utf-8'
    })
    .done(function(response) {
      $.ajax({
        type: 'GET',
        url: '/User/ProtectedPage',
        headers: {
          'Authorization': 'Bearer ' + response.tokenHash
        }
      })
      .done(function(response) {
        $('html').html(response);
      });
    })
    .fail(function(error) {
      var error = (error.status==400) ? error.responseText : "Server Error";
      $('#login-status').text('Error:  ' + error);
    });
    event.preventDefault();
  })
});