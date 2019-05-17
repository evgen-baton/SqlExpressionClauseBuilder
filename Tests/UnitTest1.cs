using SqlExpressionClauseBuilder;
using System;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var sql = ClauseBuilder.From<Users>()
                .InnerJoin<Users, UsersMetadata, int>(u => u.Id, um => um.UserId)
                .Select<Users>(u => new object[] { u.Email, })
                .Select<UsersMetadata>(um => new object[] { um.UserId })
                .Compile();
        }
    }

    public class Users
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }

    public class UsersMetadata
    {
        public int UserId { get; set; }
    }
}
