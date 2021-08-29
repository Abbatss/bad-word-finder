using System.Threading.Tasks;

namespace DynamoDB.Common
{
    /// <summary>
    /// initialize DB.
    /// </summary>
    public interface IEntityDBInitializer
    {
        /// <summary>
        /// Initialize database and indexes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Init();
    }
}
