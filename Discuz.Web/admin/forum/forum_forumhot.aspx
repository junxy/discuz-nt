<%@ Page language="c#" Inherits="Discuz.Web.Admin.forumhot" %>
<%@ Register TagPrefix="cc1" Namespace="Discuz.Control" Assembly="Discuz.Control" %>
<%@ Register Src="../UserControls/PageInfo.ascx" TagName="PageInfo" TagPrefix="uc1" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
<title>插件轮换图片管理</title>
<script type="text/javascript" src="../../javascript/common.js"></script>
<link href="../styles/dntmanager.css" type="text/css" rel="stylesheet" />
<link href="../styles/datagrid.css" type="text/css" rel="stylesheet" />
<link href="../styles/modelpopup.css" type="text/css" rel="stylesheet" />
<script type="text/javascript" src="../js/common.js"></script>
<script type="text/javascript" src="../js/modalpopup.js"></script>
<script type="text/javascript">
function isNaNEx(str){
	return (/^\d+$/.test(str));
}

function Check(form){
	if($('name').value==""){
		$('errortitle').style.display='';
		$('errortitle').innerHTML='热点名称不能为空';
		return false;
	}
	
	if($('datatype').options[$('datatype').options.selectedIndex].value==""){
		$('errortitle').style.display='';
		$('errortitle').innerHTML='数据来源必须选择';
		return false;
	}
	
	if(!isNaNEx($('cachetime').value)){
		$('errortitle').style.display='';
		$('errortitle').innerHTML='间隔时间必须为数字';
		return false;
	}
	<%if(id!=6){%>
	if(!isNaNEx($('forumnamelength').value)){
		$('errortitle').style.display='';
		$('errortitle').innerHTML='版块名称长度必须为数字';
		return false;
	}	 
	<%}%>
	if(!isNaNEx($('topictitlelength').value)){
		$('errortitle').style.display='';
		$('errortitle').innerHTML='标题长度必须为数字';
		return false;
	}
    if($('topics_select_forumid')){
	    obj=$('topics_select_forumid');
	    var k=0; 
	    for(var i=0;i<obj.options.length;i++){
		    if(k!=0){
			    $('form').submit(); 
		    }
		    else{
			    if(obj.options[i].selected){
				    k++;
			    }
		    }
	    }
	    if(k==0){
	        $('errortitle').style.display='';
	        $('errortitle').innerHTML='至少选择一个版块';
	        return false;
	    }
    }
	$('form').submit(); 
}

