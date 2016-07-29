using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Funcular.IdGenerators.Base36;
using Microsoft.SqlServer.Server;

    /// <summary>
    /// A self-contained packaging of the functionality in the Base36IdGenerator
    /// from the Funcular.IdGenerators.Base36 namespace in the Funcular.IdGenerators
    /// NuGet package.
    /// </summary>
    public static class SqlServerIdGenerator
    {
        static readonly Dictionary<string,Base36IdGenerator> _generators = new Dictionary<string,Base36IdGenerator>();


        /// <summary>
        /// Returns a new Base 36 Id with 11 timestamp characters, 4 server hash characters,
        /// 5 random characters, dash-delimited as 4 groups having 5 characters each. 
        /// TODO: Find out why delimiter not working
        /// </summary>
        /// <returns></returns>
        [return: SqlFacet(IsFixedLength = false, IsNullable = false, MaxSize = 50)]
        [SqlFunction(IsDeterministic = false, DataAccess = DataAccessKind.None)]
        private static SqlChars NewDelimitedBase36Id()
        {
            return NewBase36IdSpecified(
                numTimestampCharacters: 11,
                numServerCharacters: 4,
                numRandomCharacters: 5,
                reservedValue: "",
                delimiter: "-",
                delimiterPositions: new[] { 15, 10, 5 });
        }

    /// <summary>
    /// Returns a new Base 36 Id with 11 timestamp characters, 4 server hash characters,
    /// 5 random characters, and no delimiter. 
    /// </summary>
    /// <returns></returns>
    [return: SqlFacet(IsFixedLength = false, IsNullable = false, MaxSize = 50)]
    [SqlFunction(IsDeterministic = false, DataAccess = DataAccessKind.None)]
    public static SqlChars NewBase36Id()
        {
            return NewBase36IdSpecified(
                numTimestampCharacters: 11, 
                numServerCharacters: 4, 
                numRandomCharacters: 5, 
                reservedValue: "", 
                delimiter: "-", 
                delimiterPositions: null);
        }

        /// <summary>
        /// Return a new Id with the specified number of characters
        /// for each component.
        /// </summary>
        /// <param name="numTimestampCharacters"></param>
        /// <param name="numServerCharacters"></param>
        /// <param name="numRandomCharacters"></param>
        /// <param name="reservedValue"></param>
        /// <param name="delimiter"></param>
        /// <param name="delimiterPositions"></param>
        /// <returns></returns>
        /*[return: SqlFacet(IsFixedLength = false, IsNullable = false, MaxSize = 50)]
        [SqlFunction(IsDeterministic = false, DataAccess = DataAccessKind.None)]*/
        internal static SqlChars NewBase36IdSpecified(int numTimestampCharacters, int numServerCharacters,
            int numRandomCharacters, string reservedValue, string delimiter,
            int[] delimiterPositions)
        {
            // Ensure a generator exists with the specific desired component lengths:
            var generatorKey = string.Join("-",
                numTimestampCharacters.ToString(),
                numServerCharacters.ToString(),
                numRandomCharacters.ToString(),
                reservedValue ?? "",
                delimiter ?? "",
                delimiterPositions ?? new int[0]);
            // Cache it:
            var generator = InitializeGenerator(numTimestampCharacters, numServerCharacters, 
                numRandomCharacters, reservedValue, delimiter, delimiterPositions, generatorKey);
            return new SqlChars(generator.NewId().ToCharArray());
        }

        internal static Base36IdGenerator InitializeGenerator(int numTimestampCharacters, int numServerCharacters,
            int numRandomCharacters, string reservedValue, string delimiter, int[] delimiterPositions, string generatorKey)
        {
            Base36IdGenerator generator;
            if (_generators.ContainsKey(generatorKey) == false)
                _generators.Add(generatorKey,
                    (generator =
                        new Base36IdGenerator(numTimestampCharacters, numServerCharacters, numRandomCharacters, reservedValue,
                            delimiter, delimiterPositions)));
            else
            {
                generator = _generators[generatorKey];
            }
            return generator;
        }
    }