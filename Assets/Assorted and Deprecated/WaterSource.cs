using UnityEngine;

namespace CVRLabSJSU
{
    public class WaterSource : SolutionSource
    {
        // Use this for initialization
        private void Start()
        {
            Liquid water = ScriptableObject.CreateInstance<Liquid>(); //5, Color.white, LiquidType.Water);
            water.init(1, Color.white, LiquidType.Water);

            Solution solution = ScriptableObject.CreateInstance<Solution>();
            solution.addToSolution(water);

            solutionToAdd = solution;
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}