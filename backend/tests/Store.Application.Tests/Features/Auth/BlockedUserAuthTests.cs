using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Common.Settings;
using Store.Application.Features.Auth.Commands.Login;
using Store.Application.Features.Auth.Commands.RefreshTokens;
using Store.Application.Tests.Common;
using Store.Domain.Entities;
using Store.Infrastructure.Persistence;
using Xunit;

namespace Store.Application.Tests.Features.Auth;

public class BlockedUserAuthTests
{
    private static readonly IOptions<JwtSettings> Settings = Options.Create(new JwtSettings { RefreshTokenDays = 14 });

    private static Mock<IJwtTokenService> TokenService()
    {
        var mock = new Mock<IJwtTokenService>();
        mock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns(("access", DateTime.UtcNow.AddMinutes(20)));
        mock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh");
        mock.Setup(x => x.HashToken(It.IsAny<string>())).Returns<string>(s => "hash-" + s);
        return mock;
    }

    private static Mock<IPasswordHasher> Hasher(bool passwordMatches)
    {
        var mock = new Mock<IPasswordHasher>();
        mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(passwordMatches);
        return mock;
    }

    private static async Task<(StoreDbContext context, User user)> ArrangeUserAsync(bool blocked)
    {
        var context = TestDbContextFactory.Create();
        var user = new User { Email = "u@example.com", FullName = "U", PasswordHash = "h", IsBlocked = blocked };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return (context, user);
    }

    private static LoginCommandHandler LoginHandler(StoreDbContext context, bool passwordMatches)
        => new(context, Hasher(passwordMatches).Object, TokenService().Object, Settings, MapperFactory.Create());

    [Fact]
    public async Task Login_BlockedUserWithCorrectPassword_ShouldBeForbidden_AndIssueNoTokens()
    {
        var (context, _) = await ArrangeUserAsync(blocked: true);
        using var _ = context;

        var act = () => LoginHandler(context, passwordMatches: true)
            .Handle(new LoginCommand { Email = "u@example.com", Password = "right" }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        context.RefreshTokens.Should().BeEmpty("a blocked user must not receive a session");
    }

    [Fact]
    public async Task Login_BlockedUserWithWrongPassword_ShouldLookLikeAnyBadLogin()
    {
        // The blocked state must not be revealed to someone who doesn't know the password.
        var (context, _) = await ArrangeUserAsync(blocked: true);
        using var _ = context;

        var act = () => LoginHandler(context, passwordMatches: false)
            .Handle(new LoginCommand { Email = "u@example.com", Password = "wrong" }, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Login_ActiveUser_ShouldSucceed()
    {
        var (context, _) = await ArrangeUserAsync(blocked: false);
        using var _ = context;

        var result = await LoginHandler(context, passwordMatches: true)
            .Handle(new LoginCommand { Email = "u@example.com", Password = "right" }, CancellationToken.None);

        result.AccessToken.Should().Be("access");
    }

    private static RefreshTokenCommandHandler RefreshHandler(StoreDbContext context)
        => new(context, TokenService().Object, Settings, MapperFactory.Create());

    [Theory]
    [InlineData(false)] // still-active token
    [InlineData(true)]  // token revoked by the blocking itself — must read as "blocked", not as token theft
    public async Task Refresh_BlockedUser_ShouldBeForbidden(bool tokenAlreadyRevoked)
    {
        var (context, user) = await ArrangeUserAsync(blocked: true);
        using var _ = context;
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, TokenHash = "hash-abc", ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = tokenAlreadyRevoked ? DateTime.UtcNow : null
        });
        await context.SaveChangesAsync();

        var act = () => RefreshHandler(context).Handle(new RefreshTokenCommand { RefreshToken = "abc" }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Refresh_ActiveUser_ShouldRotateToken()
    {
        var (context, user) = await ArrangeUserAsync(blocked: false);
        using var _ = context;
        context.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = "hash-abc", ExpiresAt = DateTime.UtcNow.AddDays(7) });
        await context.SaveChangesAsync();

        var result = await RefreshHandler(context).Handle(new RefreshTokenCommand { RefreshToken = "abc" }, CancellationToken.None);

        result.RefreshToken.Should().Be("new-refresh");
    }
}
