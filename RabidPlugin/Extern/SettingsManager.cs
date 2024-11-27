using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using FFXIVClientStructs.FFXIV.Common.Lua;
using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace RabidPlugin
{
    using ConfigID = uint;
    public unsafe class SettingsManager
    {
        public readonly static string[] ConfigIDToName = ["Base", "UI", "UI Control", "UI Gamepad"];
        #region Types

        public unsafe class ConfigSettingSource(uint entryIndex, ConfigValue val, ConfigType type)
        {
            public uint EntryIndex { get; private set; } = entryIndex;
            public ConfigValue Value { get; private set; } = val;
            public ConfigType ValueType { get; private set; } = type;

            public void SetValue(ConfigValue newVal)
            {
                Value = newVal;
            }

            public void DrawEdit(string label)
            {
                switch (ValueType)
                {
                    case ConfigType.UInt:
                        {
                            int curVal = (int)Value.UInt;
                            if (ImGui.InputInt(label, ref curVal))
                            {
                                ConfigValue newVal = default;
                                newVal.UInt = (uint)curVal;
                                Value = newVal;
                            }
                            break;
                        }
                    case ConfigType.Float:
                        {
                            float curFloat = Value.Float;
                            if(ImGui.InputFloat(label, ref curFloat))
                            {
                                ConfigValue newVal = default;
                                newVal.Float = curFloat;
                                Value = newVal;
                            }
                            break;
                        }
                    case ConfigType.String:
                        {
                            string current = Helpers.ConvertBytePointerToString(Value.String->StringPtr);
                            ImGui.Text($"{label}: {current}");
                            break;
                        }
                    case ConfigType.Category:
                        {
                            ImGui.Text("Category doesn't have a value");
                            break;
                        }
                }
            }
        }
        public unsafe class ConfigSetting : DictionaryWrapper<ConfigID, ConfigSettingSource>
        {
            public void SetValue(ConfigValue newValue)
            {
                foreach(var cfgs in Values)
                {
                    cfgs.SetValue(newValue);
                }
            }

            public void ApplyToGame(ConfigBase*[] configBase)
            {
                foreach(KeyValuePair<ConfigID, ConfigSettingSource> fullSetting in this)
                {
                    ConfigEntry* target = &configBase[fullSetting.Key]->ConfigEntry[fullSetting.Value.EntryIndex];
                    switch((ConfigType)target->Type)
                    {
                        case ConfigType.UInt: target->SetValueUInt(fullSetting.Value.Value.UInt); break;
                        case ConfigType.Float: target->SetValueFloat(fullSetting.Value.Value.Float); break;
                        case ConfigType.String: target->SetValueString(fullSetting.Value.Value.String); break;
                    }
                }

            }

            public void Draw()
            {
                foreach(var fullSetting in this)
                {
                    ImGui.Text($"ID: {fullSetting.Key}");
                    ImGui.SameLine();
                    fullSetting.Value.DrawEdit($"##{fullSetting.Key}_{fullSetting.Value.EntryIndex}");
                }
            }
        }
        public unsafe class NamedSettings : DictionaryWrapper<string, ConfigSetting> { }
        public unsafe class SettingsProfiles : DictionaryWrapper<string, NamedSettings> 
        {
            public void Save(SettingsProfiles targetProfile)
            {
                targetProfile.Clear();
                foreach (var fullsetting in this)
                {
                    targetProfile.Add(fullsetting.Key, fullsetting.Value);
                }
            }

            public void Load(SettingsProfiles targetProfile)
            {
                this.Clear();
                foreach(var fullsetting in targetProfile)
                {
                    this.Add(fullsetting.Key, fullsetting.Value);
                }
            }
        }
        #endregion

        public SettingsManager(Configuration configuration)
        {
            m_Configuration = configuration;

            m_ConfigBase[0] = &(m_FrameworkInstance->SystemConfig.SystemConfigBase.ConfigBase);
            m_ConfigBase[1] = &(m_FrameworkInstance->SystemConfig.SystemConfigBase.UiConfig);
            m_ConfigBase[2] = &(m_FrameworkInstance->SystemConfig.SystemConfigBase.UiControlConfig);
            m_ConfigBase[3] = &(m_FrameworkInstance->SystemConfig.SystemConfigBase.UiControlGamepadConfig);

            ClearList();
            m_HotProfiles.Load(m_Configuration.SettingsProfiles);
        }

        public void DrawEditor()
        {
            if(ImGui.BeginChild("Settings Manager"))
            {
                DrawHeader();
                ImGui.Separator();
                DrawTableEditorView();

                ImGui.EndChild();
            }
        }

        private void DrawHeader()
        {
            bool canCreate = !string.IsNullOrWhiteSpace(m_NewProfileName) && !m_HotProfiles.ContainsKey(m_NewProfileName);
            ImGui.BeginDisabled(!canCreate);
            if (ImGui.Button("Create Profile"))
            {
                m_HotProfiles.Add(m_NewProfileName, new NamedSettings());
                m_NewProfileName = "";
            }
            if (!canCreate && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                string reason = "";
                if (string.IsNullOrWhiteSpace(m_NewProfileName))
                {
                    reason = "Enter a name";
                }
                else if (m_HotProfiles.ContainsKey(m_NewProfileName))
                {
                    reason = $"Profile ({m_NewProfileName}) already exists";
                }
                ImGui.BeginTooltip();
                ImGui.Text(reason);
                ImGui.EndTooltip();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.InputTextWithHint("##newProfile", "profile name", ref m_NewProfileName, 20);

            bool disableSave = !ImGui.IsKeyDown(ImGuiKey.LeftShift) || !ImGui.IsKeyDown(ImGuiKey.LeftCtrl);
            ImGui.BeginDisabled(disableSave);
            if (ImGui.Button("Save all"))
            {
                foreach (var profile in m_ProfilesBeingRemoved)
                {
                    m_HotProfiles.Remove(profile);
                }

                m_HotProfiles.Save(m_Configuration.SettingsProfiles);
                m_HotProfiles.Load(m_Configuration.SettingsProfiles);
                m_Configuration.Save();
            }
            if (disableSave && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.BeginTooltip();
                ImGui.Text("Hold LShift+CTRL to unlock Save All");
                ImGui.EndTooltip();
            }
            ImGui.EndDisabled();
        }

        private void DrawTableEditorView()
        {
            const ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersOuter |
                                    ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY | /*ImGuiTableFlags.Sortable | ImGuiTableFlags.SortMulti |*/
                                    ImGuiTableFlags.Hideable;

            if (ImGui.BeginTable("Settings Manager##table", 2, flags))
            {
                ImGui.TableSetupColumn("Profiles", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Editor");
                ImGui.TableHeadersRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.TableNextRow();

                if (ImGui.BeginChild("Profiles"))
                {
                    foreach (var profileName in m_HotProfiles.Keys)
                    {
                        bool beingRemoved = m_ProfilesBeingRemoved.Contains(profileName);
                        string profileChangeMarker = m_Configuration.SettingsProfiles.ContainsKey(profileName) ? "" : "*";
                        profileChangeMarker = beingRemoved ? "- " : profileChangeMarker;

                        bool isSelected = profileName == m_SelectedProfile;
                        if (ImGui.Selectable($"{profileChangeMarker} {profileName}", isSelected))
                        {
                            m_SelectedProfile = profileName;

                        }
                    }
                    ImGui.EndChild();
                }


                ImGui.TableSetColumnIndex(1);

                if (m_HotProfiles.TryGetValue(m_SelectedProfile, out NamedSettings? val))
                {
                    bool beingRemoved = m_ProfilesBeingRemoved.Contains(m_SelectedProfile);
                    string isNewProfile = m_Configuration.SettingsProfiles.ContainsKey(m_SelectedProfile) ? "" : "*";
                    isNewProfile = beingRemoved ? "- " : isNewProfile;

                    if (beingRemoved)
                    {
                        if (ImGui.Button("Restore"))
                        {
                            m_ProfilesBeingRemoved.Remove(m_SelectedProfile);
                        }
                    }
                    else
                    {
                        if (ImGui.Button("Delete"))
                        {
                            m_ProfilesBeingRemoved.Add(m_SelectedProfile);
                        }
                    }

                    foreach (var setting in val)
                    {
                        Helpers.CollapsingTreeNode(setting.Key, new Helpers.TreeFrame(), (col) =>
                        {
                            setting.Value.Draw();
                        });
                    }
                }

                //foreach (var profile in m_HotProfiles)
                //{

                //}
                ImGui.EndTable();
            }
        }

        public void Dispose()
        {
            ClearList();
        }

        public void ClearList()
        {
            m_MappedSettings.Clear();
            m_SavedSettings.Clear();
        }

        private bool DrawEditFieldForItem(uint cfgID, uint idx)
        {
            return DrawEditConfigValue(&m_ConfigBase[cfgID]->ConfigEntry[idx]);
        }

        private bool DrawEditConfigValue(ConfigEntry* entry, ConfigValue* valueToEdit = null, bool applyToGame = true)
        {
            string? name = Marshal.PtrToStringUTF8((nint)entry->Name);
            if (name == null)
            {
                return false;
            }

            name = name == null ? "NULL" : name;
            string label = $"{name}##SettingsManager_{(int)entry}";

            if (valueToEdit == null)
            {
                valueToEdit = &entry->Value;
            }

            bool applyChangesToGame = valueToEdit == &entry->Value && applyToGame;

            switch ((ConfigType)entry->Type)
            {
                case ConfigType.UInt:
                    {
                        int curVal = (int)valueToEdit->UInt;
                        if(ImGui.InputInt(label, ref curVal))
                        {
                            if (applyChangesToGame)
                            {
                                entry->SetValueUInt((uint)curVal);
                            }
                            else
                            {
                                valueToEdit->UInt = (uint)curVal;
                            }
                            return true;
                        }
                        break;
                    }
                case ConfigType.Float:
                    {
                        float current = valueToEdit->Float;
                        if (ImGui.InputFloat(label, ref current))
                        {
                            if (applyChangesToGame)
                            {
                                entry->SetValueFloat(current);
                            }
                            else
                            {
                                valueToEdit->Float = current;
                            }
                            return true;
                        }
                        break;
                    }
                case ConfigType.String:
                    {
                        string current = Helpers.ConvertBytePointerToString(valueToEdit->String->StringPtr);
                        if(ImGui.InputText(label, ref current, (uint)current.Length))
                        {
                            if (applyChangesToGame)
                            {
                                entry->SetValueString(current);
                            }
                            else
                            {
                                valueToEdit->String->SetString(current);
                            }
                            return true;
                        }
                        break;
                    }
                case ConfigType.Category:
                    {
                        ImGui.Text("Category doesn't have a value");
                        break;
                    }
            }
            return false;
        }

        public void MapSettings()
        {
            m_MappedSettings.Clear();
            for (uint cfgId = 0; cfgId < m_ConfigBase.Length; cfgId++)
            {
                for (uint i = 0; i < m_ConfigBase[cfgId]->ConfigCount; i++)
                {
                    if (m_ConfigBase[cfgId]->ConfigEntry[i].Type == 0)
                        continue;

                    string name = MemoryHelper.ReadStringNullTerminated(new IntPtr(m_ConfigBase[cfgId]->ConfigEntry[i].Name));
                    if (!m_MappedSettings.ContainsKey(name))
                    {
                        m_MappedSettings[name] = new List<Tuple<uint, uint>>();
                    }

                    m_MappedSettings[name].Add(new Tuple<uint, uint>(cfgId, m_ConfigBase[cfgId]->ConfigEntry[i].Index));
                }
            }

            m_HotProfiles.Load(m_Configuration.SettingsProfiles);
        }

        public void SetSettingsValueInt(string setting, uint value)
        {
            List<Tuple<uint, uint>> list = m_MappedSettings.GetValueOrDefault(setting, new List<Tuple<uint, uint>>());
            foreach (Tuple<uint, uint> item in list)
            {
                m_ConfigBase[item.Item1]->ConfigEntry[item.Item2].SetValueUInt(value);
            }
        }

        public uint GetSettingsValueInt(string setting, int index)
        {
            List<Tuple<uint, uint>> list = m_MappedSettings.GetValueOrDefault(setting, new List<Tuple<uint, uint>>());
            if (index >= list.Count)
            {
                return 0;
            }
            return m_ConfigBase[list[index].Item1]->ConfigEntry[list[index].Item2].Value.UInt;
        }


        public void DebugSettings(bool printAll = false)
        {
            Save(false);

            foreach (var settingByName in m_SavedSettings)
            {
                foreach (var cfgEntry in settingByName.Value)
                {
                    string name = settingByName.Key;
                    uint cfgId = cfgEntry.Key;
                    uint i = cfgEntry.Value.EntryIndex;

                    if (printAll)
                    {
                        string value = GetValueStrFromConfig(&m_ConfigBase[cfgId]->ConfigEntry[i]);
                        string message = $"Location:   {ConfigIDToName[cfgId]} | {i} name: {name} value: {value}";
                        RabidPlugin.Log!.Info(message);
                    }
                }
            }
        }

        public void Save(bool compare = false)
        {
            if (!compare)
            {
                m_SavedSettings.Clear();
                for (uint cfgId = 0; cfgId < m_ConfigBase.Length; cfgId++)
                {
                    for (uint i = 0; i < m_ConfigBase[cfgId]->ConfigCount; i++)
                    {
                        if (m_ConfigBase[cfgId]->ConfigEntry[i].Type == 0)
                            continue;

                        string name = MemoryHelper.ReadStringNullTerminated(new IntPtr(m_ConfigBase[cfgId]->ConfigEntry[i].Name));
                        if (!m_SavedSettings.ContainsKey(name))
                            m_SavedSettings[name] = new ConfigSetting();
                        ConfigEntry* currentEntry = &m_ConfigBase[cfgId]->ConfigEntry[i];
                        m_SavedSettings[name][cfgId] = new ConfigSettingSource(i, currentEntry->Value, (ConfigType)currentEntry->Type);
                    }
                }
                RabidPlugin.Log!.Info($"--- Current Settings Saved ---");
            }
            else
            {
                RabidPlugin.Log!.Info($"--- Changed Settings ---");
                foreach (var itemByName in m_SavedSettings)
                    foreach (var cfgEntry in itemByName.Value)
                        if(cfgEntry.Value.Value.UInt != m_ConfigBase[cfgEntry.Key]->ConfigEntry[cfgEntry.Value.EntryIndex].Value.UInt)
                            RabidPlugin.Log!.Info($"{cfgEntry.Key} | {cfgEntry.Value.EntryIndex} -- {itemByName.Key} -- {cfgEntry.Value.Value.UInt} {cfgEntry.Value.Value.Float} | {m_ConfigBase[cfgEntry.Key]->ConfigEntry[cfgEntry.Value.EntryIndex].Value.UInt} {m_ConfigBase[cfgEntry.Key]->ConfigEntry[cfgEntry.Value.EntryIndex].Value.Float}");
            }
        }

        private string GetValueStrFromConfig(ConfigEntry* entry)
        {
            switch ((ConfigType)entry->Type)
            {
                case ConfigType.String:
                    {
                        return Helpers.ConvertBytePointerToString(entry->Value.String->StringPtr);
                    }
                case ConfigType.UInt:
                    {
                        return entry->Value.UInt.ToString();
                    }
                case ConfigType.Float:
                {
                    return entry->Value.Float.ToString();
                }
                case ConfigType.Category:
                    {
                        return $"_cat_ {entry->Value.UInt.ToString()}";
                    }
            }

            return "NULL DEFAULT";
        }


        private Framework* m_FrameworkInstance = Framework.Instance();
        private ConfigBase*[] m_ConfigBase = new ConfigBase*[4];
        private Dictionary<string, List<Tuple<uint, uint>>> m_MappedSettings = new Dictionary<string, List<Tuple<uint, uint>>>(); // cfgID, index

        private NamedSettings m_SavedSettings = new NamedSettings();
        private SettingsProfiles m_HotProfiles = new SettingsProfiles();
        private HashSet<string> m_ProfilesBeingRemoved = new HashSet<string>();
        private Configuration m_Configuration;
        private string m_NewProfileName = "";
        private string m_SelectedProfile = "";
    }
}
