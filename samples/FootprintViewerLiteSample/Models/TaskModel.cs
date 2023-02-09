namespace FootprintViewerLiteSample.Models;

public enum TaskType
{
    Observation,
    Download
}

public class TaskModel
{
    public string Name { get; set; } = string.Empty;

    public TaskType Type { get; set; }
}
