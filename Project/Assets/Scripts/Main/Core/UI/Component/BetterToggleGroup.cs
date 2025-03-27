using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

namespace EdgeStudio.UI.Component
{
    public class BetterToggleGroup : UIBehaviour
    {
        [SerializeField] private int m_MinSelectedCount = 1;
        [SerializeField] private int m_MaxSelectedCount = 1;
        
        public int minSelectedCount
        {
            get => m_MinSelectedCount;
            private set => m_MinSelectedCount = Mathf.Max(0, value);
        }
        
        public int maxSelectedCount
        {
            get => m_MaxSelectedCount;
            private set => m_MaxSelectedCount = Mathf.Max(minSelectedCount, value);
        }

        protected List<BetterToggle> m_Toggles = new();
        private readonly HashSet<BetterToggle> m_RequiredToggles = new();
        private readonly List<BetterToggle> m_SelectedOrder = new();

        protected override void Start()
        {
            EnsureValidState();
            base.Start();
            InitializeToggles();
        }

        protected override void OnEnable()
        {
            EnsureValidState();
            base.OnEnable();
        }

        private void InitializeToggles()
        {
            var toggles = m_Toggles.ToList();
            int currentSelected = toggles.Count(t => t.isOn);

            foreach (var toggle in m_RequiredToggles)
            {
                if (!toggle.isOn)
                {
                    toggle.SetIsOnWithoutNotify(true);
                    if (!m_SelectedOrder.Contains(toggle))
                    {
                        m_SelectedOrder.Add(toggle);
                    }

                    currentSelected++;
                }
            }

            if (currentSelected < minSelectedCount)
            {
                for (int i = 0; i < toggles.Count && currentSelected < minSelectedCount; i++)
                {
                    var toggle = toggles[i];
                    if (!toggle.isOn && !m_RequiredToggles.Contains(toggle))
                    {
                        toggle.SetIsOnWithoutNotify(true);
                        if (!m_SelectedOrder.Contains(toggle))
                        {
                            m_SelectedOrder.Add(toggle);
                        }

                        currentSelected++;
                    }
                }
            }
            else
            {
                foreach (var toggle in toggles.Where(t => t.isOn))
                {
                    if (!m_SelectedOrder.Contains(toggle))
                    {
                        m_SelectedOrder.Add(toggle);
                    }
                }
            }
        }

        public void RegisterToggle(BetterToggle toggle)
        {
            if (!toggle || m_Toggles.Contains(toggle))
                return;

            m_Toggles.Add(toggle);

            if (toggle.isOn)
            {
                if (!m_SelectedOrder.Contains(toggle))
                {
                    m_SelectedOrder.Add(toggle);
                }
            }
        }

        public void UnregisterToggle(BetterToggle toggle)
        {
            if (!toggle || !m_Toggles.Contains(toggle))
                return;

            m_RequiredToggles.Remove(toggle);
            
            if (m_SelectedOrder.Contains(toggle))
            {
                m_SelectedOrder.Remove(toggle);
            }

            m_Toggles.Remove(toggle);
        }

        public void OnToggleValueChanged(BetterToggle toggle, bool isOn)
        {
            // Prevent required toggles from being turned off
            if (!isOn && m_RequiredToggles.Contains(toggle))
            {
                toggle.SetIsOnWithoutNotify(true);
                return;
            }

            int currentSelected = m_Toggles.Count(t => t.isOn);

            if (isOn)
            {
                // Handle selection
                if (currentSelected > maxSelectedCount)
                {
                    HandleExceedMaxCount();
                }
                if (!m_SelectedOrder.Contains(toggle))
                {
                    m_SelectedOrder.Add(toggle);
                }
            }
            else
            {
                // Prevent going below minimum selection count
                if (currentSelected < minSelectedCount)
                {
                    toggle.SetIsOnWithoutNotify(true);
                    return;
                }
                
                m_SelectedOrder.Remove(toggle);
            }
        }

        private void HandleExceedMaxCount()
        {
            while (m_SelectedOrder.Count >= maxSelectedCount)
            {
                var oldestToggle = m_SelectedOrder[0];
                if (!m_RequiredToggles.Contains(oldestToggle))
                {
                    oldestToggle.SetIsOnWithoutNotifyGroup(false);
                    m_SelectedOrder.RemoveAt(0);
                    break;
                }
                m_SelectedOrder.RemoveAt(0);
            }
        }

