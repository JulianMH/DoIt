using System;
using System.Windows;
using System.Windows.Controls;

namespace DoIt.Controls
{
#if WP8
    /// <summary>
    /// Wrapper Klasse, wählt immer den richtigen Long List Selector aus.
    /// </summary>
    public class LongListSelector : Microsoft.Phone.Controls.LongListSelector {

        //public static readonly DependencyProperty GroupItemTemplateProperty =
        //     DependencyProperty.Register("GroupItemTemplate", typeof(DataTemplate),
        //     typeof(LongListSelector), new PropertyMetadata(null));

        //public DataTemplate GroupItemTemplate
        //{
        //    get { return (DataTemplate)GetValue(GroupItemTemplateProperty); }
        //    set { SetValue(GroupItemTemplateProperty, value); }
        //}
       
        //public static readonly DependencyProperty GroupItemsPanelProperty =
        //     DependencyProperty.Register("GroupItemsPanel", typeof(ItemsPanelTemplate),
        //     typeof(LongListSelector), new PropertyMetadata(null));

        //public ItemsPanelTemplate GroupItemsPanel
        //{
        //    get { return (ItemsPanelTemplate)GetValue(GroupItemsPanelProperty); }
        //    set { SetValue(GroupItemsPanelProperty, value); }
        //}
    
    }
#else
    public class LongListSelector : Microsoft.Phone.Controls.LongListSelector { }
#endif
}
