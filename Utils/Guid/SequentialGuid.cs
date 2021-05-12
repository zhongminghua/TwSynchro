using System.Security.Cryptography;

namespace System
{
    /// <summary>
    /// 有序 <see cref="Guid"/> 生成器。
    /// <para>参见1：https://www.cnblogs.com/CameronWu/p/guids-as-fast-primary-keys-under-multiple-database.html</para>
    /// <para>参见2：https://www.codeproject.com/articles/388157/guids-as-fast-primary-keys-under-multiple-database</para>
    /// </summary>
    public static class SequentialGuid
    {
        /// <summary>
        /// 使用指定排序方案生成一个有序的 <see cref="Guid"/>。
        /// </summary>
        /// <returns></returns>
        public static Guid NewGuid(SequentialGuidScheme scheme)
        {
            var randomBytes = new byte[10];
            new RNGCryptoServiceProvider().GetBytes(randomBytes);

            var timestamp = DateTime.UtcNow.Ticks / 10000L;
            var timestampBytes = BitConverter.GetBytes(timestamp);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(timestampBytes);

            byte[] guidBytes = new byte[16];

            switch (scheme)
            {
                case SequentialGuidScheme.SequentialAsString:
                case SequentialGuidScheme.SequentialAsBinary:
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                    // 在 little-endian 系统上如果需要格式化为字符串则需要颠倒数据 a1 和数据 a2 块的顺序。
                    if (scheme == SequentialGuidScheme.SequentialAsString && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);
                        Array.Reverse(guidBytes, 4, 2);
                    }
                    break;

                case SequentialGuidScheme.SequentialAtEnd:
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scheme), scheme, null);
            }

            return new Guid(guidBytes);
        }

        /// <summary>
        /// 生成适用于 MySQL 数据库的有序 <see cref="Guid"/>。
        /// </summary>
        /// <returns></returns>
        public static Guid NewMySqlGuid()
        {
            return NewGuid(SequentialGuidScheme.SequentialAsString);
        }

        /// <summary>
        /// 生成适用于 PostgreSQL 数据库的有序 <see cref="Guid"/>。
        /// </summary>
        /// <returns></returns>
        public static Guid NewPostgreSqlGuid()
        {
            return NewGuid(SequentialGuidScheme.SequentialAsString);
        }

        /// <summary>
        /// 生成适用于 Oracle 数据库的有序 <see cref="Guid"/>。
        /// </summary>
        /// <returns></returns>
        public static Guid NewOracleGuid()
        {
            return NewGuid(SequentialGuidScheme.SequentialAsBinary);
        }

        /// <summary>
        /// 生成适用于 Microsoft SQL Server 数据库的有序 <see cref="Guid"/>。
        /// </summary>
        /// <returns></returns>
        public static Guid NewSqlServerGuid()
        {
            return NewGuid(SequentialGuidScheme.SequentialAtEnd);
        }
    }
}
