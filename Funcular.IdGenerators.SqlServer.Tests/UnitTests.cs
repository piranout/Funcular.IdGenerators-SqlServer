using System;
using Funcular.IdGenerators.Base36;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Funcular.IdGenerators.SqlServer.Tests
{
    [TestClass]
    public class UnitTests
    {
        private string _delimiter; 
        private int[] _delimiterPositions;

        [TestInitialize]
        public void Setup()
        {
            this._delimiter = "-";
            this._delimiterPositions = new[] {15, 10, 5};
        }

        [TestMethod]
        public void Initialize()
        {
            Console.WriteLine(SqlServerIdGenerator.NewBase36Id().ToSqlString().Value);
            Console.WriteLine(SqlServerIdGenerator.NewBase36Id16FromTimestamp(new DateTime(2020,1,1)).ToSqlString().Value);
        }


        [TestMethod]
        public void Id_With_20_Chars_Parses_Correctly()
        {
            var creationTimestamp = DateTime.UtcNow;
            var id = SqlServerIdGenerator.NewBase36IdFromTimestamp(creationTimestamp).ToSqlString().Value; // _idGenerator.NewId();

            var base36IdGenerator = new Base36IdGenerator();
            var info = base36IdGenerator.Parse(id);


            Assert.IsTrue(info.TimestampComponent.Length == base36IdGenerator.NumTimestampCharacters);
            Assert.IsTrue(info.CreationTimestamp?.Date == creationTimestamp.Date && info.CreationTimestamp?.Hour == creationTimestamp.Hour);

            var length = id.Length;
            var formatted = base36IdGenerator.Format(id);
            Assert.IsTrue(formatted.Contains(_delimiter) && formatted.Length == length + (_delimiter.Length * _delimiterPositions.Length));
        }


        [TestMethod]
        public void Id_With_16_Chars_And_Creation_Timestamp_Parses_Correctly()
        {
            var creationTimestamp = DateTime.SpecifyKind(new DateTime(2020,1,1), DateTimeKind.Utc);
            
            var id = SqlServerIdGenerator.NewBase36Id16FromTimestamp(creationTimestamp).ToSqlString().Value;

            var base36IdGenerator = new Base36IdGenerator(11,2,3, delimiterPositions: new int[]{12,8,4});
            var info = base36IdGenerator.Parse(id);


            Assert.IsTrue(info.TimestampComponent.Length == base36IdGenerator.NumTimestampCharacters);
            Assert.IsTrue(info.CreationTimestamp?.Date == creationTimestamp.Date && info.CreationTimestamp.Value.Hour == creationTimestamp.Hour);

            var length = id.Length;
            var formatted = base36IdGenerator.Format(id);
            Assert.IsTrue(formatted.Contains(_delimiter) && formatted.Length == length + (_delimiter.Length * _delimiterPositions.Length));
        }


        [TestMethod]
        public void Ids_Are_Ascending()
        {
            string id1 = SqlServerIdGenerator.NewBase36Id().ToSqlString().Value;
            string id2 = SqlServerIdGenerator.NewBase36Id().ToSqlString().Value;
            Assert.IsTrue(String.Compare(id2, id1, StringComparison.OrdinalIgnoreCase) > 0);

            id1 = SqlServerIdGenerator.NewBase36Id16().ToSqlString().Value;
            id2 = SqlServerIdGenerator.NewBase36Id16().ToSqlString().Value;
            Assert.IsTrue(String.Compare(id2, id1, StringComparison.OrdinalIgnoreCase) > 0);
        }

        [TestMethod]
        public void LengthsAreCorrect()
        {
            var newBase36Id = SqlServerIdGenerator.NewBase36Id().ToSqlString().Value;
            Assert.IsTrue(newBase36Id?.Length == 20);

            newBase36Id = SqlServerIdGenerator.NewBase36Id16().ToSqlString().Value;
            Assert.IsTrue(newBase36Id?.Length == 16);
        }
    }
}