        public void SetRequiredToggle(BetterToggle toggle, bool required)
        {
            if (!m_Toggles.Contains(toggle))
            {
                Debug.LogWarning("Toggle is not registered to this group");
                return;
            }

            if (required)
            {
                if (m_RequiredToggles.Count >= maxSelectedCount)
                {
                    Debug.LogWarning("Cannot set more required toggles than maxSelectedCount");
                    return;
                }

                m_RequiredToggles.Add(toggle);
                if (!toggle.isOn)
                {
                    toggle.isOn = true;
                }
            }
            else
            {
                m_RequiredToggles.Remove(toggle);
            }
        }

        public void SetSelectionLimits(int min, int max)
        {
            min = Mathf.Max(0, Mathf.Min(min, m_Toggles.Count));
            max = Mathf.Max(min, Mathf.Min(max, m_Toggles.Count));

            minSelectedCount = min;
            maxSelectedCount = max;

            // Handle required toggles that exceed new max
            if (m_RequiredToggles.Count > maxSelectedCount)
            {
                var excessToggles = m_RequiredToggles.Skip(maxSelectedCount).ToList();
                foreach (var toggle in excessToggles)
                {
                    m_RequiredToggles.Remove(toggle);
                }
            }

            // Ensure minimum selections
            int currentSelected = m_Toggles.Count(t => t.isOn);
            if (currentSelected < minSelectedCount)
            {
                foreach (var toggle in m_Toggles)
                {
                    if (!toggle.isOn && currentSelected < minSelectedCount)
                    {
                        toggle.isOn = true;
                        if (!m_SelectedOrder.Contains(toggle))
                        {
                            m_SelectedOrder.Add(toggle);
                        }
                        currentSelected++;
                    }
                }
            }
            // Handle excess selections
            else if (currentSelected > maxSelectedCount)
            {
                while (m_SelectedOrder.Count > maxSelectedCount)
                {
                    var toggle = m_SelectedOrder[0];
                    if (!m_RequiredToggles.Contains(toggle))
                    {
                        toggle.isOn = false;
                    }
                }
            }
        }

        public List<BetterToggle> GetSelectedToggles() => m_Toggles.Where(t => t.isOn).ToList();
        public List<BetterToggle> GetRequiredToggles() => m_RequiredToggles.ToList();
        public bool IsRequiredToggle(BetterToggle toggle) => m_RequiredToggles.Contains(toggle);
        public int GetToggleCount() => m_Toggles.Count;
        
        public void EnsureValidState()
        {
            var currentSelected = m_Toggles.Count(t => t.isOn);
            if (currentSelected < minSelectedCount && m_Toggles.Count > 0)
            {
                for (int i = 0; i < m_Toggles.Count && currentSelected < minSelectedCount; i++)
                {
                    if (!m_Toggles[i].isOn)
                    {
                        m_Toggles[i].isOn = true;
                        currentSelected++;
                    }
                }
            }
        }
        
        private void ValidateToggleIsInGroup(BetterToggle toggle)
        {
            if (!toggle || !m_Toggles.Contains(toggle))
                throw new ArgumentException($"Toggle {toggle} is not part of ToggleGroup {this}");
        }
        
        public void NotifyToggleOn(BetterToggle toggle)
        {
            ValidateToggleIsInGroup(toggle);

            // If the toggle is already in the selection order, move it to the end
            if (m_SelectedOrder.Contains(toggle))
            {
                m_SelectedOrder.Remove(toggle);
            }
            m_SelectedOrder.Add(toggle);

            // Handle maximum selection limit
            while (m_SelectedOrder.Count > maxSelectedCount)
            {
                var oldestToggle = m_SelectedOrder[0];
                if (!m_RequiredToggles.Contains(oldestToggle))
                {
                    m_SelectedOrder.RemoveAt(0);
                    oldestToggle.SetIsOnWithoutNotify(false);
                }
                else
                {
                    // If the oldest toggle is required, try the next one
                    var toggleToTurnOff = m_SelectedOrder.FirstOrDefault(t => !m_RequiredToggles.Contains(t));
                    if (toggleToTurnOff != null)
                    {
                        m_SelectedOrder.Remove(toggleToTurnOff);
                        toggleToTurnOff.SetIsOnWithoutNotify(false);
                    }
                    else
                    {
                        // If all remaining toggles are required, we can't turn any off
                        Debug.LogWarning("Cannot turn off any more toggles as all remaining ones are required");
                        break;
                    }
                }
            }
            toggle.isOn = true;
        }

        public bool AnyTogglesOn() => m_Toggles.Any(x => x.isOn);
        public IEnumerable<BetterToggle> ActiveToggles() => m_Toggles.Where(x => x.isOn);
    }
}