function changeselect(value){
     html="";
     htmlcount="";
     topictimetypehtml = "";

    if(value=="topics"){
        html+='<h3 class="item_title">排序方式</h3><select name="sorttype" id="topics_select_sorttype">';
        html+='<option value="Views">浏览量</option>';
        html+='<option value="LastPost">最后回复时间</option>';
        html+=' <option value="PostDateTime">最新主题</option>';
        html+=' <option value="Digest">精华主题</option>';
        html+='<option value="Replies">回复数</option>';
        html+='<option value="Rate">评分数</option>';
        html+=' </select>';
        html+=' <h3 class="item_title">版块列表</h3><select name="forumid" id="topics_select_forumid" size="5" multiple="multiple">';
        <%foreach(System.Data.DataRow drforum in Discuz.Forum.Forums.GetForumListForDataTable().Rows){%>
	        <%if(Discuz.Common.TypeConverter.ObjectToInt(drforum["Status"])>0 && Discuz.Common.Utils.StrIsNullOrEmpty(drforum["Password"].ToString()) && (Discuz.Common.Utils.StrIsNullOrEmpty(drforum["Viewperm"].ToString()) || Discuz.Common.Utils.InArray("7", drforum["Viewperm"].ToString()))){%>
	            html+='<option value="<%=drforum["fid"]%>"><%=drforum["name"].ToString().Replace("'", "\\'")%></option>';
	        <%}%>
        <%}%>
	        html+='</select>';
        htmlcount+='<select name="topiccount" id="topiccount">';
        htmlcount+='<option value="7">7</option>';
        htmlcount+='<option value="13">13</option>';				
        htmlcount+='</select>';

        topictimetypehtml+='<select id="topictimetypeselect" name="topictimetype">';
        topictimetypehtml+='<option value="All" >全部</option>';
        topictimetypehtml+='<option value="Day">一天</option>';
        topictimetypehtml+='<option value="ThreeDays">三天</option>';
        topictimetypehtml+='<option value="FiveDays" >五天</option>';
        topictimetypehtml+='<option value="Week" >一周</option>';
        topictimetypehtml+='<option value="Month">一个月</option>';
        topictimetypehtml+='<option value="SixMonth" >六个月</option>';
        topictimetypehtml+='<option value="Year">一年</option>';
        topictimetypehtml+='</select>';

        $('topictimetype').innerHTML=topictimetypehtml;

        $('topictimetype_tr').style.display='';
        $('topictimetype_tr_title').style.display='';

        $('forumlength_title').style.display = '';
        $('forumlength_content').style.display = '';
        $('titlelength_title').style.display = '';
	    $('titlelength_content').style.display = '';
    }
  
    if(value=="forums"){
        html+='<h3 class="item_title">排序方式</h3><select name="sorttype" id="forums_list_sorttype">';
	    html+='<option value="posts">帖子总数</option>';
	    html+='<option value="topics">主题数</option>';
	    html+='<option value="today">今日发帖数</option>';
        html+='<option value="thismonth">30天发帖数</option>';
        html+='</select>';
	
        htmlcount+='<select name="topiccount" id="topiccount">';
        htmlcount+='<option value="10">10</option>';
        htmlcount+='<option value="20">20</option>	';
        htmlcount+='</select>';

        $('topictimetype_tr').style.display='none';
        $('topictimetype_tr_title').style.display='none';

        $('forumlength_title').style.display = '';
        $('forumlength_content').style.display = '';
        $('titlelength_title').style.display = 'none';
	    $('titlelength_content').style.display = 'none';

    }
    if(value=="users"){
        html+='<h3 class="item_title">排序方式</h3><select name="sorttype" id="users_list_sorttype" onchange="user_topictimetypecontrol(this);">';
        html+='<option value="credits">积分</option>';
        html+='<option value="posts">发帖数</option>';
        html+='<option value="digestposts">精华帖数</option>';
        html+='<option value="lastactivity">最后访问时间</option>';
        html+='<option value="joindate">加入时间</option>';
        html+='</select>';
	
        htmlcount+='<select name="topiccount" id="topiccount">';
        htmlcount+='<option value="10">10</option>';
        htmlcount+='<option value="20">20</option>';		
        htmlcount+='</select>';

        topictimetypehtml+='<select id="topictimetypeselect" name="topictimetype">';
        topictimetypehtml+='<option value="posts">全部</option>';
        topictimetypehtml+='<option value="today">1天</option>';
        topictimetypehtml+='<option value="thisweek">7天</option>';
        topictimetypehtml+='<option value="thismonth">30天</option>';
        topictimetypehtml+='</select>';

        $('topictimetype').innerHTML=topictimetypehtml;

        $('topictimetype_tr').style.display='';
        $('topictimetype_tr_title').style.display='';

        $('forumlength_title').style.display = 'none';
        $('forumlength_content').style.display = 'none';

        $('titlelength_title').style.display = 'none';
	    $('titlelength_content').style.display = 'none';
    }
  
    if(value=="pictures"){
        html+='<h3 class="item_title">版块列表</h3><select name="forumid" id="topics_select_forumid" size="5" multiple="multiple">';
        <%foreach(System.Data.DataRow drforum in Discuz.Forum.Forums.GetForumListForDataTable().Rows){%>
            <%if(Discuz.Common.TypeConverter.ObjectToInt(drforum["Status"])>0 && Discuz.Common.Utils.StrIsNullOrEmpty(drforum["Password"].ToString()) && (Discuz.Common.Utils.StrIsNullOrEmpty(drforum["Viewperm"].ToString()) || Discuz.Common.Utils.InArray("7", drforum["Viewperm"].ToString()))){%>
                html+='<option value="<%=drforum["fid"]%>"><%=drforum["name"].ToString().Replace("'", "\\'")%></option>';
            <%}%>
        <%}%>
        html+='</select>';
        html+=' <h3 class="item_title">排序方式</h3><select name="sorttype" id="topics_select_sorttype">';
        html+='<option value="aid">最新图片</option>';
        html+='<option value="downloads">最多浏览</option>';
        html+='</select>';
        htmlcount+='<select name="topiccount" id="topiccount">';
        htmlcount+='<option value="3">3</option>';
        htmlcount+='<option value="5">5</option>';
        htmlcount+='</select>';
    }
    $('select_content').innerHTML=html;
    $('topiccount_div').innerHTML=htmlcount;
}

