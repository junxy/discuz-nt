using System;
using System.Web.UI;
using System.Data;

using Discuz.Control;
using Discuz.Forum;
using Discuz.Config;
using Discuz.Common;
using Discuz.Plugin.Space;
using Discuz.Plugin.Album;
using Discuz.Plugin.Mall;
using System.Collections;

namespace Discuz.Web.Admin
{
    /// <summary>
    /// 基本设置
    /// </summary>

    public partial class siteoptimization : AdminPage
    {
        protected Discuz.Control.RadioButtonList iisurlrewrite;
        protected bool haveAlbum;
        protected bool haveSpace;
        protected bool haveMall;
        protected void Page_Load(object sender, EventArgs e)
        {
            haveAlbum = AlbumPluginProvider.GetInstance() != null;
            haveSpace = SpacePluginProvider.GetInstance() != null;
            haveMall = MallPluginProvider.GetInstance() != null;
            if (!Page.IsPostBack)
            {
                LoadConfigInfo();
                
            }
        }

        public void LoadConfigInfo()
        {
            #region 加载配置信息
            GeneralConfigInfo configInfo = GeneralConfigs.GetConfig();
            fulltextsearch.SelectedValue = configInfo.Fulltextsearch.ToString();
            nocacheheaders.SelectedValue = configInfo.Nocacheheaders.ToString();
            maxonlines.Text = configInfo.Maxonlines.ToString();
            searchctrl.Text = configInfo.Searchctrl.ToString();
            //postinterval.Text = configInfo.Postinterval.ToString();
            //maxspm.Text = configInfo.Maxspm.ToString();
            statscachelife.Text = configInfo.Statscachelife.ToString();
            guestcachepagetimeout.Text = configInfo.Guestcachepagetimeout.ToString();
            oltimespan.Text = configInfo.Oltimespan.ToString();
            topiccachemark.Text = configInfo.Topiccachemark.ToString();
            if (configInfo.Onlinetimeout >= 0) showauthorstatusinpost.SelectedValue = "2";
            else showauthorstatusinpost.SelectedValue = "1";
            #endregion
        }

        private void SaveInfo_Click(object sender, EventArgs e)
        {
            #region 保存信息
            if (this.CheckCookie())
            {


                GeneralConfigInfo configInfo = GeneralConfigs.GetConfig();
                Hashtable HT = new Hashtable();
                HT.Add("最大在线人数", maxonlines.Text);
                HT.Add("搜索时间限制", searchctrl.Text);
                foreach (DictionaryEntry de in HT)
                {
                    if (!Utils.IsInt(de.Value.ToString()))
                    {
                        base.RegisterStartupScript("", "<script>alert('输入错误:" + de.Key.ToString().Trim() + ",只能是0或者正整数');window.location.href='global_safecontrol.aspx';</script>");
                        return;
                    }
                }

                if (fulltextsearch.SelectedValue == "1")
                {
                    string msg = "";
                    configInfo.Fulltextsearch = Databases.TestFullTextIndex(ref msg);
                }
                else
                    configInfo.Fulltextsearch = 0;

                configInfo.Nocacheheaders = Convert.ToInt16(nocacheheaders.SelectedValue);
                configInfo.Maxonlines = Convert.ToInt32(maxonlines.Text);
                configInfo.Searchctrl = Convert.ToInt32(searchctrl.Text);
                configInfo.Statscachelife = Convert.ToInt16(statscachelife.Text);
                configInfo.Guestcachepagetimeout = Convert.ToInt16(guestcachepagetimeout.Text);
                configInfo.Oltimespan = Convert.ToInt16(oltimespan.Text);
                configInfo.Topiccachemark = Convert.ToInt16(topiccachemark.Text);
                if (showauthorstatusinpost.SelectedValue == "1") configInfo.Onlinetimeout = 0 - Convert.ToInt32(onlinetimeout.Text);
                else configInfo.Onlinetimeout = TypeConverter.StrToInt(onlinetimeout.Text);
                //configInfo.Secques = Convert.ToInt32(secques.SelectedValue);
                //configInfo.Postinterval = Convert.ToInt32(postinterval.Text);
                //configInfo.Maxspm = Convert.ToInt32(maxspm.Text);



                GeneralConfigs.Serialiaze(configInfo, Server.MapPath("../../config/general.config"));
                Urls.config = configInfo;
                if (configInfo.Aspxrewrite == 1)
                    AdminForums.SetForumsPathList(true, configInfo.Extname);
                else
                    AdminForums.SetForumsPathList(false, configInfo.Extname);
                Discuz.Cache.DNTCache.GetCacheService().RemoveObject("/Forum/ForumList");
                Discuz.Forum.TopicStats.SetQueueCount();
                Caches.ReSetConfig();
                AdminVistLogs.InsertLog(this.userid, this.username, this.usergroupid, this.grouptitle, this.ip, "基本设置", "");
                base.RegisterStartupScript("PAGE", "window.location.href='global_siteoptimization.aspx';");
            }
            #endregion
        }

        #region Web 窗体设计器生成的代码

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.SaveInfo.Click += new EventHandler(this.SaveInfo_Click);
        }
        #endregion

    }
}