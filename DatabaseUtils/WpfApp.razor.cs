using AntDesign;
using DatabaseUtils.Helper;
using DatabaseUtils.Models;
using DatabaseUtils.Services;
using LightORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DatabaseUtils
{
    public partial class WpfApp
    {
        DbBaseType? selectedDb;
        string? connectstring;

        IEnumerable<DatabaseTable> Tables = [];
        IDbOperator? dbOperator = null;
        Config Config = new Config();
        bool showSetting;
        async Task Connect()
        {
            if (!selectedDb.HasValue || string.IsNullOrWhiteSpace(connectstring))
            {
                MessageBox.Show("数据库类型和连接字符串不能为空");
                return;
            }
            dbOperator = DbFactory.GetDbOperator(selectedDb!.Value, connectstring!);
            Tables = await dbOperator.GetTablesAsync();
        }

        async Task Build()
        {
            var selected = Tables.Where(t => t.IsSelected);
            if (!selected.Any())
            {
                MessageBox.Show("未选择表!");
                return;
            }
            foreach (var item in selected)
            {
                item.Columns = await dbOperator!.GetTableStructAsync(item.TableName);
            }
            var prefix = Config.Prefix ?? "";
            var separator = Config.Separator ?? "";
            foreach (var table in selected)
            {
                // TODO 生成CS文件
                //string formatted = table.PascalName(prefix, separator);
                //var temp = item.Template.Replace("${TableName}", table.TableName).Replace("${TableNameFormatted}", formatted);
                //if (item.HasContent)
                //{
                //    string content = table.BuildContent(prefix, separator);
                //    temp = temp.Replace("${Content}", content);
                //}
                //await item.SaveFile(item.FileNameTemplate.Replace("${TableNameFormatted}", formatted), temp);
            }
        }
    }
}
