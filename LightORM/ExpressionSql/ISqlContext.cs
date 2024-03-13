namespace LightORM.ExpressionSql;

internal interface ISqlContext
{
    int Length { get; }
    bool EndWith(string end);
    void Insert(int index, string content);
    void AppendDbParameter(object value);
    void AddEntityField(string name, object value);
    void Append(string sql);
    void AddField(string fieldName, string parameterName, string tableAlias = "", string FieldAlias = "");
    string ToSql();
}
