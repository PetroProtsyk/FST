
namespace Protsyk.PMS.FST
{
    /// <summary>
    /// Match all elements
    /// </summary>
    public class AnyMatcher<T> : IDfaMatcher<T>
    {
        public void Reset()
        {
        }

        public bool Next(T p)
        {
            return true;
        }

        public bool IsFinal()
        {
            return true;
        }

        public void Pop()
        {
        }
    }
}