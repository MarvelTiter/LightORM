using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebTest.Models.Entities;
using WebTest.Models.RequestModel;

namespace WebTest.Services
{
    public interface IJobService
    {
        IEnumerable<JOBS> JobsList(JOBSRequest req);
    }
}