function user_topictimetypecontrol(obj){
    if(obj.value=="posts"){
        $('topictimetype_tr').style.display='';
        $('topictimetype_tr_title').style.display='';
    }
    else{
        $('topictimetype_tr').style.display='none';
        $('topictimetype_tr_title').style.display='none';
    }
}

function openforumhot(status) {
    document.getElementById("forumhotlayer").style.display = (status) ? "" : "none";
}
</script>
<meta http-equiv="X-UA-Compatible" content="IE=7" />
</head>
<body>
<%if(action==""){%>
<form action="forum_forumhot.aspx?action=closeforumhot" method="post">
<uc1:PageInfo ID="info1" runat="server" Icon="information" Text="<li>论坛热点信息,可以配置是否开启和调用条件!</li>" />
<div class="ManagerForm">
<fieldset>
<legend style="background: url(../images/icons/icon18.jpg) no-repeat 6px 50%;">论坛热点设置</legend>
<table width="100%" border="0">
  <tr>
    <td class="item_title">是否开启论坛热点功能</td>
  </tr>
  <tr>
    <td class="vtop rowform">
		<input name="forumhot" type="radio" value="1" onclick="openforumhot(1)" <%if(configInfo.Disableforumhot==1){%>checked<%}%>>是
      	<input type="radio" name="forumhot" value="0" onclick="openforumhot(0)" <%if(configInfo.Disableforumhot==0){%>checked<%}%>>否
	</td>
  </tr>
</table>
<div class="Navbutton">
	<button class="ManagerButton" type="submit"><img src="../images/submit.gif">  提 交</button>
</div>
</fieldset>
</div>
<div class="ManagerForm ntcplist" id="forumhotlayer" <%if(configInfo.Disableforumhot==0){%>style="display:none"<%}%>>
	<table class="datalist">
	<tbody>
		<tr class="category">
			<td style="border:1px solid #EAE9E1">ID</td>
			<td style="border:1px solid #EAE9E1">名称</td>
			<td style="border:1px solid #EAE9E1">操作</td>
		    <td style="border:1px solid #EAE9E1">是否开启</td>
		</tr>
		<%foreach(System.Data.DataRow dr in dsSrc.Tables["hottab"].Rows){%>
		<%if(Discuz.Common.TypeConverter.ObjectToInt(dr["id"])==6){%>
		<tr>
			<td colspan="4" class="category" style="border:1px solid #EAE9E1;text-align:left;padding-left:10px;font-weight:700;background:#F2F2F2;">轮显图片配置</td>
		</tr>
		<tr onMouseOut="this.className='mouseoutstyle'" onMouseOver="this.className='mouseoverstyle'" class="mouseoutstyle">
			<td style="border:1px solid #EAE9E1"><%=dr["id"]%></td>
			<td style="border:1px solid #EAE9E1;text-align:left;padding-left:10px;"><%=dr["name"]%></td>
			<td style="border:1px solid #EAE9E1"><a href="forum_forumhot.aspx?action=edit&id=<%=dr["id"]%>">编辑</a></td>
		    <td style="border:1px solid #EAE9E1"><img src="../images/state2.gif"></td>
		</tr>	
		<%}else{%>
		<tr onMouseOut="this.className='mouseoutstyle'" onMouseOver="this.className='mouseoverstyle'" class="mouseoutstyle">
			<td style="border:1px solid #EAE9E1"><%=dr["id"]%></td>
			<td style="border:1px solid #EAE9E1;text-align:left;padding-left:10px;"><%=dr["name"]%></td>
			<td style="border:1px solid #EAE9E1"><a href="forum_forumhot.aspx?action=edit&id=<%=dr["id"]%>">编辑</a></td>
		    <td style="border:1px solid #EAE9E1">
			<%if(Discuz.Common.TypeConverter.ObjectToInt(dr["displayorder"])==1){%>
			<img src="../images/state2.gif">
			<%}else{%>
			<img src="../images/state3.gif">
			<%}%>
			</td>
		</tr>
		<%}%>
		<%}%>  
	</tbody>
	</table>
</div>
</form>
<%}%>
<%if(action=="edit"){%>
<form action="forum_forumhot.aspx?action=editsave&id&id=<%=id%>" method="post"  name="form" id="form">
<div class="ManagerForm">
<fieldset>
<legend style="background: url(&quot;../images/icons/legendimg.jpg&quot;) no-repeat scroll 6px 50% transparent;">编辑热点配置</legend>
<%foreach(System.Data.DataRow dr in dsSrc.Tables["hottab"].Select("id="+id)){%> 
<div id="errortitle" style="display:none; border: 1px dotted #DBDDD3; margin: 10px 0pt; padding: 15px 10px 10px 56px; background:#FDFFF2 url(&quot;../images/hint.gif&quot;) no-repeat 20px 15px; clear: both;text-align:left">
</div>
<table width="100%">
<tbody>
	<tr><td colspan="2" class="item_title">热点名称</td></tr>
	<tr>
		<td class="vtop rowform">
			<input type="text" size="30" onBlur="this.className='txt';" onFocus="this.className='txt_focus';" class="txt" id="name" value="<%=dr["name"]%>" name="name">
		</td>
		<td class="vtop">热点标签的名称</td>
	</tr>
  
	<tr><td colspan="2" class="item_title"><%if(id!=6){%>是否开启<%}else{%>是否开启从一个主题提取一张图片<%}%></td></tr>
	<tr>
		<td class="vtop rowform">
			<input name="displayorder" type="radio" value="1"  <%if(Discuz.Common.TypeConverter.ObjectToInt(dr["displayorder"])==1){%>checked<%}%>>启用
			<input name="displayorder" type="radio" value="0"  <%if(Discuz.Common.TypeConverter.ObjectToInt(dr["displayorder"])==0){%>checked<%}%>>关闭
			
		</td>
		<td class="vtop">可配置单个标签是否显示</td>
	</tr>
   
	<tr><td colspan="2" class="item_title">调用选项</td></tr>
	<tr>
		<td class="vtop rowform">
			<select name="datatype" onChange="changeselect(this.options[this.options.selectedIndex].value)" id="datatype">
				<%if(id!=6){%>
				<option value="topics" <%if(dr["datatype"].ToString()=="topics"){%>selected<%}%>>帖子排行</option>
				<option value="forums" <%if(dr["datatype"].ToString()=="forums"){%>selected<%}%>>版块排行</option>
				<option value="users" <%if(dr["datatype"].ToString()=="users"){%>selected<%}%>>用户排行</option>
				<%}else{%>
				<option value="pictures" <%if(dr["datatype"].ToString()=="pictures"){%>selected<%}%>>论坛图片</option>	  
				<%}%>
			</select>
			<div id="select_content"></div>
		</td>
		<td class="vtop">调用数据的方式</td>
	</tr>
	<tr><td colspan="2" class="item_title">调用条数</td></tr>
	<tr>
		<td class="vtop rowform">
		<div id="topiccount_div">
		<select name="topiccount" id="topiccount">
		    <%if(dr["datatype"].ToString().Trim()=="topics"){%>
			<option value="8" <%if(dr["topiccount"].ToString()=="8"){%>selected<%}%>>8</option>
			<option value="15" <%if(dr["topiccount"].ToString()=="15"){%>selected<%}%>>15</option>			

			<%}else{%>
			
				<%if(dr["datatype"].ToString().Trim()=="pictures"){%>
				<option value="3" <%if(dr["topiccount"].ToString()=="3"){%>selected<%}%>>3</option>
				<option value="5" <%if(dr["topiccount"].ToString()=="5"){%>selected<%}%>>5</option>				
				<%}else{%>
				<option value="10" <%if(dr["topiccount"].ToString()=="10"){%>selected<%}%>>10</option>
				<option value="20" <%if(dr["topiccount"].ToString()=="20"){%>selected<%}%>>20</option>			
				<%}%>
		    <%}%>
		</select>
		</div>
		</td>
		<td class="vtop">显示数据的条数</td>
	</tr>
	<tr style="display:none" id="topictimetype_tr_title"><td colspan="2" class="item_title">获取信息时间范围</td></tr>
	<tr style="display:none" id="topictimetype_tr">
		<td class="vtop rowform" id="topictimetype">
            <select id="topictimetypeselect">
                <option value="0">0</option>
            </select>
		</td>
		<td class="vtop">设置程序获取一定时间范围以内的数据</td>
	</tr>
	
	
	<tr><td colspan="2" class="item_title">调用间隔时间（秒）</td></tr>
	<tr>
		<td class="vtop rowform">
			<input type="text" size="10" onBlur="this.className='txt';" onFocus="this.className='txt_focus';" class="txt" id="cachetime" value="<%=dr["cachetime"]%>" name="cachetime">
		</td>
		<td class="vtop">调用间隔时间，程序一段时间将缓存数据，过期后再次调用</td>
	</tr>

  <%if(id!=6){%>
	<tr id="forumlength_title"><td colspan="2" class="item_title">版块名称长度</td></tr>
	<tr id="forumlength_content">
		<td class="vtop rowform">
			<input type="text" size="10" onBlur="this.className='txt';" onFocus="this.className='txt_focus';" class="txt" id="forumnamelength" value="<%=dr["forumnamelength"]%>" name="forumnamelength">
		</td>
		<td class="vtop">主题所在的相关版块名称的长短，超过将截取</td>
	</tr>
  <%}%>
	<tr id="titlelength_title"><td colspan="2" class="item_title">标题长度</td></tr>
	<tr id="titlelength_content">
		<td class="vtop rowform">
			<input type="text" size="10" onBlur="this.className='txt';" onFocus="this.className='txt_focus';" class="txt" id="topictitlelength" value="<%=dr["topictitlelength"]%>" name="topictitlelength">
		</td>
		<td class="vtop">主题名称的长短，超过将截取</td>
	</tr>
</tbody>
</table>
</fieldset>
<div class="Navbutton">
	<span><button class="ManagerButton" type="button"  onClick="Check()"><img src="../images/submit.gif">提 交</button><button class="ManagerButton" type="button"  onClick="javascript:window.location.href='forum_forumhot.aspx'"><img src="../images/submit.gif">返 回</button></span>
</div>
<script type="text/javascript">
function selectedoption(ele,value)
{
 var obj=$(ele);
  for(var i=0;i<obj.options.length;i++)
  {
	   if(obj.options[i].value==value)
	   {
		obj.options[i].selected=true;
	   }
  }
}

var value = $('datatype').options[$('datatype').options.selectedIndex].value;

changeselect(value);
if(value=="topics")
{

	selectedoption('topics_select_sorttype','<%=dr["sorttype"].ToString()%>')
    var v='<%=dr["forumid"].ToString()%>'.split(',');
    for (i = 0; i < v.length; i++) {
        selectedoption('topics_select_forumid', v[i]);
    }

	$('topictimetype_tr').style.display='';
	$('topictimetype_tr_title').style.display = '';
	$('forumlength_title').style.display = '';
	$('forumlength_content').style.display = '';
	$('titlelength_title').style.display = '';
	$('titlelength_content').style.display = '';
}

if(value=="forums")
{
    selectedoption('forums_list_sorttype', '<%=dr["sorttype"].ToString()%>');
    $('forumlength_title').style.display = '';
    $('forumlength_content').style.display = '';
    $('titlelength_title').style.display = 'none';
    $('titlelength_content').style.display = 'none';
}
if(value=="users") {
    var sorttype = '<%=dr["sorttype"].ToString()%>';
    if (sorttype == "thismonth" || sorttype == "thisweek" || sorttype == "today")
        sorttype = "posts";
    selectedoption('users_list_sorttype', sorttype);
    user_topictimetypecontrol($('users_list_sorttype'));
    $('forumlength_title').style.display = 'none';
    $('forumlength_content').style.display = 'none';
    $('titlelength_title').style.display = 'none';
    $('titlelength_content').style.display = 'none';
}

if(value=="pictures")
{ 
    var v='<%=dr["forumid"].ToString()%>'.split(',');
    for(i=0;i<v.length;i++)
	{
	selectedoption('topics_select_forumid',v[i]);
	}
 selectedoption('topics_select_sorttype','<%=dr["sorttype"].ToString()%>')
}
selectedoption('topiccount', '<%=dr["topiccount"].ToString()%>');
selectedoption('topictimetypeselect', '<%=dr["topictimetype"].ToString()%>');
</script>
<%}%>  
</div>
</form>
<%}%>
<%=footer%>
</body>
</html>