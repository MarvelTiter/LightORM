/*
 * 文件由 CodeGenerator 生成
 * 时间：2022-04-14
 * 作者：yaoqinglin
 */

using MDbEntity.Attributes;

namespace LightORM.Test.Models
{
    /// <summary>
    /// 
    /// </summary>
    [TableName("INSPECT_ITEMDATA_F1_XXXX")]
    public class InspectItemdataF1Xxxx
    {

        /// <summary>
        /// 检验流水号
        /// </summary>
        [ColumnName("JYLSH")]
        [TableHeader("检验流水号", 0)]
        public string Jylsh { get; set; }

        /// <summary>
        /// 安检机构编号
        /// </summary>
        [ColumnName("JYJGBH")]
        [TableHeader("安检机构编号", 1)]
        public string Jyjgbh { get; set; }

        /// <summary>
        /// 检测线代号
        /// </summary>
        [ColumnName("JCXDH")]
        [TableHeader("检测线代号", 2)]
        public string Jcxdh { get; set; }

        /// <summary>
        /// 检验次数
        /// </summary>
        [ColumnName("JYCS")]
        [TableHeader("检验次数", 3)]
        public int Jycs { get; set; }

        /// <summary>
        /// 检验项目（F1）
        /// </summary>
        [ColumnName("JYXM")]
        [TableHeader("检验项目（F1）", 4)]
        public string Jyxm { get; set; }

        /// <summary>
        /// 号牌种类
        /// </summary>
        [ColumnName("HPZL")]
        [TableHeader("号牌种类", 5)]
        public string Hpzl { get; set; }

        /// <summary>
        /// 号牌号码
        /// </summary>
        [ColumnName("HPHM")]
        [TableHeader("号牌号码", 6)]
        public string Hphm { get; set; }

        /// <summary>
        /// 车辆识别代号
        /// </summary>
        [ColumnName("CLSBDH")]
        [TableHeader("车辆识别代号", 7)]
        public string Clsbdh { get; set; }

        /// <summary>
        /// 1、号牌号码/车辆类型（0-未检，1-合格，2-不合格，下同）
        /// </summary>
        [ColumnName("RHPLX")]
        [TableHeader("1、号牌号码/车辆类型（0-未检，1-合格，2-不合格，下同）", 8)]
        public string Rhplx { get; set; }

        /// <summary>
        /// 2、车辆品牌/型号
        /// </summary>
        [ColumnName("RPPXH")]
        [TableHeader("2、车辆品牌/型号", 9)]
        public string Rppxh { get; set; }

        /// <summary>
        /// 3、车辆识别代号（或整车出厂编号）
        /// </summary>
        [ColumnName("RVIN")]
        [TableHeader("3、车辆识别代号（或整车出厂编号）", 10)]
        public string Rvin { get; set; }

        /// <summary>
        /// 4、发动机号码（或电动机号码）
        /// </summary>
        [ColumnName("RFDJH")]
        [TableHeader("4、发动机号码（或电动机号码）", 11)]
        public string Rfdjh { get; set; }

        /// <summary>
        /// 5、车辆颜色和形状
        /// </summary>
        [ColumnName("RCSYS")]
        [TableHeader("5、车辆颜色和形状", 12)]
        public string Rcsys { get; set; }

        /// <summary>
        /// 6、外廓尺寸
        /// </summary>
        [ColumnName("RWKCC")]
        [TableHeader("6、外廓尺寸", 13)]
        public string Rwkcc { get; set; }

        /// <summary>
        /// 7、轴距
        /// </summary>
        [ColumnName("RZJ")]
        [TableHeader("7、轴距", 14)]
        public string Rzj { get; set; }

        /// <summary>
        /// 8、整备质量
        /// </summary>
        [ColumnName("RZBZL")]
        [TableHeader("8、整备质量", 15)]
        public string Rzbzl { get; set; }

        /// <summary>
        /// 9、核定载人数
        /// </summary>
        [ColumnName("RHDZRS")]
        [TableHeader("9、核定载人数", 16)]
        public string Rhdzrs { get; set; }

        /// <summary>
        /// 10、核定载质量
        /// </summary>
        [ColumnName("RHDZLL")]
        [TableHeader("10、核定载质量", 17)]
        public string Rhdzll { get; set; }

        /// <summary>
        /// 11、栏板高度
        /// </summary>
        [ColumnName("RLBGD")]
        [TableHeader("11、栏板高度", 18)]
        public string Rlbgd { get; set; }

