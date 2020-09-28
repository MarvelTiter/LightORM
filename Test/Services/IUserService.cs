using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Models;
using Test.Models.Entities;
using Test.Models.QueryParam;

namespace Test.Services
{
    interface IUserService
    {
        int Insert(Users user);
        int Update(Users user);
        int Update(object dyc);
        IEnumerable<Users> Select(UserQueryParam p);

    }
}
