using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_bite_server.Models;

namespace web_bite_server.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}