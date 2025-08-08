using System.Collections;
using System.Dynamic;
using System.Reflection;
using System.Text;

#nullable disable
namespace LightORM.SqlExecutor;
internal class MapperTable
{
    private string[] fieldNames;
    private readonly Dictionary<string, int> fieldNameLookup;

    internal string[] FieldNames => fieldNames;

    public MapperTable(string[] fieldNames)
    {
        this.fieldNames = fieldNames ?? throw new ArgumentNullException(nameof(fieldNames));

        fieldNameLookup = new Dictionary<string, int>(fieldNames.Length, StringComparer.Ordinal);
        // if there are dups, we want the **first** key to be the "winner" - so iterate backwards
        for (int i = fieldNames.Length - 1; i >= 0; i--)
        {
            string key = fieldNames[i];
            if (key != null) fieldNameLookup[key] = i;
        }
    }

    internal int IndexOfName(string name)
    {
        return (name != null && fieldNameLookup.TryGetValue(name.ToUpper(), out int result)) ? result : -1;
    }

    internal int AddField(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        //name = name.ToUpper();
        if (fieldNameLookup.ContainsKey(name)) throw new InvalidOperationException("Field already exists: " + name);
        int oldLen = fieldNames.Length;
        Array.Resize(ref fieldNames, oldLen + 1); // yes, this is sub-optimal, but this is not the expected common case
        fieldNames[oldLen] = name;
        fieldNameLookup[name] = oldLen;
        return oldLen;
    }

    internal bool FieldExists(string key) => key != null && fieldNameLookup.ContainsKey(key);

    public int FieldCount => fieldNames.Length;
}

internal partial class MapperRow : IDynamicMetaObjectProvider
{
    public DynamicMetaObject GetMetaObject(Expression parameter)
    {
        return new MapperRowMetaObject(parameter, BindingRestrictions.Empty, this);
    }
}

internal partial class MapperRow : IDictionary<string, object>
{
    private readonly MapperTable table;
    private object[] values;

    public MapperRow(MapperTable table, object[] values)
    {
        this.table = table ?? throw new ArgumentNullException(nameof(table));
        this.values = values ?? throw new ArgumentNullException(nameof(values));
    }

    private sealed class DeadValue
    {
        public static readonly DeadValue Default = new DeadValue();
        private DeadValue() { /* hiding constructor */ }
    }

