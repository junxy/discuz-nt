using System;

using Discuz.Common;
using Discuz.Forum;
using Discuz.Web.UI;
using Discuz.Entity;
using Discuz.Config;

namespace Discuz.Web
{
    /// <summary>
    /// 短消息基本设置页面
    /// </summary>
    public class usercppmset : UserCpPage
    {
        /// <summary>
        /// 短消息接收设置
        /// </summary>
        public int receivepmsetting;
      
        protected override void ShowPage()
        {
            pagetitle = "用户控制面板";

            if (!IsLogin()) return; 

            receivepmsetting = (int) user.Newsletter;
            newnoticecount = Notices.GetNewNoticeCountByUid(userid);

            if (DNTRequest.IsPost())
            {
                //user.Pmsound = DNTRequest.GetInt("pmsound", 0);
                receivepmsetting = DNTRequest.GetInt("receivesetting", 1);
                //receivepmsetting = 1;
                //foreach (string rpms in DNTRequest.GetString("receivesetting").Split(','))
                //{
                //    if (!Utils.StrIsNullOrEmpty(rpms))
                //        receivepmsetting = receivepmsetting | int.Parse(rpms);
                //}
                user.Newsletter = (ReceivePMSettingType) receivepmsetting;
                Users.UpdateUserPMSetting(user);

                //ForumUtils.WriteCookie("pmsound", user.Pmsound.ToString());

                SetUrl("usercppmset.aspx");
                SetMetaRefresh();
                SetShowBackLink(true);
                AddMsgLine("短消息设置已成功更新");
            }
        }
    }
}