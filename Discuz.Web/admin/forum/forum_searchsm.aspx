<%@ Page language="c#" Inherits="Discuz.Web.Admin.searchsm" Codebehind="forum_searchsm.aspx.cs" %>
<%@ Register TagPrefix="cc1" Namespace="Discuz.Control" Assembly="Discuz.Control" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
<title>searchsm</title>		
<script type="text/javascript" src="../js/common.js"></script>
<link href="../styles/dntmanager.css" type="text/css" rel="stylesheet" />        
<link href="../styles/modelpopup.css" type="text/css" rel="stylesheet" />
<script type="text/javascript" src="../js/modalpopup.js"></script>
<meta http-equiv="X-UA-Compatible" content="IE=7" />
</head>
<body>
<div class="ManagerForm">
<form id="Form1" method="post" runat="server">
<fieldset>
<legend style="background:url(../images/icons/icon51.jpg) no-repeat 6px 50%;">清理短消息</legend>
<table width="100%">
	<tr><td class="item_title" colspan="2"></td></tr>
	<tr>
		<td class="vtop rowform">
			 <input type="checkbox" name="isnew" value="1" id="isnew" checked runat="server" /> 不删除未读信息
		</td>
		<td class="vtop"></td>
	</tr>
	<tr><td class="item_title" colspan="2">删除多少天以前的短消息</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox id="postdatetime" runat="server" RequiredFieldType="数据校验" Width="40"></cc1:TextBox>
		</td>
		<td class="vtop">不限制时间请输入</td>
	</tr>
	<tr><td class="item_title" colspan="2">按发信用户名清理</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:TextBox id="msgfromlist" runat="server" RequiredFieldType="暂无校验" Width="200"></cc1:TextBox> &nbsp; 
			&nbsp;<input type="checkbox" name="lowerupper" value="1" id="lowerupper" runat="server"> 不区分大小写
		</td>
		<td class="vtop">多用户名中间请用半角逗号 "," 分割</td>
	</tr>
	<tr><td class="item_title" colspan="2">按关键字搜索主题</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:TextBox id="subject" runat="server" RequiredFieldType="暂无校验" Width="200"></cc1:TextBox>
		</td>
		<td class="vtop">关键字中间用","分割</td>
	</tr>
	<tr><td class="item_title" colspan="2">按关键字搜索全文</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox id="message" runat="server" RequiredFieldType="暂无校验" Width="200"></cc1:TextBox>
		</td>
		<td class="vtop">关键字中间用","分割</td>
	</tr>
</table>
<cc1:Hint id="Hint1" runat="server" HintImageUrl="../images"></cc1:Hint>
<div class="Navbutton">
<cc1:Button id="SaveSearchInfo" runat="server" Text=" 删除短消息 " ButtonImgUrl="../images/del.gif" OnClientClick="if(!confirm('你确认要删除所选短消息吗？')) return false;"></cc1:Button> &nbsp;&nbsp;  
<input type="checkbox" name="isupdateusernewpm" value="1" id="isupdateusernewpm" checked runat="server" />同时更新收件人新短消息数</div>
</div>
</fieldset>
</form>	
<%=footer%>
</div>
</body>
</html>