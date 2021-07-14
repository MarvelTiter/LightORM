using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment {
    /*
     *   SELECT something FROM tableName 
     */
    internal class SelectFragment : BaseFragment {
        private readonly bool distanct;
        public List<string> SelectedFields { get; set; }

        public SelectFragment(ISqlContext context, bool distanct) {
            Context = context;
            this.distanct = distanct;
            SelectedFields = new List<string>();
        }

        protected override void DoResolve(Expression body, params Type[] types) {
            foreach (var item in types) {
                Context.SetTableAlias(item);
            }
            ExpressionVisit.Select(body, Context, this);
        }

        public override string ToString() {
            Sql.Append("SELECT ");
            if (distanct) {
                Sql.Append("DISTINCT ");
            }
            Sql.Append(string.Join(",", SelectedFields));
            Sql.Append("\nFROM ");
            Sql.Append(Context.GetTableName(true));
            return Sql.ToString();
        }
    }
}
