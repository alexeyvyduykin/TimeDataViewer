using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using TimeDataViewerLite.Core;

namespace TimeDataViewerLite.Controls;

public partial class TimelineControl
{
    private ICommand? _selectedInterval;

    public static readonly StyledProperty<PlotModel> PlotModelProperty =
        AvaloniaProperty.Register<TimelineControl, PlotModel>(nameof(PlotModel), PlotModelBuilder.Build());

    public PlotModel PlotModel
    {
        get => GetValue(PlotModelProperty);
        set => SetValue(PlotModelProperty, value);
    }

    public static readonly StyledProperty<ControlTemplate> DefaultLabelTemplateProperty =
        AvaloniaProperty.Register<TimelineControl, ControlTemplate>(nameof(DefaultLabelTemplate));

    public ControlTemplate DefaultLabelTemplate
    {
        get
        {
            return GetValue(DefaultLabelTemplateProperty);
        }

        set
        {
            SetValue(DefaultLabelTemplateProperty, value);
        }
    }

    public static readonly StyledProperty<IDataTemplate> TrackerTemplateProperty =
        AvaloniaProperty.Register<TimelineControl, IDataTemplate>(nameof(TrackerTemplate));

    public IDataTemplate TrackerTemplate
    {
        get
        {
            return GetValue(TrackerTemplateProperty);
        }

        set
        {
            SetValue(TrackerTemplateProperty, value);
        }
    }

    public static readonly DirectProperty<TimelineControl, ICommand?> SelectedIntervalProperty =
        AvaloniaProperty.RegisterDirect<TimelineControl, ICommand?>(nameof(SelectedInterval),
            timeline => timeline.SelectedInterval, (timeline, command) => timeline.SelectedInterval = command, enableDataValidation: true);

    public static readonly StyledProperty<double> ActiveCategoriesCountProperty =
        AvaloniaProperty.Register<TimelineControl, double>(nameof(ActiveCategoriesCount));

    public static readonly StyledProperty<DataTemplate> CategoryListBoxItemTemplateProperty =
        AvaloniaProperty.Register<TimelineControl, DataTemplate>(nameof(CategoryListBoxItemTemplate));

    public DataTemplate CategoryListBoxItemTemplate
    {
        get => GetValue(CategoryListBoxItemTemplateProperty);
        set => SetValue(CategoryListBoxItemTemplateProperty, value);
    }

    public double ActiveCategoriesCount
    {
        get
        {
            return GetValue(ActiveCategoriesCountProperty);
        }

        set
        {
            SetValue(ActiveCategoriesCountProperty, value);
        }
    }

    public ICommand? SelectedInterval
    {
        get => _selectedInterval;
        set => SetAndRaise(SelectedIntervalProperty, ref _selectedInterval, value);
    }

    private void OnSelectedInterval(TrackerHitResult result)
    {
        SelectedInterval?.Execute(result);
    }
}
