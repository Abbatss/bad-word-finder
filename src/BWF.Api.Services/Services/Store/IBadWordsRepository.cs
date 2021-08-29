using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BWF.Api.Services.Models;

namespace BWF.Api.Services.Store
{
    public interface IBadWordsRepository
    {
        Task<Models.BadWord> AddAsync(Models.BadWord word, CancellationToken cancellationToken = default);

        Task<Models.BadWord> GetAsync(string wordId, CancellationToken cancellationToken = default);
        Task DeleteAsync(string wordId, CancellationToken cancellationToken = default);
        Task<List<BadWord>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
