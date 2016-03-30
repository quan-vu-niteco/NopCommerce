namespace Nop.Core.Domain.News
{
    /// <summary>
    /// Represents the news sorting
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
        /// News creation date
        /// </summary>
        CreatedOn = 15,
    }
}