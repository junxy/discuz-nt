using System;
using System.Collections.Generic;
using System.Text;
using Discuz.Common;
using Discuz.Config;
using Discuz.Data;
using Discuz.Entity;

namespace Discuz.Data.MySql
{
    public partial class DataProvider : IDataProvider
    {
        public int CreateAnnouncement(AnnouncementInfo announcementInfo)
        {
            throw new NotImplementedException();
        }

        public void AddMedal(string name, int available, string image)
        {
            throw new NotImplementedException();
        }

        public int AddOnlineUser(OnlineUserInfo onlineUserInfo, int timeOut, int deletingFrequency)
        {
            throw new NotImplementedException();
        }

        public void AddParentForumTopics(string fpIdList, int topics)
        {
            throw new NotImplementedException();
        }

        public int CreatePaymentLog(int uid, int tid, int posterId, int price, float netAmount)
        {
            throw new NotImplementedException();
        }

        public void AddUserGroup(UserGroupInfo userGroupInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserAdminIdByGroupId(int adminId, int groupId)
        {
            throw new NotImplementedException();
        }

        public bool CheckUserCreditsIsEnough(int uid, float[] values, int pos, int mount)
        {
            throw new NotImplementedException();
        }

        public bool UpdateUser(UserInfo userInfo)
        {
            throw new NotImplementedException();
        }

        int IDataProvider.DeleteAnnouncements(string idList)
        {
            throw new NotImplementedException();
        }

        public void DeleteAttachmentByUid(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentListByUid(int uid)
        {
            throw new NotImplementedException();
        }

        public void DeletePostByUidAndDays(int uid, int days)
        {
            throw new NotImplementedException();
        }

        public void DeleteRateLog(int pid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAdvertisements(int type)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetAdvertisements()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetShortForumList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAllTemplateList()
        {
            throw new NotImplementedException();
        }

        public int GetTopicCountOfForumWithSub(int fid)
        {
            throw new NotImplementedException();
        }

        System.Data.IDataReader IDataProvider.GetAnnouncement(int id)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetAnnouncements()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAnnouncements(int num, int pageId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetFocusTopicList(int count, int views, int fid, string typeIdList, string startTime, string orderFieldName, string visibleForum, bool isDigest, bool onlyImg)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetForumLinks()
        {
            throw new NotImplementedException();
        }

        public int GetFirstFourmID()
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetModerators(int fid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNewTopics(string forumIdList, string postTableId)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetOnlineList()
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetPostList(string topicList, string[] postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPostRateLogs(int pid, int displayRateCount)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetRateRange(int scoreId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAnnouncementList(int maxCount)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetSmilies()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSpecialUserGroup()
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetStatisticsRow()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetModeratorLogByPostDate(DateTime postDateTime)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetModeratorLogByName(string moderatorName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetModeratorLogByPostDate(DateTime startDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
        }

        public int GetTopicCount()
        {
            throw new NotImplementedException();
        }

        public int GetTopicReplyCountByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public int GetTopicCountByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataSet GetTopTopicList()
        {
            throw new NotImplementedException();
        }

        public int GetUidByUserName(string userName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUnauditNewTopic()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUnauditPost(int currentPostTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUsersByUidlLst(string uidList)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetUserGroup()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetShortUserInfoByName(string userName)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserInfoToReader(string userName)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserListWithDigestTopicList(string digestTopicList, int digestType)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserListByGroupid(string groupIdList)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetVisibleForumList()
        {
            throw new NotImplementedException();
        }

        public int GetTopicCountInForumAndTopicIdList(string topicIdList, int fid)
        {
            throw new NotImplementedException();
        }

        public void PassAuditNewTopic(string postTableName, string tidList, string validate, string delete, string fidList)
        {
            throw new NotImplementedException();
        }

        public int GetModTopicCountByTidList(string fidList, string tidList)
        {
            throw new NotImplementedException();
        }

        public void AuditPost(int tableId, string validate, string delete, string ignore, string fidList)
        {
            throw new NotImplementedException();
        }

        public int GetModPostCountByPidList(string fidList, string postTableId, string pidList)
        {
            throw new NotImplementedException();
        }

        public void ResetUserDigestPosts()
        {
            throw new NotImplementedException();
        }

        public int Search(bool spaceEnabled, bool albumEnabled, int postTableId, int userId, int userGroupId, string keyWord, int posterId, SearchType searchType, string searchForumId, int searchTime, int searchTimeType, int resultOrder, int resultOrderType)
        {
            throw new NotImplementedException();
        }

        public string SearchModeratorManageLog(string keyWord)
        {
            throw new NotImplementedException();
        }

        public int UpdateAnnouncement(AnnouncementInfo announcementInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateModeratorName(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public void UpdatePMSenderAndReceiver(int uid, string newUserName)
        {
            throw new NotImplementedException();
        }

        public void UpdatePostPoster(int posterId, string poster)
        {
            throw new NotImplementedException();
        }

        public int UpdatePostRateTimes(string postIdList, string postTableId)
        {
            throw new NotImplementedException();
        }

        public int UpdateSmilies(int id, int displayOrder, int type, string code)
        {
            throw new NotImplementedException();
        }

        int IDataProvider.UpdateStatisticsLastUserName(int lastUserId, string lastUserName)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopic(string topicList, int fid, int topicType)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopicReplyCount(int tid, string postTableId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserCredits(int uid)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserCredits(int uid, float[] values, int pos, int mount)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserGroup(UserGroupInfo userGroupInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateAllUserPostCount(int postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetWebSiteAggHotForumList(int topNumber, string orderby, int fid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetWebSiteAggHotImages(int count, string orderby, string forumlist, int continuous)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumTags(string tagKey, int type)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.GetTopicNumber(string tagName, int from, int end, int type)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUsersRank(int count, string postTableId, string type)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTrendGraph(string field, string begin, string end)
        {
            throw new NotImplementedException();
        }

        public void CreateDebateTopic(DebateInfo debateTopic)
        {
            throw new NotImplementedException();
        }

        public bool UpdateDebateTopic(DebateInfo debateInfo)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserDiggs(int tid, int uid)
        {
            throw new NotImplementedException();
        }

        public void SynchronizeOnlineTime(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPostList(string postList, string tableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNoticeByUid(int uid, NoticeType noticeType)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNoticeByNid(int nid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNoticeByUid(int uid, NoticeType noticeType, int pageId, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int CreateNoticeInfo(NoticeInfo noticeInfo)
        {
            throw new NotImplementedException();
        }

        public bool UpdateNoticeInfo(NoticeInfo noticeInfo)
        {
            throw new NotImplementedException();
        }

        public bool DeleteNoticeByNid(int nid)
        {
            throw new NotImplementedException();
        }

        public bool DeleteNoticeByUid(int uid)
        {
            throw new NotImplementedException();
        }

        public int GetNoticeCountByUid(int uid, NoticeType noticeType)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUnauditNewTopic(string forumIdList, int tpp, int pageId, int filter)
        {
            throw new NotImplementedException();
        }

        public int GetNewNoticeCountByUid(int uid)
        {
            throw new NotImplementedException();
        }

        public void UpdateNoticeNewByUid(int uid, int newType)
        {
            throw new NotImplementedException();
        }

        public void DeleteNotice(NoticeType noticeType, int days)
        {
            throw new NotImplementedException();
        }

        public int GetAnnouncePrivateMessageCount()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAnnouncePrivateMessageList(int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNavigationData(bool getAllNavigation)
        {
            throw new NotImplementedException();
        }

        public void InsertNavigation(NavInfo nav)
        {
            throw new NotImplementedException();
        }

        public void UpdateNavigation(NavInfo nav)
        {
            throw new NotImplementedException();
        }

        public void DeleteNavigation(int id)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNavigationHasSub()
        {
            throw new NotImplementedException();
        }

        public void UpdateUserShortInfo(string location, string bio, string signature, int uid)
        {
            throw new NotImplementedException();
        }

        public void AddBannedIp(IpInfo info)
        {
            throw new NotImplementedException();
        }

        public int GetBannedIpCount()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetBannedIpList()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetBannedIpList(int num, int pageId)
        {
            throw new NotImplementedException();
        }

        public void UpdateBanUser(int groupId, string groupExpiry, int uid)
        {
            throw new NotImplementedException();
        }

        public int DeleteBanIp(string bannedIdList)
        {
            throw new NotImplementedException();
        }

        public int UpdateBanIpExpiration(int id, string endTime)
        {
            throw new NotImplementedException();
        }

        public int UpdateAnnouncementDisplayOrder(int displayOrder, int aId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumTableBySpecialUser(string userName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumTableWithSpecialUser(int fid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable SearchSpecialUser(int fid)
        {
            throw new NotImplementedException();
        }

        public void UpdateSpecialUser(string permUserList, int fid)
        {
            throw new NotImplementedException();
        }

        public int UpdateNewPms(int olId, int plusCount)
        {
            throw new NotImplementedException();
        }

        public int UpdateNewNotices(int olId, int plusCount)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttentionTopics(string fidList, int tpp, int pageId, string keyWord)
        {
            throw new NotImplementedException();
        }

        public int GetAttentionTopicCount(string fidList, string keyWord)
        {
            throw new NotImplementedException();
        }

        public void UpdateTopicAttentionByTidList(string tidList, int attention)
        {
            throw new NotImplementedException();
        }

        public void UpdateTopicAttentionByFidList(string fidList, int attention, string dateTime)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNoUsedAttachmentListByUid(int userId, string posttime, int isimage)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetEditPostAttachList(int userid, string aidList)
        {
            throw new NotImplementedException();
        }

        public int DeleteNoUsedForumAttachment()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNoUsedForumAttachment()
        {
            throw new NotImplementedException();
        }

        public int ReNewNotice(int type, int uid)
        {
            throw new NotImplementedException();
        }

        public int CreateAttachPaymetLog(AttachPaymentlogInfo attachPaymentLogInfo)
        {
            throw new NotImplementedException();
        }

        public int UpdateAttachPaymetLog(AttachPaymentlogInfo attachPaymentLogInfo)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachPaymentLogByAid(int aid)
        {
            throw new NotImplementedException();
        }

        public int GetUserExtCreditsByUserid(int uid, int extNumber)
        {
            throw new NotImplementedException();
        }

        public bool HasBoughtAttach(int userId, int aid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachPaymentLogByUid(string attachIdList, int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetNewNotices(int userId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicsByCondition(string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetHelpList()
        {
            throw new NotImplementedException();
        }

        public void UpdateHelp(int id, string title, string message, int pid, int orderBy)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetHelpTypes()
        {
            throw new NotImplementedException();
        }

        public void UpdateOrder(string orderBy, string id)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAdminGroups()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicTypes(string searthKeyWord)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMailTable(string uids)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetHotTopicsList(int pageSize, int pageIndex, int fid, string showType, int timeBetween)
        {
            throw new NotImplementedException();
        }

        public int GetHotTopicsCount(int fid, int timeBetween)
        {
            throw new NotImplementedException();
        }

        public void SetPostsBanned(string tableId, string postListId, int invisible)
        {
            throw new NotImplementedException();
        }

        public string ShowForumCondition(int sqlId, int cond)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.AuditNewUserClear(string searchUser, string regBefore, string regIp)
        {
            throw new NotImplementedException();
        }

        System.Data.DataTable IDataProvider.PostGridBind(string postTableName, string condition)
        {
            throw new NotImplementedException();
        }

        public string GetDataBaseVersion()
        {
            throw new NotImplementedException();
        }

        public int GetUnauditNewTopicCount(string fidList, int filter)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUnauditNewPost(string fidList, int ppp, int pageId, int tableId, int filter)
        {
            throw new NotImplementedException();
        }

        public int GetUnauditNewPostCount(string fidList, int tableId, int filter)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPagedLastPostList(PostpramsInfo postParmsInfo, string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAnnouncementsByCondition(string condition)
        {
            throw new NotImplementedException();
        }

        public int GetNoticeCount(int userId, int state)
        {
            throw new NotImplementedException();
        }

        public bool CheckForumRewriteNameExists(string rewriteName)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetSinglePost(int tid, string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable Global_UserGrid(string searchCondition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetLastPostNotInPidList(string postIdList, int topicId, int postId)
        {
            throw new NotImplementedException();
        }

        public int CreateInviteCode(InviteCodeInfo inviteCode)
        {
            throw new NotImplementedException();
        }

        public bool IsInviteCodeExist(string code)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetInviteCodeById(int inviteId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetInviteCodeByUid(int userId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetInviteCodeByCode(string code)
        {
            throw new NotImplementedException();
        }

        public void DeleteInviteCode(int inviteId)
        {
            throw new NotImplementedException();
        }

        public void UpdateInviteCodeSuccessCount(int inviteId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserInviteCodeList(int creatorId, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public int GetUserInviteCodeCount(int creatorId)
        {
            throw new NotImplementedException();
        }

        public int ClearExpireInviteCode()
        {
            throw new NotImplementedException();
        }

        public int GetTodayUserCreatedInviteCode(int creatorId)
        {
            throw new NotImplementedException();
        }

        public void ResetForumsPosts()
        {
            throw new NotImplementedException();
        }

        public void ResetLastPostInfo()
        {
            throw new NotImplementedException();
        }

        public void ResetLastRepliesInfoOfTopics(int postTableID)
        {
            throw new NotImplementedException();
        }

        public void UpdateMyPost(int lasttableid)
        {
            throw new NotImplementedException();
        }

        public void ResetForumsTopics()
        {
            throw new NotImplementedException();
        }

        public void ResetTodayPosts()
        {
            throw new NotImplementedException();
        }

        public int CreateCreditOrder(CreditOrderInfo creditOrderInfo)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetCreditOrderList(int pageIndex, int status, int orderId, string tradeNo, string buyer, string submitStartTime, string submitLastTime, string confirmStartTime, string confirmLastTime)
        {
            throw new NotImplementedException();
        }

        public int GetCreditOrderCount(int status, int orderId, string tradeNo, string buyer, string submitStartTime, string submitLastTime, string confirmStartTime, string confirmLastTime)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetCreditOrderByOrderCode(string orderCode)
        {
            throw new NotImplementedException();
        }

        public int UpdateCreditOrderInfo(int orderId, string tradeNo, int orderStatus, string confirmedTime)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserPostCountList(int topNumber, DateType dateType, int dateNum, string postTableName)
        {
            throw new NotImplementedException();
        }

        public bool CreateUpdateUserCreditsProcedure(string creditExpression, bool testCreditExpression)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicsByViewsOrReplies(int fid, int pageSize, int pageIndex, int startNumber, string condition, string orderFields, int sortType)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPostRateLogList(int pid, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int GetPostRateLogCount(int pid)
        {
            throw new NotImplementedException();
        }

        public int GetForumsLastPostTid(string fidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserListByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserInfoByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public int UpdateUserFavoriteViewTime(int uid, int tid)
        {
            throw new NotImplementedException();
        }

        public void UpdateTrendStat(TrendType trendType)
        {
            throw new NotImplementedException();
        }
    }
}
