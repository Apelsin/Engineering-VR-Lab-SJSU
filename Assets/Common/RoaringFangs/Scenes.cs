/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoaringFangs
{
    public static class Scenes
    {
        public struct Options
        {
            public string Name;
            public bool ReplaceExisting;
        }
        public static IEnumerable LoadTogether(IEnumerable<Options> scene_options, bool skip_if_already_loaded)
        {
            var operations_remaining = new List<AsyncOperation>();
            // Asynchronously load the scenes
            foreach (var options in scene_options)
            {
                var scene = SceneManager.GetSceneByName(options.Name);
                if (scene.isLoaded)
                {
                    if (options.ReplaceExisting)
                    {
                        var unload = SceneManager.UnloadSceneAsync(options.Name);
                        operations_remaining.Add(unload);
                    }
                    else if (skip_if_already_loaded)
                        continue;
                }
                Debug.Log("Async loading scene \"" + options.Name + "\"");
                var load = SceneManager.LoadSceneAsync(options.Name, LoadSceneMode.Additive);
                operations_remaining.Add(load);
            }
            // Wait for all of the scenes in the list to be loaded
            while (operations_remaining.Count > 0)
            {
                if (operations_remaining[0].isDone)
                    operations_remaining.RemoveAt(0);
                else
                    yield return new WaitForEndOfFrame();
            }
        }

        public static IEnumerable UnloadTogether(IEnumerable<Options> scene_options)
        {
            var operations_remaining = new List<AsyncOperation>();
            // Asynchronously unload the scenes
            foreach (var options in scene_options)
            {
                Debug.Log("Async unloading scene \"" + options.Name + "\"");
                var unload = SceneManager.UnloadSceneAsync(options.Name);
                operations_remaining.Add(unload);
            }
            // Wait for all of the scenes in the list to be unloaded
            while (operations_remaining.Count > 0)
            {
                if (operations_remaining[0].isDone)
                    operations_remaining.RemoveAt(0);
                else
                    yield return new WaitForEndOfFrame();
            }
        }
    }
}