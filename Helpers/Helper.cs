using System;
using System.Collections.Generic;
using Lamb.UI.FollowerSelect;

namespace CotLMiniMods.Helpers {
    public class Helper {
        public static List<FollowerSelectEntry> MakeSimpleFSEFromList(List<FollowerInfo> followerInfoList, FollowerSelectEntry.Status status = FollowerSelectEntry.Status.Available) {
            List<FollowerSelectEntry> followerSelectEntries = new List<FollowerSelectEntry>();
            foreach (FollowerInfo followerInfo in followerInfoList) {
                followerSelectEntries.Add(new FollowerSelectEntry(followerInfo, status));
            }
            return followerSelectEntries;
        }
    }
}
