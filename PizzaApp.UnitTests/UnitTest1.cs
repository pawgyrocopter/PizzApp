using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces.IServices;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace PizzaApp.UnitTests;

public class UnitTest1 : IClassFixture<PizzaAppSeedDataFixture>
{
    private PizzaAppSeedDataFixture _fixture;
    private AccountService _accountService;

    public UnitTest1(PizzaAppSeedDataFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Test1()
    {
        // List<User> _users = new List<User>
        // {
        //     new User() {Id = 1, UserName = "User1", Email = "user1@bv.com"},
        //     new User() {Id = 2, UserName = "User2", Email = "user2@bv.com"},
        // };
        //
        // var _userManager = MockUserManager<User>(_users).Object;

        _fixture.DataContext.Topings.Add(new Toping() {Id = 1, Name = "testToping"});
        _fixture.DataContext.SaveChanges();

        var item = _fixture.DataContext.Topings.Take(1);
        Assert.Equal(item.First().Id, 1);
    }

    public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success)
            .Callback<TUser, string>((x, y) => ls.Add(x));
        mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

        return mgr;
    }

    // public async Task<int> CreateUser(User user, string password) =>
    //     (await _userManager.CreateAsync(user, password)).Succeeded ? user.Id : -1;
}