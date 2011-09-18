using Discuz.Common;
using Discuz.Config;
using Discuz.Entity;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
namespace Discuz.Data.MySql
{
	public partial class DataProvider : IDataProvider
	{
		private static int _lastRemoveTimeout;
		private DbParameter[] GetParms(string startdate, string enddate)
		{
			DbParameter[] array = new DbParameter[2];
			if (startdate != "")
			{
				array[0] = DbHelper.MakeInParam("?startdate", DbType.Int64, 8, DateTime.Parse(startdate));
			}
			if (enddate != "")
			{
				array[1] = DbHelper.MakeInParam("?enddate", DbType.Int64, 8, DateTime.Parse(enddate).AddDays(1.0));
			}
			return array;
		}
		public DataTable GetTopicListByCondition(string postname, int forumid, string posterlist, string keylist, string startdate, string enddate, int pageSize, int currentPage)
		{
			string condition = DataProvider.GetCondition(forumid, posterlist, keylist, startdate, enddate);
			DbParameter[] parms = this.GetParms(startdate, enddate);
			int num = (currentPage - 1) * pageSize;
			int minPostTableTid = DatabaseProvider.GetInstance().GetMinPostTableTid(postname);
			int maxPostTableTid = DatabaseProvider.GetInstance().GetMaxPostTableTid(postname);
			string text = string.Format(string.Concat(new object[]
			{
				"SELECT t.*,f.`name` FROM `{1}topics` t LEFT JOIN `{1}forums` f ON t.fid=f.fid LEFT JOIN `{1}forumfields` ff ON f.`fid`=ff.`fid` AND (ff.`viewperm` IS NULL OR ff.`viewperm`='' OR InStr(','+ff.`viewperm`+',',',7,')<>0) WHERE `closed`<>1 AND `status`=1 AND `password`='' {2} AND `tid`>=", 
				minPostTableTid, 
				" AND `tid`<=", 
				maxPostTableTid, 
				" ORDER BY `tid` DESC limit {3},{4}"
			}), new object[]
			{
				pageSize, 
				BaseConfigs.get_GetTablePrefix(), 
				condition, 
				num, 
				pageSize
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text, parms).Tables[0];
		}
		private static string GetCondition(int forumid, string posterlist, string keylist, string startdate, string enddate)
		{
			string text = "";
			if (forumid != 0)
			{
				text = text + " AND f.`fid`=" + forumid;
			}
			if (posterlist != "")
			{
				string[] array = posterlist.Split(new char[]
				{
					','
				});
				text += " AND `poster` in (";
				string text2 = "";
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string str = array2[i];
					text2 = text2 + "'" + str + "',";
				}
				if (text2 != "")
				{
					text2 = text2.Substring(0, text2.Length - 1);
				}
				text = text + text2 + ")";
			}
			if (keylist != "")
			{
				string text3 = "";
				string[] array2 = keylist.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array2.Length; i++)
				{
					string str2 = array2[i];
					text3 = text3 + " `title` LIKE '%" + DataProvider.RegEsc(str2) + "%' OR";
				}
				text3 = text3.Substring(0, text3.Length - 2);
				text = text + " AND (" + text3 + ")";
			}
			if (startdate != "")
			{
				text += " AND `postdatetime`>=?startdate";
			}
			if (enddate != "")
			{
				text += " AND `postdatetime`<=?enddate";
			}
			return text;
		}
		public int GetTopicListCountByCondition(string postname, int forumid, string posterlist, string keylist, string startdate, string enddate)
		{
			string text = string.Format("SELECT COUNT(1) FROM `{0}topics` t LEFT JOIN `{0}forums` f ON t.fid=f.fid LEFT JOIN `{0}forumfields` ff ON f.`fid`=ff.`fid`   AND (ff.`viewperm` IS NULL OR ff.`viewperm`='' OR InStr(','+ff.`viewperm`+',',',7,')<>0) WHERE `closed`<>1 AND `status`=1 AND `password`=''", BaseConfigs.get_GetTablePrefix());
			string condition = DataProvider.GetCondition(forumid, posterlist, keylist, startdate, enddate);
			DbParameter[] parms = this.GetParms(startdate, enddate);
			if (condition != "")
			{
				text += condition;
			}
			return int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text, parms).ToString());
		}
		public DataTable GetTopicListByTidlist(string posttableid, string tidlist)
		{
			string text = string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE (`tid` IN (", 
				tidlist, 
				")) ORDER BY INSTR('", 
				tidlist, 
				"',`tid`)"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetWebSiteAggForumTopicList(string showtype, int topnumber)
		{
			DataTable result = new DataTable();
			if (showtype != null)
			{
				if (showtype == "1")
				{
					result = DbHelper.ExecuteDataset(string.Concat(new object[]
					{
						"SELECT t.`tid`, t.`title`, t.`postdatetime`, t.`poster`, t.`posterid`, t.`fid`, t.`views`, t.`replies`, f.`name` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` t LEFT OUTER JOIN `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forums` f ON t.`fid` = f.`fid` WHERE t.`displayorder`>=0 AND f.`status`=1  AND `layer`> 0 AND f.`fid` IN (SELECT ff.`fid` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forumfields` ff WHERE ff.`password` ='') ORDER BY t.`postdatetime` DESC LIMIT 0,", 
						topnumber
					})).Tables[0];
					return result;
				}
				if (showtype == "2")
				{
					result = DbHelper.ExecuteDataset(string.Concat(new object[]
					{
						"SELECT t.`tid`, t.`title`, t.`postdatetime`, t.`poster`, t.`posterid`, t.`fid`, t.`views`, t.`replies`, f.`name` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` t LEFT OUTER JOIN `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forums` f ON t.`fid` = f.`fid` WHERE t.`digest` >0 AND f.`status`=1  AND `layer`> 0 AND f.`fid` IN (SELECT ff.`fid` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forumfields` ff WHERE ff.`password` ='') ORDER BY t.`digest` DESC LIMIT 0,", 
						topnumber
					})).Tables[0];
					return result;
				}
				if (showtype == "3")
				{
					result = DbHelper.ExecuteDataset(string.Concat(new string[]
					{
						"SELECT f.`fid`, f.`name`, f.`lasttid` AS `tid`, f.`lasttitle` AS `title` , f.`lastposterid` AS `posterid`, f.`lastposter` AS `poster`, f.`lastpost` AS `postdatetime`, t.`views`, t.`replies` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forums` f LEFT JOIN `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` t  ON f.`lasttid` = t.`tid` WHERE f.`status`=1  AND `layer`> 0 AND f.`fid` IN (SELECT ff.`fid` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forumfields` ff WHERE ff.`password` ='') AND t.`displayorder`>=0"
					})).Tables[0];
					return result;
				}
			}
			result = DbHelper.ExecuteDataset(string.Concat(new string[]
			{
				"SELECT f.`fid`, f.`name`, f.`lasttid` AS `tid`, f.`lasttitle` AS `title` , f.`lastposterid` AS `posterid`, f.`lastposter` AS `poster`, f.`lastpost` AS `postdatetime`, t.`views`, t.`replies` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` f LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` t  ON f.`lasttid` = t.`tid` WHERE f.`status`=1  AND `layer`> 0 AND f.`fid` IN (SELECT ff.`fid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` ff WHERE ff.`password` ='') AND t.`displayorder`>=0"
			})).Tables[0];
			return result;
		}
		public DataTable GetWebSiteAggHotForumList(int topnumber)
		{
			return DbHelper.ExecuteDataset(string.Concat(new object[]
			{
				"SELECT `fid`, `name`, `topics` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `status`=1 AND `layer`> 0 AND `fid` IN (SELECT `fid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` WHERE `password`='') ORDER BY `topics` DESC, `posts` DESC, `todayposts` DESC LIMIT ", 
				topnumber
			})).Tables[0];
		}
		public IDataReader GetHelpList(int id)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "SELECT `id`,`title`,`message`,`pid`,`orderby` FROM `" + BaseConfigs.get_GetTablePrefix() + "help` WHERE `pid`=?id OR `id`=?id ORDER BY `pid` ASC,`orderby` ASC";
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader ShowHelp(int id)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "SELECT `title`,`message`,`pid`,`orderby` FROM `" + BaseConfigs.get_GetTablePrefix() + "help` WHERE `id`=?id";
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader GetHelpClass()
		{
			string text = "SELECT `id` FROM `" + BaseConfigs.get_GetTablePrefix() + "help` WHERE `pid`=0 ORDER BY `orderby` ASC";
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public void AddHelp(string title, string message, int pid, int orderby)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?title", (DbType)254, 100, title), 
				DbHelper.MakeInParam("?message", (DbType)253, 100, message), 
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid), 
				DbHelper.MakeInParam("?orderby", DbType.Boolean, 4, orderby)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "help`(`title`,`message`,`pid`,`orderby`) VALUES(?title,?message,?pid,?orderby)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DelHelp(string idlist)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?idlist", (DbType)254, 100, idlist)
			};
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "help` WHERE `id` IN (?idlist) OR `pid` IN (?idlist)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void ModHelp(int id, string title, string message, int pid, int orderby)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?title", (DbType)254, 100, title), 
				DbHelper.MakeInParam("?message", (DbType)253, 100, message), 
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid), 
				DbHelper.MakeInParam("?orderby", DbType.Boolean, 4, orderby), 
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "help` SET `title`=?title,`message`=?message,`pid`=?pid,`orderby`=?orderby WHERE `id`=?id";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int HelpCount()
		{
			string text = "SELECT COUNT(*) FROM `" + BaseConfigs.get_GetTablePrefix() + "help`";
			return int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text).ToString());
		}
		public string BindHelpType()
		{
			return "SELECT `id`,`title` FROM `" + BaseConfigs.get_GetTablePrefix() + "help` WHERE `pid`=0 ORDER BY `orderby` ASC";
		}
		public void UpOrder(string orderby, string id)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?orderby", (DbType)253, 100, orderby), 
				DbHelper.MakeInParam("?id", (DbType)253, 100, id)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "help` SET `ORDERBY`=?orderby  Where id=?id";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		private static string RegEsc(string str)
		{
			string[] array = new string[]
			{
				"%", 
				"_", 
				"'"
			};
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i];
				str = str.Replace(text, "\\" + text);
			}
			return str;
		}
		public int AddForumLink(int displayorder, string name, string url, string note, string logo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?name", DbType.Single, 100, name), 
				DbHelper.MakeInParam("?url", DbType.Single, 100, url), 
				DbHelper.MakeInParam("?note", DbType.Single, 200, note), 
				DbHelper.MakeInParam("?logo", DbType.Single, 100, logo)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "forumlinks` (`displayorder`, `name`,`url`,`note`,`logo`) VALUES (?displayorder,?name,?url,?note,?logo)";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string GetForumLinks()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "forumlinks`";
		}
		public int DeleteForumLink(string forumlinkid)
		{
			string text = string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumlinks` WHERE `id`IN (", 
				forumlinkid, 
				")"
			});
			return DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public int UpdateForumLink(int id, int displayorder, string name, string url, string note, string logo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?name", DbType.Single, 100, name), 
				DbHelper.MakeInParam("?url", DbType.Single, 100, url), 
				DbHelper.MakeInParam("?note", DbType.Single, 200, note), 
				DbHelper.MakeInParam("?logo", DbType.Single, 100, logo), 
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forumlinks` SET `displayorder`=?displayorder,`name`=?name,`url`=?url,`note`=?note,`logo`=?logo  Where `id`=?id";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetForumIndexListTable()
		{
			string text = string.Format("SELECT IF(FLOOR(UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(`lastpost`))/60)<600,'new','old') AS `havenew`,`{0}forums`.*,`{0}forums`.`fid` as `fid`,`{0}forumfields`.* FROM `{0}forums` LEFT JOIN `{0}forumfields` ON `{0}forums`.`fid`=`{0}forumfields`.`fid` WHERE `{0}forums`.`parentid` NOT IN (SELECT fid FROM `{0}forums` WHERE `status` < 1 AND `layer` = 0) AND `{0}forums`.`status` > 0 AND `layer` <= 1 ORDER BY `displayorder`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public IDataReader GetForumIndexList()
		{
			string text = string.Format("SELECT IF(FLOOR((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(`lastpost`))/60)<600,'new','old') AS `havenew`,`{0}forums`.*,`{0}forums`.`fid` as `fid`,`{0}forumfields`.* FROM `{0}forums` LEFT JOIN `{0}forumfields` ON `{0}forums`.`fid`=`{0}forumfields`.`fid` WHERE `{0}forums`.`parentid` NOT IN (SELECT `fid` FROM `{0}forums` WHERE `status` < 1 AND `layer` = 0) AND `{0}forums`.`status` > 0 AND `layer` <= 1 ORDER BY `displayorder`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public DataTable GetArchiverForumIndexList()
		{
			string text = string.Format("SELECT `{0}forums`.`fid`, `{0}forums`.`name`, `{0}forums`.`layer`, `{0}forumfields`.`viewperm` FROM `{0}forums` LEFT JOIN `{0}forumfields` ON `{0}forums`.`fid`=`{0}forumfields`.`fid` WHERE `{0}forums`.`status` > 0  ORDER BY `displayorder`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public IDataReader GetSubForumReader(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new string[]
			{
				"SELECT IF(FLOOR((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(`lastpost`))/60)<600 ,'new','old') AS `havenew`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.*,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.`fid` As `fid`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields`.* FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.`fid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields`.`fid` WHERE `parentid` = ?fid AND `status` > 0 ORDER BY `displayorder`"
			});
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public DataTable GetSubForumTable(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new string[]
			{
				"SELECT IF(FLOOR((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(`lastpost`))/60)<600,'new','old') AS `havenew`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.*,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.`fid` As `fid`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields`.* FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.`fid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields`.`fid` WHERE `parentid` = ?fid AND `status` > 0 ORDER BY `displayorder`"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public DataTable GetForumsTable()
		{
			string text = string.Format("SELECT `{0}forums`.*, `{0}forumfields`.* FROM `{0}forums` LEFT JOIN `{0}forumfields` ON `{0}forums`.`fid`=`{0}forumfields`.`fid` ORDER BY `displayorder`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int SetRealCurrentTopics(int fid)
		{
			string text = string.Format("SELECT COUNT(tid) FROM {0}topics WHERE `displayorder` >= 0 AND `fid`={1}", BaseConfigs.get_GetTablePrefix(), fid);
			int num = Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(tid) FROM {0}topics WHERE `displayorder` >= 0 AND `fid`={1}", BaseConfigs.get_GetTablePrefix(), fid)), 0);
			string text2 = string.Format("UPDATE {0}forums SET `curtopics` =" + num + "  WHERE `fid`={1}", BaseConfigs.get_GetTablePrefix(), fid);
			return DbHelper.ExecuteNonQuery(CommandType.Text, text2);
		}
		public DataTable GetForumListTable()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT `name`, `fid` FROM `{0}forums` WHERE `{0}forums`.`parentid` NOT IN (SELECT fid FROM `{0}forums` WHERE `status` < 1 AND `layer` = 0) AND `status` > 0 AND `displayorder` >=0 ORDER BY `displayorder`", BaseConfigs.get_GetTablePrefix())).Tables[0];
		}
		public string GetTemplates()
		{
			return "Select templateid,name From `" + BaseConfigs.get_GetTablePrefix() + "templates` ";
		}
		public DataTable GetUserGroupsTitle()
		{
			string text = "Select groupid,grouptitle  From `" + BaseConfigs.get_GetTablePrefix() + "usergroups`  Order By `groupid` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetAttachTypes()
		{
			string text = "Select id,extension  From `" + BaseConfigs.get_GetTablePrefix() + "attachtypes`  Order By `id` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetForums()
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` ORDER BY `displayorder` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public string GetForumsTree()
		{
			return "SELECT `fid`,`name`,`parentid` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums`";
		}
		public int GetForumsMaxDisplayOrder()
		{
			return Utils.StrToInt(DbHelper.ExecuteDataset(CommandType.Text, "Select max(displayorder) From " + BaseConfigs.get_GetTablePrefix() + "forums").Tables[0].Rows[0][0], 0) + 1;
		}
		public DataTable GetForumsMaxDisplayOrder(int parentid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"Select Max(`displayorder`) From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`  Where `parentid`=", 
				parentid
			})).Tables[0];
		}
		public void UpdateForumsDisplayOrder(int minDisplayOrder)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set displayorder=displayorder+1  Where displayorder>" + minDisplayOrder.ToString());
		}
		public void UpdateSubForumCount(int fid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set subforumcount=subforumcount+1  Where fid=" + fid.ToString());
		}
		public DataRow GetForum(int fid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"Select * From ", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums Where fid=", 
				fid.ToString(), 
				" LIMIT 1"
			})).Tables[0].Rows[0];
		}
		public DataRowCollection GetModerators(int fid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT `username` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `uid` IN(SELECT `uid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"moderators` WHERE `inherited`=1 AND `fid`=", 
				fid, 
				")"
			})).Tables[0].Rows;
		}
		public DataTable GetTopForum(int fid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT `fid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `parentid`=0 AND `layer`=0 AND `fid`=", 
				fid, 
				"LIMIT 1"
			})).Tables[0];
		}
		public int UpdateForum(int fid, string name, int subforumcount, int displayorder)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?name", (DbType)254, 50, name), 
				DbHelper.MakeInParam("?subforumcount", DbType.Boolean, 4, subforumcount), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET `name`=?name,`subforumcount`=?subforumcount,`displayorder`=?displayorder Where `fid`=?fid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetForumField(int fid, string fieldname)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT `", 
				fieldname, 
				"` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` WHERE `fid`=", 
				fid, 
				" LIMIT 1"
			})).Tables[0];
		}
		public int UpdateForumField(int fid, string fieldname)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` SET `", 
				fieldname, 
				"`='' WHERE `fid`=", 
				fid
			}));
		}
		public int UpdateForumField(int fid, string fieldname, string fieldvalue)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` SET `", 
				fieldname, 
				"`='", 
				fieldvalue, 
				"' WHERE `fid`=", 
				fid
			}));
		}
		public DataRowCollection GetDatechTableIds()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT id FROM `" + BaseConfigs.get_GetTablePrefix() + "tablelist`").Tables[0].Rows;
		}
		public int UpdateMinMaxField(string posttablename, int posttableid)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"tablelist` SET `mintid`=", 
				this.GetMinPostTableTid(posttablename), 
				",`maxtid`=", 
				this.GetMaxPostTableTid(posttablename), 
				"  WHERE `id`=", 
				posttableid
			}));
		}
		public DataRowCollection GetForumIdList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `fid` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums`").Tables[0].Rows;
		}
		public int CreateFullTextIndex(string dbname)
		{
			return 0;
		}
		public int GetMaxForumId()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT MAX(fid) FROM " + BaseConfigs.get_GetTablePrefix() + "forums"), 0);
		}
		public DataTable GetForumList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `fid`,`name` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums`").Tables[0];
		}
		public DataTable GetAllForumList()
		{
			string text = "Select * From `" + BaseConfigs.get_GetTablePrefix() + "forums` Order By `displayorder` Asc";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetForumInformation(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.*, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields`.* FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.`fid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields`.`fid` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.`fid`=?fid"
			});
			DataTable dataTable = DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
			dataTable.Columns[0].ColumnName = "fid";
			return dataTable;
		}
		public void SaveForumsInfo(ForumInfo foruminfo)
		{
			MySqlConnection mySqlConnection = new MySqlConnection(DbHelper.get_ConnectionString());
			mySqlConnection.Open();
			using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
			{
				try
				{
					DbParameter[] array = new DbParameter[]
					{
						DbHelper.MakeInParam("?name", (DbType)253, 50, foruminfo.get_Name()), 
						DbHelper.MakeInParam("?status", DbType.Boolean, 4, foruminfo.get_Status()), 
						DbHelper.MakeInParam("?colcount", DbType.Boolean, 4, foruminfo.get_Colcount()), 
						DbHelper.MakeInParam("?templateid", DbType.Boolean, 2, foruminfo.get_Templateid()), 
						DbHelper.MakeInParam("?allowsmilies", DbType.Boolean, 4, foruminfo.get_Allowsmilies()), 
						DbHelper.MakeInParam("?allowrss", DbType.Boolean, 6, foruminfo.get_Allowrss()), 
						DbHelper.MakeInParam("?allowhtml", DbType.Boolean, 4, foruminfo.get_Allowhtml()), 
						DbHelper.MakeInParam("?allowbbcode", DbType.Boolean, 4, foruminfo.get_Allowbbcode()), 
						DbHelper.MakeInParam("?allowimgcode", DbType.Boolean, 4, foruminfo.get_Allowimgcode()), 
						DbHelper.MakeInParam("?allowblog", DbType.Boolean, 4, foruminfo.get_Allowblog()), 
						DbHelper.MakeInParam("?alloweditrules", DbType.Boolean, 4, foruminfo.get_Alloweditrules()), 
						DbHelper.MakeInParam("?allowthumbnail", DbType.Boolean, 4, foruminfo.get_Allowthumbnail()), 
						DbHelper.MakeInParam("?allowtag", DbType.Boolean, 4, foruminfo.get_Allowtag()), 
						DbHelper.MakeInParam("?istrade", DbType.Boolean, 4, foruminfo.get_Istrade()), 
						DbHelper.MakeInParam("?recyclebin", DbType.Boolean, 4, foruminfo.get_Recyclebin()), 
						DbHelper.MakeInParam("?modnewposts", DbType.Boolean, 4, foruminfo.get_Modnewposts()), 
						DbHelper.MakeInParam("?jammer", DbType.Boolean, 4, foruminfo.get_Jammer()), 
						DbHelper.MakeInParam("?disablewatermark", DbType.Boolean, 4, foruminfo.get_Disablewatermark()), 
						DbHelper.MakeInParam("?inheritedmod", DbType.Boolean, 4, foruminfo.get_Inheritedmod()), 
						DbHelper.MakeInParam("?autoclose", DbType.Boolean, 2, foruminfo.get_Autoclose()), 
						DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, foruminfo.get_Displayorder()), 
						DbHelper.MakeInParam("?allowpostspecial", DbType.Boolean, 4, foruminfo.get_Allowpostspecial()), 
						DbHelper.MakeInParam("?allowspecialonly", DbType.Boolean, 4, foruminfo.get_Allowspecialonly()), 
						DbHelper.MakeInParam("?fid", DbType.Boolean, 4, foruminfo.get_Fid())
					};
					string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET `name`=?name, `status`=?status, `colcount`=?colcount, `templateid`=?templateid,`allowsmilies`=?allowsmilies ,`allowrss`=?allowrss, `allowhtml`=?allowhtml, `allowbbcode`=?allowbbcode, `allowimgcode`=?allowimgcode, `allowblog`=?allowblog,`istrade`=?istrade,`alloweditrules`=?alloweditrules ,`allowthumbnail`=?allowthumbnail ,`allowtag`=?allowtag,`recyclebin`=?recyclebin, `modnewposts`=?modnewposts,`jammer`=?jammer,`disablewatermark`=?disablewatermark,`inheritedmod`=?inheritedmod,`autoclose`=?autoclose,`displayorder`=?displayorder,`allowpostspecial`=?allowpostspecial,`allowspecialonly`=?allowspecialonly WHERE `fid`=?fid";
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, text, array);
					DbParameter[] array2 = new DbParameter[]
					{
						DbHelper.MakeInParam("?description", (DbType)752, 0, foruminfo.get_Description()), 
						DbHelper.MakeInParam("?password", (DbType)253, 16, foruminfo.get_Password()), 
						DbHelper.MakeInParam("?icon", (DbType)253, 255, foruminfo.get_Icon()), 
						DbHelper.MakeInParam("?redirect", (DbType)253, 255, foruminfo.get_Redirect()), 
						DbHelper.MakeInParam("?attachextensions", (DbType)253, 255, foruminfo.get_Attachextensions()), 
						DbHelper.MakeInParam("?rules", (DbType)752, 0, foruminfo.get_Rules()), 
						DbHelper.MakeInParam("?topictypes", (DbType)752, 0, foruminfo.get_Topictypes()), 
						DbHelper.MakeInParam("?viewperm", (DbType)752, 0, foruminfo.get_Viewperm()), 
						DbHelper.MakeInParam("?postperm", (DbType)752, 0, foruminfo.get_Postperm()), 
						DbHelper.MakeInParam("?replyperm", (DbType)752, 0, foruminfo.get_Replyperm()), 
						DbHelper.MakeInParam("?getattachperm", (DbType)752, 0, foruminfo.get_Getattachperm()), 
						DbHelper.MakeInParam("?postattachperm", (DbType)752, 0, foruminfo.get_Postattachperm()), 
						DbHelper.MakeInParam("?applytopictype", DbType.Boolean, 1, foruminfo.get_Applytopictype()), 
						DbHelper.MakeInParam("?postbytopictype", DbType.Boolean, 1, foruminfo.get_Postbytopictype()), 
						DbHelper.MakeInParam("?viewbytopictype", DbType.Boolean, 1, foruminfo.get_Viewbytopictype()), 
						DbHelper.MakeInParam("?topictypeprefix", DbType.Boolean, 1, foruminfo.get_Topictypeprefix()), 
						DbHelper.MakeInParam("?permuserlist", (DbType)752, 0, foruminfo.get_Permuserlist()), 
						DbHelper.MakeInParam("?fid", DbType.Boolean, 4, foruminfo.get_Fid())
					};
					text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forumfields` SET `description`=?description,`password`=?password,`icon`=?icon,`redirect`=?redirect,`attachextensions`=?attachextensions,`rules`=?rules,`topictypes`=?topictypes,`viewperm`=?viewperm,`postperm`=?postperm,`replyperm`=?replyperm,`getattachperm`=?getattachperm,`postattachperm`=?postattachperm,`applytopictype`=?applytopictype,`postbytopictype`=?postbytopictype,`viewbytopictype`=?viewbytopictype,`topictypeprefix`=?topictypeprefix,`permuserlist`=?permuserlist WHERE `fid`=?fid";
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, text, array2);
					mySqlTransaction.Commit();
				}
				catch (Exception ex)
				{
					mySqlTransaction.Rollback();
					throw ex;
				}
			}
			mySqlConnection.Close();
		}
		public int InsertForumsInf(ForumInfo foruminfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?parentid", DbType.Boolean, 2, foruminfo.get_Parentid()), 
				DbHelper.MakeInParam("?layer", DbType.Boolean, 4, foruminfo.get_Layer()), 
				DbHelper.MakeInParam("?pathlist", (DbType)752, 3000, (foruminfo.get_Pathlist() == null) ? " " : foruminfo.get_Pathlist()), 
				DbHelper.MakeInParam("?parentidlist", (DbType)752, 300, (foruminfo.get_Parentidlist() == null) ? " " : foruminfo.get_Parentidlist()), 
				DbHelper.MakeInParam("?subforumcount", DbType.Boolean, 4, foruminfo.get_Subforumcount()), 
				DbHelper.MakeInParam("?name", (DbType)253, 50, foruminfo.get_Name()), 
				DbHelper.MakeInParam("?status", DbType.Boolean, 4, foruminfo.get_Status()), 
				DbHelper.MakeInParam("?colcount", DbType.Boolean, 4, foruminfo.get_Colcount()), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, foruminfo.get_Displayorder()), 
				DbHelper.MakeInParam("?templateid", DbType.Boolean, 2, foruminfo.get_Templateid()), 
				DbHelper.MakeInParam("?allowsmilies", DbType.Boolean, 4, foruminfo.get_Allowsmilies()), 
				DbHelper.MakeInParam("?allowrss", DbType.Boolean, 6, foruminfo.get_Allowrss()), 
				DbHelper.MakeInParam("?allowhtml", DbType.Boolean, 4, foruminfo.get_Allowhtml()), 
				DbHelper.MakeInParam("?allowbbcode", DbType.Boolean, 4, foruminfo.get_Allowbbcode()), 
				DbHelper.MakeInParam("?allowimgcode", DbType.Boolean, 4, foruminfo.get_Allowimgcode()), 
				DbHelper.MakeInParam("?allowblog", DbType.Boolean, 4, foruminfo.get_Allowblog()), 
				DbHelper.MakeInParam("?istrade", DbType.Boolean, 4, foruminfo.get_Istrade()), 
				DbHelper.MakeInParam("?alloweditrules", DbType.Boolean, 4, foruminfo.get_Alloweditrules()), 
				DbHelper.MakeInParam("?allowthumbnail", DbType.Boolean, 4, foruminfo.get_Allowthumbnail()), 
				DbHelper.MakeInParam("?allowtag", DbType.Boolean, 4, foruminfo.get_Allowtag()), 
				DbHelper.MakeInParam("?recyclebin", DbType.Boolean, 4, foruminfo.get_Recyclebin()), 
				DbHelper.MakeInParam("?modnewposts", DbType.Boolean, 4, foruminfo.get_Modnewposts()), 
				DbHelper.MakeInParam("?jammer", DbType.Boolean, 4, foruminfo.get_Jammer()), 
				DbHelper.MakeInParam("?disablewatermark", DbType.Boolean, 4, foruminfo.get_Disablewatermark()), 
				DbHelper.MakeInParam("?inheritedmod", DbType.Boolean, 4, foruminfo.get_Inheritedmod()), 
				DbHelper.MakeInParam("?autoclose", DbType.Boolean, 2, foruminfo.get_Autoclose()), 
				DbHelper.MakeInParam("?allowpostspecial", DbType.Boolean, 4, foruminfo.get_Allowpostspecial()), 
				DbHelper.MakeInParam("?allowspecialonly", DbType.Boolean, 4, foruminfo.get_Allowspecialonly())
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "forums` (`parentid`,`layer`,`pathlist`,`parentidlist`,`subforumcount`,`name`,`status`,`colcount`,`displayorder`,`templateid`,`allowsmilies`,`allowrss`,`allowhtml`,`allowbbcode`,`allowimgcode`,`allowblog`,`istrade`,`alloweditrules`,`recyclebin`,`modnewposts`,`jammer`,`disablewatermark`,`inheritedmod`,`autoclose`,`allowthumbnail`,`allowtag`,`allowpostspecial`,`allowspecialonly`) VALUES (?parentid,?layer,?pathlist,?parentidlist,?subforumcount,?name,?status, ?colcount, ?displayorder,?templateid,?allowsmilies,?allowrss,?allowhtml,?allowbbcode,?allowimgcode,?allowblog,?istrade,?alloweditrules,?recyclebin,?modnewposts,?jammer,?disablewatermark,?inheritedmod,?autoclose,?allowthumbnail,?allowtag,?allowpostspecial,?allowspecialonly)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			int maxForumId = this.GetMaxForumId();
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, maxForumId), 
				DbHelper.MakeInParam("?description", (DbType)752, 0, foruminfo.get_Description()), 
				DbHelper.MakeInParam("?password", (DbType)253, 16, foruminfo.get_Password()), 
				DbHelper.MakeInParam("?icon", (DbType)253, 255, foruminfo.get_Icon()), 
				DbHelper.MakeInParam("?postcredits", (DbType)253, 255, foruminfo.get_Postcredits()), 
				DbHelper.MakeInParam("?replycredits", (DbType)253, 255, foruminfo.get_Replycredits()), 
				DbHelper.MakeInParam("?redirect", (DbType)253, 255, foruminfo.get_Redirect()), 
				DbHelper.MakeInParam("?attachextensions", (DbType)253, 255, foruminfo.get_Attachextensions()), 
				DbHelper.MakeInParam("?moderators", (DbType)752, 0, foruminfo.get_Moderators()), 
				DbHelper.MakeInParam("?rules", (DbType)752, 0, foruminfo.get_Rules()), 
				DbHelper.MakeInParam("?topictypes", (DbType)752, 0, foruminfo.get_Topictypes()), 
				DbHelper.MakeInParam("?viewperm", (DbType)752, 0, foruminfo.get_Viewperm()), 
				DbHelper.MakeInParam("?postperm", (DbType)752, 0, foruminfo.get_Postperm()), 
				DbHelper.MakeInParam("?replyperm", (DbType)752, 0, foruminfo.get_Replyperm()), 
				DbHelper.MakeInParam("?getattachperm", (DbType)752, 0, foruminfo.get_Getattachperm()), 
				DbHelper.MakeInParam("?postattachperm", (DbType)752, 0, foruminfo.get_Postattachperm())
			};
			text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "forumfields` (`fid`,`description`,`password`,`icon`,`postcredits`,`replycredits`,`redirect`,`attachextensions`,`moderators`,`rules`,`topictypes`,`viewperm`,`postperm`,`replyperm`,`getattachperm`,`postattachperm`) VALUES (?fid,?description,?password,?icon,?postcredits,?replycredits,?redirect,?attachextensions,?moderators,?rules,?topictypes,?viewperm,?postperm,?replyperm,?getattachperm,?postattachperm)";
			DbHelper.ExecuteDataset(CommandType.Text, text, array2);
			return maxForumId;
		}
		public void SetForumsPathList(string pathlist, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pathlist", DbType.Single, 3000, pathlist), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET pathlist=?pathlist  WHERE `fid`=?fid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void SetForumslayer(int layer, string parentidlist, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?layer", DbType.Byte, 2, layer), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid), 
				DbHelper.MakeInParam("?parentidlist", (DbType)254, 300, parentidlist)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET layer=?layer WHERE `fid`=?fid", array);
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET parentidlist='0' WHERE `fid`=?fid", array);
		}
		public int GetForumsParentidByFid(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "SELECT `parentid` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` WHERE fid=?fid LIMIT 1";
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, text, array));
		}
		public void MovingForumsPos(string currentfid, string targetfid, bool isaschildnode, string extname)
		{
			DataRow dataRow = DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"Select *  From ", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums Where fid=", 
				currentfid.ToString(), 
				" LIMIT 1"
			})).Tables[0].Rows[0];
			DataRow dataRow2 = DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"Select *  From ", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums Where fid=", 
				targetfid.ToString(), 
				" LIMIT 1"
			})).Tables[0].Rows[0];
			if (DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `fid` From ", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums Where parentid=", 
				currentfid.ToString(), 
				" LIMIT 1"
			})).Tables[0].Rows.Count > 0)
			{
				if (isaschildnode)
				{
					string text = string.Format("Update " + BaseConfigs.get_GetTablePrefix() + "forums Set displayorder=displayorder+1 Where `displayorder`>={0}", Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString()) + 1));
					DbHelper.ExecuteDataset(CommandType.Text, text);
					text = string.Format("Update " + BaseConfigs.get_GetTablePrefix() + "forums Set parentid='{1}',displayorder='{2}'  Where `fid`={0}", currentfid, dataRow2["fid"].ToString(), Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString().Trim()) + 1));
					DbHelper.ExecuteDataset(CommandType.Text, text);
				}
				else
				{
					string text = string.Format("Update `" + BaseConfigs.get_GetTablePrefix() + "forums` Set displayorder=displayorder+1 Where `displayorder`>={0} OR `fid`={1}", Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString())), dataRow2["fid"].ToString());
					DbHelper.ExecuteDataset(CommandType.Text, text);
					text = string.Format("Update " + BaseConfigs.get_GetTablePrefix() + "forums Set parentid='{1}',displayorder='{2}'  Where `fid`={0}", currentfid, dataRow2["parentid"].ToString(), Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString().Trim())));
					DbHelper.ExecuteDataset(CommandType.Text, text);
				}
				if (dataRow["topics"].ToString() != "0" && Convert.ToInt32(dataRow["topics"].ToString()) > 0 && dataRow["posts"].ToString() != "0" && Convert.ToInt32(dataRow["posts"].ToString()) > 0)
				{
					if (dataRow["parentidlist"].ToString().Trim() != "")
					{
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
						{
							"UPDATE ", 
							BaseConfigs.get_GetTablePrefix(), 
							"forums SET `topics`=`topics`-", 
							dataRow["topics"].ToString(), 
							",`posts`=`posts`-", 
							dataRow["posts"].ToString(), 
							"  WHERE `fid` IN(", 
							dataRow["parentidlist"].ToString().Trim(), 
							")"
						}));
					}
					if (dataRow2["parentidlist"].ToString().Trim() != "")
					{
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
						{
							"UPDATE ", 
							BaseConfigs.get_GetTablePrefix(), 
							"forums SET `topics`=`topics`+", 
							dataRow["topics"].ToString(), 
							",`posts`=`posts`+", 
							dataRow["posts"].ToString(), 
							"  WHERE `fid` IN(", 
							dataRow2["parentidlist"].ToString().Trim(), 
							")"
						}));
					}
				}
			}
			else
			{
				DbHelper.ExecuteDataset(CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set subforumcount=subforumcount-1 Where `fid`=" + dataRow["parentid"].ToString());
				if (isaschildnode)
				{
					if (dataRow["topics"].ToString() != "0" && Convert.ToInt32(dataRow["topics"].ToString()) > 0 && dataRow["posts"].ToString() != "0" && Convert.ToInt32(dataRow["posts"].ToString()) > 0)
					{
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
						{
							"UPDATE ", 
							BaseConfigs.get_GetTablePrefix(), 
							"forums SET `topics`=`topics`-", 
							dataRow["topics"].ToString(), 
							",`posts`=`posts`-", 
							dataRow["posts"].ToString(), 
							"  WHERE `fid` IN(", 
							dataRow["parentidlist"].ToString(), 
							")"
						}));
						if (dataRow2["parentidlist"].ToString().Trim() != "")
						{
							DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
							{
								"UPDATE ", 
								BaseConfigs.get_GetTablePrefix(), 
								"forums SET `topics`=`topics`+", 
								dataRow["topics"].ToString(), 
								",`posts`=`posts`+", 
								dataRow["posts"].ToString(), 
								"  WHERE `fid` IN(", 
								dataRow2["parentidlist"].ToString(), 
								",", 
								targetfid, 
								")"
							}));
						}
					}
					string text = string.Format("Update " + BaseConfigs.get_GetTablePrefix() + "forums Set displayorder=displayorder+1 Where `displayorder`>={0}", Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString()) + 1));
					DbHelper.ExecuteDataset(CommandType.Text, text);
					DbHelper.ExecuteDataset(CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set subforumcount=subforumcount+1 Where `fid`=" + targetfid);
					string text2 = null;
					if (dataRow2["parentidlist"].ToString().Trim() == "0")
					{
						text2 = targetfid;
					}
					else
					{
						text2 = dataRow2["parentidlist"].ToString().Trim() + "," + targetfid;
					}
					text = string.Format("Update " + BaseConfigs.get_GetTablePrefix() + "forums Set parentid='{1}',layer='{2}',pathlist='{3}', parentidlist='{4}',displayorder='{5}' Where `fid`={0}", new object[]
					{
						currentfid, 
						dataRow2["fid"].ToString(), 
						Convert.ToString(Convert.ToInt32(dataRow2["layer"].ToString()) + 1), 
						string.Concat(new string[]
						{
							dataRow2["pathlist"].ToString().Trim(), 
							"<a href=\"showforum-", 
							currentfid, 
							extname, 
							"\">", 
							dataRow["name"].ToString().Trim(), 
							"</a>"
						}), 
						text2, 
						Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString().Trim()) + 1)
					});
					DbHelper.ExecuteDataset(CommandType.Text, text);
				}
				else
				{
					if (dataRow["topics"].ToString() != "0" && Convert.ToInt32(dataRow["topics"].ToString()) > 0 && dataRow["posts"].ToString() != "0" && Convert.ToInt32(dataRow["posts"].ToString()) > 0)
					{
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
						{
							"UPDATE ", 
							BaseConfigs.get_GetTablePrefix(), 
							"forums SET `topics`=`topics`-", 
							dataRow["topics"].ToString(), 
							",`posts`=`posts`-", 
							dataRow["posts"].ToString(), 
							"  WHERE `fid` IN(", 
							dataRow["parentidlist"].ToString(), 
							")"
						}));
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
						{
							"UPDATE ", 
							BaseConfigs.get_GetTablePrefix(), 
							"forums SET `topics`=`topics`+", 
							dataRow["topics"].ToString(), 
							",`posts`=`posts`+", 
							dataRow["posts"].ToString(), 
							"  WHERE `fid` IN(", 
							dataRow2["parentidlist"].ToString(), 
							")"
						}));
					}
					string text = string.Format("Update `" + BaseConfigs.get_GetTablePrefix() + "forums` Set displayorder=displayorder+1 Where `displayorder`>={0} OR `fid`={1}", Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString()) + 1), dataRow2["fid"].ToString());
					DbHelper.ExecuteDataset(CommandType.Text, text);
					DbHelper.ExecuteDataset(CommandType.Text, "Update `" + BaseConfigs.get_GetTablePrefix() + "forums`  Set subforumcount=subforumcount+1 Where `fid`=" + dataRow2["parentid"].ToString());
					string text3 = "";
					DataTable dataTable = DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
					{
						"SELECT `pathlist` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forums` WHERE `fid`=", 
						dataRow2["parentid"].ToString(), 
						" LIMIT 1"
					})).Tables[0];
					if (dataTable.Rows.Count > 0)
					{
						text3 = DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
						{
							"SELECT `pathlist` FROM `", 
							BaseConfigs.get_GetTablePrefix(), 
							"forums` WHERE `fid`=", 
							dataRow2["parentid"].ToString(), 
							" LIMIT 1"
						})).Tables[0].Rows[0][0].ToString().Trim();
					}
					text = string.Format("Update `" + BaseConfigs.get_GetTablePrefix() + "forums` SET parentid='{1}',layer='{2}',pathlist='{3}', parentidlist='{4}',displayorder='{5}' Where `fid`={0}", new object[]
					{
						currentfid, 
						dataRow2["parentid"].ToString(), 
						Convert.ToInt32(dataRow2["layer"].ToString()), 
						string.Concat(new string[]
						{
							text3, 
							"<a href=\"showforum-", 
							currentfid, 
							extname, 
							"\">", 
							DataProvider.RegEsc(dataRow["name"].ToString().Trim()), 
							"</a>"
						}), 
						dataRow2["parentidlist"].ToString().Trim(), 
						Convert.ToString(Convert.ToInt32(dataRow2["displayorder"].ToString().Trim()))
					});
					DbHelper.ExecuteDataset(CommandType.Text, text);
				}
			}
		}
		public bool IsExistSubForum(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "Select * From " + BaseConfigs.get_GetTablePrefix() + "forums Where parentid=?fid LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0].Rows.Count > 0;
		}
		public void DeleteForumsByFid(string postname, string fid)
		{
			MySqlConnection mySqlConnection = new MySqlConnection(DbHelper.get_ConnectionString());
			mySqlConnection.Open();
			using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
			{
				try
				{
					DataRow dataRow = DbHelper.ExecuteDataset(mySqlTransaction, CommandType.Text, "Select * From " + BaseConfigs.get_GetTablePrefix() + "forums Where fid=" + fid.ToString()).Tables[0].Rows[0];
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set displayorder=displayorder-1 Where  displayorder>" + dataRow["displayorder"].ToString());
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set subforumcount=subforumcount-1 Where  fid=" + dataRow["parentid"].ToString());
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "Delete From " + BaseConfigs.get_GetTablePrefix() + "forumfields Where  fid=" + fid);
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, string.Concat(new string[]
					{
						"DELETE FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"polls` WHERE `tid` IN(SELECT `tid` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` WHERE `fid`=", 
						fid, 
						")"
					}));
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, string.Concat(new string[]
					{
						"DELETE FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"attachments` WHERE `tid` IN(SELECT `tid` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` WHERE `fid`=", 
						fid, 
						") OR `pid` IN(SELECT `pid` FROM `", 
						postname, 
						"` WHERE `fid`=", 
						fid, 
						")"
					}));
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "DELETE FROM `" + postname + "` WHERE `fid`=" + fid);
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `fid`=" + fid);
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "Delete From `" + BaseConfigs.get_GetTablePrefix() + "forums` Where  `fid`=" + fid);
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "Delete From `" + BaseConfigs.get_GetTablePrefix() + "moderators` Where  `fid`=" + fid);
					mySqlTransaction.Commit();
				}
				catch
				{
					mySqlTransaction.Rollback();
				}
			}
			mySqlConnection.Close();
		}
		public DataTable GetParentIdByFid(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "SELECT `parentid` From `" + BaseConfigs.get_GetTablePrefix() + "forums` WHERE `inheritedmod`=1 AND `fid`=?fid";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void InsertForumsModerators(string fid, string moderators, int displayorder, int inherited)
		{
			int num = displayorder;
			string text = "";
			string[] array = moderators.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				if (text2.Trim() != "")
				{
					DbParameter[] array2 = new DbParameter[]
					{
						DbHelper.MakeInParam("?username", DbType.Single, 20, text2.Trim())
					};
					DataTable dataTable = DbHelper.ExecuteDataset(CommandType.Text, "Select `uid` From `" + BaseConfigs.get_GetTablePrefix() + "users` Where `groupid`<>7 AND `groupid`<>8 AND `username`=?username LIMIT 1", array2).Tables[0];
					if (dataTable.Rows.Count > 0)
					{
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
						{
							"INSERT INTO `", 
							BaseConfigs.get_GetTablePrefix(), 
							"moderators` (uid,fid,displayorder,inherited) VALUES(", 
							dataTable.Rows[0][0].ToString(), 
							",", 
							fid, 
							",", 
							num.ToString(), 
							",", 
							inherited.ToString(), 
							")"
						}));
						text = text + text2.Trim() + ",";
						num++;
					}
				}
			}
			if (text != "")
			{
				DbParameter[] array3 = new DbParameter[]
				{
					DbHelper.MakeInParam("?moderators", DbType.Single, 255, text.Substring(0, text.Length - 1))
				};
				DbHelper.ExecuteNonQuery(CommandType.Text, "Update `" + BaseConfigs.get_GetTablePrefix() + "forumfields` SET `moderators`=?moderators  WHERE `fid` =" + fid, array3);
			}
			else
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "Update `" + BaseConfigs.get_GetTablePrefix() + "forumfields` SET `moderators`='' WHERE `fid` =" + fid);
			}
		}
		public DataTable GetFidInForumsByParentid(int parentid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?parentid", DbType.Boolean, 4, parentid)
			};
			string text = "Select fid From `" + BaseConfigs.get_GetTablePrefix() + "forums` Where `parentid`=?parentid ORDER BY `displayorder` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void CombinationForums(string sourcefid, string targetfid, string fidlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `fid`=", 
				targetfid, 
				"  WHERE `fid`=", 
				sourcefid
			}));
			int num = Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT COUNT(tid)  FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `fid` IN(", 
				fidlist, 
				")"
			})).Tables[0].Rows[0][0].ToString());
			int num2 = 0;
			foreach (DataRow dataRow in DbHelper.ExecuteDataset(CommandType.Text, "SELECT `id` From `" + BaseConfigs.get_GetTablePrefix() + "tablelist`").Tables[0].Rows)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					dataRow["id"].ToString(), 
					"` SET `fid`=", 
					targetfid, 
					"  WHERE `fid`=", 
					sourcefid
				}));
				num2 += Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
				{
					"SELECT COUNT(pid)  FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					dataRow["id"].ToString(), 
					"` WHERE `fid` IN(", 
					fidlist, 
					")"
				})).Tables[0].Rows[0][0].ToString());
			}
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` SET `topics`=", 
				num, 
				" ,`posts`=", 
				num2, 
				" WHERE `fid`=", 
				targetfid
			}));
			DataRow dataRow2 = DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"Select * From ", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums Where fid=", 
				sourcefid.ToString(), 
				" LIMIT 1"
			})).Tables[0].Rows[0];
			DbHelper.ExecuteNonQuery(CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set displayorder=displayorder-1 Where  displayorder>" + dataRow2["displayorder"].ToString());
			DbHelper.ExecuteNonQuery(CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forums Set subforumcount=subforumcount-1 Where  fid=" + dataRow2["parentid"].ToString());
			DbHelper.ExecuteNonQuery(CommandType.Text, "Delete From " + BaseConfigs.get_GetTablePrefix() + "forumfields Where  fid=" + sourcefid);
			DbHelper.ExecuteNonQuery(CommandType.Text, "Delete From " + BaseConfigs.get_GetTablePrefix() + "forums Where  fid=" + sourcefid);
		}
		public void UpdateSubForumCount(int subforumcount, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?subforumcount", DbType.Boolean, 4, subforumcount), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET `subforumcount`=?subforumcount WHERE `fid`=?fid";
			DbHelper.ExecuteDataset(CommandType.Text, text, array);
		}
		public void UpdateDisplayorderInForumByFid(int displayorder, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET `displayorder`=?displayorder WHERE `fid`=?fid";
			DbHelper.ExecuteDataset(CommandType.Text, text, array);
		}
		public DataTable GetMainForum()
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` WHERE `layer`=0 Order By `displayorder` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void SetStatusInForum(int status, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?status", DbType.Boolean, 4, status), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET `status`=?status WHERE `fid`=?fid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetForumByParentid(int parentid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?parentid", DbType.Boolean, 4, parentid)
			};
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` WHERE `parentid`=?parentid Order By DisplayOrder";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void UpdateStatusByFidlist(string fidlist)
		{
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` SET `status`=0 WHERE `fid` IN(", 
				fidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public bool BatchSetForumInf(ForumInfo foruminfo, BatchSetParams bsp, string fidlist)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder.Append("UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET ");
			if (bsp.get_SetSetting())
			{
				stringBuilder.Append("`Allowsmilies`='" + foruminfo.get_Allowsmilies() + "' ,");
				stringBuilder.Append("`Allowrss`='" + foruminfo.get_Allowrss() + "' ,");
				stringBuilder.Append("`Allowhtml`='" + foruminfo.get_Allowhtml() + "' ,");
				stringBuilder.Append("`Allowbbcode`='" + foruminfo.get_Allowbbcode() + "' ,");
				stringBuilder.Append("`Allowimgcode`='" + foruminfo.get_Allowimgcode() + "' ,");
				stringBuilder.Append("`Allowblog`='" + foruminfo.get_Allowblog() + "' ,");
				stringBuilder.Append("`istrade`='" + foruminfo.get_Istrade() + "' ,");
				stringBuilder.Append("`allowpostspecial`='" + foruminfo.get_Allowpostspecial() + "' ,");
				stringBuilder.Append("`allowspecialonly`='" + foruminfo.get_Allowspecialonly() + "' ,");
				stringBuilder.Append("`Alloweditrules`='" + foruminfo.get_Alloweditrules() + "' ,");
				stringBuilder.Append("`allowthumbnail`='" + foruminfo.get_Allowthumbnail() + "' ,");
				stringBuilder.Append("`Recyclebin`='" + foruminfo.get_Recyclebin() + "' ,");
				stringBuilder.Append("`Modnewposts`='" + foruminfo.get_Modnewposts() + "' ,");
				stringBuilder.Append("`Jammer`='" + foruminfo.get_Jammer() + "' ,");
				stringBuilder.Append("`Disablewatermark`='" + foruminfo.get_Disablewatermark() + "' ,");
				stringBuilder.Append("`Inheritedmod`='" + foruminfo.get_Inheritedmod() + "' ,");
			}
			if (stringBuilder.ToString().EndsWith(","))
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			stringBuilder.Append("WHERE `fid` IN(" + fidlist + ")");
			stringBuilder2.Append("UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forumfields` SET ");
			if (bsp.get_SetPassWord())
			{
				stringBuilder2.Append("`password`='" + foruminfo.get_Password() + "' ,");
			}
			if (bsp.get_SetAttachExtensions())
			{
				stringBuilder2.Append("`attachextensions`='" + foruminfo.get_Attachextensions() + "' ,");
			}
			if (bsp.get_SetPostCredits())
			{
				stringBuilder2.Append("`postcredits`='" + foruminfo.get_Postcredits() + "' ,");
			}
			if (bsp.get_SetReplyCredits())
			{
				stringBuilder2.Append("`replycredits`='" + foruminfo.get_Replycredits() + "' ,");
			}
			if (bsp.get_SetViewperm())
			{
				stringBuilder2.Append("`Viewperm`='" + foruminfo.get_Viewperm() + "' ,");
			}
			if (bsp.get_SetPostperm())
			{
				stringBuilder2.Append("`Postperm`='" + foruminfo.get_Postperm() + "' ,");
			}
			if (bsp.get_SetReplyperm())
			{
				stringBuilder2.Append("`Replyperm`='" + foruminfo.get_Replyperm() + "' ,");
			}
			if (bsp.get_SetGetattachperm())
			{
				stringBuilder2.Append("`Getattachperm`='" + foruminfo.get_Getattachperm() + "' ,");
			}
			if (bsp.get_SetPostattachperm())
			{
				stringBuilder2.Append("`Postattachperm`='" + foruminfo.get_Postattachperm() + "' ,");
			}
			if (stringBuilder2.ToString().EndsWith(","))
			{
				stringBuilder2.Remove(stringBuilder2.Length - 1, 1);
			}
			stringBuilder2.Append("WHERE `fid` IN(" + fidlist + ")");
			SqlConnection sqlConnection = new SqlConnection(DbHelper.get_ConnectionString());
			sqlConnection.Open();
			bool result;
			using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
			{
				try
				{
					if (stringBuilder.ToString().IndexOf("SET WHERE") < 0)
					{
						DbHelper.ExecuteNonQuery(sqlTransaction, CommandType.Text, stringBuilder.ToString());
					}
					if (stringBuilder2.ToString().IndexOf("SET WHERE") < 0)
					{
						DbHelper.ExecuteNonQuery(sqlTransaction, CommandType.Text, stringBuilder2.ToString());
					}
					sqlTransaction.Commit();
				}
				catch
				{
					sqlTransaction.Rollback();
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}
		public IDataReader GetTopForumFids(int lastfid, int statcount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastfid", DbType.Boolean, 4, lastfid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `fid` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` WHERE `fid` > ?lastfid LIMIT " + statcount.ToString(), array);
		}
		public DataSet GetOnlineList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `groupid`,(SELECT `grouptitle`  FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups`.`groupid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"onlinelist`.`groupid` LIMIT 1) As GroupName , `displayorder`,`title`,`img` From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"onlinelist` Order BY `groupid` ASC"
			}));
		}
		public int UpdateOnlineList(int groupid, int displayorder, string img, string title)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?title", DbType.Single, 50, title), 
				DbHelper.MakeInParam("?img", DbType.Single, 50, img), 
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "onlinelist` SET `displayorder`=?displayorder,`title`=?title,`img`=?img  Where `groupid`=?groupid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string GetWords()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "words`";
		}
		public DataTable GetBadWords()
		{
			string text = "SELECT `find`,`replacement` FROM `" + BaseConfigs.get_GetTablePrefix() + "words` ORDER BY `find`";
			return DbHelper.ExecuteDataset(text).Tables[0];
		}
		public int DeleteWord(int id)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?id", DbType.Boolean, 4, id);
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "words` WHERE `id`=?id";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public int GetBadWordId(string find)
		{
			string text = string.Concat(new string[]
			{
				"select id from ", 
				BaseConfigs.get_GetTablePrefix(), 
				"words where find='", 
				find, 
				"'"
			});
			return DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void UpdateBadWords(string find, string replacement)
		{
			string text = string.Concat(new string[]
			{
				"update ", 
				BaseConfigs.get_GetTablePrefix(), 
				"words set replacement='", 
				replacement, 
				"' where find ='", 
				find, 
				"'"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void InsertBadWords(string username, string find, string replacement)
		{
			string text = string.Concat(new string[]
			{
				"insert ", 
				BaseConfigs.get_GetTablePrefix(), 
				"words(admin,find,replacement) values('", 
				username, 
				"','", 
				find, 
				"','", 
				replacement, 
				"')"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void DeleteBadWords()
		{
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "words` ";
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public int UpdateWord(int id, string find, string replacement)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?find", DbType.Single, 255, find), 
				DbHelper.MakeInParam("?replacement", DbType.Single, 255, replacement), 
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "words` SET `find`=?find, `replacement`=?replacement  Where `id`=?id";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int DeleteWords(string idlist)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"words`  WHERE `ID` IN(", 
				idlist, 
				")"
			}));
		}
		public bool ExistWord(string find)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?find", DbType.Single, 255, find);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "words` WHERE `find`=?find LIMIT 1", new DbParameter[]
			{
				dbParameter
			}).Tables[0].Rows.Count > 0;
		}
		public int AddWord(string username, string find, string replacement)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", DbType.Single, 20, username), 
				DbHelper.MakeInParam("?find", DbType.Single, 255, find), 
				DbHelper.MakeInParam("?replacement", DbType.Single, 255, replacement)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "words` (`admin`, `find`, `replacement`) VALUES (?username,?find,?replacement)";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public bool IsExistTopicType(string typename, int currenttypeid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?typename", DbType.Single, 30, typename), 
				DbHelper.MakeInParam("?currenttypeid", DbType.Boolean, 4, currenttypeid)
			};
			string text = "SELECT typeid FROM `" + BaseConfigs.get_GetTablePrefix() + "topictypes` WHERE name=?typename AND typeid<>?currenttypeid";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0].Rows.Count != 0;
		}
		public bool IsExistTopicType(string typename)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?typename", DbType.Single, 30, typename);
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topictypes` WHERE `name`=?typename LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			}).Tables[0].Rows.Count != 0;
		}
		public string GetTopicTypes()
		{
			return "SELECT typeid as id,name,displayorder,description FROM `" + BaseConfigs.get_GetTablePrefix() + "topictypes` ORDER BY `displayorder` ASC";
		}
		public DataTable GetExistTopicTypeOfForum()
		{
			string text = "SELECT `fid`,`topictypes` FROM `" + BaseConfigs.get_GetTablePrefix() + "forumfields` WHERE `topictypes` NOT LIKE ''";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void UpdateTopicTypeForForum(string topictypes, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?topictypes", DbType.Single, 0, topictypes), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forumfields` SET `topictypes`=?topictypes WHERE `fid`=?fid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateTopicTypes(string name, int displayorder, string description, int typeid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?name", DbType.Single, 100, name), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?description", DbType.Single, 500, description), 
				DbHelper.MakeInParam("?typeid", DbType.Boolean, 4, typeid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topictypes` SET `name`=?name ,`displayorder`=?displayorder, `description`=?description Where `typeid`=?typeid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void AddTopicTypes(string typename, int displayorder, string description)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?name", DbType.Single, 100, typename), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?description", DbType.Single, 500, description)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "topictypes` (`name`,`displayorder`,`description`) VALUES(?name,?displayorder,?description)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int GetMaxTopicTypesId()
		{
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, "SELECT MAX(`typeid`) FROM `" + BaseConfigs.get_GetTablePrefix() + "topictypes`").ToString());
		}
		public void DeleteTopicTypesByTypeidlist(string typeidlist)
		{
			string text = string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topictypes`  WHERE `typeid` IN(", 
				typeidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public DataTable GetForumNameIncludeTopicType()
		{
			string text = string.Concat(new string[]
			{
				"SELECT f1.`fid`,`name`,`topictypes` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` AS f1 LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` AS f2 ON f1.fid=f2.fid"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetForumTopicType()
		{
			string text = "SELECT `fid`,`topictypes` FROM `" + BaseConfigs.get_GetTablePrefix() + "forumfields`";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void ClearTopicTopicType(int typeid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?typeid", DbType.Boolean, 4, typeid);
			string text = "Update " + BaseConfigs.get_GetTablePrefix() + "topics Set `typeid`=0 Where typeid=?typeid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public string GetTopicTypeInfo()
		{
			return "SELECT typeid as id,name,description FROM `" + BaseConfigs.get_GetTablePrefix() + "topictypes` ORDER BY `displayorder` ASC";
		}
		public string GetTemplateName()
		{
			return "Select templateid,name From `" + BaseConfigs.get_GetTablePrefix() + "templates`";
		}
		public DataTable GetAttachType()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "Select id,extension  From `" + BaseConfigs.get_GetTablePrefix() + "attachtypes`  Order By `id` ASC").Tables[0];
		}
		public void UpdatePermUserListByFid(string permuserlist, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?permuserlist", DbType.Single, 0, permuserlist), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "Update " + BaseConfigs.get_GetTablePrefix() + "forumfields Set Permuserlist=?permuserlist Where fid=?fid", array);
		}
		public IDataReader GetTopicsIdentifyItem()
		{
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topicidentify`");
		}
		public string ResetTopTopicListSql(int layer, string fid, string parentidlist)
		{
			string text = "";
			switch (layer)
			{
				case 0:
				{
					text = string.Format("`fid`<>{0} AND (',' + TRIM(`parentidlist`) + ',' LIKE '%,{1},%')", fid.ToString(), DataProvider.RegEsc(fid.ToString()));
					break;
				}
				case 1:
				{
					text = parentidlist.ToString().Trim();
					if (text != string.Empty)
					{
						text = string.Format("`fid`<>{0} AND (`fid`={1} OR (',' + TRIM(`parentidlist`) + ',' LIKE '%,{2},%'))", fid.ToString().Trim(), text, DataProvider.RegEsc(text));
					}
					else
					{
						text = string.Format("`fid`<>{0} AND (',' + TRIM(`parentidlist`) + ',' LIKE '%,{1},%')", fid.ToString().Trim(), DataProvider.RegEsc(text));
					}
					break;
				}
				default:
				{
					text = parentidlist.ToString().Trim();
					if (text != string.Empty)
					{
						text = Utils.CutString(text, 0, text.IndexOf(","));
						text = string.Format("`fid`<>{0} AND (`fid`={1} OR (',' + TRIM(`parentidlist`) + ',' LIKE '%,{2},%'))", fid.ToString().Trim(), text, DataProvider.RegEsc(text));
					}
					else
					{
						text = string.Format("`fid`<>{0} AND (',' + TRIM(`parentidlist`) + ',' LIKE '%,{1},%')", fid.ToString().Trim(), DataProvider.RegEsc(text));
					}
					break;
				}
			}
			return text;
		}
		public string showforumcondition(int sqlid, int cond)
		{
			string result = null;
			switch (sqlid)
			{
				case 1:
				{
					result = " AND `typeid`=";
					break;
				}
				case 2:
				{
					string arg_4B_0 = " AND `postdatetime`>='";
					DateTime dateTime = DateTime.Now;
					dateTime = dateTime.AddDays((double)(-1 * cond));
					result = arg_4B_0 + dateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
					break;
				}
				case 3:
				{
					result = " `tid`";
					break;
				}
			}
			return result;
		}
		public string DelVisitLogCondition(string deletemod, string visitid, string deletenum, string deletefrom)
		{
			string result = "";
			if (deletemod != null)
			{
				if (!(deletemod == "chkall"))
				{
					if (!(deletemod == "deleteNum"))
					{
						if (deletemod == "deleteFrom")
						{
							if (deletefrom != "")
							{
								result = " `postdatetime`<'" + deletefrom + "'";
							}
						}
					}
					else
					{
						if (deletenum != "" && Utils.IsNumeric(deletenum))
						{
							result = string.Concat(new string[]
							{
								" `visitid` not in (select `visitid` from `", 
								BaseConfigs.get_GetTablePrefix(), 
								"adminvisitlog` order by `visitid` desc limit 0,", 
								deletenum, 
								")"
							});
						}
					}
				}
				else
				{
					if (visitid != "")
					{
						result = " `visitid` IN(" + visitid + ")";
					}
				}
			}
			return result;
		}
		public string AttachDataBind(string condition, string postname)
		{
			return string.Concat(new string[]
			{
				"Select `aid`, `attachment`, `filename`, (Select `poster` FROM `", 
				postname, 
				"` WHERE `", 
				postname, 
				"`.`pid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"attachments`.`pid` LIMIT 1) AS `poster`,(Select `title` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`tid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"attachments`.`tid` LIMIT 1) AS `topictitle`, `filesize`,`downloads`  From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"attachments` ", 
				condition
			});
		}
		public DataTable GetAttachDataTable(string condition, string postname)
		{
			string text = string.Concat(new string[]
			{
				"Select `aid`, `attachment`, `filename`, (Select `poster` FROM `", 
				postname, 
				"` WHERE `", 
				postname, 
				"`.`pid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"attachments`.`pid` LIMIT 1) AS `poster`,(Select `title` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`tid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"attachments`.`tid` LIMIT 1) AS `topictitle`, `filesize`,`downloads`  From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"attachments` ", 
				condition
			});
			return DbHelper.ExecuteDataset(text).Tables[0];
		}
		public bool AuditTopicCount(string condition)
		{
			return DbHelper.ExecuteDataset("Select count(tid) From `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE " + condition).Tables[0].Rows[0][0].ToString() == "0";
		}
		public string AuditTopicBindStr(string condition)
		{
			return "Select * From `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE " + condition;
		}
		public DataTable AuditTopicBind(string condition)
		{
			return DbHelper.ExecuteDataset("Select * From `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE " + condition).Tables[0];
		}
		public string AuditNewUserClear(string searchuser, string regbefore, string regip)
		{
			string text = "";
			text += " `groupid`=8";
			if (searchuser != "")
			{
				text = text + " AND `username` LIKE '%" + searchuser + "%'";
			}
			if (regbefore != "")
			{
				string arg_78_0 = text;
				string arg_78_1 = " AND `joindate`<='";
				DateTime dateTime = DateTime.Now;
				dateTime = dateTime.AddDays(-Convert.ToDouble(regbefore));
				text = arg_78_0 + arg_78_1 + dateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
			}
			if (regip != "")
			{
				text = text + " AND `regip` LIKE '" + DataProvider.RegEsc(regip) + "%'";
			}
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE " + text;
		}
		public void UpdateStatusByFidlistOther(string fidlist)
		{
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` SET `status`=1 WHERE `status`>1 AND `fid` IN(", 
				fidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public string DelMedalLogCondition(string deletemode, string id, string deleteNum, string deleteFrom)
		{
			string result = "";
			if (deletemode != null)
			{
				if (!(deletemode == "chkall"))
				{
					if (!(deletemode == "deleteNum"))
					{
						if (deletemode == "deleteFrom")
						{
							if (deleteFrom != "")
							{
								result = " `postdatetime`<'" + DateTime.Parse(deleteFrom).ToString("yyyy-MM-dd HH:mm:ss") + "'";
							}
						}
					}
					else
					{
						if (deleteNum != "" && Utils.IsNumeric(deleteNum))
						{
							result = string.Concat(new string[]
							{
								" `id` not in (select `id` from `", 
								BaseConfigs.get_GetTablePrefix(), 
								"medalslog` order by `id` desc LIMIT 0,", 
								deleteNum, 
								")"
							});
						}
					}
				}
				else
				{
					if (id != "")
					{
						result = " `id` IN(" + id + ")";
					}
				}
			}
			return result;
		}
		public DataTable MedalsTable(string medalid)
		{
			return DbHelper.ExecuteDataset("SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "medals` WHERE `medalid`=" + medalid).Tables[0];
		}
		public string DelModeratorManageCondition(string deletemode, string id, string deleteNum, string deleteFrom)
		{
			string result = "";
			if (deletemode != null)
			{
				if (!(deletemode == "chkall"))
				{
					if (!(deletemode == "deleteNum"))
					{
						if (deletemode == "deleteFrom")
						{
							if (deleteFrom != "")
							{
								result = " `postdatetime`<'" + DateTime.Parse(deleteFrom).ToString("yyyy-MM-dd HH:mm:ss") + "'";
							}
						}
					}
					else
					{
						if (deleteNum != "" && Utils.IsNumeric(deleteNum))
						{
							result = string.Concat(new string[]
							{
								" `id` not in (select `id` from `", 
								BaseConfigs.get_GetTablePrefix(), 
								"moderatormanagelog` order by `id` desc LIMIT 0,", 
								deleteNum, 
								")"
							});
						}
					}
				}
				else
				{
					if (id != "")
					{
						result = " `id` IN(" + id + ")";
					}
				}
			}
			return result;
		}
		public DataTable GroupNameTable(string groupid)
		{
			return DbHelper.ExecuteDataset(string.Concat(new string[]
			{
				"SELECT `grouptitle` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` WHERE `groupid`=", 
				groupid, 
				" LIMIT 1"
			})).Tables[0];
		}
		public string PaymentLogCondition(string deletemode, string id, string deleteNum, string deleteFrom)
		{
			string result = "";
			if (deletemode != null)
			{
				if (!(deletemode == "chkall"))
				{
					if (!(deletemode == "deleteNum"))
					{
						if (deletemode == "deleteFrom")
						{
							if (deleteFrom != "")
							{
								result = " `buydate`<'" + DateTime.Parse(deleteFrom).ToString("yyyy-MM-dd HH:mm:ss") + "'";
							}
						}
					}
					else
					{
						if (deleteNum != "" && Utils.IsNumeric(deleteNum))
						{
							result = string.Concat(new string[]
							{
								" `id` not in (select `id` from `", 
								BaseConfigs.get_GetTablePrefix(), 
								"paymentlog` order by `id` desc LIMIT 0,", 
								deleteNum, 
								")"
							});
						}
					}
				}
				else
				{
					if (id != "")
					{
						result = " `id` IN(" + id + ")";
					}
				}
			}
			return result;
		}
		public string PostGridBind(string posttablename, string condition)
		{
			return "Select * From `" + posttablename + "` WHERE " + condition.ToString();
		}
		public string DelRateScoreLogCondition(string deletemode, string id, string deleteNum, string deleteFrom)
		{
			string result = "";
			if (deletemode != null)
			{
				if (!(deletemode == "chkall"))
				{
					if (!(deletemode == "deleteNum"))
					{
						if (deletemode == "deleteFrom")
						{
							if (deleteFrom != "")
							{
								result = " `postdatetime`<'" + DateTime.Parse(deleteFrom).ToString("yyyy-MM-dd HH:mm:ss") + "'";
							}
						}
					}
					else
					{
						if (deleteNum != "" && Utils.IsNumeric(deleteNum))
						{
							result = string.Concat(new string[]
							{
								" `id` not in (select `id` from `", 
								BaseConfigs.get_GetTablePrefix(), 
								"ratelog` order by `id` desc LIMIT 0,", 
								deleteNum, 
								")"
							});
						}
					}
				}
				else
				{
					if (id != "")
					{
						result = " `id` IN(" + id + ")";
					}
				}
			}
			return result;
		}
		public void UpdatePostSP()
		{
		}
		public void CreateStoreProc(int tablelistmaxid)
		{
		}
		public void UpdateMyTopic()
		{
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "mytopics`";
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
			text = string.Concat(new string[]
			{
				"INSERT INTO `", 
				BaseConfigs.get_GetTablePrefix(), 
				"mytopics`(`uid`, `tid`, `dateline`) SELECT `posterid`,`tid`,`postdatetime` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `posterid`<>-1"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void UpdateMyPost()
		{
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "myposts`";
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
			IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, "SELECT `id` from `" + BaseConfigs.get_GetTablePrefix() + "tablelist`");
			while (dataReader.Read())
			{
				string text2 = string.Concat(new string[]
				{
					"SELECT `posterid`,`tid`,`pid`,`postdatetime` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					dataReader["id"].ToString().Trim(), 
					"` WHERE `posterid`<>-1"
				});
				IDataReader dataReader2 = DbHelper.ExecuteReader(CommandType.Text, text2);
				while (dataReader2.Read())
				{
					DbHelper.ExecuteNonQuery(string.Concat(new string[]
					{
						"INSERT INTO `", 
						BaseConfigs.get_GetTablePrefix(), 
						"myposts`(`uid`, `tid`, `pid`, `dateline`) values(", 
						dataReader2["posterid"].ToString(), 
						",", 
						dataReader2["tid"].ToString(), 
						",", 
						dataReader2["pid"].ToString(), 
						",'", 
						dataReader2["postdatetime"].ToString(), 
						"')"
					}));
				}
				dataReader2.Close();
			}
			dataReader.Close();
		}
		public string GetAllIdentifySql()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topicidentify`";
		}
		public DataTable GetAllIdentify()
		{
			string allIdentifySql = this.GetAllIdentifySql();
			return DbHelper.ExecuteDataset(CommandType.Text, allIdentifySql).Tables[0];
		}
		public bool UpdateIdentifyById(int id, string name)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?name", (DbType)253, 50, name), 
				DbHelper.MakeInParam("?identifyid", DbType.Boolean, 4, id)
			};
			string text = "SELECT COUNT(1) FROM `" + BaseConfigs.get_GetTablePrefix() + "topicidentify` WHERE `name`=?name AND `identifyid`<>?identifyid";
			bool result;
			if (int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString()) != 0)
			{
				result = false;
			}
			else
			{
				text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topicidentify` SET `name`=?name WHERE `identifyid`=?identifyid";
				DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
				result = true;
			}
			return result;
		}
		public bool AddIdentify(string name, string filename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?name", (DbType)253, 50, name), 
				DbHelper.MakeInParam("?filename", (DbType)253, 50, filename)
			};
			string text = "SELECT COUNT(1) FROM `" + BaseConfigs.get_GetTablePrefix() + "topicidentify` WHERE `name`=?name";
			bool result;
			if (int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString()) != 0)
			{
				result = false;
			}
			else
			{
				text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "topicidentify` (`name`,`filename`) VALUES(?name,?filename)";
				DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
				result = true;
			}
			return result;
		}
		public void DeleteIdentify(string idlist)
		{
			string text = string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topicidentify` WHERE `identifyid` IN (", 
				idlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public int GetSpecifyForumTemplateCount()
		{
			string text = string.Concat(new object[]
			{
				"SELECT COUNT(*) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `templateid` <> 0 AND `templateid` <> ", 
				GeneralConfigs.GetDefaultTemplateID()
			});
			return int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text).ToString());
		}
		public IDataReader GetAttachmentByUid(int uid, string extlist, int pageIndex, int pageSize)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?extlist ", (DbType)253, 100, extlist), 
				DbHelper.MakeInParam("?pageindex", DbType.Boolean, 4, pageIndex), 
				DbHelper.MakeInParam("?pagesize", DbType.Boolean, 4, pageSize)
			};
			string text = string.Concat(new object[]
			{
				"select *  from `", 
				BaseConfigs.get_GetTablePrefix(), 
				"myattachments` where  `extname` in (", 
				extlist, 
				") and `uid`=", 
				uid, 
				" order by `aid` desc limit ", 
				(pageIndex - 1) * pageSize, 
				",", 
				pageSize
			});
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public int GetUserAttachmentCount(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = string.Format("SELECT COUNT(1) FROM `{0}` WHERE `UID`=?uid", BaseConfigs.get_GetTablePrefix() + "myattachments");
			return int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString());
		}
		public int GetUserAttachmentCount(int uid, string extlist)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = string.Format("select count(1) from {0} where `extname` IN (" + extlist + ") and `UID`=?uid", BaseConfigs.get_GetTablePrefix() + "myattachments", BaseConfigs.get_GetTablePrefix());
			return int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString());
		}
		public IDataReader GetAttachmentByUid(int uid, int pageIndex, int pageSize)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?pageindex", DbType.Boolean, 4, pageIndex), 
				DbHelper.MakeInParam("?pagesize", DbType.Boolean, 4, pageSize)
			};
			string text = string.Concat(new object[]
			{
				"select *  from `", 
				BaseConfigs.get_GetTablePrefix(), 
				"myattachments`  where  `uid`=?uid  order by `aid` desc limit ", 
				(pageIndex - 1) * pageSize, 
				",", 
				pageSize
			});
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public void DelMyAttachmentByTid(string tidlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}myattachments` WHERE `tid` IN ({1})", BaseConfigs.get_GetTablePrefix(), tidlist));
		}
		public void DelMyAttachmentByPid(string pidlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}myattachments` WHERE `pid` IN ({1})", BaseConfigs.get_GetTablePrefix(), pidlist));
		}
		public void DelMyAttachmentByAid(string aidlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}myattachments` WHERE `aid` IN ({1})", BaseConfigs.get_GetTablePrefix(), aidlist));
		}
		public IDataReader GetHotTagsListForForum(int count)
		{
			string text = string.Format("SELECT * FROM `{0}tags` WHERE `fcount` > 0 AND `orderid` > -1 ORDER BY `orderid`, `fcount` DESC LIMIT {1}", BaseConfigs.get_GetTablePrefix(), count);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public string GetForumTagsSql(string tagname, int type)
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "tags`";
			if (tagname != "")
			{
				text = text + " WHERE `tagname` LIKE '%" + DataProvider.RegEsc(tagname) + "%'";
			}
			if (type == 1)
			{
				if (tagname != "")
				{
					text += " AND `orderid` < 0 ";
				}
				else
				{
					text += " WHERE `orderid` < 0 ";
				}
			}
			if (type == 2)
			{
				if (tagname != "")
				{
					text += " AND `orderid` >= 0";
				}
				else
				{
					text += " WHERE `orderid` >= 0 ";
				}
			}
			text += " ORDER BY `fcount` DESC";
			return text;
		}
		public string GetTopicNumber(string tagname, int from, int end, int type)
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "tags`";
			if (tagname != "")
			{
				text = text + " WHERE `tagname` LIKE '%" + DataProvider.RegEsc(tagname) + "%'";
			}
			if (type == 1)
			{
				if (tagname != "")
				{
					text += " AND `orderid` < 0 ";
					object obj = text;
					text = string.Concat(new object[]
					{
						obj, 
						" AND `fcount` between ", 
						from, 
						" AND ", 
						end
					});
				}
				else
				{
					text += " WHERE `orderid` < 0 ";
					object obj = text;
					text = string.Concat(new object[]
					{
						obj, 
						" AND `fcount` between ", 
						from, 
						" AND ", 
						end
					});
				}
			}
			if (type == 2)
			{
				if (tagname != "")
				{
					text += " AND `orderid` >= 0 ";
					object obj = text;
					text = string.Concat(new object[]
					{
						obj, 
						" AND `fcount` between ", 
						from, 
						" AND ", 
						end
					});
				}
				else
				{
					text += " WHERE `orderid` >= 0 ";
					object obj = text;
					text = string.Concat(new object[]
					{
						obj, 
						" AND `fcount` between ", 
						from, 
						" AND ", 
						end
					});
				}
			}
			text += " ORDER BY `fcount` DESC";
			return text;
		}
		public void UpdateForumTags(int tagid, int orderid, string color)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?orderid", DbType.Boolean, 4, orderid), 
				DbHelper.MakeInParam("?color", (DbType)253, 6, color), 
				DbHelper.MakeInParam("?tagid", DbType.Boolean, 4, tagid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "tags` SET `orderid`=?orderid,`color`=?color WHERE `tagid`=?tagid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetOpenForumList()
		{
			string text = string.Concat(new string[]
			{
				"SELECT `parentid`,`fid`,`name` FROM (SELECT f.*,ff.`permuserlist`,ff.`viewperm` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` f LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` ff ON f.`fid`=ff.`fid`) f WHERE `status`=1 AND (`permuserlist` IS NULL OR `permuserlist` LIKE '') AND (`viewperm` IS NULL OR `viewperm` LIKE '' OR instr(','+`viewperm`+',',',7,')<>0) ORDER BY `displayorder` ASC"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public string GetOpenForumListSql()
		{
			return string.Concat(new string[]
			{
				"SELECT `fid`,`name`,`parentid` FROM (SELECT f.*,ff.`permuserlist`,ff.`viewperm` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` f LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumfields` ff ON f.`fid`=ff.`fid`) f WHERE `status`=1 AND (`permuserlist` IS NULL OR `permuserlist` LIKE '') AND (`viewperm` IS NULL OR `viewperm` LIKE '' OR instr(','+`viewperm`+',',',7,')<>0) ORDER BY `displayorder` ASC"
			});
		}
		public DataTable GetExtractTopic(string inForumList, int itemCount, string givenTids, string keyword, string tags, string typesList, string digestList, string displayorderList, string orderBy)
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `closed`=0";
			if (inForumList != "")
			{
				text = text + " AND `fid` IN (" + inForumList + ")";
			}
			if (givenTids != "")
			{
				text = text + " AND `tid` IN (" + givenTids + ")";
			}
			if (keyword != "")
			{
				keyword = keyword.ToLower().Replace(" and ", " ");
				keyword = keyword.Replace(" or ", "|").Replace(" | ", "|");
				keyword = keyword.Replace("*", "");
				keyword = keyword.Replace(" ", "%' AND `title` LIKE '%");
				keyword = keyword.Replace("|", "%' OR `title` LIKE '%");
				keyword = " AND (`title` LIKE '%" + keyword + "%')";
				text += keyword;
			}
			string text2;
			if (tags != "")
			{
				tags = "'" + tags.Replace(" ", "','") + "'";
				text = text + " AND `tid` IN (SELECT `tid` FROM `" + BaseConfigs.get_GetTablePrefix() + "topictags`";
				text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" WHERE `tagid` IN (SELECT `tagid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"tags` WHERE `tagname` IN (", 
					tags, 
					")))"
				});
			}
			if (typesList != "")
			{
				text = text + " AND `typeid` IN (" + typesList + ")";
			}
			if (digestList != "")
			{
				text = text + " AND `digest` IN (" + digestList + ")";
			}
			if (displayorderList != "")
			{
				text = text + " AND `displayorder` IN (" + displayorderList + ")";
			}
			text2 = text;
			text = string.Concat(new string[]
			{
				text2, 
				" ORDER BY `", 
				orderBy, 
				"` DESC limit ", 
				itemCount.ToString()
			});
			text = string.Concat(new string[]
			{
				"SELECT f.`name` AS `forumname`,tt.`name` AS `topictype`,t.* FROM (", 
				text, 
				") t LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` f ON t.`fid`=f.`fid` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topictypes` tt ON t.`typeid`=tt.`typeid`"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public IDataReader GetHotTopics(int count)
		{
			string text = string.Format("SELECT `views`, `tid`, `title` FROM `{0}topics` WHERE `displayorder`>=0 ORDER BY `views` DESC LIMIT {1}", BaseConfigs.get_GetTablePrefix(), count);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetHotReplyTopics(int count)
		{
			string text = string.Format("SELECT `replies`, `tid`, `title` FROM `{0}topics` WHERE `displayorder`>=0 ORDER BY `replies` DESC LIMIT {1}", BaseConfigs.get_GetTablePrefix(), count);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetTopicBonusLogs(int tid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid);
			string text = string.Format("SELECT `tid`,`authorid`,`answerid`,`answername`,`extid`,SUM(bonus) AS `bonus` FROM `{0}bonuslog` WHERE `tid`=?tid GROUP BY `answerid`,`authorid`,`tid`,`answername`,`extid`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public IDataReader GetTopicBonusLogsByPost(int tid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid);
			string text = string.Format("SELECT `pid`,`isbest`,`bonus`,`extid` FROM `{0}bonuslog` WHERE `tid`=?tid", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public DataTable GetAllOpenForum()
		{
			string text = string.Format("SELECT * FROM `{0}forums` f LEFT JOIN `{0}forumfields` ff ON f.`fid` = ff.`fid` WHERE f.`autoclose`=0 AND ff.`password`='' AND ff.`redirect`=''", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void UpdateLastPost(ForumInfo foruminfo, PostInfo postinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lasttid", DbType.Boolean, 4, postinfo.get_Tid()), 
				DbHelper.MakeInParam("?lasttitle", (DbType)253, 60, postinfo.get_Topictitle()), 
				DbHelper.MakeInParam("?lastpost", DbType.Int64, 8, postinfo.get_Postdatetime()), 
				DbHelper.MakeInParam("?lastposterid", DbType.Boolean, 4, postinfo.get_Posterid()), 
				DbHelper.MakeInParam("?lastposter", (DbType)253, 20, postinfo.get_Poster()), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, foruminfo.get_Fid())
			};
			string text = string.Format("UPDATE `{0}forums` SET `lasttid` = ?lasttid, `lasttitle` = ?lasttitle, `lastpost` = ?lastpost, `lastposterid` = ?lastposterid, `lastposter` = ?lastposter WHERE `fid` = ?fid OR `fid` IN ({1})", BaseConfigs.get_GetTablePrefix(), foruminfo.get_Parentidlist());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetUserGroupMaxspacephotosize()
		{
			string text = "SELECT `groupid`,`grouptitle`,`maxspacephotosize` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups`  ORDER BY `groupid` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetUserGroupMaxspaceattachsize()
		{
			string text = "SELECT `groupid`,`grouptitle`,`maxspaceattachsize` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups`  ORDER BY `groupid` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetUsers(string idlist)
		{
			DataTable result;
			if (!Utils.IsNumericArray(idlist.Split(new char[]
			{
				','
			})))
			{
				result = new DataTable();
			}
			else
			{
				string text = string.Format("SELECT `uid`,`username` FROM `{0}users` WHERE `groupid` IN ({1})", BaseConfigs.get_GetTablePrefix(), idlist);
				result = DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
			}
			return result;
		}
		public DataTable GetUserGroupInfoByGroupid(int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "SELECT * From  `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`=?groupid LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public DataTable GetAdmingroupByAdmingid(int admingid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?admingid", DbType.Boolean, 4, admingid)
			};
			string text = "SELECT * From  `" + BaseConfigs.get_GetTablePrefix() + "admingroups` WHERE `admingid`=?admingid LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public DataTable GetMedal()
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "medals`";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public string GetMedalSql()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "medals`";
		}
		public DataTable GetExistMedalList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `medalid`,`image` FROM `" + BaseConfigs.get_GetTablePrefix() + "medals` WHERE `image`<>''").Tables[0];
		}
		public void AddMedal(int medalid, string name, int available, string image)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?medalid", DbType.Byte, 2, medalid), 
				DbHelper.MakeInParam("?name", (DbType)253, 50, name), 
				DbHelper.MakeInParam("?available", DbType.Boolean, 4, available), 
				DbHelper.MakeInParam("?image", (DbType)253, 30, image)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "medals` (`medalid`,`name`,`available`,`image`) Values (?medalid,?name,?available,?image)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateMedal(int medalid, string name, string image)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?name", (DbType)253, 50, name), 
				DbHelper.MakeInParam("?image", (DbType)253, 30, image), 
				DbHelper.MakeInParam("?medalid", DbType.Byte, 2, medalid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "medals` SET `name`=?name,`image`=?image  Where `medalid`=?medalid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void SetAvailableForMedal(int available, string medailidlist)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?available", DbType.Boolean, 4, available)
			};
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"medals` SET `available`=?available WHERE `medalid` IN(", 
				medailidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteMedalById(string medailidlist)
		{
			string text = string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"medals` WHERE `medalid` IN(", 
				medailidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public int GetMaxMedalId()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT IFNULL(MAX(`medalid`),0) FROM `" + BaseConfigs.get_GetTablePrefix() + "medals`"), 0) + 1;
		}
		public string GetGroupInfo()
		{
			return "SELECT `groupid`, `grouptitle` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` ORDER BY `groupid`";
		}
		public DataTable GetAdminGroupList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "admingroups`").Tables[0];
		}
		public int SetAdminGroupInfo(AdminGroupInfo __admingroupsInfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?alloweditpost", DbType.Boolean, 4, __admingroupsInfo.get_Alloweditpost()), 
				DbHelper.MakeInParam("?alloweditpoll", DbType.Boolean, 4, __admingroupsInfo.get_Alloweditpoll()), 
				DbHelper.MakeInParam("?allowstickthread", DbType.Boolean, 4, __admingroupsInfo.get_Allowstickthread()), 
				DbHelper.MakeInParam("?allowmodpost", DbType.Boolean, 4, __admingroupsInfo.get_Allowmodpost()), 
				DbHelper.MakeInParam("?allowdelpost", DbType.Boolean, 4, __admingroupsInfo.get_Allowdelpost()), 
				DbHelper.MakeInParam("?allowmassprune", DbType.Boolean, 4, __admingroupsInfo.get_Allowmassprune()), 
				DbHelper.MakeInParam("?allowrefund", DbType.Boolean, 4, __admingroupsInfo.get_Allowrefund()), 
				DbHelper.MakeInParam("?allowcensorword", DbType.Boolean, 4, __admingroupsInfo.get_Allowcensorword()), 
				DbHelper.MakeInParam("?allowviewip", DbType.Boolean, 4, __admingroupsInfo.get_Allowviewip()), 
				DbHelper.MakeInParam("?allowbanip", DbType.Boolean, 4, __admingroupsInfo.get_Allowbanip()), 
				DbHelper.MakeInParam("?allowedituser", DbType.Boolean, 4, __admingroupsInfo.get_Allowedituser()), 
				DbHelper.MakeInParam("?allowmoduser", DbType.Boolean, 4, __admingroupsInfo.get_Allowmoduser()), 
				DbHelper.MakeInParam("?allowbanuser", DbType.Boolean, 4, __admingroupsInfo.get_Allowbanuser()), 
				DbHelper.MakeInParam("?allowpostannounce", DbType.Boolean, 4, __admingroupsInfo.get_Allowpostannounce()), 
				DbHelper.MakeInParam("?allowviewlog", DbType.Boolean, 4, __admingroupsInfo.get_Allowviewlog()), 
				DbHelper.MakeInParam("?disablepostctrl", DbType.Boolean, 4, __admingroupsInfo.get_Disablepostctrl()), 
				DbHelper.MakeInParam("?allowviewrealname", DbType.Boolean, 4, __admingroupsInfo.get_Allowviewrealname()), 
				DbHelper.MakeInParam("?admingid", DbType.Byte, 2, __admingroupsInfo.get_Admingid())
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "admingroups` SET `alloweditpost`=?alloweditpost,`alloweditpoll`=?alloweditpoll,`allowstickthread`=?allowstickthread,`allowmodpost`=?allowmodpost,`allowdelpost`=?allowdelpost,`allowmassprune`=?allowmassprune,`allowrefund`=?allowrefund,`allowcensorword`=?allowcensorword,`allowviewip`=?allowviewip,`allowbanip`=?allowbanip,`allowedituser`=?allowedituser,`allowmoduser`=?allowmoduser,`allowbanuser`=?allowbanuser,`allowpostannounce`=?allowpostannounce,`allowviewlog`=?allowviewlog,`disablepostctrl`=?disablepostctrl, `allowviewrealname`=?allowviewrealname WHERE `admingid`=?admingid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int CreateAdminGroupInfo(AdminGroupInfo __admingroupsInfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?admingid", DbType.Byte, 2, __admingroupsInfo.get_Admingid()), 
				DbHelper.MakeInParam("?alloweditpost", DbType.Boolean, 4, __admingroupsInfo.get_Alloweditpost()), 
				DbHelper.MakeInParam("?alloweditpoll", DbType.Boolean, 4, __admingroupsInfo.get_Alloweditpoll()), 
				DbHelper.MakeInParam("?allowstickthread", DbType.Boolean, 4, __admingroupsInfo.get_Allowstickthread()), 
				DbHelper.MakeInParam("?allowmodpost", DbType.Boolean, 4, __admingroupsInfo.get_Allowmodpost()), 
				DbHelper.MakeInParam("?allowdelpost", DbType.Boolean, 4, __admingroupsInfo.get_Allowdelpost()), 
				DbHelper.MakeInParam("?allowmassprune", DbType.Boolean, 4, __admingroupsInfo.get_Allowmassprune()), 
				DbHelper.MakeInParam("?allowrefund", DbType.Boolean, 4, __admingroupsInfo.get_Allowrefund()), 
				DbHelper.MakeInParam("?allowcensorword", DbType.Boolean, 4, __admingroupsInfo.get_Allowcensorword()), 
				DbHelper.MakeInParam("?allowviewip", DbType.Boolean, 4, __admingroupsInfo.get_Allowviewip()), 
				DbHelper.MakeInParam("?allowbanip", DbType.Boolean, 4, __admingroupsInfo.get_Allowbanip()), 
				DbHelper.MakeInParam("?allowedituser", DbType.Boolean, 4, __admingroupsInfo.get_Allowedituser()), 
				DbHelper.MakeInParam("?allowmoduser", DbType.Boolean, 4, __admingroupsInfo.get_Allowmoduser()), 
				DbHelper.MakeInParam("?allowbanuser", DbType.Boolean, 4, __admingroupsInfo.get_Allowbanuser()), 
				DbHelper.MakeInParam("?allowpostannounce", DbType.Boolean, 4, __admingroupsInfo.get_Allowpostannounce()), 
				DbHelper.MakeInParam("?allowviewlog", DbType.Boolean, 4, __admingroupsInfo.get_Allowviewlog()), 
				DbHelper.MakeInParam("?disablepostctrl", DbType.Boolean, 4, __admingroupsInfo.get_Disablepostctrl())
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "admingroups`(`admingid`,`alloweditpost`,`alloweditpoll`,`allowstickthread`,`allowmodpost`,`allowdelpost`,`allowmassprune`,`allowrefund`,`allowcensorword`,`allowviewip`,`allowbanip`,`allowedituser`,`allowmoduser`,`allowbanuser`,`allowpostannounce`,`allowviewlog`,`disablepostctrl`) VALUES (?admingid,?alloweditpost,?alloweditpoll,?allowstickthread,?allowmodpost,?allowdelpost,?allowmassprune,?allowrefund,?allowcensorword,?allowviewip,?allowbanip,?allowedituser,?allowmoduser,?allowbanuser,?allowpostannounce,?allowviewlog,?disablepostctrl)";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int DeleteAdminGroupInfo(short admingid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?admingid", DbType.Byte, 2, admingid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "admingroups` WHERE `admingid` = ?admingid", array);
		}
		public string GetAdminGroupInfoSql()
		{
			return "Select * From `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `radminid`>0 AND `radminid`<=3  Order By `groupid`";
		}
		public DataTable GetRaterangeByGroupid(int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "SELECT `raterange` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`=?groupid LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void UpdateRaterangeByGroupid(string raterange, int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?raterange", (DbType)253, 500, raterange), 
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups` SET `raterange`=?raterange  WHERE `groupid`=?groupid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string GetAudituserSql()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "users` Where `groupid`=8";
		}
		public DataSet GetAudituserUid()
		{
			string text = "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `groupid`=8";
			return DbHelper.ExecuteDataset(CommandType.Text, text);
		}
		public void ClearAuthstrByUidlist(string uidlist)
		{
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` SET `authstr`=''  WHERE `uid` IN (", 
				uidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void ClearAllUserAuthstr()
		{
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` SET `authstr`=''  WHERE `uid` IN (SELECT `uid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `groupid`=8 )"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void DeleteUserByUidlist(string uidlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` WHERE `uid` IN(", 
				uidlist, 
				")"
			}));
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `uid` IN(", 
				uidlist, 
				")"
			}));
		}
		public void DeleteAuditUser()
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` WHERE `uid` IN (SELECT `uid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `groupid`=8 )"
			}));
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `groupid`=8 ");
		}
		public DataTable GetAuditUserEmail()
		{
			string text = "SELECT `username`,`password`,`email` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `groupid`=8";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetUserEmailByUidlist(string uidlist)
		{
			string text = string.Concat(new string[]
			{
				"SELECT `username`,`password`,`email` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `uid` IN(", 
				uidlist, 
				")"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public string GetUserGroup()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `radminid`= 0 And `groupid`>8 ORDER BY `groupid`";
		}
		public string GetUserGroupTitle()
		{
			return "SELECT `groupid`,`grouptitle` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `radminid`= 0 And `groupid`>8 ORDER BY `groupid`";
		}
		public DataTable GetUserGroupWithOutGuestTitle()
		{
			return DbHelper.ExecuteDataset("SELECT `groupid`,`grouptitle` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`<>7  ORDER BY `groupid` ASC").Tables[0];
		}
		public string GetAdminUserGroupTitle()
		{
			return "SELECT `groupid`,`grouptitle` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `radminid`> 0 AND `radminid`<=3  ORDER BY `groupid`";
		}
		public void CombinationUsergroupScore(int sourceusergroupid, int targetusergroupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?sourceusergroupid", DbType.Boolean, 4, sourceusergroupid), 
				DbHelper.MakeInParam("?targetusergroupid", DbType.Boolean, 4, targetusergroupid)
			};
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` SET `creditshigher`=(SELECT `creditshigher` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` WHERE `groupid`=?sourceusergroupid) WHERE `groupid`=?targetusergroupid"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteUserGroupInfo(int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` Where `groupid`=?groupid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteAdminGroupInfo(int admingid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?admingid", DbType.Boolean, 4, admingid)
			};
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "admingroups` Where `admingid`=?admingid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void ChangeUsergroup(int soureceusergroupid, int targetusergroupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?targetusergroupid", DbType.Boolean, 4, targetusergroupid), 
				DbHelper.MakeInParam("?soureceusergroupid", DbType.Boolean, 4, soureceusergroupid)
			};
			string text = "Update `" + BaseConfigs.get_GetTablePrefix() + "users` SET `groupid`=?targetusergroupid Where `groupid`=?soureceusergroupid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetAdmingid(int admingid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?admingid", DbType.Boolean, 4, admingid)
			};
			string text = "SELECT `admingid`  FROM `" + BaseConfigs.get_GetTablePrefix() + "admingroups` WHERE `admingid`=?admingid";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void ChangeUserAdminidByGroupid(int adminid, int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?adminid", DbType.Boolean, 4, adminid), 
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `adminid`=?adminid WHERE `groupid`=?groupid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetAvailableMedal()
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "medals` WHERE `available`=1";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public bool IsExistMedalAwardRecord(int medalid, int userid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?medalid", DbType.Boolean, 4, medalid), 
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			string text = "SELECT ID FROM `" + BaseConfigs.get_GetTablePrefix() + "medalslog` WHERE `medals`=?medalid AND `uid`=?userid LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0].Rows.Count != 0;
		}
		public void AddMedalslog(int adminid, string adminname, string ip, string username, int uid, string actions, int medals, string reason)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?adminid", DbType.Boolean, 4, adminid), 
				DbHelper.MakeInParam("?adminname", (DbType)253, 50, adminname), 
				DbHelper.MakeInParam("?ip", (DbType)253, 15, ip), 
				DbHelper.MakeInParam("?username", (DbType)253, 50, username), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?actions", (DbType)253, 100, actions), 
				DbHelper.MakeInParam("?medals", DbType.Boolean, 4, medals), 
				DbHelper.MakeInParam("?reason", (DbType)253, 100, reason)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "medalslog` (`adminid`,`adminname`,`ip`,`username`,`uid`,`actions`,`medals`,`reason`) VALUES (?adminid,?adminname,?ip,?username,?uid,?actions,?medals,?reason)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateMedalslog(string newactions, DateTime postdatetime, string reason, string oldactions, int medals, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?newactions", (DbType)253, 100, newactions), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int16, 8, postdatetime), 
				DbHelper.MakeInParam("?reason", (DbType)253, 100, reason), 
				DbHelper.MakeInParam("?oldactions", (DbType)254, 100, oldactions), 
				DbHelper.MakeInParam("?medals", DbType.Boolean, 4, medals), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "Update `" + BaseConfigs.get_GetTablePrefix() + "medalslog` SET `actions`=?newactions ,`postdatetime`=?postdatetime, reason=?reason  WHERE `actions`=?oldactions AND `medals`=?medals  AND `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateMedalslog(string actions, DateTime postdatetime, string reason, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?actions", (DbType)253, 100, actions), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, postdatetime), 
				DbHelper.MakeInParam("?reason", (DbType)253, 100, reason), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "Update `" + BaseConfigs.get_GetTablePrefix() + "medalslog` SET `actions`=?actions ,`postdatetime`=?postdatetime,reason=?reason  WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateMedalslog(string newactions, DateTime postdatetime, string reason, string oldactions, string medalidlist, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?newactions", (DbType)253, 100, newactions), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, postdatetime), 
				DbHelper.MakeInParam("?reason", (DbType)253, 100, reason), 
				DbHelper.MakeInParam("?oldactions", (DbType)253, 100, oldactions), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = string.Concat(new object[]
			{
				"Update `", 
				BaseConfigs.get_GetTablePrefix(), 
				"medalslog` SET `actions`='", 
				newactions, 
				"' ,`postdatetime`='", 
				postdatetime, 
				"', reason='", 
				reason, 
				"'  WHERE `actions`='", 
				oldactions, 
				"' AND `medals` NOT IN (", 
				medalidlist, 
				") AND `uid`=", 
				uid
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void SetStopTalkUser(string uidlist)
		{
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `groupid`=4, `adminid`=0  WHERE `uid` IN (", 
				uidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void ChangeUserGroupByUid(int groupid, string uidlist)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `groupid`=?groupid  WHERE `uid` IN (", 
				uidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetTableListInfo()
		{
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "tablelist`";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void DeletePostByPosterid(int tabid, int posterid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, posterid)
			};
			string text = string.Concat(new object[]
			{
				"DELETE FROM  `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				tabid, 
				"`   WHERE `posterid`=?posterid"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteTopicByPosterid(int posterid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, posterid)
			};
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `posterid`=?posterid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void ClearPosts(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `digestposts`=0 , `posts`=0  WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateEmailValidateInfo(string authstr, DateTime authtime, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?authstr", (DbType)253, 20, authstr), 
				DbHelper.MakeInParam("?authtime", DbType.Int64, 8, authtime), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "userfields` SET `Authstr`=?authstr,`Authtime`=?authtime ,`Authflag`=1  WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int GetRadminidByGroupid(int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "SELECT `radminid` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`=?groupid LIMIT 1";
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, text, array));
		}
		public string GetTemplateInfo()
		{
			return "SELECT `templateid`, `name` FROM `" + BaseConfigs.get_GetTablePrefix() + "templates`";
		}
		public DataTable GetUserEmailByGroupid(string groupidlist)
		{
			string text = string.Concat(new string[]
			{
				"SELECT `username`,`Email`  From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `Email` Is Not null AND `Email`<>'' AND `groupid` IN(", 
				groupidlist, 
				")"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetUserGroupExceptGroupid(int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "SELECT `groupid` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `radminid`=0 And `groupid`>8 AND `groupid`<>?groupid";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public int CreateFavorites(int uid, int tid)
		{
			return this.CreateFavorites(uid, tid, 0);
		}
		public int CreateFavorites(int uid, int tid, byte type)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?type", (DbType)502, 4, type)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "favorites` (`uid`,`tid`,`typeid`) VALUES(?uid,?tid,?type)";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int DeleteFavorites(int uid, string fidlist, byte type)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?typeid", DbType.Boolean, 1, type)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"favorites` WHERE `tid` IN (", 
				fidlist, 
				") AND `uid` = ?uid  AND `typeid`=?typeid"
			}), array);
		}
		public DataTable GetFavoritesList(int uid, int pagesize, int pageindex)
		{
			return this.GetFavoritesList(uid, pagesize, pageindex, 0);
		}
		public DataTable GetFavoritesList(int uid, int pagesize, int currentpage, int typeid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pagesize", DbType.Boolean, 4, pagesize), 
				DbHelper.MakeInParam("?pageindex", DbType.Boolean, 4, currentpage), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "";
			switch (typeid)
			{
				case 1:
				{
					object[] array2 = new object[10];
					array2[0] = "SELECT `tid`, `uid`, `albumid`, `albumcateid`, `posterid`, `poster`, `title`, `description`, `logo`, `password`, `imgcount`, `views`, `type`, `postdatetime`  FROM (SELECT `f`.`tid`, `f`.`uid`, `albumid`, `albumcateid`, `userid` AS `posterid`, `username` AS `poster`, `title`, `description`, `logo`, `password`, `imgcount`, `views`, `type`, `createdatetime` AS `postdatetime` FROM `";
					array2[1] = BaseConfigs.get_GetTablePrefix();
					array2[2] = "favorites` `f`,`";
					array2[3] = BaseConfigs.get_GetTablePrefix();
					array2[4] = "albums` `albums` WHERE `f`.`tid`=`albums`.`albumid` AND `f`.`typeid`=1 AND `f`.`uid`=";
					array2[5] = uid;
					array2[6] = ") f ORDER BY `tid` DESC LIMIT ";
					object[] arg_C8_0 = array2;
					int arg_C8_1 = 7;
					int num = (currentpage - 1) * pagesize;
					arg_C8_0[arg_C8_1] = num.ToString();
					array2[8] = ",";
					array2[9] = pagesize.ToString();
					text = string.Concat(array2);
					break;
				}
				case 2:
				{
					string text2 = string.Concat(new string[]
					{
						"SELECT `f`.`tid`, `f`.`uid`, `postid`, `author` AS `poster`, `spaceposts`.`uid` AS `posterid`, `postdatetime`, `title`, `category`, `poststatus`, `commentstatus`, `postupdatetime`, `commentcount`, `views` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"favorites` `f`,`", 
						BaseConfigs.get_GetTablePrefix(), 
						"spaceposts` `spaceposts` WHERE `f`.`tid`=`spaceposts`.`postid` AND `f`.`typeid`=2 AND `f`.`uid`=", 
						uid.ToString()
					});
					string[] array3 = new string[6];
					array3[0] = "SELECT `tid`, `postid`, `poster`, `posterid`, `uid`, `postdatetime`, `title`, `category`, `poststatus`, `commentstatus`, `postupdatetime`, `commentcount`, `views`  FROM (";
					array3[1] = text2;
					array3[2] = ") f ORDER BY `tid` DESC LIMIT ";
					string[] arg_163_0 = array3;
					int arg_163_1 = 3;
					int num = (currentpage - 1) * pagesize;
					arg_163_0[arg_163_1] = num.ToString();
					array3[4] = ",";
					array3[5] = pagesize.ToString();
					text = string.Concat(array3);
					break;
				}
				default:
				{
					string text3 = string.Concat(new object[]
					{
						"SELECT `f`.`uid`,`f`.`tid`,`topics`.`title`,`topics`.`poster`,`topics`.`postdatetime`,`topics`.`replies`,`topics`.`views`,`topics`.`posterid` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"favorites` `f`,`", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` `topics` WHERE `f`.`tid`=`topics`.`tid` AND `f`.`typeid`=0 AND `f`.`uid`=", 
						uid
					});
					string[] array3 = new string[6];
					array3[0] = "SELECT `uid`,`tid`,`title`,`poster`,`postdatetime`,`replies`,`views`,`posterid`  FROM (";
					array3[1] = text3;
					array3[2] = ") f ORDER BY `tid` DESC LIMIT ";
					string[] arg_1FC_0 = array3;
					int arg_1FC_1 = 3;
					int num = (currentpage - 1) * pagesize;
					arg_1FC_0[arg_1FC_1] = num.ToString();
					array3[4] = ",";
					array3[5] = pagesize.ToString();
					text = string.Concat(array3);
					break;
				}
			}
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetFavoritesCount(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "SELECT COUNT(`uid`) as `c` FROM `" + BaseConfigs.get_GetTablePrefix() + "favorites` WHERE `uid`=?uid";
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString(), 0);
		}
		public int GetFavoritesCount(int uid, int typeid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?typeid", DbType.Boolean, 1, typeid)
			};
			string text = "SELECT COUNT(uid) as c FROM `" + BaseConfigs.get_GetTablePrefix() + "favorites` WHERE `uid`=?uid AND `typeid`=?typeid";
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString(), 0);
		}
		public int CheckFavoritesIsIN(int uid, int tid)
		{
			return this.CreateFavorites(uid, tid, 0);
		}
		public int CheckFavoritesIsIN(int uid, int tid, byte type)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?type", DbType.Boolean, 4, type)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(`tid`) AS `tidcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "favorites` WHERE `tid`=?tid AND `uid`=?uid AND `typeid`=?type", array), 0);
		}
		public void UpdateUserAllInfo(UserInfo userinfo)
		{
			string text = "Update `" + BaseConfigs.get_GetTablePrefix() + "users` Set username=?username ,nickname=?nickname,secques=?secques,gender=?gender,adminid=?adminid,groupid=?groupid,groupexpiry=?groupexpiry,extgroupids=?extgroupids, regip=?regip,joindate=?joindate , lastip=?lastip, lastvisit=?lastvisit,  lastactivity=?lastactivity, lastpost=?lastpost, lastposttitle=?lastposttitle,posts=?posts, digestposts=?digestposts,oltime=?oltime,pageviews=?pageviews,credits=?credits,avatarshowid=?avatarshowid, email=?email,bday=?bday,sigstatus=?sigstatus,tpp=?tpp,ppp=?ppp,templateid=?templateid,pmsound=?pmsound,showemail=?showemail,newsletter=?newsletter,invisible=?invisible,newpm=?newpm,accessmasks=?accessmasks,extcredits1=?extcredits1,extcredits2=?extcredits2,extcredits3=?extcredits3,extcredits4=?extcredits4,extcredits5=?extcredits5,extcredits6=?extcredits6,extcredits7=?extcredits7,extcredits8=?extcredits8   Where uid=?uid";
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)253, 20, userinfo.get_Username()), 
				DbHelper.MakeInParam("?nickname", (DbType)253, 10, userinfo.get_Nickname()), 
				DbHelper.MakeInParam("?secques", (DbType)253, 8, userinfo.get_Secques()), 
				DbHelper.MakeInParam("?gender", DbType.Boolean, 4, userinfo.get_Gender()), 
				DbHelper.MakeInParam("?adminid", DbType.Boolean, 4, (userinfo.get_Uid() == 1) ? 1 : userinfo.get_Adminid()), 
				DbHelper.MakeInParam("?groupid", DbType.Byte, 2, userinfo.get_Groupid()), 
				DbHelper.MakeInParam("?groupexpiry", DbType.Boolean, 4, userinfo.get_Groupexpiry()), 
				DbHelper.MakeInParam("?extgroupids", (DbType)253, 60, userinfo.get_Extgroupids()), 
				DbHelper.MakeInParam("?regip", (DbType)253, 15, userinfo.get_Regip()), 
				DbHelper.MakeInParam("?joindate", DbType.Int64, 4, userinfo.get_Joindate()), 
				DbHelper.MakeInParam("?lastip", (DbType)253, 15, userinfo.get_Lastip()), 
				DbHelper.MakeInParam("?lastvisit", DbType.Int64, 8, userinfo.get_Lastvisit()), 
				DbHelper.MakeInParam("?lastactivity", DbType.Int64, 8, userinfo.get_Lastactivity()), 
				DbHelper.MakeInParam("?lastpost", DbType.Int64, 8, userinfo.get_Lastpost()), 
				DbHelper.MakeInParam("?lastposttitle", (DbType)253, 80, userinfo.get_Lastposttitle()), 
				DbHelper.MakeInParam("?posts", DbType.Boolean, 4, userinfo.get_Posts()), 
				DbHelper.MakeInParam("?digestposts", DbType.Byte, 2, userinfo.get_Digestposts()), 
				DbHelper.MakeInParam("?oltime", DbType.Boolean, 4, userinfo.get_Oltime()), 
				DbHelper.MakeInParam("?pageviews", DbType.Boolean, 4, userinfo.get_Pageviews()), 
				DbHelper.MakeInParam("?credits", DbType.AnsiString, 10, userinfo.get_Credits()), 
				DbHelper.MakeInParam("?avatarshowid", DbType.Boolean, 4, userinfo.get_Avatarshowid()), 
				DbHelper.MakeInParam("?email", (DbType)253, 50, userinfo.get_Email().ToString()), 
				DbHelper.MakeInParam("?bday", (DbType)253, 10, userinfo.get_Bday().ToString()), 
				DbHelper.MakeInParam("?sigstatus", DbType.Boolean, 4, userinfo.get_Sigstatus().ToString()), 
				DbHelper.MakeInParam("?tpp", DbType.Boolean, 4, userinfo.get_Tpp()), 
				DbHelper.MakeInParam("?ppp", DbType.Boolean, 4, userinfo.get_Ppp()), 
				DbHelper.MakeInParam("?templateid", DbType.Boolean, 4, userinfo.get_Templateid()), 
				DbHelper.MakeInParam("?pmsound", DbType.Boolean, 4, userinfo.get_Pmsound()), 
				DbHelper.MakeInParam("?showemail", DbType.Boolean, 4, userinfo.get_Showemail()), 
				DbHelper.MakeInParam("?newsletter", DbType.Boolean, 4, userinfo.get_Newsletter()), 
				DbHelper.MakeInParam("?invisible", DbType.Boolean, 4, userinfo.get_Invisible()), 
				DbHelper.MakeInParam("?newpm", DbType.Boolean, 4, userinfo.get_Newpm()), 
				DbHelper.MakeInParam("?accessmasks", DbType.Boolean, 4, userinfo.get_Accessmasks()), 
				DbHelper.MakeInParam("?extcredits1", DbType.AnsiString, 10, userinfo.get_Extcredits1()), 
				DbHelper.MakeInParam("?extcredits2", DbType.AnsiString, 10, userinfo.get_Extcredits2()), 
				DbHelper.MakeInParam("?extcredits3", DbType.AnsiString, 10, userinfo.get_Extcredits3()), 
				DbHelper.MakeInParam("?extcredits4", DbType.AnsiString, 10, userinfo.get_Extcredits4()), 
				DbHelper.MakeInParam("?extcredits5", DbType.AnsiString, 10, userinfo.get_Extcredits5()), 
				DbHelper.MakeInParam("?extcredits6", DbType.AnsiString, 10, userinfo.get_Extcredits6()), 
				DbHelper.MakeInParam("?extcredits7", DbType.AnsiString, 10, userinfo.get_Extcredits7()), 
				DbHelper.MakeInParam("?extcredits8", DbType.AnsiString, 10, userinfo.get_Extcredits8()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userinfo.get_Uid())
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteModerator(int uid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"moderators` WHERE `uid`=", 
				uid
			}));
		}
		public void UpdateUserField(UserInfo __userinfo, string signature, string authstr, string sightml)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?website", (DbType)253, 80, __userinfo.get_Website()), 
				DbHelper.MakeInParam("?icq", (DbType)253, 12, __userinfo.get_Icq()), 
				DbHelper.MakeInParam("?qq", (DbType)253, 12, __userinfo.get_Qq()), 
				DbHelper.MakeInParam("?yahoo", (DbType)253, 40, __userinfo.get_Yahoo()), 
				DbHelper.MakeInParam("?msn", (DbType)253, 40, __userinfo.get_Msn()), 
				DbHelper.MakeInParam("?skype", (DbType)253, 40, __userinfo.get_Skype()), 
				DbHelper.MakeInParam("?location", (DbType)253, 50, __userinfo.get_Location()), 
				DbHelper.MakeInParam("?customstatus", (DbType)253, 50, __userinfo.get_Customstatus()), 
				DbHelper.MakeInParam("?avatar", (DbType)253, 255, __userinfo.get_Avatar()), 
				DbHelper.MakeInParam("?avatarwidth", DbType.Boolean, 4, __userinfo.get_Avatarwidth()), 
				DbHelper.MakeInParam("?avatarheight", DbType.Boolean, 4, __userinfo.get_Avatarheight()), 
				DbHelper.MakeInParam("?medals", (DbType)253, 300, __userinfo.get_Medals()), 
				DbHelper.MakeInParam("?authstr", (DbType)253, 20, authstr), 
				DbHelper.MakeInParam("?authtime", DbType.Int64, 4, __userinfo.get_Authtime()), 
				DbHelper.MakeInParam("?authflag", DbType.Byte, 1, 1), 
				DbHelper.MakeInParam("?bio", (DbType)253, 500, __userinfo.get_Bio().ToString()), 
				DbHelper.MakeInParam("?signature", (DbType)253, 500, signature), 
				DbHelper.MakeInParam("?sightml", (DbType)253, 1000, sightml), 
				DbHelper.MakeInParam("?Realname", (DbType)253, 1000, __userinfo.get_Realname()), 
				DbHelper.MakeInParam("?Idcard", (DbType)253, 1000, __userinfo.get_Idcard()), 
				DbHelper.MakeInParam("?Mobile", (DbType)253, 1000, __userinfo.get_Mobile()), 
				DbHelper.MakeInParam("?Phone", (DbType)253, 1000, __userinfo.get_Phone()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, __userinfo.get_Uid())
			};
			string text = "Update `" + BaseConfigs.get_GetTablePrefix() + "userfields` Set website=?website,icq=?icq,qq=?qq,yahoo=?yahoo,msn=?msn,skype=?skype,location=?location,customstatus=?customstatus, avatar=?avatar,avatarwidth=?avatarwidth , avatarheight=?avatarheight, medals=?medals,  authstr=?authstr, authtime=?authtime, authflag=?authflag,bio=?bio, signature=?signature,sightml=?sightml,realname=?Realname,idcard=?Idcard,mobile=?Mobile,phone=?Phone   Where uid=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdatePMSender(int msgfromid, string msgfrom)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?msgfrom", (DbType)253, 20, msgfrom), 
				DbHelper.MakeInParam("?msgfromid", DbType.Boolean, 4, msgfromid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "pms` SET `msgfrom`=?msgfrom WHERE `msgfromid`=?msgfromid", array);
		}
		public void UpdatePMReceiver(int msgtoid, string msgto)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?msgto", (DbType)253, 20, msgto), 
				DbHelper.MakeInParam("?msgtoid", DbType.Boolean, 4, msgtoid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "pms` SET `msgto`=?msgto  WHERE `msgtoid`=?msgtoid", array);
		}
		public DataRowCollection GetModerators(string oldusername)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?oldusername", (DbType)253, 20, DataProvider.RegEsc(oldusername))
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `fid`,`moderators` FROM  `" + BaseConfigs.get_GetTablePrefix() + "forumfields` WHERE `moderators` LIKE '% ?oldusername %'", array).Tables[0].Rows;
		}
		public DataTable GetModeratorsTable(string oldusername)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?oldusername", (DbType)253, 20, DataProvider.RegEsc(oldusername))
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `fid`,`moderators` FROM  `" + BaseConfigs.get_GetTablePrefix() + "forumfields` WHERE `moderators` LIKE '% ?oldusername %'", array).Tables[0];
		}
		public void UpdateModerators(int fid, string moderators)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?moderators", (DbType)253, 20, moderators), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forumfields` SET `moderators`=?moderators  WHERE `fid`=?fid", array);
		}
		public void UpdateUserCredits(int userid, float credits, float extcredits1, float extcredits2, float extcredits3, float extcredits4, float extcredits5, float extcredits6, float extcredits7, float extcredits8)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?Credits", DbType.AnsiString, 9, credits), 
				DbHelper.MakeInParam("?Extcredits1", DbType.AnsiString, 20, extcredits1), 
				DbHelper.MakeInParam("?Extcredits2", DbType.AnsiString, 20, extcredits2), 
				DbHelper.MakeInParam("?Extcredits3", DbType.AnsiString, 20, extcredits3), 
				DbHelper.MakeInParam("?Extcredits4", DbType.AnsiString, 20, extcredits4), 
				DbHelper.MakeInParam("?Extcredits5", DbType.AnsiString, 20, extcredits5), 
				DbHelper.MakeInParam("?Extcredits6", DbType.AnsiString, 20, extcredits6), 
				DbHelper.MakeInParam("?Extcredits7", DbType.AnsiString, 20, extcredits7), 
				DbHelper.MakeInParam("?Extcredits8", DbType.AnsiString, 20, extcredits8), 
				DbHelper.MakeInParam("?targetuid", DbType.Boolean, 4, userid.ToString())
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET credits=?Credits,extcredits1=?Extcredits1, extcredits2=?Extcredits2, extcredits3=?Extcredits3, extcredits4=?Extcredits4, extcredits5=?Extcredits5, extcredits6=?Extcredits6, extcredits7=?Extcredits7, extcredits8=?Extcredits8 WHERE `uid`=?targetuid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateUserCredits(int userid, int extcreditsid, float score)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?targetuid", DbType.Boolean, 4, userid.ToString()), 
				DbHelper.MakeInParam("?Extcredits", DbType.Currency, 8, score)
			};
			string text = string.Format("UPDATE `{0}users` SET extcredits{1}=extcredits{1} + ?Extcredits WHERE `uid`=?targetuid", BaseConfigs.get_GetTablePrefix(), extcreditsid);
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void CombinationUser(string posttablename, UserInfo targetuserinfo, UserInfo srcuserinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?target_uid", DbType.Boolean, 4, targetuserinfo.get_Uid()), 
				DbHelper.MakeInParam("?target_username", (DbType)253, 20, targetuserinfo.get_Username().Trim()), 
				DbHelper.MakeInParam("?src_uid", DbType.Boolean, 4, srcuserinfo.get_Uid())
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE  `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `posterid`=?target_uid,`poster`=?target_username  WHERE `posterid`=?src_uid", array);
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `posts`=", 
				srcuserinfo.get_Posts() + targetuserinfo.get_Posts(), 
				"WHERE `uid`=?target_uid"
			}), array);
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE  `" + posttablename + "` SET `posterid`=?target_uid,`poster`=?target_username  WHERE `posterid`=?src_uid", array);
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE  `" + BaseConfigs.get_GetTablePrefix() + "pms` SET `msgtoid`=?target_uid,`msgto`=?target_username  WHERE `msgtoid`=?src_uid", array);
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE  `" + BaseConfigs.get_GetTablePrefix() + "attachments` SET `uid`=?target_uid WHERE `uid`=?src_uid", array);
		}
		public int GetuidByusername(string username)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)253, 20, username)
			};
			DataTable dataTable = DbHelper.ExecuteDataset(CommandType.Text, "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `username`=?username LIMIT 1", array).Tables[0];
			int result;
			if (dataTable.Rows.Count > 0)
			{
				result = Convert.ToInt32(dataTable.Rows[0][0].ToString());
			}
			else
			{
				result = 0;
			}
			return result;
		}
		public bool DelUserAllInf(int uid, bool delposts, bool delpms)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, "Delete From `" + BaseConfigs.get_GetTablePrefix() + "users` Where `uid`=" + uid.ToString());
			DbHelper.ExecuteNonQuery(CommandType.Text, "Delete From `" + BaseConfigs.get_GetTablePrefix() + "userfields` Where `uid`=" + uid.ToString());
			DbHelper.ExecuteNonQuery(CommandType.Text, "Delete From `" + BaseConfigs.get_GetTablePrefix() + "polls` Where `userid`=" + uid.ToString());
			DbHelper.ExecuteNonQuery(CommandType.Text, "Delete From `" + BaseConfigs.get_GetTablePrefix() + "favorites` Where `uid`=" + uid.ToString());
			if (delposts)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "Delete From `" + BaseConfigs.get_GetTablePrefix() + "topics` Where `posterid`=" + uid.ToString());
				foreach (DataRow dataRow in DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "tablelist`").Tables[0].Rows)
				{
					if (dataRow["id"].ToString() != "")
					{
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
						{
							"DELETE FROM  `", 
							BaseConfigs.get_GetTablePrefix(), 
							"posts", 
							dataRow["id"].ToString(), 
							"`   WHERE `posterid`=", 
							uid
						}));
					}
				}
			}
			else
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `poster`='该用户已被删除'  Where `posterid`=" + uid.ToString());
				DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `lastposter`='该用户已被删除'  Where `lastpostid`=" + uid.ToString());
				foreach (DataRow dataRow in DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "tablelist`").Tables[0].Rows)
				{
					if (dataRow["id"].ToString() != "")
					{
						DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
						{
							"UPDATE  `", 
							BaseConfigs.get_GetTablePrefix(), 
							"posts", 
							dataRow["id"].ToString(), 
							"` SET  `poster`='该用户已被删除'  WHERE `posterid`=", 
							uid
						}));
					}
				}
			}
			if (delpms)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` Where `msgfromid`=" + uid.ToString());
			}
			else
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "pms` SET `msgfrom`='该用户已被删除'  Where `msgfromid`=" + uid.ToString());
				DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "pms` SET `msgto`='该用户已被删除'  Where `msgtoid`=" + uid.ToString());
			}
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "moderators` WHERE `uid`=" + uid.ToString());
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "Statistics` SET `totalusers`=`totalusers`-1");
			DataTable dataTable = DbHelper.ExecuteDataset(CommandType.Text, "SELECT `uid`,`username` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` ORDER BY `uid` DESC LIMIT 1").Tables[0];
			if (dataTable.Rows.Count > 0)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"statistics` SET `lastuserid`=", 
					dataTable.Rows[0][0], 
					", `lastusername`='", 
					dataTable.Rows[0][1], 
					"'"
				}));
			}
			return true;
		}
		public DataTable GetUserGroup(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`=?groupid", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public DataTable GetAdminGroup(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "admingroups` WHERE `admingid`=?groupid LIMIT 1", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public void AddUserGroup(UserGroupInfo __usergroupinfo, int Creditshigher, int Creditslower)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?Radminid", DbType.Boolean, 4, __usergroupinfo.get_Radminid()), 
				DbHelper.MakeInParam("?Grouptitle", (DbType)253, 50, Utils.RemoveFontTag(__usergroupinfo.get_Grouptitle())), 
				DbHelper.MakeInParam("?Creditshigher", DbType.Boolean, 4, Creditshigher), 
				DbHelper.MakeInParam("?Creditslower", DbType.Boolean, 4, Creditslower), 
				DbHelper.MakeInParam("?Stars", DbType.Boolean, 4, __usergroupinfo.get_Stars()), 
				DbHelper.MakeInParam("?Color", (DbType)253, 7, __usergroupinfo.get_Color()), 
				DbHelper.MakeInParam("?Groupavatar", (DbType)253, 60, __usergroupinfo.get_Groupavatar()), 
				DbHelper.MakeInParam("?Readaccess", DbType.Boolean, 4, __usergroupinfo.get_Readaccess()), 
				DbHelper.MakeInParam("?Allowvisit", DbType.Boolean, 4, __usergroupinfo.get_Allowvisit()), 
				DbHelper.MakeInParam("?Allowpost", DbType.Boolean, 4, __usergroupinfo.get_Allowpost()), 
				DbHelper.MakeInParam("?Allowreply", DbType.Boolean, 4, __usergroupinfo.get_Allowreply()), 
				DbHelper.MakeInParam("?Allowpostpoll", DbType.Boolean, 4, __usergroupinfo.get_Allowpostpoll()), 
				DbHelper.MakeInParam("?Allowdirectpost", DbType.Boolean, 4, __usergroupinfo.get_Allowdirectpost()), 
				DbHelper.MakeInParam("?Allowgetattach", DbType.Boolean, 4, __usergroupinfo.get_Allowgetattach()), 
				DbHelper.MakeInParam("?Allowpostattach", DbType.Boolean, 4, __usergroupinfo.get_Allowpostattach()), 
				DbHelper.MakeInParam("?Allowvote", DbType.Boolean, 4, __usergroupinfo.get_Allowvote()), 
				DbHelper.MakeInParam("?Allowmultigroups", DbType.Boolean, 4, __usergroupinfo.get_Allowmultigroups()), 
				DbHelper.MakeInParam("?Allowsearch", DbType.Boolean, 4, __usergroupinfo.get_Allowsearch()), 
				DbHelper.MakeInParam("?Allowavatar", DbType.Boolean, 4, __usergroupinfo.get_Allowavatar()), 
				DbHelper.MakeInParam("?Allowcstatus", DbType.Boolean, 4, __usergroupinfo.get_Allowcstatus()), 
				DbHelper.MakeInParam("?Allowuseblog", DbType.Boolean, 4, __usergroupinfo.get_Allowuseblog()), 
				DbHelper.MakeInParam("?Allowinvisible", DbType.Boolean, 4, __usergroupinfo.get_Allowinvisible()), 
				DbHelper.MakeInParam("?Allowtransfer", DbType.Boolean, 4, __usergroupinfo.get_Allowtransfer()), 
				DbHelper.MakeInParam("?Allowsetreadperm", DbType.Boolean, 4, __usergroupinfo.get_Allowsetreadperm()), 
				DbHelper.MakeInParam("?Allowsetattachperm", DbType.Boolean, 4, __usergroupinfo.get_Allowsetattachperm()), 
				DbHelper.MakeInParam("?Allowhidecode", DbType.Boolean, 4, __usergroupinfo.get_Allowhidecode()), 
				DbHelper.MakeInParam("?Allowhtml", DbType.Boolean, 4, __usergroupinfo.get_Allowhtml()), 
				DbHelper.MakeInParam("?Allowcusbbcode", DbType.Boolean, 4, __usergroupinfo.get_Allowcusbbcode()), 
				DbHelper.MakeInParam("?Allownickname", DbType.Boolean, 4, __usergroupinfo.get_Allownickname()), 
				DbHelper.MakeInParam("?Allowsigbbcode", DbType.Boolean, 4, __usergroupinfo.get_Allowsigbbcode()), 
				DbHelper.MakeInParam("?Allowsigimgcode", DbType.Boolean, 4, __usergroupinfo.get_Allowsigimgcode()), 
				DbHelper.MakeInParam("?Allowviewpro", DbType.Boolean, 4, __usergroupinfo.get_Allowviewpro()), 
				DbHelper.MakeInParam("?Allowviewstats", DbType.Boolean, 4, __usergroupinfo.get_Allowviewstats()), 
				DbHelper.MakeInParam("?Disableperiodctrl", DbType.Boolean, 4, __usergroupinfo.get_Disableperiodctrl()), 
				DbHelper.MakeInParam("?Reasonpm", DbType.Boolean, 4, __usergroupinfo.get_Reasonpm()), 
				DbHelper.MakeInParam("?Maxprice", DbType.Boolean, 2, __usergroupinfo.get_Maxprice()), 
				DbHelper.MakeInParam("?Maxpmnum", DbType.Boolean, 2, __usergroupinfo.get_Maxpmnum()), 
				DbHelper.MakeInParam("?Maxsigsize", DbType.Boolean, 2, __usergroupinfo.get_Maxsigsize()), 
				DbHelper.MakeInParam("?Maxattachsize", DbType.Boolean, 4, __usergroupinfo.get_Maxattachsize()), 
				DbHelper.MakeInParam("?Maxsizeperday", DbType.Boolean, 4, __usergroupinfo.get_Maxsizeperday()), 
				DbHelper.MakeInParam("?Attachextensions", (DbType)253, 100, __usergroupinfo.get_Attachextensions()), 
				DbHelper.MakeInParam("?Maxspaceattachsize", DbType.Boolean, 4, __usergroupinfo.get_Maxspaceattachsize()), 
				DbHelper.MakeInParam("?Maxspacephotosize", DbType.Boolean, 4, __usergroupinfo.get_Maxspacephotosize()), 
				DbHelper.MakeInParam("?Raterange", (DbType)253, 100, __usergroupinfo.get_Raterange())
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "usergroups`  (`radminid`,`grouptitle`,`creditshigher`,`creditslower`,`stars` ,`color`, `groupavatar`,`readaccess`, `allowvisit`,`allowpost`,`allowreply`,`allowpostpoll`, `allowdirectpost`,`allowgetattach`,`allowpostattach`,`allowvote`,`allowmultigroups`,`allowsearch`,`allowavatar`,`allowcstatus`,`allowuseblog`,`allowinvisible`,`allowtransfer`,`allowsetreadperm`,`allowsetattachperm`,`allowhidecode`,`allowhtml`,`allowcusbbcode`,`allownickname`,`allowsigbbcode`,`allowsigimgcode`,`allowviewpro`,`allowviewstats`,`disableperiodctrl`,`reasonpm`,`maxprice`,`maxpmnum`,`maxsigsize`,`maxattachsize`,`maxsizeperday`,`attachextensions`,`raterange`,`maxspaceattachsize`,`maxspacephotosize`) VALUES(?Radminid,?Grouptitle,?Creditshigher,?Creditslower,?Stars,?Color,?Groupavatar,?Readaccess,?Allowvisit,?Allowpost,?Allowreply,?Allowpostpoll,?Allowdirectpost,?Allowgetattach,?Allowpostattach,?Allowvote,?Allowmultigroups,?Allowsearch,?Allowavatar,?Allowcstatus,?Allowuseblog,?Allowinvisible,?Allowtransfer,?Allowsetreadperm,?Allowsetattachperm,?Allowhidecode,?Allowhtml,?Allowcusbbcode,?Allownickname,?Allowsigbbcode,?Allowsigimgcode,?Allowviewpro,?Allowviewstats,?Disableperiodctrl,?Reasonpm,?Maxprice,?Maxpmnum,?Maxsigsize,?Maxattachsize,?Maxsizeperday,?Attachextensions,?Raterange,?Maxspaceattachsize,?Maxspacephotosize)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void AddOnlineList(string grouptitle)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, this.GetMaxUserGroupId()), 
				DbHelper.MakeInParam("?title", (DbType)253, 50, grouptitle)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "onlinelist` (`groupid`, `title`, `img`) VALUES(?groupid,?title,'')";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetMinCreditHigher()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT MIN(Creditshigher) FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`>8 AND `radminid`=0 ").Tables[0];
		}
		public DataTable GetMaxCreditLower()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT MAX(Creditslower) FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`>8 AND `radminid`=0 ").Tables[0];
		}
		public DataTable GetUserGroupByCreditshigher(int Creditshigher)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?Creditshigher", DbType.Boolean, 4, Creditshigher);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `groupid`,`creditshigher`,`creditslower` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`>8 AND `radminid`=0  AND `Creditshigher`<=?Creditshigher AND ?Creditshigher<`Creditslower` LIMIT 1", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public void UpdateUserGroupCreditsHigher(int currentGroupID, int Creditslower)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?creditshigher", DbType.Boolean, 4, Creditslower), 
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, currentGroupID)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups` SET creditshigher=?creditshigher WHERE `groupid`=?groupid", array);
		}
		public void UpdateUserGroupCreidtsLower(int currentCreditsHigher, int Creditshigher)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?creditslower", DbType.Boolean, 4, Creditshigher), 
				DbHelper.MakeInParam("?creditshigher", DbType.Boolean, 4, currentCreditsHigher)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups` SET `creditslower`=?creditslower WHERE `groupid`>8 AND `radminid`=0 AND `creditshigher`=?creditshigher", array);
		}
		public DataTable GetUserGroupByCreditsHigherAndLower(int Creditshigher, int Creditslower)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?Creditshigher", DbType.Boolean, 4, Creditshigher), 
				DbHelper.MakeInParam("?Creditslower", DbType.Boolean, 4, Creditslower)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `groupid` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`>8 AND `radminid`=0 AND `Creditshigher`=?Creditshigher AND `Creditslower`=?Creditslower", array).Tables[0];
		}
		public int GetGroupCountByCreditsLower(int Creditshigher)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?creditslower", DbType.Boolean, 4, Creditshigher);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `groupid` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`>8 AND `radminid`=0 AND `creditslower`=?creditslower", new DbParameter[]
			{
				dbParameter
			}).Tables[0].Rows.Count;
		}
		public void UpdateUserGroupsCreditsLowerByCreditsLower(int Creditslower, int Creditshigher)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?Creditslower", DbType.Boolean, 4, Creditslower), 
				DbHelper.MakeInParam("?Creditshigher", DbType.Boolean, 4, Creditshigher)
			};
			DbHelper.ExecuteDataset(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups` SET `creditslower`=?Creditslower WHERE `groupid`>8 AND `radminid`=0 AND `creditslower`=?Creditshigher", array);
		}
		public void UpdateUserGroupTitleAndCreditsByGroupid(int groupid, string grouptitle, int creditslower, int creditshigher)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?grouptitle", (DbType)253, 50, grouptitle), 
				DbHelper.MakeInParam("?creditshigher", DbType.Boolean, 4, creditshigher), 
				DbHelper.MakeInParam("?creditslower", DbType.Boolean, 4, creditslower), 
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups` SET `grouptitle`=?grouptitle,`creditshigher`=?creditshigher,`creditslower`=?creditslower WHERE `groupid`=?groupid";
			DbHelper.ExecuteDataset(CommandType.Text, text, array);
		}
		public void UpdateUserGroupsCreditsHigherByCreditsHigher(int Creditshigher, int Creditslower)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?Creditshigher", DbType.Boolean, 4, Creditshigher), 
				DbHelper.MakeInParam("?Creditslower", DbType.Boolean, 4, Creditslower)
			};
			DbHelper.ExecuteDataset(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups` SET `Creditshigher`=?Creditshigher WHERE `groupid`>8 AND `radminid`=0 AND `Creditshigher`=?Creditslower", array);
		}
		public DataTable GetUserGroupCreditsLowerAndHigher(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `groupid`,`creditshigher`,`creditslower` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups`  WHERE `groupid`=?groupid LIMIT 1", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public void UpdateUserGroup(UserGroupInfo usergroupinfo, int Creditshigher, int Creditslower)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?Radminid", DbType.Boolean, 4, (usergroupinfo.get_Groupid() == 1) ? 1 : usergroupinfo.get_Radminid()), 
				DbHelper.MakeInParam("?Grouptitle", (DbType)253, 50, Utils.RemoveFontTag(usergroupinfo.get_Grouptitle())), 
				DbHelper.MakeInParam("?Creditshigher", DbType.Boolean, 4, Creditshigher), 
				DbHelper.MakeInParam("?Creditslower", DbType.Boolean, 4, Creditslower), 
				DbHelper.MakeInParam("?Stars", DbType.Boolean, 4, usergroupinfo.get_Stars()), 
				DbHelper.MakeInParam("?Color", (DbType)253, 7, usergroupinfo.get_Color()), 
				DbHelper.MakeInParam("?Groupavatar", (DbType)253, 60, usergroupinfo.get_Groupavatar()), 
				DbHelper.MakeInParam("?Readaccess", DbType.Boolean, 4, usergroupinfo.get_Readaccess()), 
				DbHelper.MakeInParam("?Allowvisit", DbType.Boolean, 4, usergroupinfo.get_Allowvisit()), 
				DbHelper.MakeInParam("?Allowpost", DbType.Boolean, 4, usergroupinfo.get_Allowpost()), 
				DbHelper.MakeInParam("?Allowreply", DbType.Boolean, 4, usergroupinfo.get_Allowreply()), 
				DbHelper.MakeInParam("?Allowpostpoll", DbType.Boolean, 4, usergroupinfo.get_Allowpostpoll()), 
				DbHelper.MakeInParam("?Allowdirectpost", DbType.Boolean, 4, usergroupinfo.get_Allowdirectpost()), 
				DbHelper.MakeInParam("?Allowgetattach", DbType.Boolean, 4, usergroupinfo.get_Allowgetattach()), 
				DbHelper.MakeInParam("?Allowpostattach", DbType.Boolean, 4, usergroupinfo.get_Allowpostattach()), 
				DbHelper.MakeInParam("?Allowvote", DbType.Boolean, 4, usergroupinfo.get_Allowvote()), 
				DbHelper.MakeInParam("?Allowmultigroups", DbType.Boolean, 4, usergroupinfo.get_Allowmultigroups()), 
				DbHelper.MakeInParam("?Allowsearch", DbType.Boolean, 4, usergroupinfo.get_Allowsearch()), 
				DbHelper.MakeInParam("?Allowavatar", DbType.Boolean, 4, usergroupinfo.get_Allowavatar()), 
				DbHelper.MakeInParam("?Allowcstatus", DbType.Boolean, 4, usergroupinfo.get_Allowcstatus()), 
				DbHelper.MakeInParam("?Allowuseblog", DbType.Boolean, 4, usergroupinfo.get_Allowuseblog()), 
				DbHelper.MakeInParam("?Allowinvisible", DbType.Boolean, 4, usergroupinfo.get_Allowinvisible()), 
				DbHelper.MakeInParam("?Allowtransfer", DbType.Boolean, 4, usergroupinfo.get_Allowtransfer()), 
				DbHelper.MakeInParam("?Allowsetreadperm", DbType.Boolean, 4, usergroupinfo.get_Allowsetreadperm()), 
				DbHelper.MakeInParam("?Allowsetattachperm", DbType.Boolean, 4, usergroupinfo.get_Allowsetattachperm()), 
				DbHelper.MakeInParam("?Allowhidecode", DbType.Boolean, 4, usergroupinfo.get_Allowhidecode()), 
				DbHelper.MakeInParam("?Allowhtml", DbType.Boolean, 4, usergroupinfo.get_Allowhtml()), 
				DbHelper.MakeInParam("?Allowcusbbcode", DbType.Boolean, 4, usergroupinfo.get_Allowcusbbcode()), 
				DbHelper.MakeInParam("?Allownickname", DbType.Boolean, 4, usergroupinfo.get_Allownickname()), 
				DbHelper.MakeInParam("?Allowsigbbcode", DbType.Boolean, 4, usergroupinfo.get_Allowsigbbcode()), 
				DbHelper.MakeInParam("?Allowsigimgcode", DbType.Boolean, 4, usergroupinfo.get_Allowsigimgcode()), 
				DbHelper.MakeInParam("?Allowviewpro", DbType.Boolean, 4, usergroupinfo.get_Allowviewpro()), 
				DbHelper.MakeInParam("?Allowviewstats", DbType.Boolean, 4, usergroupinfo.get_Allowviewstats()), 
				DbHelper.MakeInParam("?Allowtrade", DbType.Boolean, 4, usergroupinfo.get_Allowtrade()), 
				DbHelper.MakeInParam("?Allowdiggs", DbType.Boolean, 4, usergroupinfo.get_Allowdiggs()), 
				DbHelper.MakeInParam("?Disableperiodctrl", DbType.Boolean, 4, usergroupinfo.get_Disableperiodctrl()), 
				DbHelper.MakeInParam("?Allowdebate", DbType.Boolean, 4, usergroupinfo.get_Allowdebate()), 
				DbHelper.MakeInParam("?Allowbonus", DbType.Boolean, 4, usergroupinfo.get_Allowbonus()), 
				DbHelper.MakeInParam("?Minbonusprice", DbType.Boolean, 4, usergroupinfo.get_Minbonusprice()), 
				DbHelper.MakeInParam("?Maxbonusprice", DbType.Boolean, 4, usergroupinfo.get_Maxbonusprice()), 
				DbHelper.MakeInParam("?Reasonpm", DbType.Boolean, 4, usergroupinfo.get_Reasonpm()), 
				DbHelper.MakeInParam("?Maxprice", DbType.Boolean, 2, usergroupinfo.get_Maxprice()), 
				DbHelper.MakeInParam("?Maxpmnum", DbType.Boolean, 2, usergroupinfo.get_Maxpmnum()), 
				DbHelper.MakeInParam("?Maxsigsize", DbType.Boolean, 2, usergroupinfo.get_Maxsigsize()), 
				DbHelper.MakeInParam("?Maxattachsize", DbType.Boolean, 4, usergroupinfo.get_Maxattachsize()), 
				DbHelper.MakeInParam("?Maxsizeperday", DbType.Boolean, 4, usergroupinfo.get_Maxsizeperday()), 
				DbHelper.MakeInParam("?Attachextensions", (DbType)253, 100, usergroupinfo.get_Attachextensions()), 
				DbHelper.MakeInParam("?Maxspaceattachsize", DbType.Boolean, 4, usergroupinfo.get_Maxspaceattachsize()), 
				DbHelper.MakeInParam("?Maxspacephotosize", DbType.Boolean, 4, usergroupinfo.get_Maxspacephotosize()), 
				DbHelper.MakeInParam("?Groupid", DbType.Boolean, 4, usergroupinfo.get_Groupid())
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups`  SET `radminid`=?Radminid,`grouptitle`=?Grouptitle,`creditshigher`=?Creditshigher,`creditslower`=?Creditslower,`stars`=?Stars,`color`=?Color,`groupavatar`=?Groupavatar,`readaccess`=?Readaccess, `allowvisit`=?Allowvisit,`allowpost`=?Allowpost,`allowreply`=?Allowreply,`allowpostpoll`=?Allowpostpoll, `allowdirectpost`=?Allowdirectpost,`allowgetattach`=?Allowgetattach,`allowpostattach`=?Allowpostattach,`allowvote`=?Allowvote,`allowmultigroups`=?Allowmultigroups,`allowsearch`=?Allowsearch,`allowavatar`=?Allowavatar,`allowcstatus`=?Allowcstatus,`allowuseblog`=?Allowuseblog,`allowinvisible`=?Allowinvisible,`allowtransfer`=?Allowtransfer,`allowsetreadperm`=?Allowsetreadperm,`allowsetattachperm`=?Allowsetattachperm,`allowhidecode`=?Allowhidecode,`allowhtml`=?Allowhtml,`allowcusbbcode`=?Allowcusbbcode,`allownickname`=?Allownickname,`allowsigbbcode`=?Allowsigbbcode,`allowsigimgcode`=?Allowsigimgcode,`allowviewpro`=?Allowviewpro,`allowviewstats`=?Allowviewstats,`allowtrade`=?Allowtrade,`allowdiggs`=?Allowdiggs,`disableperiodctrl`=?Disableperiodctrl,`allowdebate`=?Allowdebate,`allowbonus`=?Allowbonus,`minbonusprice`=?Minbonusprice,`maxbonusprice`=?Maxbonusprice,`reasonpm`=?Reasonpm,`maxprice`=?Maxprice,`maxpmnum`=?Maxpmnum,`maxsigsize`=?Maxsigsize,`maxattachsize`=?Maxattachsize,`maxsizeperday`=?Maxsizeperday,`attachextensions`=?Attachextensions,`maxspaceattachsize`=?Maxspaceattachsize,`maxspacephotosize`=?Maxspacephotosize  WHERE `groupid`=?Groupid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateOnlineList(UserGroupInfo __usergroupinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?title", (DbType)253, 50, Utils.RemoveFontTag(__usergroupinfo.get_Grouptitle()))
			};
			string text = string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"onlinelist` SET `title`=?title WHERE `groupid`=", 
				__usergroupinfo.get_Groupid()
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public bool IsSystemOrTemplateUserGroup(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT *  FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE (`system`=1 OR `type`=1) AND `groupid`=?groupid", new DbParameter[]
			{
				dbParameter
			}).Tables[0].Rows.Count > 0;
		}
		public DataTable GetOthersCommonUserGroup(int exceptgroupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, exceptgroupid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `groupid` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `radminid`=0 And `groupid`>8 AND `groupid`<>?groupid", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public string GetUserGroupRAdminId(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `radminid` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE  `groupid`=?groupid", new DbParameter[]
			{
				dbParameter
			}).Tables[0].Rows[0][0].ToString();
		}
		public void UpdateUserGroupLowerAndHigherToLimit(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "usergroups` SET `creditshigher`=-9999999 ,creditslower=9999999  WHERE `groupid`=?groupid", new DbParameter[]
			{
				dbParameter
			});
		}
		public void DeleteUserGroup(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`=?groupid", new DbParameter[]
			{
				dbParameter
			});
		}
		public void DeleteAdminGroup(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "admingroups` WHERE `admingid`=?groupid", new DbParameter[]
			{
				dbParameter
			});
		}
		public void DeleteOnlineList(int groupid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid);
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "onlinelist` WHERE `groupid`=?groupid", new DbParameter[]
			{
				dbParameter
			});
		}
		public int GetMaxUserGroupId()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT IFNULL(MAX(groupid),0) FROM " + BaseConfigs.get_GetTablePrefix() + "usergroups"), 0);
		}
		public bool DeletePaymentLog()
		{
			bool result;
			try
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "paymentlog`");
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public bool DeletePaymentLog(string condition)
		{
			bool result;
			try
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "paymentlog` WHERE " + condition);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public DataTable GetPaymentLogList(int pagesize, int currentpage)
		{
			string text = string.Concat(new string[]
			{
				"SELECT ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.*, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.fid as fid, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.postdatetime as postdatatime, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.poster as authorname, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.title, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.username as username, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.id FROM (", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog INNER JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.tid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.tid) INNER JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.uid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.uid LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetPaymentLogList(int pagesize, int currentpage, string condition)
		{
			string text = string.Concat(new string[]
			{
				"SELECT ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.*, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.fid as fid, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.postdatetime as postdatatime, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.poster as authorname, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.title as title, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.username as username FROM (", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog INNER JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.tid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.tid) INNER JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.uid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.uid WHERE ", 
				condition, 
				" ORDER BY ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.id DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetPaymentLogListCount()
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "paymentlog`").Tables[0].Rows[0][0].ToString());
		}
		public int GetPaymentLogListCount(string condition)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "paymentlog` WHERE " + condition).Tables[0].Rows[0][0].ToString());
		}
		public void DeleteModeratorByFid(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "moderators` WHERE `fid`=?fid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetUidModeratorByFid(string fidlist)
		{
			string text = string.Concat(new string[]
			{
				"SELECT DISTINCT `uid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"moderators` WHERE `fid` IN(", 
				fidlist, 
				")"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void AddModerator(int uid, int fid, int displayorder, int inherited)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?fid", DbType.Byte, 2, fid), 
				DbHelper.MakeInParam("?displayorder", DbType.Byte, 2, displayorder), 
				DbHelper.MakeInParam("?inherited", DbType.Byte, 2, inherited)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "moderators` (uid,fid,displayorder,inherited) VALUES(?uid,?fid,?displayorder,?inherited)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetModeratorInfo(string moderator)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)253, 20, moderator.Trim())
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "Select `uid`,`groupid`  From `" + BaseConfigs.get_GetTablePrefix() + "users` Where `groupid`<>7 AND `groupid`<>8 AND `username`=?username LIMIT 1", array).Tables[0];
		}
		public void SetModerator(string moderator)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)253, 20, moderator.Trim())
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `adminid`=3,`groupid`=3 WHERE `username`=?username", array);
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "online` SET `adminid`=3,`groupid`=3 WHERE `username`=?username", array);
		}
		public DataTable GetUidAdminIdByUsername(string username)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)253, 20, username)
			};
			string text = "Select `uid`,`adminid` From `" + BaseConfigs.get_GetTablePrefix() + "users` Where `username` = ?username LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public DataTable GetUidInModeratorsByUid(int currentfid, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?currentfid", DbType.Boolean, 4, currentfid), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "Select `uid`  FROM `" + BaseConfigs.get_GetTablePrefix() + "moderators` WHERE `fid`<>?currentfid AND `uid`=?uid LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void UpdateUserOnlineInfo(int groupid, int userid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid), 
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "online` SET `groupid`=?groupid  WHERE `userid`=?userid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateUserOtherInfo(int groupid, int userid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid), 
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `groupid`=?groupid ,`adminid`=0  WHERE `uid`=?userid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public bool GetLastUserInfo(out string lastuserid, out string lastusername)
		{
			lastuserid = "";
			lastusername = "";
			IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, "SELECT `uid`,`username` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` ORDER BY `uid` DESC LIMIT 1");
			bool result;
			if (dataReader != null)
			{
				if (dataReader.Read())
				{
					lastuserid = dataReader["uid"].ToString();
					lastusername = dataReader["username"].ToString().Trim();
					dataReader.Close();
					result = true;
					return result;
				}
				dataReader.Close();
			}
			result = false;
			return result;
		}
		public IDataReader GetTopUsers(int statcount, int lastuid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("@lastuid", DbType.Boolean, 4, lastuid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new object[]
			{
				"SELECT TOP ", 
				statcount, 
				" [uid] FROM [", 
				BaseConfigs.get_GetTablePrefix(), 
				"users] WHERE [uid] > @lastuid"
			}), array);
		}
		public void ResetUserDigestPosts(int userid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userid);
			int num = Utils.StrToInt(DbHelper.ExecuteScalarToStr(CommandType.Text, string.Concat(new object[]
			{
				"SELECT COUNT(tid) AS `digestposts` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`posterid` =", 
				userid, 
				" AND `digest` > 0"
			})), 0);
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `digestposts`=", 
				num, 
				" WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid` = ?uid"
			}), new DbParameter[]
			{
				dbParameter
			});
		}
		public IDataReader GetUsers(int start_uid, int end_uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?start_uid", DbType.Boolean, 4, start_uid), 
				DbHelper.MakeInParam("?end_uid", DbType.Boolean, 4, end_uid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid` >= ?start_uid AND `uid`<=?end_uid", array);
		}
		public void UpdateUserPostCount(int postcount, int userid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?postcount", DbType.Boolean, 4, postcount), 
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `posts`=?postcount WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid` = ?userid"
			}), array);
		}
		public DataTable GetModeratorList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT * FROM `{0}moderators`", BaseConfigs.get_GetTablePrefix())).Tables[0];
		}
		public int GetOnlineAllUserCount()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(olid) FROM `" + BaseConfigs.get_GetTablePrefix() + "online`"), 1);
		}
		public int CreateOnlineTable()
		{
			int result;
			try
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DROP TABLE IF EXISTS `" + BaseConfigs.get_GetTablePrefix() + "online`");
				DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE TABLE `" + BaseConfigs.get_GetTablePrefix() + "online` (`olid` int(11) NOT NULL auto_increment,`userid` int(11) NOT NULL default '-1',`ip` varchar(15) NOT NULL default '0.0.0.0', `username` varchar(20) NOT NULL default '',`nickname` varchar(20) NOT NULL default '',`password` varchar(32) NOT NULL default '',`groupid` smallint(6) NOT NULL default '0',`olimg` varchar(80) NOT NULL default '', `adminid` smallint(6) NOT NULL default '0',`invisible` smallint(6) NOT NULL default '0',`action` smallint(6) NOT NULL default '0', `lastactivity` smallint(6) NOT NULL default '0', `lastposttime` datetime NOT NULL default '1900-01-01 00:00:00',`lastpostpmtime` datetime NOT NULL default '1900-01-01 00:00:00', `lastsearchtime` datetime NOT NULL default '1900-01-01 00:00:00', `lastupdatetime` datetime NOT NULL,`forumid` int(11) NOT NULL default '0',`forumname` varchar(50) NOT NULL default '', `titleid` int(11) NOT NULL default '0',`title` varchar(80) NOT NULL default '', `verifycode` varchar(10) NOT NULL default '',PRIMARY KEY(`olid`), KEY `forum` (`userid`,`forumid`,`invisible`),KEY `forumid` (`forumid`), KEY `invisible` (`userid`,`invisible`),KEY `ip` (`userid`,`ip`),KEY `password` (`userid`,`password`) ) ENGINE=MEMORY AUTO_INCREMENT=1 DEFAULT CHARSET=gbk");
				DbHelper.ExecuteNonQuery(CommandType.Text, "ALTER TABLE `" + BaseConfigs.get_GetTablePrefix() + "online` ADD PRIMARY KEY ( `olid` ) ");
				DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE INDEX `forum` ON `" + BaseConfigs.get_GetTablePrefix() + "online`(`userid`, `forumid`, `invisible`);");
				DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE INDEX `invisible` ON `" + BaseConfigs.get_GetTablePrefix() + "online`(`userid`, `invisible`)");
				DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE INDEX `forumid` ON `" + BaseConfigs.get_GetTablePrefix() + "online`(`forumid`)");
				DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE INDEX `password` ON `" + BaseConfigs.get_GetTablePrefix() + "online`(`userid`, `password`)");
				DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE INDEX `ip` ON `" + BaseConfigs.get_GetTablePrefix() + "online`(`userid`, `ip`)");
				result = 1;
			}
			catch
			{
				result = -1;
			}
			return result;
		}
		public int GetOnlineUserCount()
		{
			return int.Parse(DbHelper.ExecuteDataset(CommandType.Text, "SELECT COUNT(olid) FROM `" + BaseConfigs.get_GetTablePrefix() + "online` WHERE `userid`>0").Tables[0].Rows[0][0].ToString());
		}
		public DataTable GetForumOnlineUserListTable(int forumid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT * FROM `{0}online` WHERE `forumid`={1}", BaseConfigs.get_GetTablePrefix(), forumid)).Tables[0];
		}
		public DataTable GetOnlineUserListTable()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "online`").Tables[0];
		}
		public IDataReader GetForumOnlineUserList(int forumid)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM `{0}online` WHERE `forumid`={1}", BaseConfigs.get_GetTablePrefix(), forumid.ToString()));
		}
		public IDataReader GetOnlineUserList()
		{
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "online`");
		}
		public DataTable GetOnlineGroupIconTable()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `groupid`, `displayorder`, `title`, `img` FROM `" + BaseConfigs.get_GetTablePrefix() + "onlinelist` WHERE `img` <> '' ORDER BY `displayorder`").Tables[0];
		}
		public int GetOlidByUid(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, uid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalarToStr(CommandType.Text, string.Format("SELECT olid FROM `{0}online` WHERE `userid`=?userid", BaseConfigs.get_GetTablePrefix()), array), -1);
		}
		public IDataReader GetOnlineUser(int olid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?olid", DbType.Boolean, 4, olid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM `{0}online` WHERE `olid`=?olid", BaseConfigs.get_GetTablePrefix()), array);
		}
		public DataTable GetOnlineUser(int userid, string password)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid), 
				DbHelper.MakeInParam("?password", (DbType)253, 32, password)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT * FROM `{0}online` WHERE `userid`=?userid AND `password`=?password LIMit 1", BaseConfigs.get_GetTablePrefix()), array).Tables[0];
		}
		public DataTable GetOnlineUserByIP(int userid, string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid), 
				DbHelper.MakeInParam("?ip", (DbType)253, 15, ip)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT * FROM `{0}online` WHERE `userid`=?userid AND `ip`=?ip LIMIT 1", BaseConfigs.get_GetTablePrefix()), array).Tables[0];
		}
		public bool CheckUserVerifyCode(int olid, string verifycode, string newverifycode)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?olid", DbType.Boolean, 4, olid), 
				DbHelper.MakeInParam("?verifycode", (DbType)253, 10, verifycode)
			};
			DataTable dataTable = DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT `olid` FROM `{0}online` WHERE `olid`=?olid and `verifycode`=?verifycode LIMIT 1", BaseConfigs.get_GetTablePrefix()), array).Tables[0];
			array[1].Value = newverifycode;
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `verifycode`=?verifycode WHERE `olid`=?olid", BaseConfigs.get_GetTablePrefix()), array);
			return dataTable.Rows.Count > 0;
		}
		public int SetUserOnlineState(int uid, int onlinestate)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}users` SET `onlinestate`={1},`lastactivity`=now() WHERE `uid`={2}", BaseConfigs.get_GetTablePrefix(), onlinestate, uid));
		}
		public int DeleteRowsByIP(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)253, 15, ip)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `onlinestate`=0,`lastactivity`=now() WHERE `uid` IN (SELECT `userid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"online` WHERE `userid`>0 AND `ip`=?ip)"
			}), array);
			int result;
			if (ip != "0.0.0.0")
			{
				result = DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "online` WHERE `userid`=-1 AND `ip`=?ip", array);
			}
			else
			{
				result = 0;
			}
			return result;
		}
		public int DeleteRows(int olid)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "online` WHERE `olid`=" + olid.ToString());
		}
		public void UpdateAction(int olid, int action, int inid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastupdatetime", DbType.Int64, 8, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), 
				DbHelper.MakeInParam("?action", DbType.Byte, 2, action), 
				DbHelper.MakeInParam("?forumid", DbType.Boolean, 4, inid), 
				DbHelper.MakeInParam("?forumname", (DbType)253, 100, ""), 
				DbHelper.MakeInParam("?titleid", DbType.Boolean, 4, inid), 
				DbHelper.MakeInParam("?title", (DbType)253, 160, ""), 
				DbHelper.MakeInParam("?olid", DbType.Boolean, 4, olid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "online` SET `lastactivity`=`action`,`action`=?action,`lastupdatetime`=?lastupdatetime,`forumid`=?forumid,`forumname`=?forumname,`titleid`=?titleid,`title`=?title WHERE `olid`=?olid", array);
		}
		public void UpdateAction(int olid, int action, int fid, string forumname, int tid, string topictitle)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastupdatetime", DbType.Int64, 8, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))), 
				DbHelper.MakeInParam("?action", DbType.Byte, 2, action), 
				DbHelper.MakeInParam("?forumid", DbType.Boolean, 4, fid), 
				DbHelper.MakeInParam("?forumname", (DbType)253, 100, forumname), 
				DbHelper.MakeInParam("?titleid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?title", (DbType)253, 160, topictitle), 
				DbHelper.MakeInParam("?olid", DbType.Boolean, 4, olid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "online` SET `lastactivity`=`action`,`action`=?action,`lastupdatetime`=?lastupdatetime,`forumid`=?forumid,`forumname`=?forumname,`titleid`=?titleid,`title`=?title WHERE `olid`=?olid", array);
		}
		public void UpdateLastTime(int olid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastupdatetime", DbType.Int64, 8, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))), 
				DbHelper.MakeInParam("?olid", DbType.Boolean, 4, olid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "online` SET `lastupdatetime`=?lastupdatetime WHERE `olid`=?olid", array);
		}
		public void UpdatePostTime(int olid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `lastposttime`=now() WHERE `olid`={1}", BaseConfigs.get_GetTablePrefix(), olid.ToString()));
		}
		public void UpdatePostPMTime(int olid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `lastpostpmtime`=now() WHERE `olid`={1}", BaseConfigs.get_GetTablePrefix(), olid.ToString()));
		}
		public void UpdateInvisible(int olid, int invisible)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `invisible`={1} WHERE `olid`={2}", BaseConfigs.get_GetTablePrefix(), invisible.ToString(), olid.ToString()));
		}
		public void UpdatePassword(int olid, string password)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?olid", DbType.Boolean, 4, olid), 
				DbHelper.MakeInParam("?password", (DbType)254, 32, password)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `password`=now() WHERE `olid`=?olid", BaseConfigs.get_GetTablePrefix()), array);
		}
		public void UpdateIP(int olid, string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)253, 15, ip), 
				DbHelper.MakeInParam("?olid", DbType.Boolean, 4, olid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `ip`=?ip WHERE `olid`=?olid", BaseConfigs.get_GetTablePrefix()), array);
		}
		public void UpdateSearchTime(int olid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `lastsearchtime`=now() WHERE `olid`={1}", BaseConfigs.get_GetTablePrefix(), olid.ToString()));
		}
		public void UpdateGroupid(int userid, int groupid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}online` SET `groupid`={1} WHERE `userid`={2}", BaseConfigs.get_GetTablePrefix(), groupid.ToString(), userid.ToString()));
		}
		public IDataReader GetPrivateMessageInfo(int pmid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pmid", DbType.Boolean, 4, pmid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE `pmid`=?pmid LIMIT 1", array);
		}
		public IDataReader GetPrivateMessageList(int userid, int folder, int pagesize, int pageindex, int inttype)
		{
			string text = "";
			if (inttype == 1)
			{
				text = "`new`=1";
			}
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?strwhere", (DbType)253, 500, text)
			};
			string text2 = "msgtoid";
			if (folder == 1 || folder == 2)
			{
				text2 = "msgfromid";
			}
			string text3;
			if (text != "")
			{
				object[] array2 = new object[14];
				array2[0] = "SELECT `pmid`,`msgfrom`,`msgfromid`,`msgto`,`msgtoid`,`folder`,`new`,`subject`,`postdatetime`,`message` FROM `";
				array2[1] = BaseConfigs.get_GetTablePrefix();
				array2[2] = "pms` WHERE `";
				array2[3] = text2;
				array2[4] = "`= ";
				array2[5] = userid;
				array2[6] = "  AND `folder`= ";
				array2[7] = folder;
				array2[8] = "  AND  ";
				array2[9] = text;
				array2[10] = " ORDER BY `pmid` DESC LIMIT ";
				object[] arg_F6_0 = array2;
				int arg_F6_1 = 11;
				int num = (pageindex - 1) * pagesize;
				arg_F6_0[arg_F6_1] = num.ToString();
				array2[12] = ",";
				array2[13] = pagesize;
				text3 = string.Concat(array2);
			}
			else
			{
				object[] array2 = new object[12];
				array2[0] = "SELECT `pmid`,`msgfrom`,`msgfromid`,`msgto`,`msgtoid`,`folder`,`new`,`subject`,`postdatetime`,`message` FROM `";
				array2[1] = BaseConfigs.get_GetTablePrefix();
				array2[2] = "pms` WHERE `";
				array2[3] = text2;
				array2[4] = "`= ";
				array2[5] = userid;
				array2[6] = "  AND `folder`= ";
				array2[7] = folder;
				array2[8] = "  ORDER BY `pmid` DESC LIMIT ";
				object[] arg_186_0 = array2;
				int arg_186_1 = 9;
				int num = (pageindex - 1) * pagesize;
				arg_186_0[arg_186_1] = num.ToString();
				array2[10] = ",";
				array2[11] = pagesize;
				text3 = string.Concat(array2);
			}
			return DbHelper.ExecuteReader(CommandType.Text, text3, array);
		}
		public int GetPrivateMessageCount(int userid, int folder, int state)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid), 
				DbHelper.MakeInParam("?folder", DbType.Boolean, 4, folder), 
				DbHelper.MakeInParam("?state", DbType.Boolean, 4, state)
			};
			string text = string.Empty;
			if (folder == -1)
			{
				text = "SELECT COUNT(pmid) AS `pmcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE (`msgtoid`=?userid AND `folder`=0) OR (`msgfromid`=?userid AND `folder` = 1)  OR (`msgfromid` = ?userid AND `folder` = 2)";
			}
			else
			{
				if (folder == 0)
				{
					if (state == -1)
					{
						text = "SELECT COUNT(pmid) AS `pmcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE `msgtoid`=?userid AND `folder`=?folder";
					}
					else
					{
						text = "SELECT COUNT(pmid) AS `pmcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE `msgtoid`=?userid AND `folder`=?folder AND `new`=?state";
					}
				}
				else
				{
					if (state == -1)
					{
						text = "SELECT COUNT(pmid) AS `pmcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE `msgfromid`=?userid AND `folder`=?folder";
					}
					else
					{
						text = "SELECT COUNT(pmid) AS `pmcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE `msgfromid`=?userid AND `folder`=?folder AND `new`=?state";
					}
				}
			}
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString(), 0);
		}
		public int CreatePrivateMessage(PrivateMessageInfo __privatemessageinfo, int savetosentbox)
		{
			if (__privatemessageinfo.get_Folder() != 0)
			{
				__privatemessageinfo.set_Msgfrom(__privatemessageinfo.get_Msgto());
			}
			else
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users` SET `newpmcount`= ABS(IF(ISNULL(`newpmcount`),0,newpmcount)*1)+1,`newpm` = 1 WHERE `uid`=", 
					__privatemessageinfo.get_Msgtoid()
				}));
			}
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?msgfrom", (DbType)253, 20, __privatemessageinfo.get_Msgfrom()), 
				DbHelper.MakeInParam("?msgfromid", DbType.Boolean, 4, __privatemessageinfo.get_Msgfromid()), 
				DbHelper.MakeInParam("?msgto", (DbType)253, 20, __privatemessageinfo.get_Msgto()), 
				DbHelper.MakeInParam("?msgtoid", DbType.Boolean, 4, __privatemessageinfo.get_Msgtoid()), 
				DbHelper.MakeInParam("?folder", DbType.Byte, 2, __privatemessageinfo.get_Folder()), 
				DbHelper.MakeInParam("?new", DbType.Boolean, 4, __privatemessageinfo.get_New()), 
				DbHelper.MakeInParam("?subject", DbType.Single, 80, __privatemessageinfo.get_Subject()), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, DateTime.Parse(__privatemessageinfo.get_Postdatetime())), 
				DbHelper.MakeInParam("?message", (DbType)253, 0, __privatemessageinfo.get_Message()), 
				DbHelper.MakeInParam("?savetosentbox", DbType.Boolean, 4, savetosentbox), 
				DbHelper.MakeInParam("?pmid", DbType.Boolean, 4, __privatemessageinfo.get_Pmid())
			};
			string text = "insert into `" + BaseConfigs.get_GetTablePrefix() + "pms`(`msgfrom`,`msgfromid`,`msgto`,`msgtoid`,`folder`,`new`,`subject`,`postdatetime`,`message`) VALUES(?msgfrom,?msgfromid,?msgto,?msgtoid,?folder,?new,?subject,?postdatetime,?message)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			int result = Utils.StrToInt(DbHelper.ExecuteDataset(CommandType.Text, "select pmid from `" + BaseConfigs.get_GetTablePrefix() + "pms` order by pmid desc LIMIT 1").Tables[0].Rows[0][0].ToString(), -1);
			if (savetosentbox == 1 && __privatemessageinfo.get_Folder() == 0)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "pms` (`msgfrom`,`msgfromid`,`msgto`,`msgtoid`,`folder`,`new`,`subject`,`postdatetime`,`message`) VALUES (?msgfrom,?msgfromid,?msgto,?msgtoid,1,?new,?subject,?postdatetime,?message)", array);
			}
			return result;
		}
		public int DeletePrivateMessages(int userid, string pmidlist)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"pms` WHERE `pmid` IN (", 
				pmidlist, 
				") AND (`msgtoid` = ?userid OR `msgfromid` = ?userid)"
			}), array);
		}
		public int GetNewPMCount(int userid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(`pmid`) AS `pmcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE `new` = 1 AND `folder` = 0 AND `msgtoid` = ?userid", array), 0);
		}
		public int DeletePrivateMessage(int userid, int pmid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pmid", DbType.Boolean, 4, pmid), 
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` WHERE `pmid`=?pmid AND (`msgtoid` = ?userid OR `msgfromid` = ?userid)", array);
		}
		public int SetPrivateMessageState(int pmid, byte state)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?state", DbType.Byte, 1, state), 
				DbHelper.MakeInParam("?pmid", DbType.Boolean, 1, pmid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "pms` SET `new`=?state WHERE `pmid`=?pmid", array);
		}
		public int GetRAdminIdByGroup(int groupid)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT `radminid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` WHERE `groupid`=", 
				groupid, 
				" LIMIT 1"
			})).Tables[0].Rows[0][0].ToString());
		}
		public string GetUserGroupsStr()
		{
			return "SELECT `groupid`, `grouptitle` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` ORDER BY `groupid`";
		}
		public DataTable GetUserNameListByGroupid(string groupidlist)
		{
			string text = string.Concat(new string[]
			{
				"SELECT `uid` ,`username`  From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `groupid` IN(", 
				groupidlist, 
				")"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetUserNameByUid(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "SELECT `username` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=?uid LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void ResetPasswordUid(string password, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?password", (DbType)254, 32, password), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `password`=?password WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void SendPMToUser(string msgfrom, int msgfromid, string msgto, int msgtoid, int folder, string subject, DateTime postdatetime, string message)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?msgfrom", (DbType)253, 50, msgfrom), 
				DbHelper.MakeInParam("?msgfromid", DbType.Boolean, 4, msgfromid), 
				DbHelper.MakeInParam("?msgto", (DbType)254, 50, msgto), 
				DbHelper.MakeInParam("?msgtoid", DbType.Boolean, 4, msgtoid), 
				DbHelper.MakeInParam("?folder", DbType.Byte, 2, folder), 
				DbHelper.MakeInParam("?subject", (DbType)254, 60, subject), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, postdatetime), 
				DbHelper.MakeInParam("?message", (DbType)254, 0, message)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "pms` (msgfrom,msgfromid,msgto,msgtoid,folder,new,subject,postdatetime,message) VALUES (?msgfrom,?msgfromid,?msgto,?msgtoid,?folder,1,?subject,?postdatetime,?message)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?msgtoid", DbType.Boolean, 4, msgtoid)
			};
			text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `newpmcount`=`newpmcount`+1  WHERE `uid` =?msgtoid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array2);
		}
		public string GetSystemGroupInfoSql()
		{
			return "Select * From `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`<=8 Order By `groupid`";
		}
		public void UpdateUserCredits(int uid, string credits)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}users` SET `credits` = {1} WHERE `uid`=?uid", BaseConfigs.get_GetTablePrefix(), credits), array);
		}
		public void UpdateUserGroup(int uid, int groupid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}users` SET `groupid` = {1} WHERE `uid`=?uid", BaseConfigs.get_GetTablePrefix(), groupid), array);
		}
		public bool CheckUserCreditsIsEnough(int uid, float[] values)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?extcredits1", DbType.AnsiString, 8, values[0]), 
				DbHelper.MakeInParam("?extcredits2", DbType.AnsiString, 8, values[1]), 
				DbHelper.MakeInParam("?extcredits3", DbType.AnsiString, 8, values[2]), 
				DbHelper.MakeInParam("?extcredits4", DbType.AnsiString, 8, values[3]), 
				DbHelper.MakeInParam("?extcredits5", DbType.AnsiString, 8, values[4]), 
				DbHelper.MakeInParam("?extcredits6", DbType.AnsiString, 8, values[5]), 
				DbHelper.MakeInParam("?extcredits7", DbType.AnsiString, 8, values[6]), 
				DbHelper.MakeInParam("?extcredits8", DbType.AnsiString, 8, values[7])
			};
			string text = "SELECT COUNT(1) FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=?uid AND\t`extcredits1`>= IF(?extcredits1<0 ,ABS(?extcredits1),`extcredits1`) AND \t`extcredits2`>= IF(?extcredits2<0 ,ABS(?extcredits2),`extcredits2`) AND \t`extcredits3`>= IF(?extcredits3<0 ,ABS(?extcredits3),`extcredits3`) AND \t`extcredits4`>= IF(?extcredits4<0 ,ABS(?extcredits4),`extcredits4`) AND \t`extcredits5`>= IF(?extcredits5<0 ,ABS(?extcredits5),`extcredits5`) AND \t`extcredits6`>= IF(?extcredits6<0 ,ABS(?extcredits6),`extcredits6`) AND \t`extcredits7`>= IF(?extcredits7<0 ,ABS(?extcredits7),`extcredits7`) AND \t`extcredits8`>= IF(?extcredits8<0 ,ABS(?extcredits8),`extcredits8`) ";
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, text, array)) != 0;
		}
		public void UpdateUserCredits(int uid, float[] values)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?extcredits1", DbType.AnsiString, 8, values[0]), 
				DbHelper.MakeInParam("?extcredits2", DbType.AnsiString, 8, values[1]), 
				DbHelper.MakeInParam("?extcredits3", DbType.AnsiString, 8, values[2]), 
				DbHelper.MakeInParam("?extcredits4", DbType.AnsiString, 8, values[3]), 
				DbHelper.MakeInParam("?extcredits5", DbType.AnsiString, 8, values[4]), 
				DbHelper.MakeInParam("?extcredits6", DbType.AnsiString, 8, values[5]), 
				DbHelper.MakeInParam("?extcredits7", DbType.AnsiString, 8, values[6]), 
				DbHelper.MakeInParam("?extcredits8", DbType.AnsiString, 8, values[7]), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET \t\t`extcredits1`=`extcredits1` + ?extcredits1, \t\t`extcredits2`=`extcredits2` + ?extcredits2, \t\t`extcredits3`=`extcredits3` + ?extcredits3, \t\t`extcredits4`=`extcredits4` + ?extcredits4, \t\t`extcredits5`=`extcredits5` + ?extcredits5, \t\t`extcredits6`=`extcredits6` + ?extcredits6, \t\t`extcredits7`=`extcredits7` + ?extcredits7, \t\t`extcredits8`=`extcredits8` + ?extcredits8 WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public bool CheckUserCreditsIsEnough(int uid, DataRow values, int pos, int mount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?extcredits1", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits1"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits2", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits2"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits3", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits3"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits4", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits4"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits5", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits5"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits6", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits6"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits7", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits7"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits8", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits8"], 0f) * (float)pos * (float)mount)
			};
			string text = "SELECT count(1) FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=?uid AND\t`extcredits1`>= IF(?extcredits1>=0,abs(?extcredits1),0) AND \t`extcredits2`>= IF(?extcredits2>=0,abs(?extcredits2),0) AND \t`extcredits3`>= IF(?extcredits3>=0,abs(?extcredits3),0) AND \t`extcredits4`>= IF(?extcredits4>=0,abs(?extcredits4),0) AND \t`extcredits5`>= IF(?extcredits5>=0,abs(?extcredits5),0) AND \t`extcredits6`>= IF(?extcredits6>=0,abs(?extcredits6),0) AND \t`extcredits7`>= IF(?extcredits7>=0,abs(?extcredits7),0) AND \t`extcredits8`>= IF(?extcredits8>=0,abs(?extcredits8),0)";
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, text, array)) != 0;
		}
		public void UpdateUserCredits(int uid, DataRow values, int pos, int mount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?extcredits1", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits1"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits2", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits2"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits3", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits3"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits4", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits4"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits5", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits5"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits6", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits6"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits7", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits7"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?extcredits8", DbType.AnsiString, 8, Utils.StrToFloat(values["extcredits8"], 0f) * (float)pos * (float)mount), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			if (pos < 0 && mount < 0)
			{
				for (int i = 1; i < array.Length; i++)
				{
					array[i].Value = -Convert.ToInt32(array[i].Value);
				}
			}
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET \t`extcredits1`=`extcredits1` + ?extcredits1, \t`extcredits2`=`extcredits2` + ?extcredits2, \t`extcredits3`=`extcredits3` + ?extcredits3, \t`extcredits4`=`extcredits4` + ?extcredits4, \t`extcredits5`=`extcredits5` + ?extcredits5, \t`extcredits6`=`extcredits6` + ?extcredits6, \t`extcredits7`=`extcredits7` + ?extcredits7, \t`extcredits8`=`extcredits8` + ?extcredits8 WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetUserGroups()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` ORDER BY `groupid`").Tables[0];
		}
		public DataTable GetUserGroupRateRange(int groupid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `raterange` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` WHERE `groupid`=", 
				groupid.ToString(), 
				" LIMIT 1"
			})).Tables[0];
		}
		public IDataReader GetUserTodayRate(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `extcredits`, SUM(ABS(`score`)) AS `todayrate` FROM `" + BaseConfigs.get_GetTablePrefix() + "ratelog` WHERE DATEDIFF(`postdatetime`,CURDATE()) = 0 AND `uid` = ?uid GROUP BY `extcredits`", array);
		}
		public string GetSpecialGroupInfoSql()
		{
			return "Select * From `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `radminid`=-1 And `groupid`>8 Order By `groupid`";
		}
		public IDataReader GetUserInfoToReader(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string format = string.Concat(new string[]
			{
				"SELECT `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.*, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.website,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.icq,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.qq,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.yahoo,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.msn,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.skype,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.location,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.customstatus,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.avatar,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.avatarwidth,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.avatarheight,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.medals,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.bio,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.signature,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.sightml,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.authstr,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.authtime,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.authflag,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.realname,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.idcard,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.mobile,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.phone  FROM ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users LEFT JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`uid` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`=?uid LIMIT 1"
			});
			return DbHelper.ExecuteReader(CommandType.Text, string.Format(format, BaseConfigs.get_GetTablePrefix()), array);
		}
		public IDataReader GetShortUserInfoToReader(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=?uid", array);
		}
		public IDataReader GetUserInfoByIP(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?regip", (DbType)254, 15, ip)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.*, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.* FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`uid` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`regip`=?regip ORDER BY `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid` DESC LIMIT 1"
			}), array);
		}
		public IDataReader GetUserName(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `username` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`=?uid LIMIT 1"
			}), array);
		}
		public IDataReader GetUserJoinDate(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `joindate` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`=?uid LIMIT 1"
			}), array);
		}
		public IDataReader GetUserID(string username)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)253, 20, username)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `uid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`username`=?username LIMIT 1"
			}), array);
		}
		public DataTable GetUserList(int pagesize, int currentpage)
		{
			return DbHelper.ExecuteDataset(string.Concat(new string[]
			{
				"SELECT a.`uid`, a.`username`,a.`nickname`, a.`joindate`, a.`credits`, a.`posts`, a.`lastactivity`, a.`email`,a.`lastvisit`,a.`lastvisit`,a.`accessmasks`, a.`location`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups`.`grouptitle` FROM (SELECT `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.*,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`location` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`uid` = `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`) AS a LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups`.`groupid`=a.`groupid` ORDER BY a.`uid` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			})).Tables[0];
		}
		public DataTable GetUserList(int pagesize, int pageindex, string orderby, string ordertype)
		{
			string[] array = new string[]
			{
				"username", 
				"credits", 
				"posts", 
				"admin", 
				"lastactivity"
			};
			switch (Array.IndexOf<string>(array, orderby))
			{
				case 0:
				{
					orderby = string.Concat(new string[]
					{
						"ORDER BY `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`username` ", 
						ordertype, 
						",`", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`uid` ", 
						ordertype
					});
					break;
				}
				case 1:
				{
					orderby = string.Concat(new string[]
					{
						"ORDER BY `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`credits` ", 
						ordertype, 
						",`", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`uid` ", 
						ordertype
					});
					break;
				}
				case 2:
				{
					orderby = string.Concat(new string[]
					{
						"ORDER BY `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`posts` ", 
						ordertype, 
						",`", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`uid` ", 
						ordertype
					});
					break;
				}
				case 3:
				{
					orderby = string.Concat(new string[]
					{
						"WHERE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`adminid` > 0 ORDER BY `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`adminid`,`", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`uid` ", 
						ordertype
					});
					break;
				}
				case 4:
				{
					orderby = string.Concat(new string[]
					{
						"ORDER BY `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`lastactivity` ", 
						ordertype, 
						",", 
						BaseConfigs.get_GetTablePrefix(), 
						"users.uid ", 
						ordertype
					});
					break;
				}
				default:
				{
					orderby = "ORDER BY `" + BaseConfigs.get_GetTablePrefix() + "users`.`uid` " + ordertype;
					break;
				}
			}
			string text = BaseConfigs.get_GetTablePrefix() + "users";
			string text2 = BaseConfigs.get_GetTablePrefix() + "userfields";
			string text3 = string.Concat(new string[]
			{
				"SELECT `", 
				text, 
				"`.`uid`,`", 
				text, 
				"`.`username`,`", 
				text, 
				"`.`groupid`,`", 
				text, 
				"`.`nickname`, `", 
				text, 
				"`.`joindate`, `", 
				text, 
				"`.`credits`, `", 
				text, 
				"`.`posts`,`", 
				text, 
				"`.`lastactivity`, `", 
				text, 
				"`.`email`, `", 
				text2, 
				"`.`location` FROM `", 
				text, 
				"` LEFT JOIN `", 
				text2, 
				"` ON `", 
				text2, 
				"`.`uid` = `", 
				text, 
				"`.`uid` ", 
				orderby, 
				" LIMIT ", 
				((pageindex - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text3).Tables[0];
		}
		public bool Exists(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(1) FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=?uid", array)) >= 1;
		}
		public bool Exists(string username)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)254, 20, username)
			};
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(1) FROM `{0}users` WHERE `username`=?username", BaseConfigs.get_GetTablePrefix()), array)) >= 1;
		}
		public bool ExistsByIP(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?regip", (DbType)254, 15, ip)
			};
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(1) FROM `{0}users` WHERE `regip`=?regip", BaseConfigs.get_GetTablePrefix()), array)) >= 1;
		}
		public IDataReader CheckEmailAndSecques(string username, string email, string secques)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)254, 20, username), 
				DbHelper.MakeInParam("?email", (DbType)254, 50, email), 
				DbHelper.MakeInParam("?secques", (DbType)254, 8, secques)
			};
			string text = "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `username`=?username AND `email`=?email AND `secques`=?secques LIMIT 1";
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader CheckPasswordAndSecques(string username, string password, bool originalpassword, string secques)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)254, 20, username), 
				DbHelper.MakeInParam("?password", (DbType)254, 32, originalpassword ? Utils.MD5(password) : password), 
				DbHelper.MakeInParam("?secques", (DbType)254, 8, secques)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `username`=?username AND `password`=?password AND `secques`=?secques LIMIT 1", array);
		}
		public IDataReader CheckPassword(string username, string password, bool originalpassword)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)254, 20, username), 
				DbHelper.MakeInParam("?password", (DbType)254, 32, originalpassword ? Utils.MD5(password) : password)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `username`=?username AND `password`=?password LIMIT 1", array);
		}
		public IDataReader CheckDvBbsPasswordAndSecques(string username, string password)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)254, 20, username)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `uid`, `password`, `secques` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `username`=?username LIMIT 1", array);
		}
		public IDataReader CheckPassword(int uid, string password, bool originalpassword)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?password", (DbType)254, 32, originalpassword ? Utils.MD5(password) : password)
			};
			string text = "SELECT `uid`, `groupid`, `adminid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=?uid AND `password`=?password LIMIT 1";
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader FindUserEmail(string email)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?email", (DbType)254, 50, email)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `email`=?email LIMIT 1", array);
		}
		public int GetUserCount()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(uid) FROM `" + BaseConfigs.get_GetTablePrefix() + "users`"), 0);
		}
		public int GetUserCountByAdmin()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new string[]
			{
				"SELECT COUNT(uid) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`adminid` > 0"
			})), 0);
		}
		public int CreateUser(UserInfo __userinfo)
		{
			int result;
			if (this.Exists(__userinfo.get_Username()))
			{
				result = -1;
			}
			else
			{
				DbParameter[] array = new DbParameter[]
				{
					DbHelper.MakeInParam("?username", (DbType)254, 20, __userinfo.get_Username()), 
					DbHelper.MakeInParam("?nickname", (DbType)254, 20, __userinfo.get_Nickname()), 
					DbHelper.MakeInParam("?password", (DbType)254, 32, __userinfo.get_Password()), 
					DbHelper.MakeInParam("?secques", (DbType)254, 8, __userinfo.get_Secques()), 
					DbHelper.MakeInParam("?gender", DbType.Boolean, 4, __userinfo.get_Gender()), 
					DbHelper.MakeInParam("?adminid", DbType.Boolean, 4, __userinfo.get_Adminid()), 
					DbHelper.MakeInParam("?groupid", DbType.Byte, 2, __userinfo.get_Groupid()), 
					DbHelper.MakeInParam("?groupexpiry", DbType.Boolean, 4, __userinfo.get_Groupexpiry()), 
					DbHelper.MakeInParam("?extgroupids", (DbType)254, 60, __userinfo.get_Extgroupids()), 
					DbHelper.MakeInParam("?regip", (DbType)253, 0, __userinfo.get_Regip()), 
					DbHelper.MakeInParam("?joindate", (DbType)253, 0, __userinfo.get_Joindate()), 
					DbHelper.MakeInParam("?lastip", (DbType)254, 15, __userinfo.get_Lastip()), 
					DbHelper.MakeInParam("?lastvisit", (DbType)253, 0, __userinfo.get_Lastvisit()), 
					DbHelper.MakeInParam("?lastactivity", (DbType)253, 0, __userinfo.get_Lastactivity()), 
					DbHelper.MakeInParam("?lastpost", (DbType)253, 0, __userinfo.get_Lastpost()), 
					DbHelper.MakeInParam("?lastpostid", DbType.Boolean, 4, __userinfo.get_Lastpostid()), 
					DbHelper.MakeInParam("?lastposttitle", (DbType)253, 0, __userinfo.get_Lastposttitle()), 
					DbHelper.MakeInParam("?posts", DbType.Boolean, 4, __userinfo.get_Posts()), 
					DbHelper.MakeInParam("?digestposts", DbType.Byte, 2, __userinfo.get_Digestposts()), 
					DbHelper.MakeInParam("?oltime", DbType.Byte, 2, __userinfo.get_Oltime()), 
					DbHelper.MakeInParam("?pageviews", DbType.Boolean, 4, __userinfo.get_Pageviews()), 
					DbHelper.MakeInParam("?credits", DbType.Boolean, 4, __userinfo.get_Credits()), 
					DbHelper.MakeInParam("?extcredits1", DbType.Date, 8, __userinfo.get_Extcredits1()), 
					DbHelper.MakeInParam("?extcredits2", DbType.Date, 8, __userinfo.get_Extcredits2()), 
					DbHelper.MakeInParam("?extcredits3", DbType.Date, 8, __userinfo.get_Extcredits3()), 
					DbHelper.MakeInParam("?extcredits4", DbType.Date, 8, __userinfo.get_Extcredits4()), 
					DbHelper.MakeInParam("?extcredits5", DbType.Date, 8, __userinfo.get_Extcredits5()), 
					DbHelper.MakeInParam("?extcredits6", DbType.Date, 8, __userinfo.get_Extcredits6()), 
					DbHelper.MakeInParam("?extcredits7", DbType.Date, 8, __userinfo.get_Extcredits7()), 
					DbHelper.MakeInParam("?extcredits8", DbType.Date, 8, __userinfo.get_Extcredits8()), 
					DbHelper.MakeInParam("?avatarshowid", DbType.Boolean, 4, __userinfo.get_Avatarshowid()), 
					DbHelper.MakeInParam("?email", (DbType)254, 50, __userinfo.get_Email()), 
					DbHelper.MakeInParam("?bday", (DbType)253, 0, __userinfo.get_Bday()), 
					DbHelper.MakeInParam("?sigstatus", DbType.Boolean, 4, __userinfo.get_Sigstatus()), 
					DbHelper.MakeInParam("?tpp", DbType.Boolean, 4, __userinfo.get_Tpp()), 
					DbHelper.MakeInParam("?ppp", DbType.Boolean, 4, __userinfo.get_Ppp()), 
					DbHelper.MakeInParam("?templateid", DbType.Byte, 2, __userinfo.get_Templateid()), 
					DbHelper.MakeInParam("?pmsound", DbType.Boolean, 4, __userinfo.get_Pmsound()), 
					DbHelper.MakeInParam("?showemail", DbType.Boolean, 4, __userinfo.get_Showemail()), 
					DbHelper.MakeInParam("?newsletter", DbType.Boolean, 4, __userinfo.get_Newsletter()), 
					DbHelper.MakeInParam("?invisible", DbType.Boolean, 4, __userinfo.get_Invisible()), 
					DbHelper.MakeInParam("?newpm", DbType.Boolean, 4, __userinfo.get_Newpm()), 
					DbHelper.MakeInParam("?accessmasks", DbType.Boolean, 4, __userinfo.get_Accessmasks())
				};
				DbParameter[] array2 = new DbParameter[]
				{
					DbHelper.MakeInParam("?website", (DbType)253, 80, __userinfo.get_Website()), 
					DbHelper.MakeInParam("?icq", (DbType)253, 12, __userinfo.get_Icq()), 
					DbHelper.MakeInParam("?qq", (DbType)253, 12, __userinfo.get_Qq()), 
					DbHelper.MakeInParam("?yahoo", (DbType)253, 40, __userinfo.get_Yahoo()), 
					DbHelper.MakeInParam("?msn", (DbType)253, 40, __userinfo.get_Msn()), 
					DbHelper.MakeInParam("?skype", (DbType)253, 40, __userinfo.get_Skype()), 
					DbHelper.MakeInParam("?location", (DbType)253, 30, __userinfo.get_Location()), 
					DbHelper.MakeInParam("?customstatus", (DbType)253, 30, __userinfo.get_Customstatus()), 
					DbHelper.MakeInParam("?avatar", (DbType)253, 255, __userinfo.get_Avatar()), 
					DbHelper.MakeInParam("?avatarwidth", DbType.Boolean, 4, (__userinfo.get_Avatarwidth() == 0) ? 60 : __userinfo.get_Avatarwidth()), 
					DbHelper.MakeInParam("?avatarheight", DbType.Boolean, 4, (__userinfo.get_Avatarheight() == 0) ? 60 : __userinfo.get_Avatarheight()), 
					DbHelper.MakeInParam("?medals", (DbType)253, 40, __userinfo.get_Medals()), 
					DbHelper.MakeInParam("?bio", (DbType)253, 0, __userinfo.get_Bio()), 
					DbHelper.MakeInParam("?signature", (DbType)253, 0, __userinfo.get_Signature()), 
					DbHelper.MakeInParam("?sightml", (DbType)253, 0, __userinfo.get_Sightml()), 
					DbHelper.MakeInParam("?authstr", (DbType)253, 20, __userinfo.get_Authstr()), 
					DbHelper.MakeInParam("?realname", (DbType)253, 10, __userinfo.get_Realname()), 
					DbHelper.MakeInParam("?idcard", (DbType)253, 20, __userinfo.get_Idcard()), 
					DbHelper.MakeInParam("?mobile", (DbType)253, 20, __userinfo.get_Mobile()), 
					DbHelper.MakeInParam("?phone", (DbType)253, 20, __userinfo.get_Phone())
				};
				string text = string.Empty;
				MySqlConnection mySqlConnection = new MySqlConnection(DbHelper.get_ConnectionString());
				mySqlConnection.Open();
				int num2;
				using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
				{
					try
					{
						int num;
						DbHelper.ExecuteNonQuery(ref num, mySqlTransaction, CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "users`(`username`,`nickname`, `password`, `secques`, `gender`, `adminid`, `groupid`, `groupexpiry`, `extgroupids`, `regip`, `joindate`, `lastip`, `lastvisit`, `lastactivity`, `lastpost`, `lastpostid`, `lastposttitle`, `posts`, `digestposts`, `oltime`, `pageviews`, `credits`, `extcredits1`, `extcredits2`, `extcredits3`, `extcredits4`, `extcredits5`, `extcredits6`, `extcredits7`, `extcredits8`, `avatarshowid`, `email`, `bday`, `sigstatus`, `tpp`, `ppp`, `templateid`, `pmsound`, `showemail`, `newsletter`, `invisible`, `newpm`, `accessmasks`) VALUES(?username,?nickname, ?password, ?secques, ?gender, ?adminid, ?groupid, ?groupexpiry, ?extgroupids, ?regip, ?joindate, ?lastip, ?lastvisit, ?lastactivity, ?lastpost, ?lastpostid, ?lastposttitle, ?posts, ?digestposts, ?oltime, ?pageviews, ?credits, ?extcredits1, ?extcredits2, ?extcredits3, ?extcredits4, ?extcredits5, ?extcredits6, ?extcredits7, ?extcredits8, ?avatarshowid, ?email, ?bday, ?sigstatus, ?tpp, ?ppp, ?templateid, ?pmsound, ?showemail, ?newsletter, ?invisible, ?newpm, ?accessmasks);SELECT @@session.identity", array);
						num2 = num;
						text = string.Concat(new object[]
						{
							"INSERT INTO `", 
							BaseConfigs.get_GetTablePrefix(), 
							"userfields` (`uid`,`website`,`icq`,`qq`,`yahoo`,`msn`,`skype`,`location`,`customstatus`,`avatar`,`avatarwidth`,`avatarheight`,`medals`,`bio`,`signature`,`sightml`,`authstr`,`authtime`,`realname`,`idcard`,`mobile`,`phone`) VALUES (", 
							num2, 
							",?website,?icq,?qq,?yahoo,?msn,?skype,?location,?customstatus,?avatar,?avatarwidth,?avatarheight,?medals,?bio,?signature,?sightml,?authstr,NOW(),?realname,?idcard,?mobile,?phone)"
						});
						DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, text, array2);
						DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, string.Concat(new object[]
						{
							"UPDATE `", 
							BaseConfigs.get_GetTablePrefix(), 
							"statistics` SET `totalusers`=`totalusers` + 1,`lastusername`=?username,`lastuserid`=", 
							num2
						}), array);
						mySqlTransaction.Commit();
					}
					catch (Exception ex)
					{
						mySqlTransaction.Rollback();
						throw ex;
					}
					finally
					{
						mySqlConnection.Close();
					}
				}
				result = Utils.StrToInt(num2, -1);
			}
			return result;
		}
		public void UpdateAuthStr(int uid, string authstr, int authflag)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?authstr", (DbType)254, 20, authstr), 
				DbHelper.MakeInParam("?authflag", DbType.Boolean, 4, authflag), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "userfields` SET `authstr`=?authstr, `authtime` = now(), `authflag`=?authflag WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateUserProfile(UserInfo __userinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?nickname", (DbType)254, 20, __userinfo.get_Nickname()), 
				DbHelper.MakeInParam("?gender", DbType.Boolean, 4, __userinfo.get_Gender()), 
				DbHelper.MakeInParam("?email", (DbType)254, 50, __userinfo.get_Email()), 
				DbHelper.MakeInParam("?bday", (DbType)254, 10, __userinfo.get_Bday()), 
				DbHelper.MakeInParam("?showemail", DbType.Boolean, 4, __userinfo.get_Showemail()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, __userinfo.get_Uid())
			};
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?website", (DbType)253, 80, __userinfo.get_Website()), 
				DbHelper.MakeInParam("?icq", (DbType)253, 12, __userinfo.get_Icq()), 
				DbHelper.MakeInParam("?qq", (DbType)253, 12, __userinfo.get_Qq()), 
				DbHelper.MakeInParam("?yahoo", (DbType)253, 40, __userinfo.get_Yahoo()), 
				DbHelper.MakeInParam("?msn", (DbType)253, 40, __userinfo.get_Msn()), 
				DbHelper.MakeInParam("?skype", (DbType)253, 40, __userinfo.get_Skype()), 
				DbHelper.MakeInParam("?location", (DbType)253, 30, __userinfo.get_Location()), 
				DbHelper.MakeInParam("?bio", (DbType)253, 0, __userinfo.get_Bio()), 
				DbHelper.MakeInParam("?realname", (DbType)253, 10, __userinfo.get_Realname()), 
				DbHelper.MakeInParam("?idcard", (DbType)253, 20, __userinfo.get_Idcard()), 
				DbHelper.MakeInParam("?mobile", (DbType)253, 20, __userinfo.get_Mobile()), 
				DbHelper.MakeInParam("?phone", (DbType)253, 20, __userinfo.get_Phone()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, __userinfo.get_Uid())
			};
			MySqlConnection mySqlConnection = new MySqlConnection(BaseConfigs.get_GetDBConnectString());
			mySqlConnection.Open();
			using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
			{
				try
				{
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET  `nickname`=?nickname, `gender`=?gender , `email`=?email , `bday`=?bday, `showemail`=?showemail WHERE `uid`=?uid", array);
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "userfields` SET  `website`=?website , `icq`=?icq , `qq`=?qq , `yahoo`=?yahoo , `msn`=?msn , `skype`=?skype , `location`=?location , `bio`=?bio,`idcard`=?idcard,`mobile`=?mobile,`phone`=?phone,`realname`=?realname WHERE  `uid`=?uid", array2);
					mySqlTransaction.Commit();
				}
				catch (Exception ex)
				{
					mySqlTransaction.Rollback();
					throw ex;
				}
				finally
				{
					mySqlConnection.Close();
				}
			}
		}
		public void UpdateUserForumSetting(UserInfo __userinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tpp", DbType.Boolean, 4, __userinfo.get_Tpp()), 
				DbHelper.MakeInParam("?ppp", DbType.Boolean, 4, __userinfo.get_Ppp()), 
				DbHelper.MakeInParam("?invisible", DbType.Boolean, 4, __userinfo.get_Invisible()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, __userinfo.get_Uid())
			};
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?signature", (DbType)253, 500, __userinfo.get_Signature()), 
				DbHelper.MakeInParam("?sightml", (DbType)253, 1000, __userinfo.get_Sightml()), 
				DbHelper.MakeInParam("?customstatus", (DbType)253, 30, __userinfo.get_Customstatus()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, __userinfo.get_Uid())
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET  `tpp`=?tpp, `ppp`=?ppp,`invisible`=?invisible WHERE `uid`=?uid", array);
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "userfields` SET  `signature`=?signature,`sightml` = ?sightml,`customstatus`=?customstatus WHERE  `uid`=?uid", array2);
		}
		public void UpdateUserExtCredits(int uid, int extid, float pos)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `extcredits", 
				extid.ToString(), 
				"`=`extcredits", 
				extid.ToString(), 
				"` + (", 
				pos.ToString(), 
				") WHERE `uid`=", 
				uid.ToString()
			}));
		}
		public float GetUserExtCredits(int uid, int extid)
		{
			return Utils.StrToFloat(DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `extcredits", 
				extid.ToString(), 
				"` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `uid`=", 
				uid.ToString()
			})).Tables[0].Rows[0][0], 0f);
		}
		public void UpdateUserPreference(int uid, string avatar, int avatarwidth, int avatarheight, int templateid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?avatar", (DbType)253, 255, avatar), 
				DbHelper.MakeInParam("?avatarwidth", DbType.Boolean, 4, avatarwidth), 
				DbHelper.MakeInParam("?avatarheight", DbType.Boolean, 4, avatarheight), 
				DbHelper.MakeInParam("?templateid", DbType.Boolean, 4, templateid)
			};
			MySqlConnection mySqlConnection = new MySqlConnection(DbHelper.get_ConnectionString());
			mySqlConnection.Open();
			using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
			{
				try
				{
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "userfields` SET `avatar`=?avatar, `avatarwidth`=?avatarwidth, `avatarheight`=?avatarheight WHERE `uid`=?uid", array);
					DbHelper.ExecuteNonQuery(mySqlTransaction, CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `templateid`=?templateid WHERE `uid`=?uid", array);
					mySqlTransaction.Commit();
				}
				catch
				{
					mySqlTransaction.Rollback();
				}
				finally
				{
					mySqlConnection.Close();
				}
			}
		}
		public void UpdateUserPassword(int uid, string password, bool originalpassword)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?password", (DbType)254, 100, originalpassword ? Utils.MD5(password) : password), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE " + BaseConfigs.get_GetTablePrefix() + "users  SET  `password`=?password  WHERE  `uid`=?uid", array);
		}
		public void UpdateUserSecques(int uid, string secques)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?secques", (DbType)254, 8, secques), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `secques`=?secques WHERE `uid`=?uid", array);
		}
		public void UpdateUserLastvisit(int uid, string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)253, 15, ip), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `lastvisit`=NOW(), `lastip`=?ip WHERE `uid` =?uid", array);
		}
		public void UpdateUserOnlineStateAndLastActivity(string uidlist, int onlinestate, string activitytime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?onlinestate", DbType.Boolean, 4, onlinestate), 
				DbHelper.MakeInParam("?activitytime", (DbType)253, 25, activitytime)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `onlinestate`=?onlinestate,`lastactivity` = ?activitytime WHERE `uid` IN (", 
				uidlist, 
				")"
			}), array);
		}
		public void UpdateUserOnlineStateAndLastActivity(int uid, int onlinestate, string activitytime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?onlinestate", DbType.Boolean, 4, onlinestate), 
				DbHelper.MakeInParam("?activitytime", (DbType)253, 25, activitytime), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `onlinestate`=?onlinestate,`lastactivity` = ?activitytime WHERE `uid`=?uid", array);
		}
		public void UpdateUserOnlineStateAndLastVisit(string uidlist, int onlinestate, string activitytime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?onlinestate", DbType.Boolean, 4, onlinestate), 
				DbHelper.MakeInParam("?activitytime", (DbType)253, 25, activitytime)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `onlinestate`=?onlinestate,`lastvisit` = ?activitytime WHERE `uid` IN (", 
				uidlist, 
				")"
			}), array);
		}
		public void UpdateUserOnlineStateAndLastVisit(int uid, int onlinestate, string activitytime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?onlinestate", DbType.Boolean, 4, onlinestate), 
				DbHelper.MakeInParam("?activitytime", DbType.Int64, 8, DateTime.Parse(activitytime))
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `onlinestate`=?onlinestate,`lastvisit` = ?activitytime WHERE `uid`=?uid", array);
		}
		public void UpdateUserLastActivity(int uid, string activitytime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?activitytime", DbType.Int64, 8, DateTime.Parse(activitytime))
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `lastactivity` = ?activitytime  WHERE `uid` = ?uid", array);
		}
		public int SetUserNewPMCount(int uid, int pmnum)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?value", DbType.Boolean, 4, pmnum), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `newpmcount`=?value WHERE `uid`=?uid", array);
		}
		public void UpdateMedals(int uid, string medals)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?medals", (DbType)253, 300, medals), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "userfields` SET `medals`=?medals WHERE `uid`=?uid", array);
		}
		public int DecreaseNewPMCount(int uid, int subval)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?subval", DbType.Boolean, 4, subval), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			int result;
			try
			{
				result = DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `newpmcount`=IF(`newpmcount` >= 0,`newpmcount`-?subval,0) WHERE `uid`=?uid", array);
			}
			catch
			{
				result = -1;
			}
			return result;
		}
		public int GetUserNewPMCount(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT `newpmcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=?uid", array), 0);
		}
		public int UpdateUserDigest(string useridlist)
		{
			string text = string.Concat(new string[]
			{
				"SELECT COUNT(`tid`) AS `digest` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`posterid` in (", 
				useridlist, 
				") AND `digest`>0"
			});
			int num = Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, text));
			string text2 = string.Concat(new object[]
			{
				"update `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `digestposts` = ", 
				num, 
				" where `uid` in (", 
				useridlist, 
				")"
			});
			return DbHelper.ExecuteNonQuery(CommandType.Text, text2.ToString());
		}
		public void UpdateUserSpaceId(int spaceid, int userid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?spaceid", DbType.Boolean, 4, spaceid), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userid)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `spaceid`=?spaceid WHERE `uid`=?uid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetUserIdByAuthStr(string authstr)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?authstr", (DbType)253, 20, authstr)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `uid` FROM `" + BaseConfigs.get_GetTablePrefix() + "userfields` WHERE DateDiff(`authtime`,CURDATE())<=3  AND `authstr`=?authstr", array).Tables[0];
		}
		public int AddOnlineUser(OnlineUserInfo __onlineuserinfo, int timeout)
		{
			string text = "";
			if (timeout > 0)
			{
				if (__onlineuserinfo.get_Userid() > 0)
				{
					text = string.Format("{0}UPDATE `{1}users` SET `onlinestate`=1 WHERE `uid`={2};", text, BaseConfigs.get_GetTablePrefix(), __onlineuserinfo.get_Userid().ToString());
					DbHelper.ExecuteNonQuery(CommandType.Text, text);
				}
			}
			else
			{
				timeout *= -1;
			}
			if (timeout > 9999)
			{
				timeout = 9999;
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			CommandType arg_B1_0 = CommandType.Text;
			string arg_AC_0 = "SELECT `userid` FROM `{0}online` WHERE `lastupdatetime`<'{1}'";
			object arg_AC_1 = BaseConfigs.get_GetTablePrefix();
			DateTime dateTime = DateTime.Now;
			dateTime = dateTime.AddMinutes((double)(timeout * -1));
			IDataReader dataReader = DbHelper.ExecuteReader(arg_B1_0, string.Format(arg_AC_0, arg_AC_1, dateTime.ToString("yyyy-MM-dd HH:mm:ss")));
			try
			{
				while (dataReader.Read())
				{
					stringBuilder.Append(",");
					stringBuilder.Append(dataReader[0].ToString());
					if (dataReader[0].ToString() != "-1")
					{
						stringBuilder2.Append(",");
						stringBuilder2.Append(dataReader[0].ToString());
					}
				}
			}
			finally
			{
				dataReader.Close();
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(0, 1);
				text = string.Format("{0}DELETE FROM `{1}online` WHERE `userid` IN ({2});", text, BaseConfigs.get_GetTablePrefix(), stringBuilder.ToString());
				DbHelper.ExecuteNonQuery(CommandType.Text, text);
			}
			if (stringBuilder2.Length > 0)
			{
				stringBuilder2.Remove(0, 1);
				text = string.Format("{0}UPDATE `{1}users` SET `onlinestate`=0,`lastactivity`=NOW() WHERE `uid` IN ({2});", text, BaseConfigs.get_GetTablePrefix(), stringBuilder2.ToString());
				DbHelper.ExecuteNonQuery(CommandType.Text, text);
			}
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, __onlineuserinfo.get_Userid()), 
				DbHelper.MakeInParam("?ip", (DbType)253, 15, __onlineuserinfo.get_Ip()), 
				DbHelper.MakeInParam("?username", (DbType)253, 40, __onlineuserinfo.get_Username()), 
				DbHelper.MakeInParam("?nickname", (DbType)253, 40, __onlineuserinfo.get_Nickname()), 
				DbHelper.MakeInParam("?password", (DbType)254, 32, __onlineuserinfo.get_Password()), 
				DbHelper.MakeInParam("?groupid", DbType.Byte, 2, __onlineuserinfo.get_Groupid()), 
				DbHelper.MakeInParam("?olimg", (DbType)253, 80, __onlineuserinfo.get_Olimg()), 
				DbHelper.MakeInParam("?adminid", DbType.Byte, 2, __onlineuserinfo.get_Adminid()), 
				DbHelper.MakeInParam("?invisible", DbType.Byte, 2, __onlineuserinfo.get_Invisible()), 
				DbHelper.MakeInParam("?action", DbType.Byte, 2, __onlineuserinfo.get_Action()), 
				DbHelper.MakeInParam("?lastactivity", DbType.Byte, 2, __onlineuserinfo.get_Lastactivity()), 
				DbHelper.MakeInParam("?lastposttime", DbType.Int64, 8, DateTime.Parse(__onlineuserinfo.get_Lastposttime())), 
				DbHelper.MakeInParam("?lastpostpmtime", DbType.Int64, 8, DateTime.Parse(__onlineuserinfo.get_Lastpostpmtime())), 
				DbHelper.MakeInParam("?lastsearchtime", DbType.Int64, 8, DateTime.Parse(__onlineuserinfo.get_Lastsearchtime())), 
				DbHelper.MakeInParam("?lastupdatetime", DbType.Int64, 8, DateTime.Parse(__onlineuserinfo.get_Lastupdatetime())), 
				DbHelper.MakeInParam("?forumid", DbType.Boolean, 4, __onlineuserinfo.get_Forumid()), 
				DbHelper.MakeInParam("?forumname", (DbType)253, 50, ""), 
				DbHelper.MakeInParam("?titleid", DbType.Boolean, 4, __onlineuserinfo.get_Titleid()), 
				DbHelper.MakeInParam("?title", (DbType)253, 80, ""), 
				DbHelper.MakeInParam("?verifycode", (DbType)253, 10, __onlineuserinfo.get_Verifycode())
			};
			int num;
			DbHelper.ExecuteNonQuery(ref num, CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "online`(`userid`,`ip`,`username`,`nickname`,`password`,`groupid`,`olimg`,`adminid`,`invisible`,`action`,`lastactivity`,`lastposttime`,`lastpostpmtime`,`lastsearchtime`,`lastupdatetime`,`forumid`,`forumname`,`titleid`,`title`, `verifycode`) VALUES(?userid,?ip,?username,?nickname,?password,?groupid,?olimg,?adminid,?invisible,?action,?lastactivity,?lastposttime,?lastpostpmtime,?lastsearchtime,?lastupdatetime,?forumid,?forumname,?titleid,?title,?verifycode)", array);
			int num2 = num;
			int result;
			if (num2 > 2147483000)
			{
				this.CreateOnlineTable();
				DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "online`(`userid`,`ip`,`username`,`nickname`,`password`,`groupid`,`olimg`,`adminid`,`invisible`,`action`,`lastactivity`,`lastposttime`,`lastpostpmtime`,`lastsearchtime`,`lastupdatetime`,`forumid`,`titleid`,`verifycode`) VALUES(?userid,?ip,?username,?nickname,?password,?groupid,?olimg,?adminid,?invisible,?action,?lastactivity,?lastposttime,?lastpostpmtime,?lastsearchtime,?lastupdatetime,?forumid,?forumname,?titleid,?title,?verifycode)", array);
				result = 1;
			}
			else
			{
				result = 0;
			}
			return result;
		}
		private void DeleteExpiredOnlineUsers(int timeout)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			string text = "";
			CommandType arg_4D_0 = CommandType.Text;
			string arg_48_0 = "SELECT `olid`, `userid` FROM `{0}online` WHERE `lastupdatetime`<'{1}'";
			object arg_48_1 = BaseConfigs.get_GetTablePrefix();
			DateTime dateTime = DateTime.Now;
			dateTime = dateTime.AddMinutes((double)(timeout * -1));
			IDataReader dataReader = DbHelper.ExecuteReader(arg_4D_0, string.Format(arg_48_0, arg_48_1, DateTime.Parse(dateTime.ToString("yyyy-MM-dd HH:mm:ss"))));
			while (dataReader.Read())
			{
				stringBuilder.Append(",");
				stringBuilder.Append(dataReader["olid"].ToString());
				if (dataReader["userid"].ToString() != "-1")
				{
					stringBuilder2.Append(",");
					stringBuilder2.Append(dataReader["userid"].ToString());
				}
			}
			dataReader.Close();
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(0, 1);
				text = string.Format("DELETE FROM `{0}online` WHERE `olid` IN ({1});", BaseConfigs.get_GetTablePrefix(), stringBuilder.ToString());
			}
			if (stringBuilder2.Length > 0)
			{
				stringBuilder2.Remove(0, 1);
				text = string.Format("{0}UPDATE `{1}users` SET `onlinestate`=0,`lastactivity`=GETDATE() WHERE `uid` IN ({2});", text, BaseConfigs.get_GetTablePrefix(), stringBuilder2.ToString());
			}
			if (text != string.Empty)
			{
				DbHelper.ExecuteNonQuery(text);
			}
		}
		public DataTable GetUserInfo(int userid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("@uid", DbType.Boolean, 4, userid);
			string text = "SELECT TOP 1 * FROM `" + BaseConfigs.get_GetTablePrefix() + "users` WHERE `uid`=@uid";
			return DbHelper.ExecuteDataset(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public DataTable GetUserInfo(string username, string password)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?username", (DbType)254, 20, username), 
				DbHelper.MakeInParam("?password", (DbType)253, 32, password)
			};
			string text = "select * from `" + BaseConfigs.get_GetTablePrefix() + "users`  where `username`=?username And `password`=?password LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public void UpdateUserSpaceId(int userid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid);
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `spaceid`=ABS(`spaceid`) WHERE `uid`=?userid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public int GetUserIdByRewriteName(string rewritename)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?rewritename", (DbType)254, 100, rewritename);
			string text = string.Format("SELECT `userid` FROM `{0}spaceconfigs` WHERE `rewritename`=?rewritename", BaseConfigs.get_GetTablePrefix());
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			}), -1);
		}
		public void UpdateUserPMSetting(UserInfo user)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, user.get_Uid()), 
				DbHelper.MakeInParam("?pmsound", DbType.Boolean, 4, user.get_Pmsound()), 
				DbHelper.MakeInParam("?newsletter", DbType.Boolean, 4, user.get_Newsletter())
			};
			string text = string.Format("UPDATE `{0}users` SET `pmsound`=?pmsound, `newsletter`=?newsletter WHERE `uid`=?uid", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void ClearUserSpace(int uid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid);
			string text = string.Format("UPDATE `{0}users` SET `spaceid`=0 WHERE `uid`=?uid", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public IDataReader GetUserInfoByName(string username)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `uid`, `username` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE `username` like '%", 
				DataProvider.RegEsc(username), 
				"%'"
			}));
		}
		public DataTable UserList(int pagesize, int currentpage, string condition)
		{
			int num = (currentpage - 1) * pagesize;
			return DbHelper.ExecuteDataset(string.Concat(new object[]
			{
				"SELECT `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`username`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`nickname`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`joindate`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`credits`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`posts`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`lastactivity`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`email`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`lastvisit`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`lastvisit`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`accessmasks`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`location`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups`.`grouptitle` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`uid` = `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`  LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups`.`groupid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`groupid` WHERE ", 
				condition, 
				" ORDER BY `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid` DESC LIMIT ", 
				num, 
				",", 
				pagesize.ToString()
			})).Tables[0];
		}
		public void LessenTotalUsers()
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET `totalusers`=`totalusers`-1");
		}
		public void UpdateOnlineTime(int oltimespan, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?oltimespan", DbType.Boolean, 2, oltimespan), 
				DbHelper.MakeInParam("?lastupdate", DbType.Int64, 8, DateTime.Now), 
				DbHelper.MakeInParam("?expectedlastupdate", DbType.Int64, 8, DateTime.Now.AddMinutes((double)(-(double)oltimespan)))
			};
			string text = string.Format("UPDATE `{0}onlinetime` SET `thismonth`=`thismonth`+?oltimespan, `total`=`total`+?oltimespan, `lastupdate`=?lastupdate WHERE `uid`=?uid AND `lastupdate`<=?expectedlastupdate", BaseConfigs.get_GetTablePrefix());
			if (DbHelper.ExecuteNonQuery(CommandType.Text, text, array) < 1)
			{
				try
				{
					DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("INSERT INTO `{0}onlinetime`(`uid`, `thismonth`, `total`, `lastupdate`) VALUES(?uid, ?oltimespan, ?oltimespan, ?lastupdate)", BaseConfigs.get_GetTablePrefix()), array);
				}
				catch
				{
				}
			}
		}
		public void ResetThismonthOnlineTime()
		{
			DbHelper.ExecuteNonQuery(string.Format("UPDATE `{0}onlinetime` SET `thismonth`=0", BaseConfigs.get_GetTablePrefix()));
		}
		public void SynchronizeOltime(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			string text = string.Format("SELECT `total` FROM `{0}onlinetime` WHERE `uid`=?uid", BaseConfigs.get_GetTablePrefix());
			int num = Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array), 0);
			if (DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}users` SET `oltime`={1} WHERE `oltime`<{1} AND `uid`=?uid", BaseConfigs.get_GetTablePrefix(), num), array) < 1)
			{
				try
				{
					text = string.Format("UPDATE `{0}onlinetime` SET `total`=(SELECT `oltime` FROM `{0}users` WHERE `uid`=?uid) WHERE `uid`=?uid", BaseConfigs.get_GetTablePrefix());
					DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
				}
				catch
				{
				}
			}
		}
		public IDataReader GetUserByOnlineTime(string field)
		{
			string text = string.Format("SELECT  `o`.`uid`, `u`.`username`, `o`.`{0}` FROM `{1}onlinetime` `o` LEFT JOIN `{1}users` `u` ON `o`.`uid`=`u`.`uid` ORDER BY `o`.`{0}` DESC limit 20", field, BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		private string GetSqlstringByPostDatetime(string sqlstring, DateTime postdatetimeStart, DateTime postdatetimeEnd)
		{
			if (postdatetimeStart.ToString() != "")
			{
				sqlstring = sqlstring + " AND `postdatetime`>='" + postdatetimeStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
			}
			if (postdatetimeEnd.ToString() != "")
			{
				sqlstring = sqlstring + " AND `postdatetime`<='" + postdatetimeEnd.AddDays(1.0).ToString("yyyy-MM-dd HH:mm:ss") + "'";
			}
			return sqlstring;
		}
		public DataTable GetAdsTable()
		{
			CommandType arg_61_0 = CommandType.Text;
			string[] array = new string[7];
			array[0] = "SELECT `advid`, `type`, `displayorder`, `targets`, `parameters`, `code` FROM `";
			array[1] = BaseConfigs.get_GetTablePrefix();
			array[2] = "advertisements` WHERE `available`=1 AND `starttime` <='";
			string[] arg_35_0 = array;
			int arg_35_1 = 3;
			DateTime now = DateTime.Now;
			arg_35_0[arg_35_1] = now.ToShortDateString().ToString();
			array[4] = "' AND `endtime` >='";
			string[] arg_52_0 = array;
			int arg_52_1 = 5;
			now = DateTime.Now;
			arg_52_0[arg_52_1] = now.ToShortDateString().ToString();
			array[6] = "' ORDER BY `displayorder` DESC, `advid` DESC";
			return DbHelper.ExecuteDataset(arg_61_0, string.Concat(array)).Tables[0];
		}
		public DataTable GetAnnouncementList(string starttime, string endtime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?starttime", (DbType)253, 19, starttime), 
				DbHelper.MakeInParam("?endtime", (DbType)253, 19, endtime)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "announcements` WHERE `starttime` <=?starttime AND `endtime` >=?starttime ORDER BY `displayorder`, `id` DESC", array).Tables[0];
		}
		public DataTable GetSimplifiedAnnouncementList(string starttime, string endtime, int maxcount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?starttime", (DbType)253, 19, starttime), 
				DbHelper.MakeInParam("?endtime", (DbType)253, 19, endtime)
			};
			string str = " LIMIT " + maxcount;
			if (maxcount < 0)
			{
				str = "";
			}
			string text = "SELECT `id`, `title`, `poster`, `posterid`,`starttime` FROM `" + BaseConfigs.get_GetTablePrefix() + "announcements` WHERE `starttime` <=?starttime AND `endtime` >=?starttime ORDER BY `displayorder`, `id` DESC" + str;
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public int AddAnnouncement(string poster, int posterid, string title, int displayorder, string starttime, string endtime, string message)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?poster", (DbType)253, 20, poster), 
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, posterid), 
				DbHelper.MakeInParam("?title", (DbType)253, 250, title), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?starttime", DbType.Int64, 8, starttime), 
				DbHelper.MakeInParam("?endtime", DbType.Int64, 8, endtime), 
				DbHelper.MakeInParam("?message", (DbType)253, 16, message)
			};
			string text = "INSERT INTO  `" + BaseConfigs.get_GetTablePrefix() + "announcements` (`poster`,`posterid`,`title`,`displayorder`,`starttime`,`endtime`,`message`) VALUES(?poster, ?posterid, ?title, ?displayorder, ?starttime, ?endtime, ?message)";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string GetAnnouncements()
		{
			return "Select * From `" + BaseConfigs.get_GetTablePrefix() + "announcements` Order BY `id` ASC";
		}
		public void DeleteAnnouncements(string idlist)
		{
			string text = string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"announcements`  WHERE `ID` IN(", 
				idlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public DataTable GetAnnouncement(int id)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?id", DbType.Boolean, 4, id);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "announcements` WHERE `id`=?id", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public void UpdateAnnouncement(int id, string poster, string title, int displayorder, string starttime, string endtime, string message)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?title", (DbType)253, 250, title), 
				DbHelper.MakeInParam("?poster", (DbType)253, 20, poster), 
				DbHelper.MakeInParam("?starttime", DbType.Int64, 8, starttime), 
				DbHelper.MakeInParam("?endtime", DbType.Int64, 8, endtime), 
				DbHelper.MakeInParam("?message", (DbType)253, 16, message), 
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "announcements` SET `displayorder`=?displayorder,title=?title, poster=?poster,starttime=?starttime,endtime=?endtime,message=?message WHERE `id`=?id";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteAnnouncement(int id)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?id", DbType.Boolean, 4, id);
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "announcements` WHERE `id`=?id", new DbParameter[]
			{
				dbParameter
			});
		}
		public IDataReader GetVisibleForumList()
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT `name`, `fid`, `layer` FROM `{0}forums` WHERE `parentid` NOT IN (SELECT fid FROM `{0}forums` WHERE `status` < 1 AND `layer` = 0) AND `status` > 0 AND `displayorder` >=0 ORDER BY `displayorder`", BaseConfigs.get_GetTablePrefix()));
		}
		public IDataReader GetOnlineGroupIconList()
		{
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `title`, `img` FROM `" + BaseConfigs.get_GetTablePrefix() + "onlinelist` WHERE `img`<> '' ORDER BY `displayorder`");
		}
		public DataTable GetForumLinkList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `name`,`url`,`note`,`displayorder`+10000 AS `displayorder`,`logo` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumlinks` WHERE `displayorder` > 0 AND `logo` = '' UNION SELECT `name`,`url`,`note`,`displayorder`,`logo` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forumlinks` WHERE `displayorder` > 0 AND `logo` <> '' ORDER BY `displayorder`"
			})).Tables[0];
		}
		public DataTable GetBanWordList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `find`, `replacement` FROM `" + BaseConfigs.get_GetTablePrefix() + "words`").Tables[0];
		}
		public DataTable GetMedalsList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `medalid`, `name`, `image`,`available`  FROM `" + BaseConfigs.get_GetTablePrefix() + "medals` ORDER BY `medalid`").Tables[0];
		}
		public DataTable GetMagicList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "magic` ORDER BY `id`").Tables[0];
		}
		public DataTable GetTopicTypeList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `typeid`,`name` FROM `" + BaseConfigs.get_GetTablePrefix() + "topictypes` ORDER BY `displayorder`").Tables[0];
		}
		public int AddCreditsLog(int uid, int fromto, int sendcredits, int receivecredits, float send, float receive, string paydate, int operation)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?fromto", DbType.Boolean, 4, fromto), 
				DbHelper.MakeInParam("?sendcredits", DbType.Boolean, 4, sendcredits), 
				DbHelper.MakeInParam("?receivecredits", DbType.Boolean, 4, receivecredits), 
				DbHelper.MakeInParam("?send", DbType.AnsiString, 4, send), 
				DbHelper.MakeInParam("?receive", DbType.AnsiString, 4, receive), 
				DbHelper.MakeInParam("?paydate", (DbType)253, 0, paydate), 
				DbHelper.MakeInParam("?operation", DbType.Boolean, 4, operation)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "creditslog` (`uid`,`fromto`,`sendcredits`,`receivecredits`,`send`,`receive`,`paydate`,`operation`) VALUES(?uid,?fromto,?sendcredits,?receivecredits,?send,?receive,?paydate,?operation)", array);
		}
		public DataTable GetCreditsLogList(int pagesize, int currentpage, int uid)
		{
			int num = (currentpage - 1) * pagesize;
			string text = string.Format(string.Concat(new object[]
			{
				"SELECT `c`.*, `ufrom`.`username` AS `fromuser`, `uto`.`username` AS `touser` FROM `{0}creditslog` AS `c`, `{0}users` AS `ufrom`, `{0}users` AS `uto` WHERE `c`.`uid`=`ufrom`.`uid` AND `c`.`fromto`=`uto`.`uid` AND (`c`.`uid`={1} OR `c`.`fromto` = {1})  ORDER BY `id` DESC LIMIT ", 
				num, 
				",", 
				pagesize
			}), BaseConfigs.get_GetTablePrefix(), uid);
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetCreditsLogRecordCount(int uid)
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new object[]
			{
				"SELECT COUNT(1) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"creditslog` WHERE `uid`=", 
				uid
			})), 0);
		}
		public string GetTableStruct()
		{
			return "";
		}
		public void ShrinkDataBase(string shrinksize, string dbname)
		{
		}
		public void ClearDBLog(string dbname)
		{
		}
		public string RunSql(string sql)
		{
			string text = string.Empty;
			if (sql != "")
			{
				MySqlConnection mySqlConnection = new MySqlConnection(DbHelper.get_ConnectionString());
				mySqlConnection.Open();
				string[] array = Utils.SplitString(sql, "--/* Discuz!NT SQL Separator */--");
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
					{
						try
						{
							DbHelper.ExecuteNonQuery(CommandType.Text, text2);
							mySqlTransaction.Commit();
						}
						catch (Exception ex)
						{
							mySqlTransaction.Rollback();
							string text3 = ex.Message.Replace("'", " ");
							text3 = text3.Replace("\\", "/");
							text3 = text3.Replace("\r\n", "\\r\\n");
							text3 = text3.Replace("\r", "\\r");
							text3 = text3.Replace("\n", "\\n");
							text = text + text3 + "<br>";
						}
					}
				}
				mySqlConnection.Close();
			}
			return text;
		}
		public string GetDbName()
		{
			string getDBConnectString = BaseConfigs.get_GetDBConnectString();
			string[] array = getDBConnectString.Split(new char[]
			{
				';'
			});
			string result;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (text.ToLower().IndexOf("initial catalog") >= 0 || text.ToLower().IndexOf("database") >= 0)
				{
					result = text.Split(new char[]
					{
						'='
					})[1].Trim();
					return result;
				}
			}
			result = "dnt";
			return result;
		}
		public bool CreateORFillIndex(string DbName, string postsid)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = BaseConfigs.get_GetTablePrefix() + "posts" + postsid;
			bool result;
			try
			{
				stringBuilder.Remove(0, stringBuilder.Length);
				DbHelper.ExecuteNonQuery("SELECT `pid` FROM `" + text + "` where contains(`message`,'asd') ORDER BY `pid` ASC LIMIT 1");
				stringBuilder.Append("execute sp_fulltext_catalog 'pk_" + text + "_msg','start_full'; \r\n");
				stringBuilder.Append("While fulltextcatalogproperty('pk_" + text + "_msg','populateStatus')<>0 \r\n");
				stringBuilder.Append("begin \r\n");
				stringBuilder.Append("waitfor delay '0:5:30' \r\n");
				stringBuilder.Append("end \r\n");
				DbHelper.ExecuteNonQuery(stringBuilder.ToString());
				result = true;
			}
			catch
			{
				try
				{
					stringBuilder.Remove(0, stringBuilder.Length);
					stringBuilder.Append("if(select databaseproperty('[" + DbName + "]','isfulltextenabled'))=0  execute sp_fulltext_database 'enable';");
					try
					{
						stringBuilder.Append("execute sp_fulltext_table '[" + text + "]', 'drop' ;");
						stringBuilder.Append("execute sp_fulltext_catalog 'pk_" + text + "_msg','drop';");
						DbHelper.ExecuteNonQuery(stringBuilder.ToString());
					}
					catch
					{
					}
					finally
					{
						stringBuilder.Remove(0, stringBuilder.Length);
						stringBuilder.Append("execute sp_fulltext_catalog 'pk_" + text + "_msg','create';");
						stringBuilder.Append(string.Concat(new string[]
						{
							"execute sp_fulltext_table '[", 
							text, 
							"]','create','pk_", 
							text, 
							"_msg','pk_", 
							text, 
							"';"
						}));
						stringBuilder.Append("execute sp_fulltext_column '[" + text + "]','message','add';");
						stringBuilder.Append("execute sp_fulltext_table '[" + text + "]','activate';");
						stringBuilder.Append("execute sp_fulltext_catalog 'pk_" + text + "_msg','start_full';");
						DbHelper.ExecuteNonQuery(stringBuilder.ToString());
					}
					result = true;
				}
				catch (MySqlException ex)
				{
					string text2 = ex.Message.Replace("'", " ");
					text2 = text2.Replace("\\", "/");
					text2 = text2.Replace("\r\n", "\\r\\n");
					text2 = text2.Replace("\r", "\\r");
					text2 = text2.Replace("\n", "\\n");
					result = true;
				}
			}
			return result;
		}
		public string GetSpecialTableFullIndexSQL(string tablename)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Concat(new string[]
			{
				"if exists (select * from sysobjects where id = object_id(N'[", 
				tablename, 
				"]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)  drop table [", 
				tablename, 
				"];"
			}));
			stringBuilder.Append("CREATE TABLE [" + tablename + "] (`pid` `int` NOT NULL ,`fid` `int` NOT NULL ,`tid` `int` NOT NULL ,`parentid` `int` NOT NULL ,`layer` `int` NOT NULL ,`poster` `nvarchar` (20) NOT NULL ,`posterid` `int` NOT NULL ,`title` `nvarchar` (80) NOT NULL ,`postdatetime` `smalldatetime` NOT NULL ,`message` `ntext` NOT NULL ,`ip` `nvarchar` (15) NOT NULL ,`lastedit` `nvarchar` (50) NOT NULL ,`invisible` `int` NOT NULL ,`usesig` `int` NOT NULL ,`htmlon` `int` NOT NULL ,`smileyoff` `int` NOT NULL ,`parseurloff` `int` NOT NULL ,`bbcodeoff` `int` NOT NULL ,`attachment` `int` NOT NULL ,`rate` `int` NOT NULL ,`ratetimes` `int` NOT NULL ) ON `PRIMARY` TEXTIMAGE_ON `PRIMARY`");
			stringBuilder.Append(";");
			stringBuilder.Append(string.Concat(new string[]
			{
				"ALTER TABLE [", 
				tablename, 
				"] WITH NOCHECK ADD CONSTRAINT [PK_", 
				tablename, 
				"] PRIMARY KEY  CLUSTERED (`pid`)  ON `PRIMARY`"
			}));
			stringBuilder.Append(";");
			stringBuilder.Append("ALTER TABLE [" + tablename + "] ADD ");
			stringBuilder.Append("CONSTRAINT [DF_" + tablename + "_pid] DEFAULT (0) FOR `pid`,");
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_parentid] DEFAULT (0) FOR `parentid`,CONSTRAINT [DF_", 
				tablename, 
				"_layer] DEFAULT (0) FOR `layer`,"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_poster] DEFAULT ('') FOR `poster`,CONSTRAINT [DF_", 
				tablename, 
				"_posterid] DEFAULT (0) FOR `posterid`,"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_postdatetime] DEFAULT (getdate()) FOR `postdatetime`,CONSTRAINT [DF_", 
				tablename, 
				"_message] DEFAULT ('') FOR `message`,"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_ip] DEFAULT ('') FOR `ip`,CONSTRAINT [DF_", 
				tablename, 
				"_lastedit] DEFAULT ('') FOR `lastedit`,"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_invisible] DEFAULT (0) FOR `invisible`,CONSTRAINT [DF_", 
				tablename, 
				"_usesig] DEFAULT (0) FOR `usesig`,"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_htmlon] DEFAULT (0) FOR `htmlon`,CONSTRAINT [DF_", 
				tablename, 
				"_smileyoff] DEFAULT (0) FOR `smileyoff`,"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_parseurloff] DEFAULT (0) FOR `parseurloff`,CONSTRAINT [DF_", 
				tablename, 
				"_bbcodeoff] DEFAULT (0) FOR `bbcodeoff`,"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"CONSTRAINT [DF_", 
				tablename, 
				"_attachment] DEFAULT (0) FOR `attachment`,CONSTRAINT [DF_", 
				tablename, 
				"_rate] DEFAULT (0) FOR `rate`,"
			}));
			stringBuilder.Append("CONSTRAINT [DF_" + tablename + "_ratetimes] DEFAULT (0) FOR `ratetimes`");
			stringBuilder.Append(";");
			stringBuilder.Append("CREATE  INDEX `parentid` ON [" + tablename + "](`parentid`) ON `PRIMARY`");
			stringBuilder.Append(";");
			stringBuilder.Append("CREATE  UNIQUE  INDEX `showtopic` ON [" + tablename + "](`tid`, `invisible`, `pid`) ON `PRIMARY`");
			stringBuilder.Append(";");
			stringBuilder.Append("CREATE  INDEX `treelist` ON [" + tablename + "](`tid`, `invisible`, `parentid`) ON `PRIMARY`");
			stringBuilder.Append(";");
			stringBuilder.Append("USE " + this.GetDbName() + " \r\n");
			stringBuilder.Append("execute sp_fulltext_database 'enable'; \r\n");
			stringBuilder.Append("if(select databaseproperty('[" + this.GetDbName() + "]','isfulltextenabled'))=0  execute sp_fulltext_database 'enable';");
			stringBuilder.Append(string.Concat(new string[]
			{
				"if exists (select * from sysfulltextcatalogs where name ='pk_", 
				tablename, 
				"_msg')  execute sp_fulltext_catalog 'pk_", 
				tablename, 
				"_msg','drop';"
			}));
			stringBuilder.Append(string.Concat(new string[]
			{
				"if exists (select * from sysfulltextcatalogs where name ='pk_", 
				tablename, 
				"_msg')  execute sp_fulltext_table '[", 
				tablename, 
				"]', 'drop' ;"
			}));
			stringBuilder.Append("execute sp_fulltext_catalog 'pk_" + tablename + "_msg','create';");
			stringBuilder.Append(string.Concat(new string[]
			{
				"execute sp_fulltext_table '[", 
				tablename, 
				"]','create','pk_", 
				tablename, 
				"_msg','pk_", 
				tablename, 
				"';"
			}));
			stringBuilder.Append("execute sp_fulltext_column '[" + tablename + "]','message','add';");
			stringBuilder.Append("execute sp_fulltext_table '[" + tablename + "]','activate';");
			stringBuilder.Append("execute sp_fulltext_catalog 'pk_" + tablename + "_msg','start_full';");
			return stringBuilder.ToString();
		}
		public IDataReader GetCustomEditButtonList()
		{
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "bbcodes` WHERE `available` = 1");
		}
		public DataTable GetCustomEditButtonListWithTable()
		{
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "bbcodes` WHERE `available` = 1");
			DataTable result;
			if (dataSet != null)
			{
				if (dataSet.Tables.Count > 0)
				{
					result = dataSet.Tables[0];
					return result;
				}
			}
			dataSet.Dispose();
			result = null;
			return result;
		}
		public DataRowCollection GetTableListIds()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `id` FROM `" + BaseConfigs.get_GetTablePrefix() + "tablelist`").Tables[0].Rows;
		}
		public void UpdateAnnouncementPoster(int posterid, string poster)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?poster", (DbType)253, 20, poster), 
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, posterid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "announcements` SET `poster`=?poster  WHERE `posterid`=?posterid", array);
		}
		public bool HasStatisticsByLastUserId(int lastuserid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?lastuserid", DbType.Boolean, 4, lastuserid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `lastuserid` FROM  `" + BaseConfigs.get_GetTablePrefix() + "statistics`  WHERE `lastuserid`=?lastuserid LIMIT 1", new DbParameter[]
			{
				dbParameter
			}).Tables[0].Rows.Count > 0;
		}
		public void UpdateStatisticsLastUserName(int lastuserid, string lastusername)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastuserid", DbType.Boolean, 4, lastuserid), 
				DbHelper.MakeInParam("?lastusername", (DbType)253, 20, lastusername)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET `lastusername`=?lastusername WHERE `lastuserid`=?lastuserid", array);
		}
		public void AddVisitLog(int uid, string username, int groupid, string grouptitle, string ip, string actions, string others)
		{
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "adminvisitlog` (`uid`,`username`,`groupid`,`grouptitle`,`ip`,`actions`,`others`,`postdatetime`) VALUES (?uid,?username,?groupid,?grouptitle,?ip,?actions,?others,NOW())";
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?username", (DbType)253, 50, username), 
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, groupid), 
				DbHelper.MakeInParam("?grouptitle", (DbType)253, 50, grouptitle), 
				DbHelper.MakeInParam("?ip", (DbType)253, 15, ip), 
				DbHelper.MakeInParam("?actions", (DbType)253, 100, actions), 
				DbHelper.MakeInParam("?others", (DbType)253, 200, others)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteVisitLogs()
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "adminvisitlog` ");
		}
		public void DeleteVisitLogs(string condition)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "adminvisitlog` WHERE " + condition);
		}
		public DataTable GetVisitLogList(int pagesize, int currentpage)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"adminvisitlog` Order by `visitid` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			})).Tables[0];
		}
		public DataTable GetVisitLogList(int pagesize, int currentpage, string condition)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"adminvisitlog` WHERE ", 
				condition, 
				" ORDER BY `visitid` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				", ", 
				pagesize.ToString()
			})).Tables[0];
		}
		public int GetVisitLogCount()
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(visitid) FROM `" + BaseConfigs.get_GetTablePrefix() + "adminvisitlog`").Tables[0].Rows[0][0].ToString());
		}
		public int GetVisitLogCount(string condition)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(visitid) FROM `" + BaseConfigs.get_GetTablePrefix() + "adminvisitlog` WHERE " + condition).Tables[0].Rows[0][0].ToString());
		}
		public void UpdateForumAndUserTemplateId(string templateidlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` SET `templateid`=1 WHERE `templateid` IN(", 
				templateidlist, 
				")"
			}));
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `templateid`=1 WHERE `templateid` IN(", 
				templateidlist, 
				")"
			}));
		}
		public void AddTemplate(string name, string directory, string copyright, string author, string createdate, string ver, string fordntver)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?name", (DbType)253, 50, name), 
				DbHelper.MakeInParam("?directory", (DbType)253, 100, directory), 
				DbHelper.MakeInParam("?copyright", (DbType)253, 100, copyright), 
				DbHelper.MakeInParam("?author", (DbType)253, 100, author), 
				DbHelper.MakeInParam("?createdate", (DbType)253, 50, createdate), 
				DbHelper.MakeInParam("?ver", (DbType)253, 100, ver), 
				DbHelper.MakeInParam("?fordntver", (DbType)253, 100, fordntver)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "templates` (`name`,`directory`,`copyright`,`author`,`createdate`,`ver`,`fordntver`) VALUES(?name,?directory,?copyright,?author,?createdate,?ver,?fordntver)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int AddTemplate(string templateName, string directory, string copyright)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?templatename", (DbType)253, 0, templateName), 
				DbHelper.MakeInParam("?directory", (DbType)253, 0, directory), 
				DbHelper.MakeInParam("?copyright", (DbType)253, 0, copyright)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "templates`(`templatename`,`directory`,`copyright`) VALUES(?templatename, ?directory, ?copyright)", array);
			return int.Parse(DbHelper.ExecuteScalar(CommandType.Text, "SELECT `templateid` FROM FROM `" + BaseConfigs.get_GetTablePrefix() + "adminvisitlog` ORDER BY `templateid` DESC LIMIT 1").ToString());
		}
		public void DeleteTemplateItem(int templateid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?templateid", DbType.Boolean, 4, templateid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "templates` WHERE `templateid`=?templateid");
		}
		public void DeleteTemplateItem(string templateidlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"templates` WHERE `templateid` IN (", 
				templateidlist, 
				")"
			}));
		}
		public DataTable GetAllTemplateList(string templatePath)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "templates` ORDER BY `templateid`").Tables[0];
		}
		public int GetMaxTemplateId()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT IF(ISNULL(MAX(templateid)),0,MAX(templateid)) FROM " + BaseConfigs.get_GetTablePrefix() + "templates"), 0) + 1;
		}
		public bool InsertModeratorLog(string moderatoruid, string moderatorname, string groupid, string grouptitle, string ip, string postdatetime, string fid, string fname, string tid, string title, string actions, string reason)
		{
			bool result;
			try
			{
				string text = string.Format("INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "moderatormanagelog` (`moderatoruid`,`moderatorname`,`groupid`,`grouptitle`,`ip`,`postdatetime`,`fid`,`fname`,`tid`,`title`,`actions`,`reason`) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')", new object[]
				{
					moderatoruid, 
					moderatorname, 
					groupid, 
					grouptitle, 
					ip, 
					postdatetime, 
					fid, 
					fname, 
					tid, 
					title, 
					actions, 
					reason
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public bool DeleteModeratorLog(string condition)
		{
			bool result;
			try
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "moderatormanagelog` WHERE " + condition);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public DataTable GetModeratorLogList(int pagesize, int currentpage, string condition)
		{
			if (condition != "")
			{
				condition = " WHERE " + condition;
			}
			string text = string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"moderatormanagelog`  ", 
				condition, 
				"  Order by `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetModeratorLogListCount()
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "moderatormanagelog`").Tables[0].Rows[0][0].ToString());
		}
		public int GetModeratorLogListCount(string condition)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "moderatormanagelog` WHERE " + condition).Tables[0].Rows[0][0].ToString());
		}
		public bool DeleteMedalLog()
		{
			bool result;
			try
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "medalslog` ");
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public bool DeleteMedalLog(string condition)
		{
			bool result;
			try
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE  FROM `" + BaseConfigs.get_GetTablePrefix() + "medalslog` WHERE " + condition);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public DataTable GetMedalLogList(int pagesize, int currentpage)
		{
			string text = string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"medalslog` ORDER BY `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetMedalLogList(int pagesize, int currentpage, string condition)
		{
			string text = string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"medalslog` WHERE ", 
				condition, 
				"  Order by `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetMedalLogListCount()
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "medalslog`").Tables[0].Rows[0][0].ToString());
		}
		public int GetMedalLogListCount(string condition)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "medalslog` WHERE " + condition).Tables[0].Rows[0][0].ToString());
		}
		public DataTable GetErrLoginRecordByIP(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)254, 15, ip)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `errcount`, `lastupdate` FROM `" + BaseConfigs.get_GetTablePrefix() + "failedlogins` WHERE `ip`=?ip LIMIT 1", array).Tables[0];
		}
		public int AddErrLoginCount(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)254, 15, ip)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "failedlogins` SET `errcount`=`errcount`+1, `lastupdate`=NOW() WHERE `ip`=?ip", array);
		}
		public int AddErrLoginRecord(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)254, 15, ip)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "failedlogins` (`ip`, `errcount`, `lastupdate`) VALUES(?ip, 1, NOW())", array);
		}
		public int ResetErrLoginCount(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)254, 15, ip)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "failedlogins` SET `errcount`=1, `lastupdate`=now() WHERE `ip`=?ip", array);
		}
		public int DeleteErrLoginRecord(string ip)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?ip", (DbType)254, 15, ip)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "failedlogins` WHERE `ip`=?ip OR (SUBSTRING_INDEX(SEC_TO_TIME(unix_timestamp(`lastupdate`)-unix_timestamp(NOW())),':',1))*60 > 15", array);
		}
		public int GetPostCount(string posttablename)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT COUNT(pid) FROM `" + posttablename + "`").Tables[0].Rows[0][0].ToString());
		}
		public DataTable GetPostTableList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "tablelist`").Tables[0];
		}
		public int UpdateDetachTable(int fid, string description)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?description", (DbType)253, 50, description), 
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"tablelist` SET `description`='", 
				description, 
				"'  Where `id`=", 
				fid
			});
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int StartFullIndex(string dbname)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("USE " + dbname + ";");
			stringBuilder.Append("execute sp_fulltext_database 'enable';");
			return DbHelper.ExecuteNonQuery(CommandType.Text, stringBuilder.ToString());
		}
		public void CreatePostTableAndIndex(string tablename)
		{
			string specialTableFullIndexSQL = this.GetSpecialTableFullIndexSQL(tablename, "");
			DbHelper.ExecuteNonQuery(CommandType.Text, specialTableFullIndexSQL);
			DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE  INDEX `parentid` ON `" + tablename + "`(`parentid`)");
			DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE  UNIQUE  INDEX `showtopic` ON `" + tablename + "`(`tid`, `invisible`, `pid`)");
			DbHelper.ExecuteNonQuery(CommandType.Text, "CREATE  INDEX `treelist` ON `" + tablename + "`(`tid`, `invisible`, `parentid`)");
		}
		public string GetSpecialTableFullIndexSQL(string tablename, string dbname)
		{
			return "CREATE TABLE `" + tablename + "` (`pid` INT( 4 ) NOT NULL DEFAULT '0',`fid` INT( 4 ) NOT NULL DEFAULT '0',`tid` INT( 4 ) NOT NULL DEFAULT '0',`parentid` INT( 4 ) NOT NULL DEFAULT '0',`layer` INT( 4 ) NOT NULL DEFAULT '0',`poster` VARCHAR( 20 ) NULL ,`posterid` INT( 4 ) NOT NULL DEFAULT '0',`title` VARCHAR( 60 ) NOT NULL ,`postdatetime` DATETIME NOT NULL ,`message` VARCHAR( 16 ) NULL ,`ip` VARCHAR( 15 ) NULL ,`lastedit` VARCHAR( 50 ) NULL ,`invisible` INT( 4 ) NOT NULL DEFAULT '0',`usesig` INT( 4 ) NOT NULL DEFAULT '0',`htmlon` INT( 4 ) NOT NULL DEFAULT '0',`smileyoff` INT( 4 ) NOT NULL DEFAULT '0',`parseurloff` INT( 4 ) NOT NULL DEFAULT '0',`bbcodeoff` INT( 4 ) NOT NULL DEFAULT '0',`attachment` INT( 4 ) NOT NULL DEFAULT '0',`rate` INT( 4 ) NOT NULL DEFAULT '0',`ratetimes` INT( 4 ) NOT NULL DEFAULT '0',PRIMARY KEY ( `pid` ) ) ENGINE = MYISAM CHARACTER SET gbk COLLATE gbk_chinese_ci";
		}
		public void AddPostTableToTableList(string description, int mintid, int maxtid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?description", (DbType)253, 50, description), 
				DbHelper.MakeInParam("?mintid", DbType.Boolean, 4, mintid), 
				DbHelper.MakeInParam("?maxtid", DbType.Boolean, 4, maxtid)
			};
			string text = string.Concat(new object[]
			{
				"INSERT INTO `", 
				BaseConfigs.get_GetTablePrefix(), 
				"tablelist`(`createdatetime`,`description`,`mintid`,`maxtid`) VALUES(NOW(),'", 
				description, 
				"', ", 
				mintid, 
				", ", 
				maxtid, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void CreatePostProcedure(string sqltemplate)
		{
			string[] array = sqltemplate.Split(new char[]
			{
				'~'
			});
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				DbHelper.ExecuteNonQuery(CommandType.Text, text);
			}
		}
		public DataTable GetMaxTid()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT MAX(tid) FROM `" + BaseConfigs.get_GetTablePrefix() + "topics`").Tables[0];
		}
		public DataTable GetPostCountFromIndex(string postsid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `rows` FROM `sysindexes` WHERE `name`='PK_", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				postsid, 
				"' LIMIT 1"
			})).Tables[0];
		}
		public DataTable GetPostCountTable(string postsid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT Count(pid) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				postsid, 
				"`"
			})).Tables[0];
		}
		public void TestFullTextIndex(int posttableid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"SELECT `pid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` where contains(`message`,'asd') ORDER BY `pid` ASC LIMIT "
			}));
		}
		public DataRowCollection GetRateRange(int scoreid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT `groupid`, `raterange` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` WHERE `raterange` LIKE '%", 
				scoreid, 
				",True,%'"
			})).Tables[0].Rows;
		}
		public void UpdateRateRange(string raterange, int groupid)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` SET `raterange`='", 
				raterange, 
				"' WHERE `groupid`=", 
				groupid
			}));
		}
		public int GetMaxTableListId()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT IFNULL(MAX(id),0) FROM " + BaseConfigs.get_GetTablePrefix() + "tablelist"), 0);
		}
		public int GetMaxPostTableTid(string posttablename)
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT IFNULL(MAX(tid),0) FROM " + posttablename), 0) + 1;
		}
		public int GetMinPostTableTid(string posttablename)
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT IFNULL(min(tid),0) FROM " + posttablename), 0) + 1;
		}
		public void AddAdInfo(int available, string type, int displayorder, string title, string targets, string parameters, string code, string starttime, string endtime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?available", DbType.Boolean, 4, available), 
				DbHelper.MakeInParam("?type", (DbType)253, 50, type), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?title", (DbType)253, 50, title), 
				DbHelper.MakeInParam("?targets", (DbType)253, 255, targets), 
				DbHelper.MakeInParam("?parameters", (DbType)253, 0, parameters), 
				DbHelper.MakeInParam("?code", (DbType)253, 0, code), 
				DbHelper.MakeInParam("?starttime", DbType.Int64, 8, starttime), 
				DbHelper.MakeInParam("?endtime", DbType.Int64, 8, endtime)
			};
			string text = "INSERT INTO  `" + BaseConfigs.get_GetTablePrefix() + "advertisements` (`available`,`type`,`displayorder`,`title`,`targets`,`parameters`,`code`,`starttime`,`endtime`) VALUES(?available,?type,?displayorder,?title,?targets,?parameters,?code,?starttime,?endtime)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string GetAdvertisements()
		{
			return "Select * From `" + BaseConfigs.get_GetTablePrefix() + "advertisements` Order BY `advid` ASC";
		}
		public DataRowCollection GetTargetsForumName(string targets)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `name` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `fid` IN(0", 
				targets, 
				"0)"
			})).Tables[0].Rows;
		}
		public int UpdateAdvertisementAvailable(string aidlist, int available)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?available", DbType.Boolean, 4, available)
			};
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"advertisements` SET `available`=?available  WHERE `advid` IN(", 
				aidlist, 
				")"
			});
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int UpdateAdvertisement(int aid, int available, string type, int displayorder, string title, string targets, string parameters, string code, string starttime, string endtime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?aid", DbType.Boolean, 4, aid), 
				DbHelper.MakeInParam("?available", DbType.Boolean, 4, available), 
				DbHelper.MakeInParam("?type", (DbType)253, 50, type), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?title", (DbType)253, 50, title), 
				DbHelper.MakeInParam("?targets", (DbType)253, 255, targets), 
				DbHelper.MakeInParam("?parameters", (DbType)253, 16, parameters), 
				DbHelper.MakeInParam("?code", (DbType)253, 16, code), 
				DbHelper.MakeInParam("?starttime", DbType.Int64, 8, starttime), 
				DbHelper.MakeInParam("?endtime", DbType.Int64, 8, endtime)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "advertisements` SET `available`=?available,type=?type, displayorder=?displayorder,title=?title,targets=?targets,`parameters`=?parameters,code=?code,starttime=?starttime,endtime=?endtime WHERE `advid`=?aid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteAdvertisement(string aid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?aid", DbType.Boolean, 4, aid);
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "advertisements` WHERE `advid`=?aid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public void BuyTopic(int uid, int tid, int posterid, int price, float netamount, int creditsTrans)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?authorid", DbType.Boolean, 4, posterid), 
				DbHelper.MakeInParam("?buydate", DbType.Int64, 4, DateTime.Now), 
				DbHelper.MakeInParam("?amount", DbType.Boolean, 4, price), 
				DbHelper.MakeInParam("?netamount", DbType.AnsiString, 8, netamount)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET extcredits", 
				creditsTrans, 
				" = extcredits", 
				creditsTrans, 
				" - ", 
				price.ToString(), 
				" WHERE `uid` = ?uid"
			}), array);
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET extcredits", 
				creditsTrans, 
				" = extcredits", 
				creditsTrans, 
				" + ?netamount WHERE `uid` = ?authorid"
			}), array);
		}
		public int AddPaymentLog(int uid, int tid, int posterid, int price, float netamount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?authorid", DbType.Boolean, 4, posterid), 
				DbHelper.MakeInParam("?buydate", DbType.Int64, 4, DateTime.Now), 
				DbHelper.MakeInParam("?amount", DbType.Boolean, 4, price), 
				DbHelper.MakeInParam("?netamount", DbType.Currency, 8, netamount)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "paymentlog` (`uid`,`tid`,`authorid`,`buydate`,`amount`,`netamount`) VALUES(?uid,?tid,?authorid,?buydate,?amount,?netamount)", array);
		}
		public bool IsBuyer(int tid, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT `id` FROM `" + BaseConfigs.get_GetTablePrefix() + "paymentlog` WHERE `tid` = ?tid AND `uid`=?uid", array), 0) > 0;
		}
		public DataTable GetPayLogInList(int pagesize, int currentpage, int uid)
		{
			string text = string.Concat(new object[]
			{
				"SELECT ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.*, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.fid AS fid ,", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.postdatetime AS postdatetime ,", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.poster AS authorname, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.title AS title,", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.username As UserName FROM (", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog LEFT JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.tid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.tid) LEFT JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.uid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.uid WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog`.`authorid`=", 
				uid, 
				"  ORDER BY `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetPaymentLogInRecordCount(int uid)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT count(id) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog` WHERE authorid=", 
				uid
			})).Tables[0].Rows[0][0].ToString());
		}
		public DataTable GetPayLogOutList(int pagesize, int currentpage, int uid)
		{
			string text = string.Concat(new object[]
			{
				"SELECT ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.*, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.fid AS fid ,", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.postdatetime AS postdatetime ,", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.poster AS authorname, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.title AS title,", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.username As UserName FROM (", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog LEFT JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.tid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.tid) LEFT JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.uid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.uid WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog`.`uid`=", 
				uid, 
				"  ORDER BY `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetPaymentLogOutRecordCount(int uid)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT count(id) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog` WHERE uid=", 
				uid
			})).Tables[0].Rows[0][0].ToString());
		}
		public DataTable GetPaymentLogByTid(int pagesize, int currentpage, int tid)
		{
			string text = string.Concat(new object[]
			{
				"SELECT ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.*, ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.username As username FROM ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog LEFT OUTER JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.tid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics.tid LEFT OUTER JOIN ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users ON ", 
				BaseConfigs.get_GetTablePrefix(), 
				"users.uid = ", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog.uid WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog`.`tid`=", 
				tid, 
				"  ORDER BY `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetPaymentLogByTidCount(int tid)
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new object[]
			{
				"SELECT count(id) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"paymentlog` WHERE tid=", 
				tid
			})), 0);
		}
		public void AddSmiles(int id, int displayorder, int type, string code, string url)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?type", DbType.Boolean, 4, type), 
				DbHelper.MakeInParam("?code", (DbType)253, 30, code), 
				DbHelper.MakeInParam("?url", (DbType)253, 60, url)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "smilies` (id,displayorder,type,code,url) Values (?id,?displayorder,?type,?code,?url)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string GetIcons()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE TYPE=1";
		}
		public string DeleteSmily(int id)
		{
			return string.Concat(new object[]
			{
				"DELETE  FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"smilies` WHERE `id`=", 
				id
			});
		}
		public int UpdateSmilies(int id, int displayorder, int type, string code, string url)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?type", DbType.Boolean, 4, type), 
				DbHelper.MakeInParam("?code", (DbType)253, 30, code), 
				DbHelper.MakeInParam("?url", (DbType)253, 60, url)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "smilies` SET `displayorder`=?displayorder,`type`=?type,`code`=?code,`url`=?url Where `id`=?id";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int UpdateSmiliesPart(string code, int displayorder, int id)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, displayorder), 
				DbHelper.MakeInParam("?code", (DbType)253, 30, code)
			};
			string text = "Update `" + BaseConfigs.get_GetTablePrefix() + "smilies` Set code=?code,displayorder=?displayorder Where id=?id";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int DeleteSmilies(string idlist)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"smilies`  WHERE `ID` IN(", 
				idlist, 
				")"
			}));
		}
		public string GetSmilies()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE `type`=0";
		}
		public int GetMaxSmiliesId()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT IFNULL(MAX(`id`),0) FROM " + BaseConfigs.get_GetTablePrefix() + "smilies"), 0) + 1;
		}
		public DataTable GetSmiliesInfoByType(int type)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?type", DbType.Boolean, 4, type);
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE TYPE=?type";
			return DbHelper.ExecuteDataset(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public IDataReader GetSmiliesList()
		{
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE `type`=0 ORDER BY `displayorder` DESC,`id` ASC");
		}
		public DataTable GetSmiliesListDataTable()
		{
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` ORDER BY `type`,`displayorder`,`id`");
			DataTable result;
			if (dataSet != null)
			{
				if (dataSet.Tables.Count > 0)
				{
					result = dataSet.Tables[0];
					return result;
				}
			}
			result = new DataTable();
			return result;
		}
		public DataTable GetSmiliesListWithoutType()
		{
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE `type`<>0 ORDER BY `type`,`displayorder`,`id`");
			DataTable result;
			if (dataSet != null)
			{
				if (dataSet.Tables.Count > 0)
				{
					result = dataSet.Tables[0];
					return result;
				}
			}
			result = new DataTable();
			return result;
		}
		public DataTable GetSmilieTypes()
		{
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE `type`=0 ORDER BY `displayorder`,`id`");
			DataTable result;
			if (dataSet != null && dataSet.Tables.Count > 0)
			{
				result = dataSet.Tables[0];
			}
			else
			{
				result = new DataTable();
			}
			return result;
		}
		public DataRow GetSmilieTypeById(string id)
		{
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE id=" + id);
			DataRow result;
			if (dataSet != null && dataSet.Tables[0].Rows.Count == 1)
			{
				result = dataSet.Tables[0].Rows[0];
			}
			else
			{
				result = null;
			}
			return result;
		}
		public DataRow GetStatisticsRow()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "statistics` LIMIT 1").Tables[0].Rows[0];
		}
		public void SaveStatisticsRow(DataRow dr)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?totaltopic", DbType.Boolean, 4, int.Parse(dr["totaltopic"].ToString())), 
				DbHelper.MakeInParam("?totalpost", DbType.Boolean, 4, int.Parse(dr["totalpost"].ToString())), 
				DbHelper.MakeInParam("?totalusers", DbType.Boolean, 4, int.Parse(dr["totalusers"].ToString())), 
				DbHelper.MakeInParam("?lastusername", (DbType)253, 20, dr["totalusers"].ToString()), 
				DbHelper.MakeInParam("?lastuserid", DbType.Boolean, 4, int.Parse(dr["highestonlineusercount"].ToString())), 
				DbHelper.MakeInParam("?highestonlineusercount", DbType.Boolean, 4, int.Parse(dr["highestonlineusercount"].ToString())), 
				DbHelper.MakeInParam("?highestonlineusertime", DbType.Int64, 4, DateTime.Parse(dr["highestonlineusertime"].ToString()))
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET `totaltopic`=?totaltopic,`totalpost`=?totalpost, `totalusers`=?totalusers, `lastusername`=?lastusername, `lastuserid`=?lastuserid, `highestonlineusercount`=??highestonlineusercount, `highestonlineusertime`=?highestonlineusertime", array);
		}
		public void UpdateYesterdayPosts(string posttableid)
		{
			int num = Convert.ToInt32(posttableid);
			int num2 = (num > 4) ? (num - 3) : 1;
			StringBuilder stringBuilder = new StringBuilder();
			for (int num3 = num; num3 >= num2; num3--)
			{
				if (num3 < num)
				{
					stringBuilder.Append(" UNION ");
				}
				StringBuilder arg_9E_0 = stringBuilder;
				string arg_9E_1 = "SELECT COUNT(1) AS `c` FROM `{0}posts{1}` WHERE `postdatetime` < '{2}' AND `postdatetime` > '{3}' AND `invisible`=0";
				object[] array = new object[4];
				array[0] = BaseConfigs.get_GetTablePrefix();
				array[1] = num3.ToString();
				object[] arg_72_0 = array;
				int arg_72_1 = 2;
				DateTime dateTime = DateTime.Now;
				arg_72_0[arg_72_1] = dateTime.ToString("yyyy-MM-dd");
				object[] arg_9B_0 = array;
				int arg_9B_1 = 3;
				dateTime = DateTime.Now;
				dateTime = dateTime.AddDays(-1.0);
				arg_9B_0[arg_9B_1] = dateTime.ToString("yyyy-MM-dd");
				arg_9E_0.AppendFormat(arg_9E_1, array);
			}
			string text = string.Format("SELECT SUM(`c`) FROM ({0})t", stringBuilder.ToString());
			int num4 = Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text), 0);
			int num5 = Utils.StrToInt(this.GetStatisticsRow()["highestposts"], 0);
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.AppendFormat("UPDATE `{0}statistics` SET `yesterdayposts`=", BaseConfigs.get_GetTablePrefix());
			stringBuilder.Append(num4.ToString());
			if (num4 > num5)
			{
				stringBuilder.Append(", `highestposts`=");
				stringBuilder.Append(num4.ToString());
				stringBuilder.Append(", `highestpostsdate`='");
				StringBuilder arg_17C_0 = stringBuilder;
				DateTime dateTime = DateTime.Now;
				dateTime = dateTime.AddDays(-1.0);
				arg_17C_0.Append(dateTime.ToString("yyyy-MM-dd"));
				stringBuilder.Append("'");
			}
			DbHelper.ExecuteNonQuery(stringBuilder.ToString());
		}
		public IDataReader GetAllForumStatistics()
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT SUM(`topics`) AS `topiccount`,SUM(`posts`) AS `postcount`,SUM(`todayposts`)-(SELECT SUM(`todayposts`) from `{0}forums` WHERE `lastpost` < CURDATE() AND `layer`=1) AS `todaypostcount` FROM `{0}forums` WHERE `layer`=1", BaseConfigs.get_GetTablePrefix()));
		}
		public IDataReader GetForumStatistics(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT SUM(`topics`) AS `topiccount`,SUM(`posts`) AS `postcount`,SUM(`todayposts`)-(SELECT SUM(`todayposts`) from `{0}forums` WHERE `lastpost` < CURDATE() AND `layer`=1 AND `fid` = ?fid) AS `todaypostcount` FROM `{0}forums` WHERE `fid` = ?fid AND `layer`=1", BaseConfigs.get_GetTablePrefix()), array);
		}
		public int UpdateStatistics(string param, int intValue)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!param.Equals(""))
			{
				stringBuilder.Append("UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET ");
				stringBuilder.Append(param);
				stringBuilder.Append(" = ");
				stringBuilder.Append(intValue);
			}
			return DbHelper.ExecuteNonQuery(CommandType.Text, stringBuilder.ToString());
		}
		public int UpdateStatistics(string param, string strValue)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!param.Equals(""))
			{
				stringBuilder.Append("UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET ");
				stringBuilder.Append(param);
				stringBuilder.Append(" = '");
				stringBuilder.Append(strValue);
				stringBuilder.Append("'");
			}
			return DbHelper.ExecuteNonQuery(CommandType.Text, stringBuilder.ToString());
		}
		public DataTable GetValidTemplateList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "templates` ORDER BY `templateid`").Tables[0];
		}
		public DataTable GetValidTemplateIDList()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `templateid` FROM `" + BaseConfigs.get_GetTablePrefix() + "templates` ORDER BY `templateid`").Tables[0];
		}
		public DataTable GetPost(string posttablename, int pid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT *  FROM `" + posttablename + "` WHERE pid=?pid LIMIT 1", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public DataTable GetMainPostByTid(string posttablename, int tid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + posttablename + "` WHERE `layer`=0  AND `tid`=?tid LIMIT 1", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public DataTable GetAttachmentsByPid(int pid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `aid`, `tid`, `pid`, `postdatetime`, `readperm`, `filename`, `description`, `filetype`, `filesize`, `attachment`, `downloads` FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `pid`=?pid").Tables[0];
		}
		public DataTable GetAdvertisement(int aid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?aid", DbType.Boolean, 4, aid);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "advertisements` WHERE `advid`=?aid", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		private string GetSearchTopicTitleSQL(int posterid, string searchforumid, int resultorder, int resultordertype, int digest, string keyword)
		{
			keyword = Regex.Replace(keyword, "--|;|'|\"", "", RegexOptions.Multiline | RegexOptions.Compiled);
			StringBuilder stringBuilder = new StringBuilder(keyword);
			stringBuilder.Replace("'", "\\'");
			stringBuilder.Replace("%", "\\%");
			stringBuilder.Replace("_", "\\_");
			stringBuilder.Replace("[", "\\[");
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendFormat("SELECT `tid` FROM `{0}topics` WHERE `displayorder`>=0", BaseConfigs.get_GetTablePrefix());
			if (posterid > 0)
			{
				stringBuilder2.Append(" AND `posterid`=");
				stringBuilder2.Append(posterid);
			}
			if (digest > 0)
			{
				stringBuilder2.Append(" AND `digest`>0 ");
			}
			if (searchforumid != string.Empty)
			{
				stringBuilder2.Append(" AND `fid` IN (");
				stringBuilder2.Append(searchforumid);
				stringBuilder2.Append(")");
			}
			string[] array = Utils.SplitString(stringBuilder.ToString(), " ");
			stringBuilder = new StringBuilder();
			if (keyword.Length > 0)
			{
				stringBuilder.Append(" AND (1=0 ");
				for (int i = 0; i < array.Length; i++)
				{
					stringBuilder.Append(" OR `title` ");
					stringBuilder.Append("LIKE '%");
					stringBuilder.Append(DataProvider.RegEsc(array[i]));
					stringBuilder.Append("%' ");
				}
				stringBuilder.Append(")");
			}
			stringBuilder2.Append(stringBuilder.ToString());
			stringBuilder2.Append(" ORDER BY ");
			switch (resultorder)
			{
				case 1:
				{
					stringBuilder2.Append("`tid`");
					break;
				}
				case 2:
				{
					stringBuilder2.Append("`replies`");
					break;
				}
				case 3:
				{
					stringBuilder2.Append("`views`");
					break;
				}
				default:
				{
					stringBuilder2.Append("`postdatetime`");
					break;
				}
			}
			if (resultordertype == 1)
			{
				stringBuilder2.Append(" ASC");
			}
			else
			{
				stringBuilder2.Append(" DESC");
			}
			return stringBuilder2.ToString();
		}
		private string GetSearchPostContentSQL(int posterid, string searchforumid, int resultorder, int resultordertype, int searchtime, int searchtimetype, int posttableid, StringBuilder strKeyWord)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string arg = "lastpost";
			switch (resultorder)
			{
				case 1:
				{
					arg = "tid";
					break;
				}
				case 2:
				{
					arg = "replies";
					break;
				}
				case 3:
				{
					arg = "views";
					break;
				}
				default:
				{
					arg = "lastpost";
					break;
				}
			}
			stringBuilder.AppendFormat("SELECT DISTINCT `{0}posts{1}`.`tid`,`{0}topics`.`{2}` FROM `{0}posts{1}` LEFT JOIN `{0}topics` ON `{0}topics`.`tid`=`{0}posts{1}`.`tid` WHERE `{0}topics`.`displayorder`>=0 AND ", BaseConfigs.get_GetTablePrefix(), posttableid, arg);
			if (searchforumid != string.Empty)
			{
				stringBuilder.AppendFormat("`{0}posts{1}`.`fid` IN (", BaseConfigs.get_GetTablePrefix(), posttableid);
				stringBuilder.Append(searchforumid);
				stringBuilder.Append(") AND ");
			}
			if (posterid != -1)
			{
				stringBuilder.AppendFormat("`{0}posts{1}`.`posterid`=", BaseConfigs.get_GetTablePrefix(), posttableid);
				stringBuilder.Append(posterid);
				stringBuilder.Append(" AND ");
			}
			if (searchtime != 0)
			{
				stringBuilder.AppendFormat("`{0}posts{1}`.`postdatetime`", BaseConfigs.get_GetTablePrefix(), posttableid);
				if (searchtimetype == 1)
				{
					stringBuilder.Append("<'");
				}
				else
				{
					stringBuilder.Append(">'");
				}
				StringBuilder arg_14A_0 = stringBuilder;
				DateTime dateTime = DateTime.Now;
				dateTime = dateTime.AddDays((double)searchtime);
				arg_14A_0.Append(dateTime.ToString("yyyy-MM-dd 00:00:00"));
				stringBuilder.Append("'AND ");
			}
			string[] array = Utils.SplitString(strKeyWord.ToString(), " ");
			strKeyWord = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				strKeyWord.Append(" OR ");
				if (GeneralConfigs.GetConfig().get_Fulltextsearch() == 1)
				{
					strKeyWord.AppendFormat("CONTAINS(message, '\"*", BaseConfigs.get_GetTablePrefix(), posttableid);
					strKeyWord.Append(array[i]);
					strKeyWord.Append("*\"') ");
				}
				else
				{
					strKeyWord.AppendFormat("`{0}posts{1}`.`message` LIKE '%", BaseConfigs.get_GetTablePrefix(), posttableid);
					strKeyWord.Append(DataProvider.RegEsc(array[i]));
					strKeyWord.Append("%' ");
				}
			}
			stringBuilder.Append(strKeyWord.ToString().Substring(3));
			stringBuilder.AppendFormat("ORDER BY `{0}topics`.", BaseConfigs.get_GetTablePrefix());
			switch (resultorder)
			{
				case 1:
				{
					stringBuilder.Append("`tid`");
					break;
				}
				case 2:
				{
					stringBuilder.Append("`replies`");
					break;
				}
				case 3:
				{
					stringBuilder.Append("`views`");
					break;
				}
				default:
				{
					stringBuilder.Append("`lastpost`");
					break;
				}
			}
			if (resultordertype == 1)
			{
				stringBuilder.Append(" ASC");
			}
			else
			{
				stringBuilder.Append(" DESC");
			}
			return stringBuilder.ToString();
		}
		private string GetSearchSpacePostTitleSQL(int posterid, int resultorder, int resultordertype, int searchtime, int searchtimetype, string keyword)
		{
			keyword = Regex.Replace(keyword, "--|;|'|\"", "", RegexOptions.Multiline | RegexOptions.Compiled);
			StringBuilder stringBuilder = new StringBuilder(keyword);
			stringBuilder.Replace("'", "''");
			stringBuilder.Replace("%", "[%]");
			stringBuilder.Replace("_", "[_]");
			stringBuilder.Replace("[", "[[]");
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendFormat("SELECT `postid` FROM `{0}spaceposts` WHERE `{0}spaceposts`.`poststatus`=1 ", BaseConfigs.get_GetTablePrefix());
			if (posterid > 0)
			{
				stringBuilder2.Append(" AND `uid`=");
				stringBuilder2.Append(posterid);
			}
			if (searchtime != 0)
			{
				stringBuilder2.AppendFormat(" AND `{0}spaceposts`.`postdatetime`", BaseConfigs.get_GetTablePrefix());
				if (searchtimetype == 1)
				{
					stringBuilder2.Append("<'");
				}
				else
				{
					stringBuilder2.Append(">'");
				}
				StringBuilder arg_105_0 = stringBuilder2;
				DateTime dateTime = DateTime.Now;
				dateTime = dateTime.AddDays((double)searchtime);
				arg_105_0.Append(dateTime.ToString("yyyy-MM-dd 00:00:00"));
				stringBuilder2.Append("' ");
			}
			string[] array = Utils.SplitString(stringBuilder.ToString(), " ");
			stringBuilder = new StringBuilder();
			if (keyword.Length > 0)
			{
				stringBuilder.Append(" AND (1=0 ");
				for (int i = 0; i < array.Length; i++)
				{
					stringBuilder.Append(" OR `title` ");
					stringBuilder.Append("LIKE '%");
					stringBuilder.Append(DataProvider.RegEsc(array[i]));
					stringBuilder.Append("%' ");
				}
				stringBuilder.Append(")");
			}
			stringBuilder2.Append(stringBuilder.ToString());
			stringBuilder2.Append(" ORDER BY ");
			switch (resultorder)
			{
				case 1:
				{
					stringBuilder2.Append("`commentcount`");
					break;
				}
				case 2:
				{
					stringBuilder2.Append("`views`");
					break;
				}
				default:
				{
					stringBuilder2.Append("`postdatetime`");
					break;
				}
			}
			if (resultordertype == 1)
			{
				stringBuilder2.Append(" ASC");
			}
			else
			{
				stringBuilder2.Append(" DESC");
			}
			return stringBuilder2.ToString();
		}
		private string GetSearchAlbumTitleSQL(int posterid, int resultorder, int resultordertype, int searchtime, int searchtimetype, string keyword)
		{
			keyword = Regex.Replace(keyword, "--|;|'|\"", "", RegexOptions.Multiline | RegexOptions.Compiled);
			StringBuilder stringBuilder = new StringBuilder(keyword);
			stringBuilder.Replace("'", "''");
			stringBuilder.Replace("%", "[%]");
			stringBuilder.Replace("_", "[_]");
			stringBuilder.Replace("[", "[[]");
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendFormat("SELECT `albumid` FROM `{0}albums` WHERE `{0}albums`.`type`=0 ", BaseConfigs.get_GetTablePrefix());
			if (posterid > 0)
			{
				stringBuilder2.Append(" AND `userid`=");
				stringBuilder2.Append(posterid);
			}
			if (searchtime != 0)
			{
				stringBuilder2.AppendFormat(" AND `{0}albums`.`createdatetime`", BaseConfigs.get_GetTablePrefix());
				if (searchtimetype == 1)
				{
					stringBuilder2.Append("<'");
				}
				else
				{
					stringBuilder2.Append(">'");
				}
				StringBuilder arg_105_0 = stringBuilder2;
				DateTime dateTime = DateTime.Now;
				dateTime = dateTime.AddDays((double)searchtime);
				arg_105_0.Append(dateTime.ToString("yyyy-MM-dd 00:00:00"));
				stringBuilder2.Append("' ");
			}
			string[] array = Utils.SplitString(stringBuilder.ToString(), " ");
			stringBuilder = new StringBuilder();
			if (keyword.Length > 0)
			{
				stringBuilder.Append(" AND (1=0 ");
				for (int i = 0; i < array.Length; i++)
				{
					stringBuilder.Append(" OR `title` ");
					stringBuilder.Append("LIKE '%");
					stringBuilder.Append(DataProvider.RegEsc(array[i]));
					stringBuilder.Append("%' ");
				}
				stringBuilder.Append(")");
			}
			stringBuilder2.Append(stringBuilder.ToString());
			stringBuilder2.Append(" ORDER BY ");
			if (resultorder != 1)
			{
				stringBuilder2.Append("`createdatetime`");
			}
			else
			{
				stringBuilder2.Append("`albumid`");
			}
			if (resultordertype == 1)
			{
				stringBuilder2.Append(" ASC");
			}
			else
			{
				stringBuilder2.Append(" DESC");
			}
			return stringBuilder2.ToString();
		}
		private string GetSearchByPosterSQL(int posterid, int posttableid)
		{
			string result;
			if (posterid > 0)
			{
				string text = string.Format("SELECT DISTINCT `tid`, 'forum' AS `datafrom` FROM `{0}posts{1}` WHERE `posterid`={2} AND `tid` NOT IN (SELECT `tid` FROM `{0}topics` WHERE `posterid`={2} AND `displayorder`<0) UNION ALL SELECT `albumid`,'album' AS `datafrom` FROM `{0}albums` WHERE   `userid`={2} UNION ALL SELECT `postid`,'spacepost' AS `datafrom` FROM `{0}spaceposts` WHERE `uid`={2}", BaseConfigs.get_GetTablePrefix(), posttableid, posterid);
				result = text;
			}
			else
			{
				result = string.Empty;
			}
			return result;
		}
		private StringBuilder GetSearchByPosterResult(IDataReader reader)
		{
			StringBuilder stringBuilder = new StringBuilder("<ForumTopics>");
			StringBuilder stringBuilder2 = new StringBuilder("<Albums>");
			StringBuilder stringBuilder3 = new StringBuilder("<SpacePosts>");
			StringBuilder stringBuilder4 = new StringBuilder();
			if (reader != null)
			{
				while (reader.Read())
				{
					string text = reader[1].ToString();
					if (text != null)
					{
						if (!(text == "forum"))
						{
							if (!(text == "album"))
							{
								if (text == "spacepost")
								{
									stringBuilder3.AppendFormat("{0},", reader[0].ToString());
								}
							}
							else
							{
								stringBuilder2.AppendFormat("{0},", reader[0].ToString());
							}
						}
						else
						{
							stringBuilder.AppendFormat("{0},", reader[0].ToString());
						}
					}
				}
				reader.Close();
			}
			if (stringBuilder.ToString().EndsWith(","))
			{
				stringBuilder.Length--;
			}
			if (stringBuilder2.ToString().EndsWith(","))
			{
				stringBuilder2.Length--;
			}
			if (stringBuilder3.ToString().EndsWith(","))
			{
				stringBuilder3.Length--;
			}
			stringBuilder.Append("</ForumTopics>");
			stringBuilder2.Append("</Albums>");
			stringBuilder3.Append("</SpacePosts>");
			stringBuilder4.Append(stringBuilder.ToString());
			stringBuilder4.Append(stringBuilder2.ToString());
			stringBuilder4.Append(stringBuilder3.ToString());
			return stringBuilder4;
		}
		public int Search(int posttableid, int userid, int usergroupid, string keyword, int posterid, string type, string searchforumid, int keywordtype, int searchtime, int searchtimetype, int resultorder, int resultordertype)
		{
			DatabaseProvider.GetInstance().DeleteExpriedSearchCache();
			string text = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			SearchType searchType = 2;
			switch (keywordtype)
			{
				case 0:
				{
					searchType = 3;
					if (type == "digest")
					{
						searchType = 1;
					}
					break;
				}
				case 1:
				{
					searchType = 4;
					break;
				}
				case 2:
				{
					searchType = 6;
					break;
				}
				case 3:
				{
					searchType = 5;
					break;
				}
				case 8:
				{
					searchType = 7;
					break;
				}
			}
			switch (searchType)
			{
				case 0:
				{
					break;
				}
				case 1:
				{
					text = this.GetSearchTopicTitleSQL(posterid, searchforumid, resultorder, resultordertype, 1, keyword);
					break;
				}
				case 2:
				{
					text = this.GetSearchTopicTitleSQL(posterid, searchforumid, resultorder, resultordertype, 0, keyword);
					break;
				}
				case 3:
				{
					text = this.GetSearchTopicTitleSQL(posterid, searchforumid, resultorder, resultordertype, 0, keyword);
					break;
				}
				case 4:
				{
					text = this.GetSearchPostContentSQL(posterid, searchforumid, resultorder, resultordertype, searchtime, searchtimetype, posttableid, new StringBuilder(keyword));
					break;
				}
				case 5:
				{
					text = this.GetSearchAlbumTitleSQL(posterid, resultorder, resultordertype, searchtime, searchtimetype, keyword);
					break;
				}
				case 6:
				{
					text = this.GetSearchSpacePostTitleSQL(posterid, resultorder, resultordertype, searchtime, searchtimetype, keyword);
					break;
				}
				case 7:
				{
					text = this.GetSearchByPosterSQL(posterid, posttableid);
					break;
				}
				default:
				{
					text = this.GetSearchTopicTitleSQL(posterid, searchforumid, resultorder, resultordertype, 0, keyword);
					break;
				}
			}
			int result;
			if (text == string.Empty)
			{
				result = -1;
			}
			else
			{
				DbParameter[] array = new DbParameter[]
				{
					DbHelper.MakeInParam("?searchstring", (DbType)253, 255, text), 
					DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userid), 
					DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, usergroupid)
				};
				int num = Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT `searchid` FROM `{0}searchcaches` WHERE `searchstring`=?searchstring AND `groupid`=?groupid LIMIT 1", BaseConfigs.get_GetTablePrefix()), array), -1);
				if (num > -1)
				{
					result = num;
				}
				else
				{
					IDataReader dataReader;
					try
					{
						dataReader = DbHelper.ExecuteReader(CommandType.Text, text);
					}
					catch
					{
						this.ConfirmFullTextEnable();
						dataReader = DbHelper.ExecuteReader(CommandType.Text, text);
					}
					int num2 = 0;
					if (dataReader != null)
					{
						switch (searchType)
						{
							case 0:
							case 1:
							case 2:
							case 3:
							case 4:
							{
								stringBuilder.Append("<ForumTopics>");
								break;
							}
							case 5:
							{
								stringBuilder.Append("<Albums>");
								break;
							}
							case 6:
							{
								stringBuilder.Append("<SpacePosts>");
								break;
							}
							case 7:
							{
								stringBuilder = this.GetSearchByPosterResult(dataReader);
								SearchCacheInfo searchCacheInfo = new SearchCacheInfo();
								searchCacheInfo.set_Keywords(keyword);
								searchCacheInfo.set_Searchstring(text);
								searchCacheInfo.set_Postdatetime(Utils.GetDateTime());
								searchCacheInfo.set_Topics(num2);
								searchCacheInfo.set_Tids(stringBuilder.ToString());
								searchCacheInfo.set_Uid(userid);
								searchCacheInfo.set_Groupid(usergroupid);
								searchCacheInfo.set_Ip(DNTRequest.GetIP());
								searchCacheInfo.set_Expiration(Utils.GetDateTime());
								dataReader.Close();
								result = this.CreateSearchCache(searchCacheInfo);
								return result;
							}
						}
						while (dataReader.Read())
						{
							stringBuilder.Append(dataReader[0].ToString());
							stringBuilder.Append(",");
							num2++;
						}
						if (num2 > 0)
						{
							stringBuilder.Remove(stringBuilder.Length - 1, 1);
							switch (searchType)
							{
								case 0:
								case 1:
								case 2:
								case 3:
								case 4:
								{
									stringBuilder.Append("</ForumTopics>");
									break;
								}
								case 5:
								{
									stringBuilder.Append("</Albums>");
									break;
								}
								case 6:
								{
									stringBuilder.Append("</SpacePosts>");
									break;
								}
							}
							SearchCacheInfo searchCacheInfo = new SearchCacheInfo();
							searchCacheInfo.set_Keywords(keyword);
							searchCacheInfo.set_Searchstring(text);
							searchCacheInfo.set_Postdatetime(Utils.GetDateTime());
							searchCacheInfo.set_Topics(num2);
							searchCacheInfo.set_Tids(stringBuilder.ToString());
							searchCacheInfo.set_Uid(userid);
							searchCacheInfo.set_Groupid(usergroupid);
							searchCacheInfo.set_Ip(DNTRequest.GetIP());
							searchCacheInfo.set_Expiration(Utils.GetDateTime());
							dataReader.Close();
							result = this.CreateSearchCache(searchCacheInfo);
							return result;
						}
						dataReader.Close();
					}
					result = -1;
				}
			}
			return result;
		}
		public string BackUpDatabase(string backuppath, string ServerName, string UserName, string Password, string strDbName, string strFileName)
		{
			return string.Empty;
		}
		public string RestoreDatabase(string backuppath, string ServerName, string UserName, string Password, string strDbName, string strFileName)
		{
			return string.Empty;
		}
		public string SearchVisitLog(DateTime postdatetimeStart, DateTime postdatetimeEnd, string Username, string others)
		{
			string text = null;
			text += " `visitid`>0";
			if (postdatetimeStart.ToString() != "")
			{
				object obj = text;
				text = string.Concat(new object[]
				{
					obj, 
					" And `postdatetime`>='", 
					postdatetimeStart, 
					"'"
				});
			}
			if (postdatetimeEnd.ToString() != "")
			{
				text = text + " And `postdatetime`<='" + postdatetimeEnd.AddDays(1.0).ToString() + "'";
			}
			if (others != "")
			{
				text = text + " And `others` LIKE '%" + DataProvider.RegEsc(others) + "%'";
			}
			if (Username != "")
			{
				text += " And (";
				string[] array = Username.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						text = text + " `username` like '%" + DataProvider.RegEsc(text2) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			return text;
		}
		public string SearchMedalLog(DateTime postdatetimeStart, DateTime postdatetimeEnd, string Username, string reason)
		{
			string text = null;
			text += " `id`>0";
			if (postdatetimeStart.ToString() != "")
			{
				text = text + " And `postdatetime`>='" + postdatetimeStart.ToString() + "'";
			}
			if (postdatetimeEnd.ToString() != "")
			{
				text = text + " And `postdatetime`<='" + postdatetimeEnd.AddDays(1.0).ToString() + "'";
			}
			if (reason != "")
			{
				text = text + " And `reason` LIKE '%" + DataProvider.RegEsc(reason) + "%'";
			}
			if (Username != "")
			{
				text += " And (";
				string[] array = Username.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						text = text + " `username` like '%" + DataProvider.RegEsc(text2) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			return text;
		}
		public string SearchModeratorManageLog(DateTime postdatetimeStart, DateTime postdatetimeEnd, string Username, string others)
		{
			string text = null;
			text += " `id`>0";
			if (postdatetimeStart.ToString() != "")
			{
				text = text + " And `postdatetime`>='" + postdatetimeStart.ToString() + "'";
			}
			if (postdatetimeEnd.ToString() != "")
			{
				text = text + " And `postdatetime`<='" + postdatetimeEnd.AddDays(1.0).ToString() + "'";
			}
			if (others != "")
			{
				text = text + " And `reason` LIKE '%" + DataProvider.RegEsc(others) + "%'";
			}
			if (Username != "")
			{
				text += " And (";
				string[] array = Username.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						text = text + " `moderatorname` like '%" + DataProvider.RegEsc(text2) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			return text;
		}
		public string SearchPaymentLog(DateTime postdatetimeStart, DateTime postdatetimeEnd, string Username)
		{
			string text = null;
			text = text + " `" + BaseConfigs.get_GetTablePrefix() + "paymentlog`.`id`>0";
			if (postdatetimeStart.ToString() != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" And `", 
					BaseConfigs.get_GetTablePrefix(), 
					"paymentlog`.`buydate`>='", 
					postdatetimeStart.ToString("yyyy-MM-dd HH:mm:ss"), 
					"'"
				});
			}
			if (postdatetimeEnd.ToString() != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" And `", 
					BaseConfigs.get_GetTablePrefix(), 
					"paymentlog`.`buydate`<='", 
					postdatetimeEnd.AddDays(1.0).ToString("yyyy-MM-dd HH:mm:ss"), 
					"'"
				});
			}
			if (Username != "")
			{
				string text3 = " WHERE (";
				string[] array = Username.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text4 = array[i];
					if (text4.Trim() != "")
					{
						text3 = text3 + " `username` like '%" + DataProvider.RegEsc(text4) + "%' OR ";
					}
				}
				text3 = text3.Substring(0, text3.Length - 3) + ")";
				DataTable dataTable = DbHelper.ExecuteDataset("SELECT `uid` From `" + BaseConfigs.get_GetTablePrefix() + "users` " + text3).Tables[0];
				string text5 = "-1";
				if (dataTable.Rows.Count > 0)
				{
					foreach (DataRow dataRow in dataTable.Rows)
					{
						text5 = text5 + "," + dataRow["uid"].ToString();
					}
				}
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" And `", 
					BaseConfigs.get_GetTablePrefix(), 
					"paymentlog`.`uid` IN(", 
					text5, 
					")"
				});
			}
			return text;
		}
		public string SearchRateLog(DateTime postdatetimeStart, DateTime postdatetimeEnd, string Username, string others)
		{
			string text = null;
			text += " `id`>0";
			if (postdatetimeStart.ToString() != "")
			{
				text = text + " And `postdatetime`>='" + postdatetimeStart.ToString() + "'";
			}
			if (postdatetimeEnd.ToString() != "")
			{
				text = text + " And `postdatetime`<='" + postdatetimeEnd.AddDays(1.0).ToString() + "'";
			}
			if (others != "")
			{
				text = text + " And `reason` LIKE '%" + DataProvider.RegEsc(others) + "%'";
			}
			if (Username != "")
			{
				text += " And (";
				string[] array = Username.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						text = text + " `username` like '%" + DataProvider.RegEsc(text2) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			return text;
		}
		public string DeletePrivateMessages(bool isnew, string postdatetime, string msgfromlist, bool lowerupper, string subject, string message, bool isupdateusernewpm)
		{
			string text = null;
			text += "WHERE `pmid`>0";
			if (isnew)
			{
				text += " AND `new`=0";
			}
			if (postdatetime != "")
			{
				text = text + " And datediff(postdatetime,NOW())>=" + postdatetime;
			}
			if (msgfromlist != "")
			{
				text += " And (";
				string[] array = msgfromlist.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						if (lowerupper)
						{
							text = text + " `msgfrom`='" + text2 + "' OR";
						}
						else
						{
							text = text + " `msgfrom` COLLATE Chinese_PRC_CS_AS_WS ='" + text2 + "' OR";
						}
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (subject != "")
			{
				text += " And (";
				string[] array = subject.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text3 = array[i];
					if (text3.Trim() != "")
					{
						text = text + " `subject` like '%" + DataProvider.RegEsc(text3) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (message != "")
			{
				text += " And (";
				string[] array = message.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text4 = array[i];
					if (text4.Trim() != "")
					{
						text = text + " `message` like '%" + DataProvider.RegEsc(text4) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (isupdateusernewpm)
			{
				DbHelper.ExecuteNonQuery(string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users` SET `newpm`=0 WHERE `uid` IN (SELECT `msgtoid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"pms` ", 
					text, 
					")"
				}));
			}
			DbHelper.ExecuteNonQuery("DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "pms` " + text);
			return text;
		}
		public bool IsExistSmilieCode(string code, int currentid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?code", (DbType)253, 30, code), 
				DbHelper.MakeInParam("?currentid", DbType.Boolean, 4, currentid)
			};
			string text = "SELECT `id` FROM `" + BaseConfigs.get_GetTablePrefix() + "smilies` WHERE code=?code AND id<>?currentid";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0].Rows.Count != 0;
		}
		public string GetSmilieByType(int id)
		{
			return string.Concat(new object[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"smilies` WHERE TYPE=", 
				id
			});
		}
		public string AddTableData()
		{
			return "SELECT `groupid`, `grouptitle` FROM `" + BaseConfigs.get_GetTablePrefix() + "usergroups` WHERE `groupid`<=3 ORDER BY `groupid`";
		}
		public string Global_UserGrid_GetCondition(string getstring)
		{
			return string.Concat(new string[]
			{
				"`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`username`='", 
				getstring, 
				"'"
			});
		}
		public int Global_UserGrid_RecordCount()
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset("SELECT  Count(uid) From `" + BaseConfigs.get_GetTablePrefix() + "users`").Tables[0].Rows[0][0].ToString());
		}
		public int Global_UserGrid_RecordCount(string condition)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset("SELECT  Count(uid) From `" + BaseConfigs.get_GetTablePrefix() + "users`  WHERE " + condition).Tables[0].Rows[0][0].ToString());
		}
		public string Global_UserGrid_SearchCondition(bool islike, bool ispostdatetime, string username, string nickname, string UserGroup, string email, string credits_start, string credits_end, string lastip, string posts, string digestposts, string uid, string joindateStart, string joindateEnd)
		{
			string text = " `" + BaseConfigs.get_GetTablePrefix() + "users`.`uid`>0 ";
			if (islike)
			{
				if (username != "")
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2, 
						" And `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`username` like'%", 
						DataProvider.RegEsc(username), 
						"%'"
					});
				}
				if (nickname != "")
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2, 
						" And `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`nickname` like'%", 
						DataProvider.RegEsc(nickname), 
						"%'"
					});
				}
			}
			else
			{
				if (username != "")
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2, 
						" And `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`username` ='", 
						username, 
						"'"
					});
				}
				if (nickname != "")
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2, 
						" And `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`nickname` ='", 
						nickname, 
						"'"
					});
				}
			}
			if (UserGroup != "0")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" And `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users`.`groupid`=", 
					UserGroup
				});
			}
			if (email != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" AND `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users`.`email` LIKE '%", 
					DataProvider.RegEsc(email), 
					"%'"
				});
			}
			if (credits_start != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" AND `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users`.`credits` >=", 
					credits_start
				});
			}
			if (credits_end != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" AND `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users`.`credits` <=", 
					credits_end
				});
			}
			if (lastip != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" AND `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users`.`lastip` LIKE '%", 
					DataProvider.RegEsc(lastip), 
					"%'"
				});
			}
			if (posts != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" AND `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users`.`posts` >=", 
					posts
				});
			}
			if (digestposts != "")
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2, 
					" AND `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users`.`digestposts` >=", 
					digestposts
				});
			}
			if (uid != "")
			{
				uid = uid.Replace(", ", ",");
				if (uid.IndexOf(",") == 0)
				{
					uid = uid.Substring(1, uid.Length - 1);
				}
				if (uid.LastIndexOf(",") == uid.Length - 1)
				{
					uid = uid.Substring(0, uid.Length - 1);
				}
				if (uid != "")
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2, 
						" And `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users`.`uid` IN(", 
						uid, 
						")"
					});
				}
			}
			if (ispostdatetime)
			{
				string text2 = text;
				string[] array = new string[6];
				array[0] = text2;
				array[1] = " And `";
				array[2] = BaseConfigs.get_GetTablePrefix();
				array[3] = "users`.`joindate` >='";
				string[] arg_4E4_0 = array;
				int arg_4E4_1 = 4;
				DateTime dateTime = DateTime.Parse(joindateStart);
				arg_4E4_0[arg_4E4_1] = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
				array[5] = "'";
				text = string.Concat(array);
				text2 = text;
				array = new string[6];
				array[0] = text2;
				array[1] = " And `";
				array[2] = BaseConfigs.get_GetTablePrefix();
				array[3] = "users`.`joindate` <='";
				string[] arg_538_0 = array;
				int arg_538_1 = 4;
				dateTime = DateTime.Parse(joindateStart);
				arg_538_0[arg_538_1] = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
				array[5] = "'";
				text = string.Concat(array);
			}
			return text;
		}
		public DataTable Global_UserGrid_Top2(string searchcondition)
		{
			string text = string.Concat(new object[]
			{
				"SELECT `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`  FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` WHERE ", 
				searchcondition, 
				" LIMIT 0,", 
				2
			});
			return DbHelper.ExecuteDataset(text).Tables[0];
		}
		public ArrayList CheckDbFree()
		{
			ArrayList arrayList = new ArrayList();
			ArrayList result;
			if (DbHelper.get_Provider().IsDbOptimize())
			{
				IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, "SHOW TABLE STATUS");
				if (dataReader == null)
				{
					result = null;
					return result;
				}
				while (dataReader.Read())
				{
					if (int.Parse(dataReader["Data_free"].ToString()) != 0)
					{
						MySqlInfo mySqlInfo = new MySqlInfo();
						mySqlInfo.set_tablename(dataReader["Name"].ToString());
						mySqlInfo.set_tabletype(dataReader["Engine"].ToString());
						mySqlInfo.set_rowcount(dataReader["Rows"].ToString());
						mySqlInfo.set_tabledata(dataReader["Data_length"].ToString());
						mySqlInfo.set_index(dataReader["Index_length"].ToString());
						mySqlInfo.set_datafree(dataReader["Data_free"].ToString());
						arrayList.Add(mySqlInfo);
					}
				}
				dataReader.Close();
			}
			result = arrayList;
			return result;
		}
		public void DbOptimize(string tablelist)
		{
			string[] array = tablelist.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				DbHelper.ExecuteNonQuery("OPTIMIZE TABLE `" + array[i].ToString() + "`");
			}
		}
		public void UpdateAdminUsergroup(string targetadminusergroup, string sourceadminusergroup)
		{
			DbHelper.ExecuteNonQuery(string.Concat(new string[]
			{
				"Update `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` SET `groupid`=", 
				targetadminusergroup, 
				" Where `groupid`=", 
				sourceadminusergroup
			}));
		}
		public void UpdateUserCredits(string formula)
		{
			DbHelper.ExecuteNonQuery("UPDATE `" + BaseConfigs.get_GetTablePrefix() + "users` SET `credits`=" + formula);
		}
		public DataTable MailListTable(string usernamelist)
		{
			string text = " WHERE `Email` Is Not null AND (";
			string[] array = usernamelist.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				if (text2.Trim() != "")
				{
					text = text + " `username` like '%" + DataProvider.RegEsc(text2.Trim()) + "%' OR ";
				}
			}
			text = text.Substring(0, text.Length - 3) + ")";
			return DbHelper.ExecuteDataset("SELECT `username`,`Email`  From `" + BaseConfigs.get_GetTablePrefix() + "users` " + text).Tables[0];
		}
		public IDataReader GetTagsListByTopic(int topicid)
		{
			string text = string.Format("SELECT `{0}tags`.* FROM `{0}tags`, `{0}topictags` WHERE `{0}topictags`.`tagid` = `{0}tags`.`tagid` AND `{0}topictags`.`tid` = ?topicid ORDER BY `orderid`", BaseConfigs.get_GetTablePrefix());
			DbParameter dbParameter = DbHelper.MakeInParam("?topicid", DbType.Boolean, 4, topicid);
			return DbHelper.ExecuteReader(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public IDataReader GetTagInfo(int tagid)
		{
			string text = string.Format("SELECT * FROM `{0}tags` WHERE `tagid`=?tagid", BaseConfigs.get_GetTablePrefix());
			DbParameter dbParameter = DbHelper.MakeInParam("?tagid", DbType.Boolean, 4, tagid);
			return DbHelper.ExecuteReader(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public void SetLastExecuteScheduledEventDateTime(string key, string servername, DateTime lastexecuted)
		{
			string text = string.Format("DELETE FROM `{0}scheduledevents` WHERE (`key`=?key) AND (`lastexecuted` < ADDDATE( NOW(),- 7))", BaseConfigs.get_GetTablePrefix());
			string text2 = string.Format("INSERT `dnt_scheduledevents` (`key`, `servername`, `lastexecuted`) Values (?key, ?servername, ?lastexecuted)", new object[0]);
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?key", (DbType)253, 100, key), 
				DbHelper.MakeInParam("?servername", (DbType)253, 100, servername), 
				DbHelper.MakeInParam("?lastexecuted", DbType.Int64, 8, lastexecuted)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			DbHelper.ExecuteNonQuery(CommandType.Text, text2, array);
		}
		public DateTime GetLastExecuteScheduledEventDateTime(string key, string servername)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?key", (DbType)253, 100, key), 
				DbHelper.MakeInParam("?servername", (DbType)253, 100, servername)
			};
			string text = DbHelper.ExecuteScalar(CommandType.Text, "SELECT MAX(`lastexecuted`) FROM `" + BaseConfigs.get_GetTablePrefix() + "scheduledevents` WHERE `key` = ?key AND `servername` = ?servername", array).ToString();
			DateTime result;
			if (text == null || text == "")
			{
				text = DbHelper.ExecuteScalar(CommandType.Text, "select  ADDDATE(now(),-365)").ToString();
				result = DateTime.Parse(text.ToString());
			}
			else
			{
				result = DateTime.Parse(text.ToString());
			}
			return result;
		}
		public void UpdateStats(string type, string variable, int count)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?type", (DbType)253, 10, type), 
				DbHelper.MakeInParam("?variable", (DbType)253, 20, variable), 
				DbHelper.MakeInParam("?count", DbType.Boolean, 4, count)
			};
			string text = string.Format("UPDATE `{0}stats` SET `count`=`count`+?count WHERE `type`=?type AND `variable`=?variable", BaseConfigs.get_GetTablePrefix());
			int num = DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			if (num == 0)
			{
				if (count == 0)
				{
					array[2].Value = 1;
				}
				text = string.Format("INSERT INTO `{0}stats` (`type`,`variable`,`count`) VALUES(?type, ?variable, ?count)", BaseConfigs.get_GetTablePrefix());
				DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			}
		}
		public void UpdateStatVars(string type, string variable, string value)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?type", (DbType)253, 20, type), 
				DbHelper.MakeInParam("?variable", (DbType)253, 20, variable), 
				DbHelper.MakeInParam("?value", (DbType)752, 0, value)
			};
			string text = string.Format("UPDATE `{0}statvars` SET `value`=?value WHERE `type`=?type AND `variable`=?variable", BaseConfigs.get_GetTablePrefix());
			int num = DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			if (num == 0)
			{
				text = string.Format("INSERT INTO `{0}statvars` (`type`,`variable`,`value`) VALUES(?type, ?variable, ?value)", BaseConfigs.get_GetTablePrefix());
				DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			}
		}
		public IDataReader GetAllStats()
		{
			string text = string.Format("SELECT `type`, `variable`, `count` FROM `{0}stats` ORDER BY `type`,`variable`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetAllStatVars()
		{
			string text = string.Format("SELECT `type`, `variable`, `value` FROM `{0}statvars` ORDER BY `type`,`variable`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetStatsByType(string type)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?type", (DbType)253, 10, type);
			string text = string.Format("SELECT `type`, `variable`, `count` FROM `{0}stats` WHERE `type`=?type ORDER BY `type`,`variable`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetStatVarsByType(string type)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?type", (DbType)253, 10, type);
			string text = string.Format("SELECT `type`, `variable`, `value` FROM `{0}statvars` WHERE `type`=?type ORDER BY `type`,`variable`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public void DeleteOldDayposts()
		{
			string arg_2E_0 = "DELETE FROM {0}statvars WHERE `type`='dayposts' AND `variable`<'{1}'";
			object arg_2E_1 = BaseConfigs.get_GetTablePrefix();
			DateTime dateTime = DateTime.Now;
			dateTime = dateTime.AddDays(-30.0);
			string text = string.Format(arg_2E_0, arg_2E_1, dateTime.ToString("yyyy-MM-dd"));
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public DateTime GetPostStartTime()
		{
			string text = string.Format("SELECT MIN(`postdatetime`) FROM `{0}posts1` WHERE `invisible`=0", BaseConfigs.get_GetTablePrefix());
			object obj = DbHelper.ExecuteScalar(CommandType.Text, text);
			DateTime result;
			if (obj == null)
			{
				result = DateTime.Now;
			}
			else
			{
				result = Convert.ToDateTime(obj);
			}
			return result;
		}
		public int GetForumCount()
		{
			string text = string.Format("SELECT COUNT(1) FROM `{0}forums` WHERE `layer`>0 AND `status`>0", BaseConfigs.get_GetTablePrefix());
			return (int)DbHelper.ExecuteScalar(CommandType.Text, text);
		}
		public int GetTodayPostCount(string posttableid)
		{
			if (!Utils.IsNumeric(posttableid))
			{
				posttableid = "1";
			}
			string text = string.Format("SELECT COUNT(1) FROM `{0}posts{1}` WHERE `postdatetime`>='{2}' AND `invisible`=0", BaseConfigs.get_GetTablePrefix(), posttableid, DateTime.Now.ToString("yyyy-MM-dd"));
			return (int)DbHelper.ExecuteScalar(CommandType.Text, text);
		}
		public int GetTodayNewMemberCount()
		{
			string text = string.Format("SELECT COUNT(1) FROM `{0}users` WHERE `joindate`>='{1}'", BaseConfigs.get_GetTablePrefix(), DateTime.Now.ToString("yyyy-MM-dd"));
			return (int)DbHelper.ExecuteScalar(CommandType.Text, text);
		}
		public int GetAdminCount()
		{
			string text = string.Format("SELECT COUNT(1) FROM `{0}users` WHERE `adminid`>0", BaseConfigs.get_GetTablePrefix(), DateTime.Now.ToString("yyyy-MM-dd"));
			return (int)DbHelper.ExecuteScalar(CommandType.Text, text);
		}
		public int GetNonPostMemCount()
		{
			string text = string.Format("SELECT COUNT(1) FROM `{0}users` WHERE `posts`=0", BaseConfigs.get_GetTablePrefix(), DateTime.Now.ToString("yyyy-MM-dd"));
			return (int)DbHelper.ExecuteScalar(CommandType.Text, text);
		}
		public IDataReader GetBestMember(string posttableid)
		{
			if (!Utils.IsNumeric(posttableid))
			{
				posttableid = "1";
			}
			string text = string.Format("SELECT `poster`, COUNT(1) AS `posts` FROM `{0}posts{1}` WHERE `postdatetime`>='{2}' AND `invisible`=0 AND `posterid`>0 GROUP BY `poster` ORDER BY `posts` DESC limit 1", BaseConfigs.get_GetTablePrefix(), posttableid, DateTime.Now.ToString("yyyy-MM-dd"));
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetMonthPostsStats(string posttableid)
		{
			if (!Utils.IsNumeric(posttableid))
			{
				posttableid = "1";
			}
			string text = string.Format("SELECT COUNT(1) AS `count`,MONTH(`postdatetime`) AS `month`,YEAR(`postdatetime`) AS `year` FROM `{0}posts{1}` GROUP BY MONTH(`postdatetime`),YEAR(`postdatetime`) ORDER BY YEAR(`postdatetime`),MONTH(`postdatetime`)", BaseConfigs.get_GetTablePrefix(), posttableid);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetDayPostsStats(string posttableid)
		{
			if (!Utils.IsNumeric(posttableid))
			{
				posttableid = "1";
			}
			string arg_40_0 = "SELECT COUNT(1) AS `count`,YEAR(`postdatetime`) AS `year`,MONTH(`postdatetime`) AS `month`,DAY(`postdatetime`) AS `day` FROM `{0}posts{1}` WHERE `invisible`=0 AND `postdatetime` > '{2}' GROUP BY DAY(`postdatetime`), MONTH(`postdatetime`),YEAR(`postdatetime`) ORDER BY YEAR(`postdatetime`),MONTH(`postdatetime`),DAY(`postdatetime`)";
			object arg_40_1 = BaseConfigs.get_GetTablePrefix();
			object arg_40_2 = posttableid;
			DateTime dateTime = DateTime.Now;
			dateTime = dateTime.AddDays(-30.0);
			string text = string.Format(arg_40_0, arg_40_1, arg_40_2, dateTime.ToString("yyyy-MM-dd"));
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetForumsByTopicCount(int count)
		{
			string text = string.Format("SELECT `fid`, `name`, `topics` FROM `{0}forums` WHERE `status`>0 AND `layer`=0 ORDER BY `topics` DESC LIMIT {0}", BaseConfigs.get_GetTablePrefix(), count);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetForumsByPostCount(int count)
		{
			string text = string.Format("SELECT `fid`, `name`, `posts` FROM `{0}forums` WHERE `status`>0 AND `layer`=0 ORDER BY `posts` DESC LIMIT {0}", BaseConfigs.get_GetTablePrefix(), count);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetForumsByMonthPostCount(int count, string posttableid)
		{
			if (!Utils.IsNumeric(posttableid))
			{
				posttableid = "1";
			}
			string arg_5C_0 = "SELECT DISTINCT `p`.`fid`, `f`.`name`, COUNT(`pid`) AS `posts` FROM `{0}posts{1}` `p` LEFT JOIN `{0}forums` `f` ON `p`.`fid`=`f`.`fid` WHERE `postdatetime`>='{2}' AND `invisible`=0 AND `posterid`>0 GROUP BY `p`.`fid`, `f`.`name` ORDER BY `posts` DESC limit {3}";
			object[] array = new object[4];
			array[0] = BaseConfigs.get_GetTablePrefix();
			array[1] = posttableid;
			object[] arg_51_0 = array;
			int arg_51_1 = 2;
			DateTime dateTime = DateTime.Now;
			dateTime = dateTime.AddDays(-30.0);
			arg_51_0[arg_51_1] = dateTime.ToString("yyyy-MM-dd");
			array[3] = count;
			string text = string.Format(arg_5C_0, array);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetForumsByDayPostCount(int count, string posttableid)
		{
			if (!Utils.IsNumeric(posttableid))
			{
				posttableid = "1";
			}
			string text = string.Format("SELECT DISTINCT `p`.`fid`, `f`.`name`, COUNT(`pid`) AS `posts` FROM `{0}posts{1}` `p` LEFT JOIN `{0}forums` `f` ON `p`.`fid`=`f`.`fid` WHERE `postdatetime`>='{2}' AND `invisible`=0 AND `posterid`>0 GROUP BY `p`.`fid`, `f`.`name` ORDER BY `posts` DESC LIMIT {3}", new object[]
			{
				BaseConfigs.get_GetTablePrefix(), 
				posttableid, 
				DateTime.Now.ToString("yyyy-MM-dd"), 
				count
			});
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		

		public IDataReader GetSuperModerators()
		{
			string text = string.Format("SELECT `fid`, `uid` FROM `{0}moderators` WHERE `inherited`=0 ORDER BY `displayorder`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetModeratorsDetails(string uids, int oltimespan)
		{
			IDataReader result;
			if (!Utils.IsNumericArray(uids.Split(new char[]
			{
				','
			})))
			{
				result = null;
			}
			else
			{
				string text = "m.`uid` IN ({0}) OR ";
				if (uids == string.Empty)
				{
					text = string.Empty;
				}
				string text2 = string.Empty;
				string text3 = string.Empty;
				if (oltimespan > 0)
				{
					text2 = ", o.`thismonth` AS `thismonthol`, o.`total` AS `totalol`";
					text3 = string.Format(" LEFT JOIN `{0}onlinetime` o ON o.`uid`=m.`uid`", BaseConfigs.get_GetTablePrefix());
				}
				string text4 = string.Format("SELECT m.`uid`, m.`username`, m.`adminid`, m.`lastactivity`, m.`credits`, m.`posts`{0} FROM `{1}users` m{2} WHERE {3}m.`adminid` IN (1, 2) ORDER BY m.`adminid`", new object[]
				{
					text2, 
					BaseConfigs.get_GetTablePrefix(), 
					text3, 
					text
				});
				result = DbHelper.ExecuteReader(CommandType.Text, text4);
			}
			return result;
		}
		public void UpdateStatCount(string browser, string os, string visitorsadd)
		{
			DateTime now = DateTime.Now;
			object arg_30_0 = now.Year;
			now = DateTime.Now;
			int num = now.Month;
			string text = arg_30_0 + num.ToString("00");
			now = DateTime.Now;
			num = (int)now.DayOfWeek;
			string text2 = num.ToString();
			string arg_95_0 = "UPDATE `{0}stats` SET `count`=`count`+1 WHERE (`type`='total' AND `variable`='hits') {1} OR (`type`='month' AND `variable`='{2}') OR (`type`='week' AND `variable`='{3}') OR (`type`='hour' AND `variable`='{4}')";
			object[] array = new object[5];
			array[0] = BaseConfigs.get_GetTablePrefix();
			array[1] = visitorsadd;
			array[2] = text;
			array[3] = text2;
			object[] arg_92_0 = array;
			int arg_92_1 = 4;
			now = DateTime.Now;
			num = now.Hour;
			arg_92_0[arg_92_1] = num.ToString("00");
			string text3 = string.Format(arg_95_0, array);
			int num2 = DbHelper.ExecuteNonQuery(CommandType.Text, text3);
			int num3 = (visitorsadd.Trim() == string.Empty) ? 4 : 7;
			if (num3 > num2)
			{
				this.UpdateStats("browser", browser, 0);
				this.UpdateStats("os", os, 0);
				this.UpdateStats("total", "members", 0);
				this.UpdateStats("total", "guests", 0);
				this.UpdateStats("total", "hits", 0);
				this.UpdateStats("month", text, 0);
				this.UpdateStats("week", text2, 0);
				string arg_15E_1 = "hour";
				now = DateTime.Now;
				num = now.Hour;
				this.UpdateStats(arg_15E_1, num.ToString("00"), 0);
			}
		}
		public void DeleteSmilyByType(int type)
		{
			string text = string.Concat(new object[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"smilies` WHERE `type`=", 
				type
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public string SearchTopicAudit(int fid, string poster, string title, string moderatorname, DateTime postdatetimeStart, DateTime postdatetimeEnd, DateTime deldatetimeStart, DateTime deldatetimeEnd)
		{
			string text = null;
			text += " `displayorder`<0";
			if (fid != 0)
			{
				text = text + " AND `fid`=" + fid;
			}
			if (poster != "")
			{
				text += " AND (";
				string[] array = poster.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						text = text + " poster='" + text2 + "'  OR";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (title != "")
			{
				text += " AND (";
				string[] array = title.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text3 = array[i];
					if (text3.Trim() != "")
					{
						text = text + " title like '%" + DataProvider.RegEsc(text3) + "%' OR";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (moderatorname != "")
			{
				string text4 = "";
				DataTable dataTable = DatabaseProvider.GetInstance().GetTitleForModeratormanagelogByModeratorname(moderatorname);
				if (dataTable.Rows.Count > 0)
				{
					foreach (DataRow dataRow in dataTable.Rows)
					{
						text4 = text4 + dataRow["title"].ToString() + ",";
					}
					text = text + " And tid IN (" + text4.Substring(0, text4.Length - 1) + ") ";
				}
			}
			if (postdatetimeStart.ToString().IndexOf("1900") < 0)
			{
				text = text + " And `postdatetime`>='" + postdatetimeStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
			}
			if (postdatetimeEnd.ToString().IndexOf("1900") < 0)
			{
				text = text + " And `postdatetime`<='" + postdatetimeEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'";
			}
			if (deldatetimeStart.ToString().IndexOf("1900") < 0 && deldatetimeStart.ToString().IndexOf("1900") < 0)
			{
				string text5 = "";
				DataTable dataTable = DatabaseProvider.GetInstance().GetTitleForModeratormanagelogByPostdatetime(deldatetimeStart, deldatetimeStart);
				if (dataTable.Rows.Count > 0)
				{
					foreach (DataRow dataRow in dataTable.Rows)
					{
						text5 = text5 + dataRow["title"].ToString() + ",";
					}
					text = text + " And tid IN (" + text5.Substring(0, text5.Length - 1) + ") ";
				}
			}
			return text;
		}
		public void AddBBCCode(int available, string tag, string icon, string replacement, string example, string explanation, string param, string nest, string paramsdescript, string paramsdefvalue)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?available", DbType.Boolean, 4, available), 
				DbHelper.MakeInParam("?tag", DbType.Single, 100, tag), 
				DbHelper.MakeInParam("?icon", DbType.Single, 50, icon), 
				DbHelper.MakeInParam("?replacement", DbType.Single, 0, replacement), 
				DbHelper.MakeInParam("?example", DbType.Single, 255, example), 
				DbHelper.MakeInParam("?explanation", DbType.Single, 0, explanation), 
				DbHelper.MakeInParam("?params", DbType.Boolean, 4, param), 
				DbHelper.MakeInParam("?nest", DbType.Boolean, 4, nest), 
				DbHelper.MakeInParam("?paramsdescript", DbType.Single, 0, paramsdescript), 
				DbHelper.MakeInParam("?paramsdefvalue", DbType.Single, 0, paramsdefvalue)
			};
			string text = "INSERT INTO  `" + BaseConfigs.get_GetTablePrefix() + "bbcodes` (`available`,`tag`,`icon`,`replacement`,`example`,`explanation`,`params`,`nest`,`paramsdescript`,`paramsdefvalue`) VALUES(?available,?tag,?icon,?replacement,?example,?explanation,?params,?nest,?paramsdescript,?paramsdefvalue)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int CreateAttachment(AttachmentInfo attachmentinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, attachmentinfo.get_Uid()), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, attachmentinfo.get_Tid()), 
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, attachmentinfo.get_Pid()), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, DateTime.Parse(attachmentinfo.get_Postdatetime())), 
				DbHelper.MakeInParam("?readperm", DbType.Boolean, 4, attachmentinfo.get_Readperm()), 
				DbHelper.MakeInParam("?filename", DbType.Single, 100, attachmentinfo.get_Filename()), 
				DbHelper.MakeInParam("?description", DbType.Single, 100, attachmentinfo.get_Description()), 
				DbHelper.MakeInParam("?filetype", DbType.Single, 50, attachmentinfo.get_Filetype()), 
				DbHelper.MakeInParam("?filesize", DbType.Boolean, 4, attachmentinfo.get_Filesize()), 
				DbHelper.MakeInParam("?attachment", DbType.Single, 100, attachmentinfo.get_Attachment()), 
				DbHelper.MakeInParam("?downloads", DbType.Boolean, 4, attachmentinfo.get_Downloads())
			};
			int num = 0;
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "attachments`(`uid`,`tid`, `pid`, `postdatetime`, `readperm`, `filename`, `description`, `filetype`, `filesize`, `attachment`, `downloads`) VALUES(?uid, ?tid, ?pid, ?postdatetime, ?readperm, ?filename, ?description, ?filetype, ?filesize, ?attachment, ?downloads)";
			DbHelper.ExecuteNonQuery(ref num, CommandType.Text, text, array);
			int result = num;
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, attachmentinfo.get_Pid())
			};
			string text2 = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				Posts.GetPostTableID(attachmentinfo.get_Tid()), 
				"` SET `attachment`=1 WHERE `pid`=?pid"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text2, array2);
			return result;
		}
		public int UpdateTopicAttachmentType(int tid, int attType)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}topics` SET `attachment`={1} WHERE `tid`=?tid", BaseConfigs.get_GetTablePrefix(), attType), array);
		}
		public int UpdatePostAttachmentType(int pid, string postTableId, int attType)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}posts{1}` SET `attachment`={2} WHERE `pid`=?pid", BaseConfigs.get_GetTablePrefix(), postTableId, attType), array);
		}
		public IDataReader GetAttachmentInfo(int aid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?aid", DbType.Boolean, 4, aid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM `{0}attachments` WHERE `aid`=?aid LIMIT 1", BaseConfigs.get_GetTablePrefix()), array);
		}
		public int GetAttachmentCountByPid(int pid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(`aid`) AS `acount` FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `pid`=?pid", array), 0);
		}
		public int GetAttachmentCountByTid(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(`aid`) AS `acount` FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `tid`=?tid", array), 0);
		}
		public DataTable GetAttachmentListByPid(int pid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid)
			};
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `pid`=?pid", array);
			DataTable result;
			if (dataSet != null)
			{
				if (dataSet.Tables.Count > 0)
				{
					result = dataSet.Tables[0];
					return result;
				}
			}
			result = new DataTable();
			return result;
		}
		public DataTable GetAttachmentType()
		{
			DataTable result = new DataTable();
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT `id`, `extension`, `maxsize` FROM `{0}attachtypes`", BaseConfigs.get_GetTablePrefix()));
			if (dataSet != null)
			{
				result = dataSet.Tables[0];
			}
			return result;
		}
		public void UpdateAttachmentDownloads(int aid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?aid", DbType.Boolean, 4, aid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}attachments` SET `downloads`=`downloads`+1 WHERE `aid`=?aid", BaseConfigs.get_GetTablePrefix()), array);
		}
		public int UpdateTopicAttachment(int tid, int hasAttachment)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}topics` SET `attachment`={1} WHERE `tid`=?tid", BaseConfigs.get_GetTablePrefix(), hasAttachment), array);
		}
		public IDataReader GetAttachmentListByTid(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT `aid`,`filename` FROM `{0}attachments` WHERE `tid`=?tid ", BaseConfigs.get_GetTablePrefix()), array);
		}
		public IDataReader GetAttachmentListByTid(string tidlist)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT `aid`,`filename` FROM `{0}attachments` WHERE `tid` IN ({1})", BaseConfigs.get_GetTablePrefix(), tidlist));
		}
		public int DeleteAttachmentByTid(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}attachments` WHERE `tid`=?tid ", BaseConfigs.get_GetTablePrefix()), array);
		}
		public int DeleteAttachmentByTid(string tidlist)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}attachments` WHERE `tid` IN ({1})", BaseConfigs.get_GetTablePrefix(), tidlist));
		}
		public int DeleteAttachment(int aid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?aid", DbType.Boolean, 4, aid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}attachments` WHERE `aid`=?aid", BaseConfigs.get_GetTablePrefix()), array);
		}
		public int DeleteAttachment(string aidList)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}attachments` WHERE `aid` IN ({1})", BaseConfigs.get_GetTablePrefix(), aidList));
		}
		public int UpdatePostAttachment(int pid, string postTableId, int hasAttachment)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}posts{1}` SET `attachment`={2} WHERE `pid`=?pid", BaseConfigs.get_GetTablePrefix(), postTableId, hasAttachment), array);
		}
		public int DeleteAttachmentByPid(int pid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}attachments` WHERE `pid`=?pid", BaseConfigs.get_GetTablePrefix()), array);
		}
		public int UpdateAttachment(AttachmentInfo attachmentInfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, DateTime.Parse(attachmentInfo.get_Postdatetime())), 
				DbHelper.MakeInParam("?readperm", DbType.Boolean, 4, attachmentInfo.get_Readperm()), 
				DbHelper.MakeInParam("?filename", DbType.Single, 100, attachmentInfo.get_Filename()), 
				DbHelper.MakeInParam("?description", DbType.Single, 100, attachmentInfo.get_Description()), 
				DbHelper.MakeInParam("?filetype", DbType.Single, 50, attachmentInfo.get_Filetype()), 
				DbHelper.MakeInParam("?filesize", DbType.Boolean, 4, attachmentInfo.get_Filesize()), 
				DbHelper.MakeInParam("?attachment", DbType.Single, 100, attachmentInfo.get_Attachment()), 
				DbHelper.MakeInParam("?downloads", DbType.Boolean, 4, attachmentInfo.get_Downloads()), 
				DbHelper.MakeInParam("?aid", DbType.Boolean, 4, attachmentInfo.get_Aid())
			};
			string text = string.Format("UPDATE `{0}attachments` SET `postdatetime` = ?postdatetime, `readperm` = ?readperm, `filename` = ?filename, `description` = ?description, `filetype` = ?filetype, `filesize` = ?filesize, `attachment` = ?attachment, `downloads` = ?downloads\r\n\t\t\t\t\t\t\t\t\t\t\tWHERE `aid`=?aid", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int UpdateAttachment(int aid, int readperm, string description)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?readperm", DbType.Boolean, 4, readperm), 
				DbHelper.MakeInParam("?description", DbType.Single, 100, description), 
				DbHelper.MakeInParam("?aid", DbType.Boolean, 4, aid)
			};
			string text = string.Format("UPDATE `{0}attachments `SET `readperm` = ?readperm, `description` = ?description WHERE `aid` = ?aid", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public IDataReader GetAttachmentList(string aidList)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT `aid`,`filename`,`tid`,`pid` FROM `{0}attachments` WHERE `aid` IN ({1})", BaseConfigs.get_GetTablePrefix(), aidList));
		}
		public IDataReader GetAttachmentListByPid(string pidList)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM `{0}attachments` WHERE `pid` IN ({1})", BaseConfigs.get_GetTablePrefix(), pidList));
		}
		public int GetUploadFileSizeByUserId(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT SUM(`filesize`) as `todaysize` FROM `{0}attachments` WHERE `uid`=?uid AND DATEDIFF(`postdatetime`,NOW())=0", BaseConfigs.get_GetTablePrefix()), array), 0);
		}
		public IDataReader GetFirstImageAttachByTid(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `tid`=?tid AND LEFT(`filetype`, 5)='image' ORDER BY `aid` LIMIT 1", array);
		}
		public DataSet GetAttchType()
		{
			string text = "Select * From `" + BaseConfigs.get_GetTablePrefix() + "attachtypes` Order BY `id` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text);
		}
		public string GetAttchTypeSql()
		{
			return "Select * From `" + BaseConfigs.get_GetTablePrefix() + "attachtypes` Order BY `id` ASC";
		}
		public void AddAttchType(string extension, string maxsize)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?extension", DbType.Single, 256, extension), 
				DbHelper.MakeInParam("?maxsize", DbType.Boolean, 4, maxsize)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "attachtypes` (`extension`, `maxsize`) VALUES (?extension,?maxsize)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateAttchType(string extension, string maxsize, int id)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?extension", DbType.Single, 256, extension), 
				DbHelper.MakeInParam("?maxsize", DbType.Boolean, 4, maxsize), 
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "attachtypes` SET `extension`=?extension ,`maxsize`=?maxsize Where `id`=?id";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void DeleteAttchType(string attchtypeidlist)
		{
			string text = string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"attachtypes` WHERE `id` IN (", 
				attchtypeidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public bool IsExistExtensionInAttachtypes(string extensionname)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?extension", DbType.Single, 256, extensionname)
			};
			string text = "Select * From `" + BaseConfigs.get_GetTablePrefix() + "attachtypes` WHERE `extension`=?extension LIMIT 1";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0].Rows.Count > 0;
		}
		public DataTable GetTitleForModeratormanagelogByModeratorname(string moderatorname)
		{
			string text = string.Concat(new string[]
			{
				"SELECT `title` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"moderatormanagelog` WHERE (moderatorname = '", 
				moderatorname, 
				"') AND (actions = 'DELETE')"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetTitleForModeratormanagelogByPostdatetime(DateTime startDateTime, DateTime endDateTime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?startDateTime", DbType.Int64, 8, startDateTime), 
				DbHelper.MakeInParam("?endDateTime", DbType.Int64, 8, endDateTime)
			};
			string text = "SELECT `title` FROM `" + BaseConfigs.get_GetTablePrefix() + "moderatormanagelog` WHERE (postdatetime >= ?startDateTime) AND (postdatetime<= ?endDateTime)AND (actions = 'DELETE')";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public DataTable GetTidForModeratormanagelogByPostdatetime(DateTime postDateTime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, postDateTime)
			};
			string text = "SELECT `tid` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `displayorder`=-1 AND `postdatetime`<=?postdatetime";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public string GetUnauditNewTopicSQL()
		{
			return "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `displayorder`=-2";
		}
		public void PassAuditNewTopic(string postTableName, string tidlist)
		{
			string text = string.Concat(new string[]
			{
				"UPDATE  `", 
				postTableName, 
				"`  SET `invisible`=0 WHERE `layer`=0  AND `tid` IN(", 
				tidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
			text = string.Concat(new string[]
			{
				"UPDATE  `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`  SET `displayorder`=0 WHERE `tid` IN(", 
				tidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"statistics` SET `totaltopic`=`totaltopic` + ", 
				tidlist.Split(new char[]
				{
					','
				}).Length
			}));
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"statistics` SET `totalpost`=`totalpost` + ", 
				tidlist.Split(new char[]
				{
					','
				}).Length
			}));
			foreach (DataRow dataRow in DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `tid` IN(", 
				tidlist, 
				") ORDER BY `tid` ASC"
			})).Tables[0].Rows)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` SET `topics` = `topics` + 1, `curtopics` = `curtopics` + 1, `posts`=`posts` + 1, `todayposts`=(IF(DATEPART(yyyy, lastpost)=DATEPART(yyyy,NOW()) AND DATEPART(mm, lastpost)=DATEPART(mm,NOW()) AND DATEPART(dd, lastpost)=DATEPART(dd,NOW()),`todayposts`*1 + 1,1)),`lasttid`=", 
					dataRow["tid"].ToString(), 
					" ,\t`lasttitle`='", 
					dataRow["title"].ToString().Replace("'", "''"), 
					"',`lastpost`='", 
					dataRow["postdatetime"].ToString(), 
					"',`lastposter`='", 
					dataRow["poster"].ToString().Replace("'", "''"), 
					"',`lastposterid`=", 
					dataRow["posterid"].ToString(), 
					" WHERE `fid`=", 
					dataRow["fid"].ToString()
				}));
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users` SET `lastpost` = '", 
					dataRow["postdatetime"].ToString(), 
					"', `lastpostid` =", 
					dataRow["posterid"].ToString(), 
					", `lastposttitle` ='", 
					dataRow["title"].ToString().Replace("'", "''"), 
					"', `posts` = `posts` + 1\tWHERE `uid` = ", 
					dataRow["posterid"].ToString()
				}));
			}
		}
		public DataTable GetDetachTableId()
		{
			string text = "SELECT ID FROM `" + BaseConfigs.get_GetTablePrefix() + "tablelist` Order BY ID ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetCurrentPostTableRecordCount(int currentPostTableId)
		{
			string text = string.Concat(new object[]
			{
				"SELECT count(pid) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				currentPostTableId, 
				"` WHERE `invisible`=1"
			});
			return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, text));
		}
		public string GetUnauditPostSQL(int currentPostTableId)
		{
			return string.Concat(new object[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				currentPostTableId, 
				"` WHERE `invisible`=1 AND `layer`>0"
			});
		}
		public void PassPost(int currentPostTableId, string pidlist)
		{
			string text = string.Concat(new object[]
			{
				"UPDATE  `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				currentPostTableId, 
				"`  SET `invisible`=0 WHERE `pid` IN(", 
				pidlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"statistics` SET `totalpost`=`totalpost` + ", 
				pidlist.Split(new char[]
				{
					','
				}).Length
			}));
			foreach (DataRow dataRow in DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				currentPostTableId, 
				"` WHERE `pid` IN(", 
				pidlist, 
				") ORDER BY `pid` ASC"
			})).Tables[0].Rows)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` SET `posts`=`posts` + 1, `todayposts`=(IF((DATE_FORMAT(lastpost,'%Y')=DATE_FORMAT(now(),'%Y') AND DATE_FORMAT(lastpost,'%m')=DATE_FORMAT(Now(),'%m') AND DATE_FORMAT(lastpost,'%d')=DATE_FORMAT(now(),'%d'),`todayposts`*1 + 1,1)),`lastpost`='", 
					dataRow["postdatetime"].ToString(), 
					"',`lastposter`='", 
					dataRow["poster"].ToString().Replace("'", "''"), 
					"',`lastposterid`=", 
					dataRow["posterid"].ToString(), 
					" WHERE `fid`=", 
					dataRow["fid"].ToString()
				}));
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users` SET `lastpost` = '", 
					dataRow["postdatetime"].ToString(), 
					"', `lastpostid` =", 
					dataRow["posterid"].ToString(), 
					", `lastposttitle` ='", 
					dataRow["title"].ToString().Replace("'", "''"), 
					"', `posts` = `posts` + 1\tWHERE `uid` = ", 
					dataRow["posterid"].ToString()
				}));
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics`  SET `replies`=`replies`+1,`lastposter`='", 
					dataRow["poster"].ToString().Replace("'", "''"), 
					"',`lastposterid`=", 
					dataRow["posterid"].ToString(), 
					",`lastpost`='", 
					dataRow["postdatetime"].ToString(), 
					"' WHERE `tid`=", 
					dataRow["tid"].ToString()
				}));
			}
		}
		public DataTable GetPostLayer(int currentPostTableId, int postid)
		{
			string text = string.Concat(new object[]
			{
				"SELECT `layer`,`tid`  FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				currentPostTableId, 
				"` WHERE `pid`=", 
				postid.ToString(), 
				" LIMIT 1"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void UpdateBBCCode(int available, string tag, string icon, string replacement, string example, string explanation, string param, string nest, string paramsdescript, string paramsdefvalue, int id)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?available", DbType.Boolean, 4, available), 
				DbHelper.MakeInParam("?tag", DbType.Single, 100, tag), 
				DbHelper.MakeInParam("?icon", DbType.Single, 50, icon), 
				DbHelper.MakeInParam("?replacement", DbType.Single, 0, replacement), 
				DbHelper.MakeInParam("?example", DbType.Single, 255, example), 
				DbHelper.MakeInParam("?explanation", DbType.Single, 0, explanation), 
				DbHelper.MakeInParam("?params", DbType.Boolean, 4, param), 
				DbHelper.MakeInParam("?nest", DbType.Boolean, 4, nest), 
				DbHelper.MakeInParam("?paramsdescript", DbType.Single, 0, paramsdescript), 
				DbHelper.MakeInParam("?paramsdefvalue", DbType.Single, 0, paramsdefvalue), 
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "bbcodes` SET `available`=?available,tag=?tag, icon=?icon,replacement=?replacement,example=?example,explanation=?explanation,params=?params,nest=?nest,paramsdescript=?paramsdescript,paramsdefvalue=?paramsdefvalue WHERE `id`=?id";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataTable GetBBCode()
		{
			string text = "Select * From `" + BaseConfigs.get_GetTablePrefix() + "bbcodes` Order BY `id` ASC";
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetBBCode(int id)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?id", DbType.Boolean, 4, id);
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "bbcodes` WHERE `id`=?id", new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public void DeleteBBCode(string idlist)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"bbcodes`  WHERE `id` IN(", 
				idlist, 
				")"
			}));
		}
		public void SetBBCodeAvailableStatus(string idlist, int status)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?status", DbType.Boolean, 4, status)
			};
			string text = string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"bbcodes` SET `available`=?status  WHERE `id` IN(", 
				idlist, 
				")"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public DataSet GetBBCCodeById(int id)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?id", DbType.Boolean, 4, id)
			};
			string text = "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "bbcodes` WHERE `id`=?id";
			return DbHelper.ExecuteDataset(CommandType.Text, text, array);
		}
		public DataTable GetFocusTopicList(int count, int views, int fid, string starttime, string orderfieldname, string visibleForum, bool isdigest, bool onlyimg)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("@starttime", DbType.Int64, 8, DateTime.Parse(starttime));
			string str = "";
			if (isdigest)
			{
				str = " AND [t].[digest] > 0";
			}
			string str2 = "";
			if (fid > 0)
			{
				str2 = string.Concat(new object[]
				{
					" AND ([t].[fid] = ", 
					fid, 
					" OR CHARINDEX(',", 
					fid, 
					",' , ',' + RTRIM([f].[parentidlist]) + ',') > 0 ) "
				});
			}
			if (count < 0)
			{
				count = 0;
			}
			string text = "";
			if (onlyimg)
			{
				text = "AND [t].[attachment]=2";
			}
			if (visibleForum != string.Empty)
			{
				visibleForum = " AND [t].[fid] IN (" + visibleForum + ")";
			}
			string text2 = "SELECT TOP {0} [t].*, [f].[name] FROM [{1}topics] AS [t] LEFT JOIN [" + BaseConfigs.get_GetTablePrefix() + "forums] AS [f] ON [t].[fid] = [f].[fid] WHERE [t].[closed]<>1 AND  [t].[displayorder] >=0 AND [t].[views] > {2} AND [t].[postdatetime] > @starttime{3}{4} ORDER BY [t].[{5}] DESC";
			text2 = string.Format(text2, new object[]
			{
				count, 
				BaseConfigs.get_GetTablePrefix(), 
				views, 
				str2 + str + visibleForum, 
				text, 
				orderfieldname
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text2, new DbParameter[]
			{
				dbParameter
			}).Tables[0];
		}
		public void UpdateTopicLastPoster(int lastposterid, string lastposter)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastposter", DbType.Single, 20, lastposter), 
				DbHelper.MakeInParam("?lastposterid", DbType.Boolean, 4, lastposterid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `lastposter`=?lastposter  WHERE `lastposterid`=?lastposterid", array);
		}
		public void UpdateTopicPoster(int posterid, string poster)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, posterid), 
				DbHelper.MakeInParam("?poster", DbType.Single, 20, poster)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `poster`=?poster WHERE `posterid`=?posterid", array);
		}
		public void UpdatePostPoster(int posterid, string poster, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, posterid), 
				DbHelper.MakeInParam("?poster", DbType.Single, 20, poster)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` SET `poster`=?poster WHERE `posterid`=?posterid"
			}), array);
		}
		public bool UpdateTopicAllInfo(TopicInfo topicinfo)
		{
			string arg_16E_0 = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET fid='{1}',iconid='{2}',typeid='{3}',readperm='{4}',price='{5}',poster='{6}',title='{7}',postdatetime='{8}',lastpost='{9}',lastpostid='{10}',lastposter='{11}',views='{12}',replies='{13}',displayorder='{14}',highlight='{15}',digest='{16}',rate='{17}',blog='{18}',poll='{19}',attachment='{20}',moderated='{21}',closed='{22}' WHERE `tid`={0}";
			object[] array = new object[22];
			object[] arg_2D_0 = array;
			int arg_2D_1 = 0;
			int num = topicinfo.get_Tid();
			arg_2D_0[arg_2D_1] = num.ToString();
			object[] arg_3E_0 = array;
			int arg_3E_1 = 1;
			num = topicinfo.get_Fid();
			arg_3E_0[arg_3E_1] = num.ToString();
			object[] arg_4F_0 = array;
			int arg_4F_1 = 2;
			num = topicinfo.get_Iconid();
			arg_4F_0[arg_4F_1] = num.ToString();
			object[] arg_60_0 = array;
			int arg_60_1 = 3;
			num = topicinfo.get_Typeid();
			arg_60_0[arg_60_1] = num.ToString();
			object[] arg_71_0 = array;
			int arg_71_1 = 4;
			num = topicinfo.get_Readperm();
			arg_71_0[arg_71_1] = num.ToString();
			array[5] = topicinfo.get_Price();
			array[6] = topicinfo.get_Poster();
			array[7] = topicinfo.get_Title();
			array[8] = topicinfo.get_Postdatetime();
			array[9] = topicinfo.get_Lastpost();
			object[] arg_B6_0 = array;
			int arg_B6_1 = 10;
			num = topicinfo.get_Lastpostid();
			arg_B6_0[arg_B6_1] = num.ToString();
			array[11] = topicinfo.get_Lastposter();
			object[] arg_D2_0 = array;
			int arg_D2_1 = 12;
			num = topicinfo.get_Views();
			arg_D2_0[arg_D2_1] = num.ToString();
			object[] arg_E4_0 = array;
			int arg_E4_1 = 13;
			num = topicinfo.get_Replies();
			arg_E4_0[arg_E4_1] = num.ToString();
			object[] arg_F6_0 = array;
			int arg_F6_1 = 14;
			num = topicinfo.get_Displayorder();
			arg_F6_0[arg_F6_1] = num.ToString();
			array[15] = topicinfo.get_Highlight();
			object[] arg_112_0 = array;
			int arg_112_1 = 16;
			num = topicinfo.get_Digest();
			arg_112_0[arg_112_1] = num.ToString();
			object[] arg_124_0 = array;
			int arg_124_1 = 17;
			num = topicinfo.get_Rate();
			arg_124_0[arg_124_1] = num.ToString();
			object[] arg_136_0 = array;
			int arg_136_1 = 18;
			num = topicinfo.get_Hide();
			arg_136_0[arg_136_1] = num.ToString();
			object[] arg_148_0 = array;
			int arg_148_1 = 19;
			num = topicinfo.get_Attachment();
			arg_148_0[arg_148_1] = num.ToString();
			object[] arg_15A_0 = array;
			int arg_15A_1 = 20;
			num = topicinfo.get_Moderated();
			arg_15A_0[arg_15A_1] = num.ToString();
			object[] arg_16C_0 = array;
			int arg_16C_1 = 21;
			num = topicinfo.get_Closed();
			arg_16C_0[arg_16C_1] = num.ToString();
			string text = string.Format(arg_16E_0, array);
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
			return true;
		}
		public bool DeleteTopicByTid(int tid, string posttablename)
		{
			MySqlConnection mySqlConnection = new MySqlConnection(DbHelper.get_ConnectionString());
			mySqlConnection.Open();
			using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
			{
				try
				{
					DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `tid`=" + tid.ToString());
					DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "favorites` WHERE `tid`=" + tid.ToString());
					DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "polls` WHERE `tid`=" + tid.ToString());
					DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + posttablename + "` WHERE `tid`=" + tid.ToString());
					DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `tid`=" + tid.ToString());
					mySqlTransaction.Commit();
				}
				catch (Exception ex)
				{
					mySqlTransaction.Rollback();
					throw ex;
				}
			}
			mySqlConnection.Close();
			return true;
		}
		public bool SetTypeid(string topiclist, int value)
		{
			MySqlConnection mySqlConnection = new MySqlConnection(DbHelper.get_ConnectionString());
			mySqlConnection.Open();
			using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
			{
				try
				{
					DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` SET `typeid`=", 
						value.ToString(), 
						" WHERE `tid` IN(", 
						topiclist, 
						")"
					}));
					mySqlTransaction.Commit();
				}
				catch (Exception ex)
				{
					mySqlTransaction.Rollback();
					throw ex;
				}
			}
			mySqlConnection.Close();
			return true;
		}
		public DataSet GetPosts(int tid, int pagesize, int pageindex, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?posttablename", DbType.Single, 30, posttablename)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `", 
				posttablename, 
				"`.`pid`,`", 
				posttablename, 
				"`.`fid`,`", 
				posttablename, 
				"`.`title`,`", 
				posttablename, 
				"`.`layer`,`", 
				posttablename, 
				"`.`message`,`", 
				posttablename, 
				"`.`ip`,`", 
				posttablename, 
				"`.`lastedit`,`", 
				posttablename, 
				"`.`postdatetime`,`", 
				posttablename, 
				"`.`attachment`,`", 
				posttablename, 
				"`.`poster`,`", 
				posttablename, 
				"`.`posterid`,`", 
				posttablename, 
				"`.`invisible`,`", 
				posttablename, 
				"`.`usesig`,`", 
				posttablename, 
				"`.`htmlon`,`", 
				posttablename, 
				"`.`smileyoff`,`", 
				posttablename, 
				"`.`parseurloff`,`", 
				posttablename, 
				"`.`bbcodeoff`,`", 
				posttablename, 
				"`.`rate`,`", 
				posttablename, 
				"`.`ratetimes`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`nickname`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`username`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`groupid`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`spaceid`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`email`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`showemail`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`digestposts`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`credits`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits1`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits2`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits3`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits4`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits5`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits6`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits7`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`extcredits8`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`posts`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`joindate`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`onlinestate`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`lastactivity`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`invisible`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`gender`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`bday`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`avatar`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`avatarwidth`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`avatarheight`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`medals`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`sightml` AS signature,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`location`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`customstatus`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`website`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`icq`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`qq`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`msn`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`yahoo`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`skype`FROM `", 
				posttablename, 
				"` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`=`", 
				posttablename, 
				"`.`posterid` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`uid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid` WHERE `", 
				posttablename, 
				"`.`tid`=?pid AND `", 
				posttablename, 
				"`.`invisible`=0 ORDER BY `", 
				posttablename, 
				"`.`pid` LIMIT ", 
				((pageindex - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text, array);
		}
		public int GetAttachCount(int pid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 0, pid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(`aid`) AS `aidcount` FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `pid` = ?pid", array), 0);
		}
		public bool SetDisplayorder(string topiclist, int value)
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `displayorder`=", 
				value.ToString(), 
				" WHERE `tid` IN(", 
				topiclist, 
				")"
			}));
			return true;
		}
		public int InsertRateLog(int pid, int userid, string username, int extid, float score, string reason)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, Utils.StrToFloat(pid, 0f)), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userid), 
				DbHelper.MakeInParam("?username", (DbType)254, 15, username), 
				DbHelper.MakeInParam("?extcredits", DbType.Byte, 1, extid), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, DateTime.Now), 
				DbHelper.MakeInParam("?score", DbType.Byte, 6, score), 
				DbHelper.MakeInParam("?reason", DbType.Single, 20, reason)
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "ratelog` (`pid`,`uid`,`username`,`extcredits`,`postdatetime`,`score`,`reason`) VALUES (?pid,?uid,?username,?extcredits,?postdatetime,?score,?reason)";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public bool DeleteRateLog()
		{
			bool result;
			try
			{
				if (DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM [" + BaseConfigs.get_GetTablePrefix() + "ratelog] ") > 1)
				{
					result = true;
					return result;
				}
			}
			catch
			{
			}
			result = false;
			return result;
		}
		public bool DeleteRateLog(string condition)
		{
			bool result;
			try
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "ratelog` WHERE " + condition);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public DataTable RateLogList(int pagesize, int currentpage, string posttablename)
		{
			string text = string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"ratelog` Order by `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			text = string.Concat(new string[]
			{
				"SELECT r.*,p.title As title,p.poster As poster , p.posterid As posterid,  ug.grouptitle As grouptitle FROM (((", 
				text, 
				") as r LEFT JOIN `", 
				posttablename, 
				"` as p ON r.pid = p.pid) LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` as u ON u.uid = r.uid) LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` as ug ON ug.groupid = u.groupid"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable RateLogList(int pagesize, int currentpage, string posttablename, string condition)
		{
			string text = string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"ratelog` WHERE ", 
				condition, 
				"  Order by `id` DESC LIMIT ", 
				((currentpage - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			text = string.Concat(new string[]
			{
				"SELECT r.*,p.title As title,p.poster As poster , p.posterid As posterid,  ug.grouptitle As grouptitle FROM (((", 
				text, 
				") r LEFT JOIN `", 
				posttablename, 
				"` p ON r.pid = p.pid) LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` u ON u.uid = r.uid) LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"usergroups` ug ON ug.groupid = u.groupid"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public int GetRateLogCount()
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "ratelog`").Tables[0].Rows[0][0].ToString());
		}
		public int GetRateLogCount(string condition)
		{
			return Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, "SELECT count(id) FROM `" + BaseConfigs.get_GetTablePrefix() + "ratelog` WHERE " + condition).Tables[0].Rows[0][0].ToString());
		}
		public int GetPostsCount(string posttableid)
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new string[]
			{
				"SELECT COUNT(`pid`) AS `portscount` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"`"
			})), 0);
		}
		public IDataReader GetMaxAndMinTid(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT MAX(`tid`) AS `maxtid`,MIN(`tid`) AS `mintid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `fid` IN (SELECT `fid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `fid`=?fid OR (InStr(concat(',',RTRIM(?fid),','), concat(',',RTRIM(parentidlist),',')) > 0))"
			}), array);
		}
		public int GetPostCount(int fid, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(pid) AS `postcount` FROM `" + posttablename + "` WHERE `fid` = ?fid", array), 0);
		}
		public int GetPostCountByTid(int tid, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(`pid`) AS `postcount` FROM `" + posttablename + "` WHERE `tid` = ?tid AND `layer` <> 0", array), 0);
		}
		public int GetPostCount(string posttableid, int tid, int posterid)
		{
			string arg = string.Format("{0}posts{1}", BaseConfigs.get_GetTablePrefix(), posttableid);
			string text = string.Format("`{0}`.`tid`={1} AND `{0}`.`posterid`={2}", arg, tid, posterid);
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?condition", DbType.Single, 100, text), 
				DbHelper.MakeInParam("?posttablename", DbType.Single, 20, string.Format("{0}posts{1}", BaseConfigs.get_GetTablePrefix(), posttableid))
			};
			string text2 = "SELECT COUNT(pid) FROM `" + string.Format("{0}posts{1}", BaseConfigs.get_GetTablePrefix(), posttableid) + "` WHERE ?condition AND `layer`>=0";
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text2, array), 0);
		}
		public int GetTodayPostCount(int fid, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(pid) AS `postcount` FROM `" + posttablename + "` WHERE `fid` = ?fid AND DATEDIFF(`postdatetime`, NOW()) = 0 ", array), 0);
		}
		public int GetPostCount(int fid, int posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new object[]
			{
				"SELECT COUNT(pid) AS `postcount` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE  `fid` IN (SELECT `fid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `fid`=?fid OR (instr(concat(',',RTRIM(parentidlist),','),concat(',',RTRIM(?fid),',')) > 0))"
			}), array), 0);
		}
		public int GetTodayPostCount(int fid, int posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new object[]
			{
				"SELECT COUNT(pid) AS `postcount` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE  `fid` IN (SELECT `fid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `fid`=?fid OR (instr(concat(',',RTRIM(parentidlist),','),concat(',',RTRIM(?fid),',')) > 0)) AND DATEDIFF(`postdatetime`, NOW()) = 0 "
			}), array), 0);
		}
		public IDataReader GetMaxAndMinTidByUid(int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT MAX(`tid`) AS `maxtid`,MIN(`tid`) AS `mintid` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `posterid` = ?uid", array);
		}
		public int GetPostCountByUid(int uid, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return Math.Abs(Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(pid) AS `postcount` FROM `" + posttablename + "` WHERE `posterid` = ?uid", array), 0));
		}
		public int GetTodayPostCountByUid(int uid, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(pid) AS `postcount` FROM `" + posttablename + "` WHERE `posterid` = ?uid AND DATEDIFF(`postdatetime`, NOW()) = 0 ", array), 0);
		}
		public int GetTopicsCount()
		{
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(`tid`) AS `topicscount` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics`"), 0);
		}
		public void ReSetStatistic(int UserCount, int TopicsCount, int PostCount, string lastuserid, string lastusername)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?totaltopic", DbType.Boolean, 4, TopicsCount), 
				DbHelper.MakeInParam("?totalpost", DbType.Boolean, 4, PostCount), 
				DbHelper.MakeInParam("?totalusers", DbType.Boolean, 4, UserCount), 
				DbHelper.MakeInParam("?lastusername", DbType.Single, 20, lastusername), 
				DbHelper.MakeInParam("?lastuserid", DbType.Boolean, 4, Utils.StrToInt(lastuserid, 0))
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET `totaltopic`=?totaltopic,`totalpost`=?totalpost,`totalusers`=?totalusers,`lastusername`=?lastusername,`lastuserid`=?lastuserid", array);
		}
		public IDataReader GetTopicTids(int statcount, int lasttid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lasttid", DbType.Boolean, 4, lasttid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `tid` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `tid` > ?lasttid ORDER BY `tid` LIMIT " + statcount.ToString(), array);
		}
		public IDataReader GetLastPost(int tid, int posttableid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid);
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new object[]
			{
				"SELECT `pid`, `postdatetime`, `posterid`, `poster` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE `tid` = ?tid ORDER BY `pid` DESC LIMIT 1"
			}), new DbParameter[]
			{
				dbParameter
			});
		}
		public void UpdateTopic(int tid, int postcount, int lastpostid, string lastpost, int lastposterid, string poster)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastpostid", DbType.Boolean, 4, lastpostid), 
				DbHelper.MakeInParam("?lastpost", DbType.Single, 20, lastpost), 
				DbHelper.MakeInParam("?lastposterid", DbType.Boolean, 4, lastposterid), 
				DbHelper.MakeInParam("?lastposter", DbType.Single, 20, poster), 
				DbHelper.MakeInParam("?postcount", DbType.Boolean, 4, postcount), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `lastpost`=?lastpost, `lastposterid`=?lastposterid, `lastposter`=?lastposter, `replies`=?postcount WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`tid` = ?tid"
			}), array);
		}
		public void UpdateTopicLastPosterId(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `lastposterid`=(SELECT  IFNULL(MIN(`lastpostid`),-1)-1 FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`tid` = ?tid"
			}), array);
		}
		public IDataReader GetTopics(int start_tid, int end_tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?start_tid", DbType.Boolean, 4, start_tid), 
				DbHelper.MakeInParam("?end_tid", DbType.Boolean, 4, end_tid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `tid` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `tid` >= ?start_tid AND `tid`<=?end_tid  ORDER BY `tid`", array);
		}
		public IDataReader GetForumLastPost(int fid, string posttablename, int topiccount, int postcount, int lasttid, string lasttitle, string lastpost, int lastposterid, string lastposter, int todaypostcount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastfid", DbType.Boolean, 4, fid), 
				DbHelper.MakeInParam("?topiccount", DbType.Boolean, 4, topiccount), 
				DbHelper.MakeInParam("?postcount", DbType.Boolean, 4, postcount), 
				DbHelper.MakeInParam("?lasttid", DbType.Boolean, 4, lasttid), 
				DbHelper.MakeInParam("?lasttitle", DbType.Single, 80, lasttitle), 
				DbHelper.MakeInParam("?lastpost", DbType.Single, 20, lastpost), 
				DbHelper.MakeInParam("?lastposterid", DbType.Boolean, 4, lastposterid), 
				DbHelper.MakeInParam("?lastposter", DbType.Single, 20, lastposter), 
				DbHelper.MakeInParam("?todaypostcount", DbType.Boolean, 4, todaypostcount)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `tid`, `title`, `postdatetime`, `posterid`, `poster` FROM `" + posttablename + "` WHERE `fid` = ?lastfid ORDER BY `pid` DESC LIMIT 1", array);
		}
		public void UpdateForum(int fid, int topiccount, int postcount, int lasttid, string lasttitle, string lastpost, int lastposterid, string lastposter, int todaypostcount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?topiccount", DbType.Boolean, 4, topiccount), 
				DbHelper.MakeInParam("?postcount", DbType.Boolean, 4, postcount), 
				DbHelper.MakeInParam("?todaypostcount", DbType.Boolean, 4, todaypostcount), 
				DbHelper.MakeInParam("?lasttid", DbType.Boolean, 4, lasttid), 
				DbHelper.MakeInParam("?lasttitle", (DbType)253, 80, lasttitle), 
				DbHelper.MakeInParam("?lastpost", (DbType)253, 20, lastpost), 
				DbHelper.MakeInParam("?lastposterid", DbType.Boolean, 4, lastposterid), 
				DbHelper.MakeInParam("?lastposter", (DbType)253, 20, lastposter), 
				DbHelper.MakeInParam("?lastfid", DbType.Boolean, 4, fid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` SET `topics` = ?topiccount, `posts`=?postcount, `todayposts` = ?todaypostcount, `lasttid` = ?lasttid, `lasttitle` = ?lasttitle, `lastpost`=?lastpost, `lastposterid` = ?lastposterid, `lastposter`=?lastposter WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`.`fid` = ?lastfid"
			}), array);
		}
		public IDataReader GetForums(int start_fid, int end_fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?start_fid", DbType.Boolean, 4, start_fid), 
				DbHelper.MakeInParam("?end_fid", DbType.Boolean, 4, end_fid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT  `fid` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` WHERE `fid` >= ?start_fid AND `fid`<=?end_fid", array);
		}
		public void ReSetClearMove()
		{
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `closed` > 1");
		}
		public IDataReader GetLastPostByFid(int fid, string posttablename)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid);
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `tid`, `title`, `postdatetime`, `posterid`, `poster` FROM `" + posttablename + "` WHERE `fid` = ?fid ORDER BY `pid` DESC LIMIT 1", new DbParameter[]
			{
				dbParameter
			});
		}
		public int CreatePoll(PollInfo pollinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, pollinfo.get_Tid()), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, pollinfo.get_Displayorder()), 
				DbHelper.MakeInParam("?multiple", DbType.Boolean, 4, pollinfo.get_Multiple()), 
				DbHelper.MakeInParam("?visible", DbType.Boolean, 4, pollinfo.get_Visible()), 
				DbHelper.MakeInParam("?maxchoices", DbType.Boolean, 4, pollinfo.get_Maxchoices()), 
				DbHelper.MakeInParam("?expiration", DbType.Int64, 8, pollinfo.get_Expiration()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, pollinfo.get_Uid()), 
				DbHelper.MakeInParam("?voternames", (DbType)752, 0, pollinfo.get_Voternames())
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "polls` ( `tid` ,`displayorder` ,`multiple` ,`visible` ,`maxchoices` ,`expiration` ,`uid` ,`voternames` ) VALUES (?tid, ?displayorder, ?multiple, ?visible, ?maxchoices, ?expiration, ?uid, ?voternames);";
			int result;
			DbHelper.ExecuteNonQuery(ref result, CommandType.Text, text, array);
			return result;
		}
		public int CreatePollOption(PollOptionInfo polloptioninfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, polloptioninfo.get_Tid()), 
				DbHelper.MakeInParam("?pollid", DbType.Boolean, 4, polloptioninfo.get_Pollid()), 
				DbHelper.MakeInParam("?votes", DbType.Boolean, 4, polloptioninfo.get_Votes()), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, polloptioninfo.get_Displayorder()), 
				DbHelper.MakeInParam("?polloption", (DbType)253, 80, polloptioninfo.get_Polloption()), 
				DbHelper.MakeInParam("?voternames", (DbType)752, 0, polloptioninfo.get_Voternames())
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "polloptions` (`tid` ,`pollid` ,`votes` ,`displayorder` ,`polloption` ,`voternames` ) VALUES (?tid, ?pollid, ?votes, ?displayorder, ?polloption, ?voternames);";
			int result;
			DbHelper.ExecuteNonQuery(ref result, CommandType.Text, text, array);
			return result;
		}
		public IDataReader GetPollAndOptions(int tid)
		{
			string text = string.Format("SELECT `poll`.`multiple`, `poll`.`maxchoices`, `poll`.`expiration`, `poll`.`pollid`, `options`.`polloptionid`, `options`.`votes`, `options`.`polloption`, `options`.`voternames` FROM `{0}polls` AS `poll` LEFT JOIN `{0}polloptions` AS `options` ON `poll`.`tid` = `options`.`tid` WHERE `tid`={1}", BaseConfigs.get_GetTablePrefix(), tid);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public bool UpdatePoll(PollInfo pollinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, pollinfo.get_Tid()), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, pollinfo.get_Displayorder()), 
				DbHelper.MakeInParam("?multiple", DbType.Boolean, 4, pollinfo.get_Multiple()), 
				DbHelper.MakeInParam("?visible", DbType.Boolean, 4, pollinfo.get_Visible()), 
				DbHelper.MakeInParam("?maxchoices", DbType.Boolean, 4, pollinfo.get_Maxchoices()), 
				DbHelper.MakeInParam("?expiration", DbType.Int64, 8, pollinfo.get_Expiration()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, pollinfo.get_Uid()), 
				DbHelper.MakeInParam("?voternames", (DbType)752, 0, pollinfo.get_Voternames()), 
				DbHelper.MakeInParam("?pollid", DbType.Boolean, 4, pollinfo.get_Pollid())
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "polls` set `tid` = ?tid, `displayorder` = ?displayorder, `multiple` = ?multiple, `visible` = ?visible, `maxchoices` = ?maxchoices, `expiration` = ?expiration, `uid` = ?uid, `voternames` = ?voternames WHERE `pollid` = ?pollid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array) > 0;
		}
		public bool UpdatePollOption(PollOptionInfo polloptioninfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, polloptioninfo.get_Tid()), 
				DbHelper.MakeInParam("?pollid", DbType.Boolean, 4, polloptioninfo.get_Pollid()), 
				DbHelper.MakeInParam("?votes", DbType.Boolean, 4, polloptioninfo.get_Votes()), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, polloptioninfo.get_Displayorder()), 
				DbHelper.MakeInParam("?polloption", (DbType)253, 80, polloptioninfo.get_Polloption()), 
				DbHelper.MakeInParam("?voternames", (DbType)752, 0, polloptioninfo.get_Voternames()), 
				DbHelper.MakeInParam("?polloptionid", DbType.Boolean, 4, polloptioninfo.get_Polloptionid())
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "polloptions` set `tid` = ?tid, `pollid` = ?pollid, `votes` = ?votes, `displayorder` = ?displayorder, `polloption` = ?polloption, `voternames` = ?voternames WHERE `polloptionid` = ?polloptionid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array) > 0;
		}
		public bool DeletePollOption(PollOptionInfo polloptioninfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?polloptionid", DbType.Boolean, 4, polloptioninfo.get_Polloptionid())
			};
			string text = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "polloptions` WHERE `polloptionid` = ?polloptionid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array) > 0;
		}
		public IDataReader GetPollList(int tid)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM `{0}polls` WHERE `tid`={1}", BaseConfigs.get_GetTablePrefix(), tid));
		}
		public IDataReader GetPollOptionList(int tid)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM `{0}polloptions` WHERE `tid`={1}", BaseConfigs.get_GetTablePrefix(), tid));
		}
		public string GetPollUserNameList(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteScalarToStr(CommandType.Text, "SELECT  `voternames` FROM `" + BaseConfigs.get_GetTablePrefix() + "polls` WHERE `tid`=?tid limit 1", array);
		}
		public int GetPollType(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "SELECT `multiple` FROM `" + BaseConfigs.get_GetTablePrefix() + "polls` WHERE `tid`=?tid limit 1", array), 0);
		}
		public string GetPollEnddatetime(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return Utils.GetDate(DbHelper.ExecuteScalarToStr(CommandType.Text, "SELECT `expiration` FROM `" + BaseConfigs.get_GetTablePrefix() + "polls` WHERE `tid`=?tid limit 1", array), Utils.GetDate());
		}
		public DataSet GetAllPostTableName()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT * FROM `{0}tablelist` ORDER BY `id` DESC", BaseConfigs.get_GetTablePrefix()));
		}
		public int CreatePost(PostInfo postinfo, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Byte, 2, postinfo.get_Fid()), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, postinfo.get_Tid()), 
				DbHelper.MakeInParam("?parentid", DbType.Boolean, 4, postinfo.get_Parentid()), 
				DbHelper.MakeInParam("?layer", DbType.Boolean, 4, postinfo.get_Layer()), 
				DbHelper.MakeInParam("?poster", DbType.Single, 15, postinfo.get_Poster()), 
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, postinfo.get_Posterid()), 
				DbHelper.MakeInParam("?title", DbType.Single, 60, postinfo.get_Title()), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int16, 8, DateTime.Parse(postinfo.get_Postdatetime())), 
				DbHelper.MakeInParam("?message", (DbType)751, 0, postinfo.get_Message()), 
				DbHelper.MakeInParam("?ip", DbType.Single, 15, postinfo.get_Ip()), 
				DbHelper.MakeInParam("?lastedit", DbType.Single, 50, postinfo.get_Lastedit()), 
				DbHelper.MakeInParam("?invisible", DbType.Boolean, 4, postinfo.get_Invisible()), 
				DbHelper.MakeInParam("?usesig", DbType.Boolean, 4, postinfo.get_Usesig()), 
				DbHelper.MakeInParam("?htmlon", DbType.Boolean, 4, postinfo.get_Htmlon()), 
				DbHelper.MakeInParam("?smileyoff", DbType.Boolean, 4, postinfo.get_Smileyoff()), 
				DbHelper.MakeInParam("?bbcodeoff", DbType.Boolean, 4, postinfo.get_Bbcodeoff()), 
				DbHelper.MakeInParam("?parseurloff", DbType.Boolean, 4, postinfo.get_Parseurloff()), 
				DbHelper.MakeInParam("?attachment", DbType.Boolean, 4, postinfo.get_Attachment()), 
				DbHelper.MakeInParam("?rate", DbType.Byte, 2, postinfo.get_Rate()), 
				DbHelper.MakeInParam("?ratetimes", DbType.Boolean, 4, postinfo.get_Ratetimes())
			};
			string text = BaseConfigs.get_GetTablePrefix() + "posts" + posttableid;
			int num = 0;
			string text2 = "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "postid` WHERE FLOOR((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP('2007-08-08 10:52:01'))/60)>5";
			DbHelper.ExecuteNonQuery(CommandType.Text, text2);
			text2 = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "postid` (`postdatetime`) VALUES (now())";
			DbHelper.ExecuteNonQuery(ref num, CommandType.Text, text2);
			DbHelper.ExecuteNonQuery(CommandType.Text, text2, array);
			int num2 = num;
			text2 = string.Concat(new object[]
			{
				"INSERT INTO `", 
				text, 
				"`(`pid`, `fid`, `tid`, `parentid`, `layer`, `poster`, `posterid`, `title`, `postdatetime`, `message`, `ip`, `lastedit`, `invisible`, `usesig`, `htmlon`, `smileyoff`, `bbcodeoff`, `parseurloff`, `attachment`, `rate`, `ratetimes`) VALUES(", 
				num2, 
				", ?fid, ?tid, ?parentid, ?layer, ?poster, ?posterid, ?title, ?postdatetime, ?message, ?ip, ?lastedit, ?invisible, ?usesig, ?htmlon, ?smileyoff, ?bbcodeoff, ?parseurloff, ?attachment, ?rate, ?ratetimes)"
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, text2, array);
			if (postinfo.get_Parentid() == 0)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
				{
					"UPDATE `", 
					text, 
					"` SET `parentid`=", 
					num2, 
					" WHERE `pid`=", 
					num2
				}));
			}
			if (postinfo.get_Invisible() == 0)
			{
				text2 = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET `totalpost`=`totalpost` + 1";
				DbHelper.ExecuteNonQuery(CommandType.Text, text2);
				DbParameter[] array2 = new DbParameter[]
				{
					DbHelper.MakeInParam("?fid", DbType.Boolean, 2, postinfo.get_Fid())
				};
				string text3 = DbHelper.ExecuteDataset(CommandType.Text, "SELECT IFNULL(`parentidlist`,'') as `fidlist` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` WHERE `fid` = ?fid", array2).Tables[0].Rows[0][0].ToString();
				if (text3 != string.Empty)
				{
					string arg_3F7_0 = text3;
					string arg_3F7_1 = ",";
					int fid = postinfo.get_Fid();
					text3 = arg_3F7_0 + arg_3F7_1 + fid.ToString();
				}
				else
				{
					int fid = postinfo.get_Fid();
					text3 = fid.ToString();
				}
				DbParameter[] array3 = new DbParameter[]
				{
					DbHelper.MakeInParam("?tid", DbType.Boolean, 4, postinfo.get_Tid()), 
					DbHelper.MakeInParam("?title", DbType.Single, 60, postinfo.get_Title()), 
					DbHelper.MakeInParam("?postdatetime", (DbType)253, 0, postinfo.get_Postdatetime()), 
					DbHelper.MakeInParam("?poster", (DbType)253, 15, postinfo.get_Poster()), 
					DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, postinfo.get_Posterid())
				};
				text2 = string.Concat(new string[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` SET `posts`=`posts` + 1,`todayposts`=if( (Datediff(`lastpost`,now())=0),(`todayposts`*1 + 1),1),`lasttid`=?tid,`lasttitle`=?title,`lastpost`=?postdatetime,`lastposter`=?poster,`lastposterid`=?posterid WHERE (instr(',", 
					text3, 
					",',concat(',',fid,','))> 0)"
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text2, array3);
				DbParameter[] array4 = new DbParameter[]
				{
					DbHelper.MakeInParam("?postdatetime", (DbType)253, 0, postinfo.get_Postdatetime()), 
					DbHelper.MakeInParam("?title", (DbType)253, 60, postinfo.get_Title()), 
					DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, postinfo.get_Posterid())
				};
				text2 = string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users` SET `lastpost` = ?postdatetime,`lastpostid` = ", 
					num2, 
					",`lastposttitle` = ?title,`posts` = `posts` + 1, `lastactivity` = now() WHERE `uid` = ?posterid"
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text2, array4);
				if (postinfo.get_Layer() <= 0)
				{
					DbParameter[] array5 = new DbParameter[]
					{
						DbHelper.MakeInParam("?poster", (DbType)253, 15, postinfo.get_Poster()), 
						DbHelper.MakeInParam("?postdatetime", (DbType)253, 0, postinfo.get_Postdatetime()), 
						DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, postinfo.get_Posterid()), 
						DbHelper.MakeInParam("?tid", DbType.Boolean, 4, postinfo.get_Tid())
					};
					text2 = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `replies`=0,`lastposter`=?poster,`lastpost`=?postdatetime,`lastposterid`=?posterid WHERE `tid`=?tid";
					DbHelper.ExecuteNonQuery(CommandType.Text, text2, array5);
				}
				else
				{
					DbParameter[] array6 = new DbParameter[]
					{
						DbHelper.MakeInParam("?poster", (DbType)253, 15, postinfo.get_Poster()), 
						DbHelper.MakeInParam("?postdatetime", (DbType)253, 0, postinfo.get_Postdatetime()), 
						DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, postinfo.get_Posterid()), 
						DbHelper.MakeInParam("?tid", DbType.Boolean, 4, postinfo.get_Tid())
					};
					text2 = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `replies`=`replies` + 1,`lastposter`=?poster,`lastpost`=?postdatetime,`lastposterid`=?posterid WHERE `tid`=?tid";
					DbHelper.ExecuteNonQuery(CommandType.Text, text2, array6);
				}
				DbParameter[] array7 = new DbParameter[]
				{
					DbHelper.MakeInParam("?tid", DbType.Boolean, 4, postinfo.get_Tid())
				};
				text2 = string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics` SET `lastpostid`=", 
					num2, 
					" WHERE `tid`=?tid"
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text2, array7);
			}
			if (postinfo.get_Posterid() != -1)
			{
				DbParameter[] array8 = new DbParameter[]
				{
					DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, postinfo.get_Posterid()), 
					DbHelper.MakeInParam("?tid", DbType.Boolean, 4, postinfo.get_Tid()), 
					DbHelper.MakeInParam("?postdatetime", (DbType)253, 0, postinfo.get_Postdatetime())
				};
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
				{
					"INSERT into `", 
					BaseConfigs.get_GetTablePrefix(), 
					"myposts`(`uid`, `tid`, `pid`, `dateline`) VALUES(?posterid, ?tid, ", 
					num2, 
					", ?postdatetime)"
				}), array8);
			}
			return num2;
		}
		public int UpdatePost(PostInfo __postsInfo, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?title", DbType.Single, 160, __postsInfo.get_Title()), 
				DbHelper.MakeInParam("?message", DbType.Single, 0, __postsInfo.get_Message()), 
				DbHelper.MakeInParam("?lastedit", DbType.Single, 50, __postsInfo.get_Lastedit()), 
				DbHelper.MakeInParam("?invisible", DbType.Boolean, 4, __postsInfo.get_Invisible()), 
				DbHelper.MakeInParam("?usesig", DbType.Boolean, 4, __postsInfo.get_Usesig()), 
				DbHelper.MakeInParam("?htmlon", DbType.Boolean, 4, __postsInfo.get_Htmlon()), 
				DbHelper.MakeInParam("?smileyoff", DbType.Boolean, 4, __postsInfo.get_Smileyoff()), 
				DbHelper.MakeInParam("?bbcodeoff", DbType.Boolean, 4, __postsInfo.get_Bbcodeoff()), 
				DbHelper.MakeInParam("?parseurloff", DbType.Boolean, 4, __postsInfo.get_Parseurloff()), 
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, __postsInfo.get_Pid())
			};
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("UPDATE {0}posts{1} SET");
			stringBuilder.Append("`title`=?title,");
			stringBuilder.Append("`message`=?message,");
			stringBuilder.Append("`lastedit`=?lastedit,");
			stringBuilder.Append("`invisible`=?invisible,");
			stringBuilder.Append("`usesig`=?usesig,");
			stringBuilder.Append("`htmlon`=?htmlon,");
			stringBuilder.Append("`smileyoff`=?smileyoff,");
			stringBuilder.Append("`bbcodeoff`=?bbcodeoff,");
			stringBuilder.Append("`parseurloff`=?parseurloff WHERE `pid`=?pid");
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format(stringBuilder.ToString(), BaseConfigs.get_GetTablePrefix(), posttableid), array);
		}
		public int DeletePost(string posttableid, int pid, bool chanageposts)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			string text = "";
			int num4 = 0;
			string text2 = "";
			string text3 = "";
			string text4 = "";
			DataTable dataTable = new DataTable();
			text4 = string.Concat(new object[]
			{
				"SELECT `fid`, `tid`, `posterid`,`layer`, `postdatetime` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE pid =", 
				pid
			});
			DataRow dataRow = DbHelper.ExecuteDataset(CommandType.Text, text4).Tables[0].Rows[0];
			num = Convert.ToInt32(dataRow["fid"].ToString());
			int num5 = Convert.ToInt32(dataRow["tid"].ToString());
			num2 = Convert.ToInt32(dataRow["posterid"].ToString());
			int num6 = Convert.ToInt32(dataRow["layer"].ToString());
			DateTime dateTime = Convert.ToDateTime(dataRow["postdatetime"].ToString());
			text4 = string.Concat(new object[]
			{
				"SELECT if((`parentidlist` is null),'',`parentidlist`) as `fidlist` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `fid` =", 
				num
			});
			text3 = DbHelper.ExecuteScalarToStr(CommandType.Text, text4);
			if (text3 != "")
			{
				text3 = text3 + "," + num.ToString();
			}
			else
			{
				text3 = num.ToString();
			}
			if (num6 != 0)
			{
				text4 = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET `totalpost`=`totalpost` - 1";
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				DateTime dateTime2 = Convert.ToDateTime(dateTime);
				string arg_1D6_0 = dateTime2.ToShortDateString();
				dateTime2 = DateTime.Now;
				if (arg_1D6_0 == dateTime2.ToShortDateString())
				{
					text4 = string.Concat(new string[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forums` SET \t`posts`=`posts` - 1, `todayposts`=`todayposts`-1 WHERE `fid` in (", 
						text3, 
						")"
					});
				}
				else
				{
					text4 = string.Concat(new string[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forums` SET \t`posts`=`posts` - 1  WHERE `fid` in (", 
						text3, 
						")"
					});
				}
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				text4 = string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users` SET  `posts` = `posts`-1 WHERE `uid` =", 
					num2
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				text4 = string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics` SET `replies`=`replies` - 1 WHERE `tid`=", 
					num5
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				text4 = string.Concat(new object[]
				{
					"DELETE FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					posttableid, 
					"` WHERE `pid`=", 
					pid
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
			}
			else
			{
				text4 = string.Concat(new object[]
				{
					"SELECT COUNT(`pid`) FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					posttableid, 
					"` WHERE `tid` = ", 
					num5
				});
				num4 = Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, text4).Tables[0].Rows[0][0].ToString());
				text4 = string.Concat(new object[]
				{
					"SELECT COUNT(`pid`) FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					posttableid, 
					"` WHERE `tid` =", 
					num5, 
					"  AND DATEDIFF(now(), `postdatetime`) = 0"
				});
				int num7 = Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, text4).Tables[0].Rows[0][0].ToString());
				text4 = string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"statistics` SET `totaltopic`=`totaltopic` - 1, `totalpost`=`totalpost` -", 
					num4
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				text4 = string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` SET `posts`=`posts` -", 
					num4, 
					", `topics`=`topics` - 1,`todayposts`=`todayposts` -", 
					num7, 
					" WHERE `fid` in (", 
					text3, 
					")"
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				text4 = string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"users` SET `posts` = `posts` - ", 
					num4, 
					" WHERE `uid` = ", 
					num2
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				text4 = string.Concat(new object[]
				{
					"DELETE FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					posttableid, 
					"` WHERE `tid` = ", 
					num5
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				text4 = string.Concat(new object[]
				{
					"DELETE FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics` WHERE `tid` = ", 
					num5
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text4);
			}
			if (num6 != 0)
			{
				text4 = string.Concat(new object[]
				{
					"SELECT `pid`, `posterid`,`postdatetime`, `title`, `poster` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					posttableid, 
					"` WHERE `tid`=", 
					num5, 
					" ORDER BY `pid` DESC LIMIT 1"
				});
				dataTable = DbHelper.ExecuteDataset(CommandType.Text, text4).Tables[0];
				if (dataTable.Rows.Count > 0)
				{
					dataRow = dataTable.Rows[0];
					pid = Convert.ToInt32(dataRow["pid"].ToString());
					num2 = Convert.ToInt32(dataRow["posterid"].ToString());
					dateTime = Convert.ToDateTime(dataRow["postdatetime"].ToString());
					text2 = dataRow["title"].ToString();
					text = dataRow["poster"].ToString();
					text4 = string.Concat(new object[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` SET `lastposter`='", 
						text, 
						"',`lastpost`='", 
						dateTime.ToString(), 
						"',`lastpostid`=", 
						pid, 
						",`lastposterid`=", 
						num2, 
						" WHERE `tid`=", 
						num5
					});
					DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				}
			}
			text4 = string.Concat(new object[]
			{
				"SELECT `lasttid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `fid` =", 
				num
			});
			int num8 = Convert.ToInt32(DbHelper.ExecuteDataset(CommandType.Text, text4).Tables[0].Rows[0][0].ToString());
			if (num8 == num5)
			{
				text4 = string.Concat(new object[]
				{
					"SELECT `pid`, `tid`,`posterid`, `title`, `poster`, `postdatetime` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					posttableid, 
					"` WHERE `fid` = ", 
					num, 
					" ORDER BY `pid` DESC LIMIT 1"
				});
				dataTable = DbHelper.ExecuteDataset(CommandType.Text, text4).Tables[0];
				if (dataTable.Rows.Count > 0)
				{
					dataRow = dataTable.Rows[0];
					pid = Convert.ToInt32(dataRow["pid"].ToString());
					num5 = Convert.ToInt32(dataRow["tid"].ToString());
					if (dataRow["posterid"] == null)
					{
						num3 = 0;
					}
					else
					{
						num3 = Convert.ToInt32(dataRow["posterid"].ToString());
					}
					dateTime = Convert.ToDateTime(dataRow["postdatetime"].ToString());
					if (dataRow["title"] == null)
					{
						text2 = "";
					}
					else
					{
						text2 = dataRow["title"].ToString();
					}
					if (dataRow["poster"] == null)
					{
						text = "";
					}
					else
					{
						text = dataRow["poster"].ToString();
					}
					text4 = string.Concat(new object[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"forums` SET `lasttid`=", 
						num5, 
						",`lasttitle`='", 
						text2, 
						"',`lastpost`='", 
						dateTime, 
						"',`lastposter`='", 
						text, 
						"',`lastposterid`=", 
						num3, 
						" WHERE `fid` in (", 
						text3, 
						")"
					});
					DbHelper.ExecuteNonQuery(CommandType.Text, text4);
					text4 = string.Concat(new object[]
					{
						"SELECT `pid`, `tid`, `posterid`, `postdatetime`, `title`, `poster` FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"posts", 
						posttableid, 
						"` WHERE `posterid`=", 
						num2, 
						" ORDER BY `pid` DESC LIMIT 1"
					});
					dataRow = DbHelper.ExecuteDataset(CommandType.Text, text4).Tables[0].Rows[0];
					pid = Convert.ToInt32(dataRow["pid"].ToString());
					num2 = Convert.ToInt32(dataRow["posterid"].ToString());
					dateTime = Convert.ToDateTime(dataRow["postdatetime"].ToString());
					if (dataRow["title"] == null)
					{
						text2 = "";
					}
					else
					{
						text2 = dataRow["title"].ToString();
					}
					text4 = string.Concat(new object[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users` SET `lastpost` = '", 
						dateTime, 
						"',`lastpostid` = ", 
						pid, 
						",`lastposttitle` = '", 
						text2, 
						"'  WHERE `uid` = ", 
						num2
					});
					DbHelper.ExecuteNonQuery(CommandType.Text, text4);
				}
			}
			return num4;
		}
		public IDataReader GetPostInfo(string posttableid, int pid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT  * FROM `{0}posts{1}` WHERE `pid`=?pid limit 1", BaseConfigs.get_GetTablePrefix(), posttableid), array);
		}
		public DataSet GetPostList(string topiclist, string[] posttableid)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < posttableid.Length; i++)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" UNION ALL ");
				}
				stringBuilder.Append("SELECT * FROM `");
				stringBuilder.Append(BaseConfigs.get_GetTablePrefix());
				stringBuilder.Append("posts");
				stringBuilder.Append(posttableid[i]);
				stringBuilder.Append("` WHERE `tid` IN (");
				stringBuilder.Append(topiclist);
				stringBuilder.Append(")");
			}
			return DbHelper.ExecuteDataset(CommandType.Text, stringBuilder.ToString());
		}
		public DataTable GetPostListTitle(int Tid, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, Tid)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `pid`, `title`, `poster`, `posterid`,`message` FROM `" + posttablename + "` WHERE `tid`=?tid ORDER BY `pid`", array).Tables[0];
		}
		public IDataReader GetPostListByCondition(PostpramsInfo _postpramsinfo, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, _postpramsinfo.get_Tid()), 
				DbHelper.MakeInParam("?pagesize", DbType.Boolean, 4, _postpramsinfo.get_Pagesize()), 
				DbHelper.MakeInParam("?pageindex", DbType.Boolean, 4, _postpramsinfo.get_Pageindex()), 
				DbHelper.MakeInParam("?condition", DbType.Single, 100, _postpramsinfo.get_Condition()), 
				DbHelper.MakeInParam("?posttablename", DbType.Single, 20, posttablename)
			};
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, _postpramsinfo.get_Tid())
			};
			object[] array3 = new object[138];
			array3[0] = "SELECT `";
			array3[1] = posttablename;
			array3[2] = "`.`pid`,`";
			array3[3] = posttablename;
			array3[4] = "`.`fid`,`";
			array3[5] = posttablename;
			array3[6] = "`.`title`,`";
			array3[7] = posttablename;
			array3[8] = "`.`layer`,`";
			array3[9] = posttablename;
			array3[10] = "`.`message`,`";
			array3[11] = posttablename;
			array3[12] = "`.`ip`,`";
			array3[13] = posttablename;
			array3[14] = "`.`lastedit`,`";
			array3[15] = posttablename;
			array3[16] = "`.`postdatetime`,`";
			array3[17] = posttablename;
			array3[18] = "`.`attachment`,`";
			array3[19] = posttablename;
			array3[20] = "`.`poster`,`";
			array3[21] = posttablename;
			array3[22] = "`.`posterid`,`";
			array3[23] = posttablename;
			array3[24] = "`.`invisible`,`";
			array3[25] = posttablename;
			array3[26] = "`.`usesig`,`";
			array3[27] = posttablename;
			array3[28] = "`.`htmlon`,`";
			array3[29] = posttablename;
			array3[30] = "`.`smileyoff`,`";
			array3[31] = posttablename;
			array3[32] = "`.`parseurloff`,`";
			array3[33] = posttablename;
			array3[34] = "`.`bbcodeoff`,`";
			array3[35] = posttablename;
			array3[36] = "`.`rate`,`";
			array3[37] = posttablename;
			array3[38] = "`.`ratetimes`,`";
			array3[39] = BaseConfigs.get_GetTablePrefix();
			array3[40] = "users`.`spaceid`,`";
			array3[41] = BaseConfigs.get_GetTablePrefix();
			array3[42] = "users`.`nickname`,`";
			array3[43] = BaseConfigs.get_GetTablePrefix();
			array3[44] = "users`.`username`,`";
			array3[45] = BaseConfigs.get_GetTablePrefix();
			array3[46] = "users`.`groupid`,`";
			array3[47] = BaseConfigs.get_GetTablePrefix();
			array3[48] = "users`.`email`,`";
			array3[49] = BaseConfigs.get_GetTablePrefix();
			array3[50] = "users`.`showemail`,`";
			array3[51] = BaseConfigs.get_GetTablePrefix();
			array3[52] = "users`.`digestposts`,`";
			array3[53] = BaseConfigs.get_GetTablePrefix();
			array3[54] = "users`.`credits`,`";
			array3[55] = BaseConfigs.get_GetTablePrefix();
			array3[56] = "users`.`gender`,`";
			array3[57] = BaseConfigs.get_GetTablePrefix();
			array3[58] = "users`.`bday`,`";
			array3[59] = BaseConfigs.get_GetTablePrefix();
			array3[60] = "users`.`extcredits1`,`";
			array3[61] = BaseConfigs.get_GetTablePrefix();
			array3[62] = "users`.`extcredits2`,`";
			array3[63] = BaseConfigs.get_GetTablePrefix();
			array3[64] = "users`.`extcredits3`,`";
			array3[65] = BaseConfigs.get_GetTablePrefix();
			array3[66] = "users`.`extcredits4`,`";
			array3[67] = BaseConfigs.get_GetTablePrefix();
			array3[68] = "users`.`extcredits5`,`";
			array3[69] = BaseConfigs.get_GetTablePrefix();
			array3[70] = "users`.`extcredits6`,`";
			array3[71] = BaseConfigs.get_GetTablePrefix();
			array3[72] = "users`.`extcredits7`,`";
			array3[73] = BaseConfigs.get_GetTablePrefix();
			array3[74] = "users`.`extcredits8`,`";
			array3[75] = BaseConfigs.get_GetTablePrefix();
			array3[76] = "users`.`posts`,`";
			array3[77] = BaseConfigs.get_GetTablePrefix();
			array3[78] = "users`.`joindate`,`";
			array3[79] = BaseConfigs.get_GetTablePrefix();
			array3[80] = "users`.`onlinestate`,`";
			array3[81] = BaseConfigs.get_GetTablePrefix();
			array3[82] = "users`.`lastactivity`,`";
			array3[83] = BaseConfigs.get_GetTablePrefix();
			array3[84] = "users`.`invisible` AS `userinvisible`,`";
			array3[85] = BaseConfigs.get_GetTablePrefix();
			array3[86] = "userfields`.`avatar`,`";
			array3[87] = BaseConfigs.get_GetTablePrefix();
			array3[88] = "userfields`.`avatarwidth`,`";
			array3[89] = BaseConfigs.get_GetTablePrefix();
			array3[90] = "userfields`.`avatarheight`,`";
			array3[91] = BaseConfigs.get_GetTablePrefix();
			array3[92] = "userfields`.`medals`,`";
			array3[93] = BaseConfigs.get_GetTablePrefix();
			array3[94] = "userfields`.`sightml` AS signature,`";
			array3[95] = BaseConfigs.get_GetTablePrefix();
			array3[96] = "userfields`.`location`,`";
			array3[97] = BaseConfigs.get_GetTablePrefix();
			array3[98] = "userfields`.`customstatus`,`";
			array3[99] = BaseConfigs.get_GetTablePrefix();
			array3[100] = "userfields`.`website`,`";
			array3[101] = BaseConfigs.get_GetTablePrefix();
			array3[102] = "userfields`.`icq`,`";
			array3[103] = BaseConfigs.get_GetTablePrefix();
			array3[104] = "userfields`.`qq`,`";
			array3[105] = BaseConfigs.get_GetTablePrefix();
			array3[106] = "userfields`.`msn`,`";
			array3[107] = BaseConfigs.get_GetTablePrefix();
			array3[108] = "userfields`.`yahoo`,`";
			array3[109] = BaseConfigs.get_GetTablePrefix();
			array3[110] = "userfields`.`skype` FROM `";
			array3[111] = posttablename;
			array3[112] = "` LEFT JOIN `";
			array3[113] = BaseConfigs.get_GetTablePrefix();
			array3[114] = "users` ON `";
			array3[115] = BaseConfigs.get_GetTablePrefix();
			array3[116] = "users`.`uid`=`";
			array3[117] = posttablename;
			array3[118] = "`.`posterid` LEFT JOIN `";
			array3[119] = BaseConfigs.get_GetTablePrefix();
			array3[120] = "userfields` ON `";
			array3[121] = BaseConfigs.get_GetTablePrefix();
			array3[122] = "userfields`.`uid`=`";
			array3[123] = BaseConfigs.get_GetTablePrefix();
			array3[124] = "users`.`uid` WHERE `";
			array3[125] = posttablename;
			array3[126] = "`.`tid`=";
			array3[127] = _postpramsinfo.get_Tid();
			array3[128] = " AND `";
			array3[129] = posttablename;
			array3[130] = "`.`invisible`=0 AND ";
			array3[131] = _postpramsinfo.get_Condition();
			array3[132] = " ORDER BY `";
			array3[133] = posttablename;
			array3[134] = "`.`pid` LIMIT ";
			object[] arg_5D8_0 = array3;
			int arg_5D8_1 = 135;
			int num = (_postpramsinfo.get_Pageindex() - 1) * _postpramsinfo.get_Pagesize();
			arg_5D8_0[arg_5D8_1] = num.ToString();
			array3[136] = ",";
			object[] arg_5FC_0 = array3;
			int arg_5FC_1 = 137;
			num = _postpramsinfo.get_Pagesize();
			arg_5FC_0[arg_5FC_1] = num.ToString();
			string text = string.Concat(array3);
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader GetPostList(PostpramsInfo _postpramsinfo, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, _postpramsinfo.get_Tid())
			};
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?pagesize", DbType.Boolean, 4, _postpramsinfo.get_Pagesize()), 
				DbHelper.MakeInParam("?posttablename", DbType.Single, 20, posttablename), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, _postpramsinfo.get_Tid())
			};
			object[] array3 = new object[136];
			array3[0] = "SELECT `";
			array3[1] = posttablename;
			array3[2] = "`.`pid`,`";
			array3[3] = posttablename;
			array3[4] = "`.`fid`,`";
			array3[5] = posttablename;
			array3[6] = "`.`title`,`";
			array3[7] = posttablename;
			array3[8] = "`.`layer`,`";
			array3[9] = posttablename;
			array3[10] = "`.`message`,`";
			array3[11] = posttablename;
			array3[12] = "`.`ip`, `";
			array3[13] = posttablename;
			array3[14] = "`.`lastedit`,`";
			array3[15] = posttablename;
			array3[16] = "`.`postdatetime`,`";
			array3[17] = posttablename;
			array3[18] = "`.`attachment`,`";
			array3[19] = posttablename;
			array3[20] = "`.`poster`,`";
			array3[21] = posttablename;
			array3[22] = "`.`posterid`,`";
			array3[23] = posttablename;
			array3[24] = "`.`invisible`,`";
			array3[25] = posttablename;
			array3[26] = "`.`usesig`, `";
			array3[27] = posttablename;
			array3[28] = "`.`htmlon`,`";
			array3[29] = posttablename;
			array3[30] = "`.`smileyoff`,`";
			array3[31] = posttablename;
			array3[32] = "`.`parseurloff`,`";
			array3[33] = posttablename;
			array3[34] = "`.`bbcodeoff`,`";
			array3[35] = posttablename;
			array3[36] = "`.`rate`,`";
			array3[37] = posttablename;
			array3[38] = "`.`ratetimes`, `";
			array3[39] = BaseConfigs.get_GetTablePrefix();
			array3[40] = "users`.`nickname`,`";
			array3[41] = BaseConfigs.get_GetTablePrefix();
			array3[42] = "users`.`spaceid`,`";
			array3[43] = BaseConfigs.get_GetTablePrefix();
			array3[44] = "users`.`username`,`";
			array3[45] = BaseConfigs.get_GetTablePrefix();
			array3[46] = "users`.`groupid`,`";
			array3[47] = BaseConfigs.get_GetTablePrefix();
			array3[48] = "users`.`email`,`";
			array3[49] = BaseConfigs.get_GetTablePrefix();
			array3[50] = "users`.`showemail`,`";
			array3[51] = BaseConfigs.get_GetTablePrefix();
			array3[52] = "users`.`gender`,`";
			array3[53] = BaseConfigs.get_GetTablePrefix();
			array3[54] = "users`.`bday`,`";
			array3[55] = BaseConfigs.get_GetTablePrefix();
			array3[56] = "users`.`digestposts`,`";
			array3[57] = BaseConfigs.get_GetTablePrefix();
			array3[58] = "users`.`credits`,`";
			array3[59] = BaseConfigs.get_GetTablePrefix();
			array3[60] = "users`.`extcredits1`,`";
			array3[61] = BaseConfigs.get_GetTablePrefix();
			array3[62] = "users`.`extcredits2`,`";
			array3[63] = BaseConfigs.get_GetTablePrefix();
			array3[64] = "users`.`extcredits3`,`";
			array3[65] = BaseConfigs.get_GetTablePrefix();
			array3[66] = "users`.`extcredits4`,`";
			array3[67] = BaseConfigs.get_GetTablePrefix();
			array3[68] = "users`.`extcredits5`,`";
			array3[69] = BaseConfigs.get_GetTablePrefix();
			array3[70] = "users`.`extcredits6`,`";
			array3[71] = BaseConfigs.get_GetTablePrefix();
			array3[72] = "users`.`extcredits7`,`";
			array3[73] = BaseConfigs.get_GetTablePrefix();
			array3[74] = "users`.`extcredits8`,`";
			array3[75] = BaseConfigs.get_GetTablePrefix();
			array3[76] = "users`.`posts`, `";
			array3[77] = BaseConfigs.get_GetTablePrefix();
			array3[78] = "users`.`joindate`,`";
			array3[79] = BaseConfigs.get_GetTablePrefix();
			array3[80] = "users`.`onlinestate`,`";
			array3[81] = BaseConfigs.get_GetTablePrefix();
			array3[82] = "users`.`lastactivity`,`";
			array3[83] = BaseConfigs.get_GetTablePrefix();
			array3[84] = "users`.`invisible` AS `userinvisible`,`";
			array3[85] = BaseConfigs.get_GetTablePrefix();
			array3[86] = "userfields`.`avatar`,`";
			array3[87] = BaseConfigs.get_GetTablePrefix();
			array3[88] = "userfields`.`avatarwidth`,`";
			array3[89] = BaseConfigs.get_GetTablePrefix();
			array3[90] = "userfields`.`avatarheight`,`";
			array3[91] = BaseConfigs.get_GetTablePrefix();
			array3[92] = "userfields`.`medals`,`";
			array3[93] = BaseConfigs.get_GetTablePrefix();
			array3[94] = "userfields`.`sightml` AS signature,`";
			array3[95] = BaseConfigs.get_GetTablePrefix();
			array3[96] = "userfields`.`location`,`";
			array3[97] = BaseConfigs.get_GetTablePrefix();
			array3[98] = "userfields`.`customstatus`,`";
			array3[99] = BaseConfigs.get_GetTablePrefix();
			array3[100] = "userfields`.`website`,`";
			array3[101] = BaseConfigs.get_GetTablePrefix();
			array3[102] = "userfields`.`icq`,`";
			array3[103] = BaseConfigs.get_GetTablePrefix();
			array3[104] = "userfields`.`qq`,`";
			array3[105] = BaseConfigs.get_GetTablePrefix();
			array3[106] = "userfields`.`msn`,`";
			array3[107] = BaseConfigs.get_GetTablePrefix();
			array3[108] = "userfields`.`yahoo`,`";
			array3[109] = BaseConfigs.get_GetTablePrefix();
			array3[110] = "userfields`.`skype` FROM (`";
			array3[111] = posttablename;
			array3[112] = "` LEFT JOIN `";
			array3[113] = BaseConfigs.get_GetTablePrefix();
			array3[114] = "users` ON `";
			array3[115] = BaseConfigs.get_GetTablePrefix();
			array3[116] = "users`.`uid`=`";
			array3[117] = posttablename;
			array3[118] = "`.`posterid`) LEFT JOIN `";
			array3[119] = BaseConfigs.get_GetTablePrefix();
			array3[120] = "userfields` ON `";
			array3[121] = BaseConfigs.get_GetTablePrefix();
			array3[122] = "userfields`.`uid`=`";
			array3[123] = BaseConfigs.get_GetTablePrefix();
			array3[124] = "users`.`uid` WHERE `";
			array3[125] = posttablename;
			array3[126] = "`.`tid`=";
			array3[127] = _postpramsinfo.get_Tid();
			array3[128] = " AND `";
			array3[129] = posttablename;
			array3[130] = "`.`invisible`=0 ORDER BY `";
			array3[131] = posttablename;
			array3[132] = "`.`pid` LIMIT ";
			object[] arg_584_0 = array3;
			int arg_584_1 = 133;
			int num = (_postpramsinfo.get_Pageindex() - 1) * _postpramsinfo.get_Pagesize();
			arg_584_0[arg_584_1] = num.ToString();
			array3[134] = ",";
			object[] arg_5A8_0 = array3;
			int arg_5A8_1 = 135;
			num = _postpramsinfo.get_Pagesize();
			arg_5A8_0[arg_5A8_1] = num.ToString();
			string text = string.Concat(array3);
			return DbHelper.ExecuteReader(CommandType.Text, text, array2);
		}
		public DataTable GetLastPostByTid(int tid, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT * FROM {0} WHERE `tid` = ?tid ORDER BY `pid` DESC LIMIT 1", posttablename), array);
			DataTable result;
			if (dataSet != null)
			{
				if (dataSet.Tables.Count > 0)
				{
					result = dataSet.Tables[0];
					return result;
				}
			}
			result = new DataTable();
			return result;
		}
		public DataTable GetLastPostList(PostpramsInfo _postpramsinfo, string posttablename)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?postnum", DbType.Boolean, 4, _postpramsinfo.get_Pagesize()), 
				DbHelper.MakeInParam("?posttablename", DbType.Single, 20, posttablename), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, _postpramsinfo.get_Tid())
			};
			string text = string.Concat(new string[]
			{
				"SELECT  `", 
				posttablename, 
				"`.`pid`, `", 
				posttablename, 
				"`.`fid`, `", 
				posttablename, 
				"`.`layer`, `", 
				posttablename, 
				"`.`posterid`, `", 
				posttablename, 
				"`.`title`, `", 
				posttablename, 
				"`.`message`, `", 
				posttablename, 
				"`.`postdatetime`, `", 
				posttablename, 
				"`.`attachment`, `", 
				posttablename, 
				"`.`poster`, `", 
				posttablename, 
				"`.`posterid`, `", 
				posttablename, 
				"`.`invisible`, `", 
				posttablename, 
				"`.`usesig`, `", 
				posttablename, 
				"`.`htmlon`, `", 
				posttablename, 
				"`.`smileyoff`, `", 
				posttablename, 
				"`.`parseurloff`, `", 
				posttablename, 
				"`.`bbcodeoff`, `", 
				posttablename, 
				"`.`rate`, `", 
				posttablename, 
				"`.`ratetimes`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`username`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`email`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`showemail`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`avatar`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`avatarwidth`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`avatarheight`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`sightml` AS signature, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`location`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`customstatus` FROM (`", 
				posttablename, 
				"` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid`=`", 
				posttablename, 
				"`.`posterid`) LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"userfields`.`uid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"users`.`uid` WHERE `", 
				posttablename, 
				"`.`tid`=?tid ORDER BY `", 
				posttablename, 
				"`.`pid` DESC LIMIT ", 
				_postpramsinfo.get_Pagesize().ToString()
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public IDataReader GetSinglePost(out IDataReader _Attachments, PostpramsInfo _postpramsinfo, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, _postpramsinfo.get_Pid()), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, _postpramsinfo.get_Tid())
			};
			_Attachments = DbHelper.ExecuteReader(CommandType.Text, "SELECT `aid`, `tid`, `pid`, `postdatetime`, `readperm`, `filename`, `description`, `filetype`, `filesize`, `attachment`, `downloads` FROM `" + BaseConfigs.get_GetTablePrefix() + "attachments` WHERE `tid`=?tid", array);
			string format = string.Concat(new string[]
			{
				"SELECT \r\n\r\n                            `{0}`.`pid`,\r\n                            `{0}`.`fid`,\r\n                            `{0}`.`title`,\r\n                            `{0}`.`layer`,\r\n                            `{0}`.`message`,\r\n                            `{0}`.`ip`,\r\n                            `{0}`.`lastedit`,\r\n                            `{0}`.`postdatetime`,\r\n                            `{0}`.`attachment`,\r\n                            `{0}`.`poster`,\r\n                            `{0}`.`invisible`,\r\n                            `{0}`.`usesig`,\r\n                            `{0}`.`htmlon`,\r\n                            `{0}`.`smileyoff`,\r\n                            `{0}`.`parseurloff`,\r\n                            `{0}`.`bbcodeoff`,\r\n                            `{0}`.`rate`,\r\n                            `{0}`.`ratetimes`,\r\n                            `{0}`.`posterid`,\r\n                            `{1}users`.`nickname`,\r\n                            `{1}users`.`username`,\r\n                            `{1}users`.`groupid`,\r\n                            `{1}users`.`spaceid`,\r\n                            `{1}users`.`email`,\r\n                            `{1}users`.`showemail`,\r\n                            `{1}users`.`digestposts`,\r\n                            `{1}users`.`credits`,\r\n                            `{1}users`.`extcredits1`,\r\n                            `{1}users`.`extcredits2`,\r\n                            `{1}users`.`extcredits3`,\r\n                            `{1}users`.`extcredits4`,\r\n                            `{1}users`.`extcredits5`,\r\n                            `{1}users`.`extcredits6`,\r\n                            `{1}users`.`extcredits7`,\r\n                            `{1}users`.`extcredits8`,\r\n                            `{1}users`.`bday`,\r\n                            `{1}users`.`gender`,\r\n                            `{1}users`.`posts`,\r\n                            `{1}users`.`joindate`,\r\n                            `{1}users`.`onlinestate`,\r\n                            `{1}users`.`lastactivity`,\r\n                            `{1}users`.`invisible`,\r\n                            `{1}userfields`.`avatar`,\r\n                            `{1}userfields`.`avatarwidth`,\r\n                            `{1}userfields`.`avatarheight`,\r\n                            `{1}userfields`.`medals`,\r\n                            `{1}userfields`.`sightml` AS signature,\r\n                            `{1}userfields`.`location`,\r\n                            `{1}userfields`.`customstatus`,\r\n                            `{1}userfields`.`website`,\r\n                            `{1}userfields`.`icq`,\r\n                            `{1}userfields`.`qq`,\r\n                            `{1}userfields`.`msn`,\r\n                            `{1}userfields`.`yahoo`,\r\n                            `{1}userfields`.`skype`\r\n            FROM `{0}` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"users` ON `{1}users`.`uid`=`{0}`.`posterid` LEFT JOIN `{1}userfields` ON `{1}userfields`.`uid`=`{1}users`.`uid` WHERE `{0}`.`pid`=", 
				_postpramsinfo.get_Pid().ToString(), 
				" LIMIT 1"
			});
			return DbHelper.ExecuteReader(CommandType.Text, string.Format(format, BaseConfigs.get_GetTablePrefix() + "posts" + posttableid, BaseConfigs.get_GetTablePrefix()), array);
		}
		public IDataReader GetSinglePost(int tid, int posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM {0}POSTS{1} WHERE `TID`={2} AND `LAYER`=0", BaseConfigs.get_GetTablePrefix(), posttableid, tid), array);
		}
		public DataTable GetPostTree(int tid, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `pid`, `layer`, `title`, `poster`, `posterid`,`postdatetime`,`message` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE `tid`=?tid AND `invisible`=0 ORDER BY `parentid`"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public int GetPostCount(int tid, string condition, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?posttablename", DbType.Single, 20, string.Format("{0}posts{1}", BaseConfigs.get_GetTablePrefix(), posttableid)), 
				DbHelper.MakeInParam("?condition", DbType.Single, 100, condition)
			};
			string text = "SELECT COUNT(pid) FROM `?posttablename` WHERE ?condition AND `layer`>=0";
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array), 0);
		}
		public int GetFirstPostId(int tid, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `pid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE `tid`=?tid ORDER BY `pid` LIMIT 1"
			});
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array).ToString(), -1);
		}
		public bool IsReplier(int tid, int uid, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, uid)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(`pid`) AS `pidcount` FROM `{0}posts{1}` WHERE `tid` = ?tid AND `posterid`=?uid AND ?uid>0", BaseConfigs.get_GetTablePrefix(), posttableid), array), 0) > 0;
		}
		public int UpdatePostRateTimes(int ratetimes, string postidlist, string posttableid)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}posts{1}` SET `ratetimes` = `ratetimes` + {3} WHERE `pid` IN ({2})", new object[]
			{
				BaseConfigs.get_GetTablePrefix(), 
				posttableid, 
				postidlist, 
				ratetimes
			}));
		}
		public int UpdatePostRate(int pid, float rate, string posttableid)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}posts{1}` SET `rate` = `rate` + {2} WHERE `pid` IN ({3})", new object[]
			{
				BaseConfigs.get_GetTablePrefix(), 
				posttableid, 
				rate, 
				pid
			}));
		}
		public int CancelPostRate(string postidlist, string posttableid)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}posts{1}` SET `rate` = 0, `ratetimes`=0 WHERE `pid` IN ({2})", BaseConfigs.get_GetTablePrefix(), posttableid, postidlist));
		}
		public DataTable GetPostRateList(int pid, int displayRateCount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid)
			};
			string text = string.Format("SELECT * FROM `{1}ratelog` WHERE `pid`=?pid ORDER BY `id` DESC LIMIT {0}", displayRateCount, BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public IDataReader GetNewTopics(string forumidlist)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?fidlist", DbType.Single, 500, forumidlist);
			string text = "";
			string postTableName = Posts.GetPostTableName();
			if (forumidlist != null)
			{
				text = string.Concat(new string[]
				{
					"SELECT `", 
					postTableName, 
					"`.`tid`, `", 
					postTableName, 
					"`.`title`, `", 
					postTableName, 
					"`.`poster`, `", 
					postTableName, 
					"`.`postdatetime`, `", 
					postTableName, 
					"`.`message`,`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums`.`name` FROM `", 
					postTableName, 
					"`  LEFT JOIN `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` ON `", 
					postTableName, 
					"`.`fid`=`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums`.`fid` WHERE  `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums`.`fid` NOT IN (?fidlist)  AND `", 
					postTableName, 
					"`.`layer`=0 ORDER BY `", 
					postTableName, 
					"`.`pid` DESC LIMIT 10"
				});
			}
			else
			{
				text = string.Concat(new string[]
				{
					"SELECT `", 
					postTableName, 
					"`.`tid`, `", 
					postTableName, 
					"`.`title`, `", 
					postTableName, 
					"`.`poster`, `", 
					postTableName, 
					"`.`postdatetime`, `", 
					postTableName, 
					"`.`message`,`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums`.`name` FROM `", 
					postTableName, 
					"`  LEFT JOIN `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` ON `", 
					postTableName, 
					"`.`fid`=`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums`.`fid` WHERE `", 
					postTableName, 
					"`.`layer`=0 ORDER BY `", 
					postTableName, 
					"`.`pid` DESC LIMIT 10"
				});
			}
			return DbHelper.ExecuteReader(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public IDataReader GetSitemapNewTopics(string forumidlist)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?fidlist", (DbType)253, 500, forumidlist);
			string text = null;
			if (forumidlist != "")
			{
				text = "SELECT `tid`, `fid`, `title`, `poster`, `postdatetime`, `lastpost`, `replies`, `views`, `digest` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `fid` NOT IN (?fidlist) ORDER BY `tid` DESC LIMIT 0,20";
			}
			else
			{
				text = "SELECT `tid`, `fid`, `title`, `poster`, `postdatetime`, `lastpost`, `replies`, `views`, `digest` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` ORDER BY `tid` DESC LIMIT 0,20";
			}
			return DbHelper.ExecuteReader(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public IDataReader GetForumNewTopics(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`tid`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`title`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`poster`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`postdatetime`,`", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts1`.`message` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` LEFT JOIN `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts1` ON `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`tid`=`", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts1`.`tid`  WHERE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts1`.`layer`=0 AND  `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`.`fid`=?fid ORDER BY `lastpost` DESC LIMIT 10"
			});
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public int CreateSearchCache(SearchCacheInfo cacheinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?keywords", (DbType)253, 255, cacheinfo.get_Keywords()), 
				DbHelper.MakeInParam("?searchstring", (DbType)253, 255, cacheinfo.get_Searchstring()), 
				DbHelper.MakeInParam("?ip", (DbType)253, 15, cacheinfo.get_Ip()), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, cacheinfo.get_Uid()), 
				DbHelper.MakeInParam("?groupid", DbType.Boolean, 4, cacheinfo.get_Groupid()), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, DateTime.Parse(cacheinfo.get_Postdatetime())), 
				DbHelper.MakeInParam("?expiration", DbType.Single, 19, cacheinfo.get_Expiration()), 
				DbHelper.MakeInParam("?topics", DbType.Boolean, 4, cacheinfo.get_Topics()), 
				DbHelper.MakeInParam("?tids", (DbType)254, 0, cacheinfo.get_Tids())
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "searchcaches` (`keywords`,`searchstring`,`ip`,`uid`,`groupid`,`postdatetime`,`expiration`,`topics`,`tids`) VALUES (?keywords,?searchstring,?ip,?uid,?groupid,?postdatetime,?expiration,?topics,?tids)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			return int.Parse(DbHelper.ExecuteDataset(CommandType.Text, "SELECT `searchid` FROM `" + BaseConfigs.get_GetTablePrefix() + "searchcaches` ORDER BY `searchid` DESC LIMIT 1").Tables[0].Rows[0][0].ToString());
		}
		public void DeleteExpriedSearchCache()
		{
			DbParameter[] array = new DbParameter[1];
			DbParameter[] arg_3A_0 = array;
			int arg_3A_1 = 0;
			string arg_35_0 = "?expiration";
			DbType arg_35_1 = DbType.Int64;
			int arg_35_2 = 8;
			DateTime dateTime = DateTime.Now;
			dateTime = dateTime.AddMinutes(-30.0);
			arg_3A_0[arg_3A_1] = DbHelper.MakeInParam(arg_35_0, arg_35_1, arg_35_2, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
			DbParameter[] array2 = array;
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM `{0}searchcaches` WHERE `expiration`<?expiration", BaseConfigs.get_GetTablePrefix()), array2);
		}
		public DataTable GetSearchCache(int searchid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?searchid", DbType.Boolean, 4, searchid)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT `tids` FROM `{0}searchcaches` WHERE searchid=?searchid LIMIT 1", BaseConfigs.get_GetTablePrefix()), array).Tables[0];
		}
		public DataTable GetSearchDigestTopicsList(int pagesize, string strTids)
		{
			string text = string.Format("SELECT `{0}topics`.`tid`, `{0}topics`.`title`, `{0}topics`.`poster`, `{0}topics`.`posterid`, `{0}topics`.`postdatetime`, `{0}topics`.`replies`, `{0}topics`.`views`, `{0}topics`.`lastpost`,`{0}topics`.`lastposter`, `{0}forums`.`fid`,`{0}forums`.`name` AS `forumname` FROM `{0}topics` LEFT JOIN `{0}forums` ON `{0}forums`.`fid` = `{0}topics`.`fid` WHERE `{0}topics`.tid in({2}) LIMIT {1} ", BaseConfigs.get_GetTablePrefix(), pagesize, strTids);
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetSearchPostsTopicsList(int pagesize, string strTids, string postablename)
		{
			string text = string.Format("SELECT `{2}`.`tid`, `{2}`.`title`, `{2}`.`poster`, `{2}`.`posterid`, `{2}`.`postdatetime`,`{2}`.`lastedit`, `{2}`.`rate`, `{2}`.`ratetimes`, `{0}forums`.`fid`,`{0}forums`.`name` AS `forumname` FROM `{2}` LEFT JOIN `{0}forums` ON `{0}forums`.`fid` = `{2}`.`fid` WHERE `{2}`.pid in({3}) LIMIT {1}", new object[]
			{
				BaseConfigs.get_GetTablePrefix(), 
				pagesize, 
				postablename, 
				strTids
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetSearchTopicsList(int pagesize, string strTids)
		{
			string text = string.Format("SELECT `{0}topics`.`tid`, `{0}topics`.`title`, `{0}topics`.`poster`, `{0}topics`.`posterid`, `{0}topics`.`postdatetime`, `{0}topics`.`replies`, `{0}topics`.`views`, `{0}topics`.`lastpost`,`{0}topics`.`lastposter`, `{0}forums`.`fid`,`{0}forums`.`name` AS `forumname` FROM `{0}topics` LEFT JOIN `{0}forums` ON `{0}forums`.`fid` = `{0}topics`.`fid` WHERE `{0}topics`.tid in({2}) LIMIT {1}", BaseConfigs.get_GetTablePrefix(), pagesize, strTids);
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public void ConfirmFullTextEnable()
		{
		}
		public int SetTopicStatus(string topiclist, string field, string intValue)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?field", (DbType)253, 500, intValue)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}topics` SET `{1}` = ?field WHERE `tid` IN ({2})", BaseConfigs.get_GetTablePrefix(), field, topiclist), array);
		}
		public int SetTopicStatus(string topiclist, string field, byte intValue)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?field", DbType.Boolean, 1, intValue)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}topics` SET `{1}` = ?field WHERE `tid` IN ({2})", BaseConfigs.get_GetTablePrefix(), field, topiclist), array);
		}
		public int SetTopicStatus(string topiclist, string field, int intValue)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?field", DbType.Boolean, 1, intValue)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}topics` SET `{1}` = ?field WHERE `tid` IN ({2})", BaseConfigs.get_GetTablePrefix(), field, topiclist), array);
		}
		public DataSet GetTopTopicList(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `tid`,`displayorder`,`fid` FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `displayorder` > 0 ORDER BY `fid`", array);
		}
		public DataTable GetShortForums()
		{
			return DbHelper.ExecuteDataset(CommandType.Text, "SELECT `fid`,`parentid`,`parentidlist`, `layer`, CAST('' AS CHAR(1000)) AS `temptidlist`,CAST('' AS CHAR(1000)) AS `tid2list`, CAST('' AS CHAR(1000)) AS `tidlist`,CAST(0 AS SIGNED) AS `tidcount`,CAST(0 AS SIGNED) AS `tid2count`,CAST(0 AS SIGNED) AS `tid3count` FROM `" + BaseConfigs.get_GetTablePrefix() + "forums` ORDER BY `fid` DESC").Tables[0];
		}
		public IDataReader GetUserListWithTopicList(string topiclist, int losslessdel)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?Losslessdel", DbType.Boolean, 4, losslessdel)
			};
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `posterid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE DATEDIFF(`postdatetime`, NOW())<?Losslessdel AND `tid` in (", 
				topiclist, 
				")"
			}), array);
		}
		public IDataReader GetUserListWithTopicList(string topiclist)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `posterid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `tid` in (", 
				topiclist, 
				")"
			}));
		}
		public int SetTopicClose(string topiclist, short intValue)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?field", (DbType)502, 2, intValue)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `closed` = ?field WHERE `tid` IN (", 
				topiclist, 
				") AND `closed` IN (0,1)"
			}), array);
		}
		public int GetTopicStatus(string topiclist, string field)
		{
			return Utils.StrToInt(DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new string[]
			{
				"SELECT SUM(if((`", 
				field, 
				"` is null),0,`", 
				field, 
				"`)) AS `fieldcount` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `tid` IN (", 
				topiclist, 
				")"
			})).Tables[0].Rows[0][0], 0);
		}
		public int DeleteTopicByTidList(string topiclist, string posttableid, bool chanageposts)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tidlist", (DbType)253, 200, topiclist), 
				DbHelper.MakeInParam("?posttablename", (DbType)253, 200, BaseConfigs.get_GetTablePrefix() + "posts" + posttableid)
			};
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			string text = "";
			int num4 = 0;
			if (topiclist != "")
			{
				string text2 = string.Concat(new string[]
				{
					"SELECT `fid`,`posterid`,`layer`,`postdatetime` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts", 
					posttableid, 
					"` WHERE `tid` IN (?tidlist)"
				});
				IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, text2, array);
				while (dataReader.Read())
				{
					num4 = int.Parse(dataReader["fid"].ToString());
					int num5 = int.Parse(dataReader["posterid"].ToString());
					int num6 = int.Parse(dataReader["layer"].ToString());
					DateTime value = Convert.ToDateTime(dataReader["postdatetime"].ToString());
					num++;
					if (num6 == 0)
					{
						num2++;
					}
					DateTime dateTime = Convert.ToDateTime(value);
					string arg_163_0 = dateTime.ToShortDateString();
					dateTime = DateTime.Now;
					if (arg_163_0 == dateTime.ToShortDateString())
					{
						num3++;
					}
					if (("," + num4.ToString() + ",").IndexOf(text + ",") == 0)
					{
						string text3 = DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new object[]
						{
							"select IFNULL(`parentidlist`,'') FROM `", 
							BaseConfigs.get_GetTablePrefix(), 
							"forums` WHERE `fid` = ", 
							num4
						}), array).ToString();
						if (text3 != "")
						{
							text = string.Concat(new string[]
							{
								text, 
								",", 
								text3, 
								",", 
								num4.ToString()
							});
						}
						else
						{
							text = text + "," + num4.ToString();
						}
					}
					DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"users` SET `posts` = `posts`-1 WHERE `uid` =", 
						num5
					}));
				}
			}
			if (text.Length > 0)
			{
				text = text.Substring(1, text.Length - 1);
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
				{
					"UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"statistics` SET `totaltopic`=`totaltopic`-", 
					num2, 
					", `totalpost`=`totalpost` -", 
					num
				}));
				string text2 = string.Concat(new object[]
				{
					" UPDATE `", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` SET `posts`=`posts` -", 
					num, 
					",`topics`=`topics` -", 
					num2, 
					",`todayposts` = `todayposts` - ", 
					num3, 
					" WHERE `fid` IN (", 
					text, 
					")"
				});
				DbHelper.ExecuteNonQuery(CommandType.Text, text2, array);
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"DELETE FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"favorites` WHERE `tid` IN (", 
					topiclist, 
					")"
				}), array);
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"DELETE FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"polls` WHERE `tid` IN (", 
					topiclist, 
					")"
				}), array);
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"DELETE FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"posts1` WHERE `tid` IN (", 
					topiclist, 
					")"
				}), array);
				DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
				{
					"DELETE FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"mytopics` WHERE `tid` IN (", 
					topiclist, 
					")"
				}), array);
			}
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `closed` IN (", 
				topiclist, 
				") OR `tid` IN (", 
				topiclist, 
				")"
			}), array);
			return 1;
		}
		public int DeleteClosedTopics(int fid, string topiclist)
		{
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
			{
				"DELETE FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `fid`=", 
				fid, 
				" AND `closed` IN (", 
				topiclist, 
				")"
			}));
		}
		public int CopyTopicLink(int oldfid, string topiclist)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Byte, 1, oldfid)
			};
			string text = string.Format("INSERT INTO `{0}topics` (`fid`,`iconid`,`typeid`,`readperm`,`price`,`poster`,`posterid`,`title`,`postdatetime`,`lastpost`,`lastposter`,`lastposterid`,`views`,`replies`,`displayorder`,`highlight`,`digest`,`rate`,`poll`,`attachment`,`moderated`,`hide`,`lastpostid`,`magic`,`closed`) SELECT ?fid,`iconid`,`typeid`,`readperm`,`price`,`poster`,`posterid`,`title`,`postdatetime`,`lastpost`,`lastposter`,`lastposterid`,`views`,`replies`,`displayorder`,`highlight`,`digest`,`rate`,`poll`,`attachment`,`moderated`,`hide`,`lastpostid`,`magic`,`tid` AS `closed` FROM `{0}topics` WHERE `tid` IN ({1})", BaseConfigs.get_GetTablePrefix(), topiclist);
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdatePost(string topiclist, int fid, string posttable)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Byte, 1, fid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				posttable, 
				"` SET `fid`=?fid WHERE `tid` IN (", 
				topiclist, 
				")"
			}), array);
		}
		public int UpdateTopic(string topiclist, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Byte, 1, fid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `fid`=?fid,`typeid`=0 WHERE `tid` IN (", 
				topiclist, 
				")"
			}), array);
		}
		public void UpdatePostTid(string postidlist, int tid, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` SET `tid`=?tid WHERE `pid` IN (", 
				postidlist, 
				")"
			}), array);
		}
		public void SetPrimaryPost(string subject, int tid, string[] postid, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, Utils.StrToInt(postid[0], 0)), 
				DbHelper.MakeInParam("?title", DbType.Single, 80, subject)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` SET `title` = ?title, `parentid` = `pid`,`layer` = 0 WHERE `pid` = ?pid"
			}), array);
		}
		public void SetNewTopicProperty(int topicid, int Replies, int lastpostid, int lastposterid, string lastposter, DateTime lastpost)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastpostid", DbType.Boolean, 4, lastpostid), 
				DbHelper.MakeInParam("?lastposterid", DbType.Boolean, 4, lastposterid), 
				DbHelper.MakeInParam("?lastpost", DbType.Int64, 8, lastpost), 
				DbHelper.MakeInParam("?lastposter", DbType.Single, 20, lastposter), 
				DbHelper.MakeInParam("?replies", DbType.Boolean, 4, Replies), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, topicid)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `lastpostid`=?lastpostid,`lastposterid` = ?lastposterid, `lastpost` = ?lastpost, `lastposter` = ?lastposter, `replies` = ?replies WHERE `tid` = ?tid", array);
		}
		public int UpdatePostTidToAnotherTopic(int oldtid, int newtid, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, newtid), 
				DbHelper.MakeInParam("?oldtid", DbType.Boolean, 4, oldtid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` SET `tid` = ?tid, `layer` =if(`layer`=0,1,`layer`)  WHERE `tid` = ?oldtid"
			}), array);
		}
		public int UpdateAttachmentTidToAnotherTopic(int oldtid, int newtid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, newtid), 
				DbHelper.MakeInParam("?oldtid", DbType.Boolean, 4, oldtid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "attachments` SET `tid`=?tid WHERE `tid`=?oldtid", array);
		}
		public int DeleteTopic(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `tid` = ?tid", array);
		}
		public int UpdateTopic(int tid, TopicInfo __topicinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastpostid", DbType.Boolean, 4, __topicinfo.get_Lastpostid()), 
				DbHelper.MakeInParam("?lastposterid", DbType.Boolean, 4, __topicinfo.get_Lastposterid()), 
				DbHelper.MakeInParam("?lastpost", DbType.Int64, 8, DateTime.Parse(__topicinfo.get_Lastpost())), 
				DbHelper.MakeInParam("?lastposter", DbType.Single, 20, __topicinfo.get_Lastposter()), 
				DbHelper.MakeInParam("?replies", DbType.Boolean, 4, __topicinfo.get_Replies()), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `lastpostid` = ?lastpostid,`lastposterid` = ?lastposterid, `lastpost` = ?lastpost, `lastposter` = ?lastposter, `replies` = `replies` + ?replies WHERE `tid` = ?tid", array);
		}
		public int UpdateTopicReplies(int tid, int topicreplies)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?replies", DbType.Boolean, 4, topicreplies), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `replies` = `replies` + ?replies WHERE `tid` = ?tid", array);
		}
		public int RepairTopics(string topiclist, string posttable)
		{
			string[] array = topiclist.Split(new char[]
			{
				','
			});
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string s = array2[i];
				int num = int.Parse(s);
				string text = string.Concat(new object[]
				{
					"SELECT `postdatetime`,`pid`,`poster`,`posterid` FROM `", 
					posttable, 
					"` WHERE `", 
					posttable, 
					"`.`tid`=", 
					num, 
					" LIMIT 1"
				});
				IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, text);
				string text2 = string.Concat(new object[]
				{
					"select COUNT(`pid`)-1 FROM `", 
					posttable, 
					"` WHERE `", 
					posttable, 
					"`.`tid`=", 
					num
				});
				int num2 = int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text2).ToString());
				if (dataReader.Read())
				{
					DbHelper.ExecuteNonQuery(string.Concat(new object[]
					{
						"UPDATE `", 
						BaseConfigs.get_GetTablePrefix(), 
						"topics` SET `lastpost` ='", 
						dataReader["postdatetime"].ToString(), 
						"' ,`lastpostid` =", 
						dataReader["pid"].ToString(), 
						" , `lastposter` ='", 
						dataReader["poster"].ToString(), 
						"' ,`lastposterid` =", 
						dataReader["posterid"].ToString(), 
						" , `replies` =", 
						num2, 
						"  WHERE `dnt_topics`.`tid`=", 
						num
					}));
				}
				dataReader.Close();
			}
			return array.Length;
		}
		public IDataReader GetUserListWithPostList(string postlist, string posttableid)
		{
			return DbHelper.ExecuteReader(CommandType.Text, string.Concat(new string[]
			{
				"SELECT `posterid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE `pid` in (", 
				postlist, 
				")"
			}));
		}
		public string CheckRateState(int userid, string pid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, Utils.StrToFloat(pid, 0f)), 
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userid)
			};
			return DbHelper.ExecuteScalarToStr(CommandType.Text, "SELECT `pid` FROM `" + BaseConfigs.get_GetTablePrefix() + "ratelog` WHERE `pid` = ?pid AND `uid` = ?uid", array);
		}
		public IDataReader GetTopicListModeratorLog(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteReader(CommandType.Text, "SELECT `grouptitle`, `moderatorname`,`postdatetime`,`actions` FROM `" + BaseConfigs.get_GetTablePrefix() + "moderatormanagelog` WHERE `tid` = ?tid ORDER BY `id` DESC LIMIT 1", array);
		}
		public int ResetTopicTypes(int topictypeid, string topiclist)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?typeid", DbType.Boolean, 1, topictypeid), 
				DbHelper.MakeInParam("?topiclist", DbType.Single, 250, topiclist)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `typeid` = ?typeid WHERE `tid` IN (", 
				topiclist, 
				")"
			}), array);
		}
		public int GetTopicsCountbyReplyUserId(int userId)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userId)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(DISTINCT `tid`) FROM `{0}myposts` WHERE `uid` = " + userId, BaseConfigs.get_GetTablePrefix()), array), 0);
		}
		public IDataReader GetTopicsByReplyUserId(int userId, int pageIndex, int pageSize)
		{
			string text = null;
			if (pageIndex == 1)
			{
				string text2 = string.Concat(new object[]
				{
					"SELECT DISTINCT `tid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"myposts` WHERE `uid`=", 
					userId, 
					" ORDER BY `tid` DESC LIMIT ", 
					pageSize.ToString()
				});
				IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, text2);
				string text3 = null;
				if (dataReader.Read())
				{
					while (dataReader.Read())
					{
						text3 = text3 + dataReader["tid"].ToString() + ",";
					}
					dataReader.Close();
					if (text3 != null)
					{
						text3 = text3.Substring(0, text3.Length - 1);
					}
				}
				else
				{
					text3 = "''";
				}
				text = string.Concat(new string[]
				{
					"SELECT `f`.`name`,`t`.`tid`, `t`.`fid`, `t`.`iconid`, `t`.`typeid`, `t`.`readperm`, `t`.`price`, `t`.`poster`, `t`.`posterid`, `t`.`title`, `t`.`postdatetime`, `t`.`lastpost`, `t`.`lastpostid`, `t`.`lastposter`, `t`.`lastposterid`, `t`.`views`, `t`.`replies`, `t`.`displayorder`, `t`.`highlight`, `t`.`digest`, `t`.`rate`, `t`.`hide`, `t`.`attachment`, `t`.`moderated`, `t`.`closed`, `t`.`magic`,`t`.`special`\r\n\t\t\t\t\t FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics` AS `t`,`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` AS `f` WHERE `t`.`fid`=`f`.`fid` AND `tid` IN (", 
					text3, 
					") ORDER BY `tid` DESC"
				});
			}
			else
			{
				string text2 = string.Concat(new object[]
				{
					"SELECT DISTINCT `tid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"myposts` WHERE `uid`=", 
					userId, 
					" AND `tid` < (SELECT MIN(`tid`) FROM (SELECT DISTINCT `tid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"myposts` WHERE `uid`=", 
					userId, 
					"  ORDER BY `tid` DESC LIMIT 0,", 
					(pageIndex - 1) * pageSize, 
					") AS `ttt`) ORDER BY `tid` DESC LIMIT 0,", 
					pageSize
				});
				IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, text2);
				string text3 = null;
				if (dataReader.Read())
				{
					while (dataReader.Read())
					{
						text3 = text3 + dataReader["tid"].ToString() + ",";
					}
					dataReader.Close();
					if (text3 != null)
					{
						text3 = text3.Substring(0, text3.Length - 1);
					}
				}
				else
				{
					text3 = "''";
				}
				text = string.Concat(new string[]
				{
					"SELECT `f`.`name`,`t`.`tid`, `t`.`fid`, `t`.`iconid`, `t`.`typeid`, `t`.`readperm`, `t`.`price`, `t`.`poster`, `t`.`posterid`, `t`.`title`, `t`.`postdatetime`, `t`.`lastpost`, `t`.`lastpostid`, `t`.`lastposter`, `t`.`lastposterid`, `t`.`views`, `t`.`replies`, `t`.`displayorder`, `t`.`highlight`, `t`.`digest`, `t`.`rate`, `t`.`hide`,`t`.`attachment`, `t`.`moderated`, `t`.`closed`, `t`.`magic`,`t`.`special` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics` AS `t`,`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` AS `f` WHERE `t`.`fid`=`f`.`fid`  AND `tid` IN (", 
					text3, 
					") ORDER BY `tid` DESC"
				});
			}
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public int GetTopicsCountbyUserId(int userId)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?uid", DbType.Boolean, 4, userId)
			};
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(1) FROM `{0}mytopics` WHERE `uid` = ?uid", BaseConfigs.get_GetTablePrefix()), array), 0);
		}
		public IDataReader GetTopicsByUserId(int userId, int pageIndex, int pageSize)
		{
			string text = null;
			if (pageIndex == 1)
			{
				string text2 = string.Concat(new object[]
				{
					"SELECT `tid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"mytopics` WHERE `uid`=", 
					userId, 
					" ORDER BY `tid` DESC LIMIT 0,", 
					pageSize
				});
				IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, text2);
				string text3 = null;
				if (dataReader.Read())
				{
					while (dataReader.Read())
					{
						text3 = text3 + dataReader["tid"].ToString() + ",";
					}
					dataReader.Close();
					if (text3 != null)
					{
						text3 = text3.Substring(0, text3.Length - 1);
					}
				}
				else
				{
					text3 = "''";
				}
				text = string.Concat(new string[]
				{
					"SELECT `f`.`name`,`t`.`tid`, `t`.`fid`, `t`.`iconid`, `t`.`typeid`, `t`.`readperm`, `t`.`price`, `t`.`poster`, `t`.`posterid`, `t`.`title`, `t`.`postdatetime`, `t`.`lastpost`, `t`.`lastpostid`, `t`.`lastposter`, `t`.`lastposterid`, `t`.`views`, `t`.`replies`, `t`.`displayorder`, `t`.`highlight`, `t`.`digest`, `t`.`rate`, `t`.`hide`, `t`.`attachment`, `t`.`moderated`, `t`.`closed`, `t`.`magic`,`t`.`special` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics` AS `t`,`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` AS `f` WHERE `t`.`fid`=`f`.`fid` AND `tid` IN (", 
					text3, 
					") ORDER BY `tid` DESC"
				});
			}
			else
			{
				string text2 = string.Concat(new object[]
				{
					"SELECT `tid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"mytopics` WHERE `uid`=", 
					userId, 
					" AND `tid` < (SELECT MIN(`tid`) FROM (SELECT `tid` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"mytopics` WHERE `uid`=", 
					userId, 
					" ORDER BY `tid` DESC LIMIT 0,", 
					(pageIndex - 1) * pageSize, 
					") AS `ttt`) ORDER BY `tid` DESC LIMIT 0,", 
					pageSize
				});
				IDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, text2);
				string text3 = null;
				if (dataReader.Read())
				{
					while (dataReader.Read())
					{
						text3 = text3 + dataReader["tid"].ToString() + ",";
					}
					dataReader.Close();
					if (text3 != null)
					{
						text3 = text3.Substring(0, text3.Length - 1);
					}
				}
				else
				{
					text3 = "''";
				}
				text = string.Concat(new string[]
				{
					"SELECT `f`.`name`,`t`.`tid`, `t`.`fid`, `t`.`iconid`, `t`.`typeid`, `t`.`readperm`, `t`.`price`, `t`.`poster`, `t`.`posterid`, `t`.`title`, `t`.`postdatetime`, `t`.`lastpost`, `t`.`lastpostid`, `t`.`lastposter`, `t`.`lastposterid`, `t`.`views`, `t`.`replies`, `t`.`displayorder`, `t`.`highlight`, `t`.`digest`, `t`.`rate`, `t`.`hide`, `t`.`attachment`, `t`.`moderated`, `t`.`closed`, `t`.`magic`,`t`.`special` FROM `", 
					BaseConfigs.get_GetTablePrefix(), 
					"topics` AS `t`,`", 
					BaseConfigs.get_GetTablePrefix(), 
					"forums` AS `f` WHERE `t`.`fid`=`f`.`fid` AND `tid` IN (", 
					text3, 
					") ORDER BY `tid` DESC"
				});
			}
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public int CreateTopic(TopicInfo topicinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Byte, 2, topicinfo.get_Fid()), 
				DbHelper.MakeInParam("?iconid", DbType.Byte, 2, topicinfo.get_Iconid()), 
				DbHelper.MakeInParam("?title", DbType.Single, 60, topicinfo.get_Title()), 
				DbHelper.MakeInParam("?typeid", DbType.Byte, 2, topicinfo.get_Typeid()), 
				DbHelper.MakeInParam("?readperm", DbType.Boolean, 4, topicinfo.get_Readperm()), 
				DbHelper.MakeInParam("?price", DbType.Byte, 2, topicinfo.get_Price()), 
				DbHelper.MakeInParam("?poster", DbType.Single, 50, topicinfo.get_Poster()), 
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, topicinfo.get_Posterid()), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 4, DateTime.Parse(topicinfo.get_Postdatetime())), 
				DbHelper.MakeInParam("?lastpost", DbType.Int64, 8, topicinfo.get_Lastpost()), 
				DbHelper.MakeInParam("?lastpostid", DbType.Boolean, 4, topicinfo.get_Lastpostid()), 
				DbHelper.MakeInParam("?lastposter", DbType.Single, 15, topicinfo.get_Lastposter()), 
				DbHelper.MakeInParam("?views", DbType.Boolean, 4, topicinfo.get_Views()), 
				DbHelper.MakeInParam("?replies", DbType.Boolean, 4, topicinfo.get_Replies()), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, topicinfo.get_Displayorder()), 
				DbHelper.MakeInParam("?highlight", DbType.Single, 500, topicinfo.get_Highlight()), 
				DbHelper.MakeInParam("?digest", DbType.Boolean, 4, topicinfo.get_Digest()), 
				DbHelper.MakeInParam("?rate", DbType.Boolean, 4, topicinfo.get_Rate()), 
				DbHelper.MakeInParam("?hide", DbType.Boolean, 4, topicinfo.get_Hide()), 
				DbHelper.MakeInParam("?attachment", DbType.Boolean, 4, topicinfo.get_Attachment()), 
				DbHelper.MakeInParam("?moderated", DbType.Boolean, 4, topicinfo.get_Moderated()), 
				DbHelper.MakeInParam("?closed", DbType.Boolean, 4, topicinfo.get_Closed()), 
				DbHelper.MakeInParam("?magic", DbType.Boolean, 4, topicinfo.get_Magic()), 
				DbHelper.MakeInParam("?special", DbType.Boolean, 4, topicinfo.get_Special())
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `tid`>(select IFNULL(max(`tid`),0)-100) AND `lastpostid`=0");
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "topics`(`fid`, `iconid`, `title`, `typeid`, `readperm`, `price`, `poster`, `posterid`, `postdatetime`, `lastpost`, `lastpostid`, `lastposter`, `views`, `replies`, `displayorder`, `highlight`, `digest`, `rate`, `hide`, `attachment`, `moderated`, `closed`, `magic`, `special`) VALUES(?fid, ?iconid, ?title, ?typeid, ?readperm, ?price, ?poster, ?posterid, ?postdatetime, ?lastpost, ?lastpostid, ?lastposter, ?views, ?replies, ?displayorder, ?highlight, ?digest, ?rate, ?hide, ?attachment, ?moderated, ?closed, ?magic, ?special)";
			int num;
			DbHelper.ExecuteNonQuery(ref num, CommandType.Text, text, array);
			int num2 = num;
			if (topicinfo.get_Displayorder() == 0)
			{
				DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "statistics` SET `totaltopic`=`totaltopic` + 1");
				DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "forums` SET `topics` = `topics` + 1,`curtopics` = `curtopics` + 1 WHERE `fid` = ?fid", array);
				if (topicinfo.get_Posterid() != -1)
				{
					DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new object[]
					{
						"INSERT INTO `", 
						BaseConfigs.get_GetTablePrefix(), 
						"mytopics`(`tid`,`uid`,`dateline`) VALUES(", 
						num2, 
						",  ?posterid,  '", 
						DateTime.Parse(topicinfo.get_Postdatetime()), 
						"')"
					}), array);
				}
			}
			return Utils.StrToInt(num2, -1);
		}
		public void AddParentForumTopics(string fpidlist, int topics, int posts)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?topics", DbType.Boolean, 4, topics)
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE `{0}forums` SET `topics` = `topics` + ?topics WHERE `fid` IN ({1})", BaseConfigs.get_GetTablePrefix(), fpidlist), array);
		}
		public IDataReader GetTopicInfo(int tid, int fid, byte mode)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			IDataReader result;
			switch (mode)
			{
				case 1:
				{
					result = DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `fid`=?fid AND `tid`<?tid AND `displayorder`>=0 ORDER BY `tid` DESC", array);
					break;
				}
				case 2:
				{
					result = DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `fid`=?fid AND `tid`>?tid AND `displayorder`>=0 ORDER BY `tid` ASC", array);
					break;
				}
				default:
				{
					DbParameter[] array2 = new DbParameter[]
					{
						DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
					};
					result = DbHelper.ExecuteReader(CommandType.Text, "SELECT * FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `tid`=?tid", array2);
					break;
				}
			}
			return result;
		}
		public IDataReader GetTopTopics(int fid, int pagesize, int pageindex, string tids)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid), 
				DbHelper.MakeInParam("?tids", DbType.Single, 500, tids)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `tid`,`fid`,`typeid`,`iconid`,`title`,`price`,`hide`,`readperm`,`poster`,`posterid`,`replies`,`views`,`postdatetime`,`lastpost`,`lastposter`,`lastpostid`,`lastposterid`,`replies`,`highlight`,`digest`,`displayorder`,`closed`,`attachment`,`magic`,`rate`,`special` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `displayorder`>0 AND `tid` IN (", 
				tids, 
				") ORDER BY `lastpost` DESC LIMIT ", 
				((pageindex - 1) * pagesize).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader GetTopics(int fid, int pagesize, int pageindex, int startnum, string condition)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("@condition", (DbType)253, 80, condition)
			};
			string text = string.Empty;
			text = string.Concat(new object[]
			{
				"SELECT  `rate`,`tid`,`iconid`,`typeid`,`title`,`price`,`hide`,`readperm`,\r\n\r\n`special`,`poster`,`posterid`,`replies`,`views`,`postdatetime`,`lastpost`,`lastposter`,\r\n\r\n`lastpostid`,`lastposterid`,`replies`,`highlight`,`digest`,`displayorder`,`attachment`,`closed`,`magic`,`special`  FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `fid`=", 
				fid, 
				" AND `displayorder`=0  ", 
				condition, 
				"   ORDER BY `lastpostid` DESC LIMIT ", 
				(pageindex - 1) * pagesize - startnum, 
				",", 
				pagesize
			});
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader GetTopicsByDate(int fid, int pagesize, int pageindex, int startnum, string condition, string orderby, int ascdesc)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid), 
				DbHelper.MakeInParam("?condition", DbType.Single, 80, condition), 
				DbHelper.MakeInParam("?orderby", DbType.Single, 80, orderby), 
				DbHelper.MakeInParam("?ascdesc", DbType.Boolean, 4, ascdesc)
			};
			string text;
			if (ascdesc == 0)
			{
				text = "asc";
			}
			else
			{
				text = "desc";
			}
			string text2 = string.Concat(new string[]
			{
				"SELECT `tid`,`iconid`,`title`,`price`,`typeid`,`readperm`,`hide`,`poll`,`poster`,`posterid`,`replies`,`views`,`postdatetime`,`lastpost`,`lastposter`,`lastpostid`,`lastposterid`,`replies`,`highlight`,`digest`,`displayorder`,`attachment`,`closed`,`magic` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `fid`=?fid AND `displayorder`=0 ", 
				condition, 
				" ORDER BY ?orderby ", 
				text, 
				" LIMIT ", 
				((pageindex - 1) * pagesize - startnum).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteReader(CommandType.Text, text2, array);
		}
		public IDataReader GetTopicsByType(int pagesize, int currentpage, int startnum, string condition, int ascdesc)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pagesize", DbType.Boolean, 4, pagesize), 
				DbHelper.MakeInParam("?condition", DbType.Single, 1000, condition)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `tid`,`iconid`,`typeid`,`title`,`price`,`hide`,`readperm`,`poster`,`posterid`,`replies`,`views`,`postdatetime`,`lastpost`,`lastposter`,`lastpostid`,`lastposterid`,`replies`,`highlight`,`digest`,`displayorder`,`attachment`,`closed`,`magic`,`special` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE  `displayorder`>=0 ", 
				condition, 
				"  ORDER BY `tid` DESC , `lastpostid` DESC  LIMIT ", 
				((currentpage - 1) * pagesize - startnum).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader GetTopicsByTypeDate(int pagesize, int currentpage, int startnum, string condition, string orderby, int ascdesc)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?condition", DbType.Single, 1000, condition), 
				DbHelper.MakeInParam("?orderby", DbType.Single, 80, orderby), 
				DbHelper.MakeInParam("?ascdesc", DbType.Boolean, 4, ascdesc)
			};
			string text;
			if (ascdesc == 0)
			{
				text = "asc";
			}
			else
			{
				text = "desc";
			}
			string text2 = string.Concat(new string[]
			{
				"SELECT * FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `displayorder`>=0 ", 
				condition, 
				" ORDER BY ?orderby ", 
				text, 
				" LIMIT ", 
				((currentpage - 1) * pagesize - startnum).ToString(), 
				",", 
				pagesize.ToString()
			});
			return DbHelper.ExecuteReader(CommandType.Text, text2, array);
		}
		public DataTable GetTopicList(string topiclist, int displayorder)
		{
			string text = string.Format("SELECT * FROM `{0}topics` WHERE `displayorder`>{1} AND `tid` IN ({2})", BaseConfigs.get_GetTablePrefix(), displayorder, topiclist);
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, text);
			DataTable result;
			if (dataSet != null)
			{
				if (dataSet.Tables.Count > 0)
				{
					result = dataSet.Tables[0];
					return result;
				}
			}
			result = null;
			return result;
		}
		public void UpdateTopicReplies(int tid, string posttableid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			int num = int.Parse(DbHelper.ExecuteScalar(CommandType.Text, string.Concat(new string[]
			{
				"SELECT COUNT(`pid`) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				posttableid, 
				"` WHERE `tid`=?tid AND `invisible`=0"
			}), array).ToString());
			DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `replies`=", 
				num - 1, 
				" WHERE `displayorder`>=0 AND `tid`=?tid"
			}), array);
		}
		public int GetTopicCount(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new object[]
			{
				"SELECT `curtopics` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums` WHERE `fid`=", 
				fid
			});
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array), 0);
		}
		public int GetAllTopicCount(int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new object[]
			{
				"SELECT COUNT(tid) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE (`fid`=", 
				fid, 
				"   OR   `fid`  IN (  SELECT fid  FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"forums`  WHERE  INSTR(concat(',',RTRIM(parentidlist),','),',", 
				fid, 
				",') > 0))  AND `displayorder`>=0"
			});
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array), 0);
		}
		public int GetTopicCount(int fid, int state, string condition)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid), 
				DbHelper.MakeInParam("?state", DbType.Boolean, 4, state), 
				DbHelper.MakeInParam("?condition", DbType.Single, 80, condition)
			};
			string text = null;
			if ((int)array[1].Value == -1)
			{
				text = "SELECT COUNT(tid) FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `fid`=?fid AND `displayorder`>-1 AND`closed`<=1 " + condition;
			}
			else
			{
				text = "SELECT COUNT(`tid`) FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE `fid`=?fid AND `displayorder`>-1 AND `closed`=?state AND `closed`<=1 " + condition;
			}
			return Utils.StrToInt(DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0].Rows[0][0].ToString(), -1);
		}
		public int GetTopicCount(string condition)
		{
			string text = "SELECT COUNT(`tid`) FROM `" + BaseConfigs.get_GetTablePrefix() + "topics` WHERE  `displayorder`>-1 AND `closed`<=1 " + condition;
			return Utils.StrToInt(DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0].Rows[0][0].ToString(), -1);
		}
		public int UpdateTopicTitle(int tid, string topictitle)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?topictitle", DbType.Single, 60, topictitle)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `title` = ?topictitle WHERE `tid` = ?tid", array);
		}
		public int UpdateTopicIconID(int tid, int iconid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?iconid", DbType.Byte, 2, iconid), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `iconid` = ?iconid WHERE `tid` = ?tid", array);
		}
		public int UpdateTopicPrice(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `price` = 0 WHERE `tid` = ?tid", array);
		}
		public int UpdateTopicPrice(int tid, int price)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("@tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("@price", DbType.Boolean, 4, price)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE [" + BaseConfigs.get_GetTablePrefix() + "topics] SET [price] = @price WHERE [tid] = @tid", array);
		}
		public int UpdateTopicReadperm(int tid, int readperm)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?price", DbType.Boolean, 4, readperm), 
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `price` = ?price WHERE `tid` = ?tid", array);
		}
		public int UpdateTopicModerated(string topiclist, int moderated)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?moderated", DbType.Boolean, 4, moderated)
			};
			return DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` SET `moderated` = ?moderated WHERE `tid` IN (", 
				topiclist, 
				")"
			}), array);
		}
		public int UpdateTopic(TopicInfo topicinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, topicinfo.get_Tid()), 
				DbHelper.MakeInParam("?fid", DbType.Byte, 2, topicinfo.get_Fid()), 
				DbHelper.MakeInParam("?iconid", DbType.Byte, 2, topicinfo.get_Iconid()), 
				DbHelper.MakeInParam("?title", DbType.Single, 60, topicinfo.get_Title()), 
				DbHelper.MakeInParam("?typeid", DbType.Byte, 2, topicinfo.get_Typeid()), 
				DbHelper.MakeInParam("?readperm", DbType.Boolean, 4, topicinfo.get_Readperm()), 
				DbHelper.MakeInParam("?price", DbType.Byte, 2, topicinfo.get_Price()), 
				DbHelper.MakeInParam("?poster", DbType.Single, 15, topicinfo.get_Poster()), 
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, topicinfo.get_Posterid()), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 4, DateTime.Parse(topicinfo.get_Postdatetime())), 
				DbHelper.MakeInParam("?lastpost", DbType.Single, 0, topicinfo.get_Lastpost()), 
				DbHelper.MakeInParam("?lastposter", DbType.Single, 15, topicinfo.get_Lastposter()), 
				DbHelper.MakeInParam("?replies", DbType.Boolean, 4, topicinfo.get_Replies()), 
				DbHelper.MakeInParam("?displayorder", DbType.Boolean, 4, topicinfo.get_Displayorder()), 
				DbHelper.MakeInParam("?highlight", DbType.Single, 500, topicinfo.get_Highlight()), 
				DbHelper.MakeInParam("?digest", DbType.Boolean, 4, topicinfo.get_Digest()), 
				DbHelper.MakeInParam("?rate", DbType.Boolean, 4, topicinfo.get_Rate()), 
				DbHelper.MakeInParam("?hide", DbType.Boolean, 4, topicinfo.get_Hide()), 
				DbHelper.MakeInParam("?attachment", DbType.Boolean, 4, topicinfo.get_Attachment()), 
				DbHelper.MakeInParam("?moderated", DbType.Boolean, 4, topicinfo.get_Moderated()), 
				DbHelper.MakeInParam("?closed", DbType.Boolean, 4, topicinfo.get_Closed()), 
				DbHelper.MakeInParam("?magic", DbType.Boolean, 4, topicinfo.get_Magic())
			};
			string text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "topics` SET `fid`=?fid,`iconid`=?iconid,`title`=?title,`typeid`=?typeid,`readperm`=?readperm,`price`=?price,`poster`=?poster,`posterid`=?posterid,`postdatetime`=?postdatetime,`lastpost`=?lastpost,`lastposter`=?lastposter,`replies`=?replies,`displayorder`=?displayorder,`highlight`=?highlight,`digest`=?digest,`rate`=?rate,`hide`=?hide,`poll`=?poll,`attachment`=?attachment,`moderated`=?moderated,`closed`=?closed,`magic`=?magic WHERE `tid`=?tid";
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public bool InSameForum(string topicidlist, int fid)
		{
			string text = string.Format("SELECT COUNT(tid) FROM `{0}topics` WHERE `fid`={1} AND `tid` IN ({2})", BaseConfigs.get_GetTablePrefix(), fid, topicidlist);
			return Utils.SplitString(topicidlist, ",").Length == int.Parse(DbHelper.ExecuteScalar(CommandType.Text, text).ToString());
		}
		public int UpdateTopicHide(int tid)
		{
			string text = string.Format("UPDATE `{0}topics` SET `hide` = 1 WHERE `tid` = {1}", BaseConfigs.get_GetTablePrefix(), tid);
			return DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public DataTable GetTopicList(int forumid, int pageid, int tpp)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT `tid`,`title`,`replies` FROM `{1}topics` WHERE `fid`={2} AND `displayorder`>=0 ORDER BY `lastpostid` DESC LIMIT " + ((pageid - 1) * tpp).ToString() + "," + tpp.ToString(), tpp.ToString(), BaseConfigs.get_GetTablePrefix(), forumid.ToString())).Tables[0];
		}
		public DataTable GetTopicFidByTid(string tidlist)
		{
			string text = string.Concat(new string[]
			{
				"SELECT distinct `fid` From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `tid` IN(", 
				tidlist, 
				")"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text).Tables[0];
		}
		public DataTable GetTopicTidByFid(string tidlist, int fid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?fid", DbType.Boolean, 4, fid)
			};
			string text = string.Concat(new string[]
			{
				"SELECT `tid` From `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` WHERE `tid` IN(", 
				tidlist, 
				") AND `fid`=?fid"
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text, array).Tables[0];
		}
		public int UpdateTopicViewCount(int tid, int viewcount)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?viewcount", DbType.Boolean, 4, viewcount)
			};
			string text = string.Concat(new object[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics`  SET `views`= `views` + ", 
				viewcount, 
				" WHERE `tid`=?tid"
			});
			return DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string SearchTopics(int forumid, string keyword, string displayorder, string digest, string attachment, string poster, bool lowerupper, string viewsmin, string viewsmax, string repliesmax, string repliesmin, string rate, string lastpost, DateTime postdatetimeStart, DateTime postdatetimeEnd)
		{
			string text = null;
			text += " `tid`>0";
			if (forumid != 0)
			{
				text = text + " AND `fid`=" + forumid;
			}
			if (keyword != "")
			{
				text += " And (";
				string[] array = keyword.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						text = text + " `title` like '%" + DataProvider.RegEsc(text2) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (displayorder != null)
			{
				if (!(displayorder == "0"))
				{
					if (!(displayorder == "1"))
					{
						if (displayorder == "2")
						{
							text += " AND displayorder<=0";
						}
					}
					else
					{
						text += " AND displayorder>0";
					}
				}
			}
			if (digest != null)
			{
				if (!(digest == "0"))
				{
					if (!(digest == "1"))
					{
						if (digest == "2")
						{
							text += " AND digest<1";
						}
					}
					else
					{
						text += " AND digest>=1";
					}
				}
			}
			if (attachment != null)
			{
				if (!(attachment == "0"))
				{
					if (!(attachment == "1"))
					{
						if (attachment == "2")
						{
							text += " AND attachment<=0";
						}
					}
					else
					{
						text += " AND attachment>0";
					}
				}
			}
			if (poster != "")
			{
				text += " AND (";
				string[] array = poster.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text3 = array[i];
					if (text3.Trim() != "")
					{
						if (lowerupper)
						{
							text = text + " poster='" + text3 + "' OR";
						}
						else
						{
							text = text + " poster COLLATE Chinese_PRC_CS_AS_WS ='" + text3 + "' OR";
						}
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (viewsmax != "")
			{
				text = text + " And views>" + viewsmax;
			}
			if (viewsmin != "")
			{
				text = text + " And views<" + viewsmin;
			}
			if (repliesmax != "")
			{
				text = text + " And replies>" + repliesmax;
			}
			if (repliesmin != "")
			{
				text = text + " And replies<" + repliesmin;
			}
			if (rate != "")
			{
				text = text + " And rate>" + rate;
			}
			if (lastpost != "")
			{
				text = text + " And datediff(lastpost,now())>=" + lastpost;
			}
			text = this.GetSqlstringByPostDatetime(text, postdatetimeStart, postdatetimeEnd);
			return text;
		}
		public string SearchAttachment(int forumid, string posttablename, string filesizemin, string filesizemax, string downloadsmin, string downloadsmax, string postdatetime, string filename, string description, string poster)
		{
			string text = null;
			text += " WHERE `aid` > 0";
			if (forumid != 0)
			{
				object obj = text;
				text = string.Concat(new object[]
				{
					obj, 
					" AND `pid` IN (SELECT PID FROM `", 
					posttablename, 
					"` WHERE `fid`=", 
					forumid, 
					")"
				});
			}
			if (filesizemin != "")
			{
				text = text + " AND `filesize`<" + filesizemin;
			}
			if (filesizemax != "")
			{
				text = text + " AND `filesize`>" + filesizemax;
			}
			if (downloadsmin != "")
			{
				text = text + " AND `downloads`<" + downloadsmin;
			}
			if (downloadsmax != "")
			{
				text = text + " AND `downloads`>" + downloadsmax;
			}
			if (postdatetime != "")
			{
				text = text + " AND datediff(postdatetime,NOW())>=" + postdatetime;
			}
			if (filename != "")
			{
				text = text + " AND `filename` like '%" + DataProvider.RegEsc(filename) + "%'";
			}
			if (description != "")
			{
				text += " And (";
				string[] array = description.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						text = text + " `description` like '%" + DataProvider.RegEsc(text2) + "%' OR ";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (poster != "")
			{
				string text3 = text;
				text = string.Concat(new string[]
				{
					text3, 
					" AND `pid` IN (SELECT `pid` FROM `", 
					posttablename, 
					"` WHERE `poster`='", 
					poster, 
					"')"
				});
			}
			return text;
		}
		public string SearchPost(int forumid, string posttableid, DateTime postdatetimeStart, DateTime postdatetimeEnd, string poster, bool lowerupper, string ip, string message)
		{
			string text = null;
			text += " `pid`>0 ";
			if (forumid != 0)
			{
				text = text + " AND `fid`=" + forumid;
			}
			if (postdatetimeStart.ToString() != "")
			{
				text = text + " And `postdatetime`>='" + postdatetimeStart.ToString() + "'";
			}
			if (postdatetimeEnd.ToString() != "")
			{
				text = text + " And `postdatetime`<='" + postdatetimeEnd.AddDays(1.0).ToString() + "'";
			}
			if (poster != "")
			{
				text += " AND (";
				string[] array = poster.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2.Trim() != "")
					{
						if (lowerupper)
						{
							text = text + " poster='" + text2 + "' OR";
						}
						else
						{
							text = text + " poster COLLATE Chinese_PRC_CS_AS_WS ='" + text2 + "' OR";
						}
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			if (ip != "")
			{
				text = text + " And `ip` like'%" + DataProvider.RegEsc(ip.Replace(".*", "")) + "%'";
			}
			if (message != "")
			{
				text += " AND (";
				string[] array = message.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text3 = array[i];
					if (text3.Trim() != "")
					{
						text = text + " message like '%" + DataProvider.RegEsc(text3) + "%' OR";
					}
				}
				text = text.Substring(0, text.Length - 3) + ")";
			}
			return text;
		}
		public void IdentifyTopic(string topiclist, int identify)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?identify", DbType.Boolean, 4, identify)
			};
			string text = string.Format("UPDATE `{0}topics` SET `identify`=?identify WHERE `tid` IN ({1})", BaseConfigs.get_GetTablePrefix(), topiclist);
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateTopic(int tid, string title, int posterid, string poster)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?title", DbType.Single, 60, title), 
				DbHelper.MakeInParam("?posterid", DbType.Boolean, 4, posterid), 
				DbHelper.MakeInParam("?poster", DbType.Single, 20, poster)
			};
			string text = string.Format("UPDATE `{0}topics` SET `title`=?title, `posterid`=?posterid, `poster`=?poster WHERE `tid`=?tid", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public string GetTopicCountCondition(out string type, string gettype, int getnewtopic)
		{
			string text = "";
			type = string.Empty;
			if (gettype == "digest")
			{
				type = "digest";
				text += " AND digest>0 ";
			}
			if (gettype == "newtopic")
			{
				type = "newtopic";
				string arg_77_0 = text;
				string arg_77_1 = " AND postdatetime>='";
				DateTime dateTime = DateTime.Now;
				dateTime = dateTime.AddMinutes((double)(-(double)getnewtopic));
				text = arg_77_0 + arg_77_1 + dateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
			}
			return text;
		}
		public string GetRateLogCountCondition(int userid, string postidlist)
		{
			return string.Concat(new object[]
			{
				"`uid`=", 
				userid, 
				" AND `pid` = ", 
				Utils.StrToInt(postidlist, 0).ToString()
			});
		}
		public DataTable GetOtherPostId(string postidlist, int topicid, int postid)
		{
			return DbHelper.ExecuteDataset(CommandType.Text, string.Concat(new object[]
			{
				"select * from ", 
				BaseConfigs.get_GetTablePrefix(), 
				"posts", 
				postid, 
				" where pid not in (", 
				postidlist, 
				") and tid=", 
				topicid, 
				" order by pid desc"
			})).Tables[0];
		}
		public void CreateTopicTags(string tags, int topicid, int userid, string curdatetime)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid), 
				DbHelper.MakeInParam("?postdatetime", DbType.Int64, 8, curdatetime)
			};
			DbParameter[] array2 = new DbParameter[]
			{
				DbHelper.MakeInParam("?tags", (DbType)253, 55, tags)
			};
			array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, topicid)
			};
			if (tags != "")
			{
				string[] array3 = tags.Split(new char[]
				{
					' '
				});
				for (int i = 0; i < array3.Length; i++)
				{
					string text = array3[i];
					string text2 = string.Concat(new string[]
					{
						"SELECT tagid,tagname FROM `", 
						BaseConfigs.get_GetTablePrefix(), 
						"tags` where tagname='", 
						text, 
						"'"
					});
					DataTable dataTable = DbHelper.ExecuteDataset(CommandType.Text, text2).Tables[0];
					if (dataTable.Rows.Count == 0)
					{
						string text3 = string.Concat(new object[]
						{
							"INSERT INTO `", 
							BaseConfigs.get_GetTablePrefix(), 
							"tags`(`tagname`, `userid`, `postdatetime`, `orderid`, `color`, `count`, `fcount`, `pcount`, `scount`, `vcount`) VALUES('", 
							text, 
							"', ", 
							userid, 
							", '", 
							curdatetime, 
							"', 0, '', 0, 0, 0, 0, 0)"
						});
						DbHelper.ExecuteNonQuery(CommandType.Text, text3);
						string text4 = string.Concat(new string[]
						{
							"SELECT `tagid` FROM `", 
							BaseConfigs.get_GetTablePrefix(), 
							"tags` WHERE `tagname`='", 
							text, 
							"'"
						});
						int num = Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, text4));
						string text5 = string.Concat(new object[]
						{
							"INSERT INTO `", 
							BaseConfigs.get_GetTablePrefix(), 
							"topictags` (tagid, tid) VALUES(", 
							num, 
							",", 
							topicid, 
							")"
						});
						DbHelper.ExecuteNonQuery(CommandType.Text, text5);
					}
					if (dataTable.Rows.Count > 0)
					{
						for (int j = 0; j < dataTable.Rows.Count; j++)
						{
							if (dataTable.Rows[0]["tagname"].ToString() != "")
							{
								string text4 = string.Concat(new string[]
								{
									"UPDATE `", 
									BaseConfigs.get_GetTablePrefix(), 
									"tags` SET `fcount`=`fcount`+1,`count`=`count`+1 WHERE tagname='", 
									text, 
									"'"
								});
								DbHelper.ExecuteNonQuery(CommandType.Text, text4, array2);
								string text6 = string.Concat(new object[]
								{
									"INSERT INTO `", 
									BaseConfigs.get_GetTablePrefix(), 
									"topictags` (tagid, tid) VALUES(", 
									dataTable.Rows[0]["tagid"], 
									",", 
									topicid, 
									")"
								});
								DbHelper.ExecuteNonQuery(CommandType.Text, text6);
							}
						}
					}
				}
			}
		}
		public IDataReader GetTopicListByTag(int tagid, int pageindex, int pagesize)
		{
			string text = string.Concat(new object[]
			{
				"SELECT `t`.`tid`, `t`.`title`,`t`.`poster`,`t`.`posterid`,`t`.`fid`,`t`.`postdatetime`,`t`.`replies`,`t`.`views`,`t`.`lastposter`,`t`.`lastposterid`,`t`.`lastpost` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topictags` AS `tt`, `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topics` AS `t` WHERE `t`.`tid` = `tt`.`tid` AND `t`.`displayorder` >=0 AND `tt`.`tagid` = ", 
				tagid, 
				" ORDER BY `t`.`lastpostid` DESC LIMIT ", 
				(pageindex - 1) * pagesize, 
				",", 
				pagesize
			});
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public IDataReader GetRelatedTopics(int topicid, int count)
		{
			string text = string.Format("SELECT * FROM `{0}topictagcaches` WHERE `tid`=?tid ORDER BY `linktid` DESC LIMIT {1} ", BaseConfigs.get_GetTablePrefix(), count);
			DbParameter dbParameter = DbHelper.MakeInParam("?tid", DbType.Boolean, 4, topicid);
			return DbHelper.ExecuteReader(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public int GetTopicsCountByTag(int tagid)
		{
			string text = string.Format("SELECT COUNT(1) FROM `{0}topictags` AS `tt`, `{0}topics` AS `t` WHERE `t`.`tid` = `tt`.`tid` AND `t`.`displayorder` >=0 AND `tt`.`tagid` = ?tagid", BaseConfigs.get_GetTablePrefix());
			DbParameter dbParameter = DbHelper.MakeInParam("?tagid", DbType.Boolean, 4, tagid);
			return Utils.StrToInt(DbHelper.ExecuteDataset(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			}).Tables[0].Rows[0][0], 0);
		}
		public void NeatenRelateTopics()
		{
			string text = string.Format("{0}neatenrelatetopic", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text);
		}
		public void DeleteTopicTags(int topicid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("@tid", DbType.Boolean, 4, topicid);
			DbHelper.ExecuteNonQuery(CommandType.Text, string.Concat(new string[]
			{
				"UPDATE `", 
				BaseConfigs.get_GetTablePrefix(), 
				"tags` SET `count`=`count`-1,`fcount`=`fcount`-1 WHERE EXISTS (SELECT `tagid` FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"topictags` WHERE `tid` = ?tid AND `tagid` = `", 
				BaseConfigs.get_GetTablePrefix(), 
				"tags`.`tagid`)"
			}), new DbParameter[]
			{
				dbParameter
			});
			DbHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM `" + BaseConfigs.get_GetTablePrefix() + "topictags` WHERE `tid` = ?tid\t", new DbParameter[]
			{
				dbParameter
			});
		}
		public void DeleteRelatedTopics(int topicid)
		{
			DbParameter dbParameter = DbHelper.MakeInParam("?tid", DbType.Boolean, 4, topicid);
			string text = string.Format("DELETE FROM `{0}topictagcaches` WHERE `tid` = ?tid OR `linktid` = ?tid", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, new DbParameter[]
			{
				dbParameter
			});
		}
		public void AddBonusLog(int tid, int authorid, int winerid, string winnerName, int postid, int bonus, int extid, int isbest)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?authorid", DbType.Boolean, 4, authorid), 
				DbHelper.MakeInParam("?winerid", DbType.Boolean, 4, winerid), 
				DbHelper.MakeInParam("?winnername", (DbType)253, 20, winnerName), 
				DbHelper.MakeInParam("?postid", DbType.Boolean, 4, postid), 
				DbHelper.MakeInParam("?dateline", DbType.Int64, 4, DateTime.Now), 
				DbHelper.MakeInParam("?bonus", DbType.Boolean, 4, bonus), 
				DbHelper.MakeInParam("?extid", DbType.Boolean, 1, extid), 
				DbHelper.MakeInParam("?isbest", DbType.Boolean, 4, isbest)
			};
			string text = string.Format("INSERT INTO `{0}bonuslog` VALUES(?tid, ?authorid, ?winerid, ?winnername, ?postid, ?dateline, ?bonus, ?extid, ?isbest)", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateMagicValue(int tid, int magic)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?magic", DbType.Boolean, 4, magic)
			};
			string text = string.Format("UPDATE `{0}topics` SET `magic`=?magic WHERE `tid`=?tid", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public IDataReader GetPostDebate(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			string text = string.Format("SELECT  `pid`,`opinion` FROM `{0}postdebatefields`", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public void AddDebateTopic(DebateInfo debatetopic)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, debatetopic.get_Tid()), 
				DbHelper.MakeInParam("?positiveopinion", (DbType)253, 200, debatetopic.get_Positiveopinion()), 
				DbHelper.MakeInParam("?negativeopinion", (DbType)253, 200, debatetopic.get_Negativeopinion()), 
				DbHelper.MakeInParam("?terminaltime", DbType.Int64, 8, debatetopic.get_Terminaltime())
			};
			string text = string.Format("INSERT INTO `{0}debates`(`tid`, `positiveopinion`, `negativeopinion`, `terminaltime`) VALUES(?tid, ?positiveopinion, ?negativeopinion, ?terminaltime)", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public void UpdateTopicSpecial(int tid, byte special)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?special", DbType.Boolean, 1, special)
			};
			string text = string.Format("UPDATE `{0}topics` SET `special` = ?special WHERE `tid` = ?tid", BaseConfigs.get_GetTablePrefix());
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public IDataReader GetDebateTopic(int tid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			string text = string.Format("SELECT  * FROM `{0}debates` WHERE `tid`=?tid", BaseConfigs.get_GetTablePrefix());
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public IDataReader GetHotDebatesList(string hotfield, int defhotcount, int getcount)
		{
			string text = string.Format("SELECT t.*,d.* FROM `{0}topics` AS t LEFT JOIN  `{0}debates` AS d ON t.`tid`=d.`tid` WHERE t.`{1}`>=`{2}` AND t.`special`=4 limit {3}", new object[]
			{
				BaseConfigs.get_GetTablePrefix(), 
				hotfield, 
				defhotcount, 
				getcount
			});
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public void CreateDebatePostExpand(DebatePostExpandInfo dpei)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, dpei.get_Tid()), 
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, dpei.get_Pid()), 
				DbHelper.MakeInParam("?opinion", DbType.Boolean, 4, dpei.get_Opinion()), 
				DbHelper.MakeInParam("?diggs", DbType.Boolean, 4, dpei.get_Diggs())
			};
			DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "postdebatefields` VALUES(?tid, ?pid, ?opinion, ?diggs)", array);
			string text;
			if (dpei.get_Opinion() == 1)
			{
				text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "debates` SET `positivediggs` = `positivediggs` + 1 WHERE `tid` = ?tid";
			}
			else
			{
				text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "debates` SET `negativediggs` = `negativediggs` + 1 WHERE `tid` = ?tid";
			}
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public IDataReader GetRecommendDebates(string tidlist)
		{
			string text = string.Format("SELECT  t.`tid`,t.`title`,t.`lastpost`,t.`lastposter`,d.* FROM {0}topics AS t LEFT JOIN `{0}debates` AS d ON t.`tid`=d.`tid` WHERE t.tid in ({1}) AND t.`special`=4", BaseConfigs.get_GetTablePrefix(), tidlist);
			return DbHelper.ExecuteReader(CommandType.Text, text);
		}
		public void AddCommentDabetas(int tid, int tableid, string commentmsg)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid)
			};
			string text = string.Format("SELECT `MESSAGE` FROM {0}posts{1} where `tid`=?tid and `layer`=0", BaseConfigs.get_GetTablePrefix(), tableid);
			string str = DbHelper.ExecuteScalarToStr(CommandType.Text, text, array);
			string text2 = string.Format("UPDATE {0}posts{1} SET  `MESSAGE`='{2}' where `tid`=?tid and `layer`=0", BaseConfigs.get_GetTablePrefix(), tableid, str + commentmsg);
			DbHelper.ExecuteNonQuery(CommandType.Text, text2, array);
		}
		public void AddDebateDigg(int tid, int pid, int field, string ip, UserInfo userinfo)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid), 
				DbHelper.MakeInParam("?debates", DbType.Boolean, 4, field), 
				DbHelper.MakeInParam("?diggerip", (DbType)253, 15, ip), 
				DbHelper.MakeInParam("?diggdatetime", DbType.Int64, 8, DateTime.Now), 
				DbHelper.MakeInParam("?diggerid", DbType.Boolean, 4, userinfo.get_Uid()), 
				DbHelper.MakeInParam("?digger", (DbType)253, 20, userinfo.get_Username())
			};
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "debatediggs`(`tid`,`pid`,`digger`,`diggerid`,`diggerip`,`diggdatetime`) VALUES(?tid,?pid,?digger,?diggerid,?diggerip,?diggdatetime)";
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			string text2 = "UPDATE  `" + BaseConfigs.get_GetTablePrefix() + "postdebatefields` SET `opinion`=?debates,`diggs`=`diggs`+1 WHERE `tid`=?tid AND `pid`=?pid";
			DbHelper.ExecuteNonQuery(CommandType.Text, text2, array);
			string text3 = string.Format("UPDATE  `" + BaseConfigs.get_GetTablePrefix() + "debates` SET {0}={0}+1 where `tid`=?tid", Enum.GetName(typeof(DebateType), field));
			DbHelper.ExecuteNonQuery(CommandType.Text, text3, array);
		}
		public bool AllowDiggs(int pid, int userid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?pid", DbType.Boolean, 4, pid), 
				DbHelper.MakeInParam("?userid", DbType.Boolean, 4, userid)
			};
			string text = "SELECT  COUNT(0) FROM `" + BaseConfigs.get_GetTablePrefix() + "debatediggs` WHERE `pid`=?pid AND `diggerid`=?userid";
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text, array), 0) < 1;
		}
		public IDataReader GetDebatePostList(int tid, int opinion, int pageSize, int pageIndex, string postTableName, PostOrderType postOrderType)
		{
			string text = "diggs";
			string text2 = " ASC";
			if (postOrderType.get_Orderfield() == 1)
			{
				text = "postdatetime";
			}
			if (postOrderType.get_Orderdirection() == 1)
			{
				text2 = " DESC";
			}
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?opinion", DbType.Boolean, 4, opinion), 
				DbHelper.MakeInParam("?pagesize", DbType.Boolean, 4, pageSize), 
				DbHelper.MakeInParam("?pageindex", DbType.Boolean, 4, pageIndex), 
				DbHelper.MakeInParam("?posttablename", (DbType)253, 20, postTableName), 
				DbHelper.MakeInParam("?orderby", (DbType)253, 20, text), 
				DbHelper.MakeInParam("?ascdesc", (DbType)253, 5, text2)
			};
			string text3 = string.Format("SELECT * FROM   {0}  LEFT JOIN {1}users ON {1}users.uid = {0}.posterid  LEFT JOIN `{1}userfields` ON `{1}userfields`.`uid`=`{1}users`.`uid` WHERE  {0} .invisible=0 AND  {0} .pid IN \r\n\t\t\t\t\t(SELECT  {2} pid FROM {1}postdebatefields \r\n\t\t\t\t\t WHERE opinion={3} AND \r\n\t\t\t\t\t\t\ttid={4})", new object[]
			{
				postTableName, 
				BaseConfigs.get_GetTablePrefix(), 
				pageSize, 
				opinion, 
				tid
			});
			return DbHelper.ExecuteReader(CommandType.Text, text3, array);
		}
		public DataTable GetLastPostList(int fid, int count, string posttablename, string visibleForum)
		{
			string text = "";
			if (fid > 0)
			{
				text = string.Concat(new object[]
				{
					" AND (`p`.`fid` = ", 
					fid, 
					" OR CHARINDEX(',", 
					fid, 
					",' , ',' + RTRIM(`f`.`parentidlist`) + ',') > 0 ) "
				});
			}
			if (visibleForum != string.Empty)
			{
				visibleForum = " AND `p`.`fid` IN (" + visibleForum + ")";
			}
			string text2 = string.Format("SELECT `p`.`pid`, `p`.`fid`, `p`.`tid`, `p`.`layer`, `p`.`posterid`, `p`.`title`, `p`.`postdatetime`, `p`.`attachment`, `p`.`poster`, `p`.`posterid`  FROM `{0}` AS `p` LEFT JOIN `{1}forums` AS `f` ON `p`.`fid` = `f`.`fid` LEFT JOIN `{1}topics` AS `t` ON `p`.`tid`=`t`.`tid` WHERE `p`.`layer`>0 AND `t`.`closed`<>1 AND  `t`.`displayorder` >=0 AND `p`.`invisible`<>1 {2} {3} ORDER BY `p`.`pid` DESC limit {4}", new object[]
			{
				posttablename, 
				BaseConfigs.get_GetTablePrefix(), 
				text, 
				visibleForum, 
				count
			});
			return DbHelper.ExecuteDataset(CommandType.Text, text2).Tables[0];
		}
		public IDataReader GetUesrDiggs(int tid, int uid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?diggerid", DbType.Boolean, 4, uid)
			};
			string text = "SELECT `pid` FROM `" + BaseConfigs.get_GetTablePrefix() + "debatediggs` WHERE `tid`=?tid AND `diggerid`=?diggerid";
			return DbHelper.ExecuteReader(CommandType.Text, text, array);
		}
		public int ReviseDebateTopicDiggs(int tid, int debateOpinion)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?tid", DbType.Boolean, 4, tid), 
				DbHelper.MakeInParam("?opinion", DbType.Boolean, 4, debateOpinion), 
				DbHelper.MakeOutParam("?count", DbType.Boolean, 4)
			};
			int result = Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, "select COUNT(1) FROM `" + BaseConfigs.get_GetTablePrefix() + "postdebatefields` WHERE `tid` = ?tid AND `opinion` = ?opinion", array), -1);
			string text;
			if (debateOpinion == 1)
			{
				text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "debates` SET `positivediggs`= ?count WHERE `tid` = ?tid";
			}
			else
			{
				text = "UPDATE `" + BaseConfigs.get_GetTablePrefix() + "debates` SET `negativediggs`= ?count WHERE `tid` = ?tid";
			}
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
			return result;
		}
		public IDataReader GetDebatePostDiggs(string pidlist)
		{
			IDataReader result;
			if (!Utils.IsNumericList(pidlist))
			{
				result = null;
			}
			else
			{
				string text = string.Format("SELECT `pid`,`diggs` FROM `{0}postdebatefields` WHERE `pid` IN ({1})", BaseConfigs.get_GetTablePrefix(), pidlist);
				result = DbHelper.ExecuteReader(CommandType.Text, text);
			}
			return result;
		}
		public int GetLastPostTid(ForumInfo foruminfo, string visibleForum)
		{
			string text = "";
			if (foruminfo.get_Fid() > 0)
			{
				text = string.Concat(new object[]
				{
					"AND (`t`.`fid` = ", 
					foruminfo.get_Fid(), 
					" OR instr(',", 
					foruminfo.get_Fid(), 
					",' , ',' + RTRIM(`f`.`parentidlist`) + ',') > 0 )  "
				});
			}
			if (!Utils.StrIsNullOrEmpty(visibleForum))
			{
				text = text + " AND `t`.`fid` IN (" + visibleForum + ")";
			}
			string text2 = string.Format("SELECT `tid` FROM `{0}topics` AS t LEFT JOIN `{0}forums` AS f  ON `t`.`fid` = `f`.`fid` WHERE `t`.`closed`<>1 AND  `t`.`displayorder` >=0  {1}  ORDER BY `t`.`lastpost` DESC limit 1", BaseConfigs.get_GetTablePrefix(), text);
			return Utils.StrToInt(DbHelper.ExecuteScalar(CommandType.Text, text2), -1);
		}
		public void SetPostsBanned(int tableid, string postlistid, int invisible)
		{
			string text = string.Format("UPDATE `{0}` SET `invisible`={1} WHERE `PID` IN ({2})", BaseConfigs.get_GetTablePrefix() + "posts" + tableid, invisible, postlistid);
			DbHelper.ExecuteNonQuery(text);
		}
		public void SetTopicsBump(string tidlist, int lastpostid)
		{
			DbParameter[] array = new DbParameter[]
			{
				DbHelper.MakeInParam("?lastpostid", DbType.Boolean, 4, lastpostid)
			};
			string text = string.Format("UPDATE {0} SET `lastpostid`={1} WHERE `tid` IN ({2})", BaseConfigs.get_GetTablePrefix() + "topics", (lastpostid != 0) ? "?lastpostid" : "0 - `lastpostid`", tidlist);
			DbHelper.ExecuteNonQuery(CommandType.Text, text, array);
		}
		public int GetPostId()
		{
			string text = "INSERT INTO `" + BaseConfigs.get_GetTablePrefix() + "postid` (`postdatetime`) VALUES(now());";
			int result;
			DbHelper.ExecuteNonQuery(ref result, CommandType.Text, text);
			return result;
		}
		public DataTable GetPostList(string postlist, int tableid)
		{
			string text = string.Format("SELECT * FROM `{0}posts{1}` WHERE `pid` IN ({2})", BaseConfigs.get_GetTablePrefix(), tableid, postlist);
			DataSet dataSet = DbHelper.ExecuteDataset(CommandType.Text, text);
			DataTable result;
			if (dataSet != null)
			{
				if (dataSet.Tables.Count > 0)
				{
					result = dataSet.Tables[0];
					return result;
				}
			}
			result = null;
			return result;
		}
		public string GetTopicFilterCondition(string filter)
		{
			string text = filter.ToLower().Trim();
			string result;
			switch (text)
			{
				case "poll":
				{
					result = " AND `special` = 1 ";
					return result;
				}
				case "reward":
				{
					result = " AND (`special` = 2 OR `special` = 3) ";
					return result;
				}
				case "rewarded":
				{
					result = " AND `special` = 3 ";
					return result;
				}
				case "rewarding":
				{
					result = " AND `special` = 2 ";
					return result;
				}
				case "debate":
				{
					result = " AND `special` = 4 ";
					return result;
				}
				case "digest":
				{
					result = " AND `digest` > 0 ";
					return result;
				}
			}
			result = "";
			return result;
		}
		public int GetDebatesPostCount(int tid, int debateOpinion)
		{
			string text = string.Concat(new object[]
			{
				"SELECT COUNT(1) FROM `", 
				BaseConfigs.get_GetTablePrefix(), 
				"postdebatefields` WHERE `tid` = ", 
				tid, 
				" AND `opinion` =", 
				debateOpinion
			});
			return Utils.StrToInt(DbHelper.ExecuteScalarToStr(CommandType.Text, text), 0);
		}
		public void DeleteDebatePost(int tid, string opinion, int pid)
		{
			int num = DbHelper.ExecuteNonQuery(string.Concat(new object[]
			{
				"DELETE FROM ", 
				BaseConfigs.get_GetTablePrefix(), 
				"postdebatefields WHERE `pid`=", 
				pid
			}));
			int num2 = DbHelper.ExecuteNonQuery(string.Concat(new object[]
			{
				"DELETE FROM ", 
				BaseConfigs.get_GetTablePrefix(), 
				"debatediggs WHERE `pid`=", 
				pid
			}));
			DbHelper.ExecuteNonQuery(string.Format("UPDATE " + BaseConfigs.get_GetTablePrefix() + "DEBATES SET {0}={0}-{1} WHERE `TID`={2}", opinion, num + num2, tid));
		}
	}
}
