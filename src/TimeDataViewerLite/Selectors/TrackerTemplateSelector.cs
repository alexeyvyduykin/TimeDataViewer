﻿using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using TimeDataViewerLite.Core;

namespace TimeDataViewerLite.Selectors;

public class TrackerTemplateSelector : IDataTemplate
{
    private static readonly IDataTemplate s_defaultTemplate;
    private readonly string _defaultKey = "Default";

    static TrackerTemplateSelector()
    {
        s_defaultTemplate = new FuncDataTemplate<TrackerHitResult>((value, namescope) =>
        new TextBlock
        {
            Margin = new Avalonia.Thickness(7),
            [!TextBlock.TextProperty] = new Binding("Text"),
        });
    }

    [Content]
    public Dictionary<string, IDataTemplate> Templates { get; } = new Dictionary<string, IDataTemplate>();

    public Control Build(object? param)
    {
        if (param is null)
        {
            throw new Exception();
        }

        var key = ((TrackerHitResult)param).Series?.TrackerKey;

        if (key != null && Templates.ContainsKey(key))
        {
            return Templates[key].Build(param)!;
        }

        if (Templates.ContainsKey(_defaultKey))
        {
            return Templates[_defaultKey].Build(param)!;
        }

        return s_defaultTemplate.Build(param)!;
    }

    public bool Match(object? data)
    {
        return data is TrackerHitResult;
    }
}
