using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    /// <summary>
    /// 产品
    /// </summary>
    public class Product
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public string? ProductCode { get; set; }

        public string? ProductName { get; set; }

        public bool DeleteMark { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? ModifyTime { get; set; }
    }

    public class ProductV2
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public string? ProductCode { get; set; }

        public string? ProductName { get; set; }

        public bool DeleteMark { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? ModifyTime { get; set; }
    }
}
