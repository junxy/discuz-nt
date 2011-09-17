<%@ Page Language="c#" Inherits="Discuz.Web.Admin.usergroupsendemail" Codebehind="global_usergroupsendemail.aspx.cs" %>
<%@ Register TagPrefix="cc1" Namespace="Discuz.Control" Assembly="Discuz.Control" %>
<%@ Register TagPrefix="uc1" TagName="TextareaResize" Src="../UserControls/TextareaResize.ascx" %>
<%@ Register TagPrefix="cc3" Namespace="Discuz.Control" Assembly="Discuz.Control" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
<title>用户组邮件群发</title>
<link href="../styles/tab.css" type="text/css" rel="stylesheet" />
<link href="../styles/dntmanager.css" type="text/css" rel="stylesheet" />
<link href="../styles/modelpopup.css" type="text/css" rel="stylesheet" />
<script type="text/javascript" src="../js/common.js"></script>
<script type="text/javascript" src="../js/modalpopup.js"></script>
<script type="text/javascript">
	function nodeCheckChanged(node)
	{
		var status = "未选取"; 
		if (node.Checked) status = "选取"; 
	}  
</script>
<script type="text/javascript">
	function changelayer(objectname)
	{
		if(objectname == "user")
		{
			document.getElementById("user").style.display = 'block';
			document.getElementById("usernamelist").focus();
			document.getElementById("chkall").checked = false;
			CheckByName(document.getElementById("Form1"),'Usergroups','null');
			document.getElementById("usergroup").style.display = 'none';
		}
		else
		{
			document.getElementById("usergroup").style.display = 'block';
			document.getElementById("usernamelist").value = "";
			document.getElementById("user").style.display = 'none';
		}
	}
</script>
<meta http-equiv="X-UA-Compatible" content="IE=7" />
</head>
<body>
<form id="Form1" method="post" runat="server">
<cc1:Hint ID="Hint1" runat="server" HintImageUrl="../images"></cc1:Hint>
<div class="ManagerForm">
<fieldset>
<legend style="background: url(../images/icons/icon41.jpg) no-repeat 6px 50%;">批量邮件发送</legend>
<table width="100%">
	<tr><td class="item_title" colspan="2">要邮件发送给谁?</td></tr>
	<tr>
		<td class="vtop rowform">
			<input type="radio" name="sendobject" onclick="changelayer('user');" checked="checked" />&nbsp;用户&nbsp;&nbsp;&nbsp;&nbsp;
            <input type="radio" name="sendobject" onclick="changelayer('usergroup');" />&nbsp;用户组
		</td>
		<td class="vtop"></td>
	</tr>
	<tbody id="user">
	<tr><td class="item_title" colspan="2">接收邮件用户名称</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox ID="usernamelist" runat="server" CanBeNull="可为空" RequiredFieldType="暂无校验" Width="400" Height="40" TextMode="MultiLine"></cc1:TextBox>
		</td>
		<td class="vtop">要发送的用户名列表,以","进行分割</td>
	</tr>
	</tbody>
	<tbody id="usergroup" style="display:none">
	<tr><td class="item_title" colspan="2">接收邮件的用户组</td></tr>
	<tr>
		<td class="vtop rowform">
			<input type="checkbox" id="chkall" name="chkall" onclick="CheckAll(this.form);" />选择全部/取消
			<br />
			<cc1:CheckBoxList ID="Usergroups" runat="server" RepeatColumns="4"></cc1:CheckBoxList>
			<input type="hidden" name="flag" id="flag" />
		</td>
		<td class="vtop"><a href = "#" onclick="document.getElementById('flag').value=1;Form1.submit()">导出所有选中用户组用户的Email</a></td>
	</tr>
	</tbody>
	<tr><td class="item_title" colspan="2">邮件标题</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:TextBox ID="subject" runat="server" CanBeNull="必填" RequiredFieldType="暂无校验" Width="400"></cc1:TextBox>
		</td>
		<td class="vtop"></td>
	</tr>
	<tr><td class="item_title" colspan="2">邮件内容</td></tr>
	<tr>
		<td class="vtop rowform">
			<uc1:TextareaResize ID="body" runat="server" controlname="TabControl1:tabPage51:body" rows="18" cols="80"></uc1:TextareaResize>
		</td>
		<td class="vtop"></td>
	</tr>
</table>
<div class="Navbutton">
	<cc1:Button ID="BatchSendEmail" runat="server" Text=" 提 交 "></cc1:Button>
</div>
<asp:Label ID="lblClientSideCheck" runat="server" CssClass="hint">&nbsp;</asp:Label>
<asp:Label ID="lblCheckedNodes" runat="server" CssClass="hint">&nbsp;</asp:Label>
<asp:Label ID="lblServerSideCheck" runat="server" CssClass="hint">&nbsp;</asp:Label>
<script type="text/javascript">
	document.getElementById("lblClientSideCheck").innerText = document.getElementById("lblServerSideCheck").innerText;
</script>
</fieldset>
</div>
</form>
<%=footer%>
</body>
</html>