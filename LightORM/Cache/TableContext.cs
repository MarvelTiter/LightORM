using LightORM.ExpressionSql;
using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LightORM.Cache
{
    internal static class TableContext
    {
        public static TableEntity GetTableInfo<T>()
        {
            return GetTableInfo(typeof(T));
        }
        public static TableEntity GetTableInfo(Type type)
        {
            var cacheKey = $"DbTable_{type.FullName}";

            var entityInfoCache = StaticCache<TableEntity>.GetOrAdd(cacheKey, () =>
            {
                var entityInfo = new TableEntity(type);
                var lightTableAttribute = type.GetAttribute<LightTableAttribute>();
#if NET6_0_OR_GREATER
                var tableAttribute = type.GetAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
                entityInfo.CustomName = lightTableAttribute?.Name ?? tableAttribute?.Name ?? type.Name;
#else
                entityInfo.CustomName = lightTableAttribute?.Name ?? type.Name;
#endif
                if (!entityInfo.IsAnonymousType)
                {
                    entityInfo.TargetDatabase = lightTableAttribute?.DatabaseKey;
                    var descriptionAttribute = type.GetAttribute<DescriptionAttribute>();
                    if (descriptionAttribute != null)
                    {
                        entityInfo.Description = descriptionAttribute.Description;
                    }
                }

                var propertyInfos = type.GetProperties();

                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                var propertyColumnInfos = propertyInfos.Select(property => new ColumnInfo(entityInfo, property));
                entityInfo.Columns.AddRange(propertyColumnInfos);
                entityInfo.Alias = $"a{StaticCache<TableEntity>.Count}";
                return entityInfo;
            });
            // 拷贝
            return entityInfoCache with { };
        }
    }
}
