using System;

namespace DomainSessionDemo
{
    [Serializable]
    internal struct TokenData
    {
        public string access_token;
    }

    [Serializable]
    internal struct TokenPayloadData
    {
        public long exp;
    }

    [Serializable]
    internal struct SpaceData
    {
        public string id;
        public string expires_at;
    }

    [Serializable]
    internal struct SpacesListData
    {
        public SpaceData[] spaces;
    }

    [Serializable]
    internal struct SpaceReportData
    {
        public string space_id;

        public SpaceReportData(string spaceID)
        {
            space_id = spaceID;
        }
    }
}