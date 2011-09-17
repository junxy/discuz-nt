using System;
using System.Data;
using System.Web.UI.HtmlControls;
using System.Web.UI;

using Discuz.Control;
using Discuz.Forum;
using Discuz.Config;
using Discuz.Common;
using Discuz.Entity;
using Discuz.Cache;

namespace Discuz.Web.Admin
{
    public partial class forumhot : AdminPage
    {
        public DataSet dsSrc = new DataSet();
        public string action = DNTRequest.GetString("action");
        public int id = DNTRequest.GetInt("id", -1);
        protected global::Discuz.Web.Admin.pageinfo info1;
        public GeneralConfigInfo configInfo = new GeneralConfigInfo();
        protected void Page_Load(object sender, EventArgs e)
        {
            configInfo = GeneralConfigs.GetConfig();
            dsSrc.ReadXml(Server.MapPath("../../config/forumhot.config"));
            if (action == "editsave" && id != -1)
            {

                EditSave();
                //base.RegisterStartupScript("page", "window.location.href='forum_forumhot.aspx';");
                Response.Redirect("forum_forumhot.aspx");
            }
            if (action == "closeforumhot")
            {
                configInfo.Disableforumhot = TypeConverter.StrToInt(DNTRequest.GetString("forumhot"));
                GeneralConfigs.Serialiaze(configInfo, Server.MapPath("../../config/general.config"));
                Response.Redirect("forum_forumhot.aspx");
            }
        }

        private void EditSave()
        {
            foreach (DataRow dr in dsSrc.Tables["hottab"].Rows)
            {
                if (id == TypeConverter.ObjectToInt(dr["id"]))
                {
                    dr["displayorder"] = DNTRequest.GetInt("displayorder", -1);
                    dr["name"] = DNTRequest.GetString("name");
                    dr["datatype"] = DNTRequest.GetString("datatype");

                    if (DNTRequest.GetString("datatype") == "users" && DNTRequest.GetString("sorttype") == "posts")
                        dr["sorttype"] = DNTRequest.GetString("topictimetype");
                    else
                        dr["sorttype"] = DNTRequest.GetString("sorttype");

                    dr["forumid"] = DNTRequest.GetString("forumid");
                    dr["topiccount"] = DNTRequest.GetInt("topiccount", -1);
                    dr["forumnamelength"] = DNTRequest.GetInt("forumnamelength", -1);
                    dr["topictitlelength"] = DNTRequest.GetInt("topictitlelength", -1);
                    dr["cachetime"] = DNTRequest.GetInt("cachetime", -1);
                    dr["topictimetype"] = DNTRequest.GetString("topictimetype");
                }
            }
            try
            {
                dsSrc.WriteXml(Server.MapPath("../../config/forumhot.config"));
                DNTCache.GetCacheService().RemoveObject("/Forum/ForumHot");
                DNTCache.GetCacheService().RemoveObject("/Forum/ForumHostList-" + id);
                DNTCache.GetCacheService().RemoveObject("/Aggregation/HotForumList" + id);
                DNTCache.GetCacheService().RemoveObject("/Aggregation/Users_" + id + "List");
                DNTCache.GetCacheService().RemoveObject("/Aggregation/HotImages_" + id + "List");
                dsSrc.Dispose();
            }
            catch
            {
                return;
            }
        }

    }
}
