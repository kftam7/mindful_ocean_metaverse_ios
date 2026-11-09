using System;
using System.Collections.Generic;
using ProGamer;
using UnityEngine;

#region Version

public class GetLatestVersionData
{
    public LatestVersionResultData[] data;
    public string meta;
}

public class LatestVersionResultData
{
    public int versionId { get; set; }
    public string versionNo { get; set; }
    public string versionName { get; set; }
    public DateTime releaseDate { get; set; }
    public DateTime expireDate { get; set; }
    public string description { get; set; }
    public int isActive { get; set; }
}

#endregion

#region Account

public class LoginRequestData
{
    public LoginRequestNameData name { get; set; }
    public string email { get; set; }
    public string refresh_token { get; set; }
    public string id_token { get; set; }
}

public class LoginRequestNameData
{
    public string firstName { get; set; }
    public string lastName { get; set; }
}

public class LoginResultData
{
    public LoginRequestNameData[] data;
    public string message;
}

public class GetUserInfoResultData
{
    public UserInfoResultData data = new();
}

public class UserInfoResultData
{
    public string email { get; set; }
    public string userId { get; set; }
    public string appleId { get; set; }
    public string lastName { get; set; }
    public string firstName { get; set; }
    public string createTime { get; set; }
    public string personInfo { get; set; }
    public string updateTime { get; set; }
    public string joinedEvent { get; set; }
}

#endregion

#region Events

public enum EventStatus
{
    Inactive,
    Coming,
    Active,
    Ended
}

public class GetEventsResultData
{
    public List<EventItemResultData> data = new();

    public EventItemResultData GetEventById(int id)
    {
        return data.Find(e => e.eventId == id);
    }
}

public class EventItemResultData
{
    public int eventId { get; set; }
    public string title { get; set; }
    public string titleCN { get; set; }
    public string titleTW { get; set; }
    public string paragraphText { get; set; }
    public string paragraphTextCN { get; set; }
    public string paragraphTextTW { get; set; }
    public string type { get; set; }

    /// <summary>
    /// Url of template image, format:
    /// {baseUrl}/tpl_outline_{modelName}_{suffixes}.png/jpg
    /// </summary>
    public string image { get; set; }

    public string hyperLink { get; set; }
    public string timerDescription { get; set; }
    public string timerDescriptionCN { get; set; }
    public string timerDescriptionTW { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public int isActive { get; set; }

    public EventStatus GetStatus()
    {
        if (isActive == 0) return EventStatus.Inactive;
        var nowUtc = DateTime.UtcNow;
        if (startTime > nowUtc) return EventStatus.Coming;
        if (startTime <= nowUtc && endTime > nowUtc) return EventStatus.Active;
        return EventStatus.Ended;
    }

    public TimeSpan timeSpan
    {
        get
        {
            switch (GetStatus())
            {
                case EventStatus.Coming:
                    return startTime - DateTime.UtcNow;
                case EventStatus.Active:
                    return endTime - DateTime.UtcNow;
                case EventStatus.Ended:
                    return DateTime.UtcNow - endTime;
                default:
                    return TimeSpan.Zero;
            }
        }
    }

    public string GetTile()
    {
        return Localization.language switch
        {
            "English" => title,
            "SimplifiedChinese" => titleCN,
            "TraditionalChinese" => titleTW,
            _ => title
        };
    }

    public string GetParagraphText()
    {
        return Localization.language switch
        {
            "English" => paragraphText,
            "SimplifiedChinese" => paragraphTextCN,
            "TraditionalChinese" => paragraphTextTW,
            _ => paragraphText
        };
    }

    public string GetTimerDescription()
    {
        return Localization.language switch
        {
            "English" => timerDescription,
            "SimplifiedChinese" => timerDescriptionCN,
            "TraditionalChinese" => timerDescriptionTW,
            _ => timerDescription
        };
    }
}

#endregion

#region Nft

public class GetNftsResultData
{
    public List<NftItemResultData> data = new();
}

public class NftItemResultData
{
    public string walletAddress { get; set; }
    public string tokenId { get; set; }
    public string tokenUriRaw { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string description { get; set; }
    public string extraInfo { get; set; }
    public string imageURL { get; set; }

    private string mFileName;

    public string fileName
    {
        get
        {
            if (string.IsNullOrEmpty(mFileName))
            {
                mFileName = ImageFileNameUtils.GetFullFileNameFromUrl(imageURL);
            }

            return mFileName;
        }
    }
}

#endregion

#region Artwork

public class ArtworkItemRequestData
{
    public string appleId { get; set; }
    public int eventId { get; set; }
    public string URL { get; set; }
    public string type { get; set; } // model type
    public string artistMessage { get; set; }
    public int ifHomepage { get; set; }
    public int ifMinted { get; set; }
    public int ifRedeemed { get; set; }
    public int isActive { get; set; }
}

public class GetArtworksResultData
{
    public List<ArtworkItemResultData> data = new();
}

public class ArtworkItemResultData
{
    public int userId { get; set; }
    public string appleId { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public int artworkId { get; set; }
    public int eventId { get; set; }
    public string URL { get; set; }
    public string type { get; set; }
    public string artistMessage { get; set; }
    public string extraInfo { get; set; }
    public string mintpassStorage { get; set; }
    public string mindfultokenStorage { get; set; }
    public int ifHomepage { get; set; }
    public int ifMinted { get; set; }
    public int ifRedeemed { get; set; }
    public string authenticScarcity { get; set; }
    public string scarcityRank { get; set; }
    public string clusterId { get; set; }
    public string clusterSize { get; set; }
    public string superClusterId { get; set; }
    public string superClusterSize { get; set; }
    public int isActive { get; set; }
    public string createTime { get; set; }
    public string updateTime { get; set; }

    private string mName;

    public string name
    {
        get
        {
            if (string.IsNullOrEmpty(mName))
            {
                mName = ImageFileNameUtils.GetFullFileNameFromUrl(URL);
            }

            return mName;
        }
    }
}

#endregion

#region Artwork File

public class ArtworkFileItemResultData
{
    public string name { get; set; }
    public string url { get; set; }
}

#endregion