        /// <summary>
        /// 12、后轴钢板弹簧片数
        /// </summary>
        [ColumnName("RHZGBTHPS")]
        [TableHeader("12、后轴钢板弹簧片数", 19)]
        public string Rhzgbthps { get; set; }

        /// <summary>
        /// 13、客车应急出口
        /// </summary>
        [ColumnName("RKCYJCK")]
        [TableHeader("13、客车应急出口", 20)]
        public string Rkcyjck { get; set; }

        /// <summary>
        /// 14、客车乘客通道和引道
        /// </summary>
        [ColumnName("RKCCKTD")]
        [TableHeader("14、客车乘客通道和引道", 21)]
        public string Rkccktd { get; set; }

        /// <summary>
        /// 15、货厢
        /// </summary>
        [ColumnName("RHX")]
        [TableHeader("15、货厢", 22)]
        public string Rhx { get; set; }

        /// <summary>
        /// 16、车身外观
        /// </summary>
        [ColumnName("RCSWG")]
        [TableHeader("16、车身外观", 23)]
        public string Rcswg { get; set; }

        /// <summary>
        /// 17、外观标识、标注和标牌
        /// </summary>
        [ColumnName("RWGBS")]
        [TableHeader("17、外观标识、标注和标牌", 24)]
        public string Rwgbs { get; set; }

        /// <summary>
        /// 18、外部照明和信号灯具
        /// </summary>
        [ColumnName("RWBZM")]
        [TableHeader("18、外部照明和信号灯具", 25)]
        public string Rwbzm { get; set; }

        /// <summary>
        /// 19、轮胎
        /// </summary>
        [ColumnName("RLT")]
        [TableHeader("19、轮胎", 26)]
        public string Rlt { get; set; }

        /// <summary>
        /// 20、号牌及号牌安装
        /// </summary>
        [ColumnName("RHPAZ")]
        [TableHeader("20、号牌及号牌安装", 27)]
        public string Rhpaz { get; set; }

        /// <summary>
        /// 21、加装/改装灯具
        /// </summary>
        [ColumnName("RJZGJ")]
        [TableHeader("21、加装/改装灯具", 28)]
        public string Rjzgj { get; set; }

        /// <summary>
        /// 22、汽车安全带
        /// </summary>
        [ColumnName("RQCAQD")]
        [TableHeader("22、汽车安全带", 29)]
        public string Rqcaqd { get; set; }

        /// <summary>
        /// 23、机动车用三角警告牌
        /// </summary>
        [ColumnName("RSJP")]
        [TableHeader("23、机动车用三角警告牌", 30)]
        public string Rsjp { get; set; }

        /// <summary>
        /// 24、灭火器
        /// </summary>
        [ColumnName("RMHQ")]
        [TableHeader("24、灭火器", 31)]
        public string Rmhq { get; set; }

        /// <summary>
        /// 25、行驶记录装置
        /// </summary>
        [ColumnName("RXSJLY")]
        [TableHeader("25、行驶记录装置", 32)]
        public string Rxsjly { get; set; }

        /// <summary>
        /// 26、车身反光标识
        /// </summary>
        [ColumnName("RCSFGBS")]
        [TableHeader("26、车身反光标识", 33)]
        public string Rcsfgbs { get; set; }

        /// <summary>
        /// 27、车辆尾部标志板
        /// </summary>
        [ColumnName("RCLWBZB")]
        [TableHeader("27、车辆尾部标志板", 34)]
        public string Rclwbzb { get; set; }

        /// <summary>
        /// 28、侧后防护装置
        /// </summary>
        [ColumnName("RCHFH")]
        [TableHeader("28、侧后防护装置", 35)]
        public string Rchfh { get; set; }

        /// <summary>
        /// 29、应急锤
        /// </summary>
        [ColumnName("RYJC")]
        [TableHeader("29、应急锤", 36)]
        public string Ryjc { get; set; }

        /// <summary>
        /// 30、急救箱
        /// </summary>
        [ColumnName("RJJX")]
        [TableHeader("30、急救箱", 37)]
        public string Rjjx { get; set; }

        /// <summary>
        /// 31、限速功能或限速装置
        /// </summary>
        [ColumnName("RXSGN")]
        [TableHeader("31、限速功能或限速装置", 38)]
        public string Rxsgn { get; set; }

        /// <summary>
        /// 32、防抱死制动装置
        /// </summary>
        [ColumnName("RFBS")]
        [TableHeader("32、防抱死制动装置", 39)]
        public string Rfbs { get; set; }

