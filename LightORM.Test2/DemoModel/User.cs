using MDbEntity.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LightORM.Test2.DemoModel
{
    public enum YesOrNo
    {
        [Display(Name = "是")]
        Yes = 1,
        [Display(Name = "否")]
        No = 0,
    }
    [Table(Name = "USER222")]
    [TableIndex(nameof(UserName), nameof(Password))]
    public class User
    {
        [Column(Name = "ID", AutoIncrement = true, PrimaryKey = true)]
        public long? Id { get; set; }
        [Column(Name = "USER_ID", PrimaryKey = true, Comment = "用户ID")]
        public string UserId { get; set; }
        [Column(Name = "USER_NAME", Default = "TEST", Comment = "用户姓名")]
        public string UserName { get; set; }
        [Column(Name = "PASSWORD", PrimaryKey = true)]
        public string Password { get; set; }
        /// <summary>
        /// 用户是否启用(1-是，0-否）
        /// </summary>
        public YesOrNo Enable { get; set; }
        [MDbEntity.Attributes.Ignore]
        public object Test { get; set; }

        public override string ToString()
        {
            return $"{UserId}-{UserName}";
        }
    }


    [Table(Name = "machineStatus")]
    public class MachineStatus
    {
        [Column(Name = "ID", AutoIncrement = true, PrimaryKey = true)]
        public long? Id { get; set; }
        [Column(Name = "serverIp")]
        public string ServerIp { get; set; }
        [Column(Name = "serverName")]
        public string ServerName { get; set; }
        [Column(Name = "serverSeq")]
        public string ServerSeq { get; set; }
        [Column(Name = "recordTime")]
        public DateTime RecordTime { get; set; }
        [Column(Name = "cpuAvg")]
        public float CpuAvg { get; set; }
        [Column(Name = "cpuPeak")]
        public float CpuPeak { get; set; }
        [Column(Name = "memoryAvg")]
        public float MemoryAvg { get; set; }
        [Column(Name = "memoryPeak")]
        public float MemoryPeak { get; set; }
        [Column(Name = "disk1Usage")]
        public float Disk1Usage { get; set; }
        [Column(Name = "disk2Usage")]
        public float Disk2Usage { get; set; }
        [Column(Name = "disk3Usage")]
        public float Disk3Usage { get; set; }
        [Column(Name = "disk1IO")]
        public float Disk1IO { get; set; }
        [Column(Name = "disk2IO")]
        public float Disk2IO { get; set; }
        [Column(Name = "disk3IO")]
        public float Disk3IO { get; set; }
        [Column(Name = "diskReadAvg")]
        public float DiskReadAvg { get; set; }
        [Column(Name = "diskWriteAvg")]
        public float DiskWriteAvg { get; set; }
        [Column(Name = "databaseIO")]
        public float DatabaseIO { get; set; }
        [Column(Name = "databaseAccess")]
        public float DatabaseAccess { get; set; }

    }
}
