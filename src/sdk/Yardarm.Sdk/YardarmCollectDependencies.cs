using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Yardarm.CommandLine.Interop;

namespace Yardarm.Build.Tasks;

public class YardarmCollectDependencies : YardarmCommonTask
{
    private const string AddItemPrefix = "AddItem: ";

    private static readonly JsonSerializerOptions s_serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected override string Verb => "collect-dependencies";

    private readonly List<ITaskItem> _packageReference = new();
    private readonly List<ITaskItem> _packageDownload = new();

    [Output]
    public ITaskItem[]? PackageReference { get; set; }

    [Output]
    public ITaskItem[]? PackageDownload { get; set; }

    public override bool Execute()
    {
        bool result = base.Execute();

        PackageReference = _packageReference.ToArray();
        PackageDownload = _packageDownload.ToArray();

        return result;
    }

    protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
    {
        if (!singleLine.StartsWith(AddItemPrefix))
        {
            base.LogEventsFromTextOutput(singleLine, messageImportance);
            return;
        }

        try
        {
            singleLine = singleLine.Substring(AddItemPrefix.Length);

            var item = JsonSerializer.Deserialize<AddItemDto>(singleLine, s_serializerOptions)!;
            if (!string.IsNullOrWhiteSpace(item.Identity))
            {
                var taskItem = new TaskItem(item.Identity);

                if (item.Metadata is not null)
                {
                    foreach (var metadata in item.Metadata)
                    {
                        taskItem.SetMetadata(metadata.Key, metadata.Value);
                    }
                }

                switch (item.ItemType)
                {
                    case "PackageReference":
                        _packageReference.Add(taskItem);
                        break;

                    case "PackageDownload":
                        _packageDownload.Add(taskItem);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, false);
        }
    }
}
