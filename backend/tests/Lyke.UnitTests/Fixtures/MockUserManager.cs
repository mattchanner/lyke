using Lyke.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Lyke.UnitTests.Fixtures;

public static class MockUserManager
{
    public static Mock<UserManager<User>> Create()
    {
        var store = new Mock<IUserStore<User>>();
        var mgr = new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        mgr.Object.UserValidators.Add(new UserValidator<User>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<User>());

        return mgr;
    }

    public static Mock<UserManager<User>> CreateWithUser(User user)
    {
        var mock = Create();

        mock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        mock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        mock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);

        mock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        mock.Setup(x => x.Users)
            .Returns(new List<User> { user }.AsQueryable().BuildMock());

        return mock;
    }
}

public static class QueryableMockExtensions
{
    public static IQueryable<T> BuildMock<T>(this IQueryable<T> data) where T : class
    {
        return new TestAsyncEnumerable<T>(data);
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}
