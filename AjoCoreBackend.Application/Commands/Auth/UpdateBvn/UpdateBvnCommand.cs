using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.UpdateBvn
{
    public class UpdateBvnCommand : IRequest<bool>
    {
        public string Bvn { get; set; } = string.Empty;
    }
}
