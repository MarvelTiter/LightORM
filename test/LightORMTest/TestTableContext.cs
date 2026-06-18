using LightORM;
using LightORM.Models;
using System.Linq.Expressions;

namespace LightORMTest;

using static LightORMTest.TestTableContext;

/// <summary>
/// 数据表上下文缓存
/// </summary>
[LightORMTableContext]
internal partial class TestTableContext
{
}

/// <inheritdoc/>
[global::System.CodeDom.Compiler.GeneratedCode("LightOrmTableContextGenerator.TableContextGenerator", "2026.5.15.1")]
public partial record LightORMTest_Models_User22TableInfo : global::LightORM.Interfaces.ITableEntityInfo, global::LightORM.Interfaces.ITableEntityInfo<LightORMTest.Models.User>
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

    public void HandleInclude(global::LightORM.IContext context, object value, global::System.Collections.Generic.IEnumerable<global::LightORM.Models.IncludeInfo> info)
    {
        foreach (var item in info)
        {
            var nt = item.NavigateInfo?.NavigateType;
            if (nt == typeof(City))
            {
                IncludeCity(context, value, item);
            }
            else if (nt == typeof(Role))
            {
                IncludeRole(context, value, item);
            }
        }
    }

    public Task HandleIncludeAsync(IContext dbContext, object entity, IEnumerable<IncludeInfo> infos, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public static void IncludeCity(IContext context, object value, IncludeInfo info)
    {
        if (value is User user)
        {
            var whereExpression = info.IncludeWhereExpression as Expression<Func<City, bool>>;
            var includeValue = context.Select<City>()
                .Where(p => p.Uid == user.Id)
                .WhereIf(whereExpression is not null, whereExpression!)
                .First();
            if (includeValue is not null && info.ThenIncludes?.Count > 0)
                LightORMTest_Models_City.HandleInclude(context, includeValue, info.ThenIncludes);
            user.City = includeValue;
        }
        else if (value is IEnumerable<User> users)
        {
            var userList = users as IList<User> ?? users.ToList();
            if (userList.Count == 0) return;
            var whereExpression = info.IncludeWhereExpression as Expression<Func<City, bool>>;
            var ids = userList.Select(p => p.Id);
            var includeValue = context.Select<City>()
                .Where(p => ids.Contains(p.Uid))
                .WhereIf(whereExpression is not null, whereExpression!)
                .ToList().GroupBy(p => p.Uid).ToDictionary(g => g.Key, g => g.First());
            foreach (var u in userList)
            {
                if (includeValue.TryGetValue(u.Id, out var city))
                {
                    u.City = city;
                }
            }

            if (info.ThenIncludes?.Count > 0 && includeValue.Count > 0)
            {
                LightORMTest_Models_City.HandleInclude(context, includeValue.Values.ToList(), info.ThenIncludes);
            }
        }
    }

    public static void IncludeRole(IContext context, object value, IncludeInfo info)
    {
        if (value is User user)
        {
            var whereExpression = info.IncludeWhereExpression as Expression<Func<Role, bool>>;
            var userRoles = context.Select<Role>()
                .InnerJoin<UserRole>((r, ur) => r.RoleId == ur.RoleId)
                .InnerJoin<User>((_, ur, u) => ur.UserId == u.UserId)
                .Where((_, ur, u) => u.UserId == user.UserId)
                .WhereIf(whereExpression is not null, whereExpression!)
                .ToList();
            var userUserRoles = userRoles as IList<Role> ?? userRoles.ToList();
            if (userRoles is not null && userUserRoles.Any() && info.ThenIncludes?.Count > 0)
            {
                LightORMTest_Models_Role.HandleInclude(context, userRoles, info.ThenIncludes);
            }

            user.UserRoles = userUserRoles;
        }
        else if (value is IEnumerable<User> users)
        {
            var userList = users as IList<User> ?? users.ToList();
            if (userList.Count == 0) return;
            var whereExpression = info.IncludeWhereExpression as Expression<Func<Role, bool>>;
            var ids = userList.Select(p => p.UserId);
            var includeValue = context.Select<Role>()
                .InnerJoin<UserRole>((r, ur) => r.RoleId == ur.RoleId)
                .InnerJoin<User>((_, ur, u) => ur.UserId == u.UserId)
                .Where((_, ur, u) => ids.Contains(u.UserId))
                .WhereIf(whereExpression is not null, whereExpression!)
                .ToList((r, ur, u) => new { Role = r, u.UserId })
                .GroupBy(v => v.UserId)
                .ToDictionary(g => g.Key, g => g.Select(gg => gg.Role).ToList());
            foreach (var u in userList)
            {
                if (includeValue.TryGetValue(u.UserId, out var roles))
                {
                    u.UserRoles = roles;
                }
            }
            if (includeValue is not null && includeValue.Any() && info.ThenIncludes?.Count > 0)
            {
                var distinctRoles = includeValue.Values.SelectMany(r => r).Distinct().ToList();
                LightORMTest_Models_Role.HandleInclude(context, distinctRoles, info.ThenIncludes);
            }
        }
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