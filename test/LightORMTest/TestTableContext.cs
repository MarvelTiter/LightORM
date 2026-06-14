using LightORM;
using LightORM.Models;
using System.Linq.Expressions;

namespace LightORMTest;

/// <summary>
/// 数据表上下文缓存
/// </summary>
[LightORMTableContext]
internal partial class TestTableContext
{


}



/// <inheritdoc/>
[global::System.CodeDom.Compiler.GeneratedCode("LightOrmTableContextGenerator.TableContextGenerator", "2026.5.15.1")]
public partial record LightORMTest_Models_UserTableInfo : global::LightORM.Interfaces.ITableEntityInfo, global::LightORM.Interfaces.ITableEntityInfo<LightORMTest.Models.User>
{
    private static global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]> columns = new global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]>(CollectColumnInfo);


    public Type Type { get; } = typeof(LightORMTest.Models.User);


    public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");


    public bool IsAnonymousType { get; set; } = false;


    public bool IsTempTable { get; set; } = false;


    public string? CustomName { get; set; } = "USER";


    public string? TargetDatabase => "Test";


    public string? Schema => null;


    public string? Description => null;


    public global::LightORM.Interfaces.ITableColumnInfo[] Columns => columns.Value;


    public global::System.Func<global::System.Data.IDataReader, LightORMTest.Models.User>? DataReaderDeserializer => DeserializeUserFromDbDataReader;


    private static global::LightORM.Interfaces.ITableColumnInfo[] CollectColumnInfo()
    {
        var cols = new global::LightORM.Interfaces.ITableColumnInfo[13];
        cols[0] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(int), "Id", "ID", true, false, true, false, null, null, "自增ID", true, true, true, null, null, false, false, false, false, false, false);
        cols[1] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(string), "UserId", "USER_ID", true, false, false, false, null, null, "用户ID", true, true, true, null, null, false, false, false, false, false, false);
        cols[2] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(string), "UserName", "USER_NAME", false, false, false, false, null, null, "名称", true, true, true, null, null, false, false, false, false, false, false);
        cols[3] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(string), "Password", "PASSWORD", false, false, false, false, null, null, "密码", true, true, true, null, null, false, false, false, false, false, false);
        cols[4] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(int?), "Age", "AGE", false, false, false, false, null, null, "年龄", true, true, true, null, null, false, false, false, false, false, false);
        cols[5] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(LightORMTest.Models.SignType), "Sign", "SIGN", false, false, false, false, null, null, "签名", true, true, true, null, null, false, false, false, false, false, false);
        cols[6] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(System.DateTime?), "LastLogin", "LAST_LOGIN", false, false, false, false, null, null, "最后登录时间", true, true, true, null, null, false, false, false, false, false, false);
        cols[7] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(System.DateTime?), "ModifyTime", "MODIFY_DATE", false, false, false, false, null, null, "修改时间", true, true, true, null, null, false, false, false, false, false, false);
        cols[8] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(bool?), "IsLock", "IS_LOCK", false, false, false, false, null, null, "是否锁定", true, true, true, null, null, false, false, false, false, false, false);
        cols[9] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(int), "Version", "VERSION", false, false, false, false, null, null, null, true, true, true, null, null, false, false, true, false, false, false);
        cols[10] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(byte[]), "Avator", "AVATOR", false, false, false, false, null, null, "头像", true, true, true, null, null, false, false, false, false, false, false);
        cols[11] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(System.Collections.Generic.IEnumerable<LightORMTest.Models.Role>), "UserRoles", null, false, false, false, false, null, null, null, true, true, true, new global::LightORM.Models.NavigateInfo(typeof(LightORMTest.Models.Role), typeof(LightORMTest.Models.UserRole), "UserId", "UserId", true), null, false, false, false, false, false, false);
        cols[12] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.User), typeof(LightORMTest.Models.City), "City", null, false, false, false, false, null, null, null, true, true, true, new global::LightORM.Models.NavigateInfo(typeof(LightORMTest.Models.City), null, "Id", "Uid", false), null, false, false, false, false, false, false);
        return cols;
    }


    public static LightORMTest.Models.User DeserializeUserFromDbDataReader(global::System.Data.IDataReader reader)
    {
        throw new NotImplementedException();
    }

    public void HandleInclude(object value, IncludeInfo info, IContext context)
    {
        var nt = info.NavigateInfo?.NavigateType;
        if (nt == typeof(City))
        {
            IncludeCity((User)value, context, info.IncludeWhereExpression as Expression<Func<City, bool>>);
        }
        else if (nt == typeof(Role))
        {
            IncludeRole((User)value, context, info.IncludeWhereExpression as Expression<Func<Role, bool>>);
        }
    }

    public static void IncludeCity(object value, IContext context, Expression<Func<City, bool>>? where)
    {
        if (value is not User user)
        {
            throw new ArgumentException();
        }
        user.City = context.Select<City>().Where(p => p.Uid == user.Id).First();
    }

    public static void IncludeRole(object value, IContext context, Expression<Func<Role, bool>>? where)
    {
        if (value is not User user)
        {
            throw new ArgumentException();
        }
        user.UserRoles = context.Select<Role>()
            .InnerJoin<UserRole>((r, ur) => r.RoleId == ur.RoleId)
            .InnerJoin<User>((_, ur, u) => ur.UserId == u.UserId)
            .Where((_, ur, u) => u.UserId == user.UserId)
            .WhereIf(where is not null, where!)
            .ToList();

    }


    public static object? GetValue(global::LightORM.Interfaces.ITableColumnInfo col, object target)
    {
        var p = target as LightORMTest.Models.User;
        ArgumentNullException.ThrowIfNull(p);
        if (!col.CanRead)
            return null;
        switch (col.PropertyName)
        {
            case "Id":
                return p.Id;
            case "UserId":
                return p.UserId;
            case "UserName":
                return p.UserName;
            case "Password":
                return p.Password;
            case "Age":
                return p.Age;
            case "Sign":
                return p.Sign;
            case "LastLogin":
                return p.LastLogin;
            case "ModifyTime":
                return p.ModifyTime;
            case "IsLock":
                return p.IsLock;
            case "Version":
                return p.Version;
            case "Avator":
                return p.Avator;
            case "UserRoles":
                return p.UserRoles;
            case "City":
                return p.City;
            default:
                throw new ArgumentException();
        }
    }


    public static void SetValue(global::LightORM.Interfaces.ITableColumnInfo col, object target, object? value)
    {
        var p = target as LightORMTest.Models.User;
        ArgumentNullException.ThrowIfNull(p);
        if (!col.CanWrite)
            return;
        if (value == null)
            return;
        switch (col.PropertyName)
        {
            case "Id":
                p.Id = (int)value;
                break;
            case "UserId":
                p.UserId = (string)value;
                break;
            case "UserName":
                p.UserName = (string)value;
                break;
            case "Password":
                p.Password = (string)value;
                break;
            case "Age":
                p.Age = (int?)value;
                break;
            case "Sign":
                p.Sign = (LightORMTest.Models.SignType)value;
                break;
            case "LastLogin":
                p.LastLogin = (System.DateTime?)value;
                break;
            case "ModifyTime":
                p.ModifyTime = (System.DateTime?)value;
                break;
            case "IsLock":
                p.IsLock = (bool?)value;
                break;
            case "Version":
                p.Version = (int)value;
                break;
            case "Avator":
                p.Avator = (byte[])value;
                break;
            case "UserRoles":
                p.UserRoles = (System.Collections.Generic.IEnumerable<LightORMTest.Models.Role>)value;
                break;
            case "City":
                p.City = (LightORMTest.Models.City)value;
                break;
            default:
                throw new ArgumentException();
        }
    }
}
