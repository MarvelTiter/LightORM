using AntDesign;
using DatabaseUtils.Helper;
using DatabaseUtils.Models;
using DatabaseUtils.Services;
using DatabaseUtils.Template;
using LightORM;
using LightORM.Interfaces;
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
        bool showSetting;
        bool showAttributeSetting;
        private Config config = new Config();
        private string? dbKey;
        private List<string> externalAttributes = [];
        [Inject, NotNull] IExpressionContext? Context { get; set; }
        private List<IDatabaseTableHandler> supportedDb = [];
        async Task Connect()
        {
            if (string.IsNullOrEmpty(config.LastSelectedDb) || string.IsNullOrWhiteSpace(config.Connectstring))
            {
                MessageBox.Show("数据库类型和连接字符串不能为空");
                return;
            }

            dbOperator = DbFactory.GetDbOperator(Context, new DbBaseType(config.LastSelectedDb), config.Connectstring);
            Tables = await dbOperator.GetTablesAsync();
        }

        List<DatabaseTable> GeneratedTables = [];

        async Task Build()
        {
            if (string.IsNullOrEmpty(config.Namespace))
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

            var prefix = config.Prefix ?? "";
            var separator = config.Separator ?? "";
            GeneratedTables.Clear();
            foreach (var table in selected)
            {
                try
                {
                    table.Columns = await dbOperator!.GetTableStructAsync(table.TableName);
                    //string formatted = table.PascalName(prefix, separator);
                    //var content = dbOperator.BuildContent(table, prefix, separator);
                    //var classcontent = string.IsNullOrEmpty(dbKey) ? string.Format(ClassTemplate.Class, config.Namespace, table.TableName, formatted, content) : string.Format(ClassTemplate.ClassWithDatabaseKey, config.Namespace, table.TableName, formatted, content, dbKey);
                    var cb = ClassBuilder.Create(table, dbKey);
                    foreach (var item in table.Columns)
                    {
                       var prop = cb.AddProperty(item);
                        foreach (var ea in externalAttributes)
                        {
                            if (string.IsNullOrEmpty(ea)) continue;
                            prop.AddAttribute(ea);
                        }
                    }
                    GeneratedTables.Add(new()
                    {
                        TableName = table.TableName,
                        GeneratedResult = cb.ToString(dbOperator!, config),
                        CsFileName = cb.ClassName,
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
            if (string.IsNullOrEmpty(config.SavedPath))
            {
                MessageBox.Show("保存路径为空!");
                return;
            }

            if (!Directory.Exists(config.SavedPath))
            {
                Directory.CreateDirectory(config.SavedPath);
            }

            foreach (var item in GeneratedTables)
            {
                var path = Path.Combine(config.SavedPath, $"{item.CsFileName}.cs");
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
                    config.SavedPath = dialog.FolderName;
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

        private static void CopyToClipboard(DatabaseTable table)
        {
            Clipboard.SetText(table.GeneratedResult);
        }
    }
}