        /// <summary>
        /// 33、辅助制动装置
        /// </summary>
        [ColumnName("RFZZD")]
        [TableHeader("33、辅助制动装置", 40)]
        public string Rfzzd { get; set; }

        /// <summary>
        /// 34、盘式制动器
        /// </summary>
        [ColumnName("RPSZDQ")]
        [TableHeader("34、盘式制动器", 41)]
        public string Rpszdq { get; set; }

        /// <summary>
        /// 35、紧急切断装置
        /// </summary>
        [ColumnName("RJJQD")]
        [TableHeader("35、紧急切断装置", 42)]
        public string Rjjqd { get; set; }

        /// <summary>
        /// 36、发动机舱自动灭火装置
        /// </summary>
        [ColumnName("RFDJCMH")]
        [TableHeader("36、发动机舱自动灭火装置", 43)]
        public string Rfdjcmh { get; set; }

        /// <summary>
        /// 37、手动机械断电开关
        /// </summary>
        [ColumnName("RSDDD")]
        [TableHeader("37、手动机械断电开关", 44)]
        public string Rsddd { get; set; }

        /// <summary>
        /// 38、副制动踏板
        /// </summary>
        [ColumnName("RFZDTB")]
        [TableHeader("38、副制动踏板", 45)]
        public string Rfzdtb { get; set; }

        /// <summary>
        /// 39、校车标志灯和校车停车指示标志牌
        /// </summary>
        [ColumnName("RXCBZ")]
        [TableHeader("39、校车标志灯和校车停车指示标志牌", 46)]
        public string Rxcbz { get; set; }

        /// <summary>
        /// 40、危险货物运输车标志
        /// </summary>
        [ColumnName("RWXHWBZ")]
        [TableHeader("40、危险货物运输车标志", 47)]
        public string Rwxhwbz { get; set; }

        /// <summary>
        /// 80、联网查询
        /// </summary>
        [ColumnName("RLWCX")]
        [TableHeader("80、联网查询", 48)]
        public string Rlwcx { get; set; }

        /// <summary>
        /// 车外廓长
        /// </summary>
        [ColumnName("CWKC")]
        [TableHeader("车外廓长", 49)]
        public int Cwkc { get; set; }

        /// <summary>
        /// 车外廓宽
        /// </summary>
        [ColumnName("CWKK")]
        [TableHeader("车外廓宽", 50)]
        public int Cwkk { get; set; }

        /// <summary>
        /// 车外廓高
        /// </summary>
        [ColumnName("CWKG")]
        [TableHeader("车外廓高", 51)]
        public int Cwkg { get; set; }

        /// <summary>
        /// 整备质量
        /// </summary>
        [ColumnName("ZBZL")]
        [TableHeader("整备质量", 52)]
        public int Zbzl { get; set; }

        /// <summary>
        /// 机动车所有人
        /// </summary>
        [ColumnName("SYR")]
        [TableHeader("机动车所有人", 53)]
        public string Syr { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [ColumnName("SJHM")]
        [TableHeader("手机号码", 54)]
        public string Sjhm { get; set; }

        /// <summary>
        /// 联系地址
        /// </summary>
        [ColumnName("LXDZ")]
        [TableHeader("联系地址", 55)]
        public string Lxdz { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        [ColumnName("YZBM")]
        [TableHeader("邮政编码", 56)]
        public string Yzbm { get; set; }

        /// <summary>
        /// 检验员建议
        /// </summary>
        [ColumnName("JYYJY")]
        [TableHeader("检验员建议", 57)]
        public string Jyyjy { get; set; }

        /// <summary>
        /// 车辆外观检验检验员
        /// </summary>
        [ColumnName("WGJCJYY")]
        [TableHeader("车辆外观检验检验员", 58)]
        public string Wgjcjyy { get; set; }

        /// <summary>
        /// 车辆外观检验检验员（身份证号）
        /// </summary>
        [ColumnName("WGJCJYYSFZH")]
        [TableHeader("车辆外观检验检验员（身份证号）", 59)]
        public string Wgjcjyysfzh { get; set; }

        /// <summary>
        /// 81肢体残疾人操纵辅助装置
        /// </summary>
        [ColumnName("ZTCJRFZZZ")]
        [TableHeader("81肢体残疾人操纵辅助装置", 60)]
        public string Ztcjrfzzz { get; set; }

    }
}
