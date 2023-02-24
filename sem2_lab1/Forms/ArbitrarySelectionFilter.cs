using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Laborator1;

public class ArbitrarySelectionFilterModel
{
    private readonly BitArray _checkedStatesArray;
    private readonly ObservableCollection<bool> _checkedStates = new();

    public ArbitrarySelectionFilterModel(IChangedEvent<int> itemCount)
    {
        itemCount.ValueChanged += OnItemCountChanged;
        
        // In our case we know 100 is the max, so I can shortcut resizing this thing.
        _checkedStatesArray = new BitArray(100, true);
        
        // synchronize
        _checkedStates.CollectionChanged += (_, e) =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    break;
                
                case NotifyCollectionChangedAction.Move:
                    throw new System.NotSupportedException();

                case NotifyCollectionChangedAction.Replace:
                {
                    for (int i = 0; i < e.NewItems!.Count; i++)
                        _checkedStatesArray[e.NewStartingIndex + i] = (bool) e.NewItems[i]!;
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    // might need to copy everything over?
                    break;
            }
        };
    }
    
    private void OnItemCountChanged(int v)
    {
        while (_checkedStates.Count < v)
            _checkedStates.Add(_checkedStatesArray[_checkedStates.Count]);
        while (_checkedStates.Count > v)
            _checkedStates.RemoveAt(_checkedStates.Count - 1);
    }
    
    public IEnumerable<int> SelectedIndices
    {
        get
        {
            for (int i = 0; i < _checkedStates.Count; i++)
            {
                if (_checkedStates[i])
                    yield return i;
            }
        }
    }
    
    public ObservableCollection<bool> CheckedStates => _checkedStates;
}

public static class ArbitrarySelectionFilterViewHelper
{
    private record struct CheckBoxWithParent(int Index, StackPanel ParentPanel, CheckBox? CheckBox);

    private static IEnumerable<CheckBoxWithParent> GetCheckBoxesWithParent(ItemContainerGenerator g)
    {
        for (int i = 0; i < g.Items.Count; i++)
        {
            var child = (Visual) g.ContainerFromIndex(i);
            var stackPanel = child.SelfAndDescendants().OfType<StackPanel>().First();
            var checkBox = stackPanel.Descendats().OfType<CheckBox>().FirstOrDefault();
            yield return new(i, stackPanel, checkBox);
        }
    }

    public static void RemoveFilterCheckboxes(this ItemContainerGenerator g)
    {
        foreach (var t in GetCheckBoxesWithParent(g)
            .Where(t => t.CheckBox is not null))
        {
            t.ParentPanel.Children.Remove(t.CheckBox);
        }
    }

    public static void CreateOrRebindFilterCheckboxes(this ItemContainerGenerator g, ArbitrarySelectionFilterModel model)
    {
        foreach (var t in GetCheckBoxesWithParent(g))
        {
            if (t.CheckBox is null)
            {
                CreateCheckBox(t.ParentPanel, t.Index);
                continue;
            }
            
            // Check if the binding property has the correct index
            var binding = (Binding) t.CheckBox.GetBindingExpression(ToggleButton.IsCheckedProperty)!.ParentBinding;
            var pathParams = binding.Path.PathParameters;
            if (pathParams.Count > 0 && (int) pathParams[0] == t.Index)
                continue;

            BindingOperations.ClearBinding(t.CheckBox, ToggleButton.IsCheckedProperty);
            SetBinding(t.CheckBox, t.Index);
        }
        
        void CreateCheckBox(StackPanel panel, int index)
        {
            var checkBox = new CheckBox();
            checkBox.HorizontalAlignment = HorizontalAlignment.Left;
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.Margin = new Thickness(0, 0, 0, 0);
            checkBox.Width = 15;
            checkBox.Height = 15;
            // The position of the checkbox must be to the left of the itemControl.
            panel.Children.Insert(0, checkBox);
            SetBinding(checkBox, index);
        }

        void SetBinding(CheckBox checkBox, int index)
        {
            var binding = new Binding($"[{index}]") {Source = model.CheckedStates};
            checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);
        }
    }
}