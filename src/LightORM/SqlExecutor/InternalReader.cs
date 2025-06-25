using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace LightORM.SqlExecutor;

internal partial class SqlExecutor
{
    private class InternalDataReader : DbDataReader
    {
        private readonly DbDataReader dataReader;
        private readonly CommandResult commandResult;
        private readonly ConnectionPool pool;

        public InternalDataReader(DbDataReader dataReader
            , CommandResult commandResult
            , ConnectionPool pool)
        {
            this.dataReader = dataReader;
            this.commandResult = commandResult;
            this.pool = pool;
        }

        public override void Close()
        {
            dataReader.Close();
            commandResult.Command.Parameters.Clear();
            commandResult.Command.Dispose();
            pool.Return(commandResult.Connection);
        }
#if NET6_0_OR_GREATER
        public override async Task CloseAsync()
        {
            await dataReader.CloseAsync();
            commandResult.Command.Parameters.Clear();
            commandResult.Command.Dispose();
            pool.Return(commandResult.Connection);
        }
#endif
        public override object this[int ordinal] => dataReader[ordinal];

        public override object this[string name] => dataReader[name];

        public override int Depth => dataReader.Depth;

        public override int FieldCount => dataReader.FieldCount;

        public override bool HasRows => dataReader.HasRows;

        public override bool IsClosed => dataReader.IsClosed;

        public override int RecordsAffected => dataReader.RecordsAffected;

        public override bool GetBoolean(int ordinal) => dataReader.GetBoolean(ordinal);

        public override byte GetByte(int ordinal) => dataReader.GetByte(ordinal);

        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
        => dataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

        public override char GetChar(int ordinal) => dataReader.GetChar(ordinal);

        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
        => dataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

        public override string GetDataTypeName(int ordinal) => dataReader.GetDataTypeName(ordinal);

        public override DateTime GetDateTime(int ordinal) => dataReader.GetDateTime(ordinal);

        public override decimal GetDecimal(int ordinal) => dataReader.GetDecimal(ordinal);

        public override double GetDouble(int ordinal) => dataReader.GetDouble(ordinal);

        public override IEnumerator GetEnumerator() => dataReader.GetEnumerator();

        public override Type GetFieldType(int ordinal) => dataReader.GetFieldType(ordinal);

        public override float GetFloat(int ordinal) => dataReader.GetFloat(ordinal);

        public override Guid GetGuid(int ordinal) => dataReader.GetGuid(ordinal);

        public override short GetInt16(int ordinal) => dataReader.GetInt16(ordinal);

        public override int GetInt32(int ordinal) => dataReader.GetInt32(ordinal);

        public override long GetInt64(int ordinal) => dataReader.GetInt64(ordinal);

        public override string GetName(int ordinal) => dataReader.GetName(ordinal);

        public override int GetOrdinal(string name) => dataReader.GetOrdinal(name);

        public override string GetString(int ordinal) => dataReader.GetString(ordinal);

        public override object GetValue(int ordinal) => dataReader.GetValue(ordinal);

        public override int GetValues(object[] values) => dataReader.GetValues(values);

        public override bool IsDBNull(int ordinal) => dataReader.IsDBNull(ordinal);

        public override bool NextResult() => dataReader.NextResult();

        public override bool Read() => dataReader.Read();

        public override DataTable? GetSchemaTable() => dataReader.GetSchemaTable();

        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            return base.GetDbDataReader(ordinal);
        }

#if NET6_0_OR_GREATER
        public override Task<DataTable?> GetSchemaTableAsync(CancellationToken cancellationToken = default)
            => dataReader.GetSchemaTableAsync(cancellationToken);
#endif


    }
}

internal class EmptyDataReader : DbDataReader
{
    public override object this[int ordinal] => throw new NotImplementedException();

    public override object this[string name] => throw new NotImplementedException();

    public override int Depth => 0;

    public override int FieldCount => 0;

    public override bool HasRows => false;

    public override bool IsClosed => false;

    public override int RecordsAffected => 0;

    public override bool GetBoolean(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override byte GetByte(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override decimal GetDecimal(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override double GetDouble(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public override Type GetFieldType(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override float GetFloat(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override Guid GetGuid(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override short GetInt16(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetInt32(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetInt64(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override string GetName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetOrdinal(string name)
    {
        throw new NotImplementedException();
    }

    public override string GetString(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override object GetValue(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public override bool IsDBNull(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override bool NextResult() => false;

    public override bool Read() => false;
    public override DataTable? GetSchemaTable() => new();
}