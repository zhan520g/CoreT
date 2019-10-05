using System;
using System.Collections.Generic;
using System.Text;

namespace CoreT.Entity.ViewModels
{
    public class JwtAuthorizationDto
    {
        public object UserId { get;  set; }
        public string Token { get;  set; }
        public long Auths { get;  set; }
        public long Expires { get;  set; }
        public bool Success { get;  set; }
    }
}
