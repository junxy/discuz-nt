<%@ Page Language="c#" Inherits="Discuz.Web.Admin.sendsmtogroup" CodeBehind="global_sendsmtogroup.aspx.cs" %>

<%@ Register TagPrefix="cc1" Namespace="Discuz.Control" Assembly="Discuz.Control" %>
<%@ Register TagPrefix="uc1" TagName="textarea" Src="../UserControls/TextareaResize.ascx" %>
<%@ Register TagPrefix="cc3" Namespace="Discuz.Control" Assembly="Discuz.Control" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
<title>批量发送短消息</title>
<link href="../styles/dntmanager.css" type="text/css" rel="stylesheet" />
<link href="../styles/tab.css" type="text/css" rel="stylesheet" />
<script type="text/javascript" src="../js/common.js"></script>
<meta http-equiv="X-UA-Compatible" content="IE=7" />
</head>
<body>
<form id="Form1" method="post" runat="server">
<script type="text/javascript">
	document.getElementById('Layer5').innerHTML = "<br /><table><tr><td valign=top><img border=\"0\" src=\"../images/ajax_loading.gif\"  /></td><td valign=middle style=\"font-size: 14px;\" >程序已启动. 如果用户较多, <br />系统要运行一段时间....</td></tr></table><BR />";
</script>
<div class="ManagerForm">
<fieldset>
<legend style="background: url(../images/icons/icon57.jpg) no-repeat 6px 50%;">批量短消息发送</legend>
<table width="100%">
	<tr><td class="item_title" colspan="2">接收短消息的用户组</td></tr>
	<tr>
		<td class="vtop" colspan="2">
			<input type="checkbox" id="chkall" name="chkall" onclick="javascript:CheckAll(this.form);" />选择全部/取消
			<br />
			<cc1:CheckBoxList ID="Usergroups" runat="server" RepeatColumns="4">
			</cc1:CheckBoxList>
		</td>
	</tr>
	<tr><td class="item_title" colspan="2">标题</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox ID="subject" runat="server" CanBeNull="必填" RequiredFieldType="暂无校验" MaxLength="80" Size="60"></cc1:TextBox>
		</td>
		<td class="vtop"></td>
	</tr>
	<tr><td class="item_title" colspan="2">发布者用户名</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox ID="msgfrom" runat="server" CanBeNull="必填" RequiredFieldType="暂无校验"></cc1:TextBox>
		</td>
		<td class="vtop"></td>
	</tr>
	<tr><td class="item_title" colspan="2">文件箱</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:DropDownList ID="folder" runat="server">
				<asp:ListItem Value="0" Selected="True">收件箱</asp:ListItem>
				<asp:ListItem Value="1">发件箱</asp:ListItem>
				<asp:ListItem Value="2">草稿箱</asp:ListItem>
			</cc1:DropDownList>
		</td>
		<td class="vtop"></td>
	</tr>
	<tr><td class="item_title" colspan="2">发送日期</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox ID="postdatetime" runat="server" CanBeNull="必填" RequiredFieldType="日期" Size="10"></cc1:TextBox>
		</td>
		<td class="vtop">格式:2005-5-5</td>
	</tr>
	<tr><td class="item_title" colspan="2">每次循环发送消息数</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox ID="postcountpercircle" runat="server" CanBeNull="必填" RequiredFieldType="数据校验" Size="10">100</cc1:TextBox>
		</td>
		<td class="vtop">格式:2005-5-5</td>
	</tr>
	<tr><td class="item_title" colspan="2">短消息内容</td></tr>
	<tr>
		<td class="vtop rowform">
			  <uc1:textarea ID="message" runat="server" controlname="TabControl1:tabPage51:message" Rows="10" cols="80"></uc1:textarea>
		</td>
		<td class="vtop">格式:2005-5-5</td>
	</tr>
</table>
<div class="Navbutton">
	<cc1:Button ID="BatchSendSM" runat="server" Style="align: center" Text=" 提 交 ">
	</cc1:Button>
</div>
</fieldset>
</div>
<asp:Label ID="lblClientSideCheck" runat="server" CssClass="hint">&nbsp;</asp:Label>
<asp:Label ID="lblCheckedNodes" runat="server" CssClass="hint">&nbsp;</asp:Label>
<asp:Label ID="lblServerSideCheck" runat="server" CssClass="hint">&nbsp;</asp:Label>
<script type="text/javascript">
	document.getElementById("lblClientSideCheck").innerText = document.getElementById("lblServerSideCheck").innerText;
</script>
</form>
<%=footer%>
</body>
</html>