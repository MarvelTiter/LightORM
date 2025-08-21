namespace LightORM.SqlExecutor;

internal partial class ExpressionBuilder
{
    public static string CustomStringToBoolean(string valueString)
    {
        return ",是,1,Y,YES,TRUE,".Contains(valueString.ToUpper()) ? "True" : "False";
    }
    public static byte[] RecordFieldToBytes(IDataRecord Reader, int Column)
    {
        long blobSize = Reader.GetBytes(Column, 0, null, 0, 0);
        if (blobSize > int.MaxValue)
            throw new ArgumentOutOfRangeException("MemoryStream cannot be larger than " + int.MaxValue);
        // 处理空数据
        if (blobSize == 0)
            return [];
        byte[] Buffer = new byte[blobSize];
        Reader.GetBytes(Column, 0, Buffer, 0, Buffer.Length);
        return Buffer;
    }

    public static uint RecordFieldToUInt32(IDataRecord Reader, int Column)
    {
        var value = Reader.GetInt32(Column);
        return value >= 0 ? (uint)value : throw new OverflowException("Negative value cannot be converted to uint");
    }

    public static ushort RecordFieldToUInt16(IDataRecord Reader, int Column)
    {
        var value = Reader.GetInt16(Column);
        return value >= 0 ? (ushort)value : throw new OverflowException("Negative value cannot be converted to ushort");
    }

    public static ulong RecordFieldToUInt64(IDataRecord Reader, int Column)
    {
        var value = Reader.GetInt64(Column);
        return value >= 0 ? (ulong)value : throw new OverflowException("Negative value cannot be converted to ulong");
    }
}
