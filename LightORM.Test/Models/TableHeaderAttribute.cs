/*
 * 文件由 CodeGenerator 生成
 * 时间：2022-04-14
 * 作者：yaoqinglin
 */

using System;

namespace LightORM.Test.Models
{
    internal class TableHeaderAttribute : Attribute
    {
        public TableHeaderAttribute(string label, int sort)
        {
            Label = label;
            Sort = sort;
        }

        public string Label { get; }
        public int Sort { get; }
    }
}