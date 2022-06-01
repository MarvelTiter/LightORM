namespace MDbContext.NewExpSql.Interface.Select
{
    public class TypeSet<T1, T2>
    {
        public TypeSet(T1 t1, T2 t2)
        {
            this.Tb1 = t1;
            this.Tb2 = t2;
        }

        public T1 Tb1 { get; }
        public T2 Tb2 { get; }
    }
    public class TypeSet<T1, T2, T3>
    {
        public TypeSet(T1 t1, T2 t2, T3 t3)
        {
            this.Tb1 = t1;
            this.Tb2 = t2;
            this.Tb3 = t3;
        }

        public T1 Tb1 { get; }
        public T2 Tb2 { get; }
        public T3 Tb3 { get; }
    }
    public class TypeSet<T1, T2, T3, T4>
    {
        public TypeSet(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            this.Tb1 = t1;
            this.Tb2 = t2;
            this.Tb3 = t3;
            this.Tb4 = t4;
        }

        public T1 Tb1 { get; }
        public T2 Tb2 { get; }
        public T3 Tb3 { get; }
        public T4 Tb4 { get; }
    }
    public class TypeSet<T1, T2, T3, T4, T5>
    {
        public TypeSet(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            this.Tb1 = t1;
            this.Tb2 = t2;
            this.Tb3 = t3;
            this.Tb4 = t4;
            this.Tb5 = t5;
        }

        public T1 Tb1 { get; }
        public T2 Tb2 { get; }
        public T3 Tb3 { get; }
        public T4 Tb4 { get; }
        public T5 Tb5 { get; }
    }
}
