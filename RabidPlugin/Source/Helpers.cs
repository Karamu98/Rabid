using Dalamud.Interface.ImGuiFileDialog;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using FFXIVClientStructs.FFXIV.Common.Lua;
using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabidPlugin
{
    class Helpers
    {
        public class TreeFrame
        {
            public bool ShouldCollapse = false;
            public bool ShouldExpand = false;
        }

        public static void CollapsingTreeNode(string label, TreeFrame frameData, Action<TreeFrame>? action = null)
        {
            if (frameData.ShouldCollapse)
            {
                ImGui.SetNextItemOpen(false);
            }
            else if (frameData.ShouldExpand)
            {
                ImGui.SetNextItemOpen(true);
            }

            bool tree = ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.NoAutoOpenOnLog);
            if (ImGui.IsKeyDown(ImGuiKey.LeftAlt) && ImGui.IsItemHovered())
            {
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                {
                    frameData.ShouldCollapse = true;
                }
                else if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    frameData.ShouldExpand = true;
                }
            }

            if (tree)
            {
                action?.Invoke(frameData);
                ImGui.TreePop();
            }
        }

        public unsafe static string ConvertBytePointerToString(byte* bytePointer)
        {
            int length = 0;
            while (bytePointer[length] != 0) { length++; }
            return Encoding.UTF8.GetString(bytePointer, length);
        }

        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>(Dictionary<TKey, TValue> original) where TValue : ICloneable where TKey : notnull
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                                                                    original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }
    }

    public class DictionaryWrapper<T, V> : Dictionary<T, V> where T : notnull { }


}
