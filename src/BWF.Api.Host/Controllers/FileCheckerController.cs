using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BWF.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BWF.Api.Host.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileCheckerController : ControllerBase
    {
        private readonly ICheckEngine checkEngine;

        public FileCheckerController(ICheckEngine checkEngine)
        {
            if (checkEngine is null)
            {
                throw new ArgumentNullException(nameof(checkEngine));
            }

            this.checkEngine = checkEngine;
        }

        [HttpPost]
        public async Task<Models.CheckResult> Check([Required] IFormFile file)
        {
            var res = new Models.CheckResult();
            using (var stream = file.OpenReadStream())
            {
                var check = await checkEngine.Run(stream);
                foreach (var item in check.Words)
                {
                    res.Words.Add(new Models.WordResult { Count = item.Value, Word = item.Key });
                }
                res.MilisecondsDuration = check.ProcessingTime.Milliseconds;
                res.DbReadMilisecondsDuration = check.DBReadTime.Milliseconds;
            }

            return res;
        }
    }
}
