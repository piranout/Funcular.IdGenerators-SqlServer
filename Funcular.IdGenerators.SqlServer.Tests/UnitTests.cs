using System;
using Funcular.IdGenerators.Base36;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Funcular.IdGenerators.SqlServer.Tests
{
    [TestClass]
    public class UnitTests
    {
        private Base36IdGenerator _idGenerator;
        private int[] _delimiterPositions;

        [TestInitialize]
        public void Setup()
        {
            this._delimiterPositions = new[] {15, 10, 5};
            this._idGenerator = new Base36IdGenerator(
                numTimestampCharacters: 11,
                numServerCharacters: 4,
                numRandomCharacters: 5,
                reservedValue: null,
                delimiter: "-",
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
            var newBase36Id = SqlServerIdGenerator.NewBase36Id();
            Assert.IsTrue(newBase36Id?.Length == 20);
        }
    }
}
