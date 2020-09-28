using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebTest.Models.RequestModel;
using WebTest.Services;

namespace WebTest.Controllers
{
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IJobService jobService;

        public TestController(IJobService jobService)
        {
            this.jobService = jobService;
        }

        [HttpGet]
        public IActionResult JobList([FromQuery] JOBSRequest param)
        {
            var jobs = jobService.JobsList(param);
            return Ok();
        }
    }
}
