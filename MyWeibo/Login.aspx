<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>登录</title>
		<link rel="stylesheet" type="text/css" href="css/reset.css" />
	<link rel="stylesheet" type="text/css" href="css/stylesheet.css" />
	<script type="text/javascript" src="scripts/jquery-1.7.2.js"></script>

    <style type="text/css">
        .style2
        {
            color: #FF3300;
            font-size: medium;
        }
        .style3
        {
            font-size: medium;
        }
    </style>

</head>
<body>
    <form id="form1" runat="server">
	<div id="header">
		<div id="header_wrap" class="row">
			<p id="logo">
				<a href="#">我的<span>微博达人</span>PK</a>
			</p>
			<ul id="page-links">
				<li><a href="#" class="selected">主页</a></li>
				<li><a href="#">联系</a></li>
				<li><a href="#">评论</a></li>
                <li><a href="#">PK</a></li>
			</ul>
			<div id="search-box" class="row">
				<input type="text" id="search-box-text" />
				<span id="search-box-button"><a href="#" class="magnify"></a></span><span id="search-box-tips">搜索微博内容</span>
				<script type="text/javascript">
					var searchTips = $("#search-box-tips");
					var searchText = $("#search-box-text");
					searchText.focus(function () {
						searchTips.hide();
						$(this).addClass("focus");
					}).blur(function () {
						if ($(this).val().length == 0) {
							$(this).val("");
							searchTips.show();
						}
						$(this).removeClass("focus");
					});
					searchTips.click(function () {
						searchTips.hide();
						searchText.trigger("focus");
					});
				</script>
			</div>
		</div>
	</div>
	<div id="content_wrap" class="row">
		<div id="content_left_wrap">
			<div class="panel">
				<h2 class="title">登录</h2>
				<div class="box">
				&nbsp;&nbsp; <span class="style3">用户名：</span><input id="Text1" type="text" 
                        class="style3" /><span class="style2">*</span><br class="style3" />
                    <br class="style3" />
                    <span class="style3">&nbsp;&nbsp;
                    密&nbsp;&nbsp; 码：</span><input id="Password1" type="password" class="style3" /><span 
                        class="style2">*</span></div>
                <div class="box">
					&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
					<asp:Button runat="server" ClientIDMode="Static" ID="btnSend" CssClass="status-box-submit2" Text="登   录" OnClick="btnSend_Click" />
                        <br />
				</div>
                <div class="box">
				
				<asp:HyperLink id="authUrl" runat="server">
				<img src="images/240.png" alt="点击此处进行授权"/>
			</asp:HyperLink>
				
				</div>
			</div>
		</div>
		<div id="content_right_wrap">
			<h2 class="title">Loading.... </h2>
			<asp:Literal runat="server" ID="statusesHtml"></asp:Literal>
		    <br />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Image ID="Image1" runat="server" ImageUrl="~/images/13.gif" />
		</div>
	</div>
    </form>
</body>
</html>
