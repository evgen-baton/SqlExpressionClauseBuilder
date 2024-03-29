using SqlExpressionClauseBuilder;
using System;
using Xunit;

namespace Tests
{
    public class SqlGenerationTests
    {
        [Fact]
        public void Test1()
        {
            var sql = ClauseBuilder
                .From<Users>()
                .InnerJoin<Users, UsersMetadata, int>(u => u.Id, um => um.UserId)
                .Select<Users>(u => new object[] { u.Email, })
                .Select<UsersMetadata>(um => new object[] { um.UserId })
                .Build();

            const string expected = "SELECT Users.Email, UsersMetadata.UserId\r\n" +
                                    "FROM Users\r\n" +
                                    "INNER JOIN UsersMetadata ON Users.Id = UsersMetadata.UserId\r\n";

            Assert.Equal(expected, sql);
        }

        [Fact]
        public void Test2()
        {
            var sql = ClauseBuilder
                .From<Users>()
                .Select<Users>(u => u.Id, u => u.Email)
                .Build();

            const string expected = "SELECT Users.Id, Users.Email\r\n" +
                                    "FROM Users\r\n";

            Assert.Equal(expected, sql);
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
