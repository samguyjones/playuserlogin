# User Handling Example

This page handles some aspects of authentication like a user creation
form, a login page and a forgot password page.  A few notes on its use:

  * Most of the pages are API calls taking and returning JSON.
  * The exceptions are the landing page and the forgot password form.
    On an actual product, these would be somewhere distinct from the API.
  * There are two tables:  Member, which stores the user information
    (including MD5 password) and MemberToken, which stores login credentials.
  * The reset password code is an MD5 of both the password hash and the random
    token for authentication.  It will last any length of time, but it will
    stop being valid once the password changes.
