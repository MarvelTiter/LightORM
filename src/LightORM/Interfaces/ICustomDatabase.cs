using System.Text;

namespace LightORM.Interfaces
{
    public interface ICustomDatabase
    {
        string Prefix { get; }
        string Emphasis { get; }
        ///// <summary>
        ///// 获取删除语句的模板，将提供两个参数，{0} 表示表名，{1} 表示别名
        ///// </summary>
        //string DeleteTemplate { get; }
        void Paging(ISelectSqlBuilder builder, StringBuilder sql);
        string ReturnIdentitySql();
        string HandleBooleanValue(bool value);
        ISqlMethodResolver MethodResolver { get; }
        /// <summary>
        /// 是否使用标识引用符号，数据库关键词会强制使用，可通过<see cref="AddKeyWord"/>新增
        /// </summary>
        bool UseIdentifierQuote { get; set; }
        bool IsKeyWord(string keyWork);
        void AddKeyWord(params string[] keyWords);
        
    }
}
