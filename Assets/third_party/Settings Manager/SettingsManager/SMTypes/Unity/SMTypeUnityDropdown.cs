﻿using BattlePhaze.SettingsManager.Types;
using UnityEngine;
using UnityEngine.UI;

namespace BattlePhaze.SettingsManager
{
    public class SMTypeUnityDropdown : SettingsManagerAbstractTypeDropdown
    {
        public void SetResetAction(SettingsManager Manager, int OptionIndex)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ResetToDefault, typeof(Button)))
            {
                Button Button = (Button)Manager.Options[OptionIndex].ResetToDefault;
                if (Manager.Options[OptionIndex].ResetAction != null)
                {
                    Button.onClick.RemoveListener(Manager.Options[OptionIndex].ResetAction);
                }
                Manager.Options[OptionIndex].ResetAction = delegate { SettingsManagerStorageManagement.SetDefault(Manager, OptionIndex, true); };
                Button.onClick.AddListener(Manager.Options[OptionIndex].ResetAction);
            }
        }
        public override bool DropDownListener(SettingsManager Manager, int OptionIndex,bool IsDynamic)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(Dropdown)))
            {
                Dropdown UnityDropDown = (Dropdown)Manager.Options[OptionIndex].ObjectInput;
                DropDownClearDropDown(Manager, OptionIndex);
                SetListenerOptions(Manager, OptionIndex);
                CaptionText(UnityDropDown, Manager, OptionIndex);
                UnityDropDown.RefreshShownValue();
                if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ApplyInput, typeof(Button)))
                {
                    Button Button = (Button)Manager.Options[OptionIndex].ApplyInput;
                    if (Manager.Options[OptionIndex].ApplyAction != null)
                    {
                        Button.onClick.RemoveListener(Manager.Options[OptionIndex].ApplyAction);
                    }
                    if(IsDynamic)
                    {
                        Manager.Options[OptionIndex].ApplyAction = delegate { SettingsManagerDynamics.DynamicExecution(OptionIndex, Manager, UnityDropDown.value, true); };
                    }
                    else
                    {
                        Manager.Options[OptionIndex].ApplyAction = delegate { SettingsManagerDropDown.DropDownExecution(OptionIndex, Manager, UnityDropDown.value, true); };
                    }
                    Button.onClick.AddListener(Manager.Options[OptionIndex].ApplyAction);
                }
                else
                {
                    if (Manager.Options[OptionIndex].IntAction != null)
                    {
                        UnityDropDown.onValueChanged.RemoveListener(Manager.Options[OptionIndex].IntAction);
                    }
                    if (IsDynamic)
                    {
                        Manager.Options[OptionIndex].IntAction = delegate { SettingsManagerDynamics.DynamicExecution(OptionIndex, Manager, UnityDropDown.value, true); };
                    }
                    else
                    {
                        Manager.Options[OptionIndex].IntAction = delegate { SettingsManagerDropDown.DropDownExecution(OptionIndex, Manager, UnityDropDown.value, true); };
                    }
                    UnityDropDown.onValueChanged.AddListener(Manager.Options[OptionIndex].IntAction);
                }
                SetResetAction(Manager, OptionIndex);
                return true;
            }
            SetResetAction(Manager, OptionIndex);
            return false;
        }
        public void SetListenerOptions(SettingsManager Manager, int OptionIndex)
        {
            for (int OptionValue = 0; OptionValue < Manager.Options[OptionIndex].SelectableValueList.Count; OptionValue++)
            {
                DropDownAddOption(Manager, OptionIndex, Manager.Options[OptionIndex].SelectableValueList[OptionValue].UserValue);
                if (Manager.Options[OptionIndex].SelectedValue == Manager.Options[OptionIndex].SelectableValueList[OptionValue].RealValue)
                {
                    DropDownSetOptionsValue(Manager, OptionIndex, OptionValue, false);
                }
            }
        }
        public void CaptionText(Dropdown UnityDropDown, SettingsManager Manager, int OptionIndex)
        {
            if (UnityDropDown.captionText != null)
            {
                UnityDropDown.captionText.text = Manager.Options[OptionIndex].SelectedValue;
            }
        }
        public override void DropDownEnabledState(SettingsManager Manager, int OptionIndex, bool EnabledState)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(Dropdown)))
            {
                Dropdown UnityDropDown = (Dropdown)Manager.Options[OptionIndex].ObjectInput;
                UnityDropDown.gameObject.SetActive(EnabledState);
            }
        }
        public override void DropDownSetOptionsValue(SettingsManager Manager, int OptionIndex, int OptionsIndexValue, bool Silent)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(Dropdown)))
            {
                Dropdown UnityDropDown = (Dropdown)Manager.Options[OptionIndex].ObjectInput;
                if (Silent)
                {
                    UnityDropDown.SetValueWithoutNotify(OptionsIndexValue);
                }
                else
                {
                    UnityDropDown.value = OptionsIndexValue;
                    UnityDropDown.RefreshShownValue();
                }
            }
        }
        public override bool DropDownGetOptionsGameobject(SettingsManager Manager, int OptionIndex, out GameObject GameObject)
        {
            GameObject = null;
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(Dropdown)))
            {
                Dropdown UnityDropDown = (Dropdown)Manager.Options[OptionIndex].ObjectInput;
                GameObject = UnityDropDown.gameObject;
                return true;
            }
            return false;
        }
        public override void DropDownClearDropDown(SettingsManager Manager, int OptionIndex)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(Dropdown)))
            {
                Dropdown UnityDropDown = (Dropdown)Manager.Options[OptionIndex].ObjectInput;
                UnityDropDown.options.Clear();
            }
        }
        public override void DropDownAddOption(SettingsManager Manager, int OptionIndex, string Option)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(Dropdown)))
            {
                Dropdown UnityDropDown = (Dropdown)Manager.Options[OptionIndex].ObjectInput;
                UnityDropDown.options.Add(new Dropdown.OptionData(Option));
            }
        }
        public override SettingsManagerEnums.IsTypeInterpreter GetActiveType()
        {
            return SettingsManagerEnums.IsTypeInterpreter.DropDown;
        }
    }
}