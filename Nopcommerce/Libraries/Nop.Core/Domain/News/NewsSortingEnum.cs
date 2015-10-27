namespace Nop.Core.Domain.News
{
    /// <summary>
    /// Represents the product sorting
    /// </summary>
    public enum NewsSortingEnum
    {
        /// <summary>
        /// Position (display order)
        /// </summary>
        Position = 0,
        /// <summary>
        /// Name: A to Z
        /// </summary>
        NameAsc = 5,
        /// <summary>
        /// Name: Z to A
        /// </summary>
        NameDesc = 6,      
        /// <summary>
        /// Product creation date
        /// </summary>
        CreatedOn = 15,
    }
}