using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment
{
    /*
     *   SELECT something FROM tableName 
     */
    internal class SelectFragment : BaseFragment
    {
        private readonly bool distanct;
        public List<string> SelectedFields { get; set; }
        public bool MustUseSelectedFiles { get; set; }
        public SelectFragment(bool distanct)
        {
            this.distanct = distanct;
            SelectedFields = new List<string>();
        }

        protected override void DoResolve(Expression body, params Type[] types)
        {
            foreach (var item in types)
            {
                Tables.SetTableAlias(item);
            }
            ExpressionVisit.Select(body, this);
            SqlAppend("SELECT ");
            if (distanct)
            {
                SqlAppend("DISTINCT ");
            }
            SqlAppend(MustUseSelectedFiles ? string.Join(",", SelectedFields) : "*");
            SqlAppend("\nFROM ");
            SqlAppend(Tables.GetTableName(true));
        }
    }
}
