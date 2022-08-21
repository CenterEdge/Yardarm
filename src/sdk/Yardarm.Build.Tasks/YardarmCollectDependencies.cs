using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Yardarm.CommandLine.Interop;

#if NETCOREAPP
using System.Text.Json;
#else
using System.IO;
using System.Runtime.Serialization.Json;
#endif

namespace Yardarm.Build.Tasks;

public class YardarmCollectDependencies : YardarmCommonTask
{
    private const string AddItemPrefix = "AddItem: ";

#if NETCOREAPP
    private static readonly JsonSerializerOptions s_serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
#else
    private static readonly DataContractJsonSerializer s_serializer = new(typeof(AddItemDto), new DataContractJsonSerializerSettings
    {
        UseSimpleDictionaryFormat = true
    });
#endif

    protected override string Verb => "collect-dependencies";

    private readonly List<ITaskItem> _packageReference = new();
    private readonly List<ITaskItem> _packageDownload = new();
    private readonly List<ITaskItem> _frameworkReference = new();

    [Output]
    public ITaskItem[] PackageReference => _packageReference.ToArray();

    [Output]
    public ITaskItem[] PackageDownload => _packageDownload.ToArray();

    [Output]
    public ITaskItem[] FrameworkReference => _frameworkReference.ToArray();

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

#if NETCOREAPP
            var item = JsonSerializer.Deserialize<AddItemDto>(singleLine, s_serializerOptions)!;
#else
            // Since we need to be compatible with net472 and don't want to take dependencies, we must engage in this ugliness
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(singleLine.Substring(AddItemPrefix.Length)));
            var item = (AddItemDto) s_serializer.ReadObject(stream);
#endif

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

                    case "FrameworkReference":
                        _frameworkReference.Add(taskItem);
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
