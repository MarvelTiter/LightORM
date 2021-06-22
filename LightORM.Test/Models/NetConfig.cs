using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Test.Models {
    public class NetConfig {
        [PrimaryKey]
        public string ConfigName { get; set; }

        public string IP { get; set; }

        public string Mask { get; set; }

        public string NetGate { get; set; }

        public string DNS { get; set; }

        public string DNS2 { get; set; }
    }
}
