using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Class
{
    class InvalidUsername : Exception
    {
        public InvalidUsername() { }
        public InvalidUsername(string message)
            : base(String.Format("Invalid Username: {0}", message)) { }
    }
    class InvalidPassword : Exception
    {
        public InvalidPassword() { }
        public InvalidPassword(string message)
            : base(String.Format("Invalid Password: {0}", message)) { }
    }
    class InvalidProxy : Exception
    {
        public InvalidProxy() { }
        public InvalidProxy(string message)
            : base(String.Format("Invalid Proxy: {0}", message)) { }
    }

    class ResponseIsNotOK : Exception
    {
        public ResponseIsNotOK() { }
        public ResponseIsNotOK(string message)
            : base(String.Format("Response is not OK: {0}", message)) { }
    }
    class CookieNotFound : Exception
    {
        public CookieNotFound() { }
        public CookieNotFound(string message)
            : base(String.Format("This cookie is not set: {0}", message)) { }
    }
    class AlreadyLoggedIn : Exception
    {
        public AlreadyLoggedIn() { }
        public AlreadyLoggedIn(string message)
            : base(String.Format("This user is already logged in: {0}", message)) { }
    }
    class LoginFailed : Exception
    {
        public LoginFailed() { }
    }
    class NotLoggedIn : Exception
    {
        public NotLoggedIn() { }
    }
}
