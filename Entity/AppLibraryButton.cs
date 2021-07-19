namespace Entity
{

    /// <summary>
    /// 同步到SqlServer菜单表按钮DTO
    /// </summary>
    public record AppLibraryButton
    {
        /// <summary>
        /// 
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        public string Note { get; set; }
        public int Sort { get; set; } = 0;

    }
}
