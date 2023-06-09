﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;

namespace LXIntegratedNavigation.WPF.Views;

/// <summary>
/// https://stackoverflow.com/questions/2006729/how-can-i-have-a-listbox-auto-scroll-when-a-new-item-is-added
/// </summary>
public class ListBoxBehavior
{
    static readonly Dictionary<ListBox, Capture> _associations =
          new();

    public static bool GetScrollOnNewItem(DependencyObject obj)
    {
        return (bool)obj.GetValue(ScrollOnNewItemProperty);
    }

    public static void SetScrollOnNewItem(DependencyObject obj, bool value)
    {
        obj.SetValue(ScrollOnNewItemProperty, value);
    }

    public static readonly DependencyProperty ScrollOnNewItemProperty =
        DependencyProperty.RegisterAttached(
            "ScrollOnNewItem",
            typeof(bool),
            typeof(ListBoxBehavior),
            new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

    public static void OnScrollOnNewItemChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox) return;
        bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
        if (newValue == oldValue) return;
        if (newValue)
        {
            listBox.Loaded += ListBox_Loaded;
            listBox.Unloaded += ListBox_Unloaded;
            var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
            itemsSourcePropertyDescriptor?.AddValueChanged(listBox, ListBox_ItemsSourceChanged);
        }
        else
        {
            listBox.Loaded -= ListBox_Loaded;
            listBox.Unloaded -= ListBox_Unloaded;
            if (_associations.ContainsKey(listBox))
                _associations[listBox].Dispose();
            var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
            itemsSourcePropertyDescriptor?.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
        }
    }

    private static void ListBox_ItemsSourceChanged(object? sender, EventArgs e)
    {
        if (sender is null)
            return;
        var listBox = (ListBox)sender;
        if (_associations.ContainsKey(listBox))
            _associations[listBox].Dispose();
        _associations[listBox] = new Capture(listBox);
    }

    static void ListBox_Unloaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        if (_associations.ContainsKey(listBox))
            _associations[listBox].Dispose();
        listBox.Unloaded -= ListBox_Unloaded;
    }

    static void ListBox_Loaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        if (listBox.Items is not INotifyCollectionChanged incc) return;
        listBox.Loaded -= ListBox_Loaded;
        _associations[listBox] = new Capture(listBox);
    }

    class Capture : IDisposable
    {
        private readonly ListBox _listBox;
        private readonly INotifyCollectionChanged? _incc;

        public Capture(ListBox listBox)
        {
            this._listBox = listBox;
            _incc = (INotifyCollectionChanged)listBox.ItemsSource;
            if (_incc != null)
            {
                _incc.CollectionChanged += Incc_CollectionChanged;
            }
        }

        void Incc_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                _listBox.ScrollIntoView(e.NewItems?[0]);
                _listBox.SelectedItem = e.NewItems?[0];
            }
        }

        public void Dispose()
        {
            if (_incc != null)
                _incc.CollectionChanged -= Incc_CollectionChanged;
        }
    }
}