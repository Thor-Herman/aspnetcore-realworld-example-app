using Application.Extensions;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Profiles.Queries;

public class ProfileDTO
{

    public string Username { get; set; } = default!;

    public string Bio { get; set; } = null!;

    public string Image { get; set; } = null!;

    public bool Following { get; set; }
}

public record ProfileResponse(ProfileDTO Profile);

public record ProfileGetQuery(string Username) : IRequest<ProfileResponse>;

public class ProfileGetHandler : IRequestHandler<ProfileGetQuery, ProfileResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public ProfileGetHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ProfileResponse> Handle(ProfileGetQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Followers)
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        return new ProfileResponse(_mapper.Map<ProfileDTO>(user, opts => opts.AfterMap((src, dest) =>
        {
            dest.Following = _currentUser.IsAuthenticated && _currentUser.User!.IsFollowing(user);
        })));
    }
}