using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql;

partial class ExpressionCoreSql
{
	//public IExpSelect Select()
	//{
	//	return new SelectProvider0(Ado);
	//}
	//public IExpSelect<TResult> WithTemp<TResult>(Func<IExpSelect<TTemp>> createTemp)
	//{
	//	var temp = createTemp.Invoke();
	//	throw new NotImplementedException();
	//}
	public IExpSelect<TResult> UnionAll<TResult>(params IExpSelect<TResult>[] selects)
	{
		throw new NotImplementedException();
	}
}
