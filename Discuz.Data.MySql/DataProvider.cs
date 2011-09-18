using System;
using System.Collections.Generic;
using System.Text;
using Discuz.Common;
using Discuz.Config;
using Discuz.Data;
using Discuz.Entity;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;

namespace Discuz.Data.MySql
{
    public partial class DataProvider : IDataProvider
    {

        public void AddAdInfo(int available, string type, int displayOrder, string title, string targets, string parameters, string code, string startTime, string endTime)
        {
            DbParameter[] parms = { 
                                      DbHelper.MakeInParam("?available", DbType.Boolean, 4, available), 
				DbHelper.MakeInParam("?type", (DbType)MySqlDbType.VarChar, 50, type), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayOrder), 
				DbHelper.MakeInParam("?title", (DbType)MySqlDbType.VarChar, 50, title), 
				DbHelper.MakeInParam("?targets", (DbType)MySqlDbType.VarChar, 255, targets), 
				DbHelper.MakeInParam("?parameters", (DbType)MySqlDbType.VarChar, 0, parameters), 
				DbHelper.MakeInParam("?code", (DbType)MySqlDbType.VarChar, 0, code), 
				DbHelper.MakeInParam("?starttime", DbType.Int64, 8, startTime), 
				DbHelper.MakeInParam("?endtime", DbType.Int64, 8, endTime),
                                    };

