using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using TimeDataViewerLite.Core;

namespace TimeDataViewerLite.Controls;

public partial class TimelineControl
{
    private ICommand? _selectedInterval;

    public static readonly StyledProperty<double> BeginProperty =
        AvaloniaProperty.Register<TimelineControl, double>(nameof(Begin));

    public double Begin
    {
        get
        {
            return GetValue(BeginProperty);
        }

        set
        {
            SetValue(BeginProperty, value);
        }
    }

    public static readonly StyledProperty<double> DurationProperty =
        AvaloniaProperty.Register<TimelineControl, double>(nameof(Duration));

    public double Duration
    {
        get
        {
            return GetValue(DurationProperty);
        }

        set
        {
            SetValue(DurationProperty, value);
        }
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

    public static readonly StyledProperty<IList<double>> CategoriesProperty =
        AvaloniaProperty.Register<TimelineControl, IList<double>>(nameof(Categories));

    public IList<double> Categories
    {
        get
        {
            return GetValue(CategoriesProperty);
        }

        set
        {
            SetValue(CategoriesProperty, value);
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
