using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVRLabSJSU
{
    public static class Utility
    {
        public static void DoBatchOperation<T>(ref int index, int batch_size, T[] elements, Action<int, int, T> operation)
        {
            var number_of_elements = elements.Length;
            if (number_of_elements == 0)
                return;
            index = index % number_of_elements;
            batch_size = Math.Min(batch_size, number_of_elements);
            for (int i = 0; i < batch_size; i++)
            {
                operation(index, batch_size, elements[index]);
                index = (index + 1) % number_of_elements;
            }
        }

        public static void DoBatchOperation<T>(ref int index, int batch_size, List<T> elements, Action<int, int, T> operation)
        {
            var number_of_elements = elements.Count;
            if (number_of_elements == 0)
                return;
            index = index % number_of_elements;
            batch_size = Math.Min(batch_size, number_of_elements);
            for (int i = 0; i < batch_size; i++)
            {
                operation(index, batch_size, elements[index]);
                index = (index + 1) % number_of_elements;
            }
        }

        public static void DoBatchOperation<T>(ref int index, int batch_size, T[] elements, Action<T> operation)
        {
            var number_of_elements = elements.Length;
            if (number_of_elements == 0)
                return;
            index = index % number_of_elements;
            batch_size = Math.Min(batch_size, number_of_elements);
            for (int i = 0; i < batch_size; i++)
            {
                operation(elements[index]);
                index = (index + 1) % number_of_elements;
            }
        }

        public static void DoBatchOperation<T>(ref int index, int batch_size, List<T> elements, Action<T> operation)
        {
            var number_of_elements = elements.Count;
            if (number_of_elements == 0)
                return;
            index = index % number_of_elements;
            batch_size = Math.Min(batch_size, number_of_elements);
            for (int i = 0; i < batch_size; i++)
            {
                operation(elements[index]);
                index = (index + 1) % number_of_elements;
            }
        }
    }
}
