using System;
using System.IO;
using System.Text;
using System.Web.UI;

using Discuz.Control;
using Discuz.Common;
using Discuz.Forum;

namespace Discuz.Web.Admin
{
    /// <summary>
    /// 模板文件编辑
    /// </summary>
    public partial class templatesedit : AdminPage
    {
        public string filenamefullpath;
        public string path;
        public string filename;

        protected void Page_Load(object sender, EventArgs e)
        {
            path = DNTRequest.GetString("path");
            if (path == "")
            {
                Response.Redirect("global_templatetree.aspx");
                return;
            }

            filename = DNTRequest.GetString("filename");
            filenamefullpath = "../../templates/" + path + "/" + filename;

            ViewState["path"] = path;
            ViewState["filename"] = filename;
            ViewState["templateid"] = DNTRequest.GetString("templateid");
            ViewState["templatename"] = DNTRequest.GetString("templatename");

            if (!Page.IsPostBack)
            {
                using (StreamReader objReader = new StreamReader(Server.MapPath(filenamefullpath), Encoding.UTF8))
                {
                    templatenew.Text = objReader.ReadToEnd();
                    objReader.Close();
                }
            }
        }

        private void SavaTemplateInfo_Click(object sender, EventArgs e)
        {
            #region 保存相关模板信息

            if (this.CheckCookie())
            {
                string path = ViewState["path"].ToString();
                string filename = ViewState["filename"].ToString();
                filenamefullpath = Server.MapPath("../../templates/" + path + "/" + filename);

                using (FileStream fs = new FileStream(filenamefullpath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    Byte[] info = Encoding.UTF8.GetBytes(templatenew.Text);
                    fs.Write(info, 0, info.Length);
                    fs.Close();
                }

                base.RegisterStartupScript( "PAGE",  "window.location.href='global_templatetree.aspx?path=" + ViewState["path"].ToString().Split('\\')[0] + "&templateid=" + ViewState["templateid"].ToString() + "&templatename=" + ViewState["templatename"].ToString() + "';");
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
            this.SavaTemplateInfo.Click += new EventHandler(this.SavaTemplateInfo_Click);
        }

        #endregion

    }
}