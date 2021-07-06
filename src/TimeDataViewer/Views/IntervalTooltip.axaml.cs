using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Timeline.Views
{
    public class IntervalTooltip : UserControl
    {
        public IntervalTooltip()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
