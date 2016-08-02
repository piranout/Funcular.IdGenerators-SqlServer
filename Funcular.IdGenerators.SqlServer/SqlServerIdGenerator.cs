using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using Funcular.IdGenerators.Base36;
using Microsoft.SqlServer.Server;

/// <summary>
/// A self-contained packaging of the functionality in the Base36IdGenerator
/// from the Funcular.IdGenerators.Base36 namespace in the Funcular.IdGenerators
/// NuGet package.
/// </summary>
[CompilerGenerated]
public static class SqlServerIdGenerator
    {
        [CompilerGenerated]
        static readonly Dictionary<string,Base36IdGenerator> _generators = new Dictionary<string,Base36IdGenerator>();


        /// <summary>
        /// Returns a new Base 36 Id with 11 timestamp characters, 4 server hash characters,
        /// 5 random characters, and no delimiter. 
        /// </summary>
        /// <returns></returns>
        [return: SqlFacet(IsFixedLength = false, IsNullable = false, MaxSize = 50)]
        [SqlFunction(IsDeterministic = false, DataAccess = DataAccessKind.None)]
        public static SqlChars NewBase36Id()
        {
            // Ensure a generator exists with the specific desired component lengths:
            var generatorKey = string.Join("-",
                11.ToString(),
                4.ToString(),
                5.ToString(),
                "" ?? "",
                "-" ?? "",
                null);
            // Cache it:
            var generator = InitializeGenerator(11, 4, 5, "", "-", null, generatorKey);
            return new SqlChars(generator.NewId().ToCharArray());
        }


        [return: SqlFacet(IsFixedLength = false, IsNullable = false, MaxSize = 50)]
        [SqlFunction(IsDeterministic = false, DataAccess = DataAccessKind.None)]
        public static SqlChars CustomBase36Id(int numTimestampCharacters = 11, int numServerCharacters = 4,
            int numRandomCharacters = 5, string reservedValue = "", string delimiter = "-",
            int numDelimiters = 3)
        {
            var generatorKey = string.Join("-",
                numTimestampCharacters.ToString(),
                numServerCharacters.ToString(),
                numRandomCharacters.ToString(),
                reservedValue ?? "",
                delimiter ?? "",
                numDelimiters.ToString());

            var rawIdLength = numTimestampCharacters + numServerCharacters + numRandomCharacters + reservedValue?.Length ?? 0;
            var segmentLength = rawIdLength/(numDelimiters + 1);

            List<int> positions = new List<int>();
            int x = rawIdLength;
            while (x > 0 && positions.Count < numDelimiters)
            {
                positions.Add(x = (x - segmentLength));
            }
            var generator = InitializeGenerator(numTimestampCharacters, numServerCharacters,
                numRandomCharacters, reservedValue, delimiter, positions.ToArray(), generatorKey);
            return new SqlChars(generator.NewId(delimiter?.Length > 0));
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