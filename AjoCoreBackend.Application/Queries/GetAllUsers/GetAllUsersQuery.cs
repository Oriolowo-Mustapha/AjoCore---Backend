using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
    }
}
