using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI;

using Discuz.Common;
using Discuz.Entity;
using Discuz.Forum;
using Button = Discuz.Control.Button;
using DataGrid = Discuz.Control.DataGrid;
using Discuz.Config;

namespace Discuz.Web.Admin
{
    /// <summary>
    /// 审核新主题 
    /// </summary>

    public partial class auditnewtopic : AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindData();
            }
        }

        public void BindData()
        {
            #region 绑定审核主题
            DataGrid1.AllowCustomPaging = false;
            DataGrid1.TableHeaderName = "审核主题列表";
            DataGrid1.DataKeyField = "tid";
            DataGrid1.BindData(AdminTopics.GetUnauditNewTopic());
            #endregion
        }

        protected void Sort_Grid(Object sender, DataGridSortCommandEventArgs e)
        {
            DataGrid1.Sort = e.SortExpression.ToString();
        }

        protected void DataGrid_PageIndexChanged(object source, DataGridPageChangedEventArgs e)
        {
            DataGrid1.LoadCurrentPageIndex(e.NewPageIndex);
            BindData();
        }

        private void SelectPass_Click(object sender, EventArgs e)
        {
            #region 对选中的主题设置为通过审核

            if (this.CheckCookie())
            {
                string tidlist = DNTRequest.GetString("tid");
                if (tidlist != "")
                {
                    //UpdateUserCredits(tidlist);
                    Topics.PassAuditNewTopic(tidlist);
                    base.RegisterStartupScript("", "<script>window.location='forum_auditnewtopic.aspx';</script>");
                }
                else
                {
                    base.RegisterStartupScript("", "<script>alert('您未选中任何选项');window.location='forum_auditnewtopic.aspx';</script>");
                }
            }

            #endregion
        }

        /// <summary>
        /// 更新用户积分
        /// </summary>
        /// <param name="tidlist">通过审核主题的Tid列表</param>
        //private void UpdateUserCredits(string tidlist)
        //{
        //    string[] tidarray = tidlist.Split(',');
        //    float[] values = null;
        //    ForumInfo forum = null;
        //    TopicInfo topic = null;
        //    int fid = -1;
        //    foreach(string tid in tidarray)
        //    {
        //        topic = Discuz.Forum.Topics.GetTopicInfo(int.Parse(tid));    //获取主题信息
        //        if(fid != topic.Fid)    //当上一个和当前主题不在一个版块内时，重新读取版块的积分设置
        //        {
        //            fid = topic.Fid;
        //            forum = Discuz.Forum.Forums.GetForumInfo(fid);
        //            if (!forum.Postcredits.Equals(""))
        //            {
        //                int index = 0;
        //                float tempval = 0;
        //                values = new float[8];
        //                foreach (string ext in Utils.SplitString(forum.Postcredits, ","))
        //                {

        //                    if (index == 0)
        //                    {
        //                        if (!ext.Equals("True"))
        //                        {
        //                            values = null;
        //                            break;
        //                        }
        //                        index++;
        //                        continue;
        //                    }
        //                    tempval = Utils.StrToFloat(ext, 0);
        //                    values[index - 1] = tempval;
        //                    index++;
        //                    if (index > 8)
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //        }

        //        if (values != null)
        //        {
        //            ///使用版块内积分
        //            Discuz.Forum.UserCredits.UpdateUserCreditsByPostTopic(topic.Posterid, values);
        //        }
        //        else
        //        {
        //            ///使用默认积分
        //            Discuz.Forum.UserCredits.UpdateUserCreditsByPostTopic(topic.Posterid);
        //        }
        //    }
        //}

        private void SelectDelete_Click(object sender, EventArgs e)
        {
            #region 对选中的主题进行删除

            if (this.CheckCookie())
            {
                if (DNTRequest.GetString("tid") != "")
                {
                    //TopicAdmins.DeleteTopics(DNTRequest.GetString("tid"), 0, false);
                    Discuz.Forum.TopicAdmins.DeleteTopicsWithoutChangingCredits(DNTRequest.GetString("tid"), false);
                    base.RegisterStartupScript("",  "<script>window.location='forum_auditnewtopic.aspx';</script>");
                }
                else
                {
                    base.RegisterStartupScript("", "<script>alert('您未选中任何选项');window.location='forum_auditnewtopic.aspx';</script>");
                }
            }

            #endregion
        }

        public string GetTopicType(string topicType)
        {
            switch(topicType)
            {
                case "0":
                    return "普通主题";
                case "1":
                    return "投票帖";
                case "2":
                case "3":
                    return "悬赏帖";
                case "4":
                    return "辩论帖";
                default:
                    return topicType;
            }
        }

        protected string GetTopicStatus(string displayOrder)
        {
            //>0为置顶,<0不显示,==0正常   -1为回收站   -2待审核 -3为被忽略
            int order = int.Parse(displayOrder);
            if (order > 0)
                return "置顶";
            if (order == 0)
                return "正常";
            if (order == -1)
                return "回收站";
            if (order == -2)
                return "待审核";
            if (order == -3)
                return "被忽略";
            return displayOrder;
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.SelectPass.Click += new EventHandler(this.SelectPass_Click);
            this.SelectDelete.Click += new EventHandler(this.SelectDelete_Click);

            DataGrid1.DataKeyField = "tid";
            DataGrid1.TableHeaderName = "审核主题列表";
            DataGrid1.ColumnSpan = 10;
        }

        #endregion
    }
}