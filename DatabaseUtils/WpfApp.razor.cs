using AntDesign;
using DatabaseUtils.Helper;
using DatabaseUtils.Models;
using DatabaseUtils.Services;
using DatabaseUtils.Template;
using LightORM;
using Microsoft.AspNetCore.Components;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DatabaseUtils
{
    public partial class WpfApp
    {
        IEnumerable<DatabaseTable> Tables = [];
        IDbOperator? dbOperator = null;
        Config Config = new Config();
        bool showSetting;
        public static string? LastSelectedDb
        {
            get => Properties.Local.Default.LastSelectedDb;
            set
            {
                Properties.Local.Default.LastSelectedDb = value;
                Properties.Local.Default.Save();
            }
        }

        public static string? Connectstring
        {
            get => Properties.Local.Default.Connectstring;
            set
            {
                Properties.Local.Default.Connectstring = value;
                Properties.Local.Default.Save();
            } 
        }
        [Inject, NotNull] IExpressionContext? Context { get; set; }

        async Task Connect()
        {
            if (LastSelectedDb == null || string.IsNullOrWhiteSpace(Connectstring))
            {
                MessageBox.Show("数据库类型和连接字符串不能为空");
                return;
            }
            dbOperator = DbFactory.GetDbOperator(Context, new DbBaseType(LastSelectedDb), Connectstring!);
            Tables = await dbOperator.GetTablesAsync();
        }

        List<DatabaseTable> GeneratedTables = [];

        async Task Build()
        {
            if (string.IsNullOrEmpty(Config.Namespace))
            {
                MessageBox.Show("未设置命名空间!");
                return;
            }
            var selected = Tables.Where(t => t.IsSelected);
            if (!selected.Any())
            {
                MessageBox.Show("未选择表!");
                return;
            }
            var prefix = Config.Prefix ?? "";
            var separator = Config.Separator ?? "";
            GeneratedTables.Clear();
            foreach (var table in selected)
            {
                try
                {
                    table.Columns = await dbOperator!.GetTableStructAsync(table.TableName);
                    string formatted = table.PascalName(prefix, separator);
                    var content = table.BuildContent(prefix, separator);
                    var classcontent = string.Format(ClassTemplate.Class, Config.Namespace, table.TableName, formatted, content);
                    GeneratedTables.Add(new()
                    {
                        TableName = table.TableName,
                        CsFileName = formatted,
                        GeneratedResult = classcontent,
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        async Task SaveToLocal()
        {
            if (string.IsNullOrEmpty(Config.SavedPath))
            {
                MessageBox.Show("保存路径为空!");
                return;
            }
            if (!Directory.Exists(Config.SavedPath))
            {
                Directory.CreateDirectory(Config.SavedPath);
            }
            foreach (var item in GeneratedTables)
            {
                var path = Path.Combine(Config.SavedPath, $"{item.CsFileName}.cs");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                await File.WriteAllTextAsync(path, item.GeneratedResult);
            }
        }

        Task SelectPath()
        {
            return Task.Run(() =>
            {

                OpenFolderDialog dialog = new OpenFolderDialog();
                if (dialog.ShowDialog() == true)
                {
                    Config.SavedPath = dialog.FolderName;
                }
            });

        }

        void AllHandle(bool newValue)
        {
            foreach (var table in Tables)
            {
                table.IsSelected = newValue;
            }
        }
    }
}
