using System;
using System.Collections.Generic;
using System.Text;

namespace Shopiva.Contracts.Auth
{
    public record RegisterRequest
    (
        string Email,
        string Password,
        
        string FirstName,
        string LastName
        );
}
