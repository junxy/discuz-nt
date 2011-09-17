using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI;

using Discuz.Common;
using Discuz.Entity;
using Discuz.Forum;
using Discuz.Config;

namespace Discuz.Web.Admin
{
    /// <summary>
    /// 审核帖子
    /// </summary>
    public partial class auditpost : AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Posts.GetPostsCount(postlist.SelectedValue) == 0)
                {
                    msg.Visible = true;
                }
                BindData();
            }
        }

        public void BindData()
        {
            #region 绑定审核帖子
            DataGrid1.AllowCustomPaging = false;
            DataGrid1.TableHeaderName = "审核帖子列表";
            DataGrid1.DataKeyField = "pid";
            DataGrid1.BindData(AdminTopics.GetUnauditPost(int.Parse(postlist.SelectedValue)));
            #endregion
        }

        protected void Sort_Grid(Object sender, DataGridSortCommandEventArgs e)
        {
            DataGrid1.Sort = e.SortExpression.ToString();
        }

        protected void DataGrid_PageIndexChanged(object source, DataGridPageChangedEventArgs e)
        {
            DataGrid1.LoadCurrentPageIndex(e.NewPageIndex);
        }

        private void postslist_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindData();
        }

        public void initPostTable()
        {
            #region 初始化分表控件

            postlist.AutoPostBack = true;

            DataTable dt = Discuz.Forum.Posts.GetAllPostTableName();
            postlist.Items.Clear();
            foreach (DataRow r in dt.Rows)
            {
                postlist.Items.Add(new ListItem(BaseConfigs.GetTablePrefix + "posts" + r[0].ToString(), r[0].ToString()));
            }
            postlist.DataBind();
            postlist.SelectedValue = Discuz.Forum.Posts.GetPostTableId();

            #endregion
        }

        private void SelectPass_Click(object sender, EventArgs e)
        {
            #region 对选中的帖子设置为通过审核

            string idlist = DNTRequest.GetString("pid");
            string pidlist = "";
            string tidlist = "";
            foreach (string doubleid in idlist.Split(','))
            {
                string[] idarray = doubleid.Split('|');
                pidlist += idarray[0] + ",";
                tidlist += idarray[1] + ",";
            }
            pidlist = pidlist.TrimEnd(',');
            tidlist = tidlist.TrimEnd(',');
            if (this.CheckCookie())
            {
                if (pidlist != "")
                {
                    UpdateUserCredits(tidlist, pidlist);
                    Posts.PassPost(int.Parse(postlist.SelectedValue), pidlist);
                    base.RegisterStartupScript( "PAGE", "window.location.href='forum_auditpost.aspx';");
                }
                else
                {
                    base.RegisterStartupScript( "", "<script>alert('您未选中任何选项');window.location.href='forum_auditpost.aspx';</script>");
                }
            }

            #endregion
        }

        /// <summary>
        /// 更新用户积分
        /// </summary>
        /// <param name="tidlist">审核主题id</param>
        /// <param name="pidlist">通过审核帖子的Pid列表</param>
        private void UpdateUserCredits(string tidlist, string pidlist)
        {
            string[] tidarray = tidlist.Split(',');
            string[] pidarray = pidlist.Split(',');
            float[] values = null;
            ForumInfo forum = null;
            PostInfo post = null;
            int fid = -1;
            for(int i = 0; i < pidarray.Length; i++)
            {
                //Topics.get
                post = Discuz.Forum.Posts.GetPostInfo(int.Parse(tidarray[i]), int.Parse(pidarray[i]));  //获取帖子信息
                if (fid != post.Fid)    //当上一个和当前主题不在一个版块内时，重新读取版块的积分设置
                {
                    fid = post.Fid;
                    forum = Discuz.Forum.Forums.GetForumInfo(fid);
                    if (!forum.Replycredits.Equals(""))
                    {
                        int index = 0;
                        float tempval = 0;
                        values = new float[8];
                        foreach (string ext in Utils.SplitString(forum.Replycredits, ","))
                        {

                            if (index == 0)
                            {
                                if (!ext.Equals("True"))
                                {
                                    values = null;
                                    break;
                                }
                                index++;
                                continue;
                            }
                            tempval = Utils.StrToFloat(ext, 0.0f);
                            values[index - 1] = tempval;
                            index++;
                            if (index > 8)
                            {
                                break;
                            }
                        }
                    }
                }

                if (values != null)
                {
                    ///使用版块内积分
                    Discuz.Forum.UserCredits.UpdateUserExtCredits(post.Posterid, values, false);
                    Discuz.Forum.UserCredits.UpdateUserCredits(post.Posterid);                  
                }
                else
                {
                    ///使用默认积分
                    Discuz.Forum.UserCredits.UpdateUserCreditsByPosts(post.Posterid);
                }
            }
        }

        private void SelectDelete_Click(object sender, EventArgs e)
        {
            #region 对选中的帖子进行删除

            if (this.CheckCookie())
            {
                if (DNTRequest.GetString("pid") != "")
                {
                    Posts.GetPostLayer(int.Parse(postlist.SelectedValue));
                    //DataTable dt = new DataTable();
                    //string pid = "";
                    //foreach (string idlist in DNTRequest.GetString("pid").Split(','))
                    //{
                    //    pid = idlist.Split('|')[0];
                    //    if (pid.Trim() != "")
                    //    {
                    //        dt = Posts.GetPostLayer(int.Parse(postlist.SelectedValue), int.Parse(pid));
                    //        if (dt.Rows.Count > 0)
                    //        {
                    //            if (dt.Rows[0]["layer"].ToString().Trim() == "0")
                    //            {
                    //                Discuz.Forum.TopicAdmins.DeleteTopics(dt.Rows[0]["tid"].ToString(), false);
                    //            }
                    //            else
                    //            {
                    //                Discuz.Forum.Posts.DeletePost(postlist.SelectedValue, Convert.ToInt32(pid), false,false);
                    //            }
                    //        }
                    //    }
                    //}
                    base.RegisterStartupScript( "PAGE", "window.location.href='forum_auditpost.aspx';");
                }
                else
                {
                    base.RegisterStartupScript( "", "<script>alert('您未选中任何选项');window.location.href='forum_auditpost.aspx';</script>");
                }
            }

            #endregion
        }

        protected string GetPostStatus(string invisible)
        {
            if (invisible == "1")
                return "未审核";
            if (invisible == "-3")
                return "忽略";
            return invisible;
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }


        private void InitializeComponent()
        {
            this.postlist.SelectedIndexChanged += new EventHandler(this.postslist_SelectedIndexChanged);
            this.SelectPass.Click += new EventHandler(this.SelectPass_Click);
            this.SelectDelete.Click += new EventHandler(this.SelectDelete_Click);

            DataGrid1.DataKeyField = "pid";
            DataGrid1.TableHeaderName = "审核帖子列表";
            DataGrid1.ColumnSpan = 7;

            initPostTable();
        }

        #endregion

    }
}