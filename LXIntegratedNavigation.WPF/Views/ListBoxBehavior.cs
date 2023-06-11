﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LXIntegratedNavigation.WPF.Views;

/// <summary>
/// https://stackoverflow.com/questions/2006729/how-can-i-have-a-listbox-auto-scroll-when-a-new-item-is-added
/// </summary>
public class ListBoxBehavior
{
    static readonly Dictionary<ListBox, Capture> Associations =
          new Dictionary<ListBox, Capture>();

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
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
            itemsSourcePropertyDescriptor?.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
        }
    }

    private static void ListBox_ItemsSourceChanged(object? sender, EventArgs e)
    {
        if (sender is null)
            return;
        var listBox = (ListBox)sender;
        if (Associations.ContainsKey(listBox))
            Associations[listBox].Dispose();
        Associations[listBox] = new Capture(listBox);
    }

    static void ListBox_Unloaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        if (Associations.ContainsKey(listBox))
            Associations[listBox].Dispose();
        listBox.Unloaded -= ListBox_Unloaded;
    }

    static void ListBox_Loaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        if (listBox.Items is not INotifyCollectionChanged incc) return;
        listBox.Loaded -= ListBox_Loaded;
        Associations[listBox] = new Capture(listBox);
    }

    class Capture : IDisposable
    {
        private readonly ListBox listBox;
        private readonly INotifyCollectionChanged? incc;

        public Capture(ListBox listBox)
        {
            this.listBox = listBox;
            incc = (INotifyCollectionChanged)listBox.ItemsSource;
            if (incc != null)
            {
                incc.CollectionChanged += Incc_CollectionChanged;
            }
        }

        void Incc_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                listBox.ScrollIntoView(e.NewItems?[0]);
                listBox.SelectedItem = e.NewItems?[0];
            }
        }

        public void Dispose()
        {
            if (incc != null)
                incc.CollectionChanged -= Incc_CollectionChanged;
        }
    }
}