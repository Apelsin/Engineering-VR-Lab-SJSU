using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class TensionCompressionQuiz : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct TCObjects : IEnumerable<GameObject>
        {
            public GameObject Reference;
            public GameObject Metal;
            public GameObject Ceramic;
            public GameObject Polymer;

            public IEnumerator<GameObject> GetEnumerator()
            {
                yield return Reference;
                yield return Metal;
                yield return Ceramic;
                yield return Polymer;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield return Reference;
                yield return Metal;
                yield return Ceramic;
                yield return Polymer;
            }
        }

        public TCObjects TensionObjects;
        public TCObjects CompressionObjects;

        private void HandleMenuAddedCallback(object sender, PointerMenuManager.PointerMenuEventArgs args)
        {
            var pointer_menu = (ManagedPointerMenu)sender;
            var buttons = args.Menu.Buttons;

            // Record initial color values
            var normal_colors = args.Menu.Buttons.First().colors;
            var checked_colors = normal_colors;
            checked_colors.normalColor = args.Menu.CheckedColor;
            checked_colors.highlightedColor = args.Menu.CheckedColor;

            // Use a current buttons array
            var current_infos = pointer_menu.Buttons;

            // Set up button click handler
            args.Menu.ButtonClick += (sender2, args2) =>
            {
                // TODO: replace with some pretty Linq statement (it's harder b/c struct type)
                // Read from current buttons array
                foreach (var info in current_infos)
                {
                    if (info.Id == args2.Info.Id)
                    {
                        // TODO: separate color and checked logic (decouple)
                        // Clear colors
                        foreach (var button in buttons)
                            button.colors = normal_colors;
                        // Clear logic
                        pointer_menu.ClearCheckedButtons();
                        // Set colors
                        args2.Button.colors = checked_colors;
                        // Set logic
                        pointer_menu.SetButtonChecked(args2.Info.Id, true);
                        break;
                    }
                }
            };

            // Navigation handler
            args.Menu.Navigate += (sender2, args2) =>
            {
                // Replace current buttons array
                current_infos = args2.Parent.Children;
                CheckButtons(buttons, current_infos, ref normal_colors, ref checked_colors);
            };

            CheckButtons(buttons, current_infos, ref normal_colors, ref checked_colors);
        }

        private void Start()
        {
            // Here's where the magic happens
            var objects = TensionObjects.Zip(CompressionObjects, (t, c) => new { t, c });
            // For all of our objects
            foreach (var o in objects)
            {
                // Add button logic to all of the menus
                // "Check" the last-pressed button and record the check
                var tension_pointer_menu = o.t.GetComponent<ManagedPointerMenu>();
                var compression_pointer_menu = o.c.GetComponent<ManagedPointerMenu>();
                tension_pointer_menu.MenuAddedCallback += HandleMenuAddedCallback;
                compression_pointer_menu.MenuAddedCallback += HandleMenuAddedCallback;
                // Copy the tension menu's instantiated template to the compression menu
                compression_pointer_menu.SharedTemplate = tension_pointer_menu.Template;
            }
        }

        private static void CheckButtons(
            Button[] buttons,
            ButtonInfo[] infos,
            ref ColorBlock normal_colors,
            ref ColorBlock checked_colors)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                // Set Button component colors based on navigation event args and parent button info
                var button = buttons[i];
                var info = infos[i];
                button.colors = info.Checked ? checked_colors : normal_colors;
            }
        }

        private void Update()
        {
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }
    }
}