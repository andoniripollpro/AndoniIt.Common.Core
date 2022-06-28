namespace AndoIt.Common.Interface
{
    public interface IMapper <T1, T2>
    {
        T2 Map(T1 toConvert);
        T1 InverseMap(T2 toConvert);
    }
}