            string commandText = string.Format("INSERT INTO  `{0}advertisements` (`available`,`type`,`displayorder`,`title`,`targets`,`parameters`,`code`,`starttime`,`endtime`) VALUES(?available,?type,?displayorder,?title,?targets,?parameters,?code,?starttime,?endtime)",
                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void AddAttchType(string extension, string maxSize)
        {
            throw new NotImplementedException();
        }

        public void AddBBCCode(int available, string tag, string icon, string replacement, string example, string explanation, string param, string nest, string paramsDescript, string paramsDefvalue)
        {
            throw new NotImplementedException();
        }

        public int AddCreditsLog(int uid, int fromTo, int sendCredits, int receiveCredits, float send, float receive, string payDate, int operation)
        {
            throw new NotImplementedException();
        }

        public int AddErrLoginCount(string ip)
        {
            throw new NotImplementedException();
        }

        public int AddErrLoginRecord(string ip)
        {
            throw new NotImplementedException();
        }

        public int AddForumLink(int displayOrder, string name, string url, string note, string logo)
        {
            throw new NotImplementedException();
        }

        public void AddMedalslog(int adminId, string adminName, string ip, string userName, int uid, string actions, int medals, string reason)
        {
            throw new NotImplementedException();
        }

        public void AddModerator(int uid, int fid, int displayOrder, int inherited)
        {
            throw new NotImplementedException();
        }

        public void AddOnlineList(string grouptitle)
        {
            throw new NotImplementedException();
        }

        public void AddPostTableToTableList(string description, int minTid, int maxTid)
        {
            throw new NotImplementedException();
        }

        public void AddSmiles(int id, int displayOrder, int type, string code, string url)
        {
            throw new NotImplementedException();
        }

        public int AddTemplate(string templateName, string directory, string copyRight)
        {
            throw new NotImplementedException();
        }

        public void AddTemplate(string name, string directory, string copyRight, string author, string createDate, string ver, string forDntVer)
        {
            throw new NotImplementedException();
        }

        public void AddVisitLog(int uid, string userName, int groupId, string groupTitle, string ip, string actions, string others)
        {
            throw new NotImplementedException();
        }

        public int AddWord(string userName, string find, string replacement)
        {
            throw new NotImplementedException();
        }

        public string BackUpDatabase(string backUpPath, string serverName, string userName, string passWord, string strDbName, string strFileName)
        {
            throw new NotImplementedException();
        }

        public bool BatchSetForumInf(ForumInfo forumInfo, BatchSetParams bsp, string fidList)
        {
            throw new NotImplementedException();
        }

        public void BuyTopic(int uid, int tid, int posterId, int price, float netAmount, int creditsTrans)
        {
            throw new NotImplementedException();
        }

        public void ChangeUsergroup(int sourceUserGroupId, int targetUserGroupId)
        {
            throw new NotImplementedException();
        }

        public void ChangeUserGroupByUid(int groupId, string uidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader CheckEmailAndSecques(string userName, string email, string secques)
        {
            throw new NotImplementedException();
        }

        public int CheckFavoritesIsIN(int uid, int tid, byte type)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader CheckPassword(string userName, string passWord, bool originalPassWord)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader CheckPassword(int uid, string passWord, bool originalPassword)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader CheckPasswordAndSecques(string userName, string passWord, bool originalPassword, string secques)
        {
            throw new NotImplementedException();
        }

        public string CheckRateState(int userId, string pid)
        {
            throw new NotImplementedException();
        }

        public bool CheckUserCreditsIsEnough(int uid, float[] values)
        {
            throw new NotImplementedException();
        }

        public bool CheckUserVerifyCode(int olId, string verifyCode, string newverifyCode)
        {
            throw new NotImplementedException();
        }

        public void ClearAuthstrByUidlist(string uidList)
        {
            throw new NotImplementedException();
        }

        public void ClearDBLog(string dbName)
        {
            throw new NotImplementedException();
        }

        public void ClearPosts(int uid)
        {
            throw new NotImplementedException();
        }

        public void CombinationForums(string sourceFid, string targetFid, string fidList)
        {
            throw new NotImplementedException();
        }

        public void CombinationUser(string postTableName, UserInfo targetUserInfo, UserInfo srcUserInfo)
        {
            throw new NotImplementedException();
        }

        public void ConfirmFullTextEnable()
        {
            throw new NotImplementedException();
        }

        public int CopyTopicLink(int oldFid, string topicList)
        {
            throw new NotImplementedException();
        }

        public int CreateAdminGroupInfo(AdminGroupInfo adminGroupsInfo)
        {
            throw new NotImplementedException();
        }

        public int CreateAttachment(AttachmentInfo attachmentInfo)
        {
            throw new NotImplementedException();
        }

        public int CreateFavorites(int uid, int tid, byte type)
        {
            throw new NotImplementedException();
        }

        public int CreateFullTextIndex(string dbName)
        {
            throw new NotImplementedException();
        }

        public int CreateOnlineTable()
        {
            throw new NotImplementedException();
        }

        public bool CreateORFillIndex(string dbName, string postsId)
        {
            throw new NotImplementedException();
        }

        public int CreatePoll(PollInfo pollInfo)
        {
            throw new NotImplementedException();
        }

        public int CreatePollOption(PollOptionInfo pollOptionInfo)
        {
            throw new NotImplementedException();
        }

        public bool UpdatePollOption(PollOptionInfo pollOptionInfo)
        {
            throw new NotImplementedException();
        }

        public bool DeletePollOption(PollOptionInfo pollOptionInfo)
        {
            throw new NotImplementedException();
        }

        public int CreatePost(PostInfo postInfo, string postTableId)
        {
            throw new NotImplementedException();
        }

        public void CreatePostProcedure(string sqlTemplate)
        {
            throw new NotImplementedException();
        }

        public void CreatePostTableAndIndex(string tableName)
        {
            throw new NotImplementedException();
        }

        public int CreatePrivateMessage(PrivateMessageInfo privateMessageInfo, int saveToSentBox)
        {
            throw new NotImplementedException();
        }

        public int CreateSearchCache(SearchCacheInfo cacheInfo)
        {
            throw new NotImplementedException();
        }

        public int CreateTopic(TopicInfo topicInfo)
        {
            throw new NotImplementedException();
        }

        public int CreateUser(UserInfo userinfo)
        {
            throw new NotImplementedException();
        }

        public int DecreaseNewPMCount(int uid, int subVal)
        {
            throw new NotImplementedException();
        }

        public int DeleteAdminGroupInfo(short adminGid)
        {
            throw new NotImplementedException();
        }

        public void DeleteAdvertisement(string aidList)
        {
            throw new NotImplementedException();
        }

        public int DeleteAttachment(string aidList)
        {
            throw new NotImplementedException();
        }

        public int DeleteAttachment(int aId)
        {
            throw new NotImplementedException();
        }

        public int DeleteAttachmentByTid(int tid)
        {
            throw new NotImplementedException();
        }

        public int DeleteAttachmentByTid(string tidList)
        {
            throw new NotImplementedException();
        }

        public void DeleteAttchType(string attchTypeIdList)
        {
            throw new NotImplementedException();
        }

        public void DeleteBBCode(string idList)
        {
            throw new NotImplementedException();
        }

        public int DeleteClosedTopics(int fid, string topicList)
        {
            throw new NotImplementedException();
        }

        public int DeleteErrLoginRecord(string ip)
        {
            throw new NotImplementedException();
        }

        public void DeleteExpriedSearchCache()
        {
            throw new NotImplementedException();
        }

        public int DeleteFavorites(int uid, string fidList, byte type)
        {
            throw new NotImplementedException();
        }

        public int DeleteForumLink(string forumLinkIdList)
        {
            throw new NotImplementedException();
        }

        public void DeleteForumsByFid(string postName, string fid)
        {
            throw new NotImplementedException();
        }

        public bool DeleteMedalLog(string condition)
        {
            throw new NotImplementedException();
        }

        public bool DeleteMedalLog()
        {
            throw new NotImplementedException();
        }

        public void DeleteModerator(int uid)
        {
            throw new NotImplementedException();
        }

        public void DeleteModeratorByFid(int fid)
        {
            throw new NotImplementedException();
        }

        public bool DeleteModeratorLog(string condition)
        {
            throw new NotImplementedException();
        }

        public void DeleteOnlineList(int groupId)
        {
            throw new NotImplementedException();
        }

        public bool DeletePaymentLog(string condition)
        {
            throw new NotImplementedException();
        }

        public bool DeletePaymentLog()
        {
            throw new NotImplementedException();
        }

        public int DeletePost(string postTableId, int pid, bool changePosts)
        {
            throw new NotImplementedException();
        }

        public void DeletePostByPosterid(int tableId, int posterId)
        {
            throw new NotImplementedException();
        }

        public string DeletePrivateMessages(bool isnew, string postDateTime, string msgFromList, bool lowerUpper, string subject, string message, bool isUpdateUserNewPm)
        {
            throw new NotImplementedException();
        }

        public int DeletePrivateMessages(int userId, string pmIdList)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRateLog()
        {
            throw new NotImplementedException();
        }

        public bool DeleteRateLog(string condition)
        {
            throw new NotImplementedException();
        }

        public int DeleteRows(int olId)
        {
            throw new NotImplementedException();
        }

        public int DeleteRowsByIP(string ip)
        {
            throw new NotImplementedException();
        }

        public int DeleteSmilies(string idList)
        {
            throw new NotImplementedException();
        }

        public void DeleteTemplateItem(string templateIdList)
        {
            throw new NotImplementedException();
        }

        public void DeleteTemplateItem(int templateId)
        {
            throw new NotImplementedException();
        }

        public int DeleteTopic(int tid)
        {
            throw new NotImplementedException();
        }

        public void DeleteTopicByPosterid(int posterId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTopicByTid(int tid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public int DeleteTopicByTidList(string topicList, string postTableId, bool changePosts)
        {
            throw new NotImplementedException();
        }

        public void DeleteUserByUidlist(string uidList)
        {
            throw new NotImplementedException();
        }

        public void DeleteUserGroupInfo(int groupId)
        {
            throw new NotImplementedException();
        }

        public void DeleteVisitLogs()
        {
            throw new NotImplementedException();
        }

        public void DeleteVisitLogs(string condition)
        {
            throw new NotImplementedException();
        }

        public int DeleteWords(string idList)
        {
            throw new NotImplementedException();
        }

        public bool DelUserAllInf(int uid, bool delPosts, bool delPms)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader FindUserEmail(string email)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAdminGroupList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAdsTable()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAdvertisement(int aid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAllForumStatistics()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataSet GetAllPostTableName()
        {
            throw new NotImplementedException();
        }

        public int GetTopicCount(int fid, int state, string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetArchiverForumIndexList()
        {
            throw new NotImplementedException();
        }

        public int GetAttachmentCountByPid(int pid)
        {
            throw new NotImplementedException();
        }

        public int GetAttachmentCountByTid(int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentInfo(int aid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentList(string aidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentListByPid(string pidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAttachmentListByPid(int pid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentListByTid(string tidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentListByTid(int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAttachmentType()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetBanWordList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetBBCode(int id)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetBBCode()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetCreditsLogList(int pageSize, int currentPage, int uid)
        {
            throw new NotImplementedException();
        }

        public int GetCreditsLogRecordCount(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetCustomEditButtonList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataRowCollection GetDatechTableIds()
        {
            throw new NotImplementedException();
        }

        public string GetDbName()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetErrLoginRecordByIP(string ip)
        {
            throw new NotImplementedException();
        }

        public int GetFavoritesCount(int uid, int typeId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetFavoritesList(int uid, int pageSize, int pageIndex, int typeId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetFirstImageAttachByTid(int tid)
        {
            throw new NotImplementedException();
        }

        public int GetFirstPostId(int tid, string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumByParentid(int parentId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumField(int fid, string fieldName)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumIndexList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumIndexListTable()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumLastPost(int fid, string postTableName, int topicCount, int postCount, int lastTid, string lastTitle, string lastPost, int lastPosterId, string lastPoster, int todayPostCount)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumLinkList()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumOnlineUserList(int forumId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForums(int startFid, int endFid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumsMaxDisplayOrder(int parentId)
        {
            throw new NotImplementedException();
        }

        public int GetForumsMaxDisplayOrder()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumsTable()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumStatistics(int fid)
        {
            throw new NotImplementedException();
        }

        public int GetGroupCountByCreditsLower(int creditsHigher)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetLastPostByFid(int fid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetLastPostByTid(int tid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetLastPostList(PostpramsInfo postParmsInfo, string postTableId)
        {
            throw new NotImplementedException();
        }

        public bool GetLastUserInfo(out string lastUserId, out string lastUserName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMainForum()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMainPostByTid(string postTableName, int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetMaxAndMinTid(int fid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMaxCreditLower()
        {
            throw new NotImplementedException();
        }

        public int GetMaxForumId()
        {
            throw new NotImplementedException();
        }

        public int GetMaxPostTableTid(string postTableName)
        {
            throw new NotImplementedException();
        }

        public int GetMaxSmiliesId()
        {
            throw new NotImplementedException();
        }

        public int GetMaxTableListId()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMaxTid()
        {
            throw new NotImplementedException();
        }

        public int GetMaxUserGroupId()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMedal()
        {
            throw new NotImplementedException();
        }

        public string GetMedalSql()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMedalLogList(int pageSize, int currentPage, string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMedalLogList(int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        public int GetMedalLogListCount()
        {
            throw new NotImplementedException();
        }

        public int GetMedalLogListCount(string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMedalsList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetMinCreditHigher()
        {
            throw new NotImplementedException();
        }

        public int GetMinPostTableTid(string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetModeratorInfo(string moderator)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetModeratorList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetModeratorLogList(int pageSize, int currentPage, string condition)
        {
            throw new NotImplementedException();
        }

        public int GetModeratorLogListCount(string condition)
        {
            throw new NotImplementedException();
        }

        public int GetModeratorLogListCount()
        {
            throw new NotImplementedException();
        }

        public int GetNewPMCount(int userId)
        {
            throw new NotImplementedException();
        }

        public int GetOnlineAllUserCount()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetOnlineGroupIconList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetOnlineGroupIconTable()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetOnlineUser(int userId, string passWord)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetOnlineUserByIP(int userId, string ip)
        {
            throw new NotImplementedException();
        }

        public int GetOnlineUserCount()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetOnlineUserList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetOnlineUserListTable()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetParentIdByFid(int fid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPayLogInList(int pageSize, int currentPage, int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPayLogOutList(int pageSize, int currentPage, int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPaymentLogByTid(int pageSize, int currentPage, int tid)
        {
            throw new NotImplementedException();
        }

        public int GetPaymentLogByTidCount(int tid)
        {
            throw new NotImplementedException();
        }

        public int GetPaymentLogInRecordCount(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPaymentLogList(int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPaymentLogList(int pageSize, int currentPage, string condition)
        {
            throw new NotImplementedException();
        }

        public int GetPaymentLogListCount()
        {
            throw new NotImplementedException();
        }

        public int GetPaymentLogListCount(string condition)
        {
            throw new NotImplementedException();
        }

        public int GetPaymentLogOutRecordCount(int uid)
        {
            throw new NotImplementedException();
        }

        public string GetPollEnddatetime(int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPollList(int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPollOptionList(int tid)
        {
            throw new NotImplementedException();
        }

        public int GetPollType(int tid)
        {
            throw new NotImplementedException();
        }

        public string GetPollUserNameList(int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPost(string postTableName, int pid)
        {
            throw new NotImplementedException();
        }

        public int GetPostCount(int fid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public int GetPostCountByTid(int tid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public int GetPostCount(string postTableId, int tid, int posterId)
        {
            throw new NotImplementedException();
        }

        public int GetPostCount(string postTableName)
        {
            throw new NotImplementedException();
        }

        public int GetPostCountByUid(int uid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPostCountFromIndex(string postsId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPostCountTable(string postsId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPostInfo(string postTableId, int pid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPostLayer(int currentPostTableId, int postId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPostList(PostpramsInfo postParmsInfo, string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPostListByCondition(PostpramsInfo postParmsInfo, string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPostListTitle(int tid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataSet GetPosts(int pid, int pageSize, int pageIndex, string postTableId)
        {
            throw new NotImplementedException();
        }

        public int GetPostsCount(string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPostTableList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetPostTree(int tid, string postTableId)
        {
            throw new NotImplementedException();
        }

        public int GetPrivateMessageCount(int userId, int folder, int state)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPrivateMessageInfo(int pmId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPrivateMessageList(int userId, int folder, int pageSize, int pageIndex, int intType)
        {
            throw new NotImplementedException();
        }

        public int GetRateLogCount()
        {
            throw new NotImplementedException();
        }

        public int GetRateLogCount(string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSearchCache(int searchId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSearchDigestTopicsList(int pageSize, string strTids)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSearchPostsTopicsList(int pageSize, string strTids, string posTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSearchTopicsList(int pageSize, string strTids)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetShortForums()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetShortUserInfoToReader(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetSinglePost(out System.Data.IDataReader attachments, PostpramsInfo postParmsInfo, string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetSmiliesList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSmiliesListDataTable()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSmiliesListWithoutType()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSmilieTypes()
        {
            throw new NotImplementedException();
        }

        public string GetSpecialTableFullIndexSQL(string tableName, string dbName)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetSubForumReader(int fid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSubForumTable(int fid)
        {
            throw new NotImplementedException();
        }

        public string GetSystemGroupInfoSql()
        {
            throw new NotImplementedException();
        }

        public int GetTodayPostCount(int fid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public int GetTodayPostCountByUid(int uid, string postTableName)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopForumFids(int lastFid, int statCount)
        {
            throw new NotImplementedException();
        }

        public int GetTopicCount(int fid)
        {
            throw new NotImplementedException();
        }

        public int GetTopicCount(string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicFidByTid(string tidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicInfo(int tid, int fid, byte mode)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicList(string topicList, int displayOrder)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicList(int forumId, int pageId, int tpp)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicListModeratorLog(int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopics(int startTid, int endTid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopics(int fid, int pageSize, int pageIndex, int startNum, string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicsByDate(int fid, int pageSize, int pageIndex, int startNumber, string condition, string orderFields, int sortType)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicsByReplyUserId(int userId, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicsByType(int pageSize, int pageIndex, int startNum, string condition, int ascDesc)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicsByTypeDate(int pageSize, int pageIndex, int startNum, string condition, string orderBy, int ascDesc)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicsByUserId(int userId, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int GetTopicStatus(string topicList, string field)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicTidByFid(string tidList, int fid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicTids(int statCount, int lastTid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicTypeList()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopTopics(int fid, int pageSize, int pageIndex, string tids)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopUsers(int statCount, int lastUid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUidAdminIdByUsername(string userName)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUidInModeratorsByUid(int currentFid, int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUidModeratorByFid(string fidList)
        {
            throw new NotImplementedException();
        }

        public int GetUploadFileSizeByUserId(int uid)
        {
            throw new NotImplementedException();
        }

        public int GetUserCount()
        {
            throw new NotImplementedException();
        }

        public int GetUserCountByAdmin()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserEmailByGroupid(string groupIdList)
        {
            throw new NotImplementedException();
        }

        public float GetUserExtCredits(int uid, int exTid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupByCreditshigher(int creditsHigher)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupByCreditsHigherAndLower(int creditsHigher, int creditsLower)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupCreditsLowerAndHigher(int groupId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupExceptGroupid(int groupId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupInfoByGroupid(int groupId)
        {
            throw new NotImplementedException();
        }

        public string GetUserGroupRAdminId(int groupId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupRateRange(int groupId)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroups()
        {
            throw new NotImplementedException();
        }

        public string GetUserGroupsStr()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupsTitle()
        {
            throw new NotImplementedException();
        }

        public string GetUserGroupTitle()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupWithOutGuestTitle()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserIdByAuthStr(string authStr)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserInfo(string userName, string passWord)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserInfo(int userId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserInfoByIP(string ip)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserInfoToReader(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserJoinDate(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserList(int pageSize, int pageIndex, string column, string orderType)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserList(int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserListWithPostList(string postList, string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserListWithTopicList(string topicList, int lossLessDel)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserListWithTopicList(string topicList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserName(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserNameByUid(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUsers(int startUid, int endUid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserTodayRate(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetValidTemplateIDList()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetValidTemplateList()
        {
            throw new NotImplementedException();
        }

        public int GetVisitLogCount(string condition)
        {
            throw new NotImplementedException();
        }

        public int GetVisitLogCount()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetVisitLogList(int pageSize, int currentPage, string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetVisitLogList(int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        public int InsertForumsInf(ForumInfo forumInfo)
        {
            throw new NotImplementedException();
        }

        public void InsertForumsModerators(string fid, string moderators, int displayOrder, int inherited)
        {
            throw new NotImplementedException();
        }

        public bool InsertModeratorLog(string moderatorUid, string moderatorName, string groupId, string groupTitle, string ip, string postDateTime, string fid, string fName, string tid, string title, string actions, string reason)
        {
            throw new NotImplementedException();
        }

        public int InsertRateLog(int pid, int userId, string userName, int extId, float score, string reason)
        {
            throw new NotImplementedException();
        }

        public bool IsBuyer(int tid, int uid)
        {
            throw new NotImplementedException();
        }

        public bool IsExistMedalAwardRecord(int medalId, int userId)
        {
            throw new NotImplementedException();
        }

        public bool IsExistSubForum(int fid)
        {
            throw new NotImplementedException();
        }

        public bool IsReplier(int tid, int uid, string postTableId)
        {
            throw new NotImplementedException();
        }

        public bool IsSystemOrTemplateUserGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public void MovingForumsPos(string currentFid, string targetFid, bool isAsChildNode, string extName)
        {
            throw new NotImplementedException();
        }

        public void PassAuditNewTopic(string postTableName, string tidList)
        {
            throw new NotImplementedException();
        }

        public void PassPost(int currentPostTableId, string pidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable RateLogList(int pageSize, int currentPage, string postTableName, string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable RateLogList(int pageSize, int currentPage, string postTableName)
        {
            throw new NotImplementedException();
        }

        public int RepairTopics(string topicList, string postTable)
        {
            throw new NotImplementedException();
        }

        public void ReSetClearMove()
        {
            throw new NotImplementedException();
        }

        public int ResetErrLoginCount(string ip)
        {
            throw new NotImplementedException();
        }

        public void ReSetStatistic(int userCount, int topicsCount, int postCount, string lastUserId, string lastUserName)
        {
            throw new NotImplementedException();
        }

        public int ResetTopicTypes(int topicTypeId, string topicList)
        {
            throw new NotImplementedException();
        }

        public string RestoreDatabase(string backUpPath, string serverName, string userName, string passWord, string strDbName, string strFileName)
        {
            throw new NotImplementedException();
        }

        public string RunSql(string sql)
        {
            throw new NotImplementedException();
        }

        public void SaveForumsInfo(ForumInfo forumInfo)
        {
            throw new NotImplementedException();
        }

        public string SearchAttachment(int forumId, string postTableName, string fileSizeMin, string fileSizeMax, string downLoadsMin, string downLoadsMax, string postDateTime, string fileName, string description, string poster)
        {
            throw new NotImplementedException();
        }

        public string SearchMedalLog(DateTime postDateTimeStart, DateTime postDateTimeEnd, string userName, string reason)
        {
            throw new NotImplementedException();
        }

        public string SearchModeratorManageLog(DateTime postDateTimeStart, DateTime postDateTimeEnd, string userName, string others)
        {
            throw new NotImplementedException();
        }

        public string SearchPaymentLog(DateTime postDateTimeStart, DateTime postDateTimeEnd, string userName)
        {
            throw new NotImplementedException();
        }

        public string SearchPost(int forumId, string posttableiId, DateTime postDateTimeStart, DateTime postDateTimeEnd, string poster, bool lowerUpper, string ip, string message)
        {
            throw new NotImplementedException();
        }

        public string SearchRateLog(DateTime postDateTimeStart, DateTime postDateTimeEnd, string userName, string others)
        {
            throw new NotImplementedException();
        }

        public string SearchTopicAudit(int fid, string poster, string title, string moderatorName, DateTime postDateTimeStart, DateTime postDateTimeEnd, DateTime delDateTimeStart, DateTime delDateTimeEnd)
        {
            throw new NotImplementedException();
        }

        public string SearchTopics(int forumId, string keyWord, string display0rder, string digest, string attachment, string poster, bool lowerUpper, string viewsMin, string viewsMax, string repliesMax, string repliesMin, string rate, string lastPost, DateTime postDateTimeStart, DateTime postDateTimeEnd)
        {
            throw new NotImplementedException();
        }

        public string SearchVisitLog(DateTime postDateTimeStart, DateTime postDateTimeEnd, string userName, string others)
        {
            throw new NotImplementedException();
        }

        public int SetAdminGroupInfo(AdminGroupInfo adminGroupsInfo)
        {
            throw new NotImplementedException();
        }

        public void SetAvailableForMedal(int available, string medalIdList)
        {
            throw new NotImplementedException();
        }

        public void SetBBCodeAvailableStatus(string idList, int status)
        {
            throw new NotImplementedException();
        }

        public bool SetDisplayorder(string topicList, int value)
        {
            throw new NotImplementedException();
        }

        public void SetModerator(string moderator)
        {
            throw new NotImplementedException();
        }

        public void SetPrimaryPost(string subject, int tid, string[] postId, string postTableId)
        {
            throw new NotImplementedException();
        }

        public int SetPrivateMessageState(int pmId, byte state)
        {
            throw new NotImplementedException();
        }

        public int SetRealCurrentTopics(int fid)
        {
            throw new NotImplementedException();
        }

        public void SetStatusInForum(int status, int fid)
        {
            throw new NotImplementedException();
        }

        public void SetStopTalkUser(string uidList)
        {
            throw new NotImplementedException();
        }

        public int SetTopicClose(string topicList, short intValue)
        {
            throw new NotImplementedException();
        }

        public int SetTopicStatus(string topicList, string field, string intValue)
        {
            throw new NotImplementedException();
        }

        public bool SetTypeid(string topicList, int value)
        {
            throw new NotImplementedException();
        }

        public int SetUserNewPMCount(int uid, int pmNum)
        {
            throw new NotImplementedException();
        }

        public int SetUserOnlineState(int uid, int onlineState)
        {
            throw new NotImplementedException();
        }

        public void ShrinkDataBase(string shrinkSize, string dbName)
        {
            throw new NotImplementedException();
        }

        public int StartFullIndex(string dbName)
        {
            throw new NotImplementedException();
        }

        public void TestFullTextIndex(int postTableId)
        {
            throw new NotImplementedException();
        }

        public void UpdateAction(int olId, int action, int fid, string forumName, int tid, string topicTitle)
        {
            throw new NotImplementedException();
        }

        public void UpdateAction(int olId, int action, int inid)
        {
            throw new NotImplementedException();
        }

        public int UpdateAdvertisement(int aid, int available, string type, int displayOrder, string title, string targets, string parameters, string code, string startTime, string endTime)
        {
            throw new NotImplementedException();
        }

        public int UpdateAdvertisementAvailable(string aidList, int available)
        {
            throw new NotImplementedException();
        }

        public void UpdateAnnouncementPoster(int posterId, string poster)
        {
            throw new NotImplementedException();
        }

        public int UpdateAttachment(AttachmentInfo attachmentInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateAttachmentDownloads(int aid)
        {
            throw new NotImplementedException();
        }

        public int UpdateAttachmentTidToAnotherTopic(int oldTid, int newTid)
        {
            throw new NotImplementedException();
        }

        public void UpdateAttchType(string extension, string maxSize, int id)
        {
            throw new NotImplementedException();
        }

        public void UpdateAuthStr(int uid, string authStr, int authFlag)
        {
            throw new NotImplementedException();
        }

        public void UpdateBBCCode(int available, string tag, string icon, string replacement, string example, string explanation, string param, string nest, string paramsDescript, string paramsDefvalue, int id)
        {
            throw new NotImplementedException();
        }

        public int UpdateDetachTable(int fid, string description)
        {
            throw new NotImplementedException();
        }

        public void UpdateEmailValidateInfo(string authStr, DateTime authTime, int uid)
        {
            throw new NotImplementedException();
        }

        public void UpdateForum(int fid, int topicCount, int postCount, int lastTid, string lastTitle, string lastPost, int lastPosterId, string lastPoster, int todayPostCount)
        {
            throw new NotImplementedException();
        }

        public void UpdateForumAndUserTemplateId(string templateIdList)
        {
            throw new NotImplementedException();
        }

        public int UpdateForumField(int fid, string fieldName, string fieldValue)
        {
            throw new NotImplementedException();
        }

        public int UpdateForumLink(int id, int displayOrder, string name, string url, string note, string logo)
        {
            throw new NotImplementedException();
        }

        public void UpdateForumsDisplayOrder(int minDisplayOrder)
        {
            throw new NotImplementedException();
        }

        public void UpdateGroupid(int userId, int groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateInvisible(int olId, int invisible)
        {
            throw new NotImplementedException();
        }

        public void UpdateIP(int olId, string ip)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastTime(int olId)
        {
            throw new NotImplementedException();
        }

        public void UpdateMedal(int medalId, string name, string image)
        {
            throw new NotImplementedException();
        }

        public void UpdateMedals(int uid, string medals)
        {
            throw new NotImplementedException();
        }

        public void UpdateMedalslog(string newActions, DateTime postDateTime, string reason, string oldActions, int medals, int uid)
        {
            throw new NotImplementedException();
        }

        public void UpdateMedalslog(string actions, DateTime postDateTime, string reason, int uid)
        {
            throw new NotImplementedException();
        }

        public int UpdateMinMaxField(string postTableName, int postTableId)
        {
            throw new NotImplementedException();
        }

        public int UpdateOnlineList(int groupId, int displayOrder, string img, string title)
        {
            throw new NotImplementedException();
        }

        public void UpdateOnlineList(UserGroupInfo userGroupInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdatePassword(int olId, string passWord)
        {
            throw new NotImplementedException();
        }

        public bool UpdatePoll(PollInfo pollInfo)
        {
            throw new NotImplementedException();
        }

        public int UpdatePost(PostInfo postsInfo, string postTableId)
        {
            throw new NotImplementedException();
        }

        public void UpdatePost(string topicList, int fid, string postTable)
        {
            throw new NotImplementedException();
        }

        public int UpdatePostAttachment(int pid, string postTableId, int hasAttachment)
        {
            throw new NotImplementedException();
        }

        public int UpdatePostAttachmentType(int pid, string postTableId, int attType)
        {
            throw new NotImplementedException();
        }

        public void UpdatePostPMTime(int olId)
        {
            throw new NotImplementedException();
        }

        public int UpdatePostRate(int pid, float rate, string postTableId)
        {
            throw new NotImplementedException();
        }

        public int CancelPostRate(string postIdList, string postTableId)
        {
            throw new NotImplementedException();
        }

        public void UpdatePostTid(string postIdList, int tid, string postTableId)
        {
            throw new NotImplementedException();
        }

        public int UpdatePostTidToAnotherTopic(int oldTid, int newTid, string postTableId)
        {
            throw new NotImplementedException();
        }

        public void UpdatePostTime(int olId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRateRange(string rateRange, int groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRaterangeByGroupid(string rateRange, int groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateSearchTime(int olId)
        {
            throw new NotImplementedException();
        }

        public int UpdateStatistics(string param, string strValue)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatusByFidlist(string fidList)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatusByFidlistOther(string fidList)
        {
            throw new NotImplementedException();
        }

        public void UpdateSubForumCount(int subForumCount, int fid)
        {
            throw new NotImplementedException();
        }

        public void UpdateSubForumCount(int fid)
        {
            throw new NotImplementedException();
        }

        public void UpdateTopic(int tid, int postCount, int lastPostId, string lastPost, int lastPosterId, string poster)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopic(TopicInfo topicInfo)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopicAttachment(int tid, int hasAttachment)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopicAttachmentType(int tid, int attType)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopicHide(int tid)
        {
            throw new NotImplementedException();
        }

        public void UpdateTopicLastPoster(int lastPosterId, string lastPoster)
        {
            throw new NotImplementedException();
        }

        public void UpdateTopicLastPosterId(int tid)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopicModerated(string topicList, int moderated)
        {
            throw new NotImplementedException();
        }

        public void UpdateTopicPoster(int posterId, string poster)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopicViewCount(int tid, int viewCount)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserPreference(int uid, string avatar, int avatarWidth, int avatarHeight, int templateId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserCredits(int uid, float[] values)
        {
            throw new NotImplementedException();
        }

        public int UpdateUserDigest(string userIdList)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserExtCredits(int uid, int extId, float pos)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserForumSetting(UserInfo userInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserGroupCreidtsLower(int currentCreditsHigher, int creditsHigher)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserGroupLowerAndHigherToLimit(int groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserGroupsCreditsHigherByCreditsHigher(int creditsHigher, int creditsLower)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserGroupsCreditsLowerByCreditsLower(int creditsLower, int creditsHigher)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserLastvisit(int uid, string ip)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserOnlineInfo(int groupId, int userId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserOnlineStateAndLastActivity(string uidList, int onlineState, string activityTime)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserOnlineStateAndLastActivity(int uid, int onlineState, string activityTime)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserOnlineStateAndLastVisit(int uid, int onlineState, string activityTime)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserOnlineStateAndLastVisit(string uidList, int onlineState, string activityTime)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserLastActivity(int uid, string activityTime)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserOtherInfo(int groupId, int userId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserPassword(int uid, string passWord, bool originalPassWord)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserPostCount(int postCount, int userId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProfile(UserInfo userInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserSecques(int uid, string secques)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserSpaceId(int userId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserSpaceId(int spaceId, int userId)
        {
            throw new NotImplementedException();
        }

        public int UpdateWord(int id, string find, string replacement)
        {
            throw new NotImplementedException();
        }

        public void UpdateBadWords(string find, string replacement)
        {
            throw new NotImplementedException();
        }

        public void CreateTopicTags(string tags, int topicId, int userId, string curdateTime)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTagsListByTopic(int topicId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetHotTagsListForForum(int count)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTagInfo(int tagId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicListByTag(int tagId, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetRelatedTopics(int topicId, int count)
        {
            throw new NotImplementedException();
        }

        public int GetTopicsCountByTag(int tagId)
        {
            throw new NotImplementedException();
        }

        public void SetLastExecuteScheduledEventDateTime(string key, string serverName, DateTime lastExecuted)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastExecuteScheduledEventDateTime(string key, string serverName)
        {
            throw new NotImplementedException();
        }

        public void NeatenRelateTopics()
        {
            throw new NotImplementedException();
        }

        public void DeleteTopicTags(int topicId)
        {
            throw new NotImplementedException();
        }

        public void DeleteRelatedTopics(int topicId)
        {
            throw new NotImplementedException();
        }

        public void UpdateYesterdayPosts(string postTableId)
        {
            throw new NotImplementedException();
        }

        public void UpdateForumTags(int tagId, int orderId, string color)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetOpenForumList()
        {
            throw new NotImplementedException();
        }

        public void AddBonusLog(int tid, int authorId, int winerId, string winnerName, int postId, int bonus, int extId, int isBest)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicBonusLogs(int tid)
        {
            throw new NotImplementedException();
        }

        public void UpdateStats(string type, string variable, int count)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatVars(string type, string variable, string value)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAllStats()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAllStatVars()
        {
            throw new NotImplementedException();
        }

        public void DeleteOldDayposts()
        {
            throw new NotImplementedException();
        }

        public int GetForumCount()
        {
            throw new NotImplementedException();
        }

        public int GetTodayPostCount(string postTableId)
        {
            throw new NotImplementedException();
        }

        public int GetTodayNewMemberCount()
        {
            throw new NotImplementedException();
        }

        public int GetAdminCount()
        {
            throw new NotImplementedException();
        }

        public int GetNonPostMemCount()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetBestMember(string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetMonthPostsStats(string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetDayPostsStats(string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetHotTopics(int count)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetHotReplyTopics(int count)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumsByTopicCount(int count)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumsByPostCount(int count)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumsByMonthPostCount(int count, string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetForumsByDayPostCount(int count, string postTableId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserByOnlineTime(string filed)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicBonusLogsByPost(int tid)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatCount(string browser, string os, string visitorsAdd)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAllOpenForum()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetDebateTopic(int tid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetHotDebatesList(string hotField, int defHotCount, int getCount)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetRecommendDebates(string tidList)
        {
            throw new NotImplementedException();
        }

        public void CreateDebatePostExpand(DebatePostExpandInfo dpei)
        {
            throw new NotImplementedException();
        }

        public void AddCommentDabetas(int tid, int tableId, string commentMsg)
        {
            throw new NotImplementedException();
        }

        public void AddDebateDigg(int tid, int pid, int field, string ip, UserInfo userInfo)
        {
            throw new NotImplementedException();
        }

        public bool AllowDiggs(int pid, int userId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetDebatePostList(int tid, int opinion, int pageSize, int pageIndex, string postTableId, PostOrderType postOrderType)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetLastPostList(int fid, int count, string postTableName, string visibleForum)
        {
            throw new NotImplementedException();
        }

        public int ReviseDebateTopicDiggs(int tid, int debateOpinion)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetDebatePostDiggs(string pidList)
        {
            throw new NotImplementedException();
        }

        public int GetLastPostTid(ForumInfo forumInfo, string visibleForum)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastPost(ForumInfo forumInfo, PostInfo postInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateOnlineTime(int olTimeSpan, int uid)
        {
            throw new NotImplementedException();
        }

        public void ResetThismonthOnlineTime()
        {
            throw new NotImplementedException();
        }

        public string GetTopicFilterCondition(string filter)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupMaxspacephotosize()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUserGroupMaxspaceattachsize()
        {
            throw new NotImplementedException();
        }

        public void ClearUserSpace(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader ShowHelp(int id)
        {
            throw new NotImplementedException();
        }

        public void AddHelp(string title, string message, int pid, int orderBy)
        {
            throw new NotImplementedException();
        }

        public void DelHelp(string idList)
        {
            throw new NotImplementedException();
        }

        public int HelpCount()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetExistTopicTypeOfForum()
        {
            throw new NotImplementedException();
        }

        public void UpdateTopicTypeForForum(string topicTypes, int fid)
        {
            throw new NotImplementedException();
        }

        public void UpdateTopicTypes(string name, int displayOrder, string description, int typeId)
        {
            throw new NotImplementedException();
        }

        public void AddTopicTypes(string typeName, int displayOrder, string description)
        {
            throw new NotImplementedException();
        }

        public void DeleteTopicTypesByTypeidlist(string typeIdList)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetForumNameIncludeTopicType()
        {
            throw new NotImplementedException();
        }

        public void ClearTopicTopicType(int typeId)
        {
            throw new NotImplementedException();
        }

        public int UpdateSmiliesPart(string code, int displayOrder, int id)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetSmiliesInfoByType(int typeId)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetTopicsIdentifyItem()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicListByTidlist(string postTableId, string tidList)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserPMSetting(UserInfo user)
        {
            throw new NotImplementedException();
        }

        public void IdentifyTopic(string topicList, int identify)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable MailListTable(string userNameList)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetTopicListByCondition(string postName, int forumId, string posterList, string keyList, string startDate, string endDate, int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        public int GetTopicListCountByCondition(string postName, int forumId, string posterList, string keyList, string startDate, string endDate)
        {
            throw new NotImplementedException();
        }

        public int GetUserIdByRewriteName(string rewriteName)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetUserInfoByName(string userName)
        {
            throw new NotImplementedException();
        }

        public string ResetTopTopicListSql(int layer, string fid, string parentIdList)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable UserList(int pageSize, int currentPage, string condition)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserCredits(string credits)
        {
            throw new NotImplementedException();
        }

        public string DelVisitLogCondition(string deleteMod, string visitId, string deleteNum, string deleteFrom)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAttachDataTable(string condition, string postName)
        {
            throw new NotImplementedException();
        }

        public bool AuditTopicCount(string condition)
        {
            throw new NotImplementedException();
        }

        public string AuditTopicBindStr(string condition)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable AuditTopicBind(string condition)
        {
            throw new NotImplementedException();
        }

        public string DelMedalLogCondition(string deleteMode, string id, string deleteNum, string deleteFrom)
        {
            throw new NotImplementedException();
        }

        public string DelModeratorManageCondition(string deleteMode, string id, string deleteNum, string deleteFrom)
        {
            throw new NotImplementedException();
        }

        public string GetTopicCountCondition(out string type, string getType, int getNewTopic)
        {
            throw new NotImplementedException();
        }

        public void UpdatePostSP()
        {
            throw new NotImplementedException();
        }

        public void CreateStoreProc(int tableListMaxId)
        {
            throw new NotImplementedException();
        }

        public void DeleteSmilyByType(int type)
        {
            throw new NotImplementedException();
        }

        public void UpdateMyTopic()
        {
            throw new NotImplementedException();
        }

        public void UpdateMyPost()
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetAllIdentify()
        {
            throw new NotImplementedException();
        }

        public bool UpdateIdentifyById(int id, string name)
        {
            throw new NotImplementedException();
        }

        public bool AddIdentify(string name, string fileName)
        {
            throw new NotImplementedException();
        }

        public void DeleteIdentify(string idList)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetExistMedalList()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetSitemapNewTopics(string p)
        {
            throw new NotImplementedException();
        }

        public string GetRateLogCountCondition(int userId, string postIdList)
        {
            throw new NotImplementedException();
        }

        public int DeleteAttachmentByPid(int pid)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetOnlineUser(int olId)
        {
            throw new NotImplementedException();
        }

        public int GetOlidByUid(int uid)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetUsers(string groupIdList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentByUid(int uid, string extList, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int GetUserAttachmentCount(int uid)
        {
            throw new NotImplementedException();
        }

        public int GetUserAttachmentCount(int uid, string extList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetAttachmentByUid(int uid, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public void DelMyAttachmentByTid(string tidList)
        {
            throw new NotImplementedException();
        }

        public void DelMyAttachmentByPid(string pidList)
        {
            throw new NotImplementedException();
        }

        public void DelMyAttachmentByAid(string aidList)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetPostDebate(int tid)
        {
            throw new NotImplementedException();
        }

        public void SetTopicsBump(string tidList, int type)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable GetWebSiteAggForumTopicList(string showType, int topNumber)
        {
            throw new NotImplementedException();
        }

        public int GetDebatesPostCount(int tid, int debateOpinion)
        {
            throw new NotImplementedException();
        }

        public void DeleteDebatePost(int tid, string opinion, int pid)
        {
            throw new NotImplementedException();
        }

        public string Global_UserGrid_GetCondition(string getString)
        {
            throw new NotImplementedException();
        }

        public int Global_UserGrid_RecordCount(string condition)
        {
            throw new NotImplementedException();
        }

        public string Global_UserGrid_SearchCondition(bool isLike, bool isPostDateTime, string userName, string nickName, string userGroup, string email, string creditsStart, string creditsEnd, string lastIp, string posts, string digestPosts, string uid, string joindateStart, string joindateEnd)
        {
            throw new NotImplementedException();
        }

        public int GetPostId()
        {
            throw new NotImplementedException();
        }
    }
}
