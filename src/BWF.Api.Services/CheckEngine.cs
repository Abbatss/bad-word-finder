using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BWF.Api.Services.Models;
using BWF.Api.Services.Store;

namespace BWF.Api.Services
{
    public interface ICheckEngine
    {
        Task<CheckResult> Run(Stream stream, CancellationToken cancellationToken = default);
    }

    public class CheckEngine : ICheckEngine
    {
        private readonly IBadWordsRepository badWordsRepository;

        public CheckEngine(IBadWordsRepository badWordsRepository)
        {
            this.badWordsRepository = badWordsRepository ?? throw new ArgumentNullException(nameof(badWordsRepository));
        }
        // TODO: Consider loading 1 time load and cache + invalidate cache on add/delete bad word.
        // Only requires if we have a stable load and deployment not in lambdas.
        public async Task<CheckResult> Run(Stream stream, CancellationToken cancellationToken = default)
        {
            var watch = new System.Diagnostics.Stopwatch();

            var res = new CheckResult();
            watch.Start();
            var bwMap = await GetBadWordsMap(cancellationToken).ConfigureAwait(false);
            res.DBReadTime = watch.Elapsed;

            using (var sr = new StreamReader(stream))
            {
                var s = string.Empty;
                int i = 0;
                while ((i = sr.Read()) != -1)
                {
                    var c = Convert.ToChar(i);
                    if (char.IsLetterOrDigit(c))
                    {
                        s += c;
                        continue;
                    }

                    if (s.Trim() != string.Empty)
                    {
                        if(IsBadWord(bwMap, s))
                        {
                            res.AddBadWord(s);
                        }

                        s = string.Empty;
                    }
                }
            }
            System.Threading.Thread.Sleep(100);
            res.ProcessingTime = watch.Elapsed;
            return res;
        }

        private static bool IsBadWord(Dictionary<string, bool> bwMap, string s)
        {
            return  bwMap.ContainsKey(s.ToLowerInvariant());
        }

        private async Task<Dictionary<string, bool>> GetBadWordsMap(CancellationToken cancellationToken = default)
        {
            var items = await this.badWordsRepository.GetAllAsync(cancellationToken);
            var ht = new Dictionary<string, bool>();
            foreach (var item in items)
            {
                ht[item.Word] = true;
            }
            return ht;
        }
    }
}
