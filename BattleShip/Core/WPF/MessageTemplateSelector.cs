using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WPF_App.Core.WPF
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PlayerMessageTemplate { get; set; }
        public DataTemplate OpponentMessageTemplate { get; set; }
        public DataTemplate ServerMessageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MVVM.Model.Message message)
            {
                // If isMine => MyMessageTemplate
                // If isServer => ServerMessageTemplate
                // Else => OtherMessageTemplate

                if (message.IsMine)
                {
                    return PlayerMessageTemplate;
                }
                else if (message.IsServer)
                {
                    return ServerMessageTemplate;
                }
                else
                {
                    return OpponentMessageTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