    int ICollection<KeyValuePair<string, object>>.Count
    {
        get
        {
            int count = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (!(values[i] is DeadValue)) count++;
            }
            return count;
        }
    }

    public bool TryGetValue(string key, out object value)
        => TryGetValue(table.IndexOfName(key), out value);

    internal bool TryGetValue(int index, out object value)
    {
        if (index < 0)
        { // doesn't exist
            value = null;
            return false;
        }
        // exists, **even if** we don't have a value; consider table rows heterogeneous
        value = index < values.Length ? values[index] : null;
        if (value is DeadValue)
        { // pretend it isn't here
            value = null;
            return false;
        }
        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder().Append("{MapperRow");
        foreach (var kv in this)
        {
            var value = kv.Value;
            sb.Append(", ").Append(kv.Key);
            if (value != null)
            {
                sb.Append(" = '").Append(kv.Value).Append('\'');
            }
            else
            {
                sb.Append(" = NULL");
            }
        }

        return sb.Append('}').ToString();
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        var names = table.FieldNames;
        for (var i = 0; i < names.Length; i++)
        {
            object value = i < values.Length ? values[i] : null;
            if (!(value is DeadValue))
            {
                yield return new KeyValuePair<string, object>(names[i], value);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #region Implementation of ICollection<KeyValuePair<string,object>>

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
        IDictionary<string, object> dic = this;
        dic.Add(item.Key, item.Value);
    }

    void ICollection<KeyValuePair<string, object>>.Clear()
    { // removes values for **this row**, but doesn't change the fundamental table
        for (int i = 0; i < values.Length; i++)
            values[i] = DeadValue.Default;
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
    {
        return TryGetValue(item.Key, out object value) && Equals(value, item.Value);
    }

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        foreach (var kv in this)
        {
            array[arrayIndex++] = kv; // if they didn't leave enough space; not our fault
        }
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
        IDictionary<string, object> dic = this;
        return dic.Remove(item.Key);
    }

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;
    #endregion

    #region Implementation of IDictionary<string,object>

    bool IDictionary<string, object>.ContainsKey(string key)
    {
        int index = table.IndexOfName(key);
        if (index < 0 || index >= values.Length || values[index] is DeadValue) return false;
        return true;
    }

    void IDictionary<string, object>.Add(string key, object value)
    {
        SetValue(key, value, true);
    }

    bool IDictionary<string, object>.Remove(string key)
        => Remove(table.IndexOfName(key));

    internal bool Remove(int index)
    {
        if (index < 0 || index >= values.Length || values[index] is DeadValue) return false;
        values[index] = DeadValue.Default;
        return true;
    }

    object IDictionary<string, object>.this[string key]
    {
        get { TryGetValue(key, out object val); return val; }
        set { SetValue(key, value, false); }
    }

    public object SetValue(string key, object value)
    {
        return SetValue(key, value, false);
    }

    private object SetValue(string key, object value, bool isAdd)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        int index = table.IndexOfName(key);
        if (index < 0)
        {
            index = table.AddField(key);
        }
        else if (isAdd && index < values.Length && !(values[index] is DeadValue))
        {
            // then semantically, this value already exists
            throw new ArgumentException("An item with the same key has already been added", nameof(key));
        }
        return SetValue(index, value);
    }
    internal object SetValue(int index, object value)
    {
        int oldLength = values.Length;
        if (oldLength <= index)
        {
            // we'll assume they're doing lots of things, and
            // grow it to the full width of the table
            Array.Resize(ref values, table.FieldCount);
            for (int i = oldLength; i < values.Length; i++)
            {
                values[i] = DeadValue.Default;
            }
        }
        return values[index] = value;
    }

    ICollection<string> IDictionary<string, object>.Keys
    {
        get { return this.Select(kv => kv.Key).ToArray(); }
    }

    ICollection<object> IDictionary<string, object>.Values
    {
        get { return this.Select(kv => kv.Value).ToArray(); }
    }

    #endregion               
}

internal class MapperRowMetaObject : DynamicMetaObject
{
    private static readonly MethodInfo getValueMethod = typeof(IDictionary<string, object>).GetProperty("Item").GetGetMethod();
    private static readonly MethodInfo setValueMethod = typeof(MapperRow).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) });

    public MapperRowMetaObject(Expression expression, BindingRestrictions restrictions)
        : base(expression, restrictions)
    {
    }

    public MapperRowMetaObject(Expression expression, BindingRestrictions restrictions, object value)
        : base(expression, restrictions, value)
    {
    }

    private DynamicMetaObject CallMethod(MethodInfo method, Expression[] parameters)
    {
        var callMethod = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType),
                method,
                parameters),
            BindingRestrictions.GetTypeRestriction(Expression, LimitType)
            );
        return callMethod;
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
    {
        var parameters = new Expression[]
                             {
                                     Expression.Constant(binder.Name)
                             };

        var callMethod = CallMethod(getValueMethod, parameters);

        return callMethod;
    }

    // Needed for Visual basic dynamic support
    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
    {
        var parameters = new Expression[]
                             {
                                     Expression.Constant(binder.Name)
                             };

        var callMethod = CallMethod(getValueMethod, parameters);

        return callMethod;
    }

    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
    {
        var parameters = new Expression[]
                             {
                                     Expression.Constant(binder.Name),
                                     value.Expression,
                             };

        var callMethod = CallMethod(setValueMethod, parameters);

        return callMethod;
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        if (HasValue && Value is IDictionary<string, object> lookup) return lookup.Keys;
        // return Array.Empty<string>();
        return [];
    }
}
