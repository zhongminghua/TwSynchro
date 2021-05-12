using System;
using System.Collections.Generic;

namespace DapperFactory
{
    /// <summary>
    /// 分页API请求响应结果实体接口定义。
    /// </summary>
    public interface IResultPager
    {
        /// <summary>
        /// 页长。
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// 页码。
        /// </summary>
        int PageIndex { get; set; }

        /// <summary>
        /// 总页数。
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// 总记录数。
        /// </summary>
        long TotalCount { get; set; }
    }

    /// <summary>
    /// 分页API请求响应结果实体接口定义。
    /// </summary>
    public interface IResultPager<T> : IResultPager where T : notnull
    {
        /// <summary>
        /// 响应数据。
        /// </summary>
        IEnumerable<T> Data { get; set; }
    }

    public  class ResultPager : IResultPager
    {

        /// <summary>
        /// 页长，默认为10。
        /// </summary>

        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 页码，默认为1。
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 总页数。
        /// </summary>
        public int PageCount => (int)Math.Ceiling((decimal)TotalCount / PageSize);

        /// <summary>
        /// 总记录数。
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 是否存在上一页。
        /// </summary>
        public bool HasPrevious => PageIndex > 1;

        /// <summary>
        /// 是否存在下一页。
        /// </summary>
        public bool HasNext => PageCount > PageIndex;
    }

    /// <summary>
    /// 带数据的分页API请求响应结果。
    /// </summary>
    /// <typeparam name="T">响应数据实体类型。</typeparam>
    public class ResultPager<T> : ResultPager, IResultPager<T> where T : notnull
    {
        /// <summary>
        /// 响应数据。
        /// </summary>
        public IEnumerable<T> Data { get; set; } = default!;
    }
}
