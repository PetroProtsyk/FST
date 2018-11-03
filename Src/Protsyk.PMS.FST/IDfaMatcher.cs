
namespace Protsyk.PMS.FST
{
    public interface IDfaMatcher<in T>
    {
        void Reset();

        bool Next(T p);

        bool IsFinal();

        void Pop();
    }
}