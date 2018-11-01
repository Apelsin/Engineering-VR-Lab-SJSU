using UnityEngine.Events;

namespace CVRLabSJSU
{
    interface IMCQuizController
    {
        event UnityAction<object, MCQuizResultsEventArgs> DisplayResults;
    }
}
