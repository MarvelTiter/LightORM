using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseUtils.Helper
{

    public class TypeMap
    {
        public static bool Map(string dbType, string nullable, out string type)
        {
            //TODO临时硬编码
            var d = dbType.ToLower();
            if (d.Contains("char"))
            {
                type = "string";
            }
            else if (d.Contains("int") || d.Contains("bit") || d.Contains("number"))
            {
                type = nullable == "YES" ? "int?" : "int";
            }
            else if (d.Contains("date"))
            {
                type = nullable == "YES" ? "DateTime?" : "DateTime";
            }
            else
            {
                type = "";
                return false;
                //throw new Exception($"未知数据类型 {dbType}");
            }
            return true;
        }
        public TypeMap()
        {

        }
    }


    #region Oracle
    /*
DATE(7)
VARCHAR2(128)
CHAR(1)
VARCHAR2(100)
VARCHAR2(3)
VARCHAR2(2000)
CHAR(8)
CHAR(4)
CHAR(13)
VARCHAR2(54)
VARCHAR2(120)
DATE(8)
VARCHAR2(15)
VARCHAR2(10)
VARCHAR2(18)
VARCHAR2(200)
BLOB(4000)
VARCHAR2(3000)
FRM_TMS_TRANS_AQ(64)
VARCHAR2(2)
VARCHAR2(64)
VARCHAR2(4)
VARCHAR2(22)
VARCHAR2(514)
VARCHAR2(11)
VARCHAR2(80)
RAW(16)
CHAR(10)
VARCHAR2(132)
VARCHAR2(40)
VARCHAR2(5)
NUMBER(22)
VARCHAR2(1000)
VARCHAR2(50)
VARCHAR2(300)
VARCHAR2(7)
VARCHAR2(68)
TIMESTAMP(0)(7)
VARCHAR2(3024)
FRM_TMS_TRANS_AQ(1)
VARCHAR2(256)
VARCHAR2(6)
VARCHAR2(20)
VARCHAR2(1)
VARCHAR2(14)
VARCHAR2(400)
VARCHAR2(26)
VARCHAR2(2048)
CHAR(6)
TIMESTAMP(6)(20)
VARCHAR2(24)
VARCHAR2(512)
VARCHAR2(30)
VARCHAR2(500)
VARCHAR2(510)
TIMESTAMP(6)(11)
VARCHAR2(4000)
VARCHAR2(32)
VARCHAR2(1024)
VARCHAR2(8)
CHAR(2)
VARCHAR2(96)
VARCHAR2(36)
VARCHAR2(3500)
VARCHAR2(9)
ANYDATA(4360)
ANYDATA(3748)
ROWID(10)
VARCHAR2(12)
VARCHAR2(255)
VARCHAR2(16)
CLOB(4000)
VARCHAR2(95)
VARCHAR2(250)
VARCHAR2(13)
*/
    #endregion
}
