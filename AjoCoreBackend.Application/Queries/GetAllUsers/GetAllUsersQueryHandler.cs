using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var traders = await _unitOfWork.Repository<Trader>().GetAllAsync();
            var cooperativeAdmins = await _unitOfWork.Repository<CooperativeAdmin>().GetAllAsync();
            
            var users = new List<UserDto>();
            
            users.AddRange(traders.Select(t => new UserDto
            {
                Id = t.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                Email = t.Email,
                PhoneNumber = t.PhoneNumber,
                Role = t.Role.ToString(),
                CreatedAt = t.CreatedAt
            }));
            
            users.AddRange(cooperativeAdmins.Select(c => new UserDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Role = c.Role.ToString(),
                CreatedAt = c.CreatedAt
            }));
            
            return users.OrderByDescending(u => u.CreatedAt).ToList();
        }
    }
}
