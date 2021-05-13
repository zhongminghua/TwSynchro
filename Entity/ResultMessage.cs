namespace Entity
{
    /// <summary>
    /// 响应操作处理结果及提示信息。
    /// </summary>
    public class ResultMessage
    {
        /// <summary>
        /// 操作处理结果。
        /// </summary>
        public bool Result = default!;

        /// <summary>
        /// 消息。
        /// </summary>
        public string Message = default!;

        /// <summary>
        /// 响应操作处理结果及提示信息。
        /// </summary>
        public ResultMessage() : this(false, null) { }

        /// <summary>
        /// 响应操作处理结果及提示信息。
        /// </summary>
        /// <param name="message">提示信息。</param>
        public ResultMessage(string message) : this(false, message) { }

        /// <summary>
        /// 响应操作处理结果及提示信息。
        /// </summary>
        /// <param name="result">操作处理结果。</param>
        /// <param name="message">提示信息。</param>
        public ResultMessage(bool result, string message)
        {
            Result = result;
            Message = message;
        }
    }

    /// <summary>
    /// 响应操作处理结果及提示信息并附带相关数据DTO。
    /// </summary>
    /// <typeparam name="T">数据。</typeparam>
    public class ResultMessage<T> : ResultMessage where T : notnull
    {
        /// <summary>
        /// 响应数据。
        /// </summary>
        public T Data { get; set; } = default!;

        /// <summary>
        /// 响应操作处理结果及提示信息并附带相关数据。
        /// </summary>
        /// <param name="result">操作处理结果。</param>
        /// <param name="message">提示信息。</param>
        /// <param name="data">附带数据。</param>
        public ResultMessage(bool result, string message, T data) : base(result, message) { Data = data; }
    }
}
