using LightORM;
using LightORM.Utils;
using System.Collections.Concurrent;
using System.Data;

namespace LightORMTest;

/// <summary>
/// 数据表上下文缓存
/// </summary>
[LightORMTableContext]
internal partial class TestTableContext
{

}

/// <inheritdoc/>
[global::System.CodeDom.Compiler.GeneratedCode("LightOrmTableContextGenerator.TableContextGenerator", "3.1.7.7")]
public partial record LightORMTest_Models_UserRoleTableInfo : global::LightORM.Interfaces.ITableEntityInfo, global::LightORM.Interfaces.ITableEntityInfo<LightORMTest.Models.UserRole>
{
    private static global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]> columns = new global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]>(CollectColumnInfo);
    public Type Type { get; } = typeof(LightORMTest.Models.UserRole);
    public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
    public bool IsAnonymousType { get; set; } = false;
    public bool IsTempTable { get; set; } = false;
    public string? CustomName { get; set; } = "USER_ROLE";
    public string? TargetDatabase => null;
    public string? Description => null;
    public global::LightORM.Interfaces.ITableColumnInfo[] Columns => columns.Value;
    public global::System.Func<global::System.Data.IDataReader, LightORMTest.Models.UserRole>? DataReaderDeserializer => DeserializeUserRoleFromDbDataReader;
    private static global::LightORM.Interfaces.ITableColumnInfo[] CollectColumnInfo()
    {
        var cols = new global::LightORM.Interfaces.ITableColumnInfo[4];
        cols[0] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.UserRole), "UserId", "USER_ID", true, false, false, false, null, null, "用户ID", true, true, true, null, null, false, false, false, false);
        cols[1] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.UserRole), "RoleId", "ROLE_ID", true, false, false, false, null, null, "角色ID", true, true, true, null, null, false, false, false, false);
        cols[2] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.UserRole), "User", null, false, false, false, false, null, null, null, true, true, true, new global::LightORM.Models.NavigateInfo(typeof(LightORMTest.Models.User), null, "UserId", "UserId", false), null, false, false, false, false);
        cols[3] = new global::LightORM.Models.ColumnInfo(typeof(LightORMTest.Models.UserRole), "Role", null, false, false, false, false, null, null, null, true, true, true, new global::LightORM.Models.NavigateInfo(typeof(LightORMTest.Models.Role), null, "RoleId", "RoleId", false), null, false, false, false, false);
        return cols;
    }
    private readonly struct SchemeInfo
    {
        public int UserId { get; }
        public int RoleId { get; }

        public SchemeInfo(IDataReader reader)
        {

            //string[] fields = new string[reader.FieldCount];
            //for (int i = 0; i < reader.FieldCount; i++)
            //{
            //    fields[i] = reader.GetName(i);
            //}
            //UserId = Array.IndexOf(fields, "UserId");
            //RoleId = Array.IndexOf(fields, "RoleId");
            UserId = -1;
            RoleId = -1;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);

                if (UserId == -1 && string.Equals(fieldName, "UserId", StringComparison.OrdinalIgnoreCase))
                    UserId = i;
                else if (RoleId == -1 && string.Equals(fieldName, "RoleId", StringComparison.OrdinalIgnoreCase))
                    RoleId = i;

                // 如果两个都找到了，提前退出
                if (UserId != -1 && RoleId != -1)
                    break;
            }
        }
    }
    private static readonly ConcurrentDictionary<string, SchemeInfo> schemes = [];

    public static LightORMTest.Models.UserRole DeserializeUserRoleFromDbDataReader(global::System.Data.IDataReader reader)
    {
        var scheme = schemes.GetOrAdd(key, k => new(reader));
        UserRole e = new UserRole()
        {
            UserId = reader.GetStringSafe(scheme.UserId),
            RoleId = ReaderHelper.GetString(reader, scheme.RoleId)
        };
        return e;
    }
    public static object? GetValue(global::LightORM.Interfaces.ITableColumnInfo col, object target)
    {
        var p = target as LightORMTest.Models.UserRole;
        ArgumentNullException.ThrowIfNull(p);
        if (!col.CanRead)
            return null;
        switch (col.PropertyName)
        {
            case "UserId":
                return p.UserId;
            case "RoleId":
                return p.RoleId;
            case "User":
                return p.User;
            case "Role":
                return p.Role;
            default:
                throw new ArgumentException();
        }
    }
    public static void SetValue(global::LightORM.Interfaces.ITableColumnInfo col, object target, object? value)
    {
        var p = target as LightORMTest.Models.UserRole;
        ArgumentNullException.ThrowIfNull(p);
        if (!col.CanWrite)
            return;
        if (value == null)
            return;
        switch (col.PropertyName)
        {
            case "UserId":
                p.UserId = (string)value;
                break;
            case "RoleId":
                p.RoleId = (string)value;
                break;
            case "User":
                p.User = (LightORMTest.Models.User)value;
                break;
            case "Role":
                p.Role = (LightORMTest.Models.Role)value;
                break;
            default:
                throw new ArgumentException();
        }
    }
}

internal static class ReaderHelper
{
    public static string? GetString(IDataReader reader, int ordinal)
    {
        if (ordinal < 0)
            return default;
        if (reader.IsDBNull(ordinal))
            return default;
        return reader.GetString(ordinal);
    }
}
