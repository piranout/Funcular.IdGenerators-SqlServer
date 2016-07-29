using System;
using Funcular.IdGenerators.Base36;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Funcular.IdGenerators.SqlServer.Tests
{
    [TestClass]
    public class IdGenerationTests
    {
        private Base36IdGenerator _idGenerator;
        private string _delimiter;
        private int[] _delimiterPositions;

        [TestInitialize]
        public void Setup()
        {
            this._delimiter = "-";
            this._delimiterPositions = new[] {15, 10, 5};
            this._idGenerator = new Base36IdGenerator(
                numTimestampCharacters: 11,
                numServerCharacters: 5,
                numRandomCharacters: 4,
                reservedValue: "",
                delimiter: this._delimiter,
                // give the positions in reverse order if you
                // don't want to have to account for modifying
                // the loop internally. To do the same in ascending
                // order, you would need to pass 5, 11, 17:
                delimiterPositions: this._delimiterPositions);
            // delimiterPositions: new[] {5, 11, 17});
        }

        [TestMethod]
        public void Initialize()
        {
            _idGenerator.NewId();
        }

        [TestMethod]
        public void Ids_Are_Ascending()
        {
            string id1 = this._idGenerator.NewId();
            string id2 = this._idGenerator.NewId();
            Assert.IsTrue(String.Compare(id2, id1, StringComparison.OrdinalIgnoreCase) > 0);
        }

        [TestMethod]
        public void Index_Is_Not_outside_bounds()
        {
            Assert.IsTrue(SqlServerIdGenerator.NewBase36Id()?.Length > 10);
        }
    }
}
