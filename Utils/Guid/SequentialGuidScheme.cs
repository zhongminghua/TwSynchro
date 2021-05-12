namespace System
{
    /// <summary>
    /// 指定 <see cref="Guid"/> 排序方案。
    /// </summary>
    public enum SequentialGuidScheme
    {
        /// <summary>
        /// 指定 <see cref="Guid"/> 按照字符串顺序排列，适用于 MySQL、PostgreSQL 数据库。
        /// </summary>
        SequentialAsString,

        /// <summary>
        /// 指定 <see cref="Guid"/> 按照二进制的顺序排列，适用于 Oracle 数据库。
        /// </summary>
        SequentialAsBinary,

        /// <summary>
        /// 指定 <see cref="Guid"/> 按照末尾部分排列，适用于 Microsoft SQL Server 数据库。
        /// </summary>
        SequentialAtEnd
    }
}
