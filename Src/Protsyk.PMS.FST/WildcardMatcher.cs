using System;

namespace Protsyk.PMS.FST
{
    public class WildcardMatcher : AutomataMatcher
    {
        public WildcardMatcher(string pattern, int maxLength)
            : base(maxLength, () => CreateDFA(pattern))
        {
        }

        private static DFA CreateDFA(string pattern)
        {
            return AutomatonWildcard.CreateAutomaton(pattern).Determinize();
        }
    }
}