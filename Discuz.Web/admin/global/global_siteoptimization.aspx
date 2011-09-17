<%@ Page Language="c#" Inherits="Discuz.Web.Admin.siteoptimization" CodeBehind="global_siteoptimization.aspx.cs" %>
<%@ Register TagPrefix="cc1" Namespace="Discuz.Control" Assembly="Discuz.Control" %>
<%@ Register TagPrefix="uc1" TagName="TextareaResize" Src="../UserControls/TextareaResize.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
<title>baseset</title>
<link href="../styles/dntmanager.css" type="text/css" rel="stylesheet" />
<link href="../styles/modelpopup.css" type="text/css" rel="stylesheet" />
<script type="text/javascript" src="../js/common.js"></script>
<script type="text/javascript" src="../js/modalpopup.js"></script>
<script type="text/javascript">
	function setStatus(status) {
	    document.getElementById("isclosedforum").style.display = (status) ? "block" : "none";
	}
</script>
<meta http-equiv="X-UA-Compatible" content="IE=7" />
</head>
<body>
<div class="ManagerForm">
<form id="Form1" method="post" runat="server" name="Form1">
<fieldset>
	<legend style="background: url(../images/icons/legendimg.jpg) no-repeat 6px 50%;">站点优化</legend>
	<table width="100%">
	<tr><td class="item_title" colspan="2">数据库全文搜索</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:RadioButtonList id="fulltextsearch" runat="server" RepeatLayout="flow">
				<asp:ListItem Value="1">是</asp:ListItem>
				<asp:ListItem Value="0">否</asp:ListItem>
			</cc1:RadioButtonList>
		</td>
		<td class="vtop">论坛会对查询使用SQL2000的全文搜索功能以提升效率. 注意: 本功能会增加数据库的体积</td>
	</tr>
	<tr><td class="item_title" colspan="2">禁止浏览器缓冲</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:RadioButtonList id="nocacheheaders" runat="server"  RepeatLayout="flow">
				<asp:ListItem Value="1">是</asp:ListItem>
				<asp:ListItem Value="0">否</asp:ListItem>
			</cc1:RadioButtonList>
		</td>
		<td class="vtop">选择"是"将禁止浏览器对论坛页面进行缓冲, 用于解决极个别浏览器内容刷新不正常的问题. 注意: 本功能会加重服务器负担</td>
	</tr>
	<tr><td class="item_title" colspan="2">最大在线人数</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:TextBox SetFocusButtonID="SaveInfo" ID="maxonlines" runat="server" RequiredFieldType="数据校验" CanBeNull="必填" Text="9000" MaxLength="5" Size="6" MinimumValue="10" MaximumValue="65535">
			</cc1:TextBox>
		</td>
		<td class="vtop">请设置合理的数值, 范围 10~65535, 建议设置为平均在线人数的 10 倍左右</td>
	</tr>
	<tr><td class="item_title" colspan="2">搜索时间限制</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox SetFocusButtonID="SaveInfo" ID="searchctrl" runat="server" RequiredFieldType="数据校验" CanBeNull="必填" Text="0" MaxLength="5" Size="6"></cc1:TextBox>(单位:秒)
		</td>
		<td class="vtop">两次搜索间隔小于此时间将被禁止, 0 为不限制</td>
	</tr>
	<tr><td class="item_title" colspan="2">统计系统缓存时间</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox id="statscachelife" runat="server" CanBeNull="必填" RequiredFieldType="数据校验"  Size="6" MaxLength="4"></cc1:TextBox>(单位:分钟)
		</td>
		<td class="vtop">统计数据缓存更新的时间,数值越大,数据更新频率越低,越节约资源,但数据实时程度越低,建议设置为 60 以上,以免占用过多的服务器资源</td>
	</tr>
	<tr><td class="item_title" colspan="2">缓存游客页面的失效时间</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:TextBox id="guestcachepagetimeout" runat="server" RequiredFieldType="数据校验" CanBeNull="必填" Size="4" Text="10" MaxLength="3"></cc1:TextBox>(单位:分钟)
		</td>
		<td class="vtop">论坛在线人数大时建议开启, 为0不缓存, 大于0则缓存该值的时间,单位为分钟, 建议值为10.</td>
	</tr>
	<tr><td class="item_title" colspan="2">用户在线时间更新时长</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:TextBox id="oltimespan" runat="server" CanBeNull="必填" RequiredFieldType="数据校验"  Size="6" MaxLength="4"></cc1:TextBox>(单位:分钟)
		</td>
		<td class="vtop">可统计每个用户总共和当月的在线时间,本设置用以设定更新用户在线时间的时间频率.例如设置为 10,则用户每在线 10 分钟更新一次记录.本设置值越小,则统计越精确,但消耗资源越大.建议设置为 5～30 范围内,0 为不记录用户在线时间</td>
	</tr>
	<tr><td class="item_title" colspan="2">缓存游客查看主题页面的权重</td></tr>
	<tr>
		<td class="vtop rowform">
			<cc1:TextBox id="topiccachemark" runat="server" RequiredFieldType="数据校验" CanBeNull="必填" Text="10"  Size="4" MaxLength="3" MaximumValue="100" MinimumValue="0"></cc1:TextBox>
		</td>
		<td class="vtop">为0则不缓存, 范围0 - 100  (数字越大, 缓存数据越多)</td>
	</tr>
	<tr><td class="item_title" colspan="2">帖子中显示作者状态</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:RadioButtonList id="showauthorstatusinpost" runat="server" RepeatColumns="1">
				<asp:ListItem Value="1" selected="true">简单判断作者在线状态并显示(推荐)</asp:ListItem>
				<asp:ListItem Value="2">精确判断作者在线状态并显示</asp:ListItem>
			</cc1:RadioButtonList>		</td>
		<td class="vtop">浏览帖子时显示作者在线状态, 如果在线用户数量较多时, 启用"精确判断作者在线状态"功能会加重服务器负担, 此时建议使用"简单判断作者在线状态"</td>
	</tr>
	<tr><td class="item_title" colspan="2">无动作离线时间</td></tr>
	<tr>
		<td class="vtop rowform">
			 <cc1:TextBox id="onlinetimeout" runat="server" RequiredFieldType="数据校验" CanBeNull="必填" Size="5" Text="10" MaxLength="4"></cc1:TextBox>(单位:分钟)					        </td>
		<td class="vtop">多久无动作视为离线, 默认为10</td>
	</tr>	
	</table>
</fieldset>
<div class="Navbutton">
	<cc1:Button ID="SaveInfo" runat="server" Text="提 交"></cc1:Button>
</div>
</form>
</div>
<%=footer%>
</body>
</html>
