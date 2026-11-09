using System;

public class ImageData : IEquatable<ImageData>
{
    private string id;

    public string Id
    {
        get
        {
            if (string.IsNullOrEmpty(id))
            {
                id = ImageFileNameUtils.GetFileId(ImageFile) + (IsDebugData ? "_debug" : "");
            }

            return id;
        }
    }

    /// <summary>
    /// Full file name.
    /// artwork: usr_{modelName}_{suffixes}_{id}.jpg
    /// aquarium: hatchery_aquarium.jpg / metaverse_aquarium.jpg
    /// </summary>
    public string ImageFile { get; set; }

    public string Url { get; set; }

    public int EventId { get; set; }
    public string Type { get; set; }
    public DateTime CaptureTime { get; set; }
    public bool InAquarium { get; set; }
    public bool Submitted { get; set; }
    public bool IsNft { get; set; }
    public bool Unlocked { get; set; }
    public bool IsDebugData { get; set; } // used for temporarily created image data
    public bool IsDefault { get; set; }
    public bool ConfirmedByServer { get; set; } // used to check if a local image data is confirmed by ImageDataManager.UpdateServerImageData

    // tpl_outline_{modelName}_{suffixes}.png
    private string templateFile;
    private string modelName;

    /// <summary>
    /// For snapshot.
    /// </summary>
    public ImageData() { }

    /// <summary>
    /// For artwork.
    /// </summary>
    public ImageData(string fullTemplateFileName)
    {
        templateFile = fullTemplateFileName;
        string templateFileName = ImageFileNameUtils.GetFileName(templateFile);
        var elements = templateFileName.Split("_");
        ImageFile = $"usr_{elements[2]}_{elements[3]}_{Guid.NewGuid():N}.jpg";
    }

    public string GetTemplateFile()
    {
        if (string.IsNullOrWhiteSpace(templateFile))
        {
            string imageFileName = ImageFileNameUtils.GetFileName(ImageFile);
            var elements = imageFileName.Split("_");
            templateFile = $"tpl_outline_{elements[1]}_{elements[2]}.png";
        }

        return templateFile;
    }

    public string GetModelName()
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            modelName = ImageFileNameUtils.GetModelName(ImageFile);
        }

        return modelName;
    }

    public DateTime GetStartFromEvent()
    {
        var e = ServerCenter.Instance.EventDataList.Find(e => e.eventId == EventId);
        return e?.startTime ?? DateTime.MinValue;
    }

    public DateTime GetEndFromEvent()
    {
        var e = ServerCenter.Instance.EventDataList.Find(e => e.eventId == EventId);
        return e?.endTime ?? DateTime.MinValue;
    }

    public EventStatus GetStatus()
    {
        if (Debugger.Instance.AllFishDeactived)
        {
            return EventStatus.Ended;
        }
        else
        {
            var nowUtc = DateTime.UtcNow;
            if (GetStartFromEvent() > nowUtc) return EventStatus.Coming;
            if (GetStartFromEvent() <= nowUtc && GetEndFromEvent() > nowUtc) return EventStatus.Active;
            return EventStatus.Ended;
        }
    }

    public TimeSpan GetTimeSpan()
    {
        switch (GetStatus())
        {
            case EventStatus.Coming:
                return GetStartFromEvent() - DateTime.UtcNow;
            case EventStatus.Active:
                return GetEndFromEvent() - DateTime.UtcNow;
            case EventStatus.Ended:
                return DateTime.UtcNow - GetEndFromEvent();
            default:
                return TimeSpan.Zero;
        }
    }

    public ImageData Clone()
    {
        var data = new ImageData
        {
            ImageFile = ImageFile,
            CaptureTime = CaptureTime,
            Submitted = Submitted,
            InAquarium = InAquarium
        };
        return data;
    }

    public bool Equals(ImageData other)
    {
        return Id == other?.Id;
    }

    public static bool operator ==(ImageData data1, ImageData data2)
    {
        if (ReferenceEquals(data1, data2)) return true;
        if (ReferenceEquals(data1, null) || ReferenceEquals(data2, null)) return false;
        return data1.Equals(data2);
    }

    public static bool operator !=(ImageData data1, ImageData data2)
    {
        return !(data1 == data2);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}