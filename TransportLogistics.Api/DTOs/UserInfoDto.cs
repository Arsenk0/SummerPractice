// TransportLogistics.Api/DTOs/UserInfoDto.cs
using System;

namespace TransportLogistics.Api.DTOs
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}