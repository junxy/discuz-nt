using System;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

using Discuz.Common;
using Discuz.Forum;
using Discuz.Entity;
using Discuz.Config;
using Discuz.Plugin.Album;
using Discuz.Plugin.Space;

namespace Discuz.Web
{
    /// <summary>
    /// 发表主题页面
    /// </summary>
    public class referrer : Discuz.Web.UI.TopicPage
    {
        #region 页面变量
        public int blockid = DNTRequest.GetFormInt("blockid",0);
        public string blockName = "";
        public string title = "";
        public string summary = "";
        public string poster = "";
        public string postdatetime = "" ;
        public string imagePath = "";
        public int tid = DNTRequest.GetInt("topicid", -1);
        /// <summary>
        /// 是否需要登录
        /// </summary>
        public bool needlogin = false;
        #endregion
        protected override void ShowPage()
        {
            if (usergroupinfo.Radminid == 0)
            {
                AddErrLine("您没有推荐主题的权限！");
                return;
            }
            if (tid == -1)
	        {
		        return;
	        }
            DataTable postInfo = BlockEntries.GetPostInfoByTid(tid);
            
            title = postInfo.Rows[0]["title"].ToString();
            summary = Utils.RemoveHtml(Utils.ClearUBB(postInfo.Rows[0]["message"].ToString().Trim()));
            poster = postInfo.Rows[0]["poster"].ToString();
            postdatetime = postInfo.Rows[0]["postdatetime"].ToString().Trim();
            //如果是提交...
            if (ispost)
            {
                blockid = DNTRequest.GetFormInt("blockid",-1);
                blockName = DNTRequest.GetFormString("blockname");
                title = DNTRequest.GetFormString("topicTitle");
                summary = DNTRequest.GetFormString("summary");
                NormalValidate(blockName, blockid, title, summary);

                BlockEntryInfo blockEntries = new BlockEntryInfo();
                blockEntries.title = title;
                blockEntries.blockid = blockid;
                blockEntries.image = DNTRequest.GetFormString("selectImage");
                blockEntries.summary = summary;
                blockEntries.author = DNTRequest.GetFormString("poster");
                blockEntries.postdatetime = TypeConverter.StrToDateTime(DNTRequest.GetFormString("postdatetime"));
                blockEntries.link = "showtopic.aspx?tid="+tid;
                blockEntries.foretag = "";
                blockEntries.reartag = "";
                blockEntries.color = "";
                blockEntries.html = "";
                blockEntries.pushedbyuid = userid;
                blockEntries.pubshedbyusername = username;
                blockEntries.pusheddatetime = DateTime.Now;
                blockEntries.displayorder = 1;
                BlockEntries.AddBlockEntry(blockEntries);
                AddMsgLine("推荐成功");
                SetUrl(Urls.ShowDebateAspxRewrite(topicid));
                #region 验证提交信息
                if (IsErr()) return;

                //// 如果用户上传了附件,则检测用户是否有上传附件的权限
                //if (ForumUtils.IsPostFile())
                //{
                //    if (Utils.StrIsNullOrEmpty(Attachments.GetAttachmentTypeArray(attachmentTypeSelect)))
                //        AddErrLine("系统不允许上传附件");

                //    if (!UserAuthority.PostAttachAuthority(forum, usergroupinfo, userid, ref msg))
                //        AddErrLine(msg);
                //}

                ////发悬赏校验
                //int topicprice = 0;
                //bool isbonus = type == "bonus";
                //ValidateBonus(ref topicprice, ref isbonus);

                ////发特殊主题校验
                //ValidatePollAndDebate();

                //if (IsErr())
                //    return;
                #endregion
                if (IsErr())
                    return;

                if (IsErr())
                    return;

                //如果已登录就不需要再登录
                if (needlogin && userid > 0)
                    needlogin = false;
            }
            else //非提交操作
                AddLinkCss(BaseConfigs.GetForumPath + "templates/" + templatepath + "/editor.css", "css");
        }

        /// <summary>
        /// 常规验证
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="blockid"></param>
        /// <param name="topicTitle"></param>
        /// <param name="summary"></param>
        private void NormalValidate(string blockName, int blockid, string topicTitle, string summary)
        {
            if (ForumUtils.IsCrossSitePost())
            {
                AddErrLine("您的请求来路不正确，无法提交。如果您安装了某种默认屏蔽来路信息的个人防火墙软件(如 Norton Internet Security)，请设置其不要禁止来路信息后再试。");
                return;
            }
            if (blockName.Equals("") || blockid.Equals(""))
            {
                AddErrLine("区块名称和ID不能为空");
                return;
            }

            if (topicTitle.Equals(""))
            {
                AddErrLine("标题不能为空");
                return;
            }

            if (summary.Equals(""))
            {
                AddErrLine("摘要不能为空");
                return;
            }
        }
    }
}
